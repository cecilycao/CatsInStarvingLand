/* Copyright (c) 2014 Advanced Platformer 2D */

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
[AddComponentMenu("Advanced Platformer 2D/Ladder")]

public class APLadder : MonoBehaviour 
{
	////////////////////////////////////////////////////////
	// PUBLIC/HIGH LEVEL
	[System.Serializable]
	public class Settings
	{
		public float m_maxSpeed = 5f; 							// max speed length	
		public float m_acceleration = 30f; 						// acceleration
		public float m_deceleration = 30f; 						// deceleration
		public bool m_canJump = true;  							// allow jumping
		public Vector2 m_jumpExitPower = new Vector2(4, 15f); 	// power velocity when exiting with jump
		public APAnimation m_animClimbUp;
		public APAnimation m_animClimbDown;
	}

	public string m_playerName = "Player";		// player name to follow
	public bool m_autoCatch = false;			// auto catch, must mush UP to catch if disabled
	public bool m_canExitRight = false;			// player can exit on right
	public bool m_canExitLeft = false;			// player can exit on left
	public bool m_canExitDown = false;			// player can exit on down

	public bool m_usePlayerSettings = true;		// use player settings or custom ones
	public Settings m_customSettings;			// settings to use when m_usePlayerSettings is false


	// Use this for initialization
	void Awake()
	{
		m_boxCollider = GetComponent<BoxCollider2D>();

		// init ladder central axis in world space
		Vector2 axis = new Vector2(0f, 0.5f * m_boxCollider.size.y);
		m_axisStart = transform.TransformPoint(m_boxCollider.offset - axis);
		m_axisEnd = transform.TransformPoint(m_boxCollider.offset + axis);
		m_axisDir = m_axisEnd - m_axisStart;
		m_axisLength = m_axisDir.magnitude;
		m_axisDir /= m_axisLength;

		// compute box in world space
		Vector2 pointA = transform.TransformPoint(m_boxCollider.offset - 0.5f * m_boxCollider.size);
		Vector2 pointB = transform.TransformPoint(m_boxCollider.offset + 0.5f * m_boxCollider.size);
		m_boxMin = Vector2.Min(pointA, pointB);
		m_boxMax = Vector2.Max(pointA, pointB);
	}

	void ClearRuntimeValues()
	{
		m_isControlling = false;
		m_timeForNextCatch = 0f;
		m_timeOnLadder = 0f;
		m_releaseJump = false;
		m_heroBoxComputed = false;
		m_defaultExitDir = false;
		m_moveDir = MoveDir.undefined;
	}
	
	void Start () 
	{
		ClearRuntimeValues();

		// link to at least one player if possible
		GameObject gameObject = GameObject.Find(m_playerName);
		if(gameObject != null)
		{
			m_player = gameObject.GetComponent<APCharacterController>();
			if(m_player != null)
			{
				m_motor = m_player.GetMotor();
				if(m_motor == null)
				{
					Debug.LogWarning("Ladder : player does not have any motor");
				}
			}
			else
			{
				Debug.LogWarning("Ladder : player does not have any CharacterController");
			}
		}
		else
		{
			Debug.LogWarning("Ladder : could not find any game object with name " + m_playerName);
		}
	}

	void FixedUpdate () 
	{
		if(APSettings.m_fixedUpdate)
		{
			UpdateLadder();
		}
	}
	
	void Update () 
	{
		if(!APSettings.m_fixedUpdate)
		{
			UpdateLadder();
		}
	}

