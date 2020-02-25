/* Copyright (c) 2014 Advanced Platformer 2D */

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
[AddComponentMenu("Advanced Platformer 2D/Railings")]

public class APRailings : MonoBehaviour 
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
		public Vector2 m_jumpExitPower = new Vector2(4f, 15f); 	// power velocity when exiting with jump
		public APAnimation m_animClimbUp;				// anim name for climb up
		public APAnimation m_animClimbRight;			// anim name for climb right
		public APAnimation m_animClimbDown;				// anim name for climb down
		public APAnimation m_animClimbLeft;				// anim name for climb left
	}

	public string m_playerName = "Player";		// name of player to catch
	public bool m_autoCatch = false;			// enable autocatch
	public bool m_canExitRight = false;			// player can exit on right
	public bool m_canExitLeft = false;			// player can exit on left
	public bool m_canExitDown = false;			// player can exit on down

	public bool m_usePlayerSettings = true;
	public Settings m_customSettings;
		
	class Box
	{
		public Vector2 m_min;
		public Vector2 m_max;
	}	
	Box[] m_boxes;


	// Use this for initialization
	void Awake()
	{
		m_boxes = null;

		BoxCollider2D[] colliders = GetComponents<BoxCollider2D>();
		if(colliders.Length > 0)
		{
			m_boxes = new Box[colliders.Length];
			for(int i = 0; i < colliders.Length; i++)
			{
				// compute box in world space
				BoxCollider2D collider = colliders[i];
				Vector2 pointA = transform.TransformPoint(collider.offset - 0.5f * collider.size);
				Vector2 pointB = transform.TransformPoint(collider.offset + 0.5f * collider.size);

				Box newBox = new Box();
				newBox.m_min = Vector2.Min(pointA, pointB);
				newBox.m_max = Vector2.Max(pointA, pointB);
				m_boxes[i] = newBox;
			}
		}
	}

	void ClearRuntimeValues()
	{
		m_isControlling = false;
		m_timeForNextCatch = 0f;
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
					Debug.LogWarning("Railings : player does not have any motor");
				}
			}
			else
			{
				Debug.LogWarning("Railings : player does not have any CharacterController");
			}
		}
		else
		{
			Debug.LogWarning("Railings : could not find any game object with name " + m_playerName);
		}
	}

	void FixedUpdate () 
	{
		if(APSettings.m_fixedUpdate)
		{
			UpdateRailings();
		}
	}

	void Update () 
	{
		if(!APSettings.m_fixedUpdate)
		{
			UpdateRailings();
		}
	}

	void LateUpdate()
	{
		// update animation
		if(m_isControlling)
		{
			Settings oSettings = GetSettings();

			float fAbsY = Mathf.Abs(m_motor.m_velocity.y);
			float fAbsX = Mathf.Abs(m_motor.m_velocity.x);

			// set speed
            float fSpeed = oSettings.m_maxSpeed > Mathf.Epsilon ? Mathf.Max(fAbsY, fAbsX) / oSettings.m_maxSpeed : 0f;
			m_player.GetAnim().speed = fSpeed;

			// set animation
			MoveDir ePrevMoveDir = m_moveDir;
			APAnimation sAnim = oSettings.m_animClimbUp;
			if(fSpeed < 1e-3f)
			{
				// special case when stopped, pause & keek previous animation
				m_player.GetAnim().speed = 0f;

				// first animation play is always up
				if(m_moveDir == MoveDir.undefined)
				{				
					m_moveDir = MoveDir.up;
				}
			}
			else if(fAbsX > fAbsY)
			{
				if(m_motor.m_velocity.x > 0f)
				{
					sAnim = oSettings.m_animClimbRight;
					m_moveDir = MoveDir.right;
				}
				else
				{
					sAnim = oSettings.m_animClimbLeft;
					m_moveDir = MoveDir.left;
				}
			}
			else
			{
				if(m_motor.m_velocity.y > 0f)
				{
					sAnim = oSettings.m_animClimbUp;
					m_moveDir = MoveDir.up;
				}
				else
				{
					sAnim = oSettings.m_animClimbDown;
					m_moveDir = MoveDir.down;
				}
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
			return m_player.m_railings;
		}
		else
		{
			return m_customSettings;
		}
	}

	// Update is called once per frame
	void UpdateRailings () 
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
			m_motor.m_velocity.x = APInputJoystick.Update(m_motor.m_velocity.x, oSettings.m_maxSpeed * m_player.m_inputs.m_axisX.GetValue(), true, oSettings.m_acceleration, oSettings.m_deceleration, Time.deltaTime);
			m_motor.m_velocity.y = APInputJoystick.Update(m_motor.m_velocity.y, oSettings.m_maxSpeed * m_player.m_inputs.m_axisY.GetValue(), true, oSettings.m_acceleration, oSettings.m_deceleration, Time.deltaTime);
			m_motor.m_velocity = Vector2.ClampMagnitude(m_motor.m_velocity, oSettings.m_maxSpeed);

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

		// up/down
		float maxMoveUp = ComputeMaxMoveUp();
		if(m_motor.m_velocity.y > maxMoveUp)
		{
			m_motor.m_velocity.y = maxMoveUp;
		}

		float maxMoveDown = ComputeMaxMoveDown();
		if(m_motor.m_velocity.y < maxMoveDown)
		{
			if(m_canExitDown && m_motor.m_velocity.y < 0f)
			{
				ReleaseControl(false);
				return;
			}
			else
				m_motor.m_velocity.y = maxMoveDown;
		}

		// left/right
		float maxMoveRight = ComputeMaxMoveRight();
		if(m_motor.m_velocity.x > maxMoveRight)
		{
			if(m_canExitRight && m_motor.m_velocity.x > 0f)
			{
				ReleaseControl(false);
				return;
			}
			else
				m_motor.m_velocity.x = maxMoveRight;
		}

		float maxMoveLeft = ComputeMaxMoveLeft();
		if(m_motor.m_velocity.x < maxMoveLeft)
		{
			if(m_canExitLeft && m_motor.m_velocity.x < 0f)
			{
				ReleaseControl(false);
				return;
			}
			else
				m_motor.m_velocity.x = maxMoveLeft;
		}
	}

	float ComputeMaxMoveUp()
	{
		ComputeHeroBox();

		float fMaxMove = float.MinValue;
		for(int i = 0; i < m_boxes.Length; i++)
		{
			// consider inside boxes only
			Box box = m_boxes[i];
			if((m_heroBoxMax.x <= box.m_max.x + m_maxMoveTolerance) && (m_heroBoxMin.x >= box.m_min.x - m_maxMoveTolerance) && (m_heroBoxMin.y >= box.m_min.y - m_maxMoveTolerance))
			{
				fMaxMove = Mathf.Max((box.m_max.y - m_heroBoxMax.y) / Time.deltaTime, fMaxMove);
			}
		}

		return fMaxMove;
	}

	float ComputeMaxMoveDown()
	{
		ComputeHeroBox();
		
		float fMaxMove = float.MaxValue;
		for(int i = 0; i < m_boxes.Length; i++)
		{
			// consider inside boxes only
			Box box = m_boxes[i];
			if((m_heroBoxMax.x <= box.m_max.x + m_maxMoveTolerance) && (m_heroBoxMin.x >= box.m_min.x - m_maxMoveTolerance) && (m_heroBoxMax.y <= box.m_max.y + m_maxMoveTolerance))
			{
				fMaxMove = Mathf.Min((box.m_min.y - m_heroBoxMin.y) / Time.deltaTime, fMaxMove);
			}
		}
		
		return fMaxMove;
	}

	float ComputeMaxMoveRight()
	{
		ComputeHeroBox();
		
		float fMaxMove = float.MinValue;
		for(int i = 0; i < m_boxes.Length; i++)
		{
			// consider inside boxes only
			Box box = m_boxes[i];
			if((m_heroBoxMax.y <= box.m_max.y + m_maxMoveTolerance) && (m_heroBoxMin.y >= box.m_min.y - m_maxMoveTolerance) && (m_heroBoxMin.x >= box.m_min.x - m_maxMoveTolerance))
			{
				fMaxMove = Mathf.Max((box.m_max.x - m_heroBoxMax.x) / Time.deltaTime, fMaxMove);
			}
		}
		
		return fMaxMove;
	}

	float ComputeMaxMoveLeft()
	{
		ComputeHeroBox();
		
		float fMaxMove = float.MaxValue;
		for(int i = 0; i < m_boxes.Length; i++)
		{
			// consider inside boxes only
			Box box = m_boxes[i];
			if((m_heroBoxMax.y <= box.m_max.y + m_maxMoveTolerance) && (m_heroBoxMin.y >= box.m_min.y - m_maxMoveTolerance) && (m_heroBoxMax.x <= box.m_max.x + m_maxMoveTolerance))
			{
				fMaxMove = Mathf.Min((box.m_min.x - m_heroBoxMin.x) / Time.deltaTime, fMaxMove);
			}
		}
		
		return fMaxMove;
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

		// we must be inside
		if(IsInside())
		{
			Takecontrol();
		}
	}

	bool IsInside()
	{
		ComputeHeroBox();
		for(int i = 0; i < m_boxes.Length; i++)
		{
			Box box = m_boxes[i];
			if((m_heroBoxMax.x <= box.m_max.x) && (m_heroBoxMax.y <= box.m_max.y) && (m_heroBoxMin.x >= box.m_min.x) && (m_heroBoxMin.y >= box.m_min.y))
				return true;
		}
		return false;
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
			m_isControlling = true;
			m_player.IsControlled = true;
			m_releaseJump = !m_player.m_jump.m_button.IsSpecified() || !m_player.m_jump.m_button.GetButton();
			m_defaultExitDir = m_motor.m_faceRight;

			// make sure always facing right for animation
			if(!m_motor.m_faceRight)
				m_motor.Flip();
			
			// stop all velocities
			m_motor.m_velocity = Vector2.zero;

			m_player.EventListeners.ForEach(e => e.OnRailingsCatch(this));
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

			m_player.EventListeners.ForEach(e => e.OnRailingsRelease(this));
		}
	}

	
	////////////////////////////////////////////////////////
	// PRIVATE/LOW LEVEL
	enum MoveDir
	{
		undefined = 0,
		up,
		down,
		right,
		left
	}
	
	APCharacterController m_player;		// ref to player
	APCharacterMotor m_motor;			// ref to motor
	bool m_isControlling;					// check if controlling
	float m_minTimeBetweenCatch = 0.3f; 	// to prevent recatch just after exiting and to leave railings too soon
	float m_maxMoveTolerance = 0.1f; 		// tolerance threshold for max move handling
	float m_timeForNextCatch;				// time for new catch
	bool m_releaseJump;						// release jump status
	Vector2 m_heroBoxMin, m_heroBoxMax;		// hero box
	bool m_heroBoxComputed;					// hero box updated
	bool m_defaultExitDir;					// exit direction
	MoveDir m_moveDir;						// move direction for animation
}