	void LateUpdate()
	{
		// update animation
		if(m_isControlling)
		{
			Settings oSettings = GetSettings();
			
			float fAbsY = Mathf.Abs(m_motor.m_velocity.y);

			// set speed
            float fSpeed = oSettings.m_maxSpeed > Mathf.Epsilon ? fAbsY / oSettings.m_maxSpeed : 0f;
			m_player.GetAnim().speed = fSpeed;
			
			// set animation
			MoveDir ePrevMoveDir = m_moveDir;
			APAnimation sAnim = oSettings.m_animClimbUp;
			if(fSpeed < 1e-3f)
			{
				// special case when stopped, pause & keep previous animation
				m_player.GetAnim().speed = 0f;
				
				// first animation play is always up
				if(m_moveDir == MoveDir.undefined)
				{				
					m_moveDir = MoveDir.up;
				}
			}
			else if(m_motor.m_velocity.y > 0f)
			{
				sAnim = oSettings.m_animClimbUp;
				m_moveDir = MoveDir.up;
			}
			else
			{
				sAnim = oSettings.m_animClimbDown;
				m_moveDir = MoveDir.down;
			}

			if(ePrevMoveDir != m_moveDir)
			{
				m_player.PlayAnim(sAnim);
			}
		}
	}

	Settings GetSettings()
	{
		if(m_usePlayerSettings && m_player != null)
		{
			return m_player.m_ladder;
		}
		else
		{
			return m_customSettings;
		}
	}
	
	// Update is called once per frame
	void UpdateLadder () 
	{
		m_heroBoxComputed = false;

		// prevent any crash
		if(m_player == null || m_motor == null)
		{
			return;
		}

		// Handle dynamic of controlled hero
		if(m_isControlling)
		{
			Settings oSettings = GetSettings();

			// update default jump dir
			float fX = m_player.m_inputs.m_axisX.GetValue();
			if(fX > 0f)
				m_defaultExitDir = true;
			else if(fX < 0f)
				m_defaultExitDir = false;


			// handle jump exit first
			if(m_player.m_jump.m_button.IsSpecified())
			{
				bool bJump = m_player.m_jump.m_button.GetButton();
				m_releaseJump |= !bJump;

				if(m_releaseJump && bJump && GetSettings().m_canJump)
				{
					ReleaseControl(true);
					return;
				}
			}


			// update velocity
			m_motor.m_velocity.x = 0f;
			m_motor.m_velocity.y = APInputJoystick.Update(m_motor.m_velocity.y, oSettings.m_maxSpeed * m_player.m_inputs.m_axisY.GetValue(), true, oSettings.m_acceleration, oSettings.m_deceleration, Time.deltaTime);

			// Handle exit and move
			HandleExit();
			m_motor.Move();
		}
		else
		{
			if(m_timeForNextCatch > 0f)
			{
				m_timeForNextCatch -= Time.deltaTime; 
			}

			TryTakecontrol();
		}
	}

	void HandleExit()
	{
		ComputeHeroBox();
		float maxMoveUp = (m_boxMax.y - m_heroBoxMax.y) / Time.deltaTime;
		if(m_motor.m_velocity.y > maxMoveUp)
		{
			m_motor.m_velocity.y = maxMoveUp;
		}

		float maxMoveDown = (m_boxMin.y - m_heroBoxMin.y) / Time.deltaTime;
		if(m_motor.m_velocity.y < maxMoveDown)
		{
			if(m_canExitDown && m_motor.m_velocity.y < 0f)
				ReleaseControl(false);
			else
				m_motor.m_velocity.y = maxMoveDown;
		}

		m_timeOnLadder -= Time.deltaTime;
		if(m_timeOnLadder < 0f)
		{
			float horAxis = m_player.m_inputs.m_axisX.GetValue();
			if(m_canExitLeft && horAxis < -0.5f)
			{
				ReleaseControl(false);
			}
			else if(m_canExitRight && horAxis > 0.5f)
			{
				ReleaseControl(false);
			}
		}
	}

	void ComputeHeroBox()
	{
		if(!m_heroBoxComputed)
		{
			BoxCollider2D heroBox = m_motor.GetBoxCollider();
			Vector2 boxA = m_player.transform.TransformPoint(heroBox.offset - 0.5f * heroBox.size);
			Vector2 boxB = m_player.transform.TransformPoint(heroBox.offset + 0.5f * heroBox.size);
			
			m_heroBoxMin = Vector2.Min(boxA, boxB);
			m_heroBoxMax = Vector2.Max(boxA, boxB);
			m_heroBoxComputed = true;
		}
	}

	void TryTakecontrol()
	{
		// do nothing if catch is not allowed
		if((m_timeForNextCatch > 0f) || m_player.IsControlled || (!m_autoCatch && !GetCatchInput()))
			return;

		// Check if ladder collides with character's box
		ComputeHeroBox();

		// we must overlap and be inside one rectangle axis
		bool bCanCatch = (m_heroBoxMax.x >= m_boxMin.x) && (m_heroBoxMax.y >= m_boxMin.y) && (m_heroBoxMin.x <= m_boxMax.x) && (m_heroBoxMin.y <= m_boxMax.y);
		bCanCatch &= (m_heroBoxMax.y <= m_boxMax.y) && (m_heroBoxMin.y >= m_boxMin.y);

		if(bCanCatch)
		{
			Takecontrol();
		}
	}

	bool GetCatchInput()
	{
		float fYAxis = m_player.m_inputs.m_axisY.GetValue();
		return Mathf.Abs(fYAxis) > 0.5f;
	}

	void Takecontrol()
	{
		if(!m_isControlling)
		{
			m_moveDir = MoveDir.undefined;
			m_timeOnLadder = m_minTimeOnLadder;
			m_isControlling = true;
			m_player.IsControlled = true;
			m_releaseJump = !m_player.m_jump.m_button.IsSpecified() || !m_player.m_jump.m_button.GetButton();
			m_defaultExitDir = m_motor.m_faceRight;

			// make sure always facing right for animation
			if(!m_motor.m_faceRight)
				m_motor.Flip();

			// project controlled on ladder axis
			Vector2 curPos = m_motor.transform.position;
			Vector2 vFl = curPos - m_axisStart;
			float fDot = Vector2.Dot(vFl, m_axisDir);
			Vector2 v2Proj = m_axisStart + m_axisDir * Mathf.Clamp(fDot, 0f, m_axisLength);
			
			// move to reach destination
			m_motor.m_velocity = (v2Proj - curPos) / Time.deltaTime;
			m_motor.Move();

			m_player.EventListeners.ForEach(e => e.OnLadderCatch(this));
		}
	}

	void ReleaseControl(bool bJump)
	{
		if(m_isControlling)
		{
			m_timeForNextCatch = m_minTimeBetweenCatch;
			m_isControlling = false;
			m_player.IsControlled = false;

			// exit with small impulse
			if(bJump)
			{
				float horAxis = m_player.m_inputs.m_axisX.GetValue();
				float signAxis = horAxis != 0f ? Mathf.Sign(horAxis) : (m_defaultExitDir ? 1f : -1f);
				m_motor.m_velocity = GetSettings().m_jumpExitPower;
				m_motor.m_velocity.x *= signAxis;

				// make sure to exit in right side
				if((signAxis > 0f && !m_motor.FaceRight) || (signAxis < 0f && m_motor.FaceRight))
				{
					m_motor.Flip();
				}
			}

			m_player.EventListeners.ForEach(e => e.OnLadderRelease(this));
		}
	}

	////////////////////////////////////////////////////////
	// PRIVATE/LOW LEVEL
	enum MoveDir
	{
		undefined = 0,
		up,
		down
	}
	
	APCharacterController m_player;
	APCharacterMotor m_motor;
	BoxCollider2D m_boxCollider;
	bool m_isControlling;
	
	Vector2 m_boxMin, m_boxMax;
	Vector2 m_axisStart, m_axisEnd, m_axisDir;
	float m_axisLength;
	
	float m_minTimeOnLadder = 0.3f;
	float m_timeOnLadder = 0;
	float m_minTimeBetweenCatch = 0.3f; // to prevent recatch just after exiting and to leave ladder too soon
	float m_timeForNextCatch;
	bool m_releaseJump;
	Vector2 m_heroBoxMin, m_heroBoxMax;
	bool m_heroBoxComputed;
	bool m_defaultExitDir;
	MoveDir m_moveDir;
}
