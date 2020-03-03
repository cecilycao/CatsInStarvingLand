/* Copyright (c) 2014 Advanced Platformer 2D */

using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu("Advanced Platformer 2D/EdgeGrab")]
public class APEdgeGrab : MonoBehaviour 
{
	////////////////////////////////////////////////////////
	// PUBLIC/HIGH LEVEL
    public Direction m_direction = Direction.Right;			// grab direction
    public GrabInAirDesc m_grabInAir;                       // desc for grab in air settings
    public GrabFromTopDesc m_grabFromTop;                   // desc for grab from top settings
    public ExitClimbUpDesc m_exitClimbUp;                   // desc for exit climb up settings

	public float m_inputExitFront = 1f;						// min forward input value for exiting (1 to disable)
	public float m_inputExitBack = 1f;						// min backward input value for exiting (1 to disable)
	public float m_inputExitDown = 0.5f;					// min down input for exiting (1 to disable)
	public bool m_jumpExit = true;							// allows exiting with jump button
	public Vector2 m_jumpExitPower = new Vector2(4, 15f); 	// power velocity when exiting with jump
	public float m_minGrabTime = 0.1f;						// minimum time to grab before allowing to exit
    

    // Direction type of the grab
    public enum Direction
    {
        Right,
        Left
    }

    // Grab in air
    [System.Serializable]
    public class GrabInAirDesc
    {
        public bool m_enabled;                      // enable this action
        public Vector3 m_handle = Vector2.zero;		// handle used for detection
        public float m_handleRadius = 0.1f;         // radius of handle detector
        public APAnimation m_anim;                  // player's animation state to launch when grabbing directly
    };

    // Exit climb up
    [System.Serializable]
    public class ExitClimbUpDesc
    {
        public bool m_enabled;                       // enable this action
        public APInputButton m_button;               // button for exiting with climb up
        public APAnimation m_anim;                   // player's animation state to launch when exiting by climbing up
        public float m_maxExitInput = 1f;            // max input ratio at which character is allowed to exits
        public float m_maxExitPower = 0;             // additional exit power in function of input pressed while exiting
        public float m_maxExitPowerAnimTime = 0.5f;  // time of animation at which exit power starts to apply
    };

    // Grab from top
    [System.Serializable]
    public class GrabFromTopDesc
    {
        public bool m_enabled;                      // enable this action
        public Vector3 m_handle = Vector2.zero;	    // handle used for detection
        public float m_handleRadius = 0.1f;         // radius of handle detector
        public APInputButton m_button;              // button to used in order to launch grab from top
        public APAnimation m_anim;                  // player's animation state to launch when grabbing from top of edge
    };

	////////////////////////////////////////////////////////
	// PRIVATE/HIGH LEVEL
	APCharacterController m_controlled;
	float m_grabTime;
	float m_releaseTime;
	bool m_isClimbUpExit;
	CircleCollider2D m_colliderAligned;
	CircleCollider2D m_colliderFromTop;
	Vector2 m_curGrabHandle;
    bool m_bPhysicAnim;
    Vector2 m_initPhysicAnimPos;
    float m_curExitPower;

    // Direction type of the grab
    enum ExitType
    {
        Inputs,
        Jump,
        ClimbExit
    }

	void ClearRuntimeValues()
	{
		m_grabTime = Time.time;
		m_releaseTime = 0f;
		m_isClimbUpExit = false;
        m_curGrabHandle = Vector2.zero;
        m_bPhysicAnim = false;
        m_initPhysicAnimPos = Vector2.zero;
        m_curExitPower = 0f;
	}

	void Awake () 
	{
		m_controlled = null;
	}

	void Start () 
	{
		ClearRuntimeValues();

		// Create and setup triggers
        if (m_grabInAir.m_enabled)
        {
            CreateTrigger(ref m_colliderAligned, m_grabInAir.m_handle, m_grabInAir.m_handleRadius);
        }

        if (m_grabFromTop.m_enabled)
        {
            CreateTrigger(ref m_colliderFromTop, m_grabFromTop.m_handle, m_grabFromTop.m_handleRadius);
        }
	}

	void CreateTrigger(ref CircleCollider2D collider, Vector3 pos, float radius)
	{
		collider = gameObject.AddComponent<CircleCollider2D>();
		collider.isTrigger = true;
		collider.offset = pos;
        collider.radius = radius;
	}

	void FixedUpdate()
	{
		if(m_controlled)
		{
			// always stay on anchor every frame
			MatchGrabAnchor();

			// check if currently exiting
			if(m_isClimbUpExit)
			{
				// wait for end of character animation
                float fClimbUpExitAnimTime = m_controlled.GetAnim().GetCurrentAnimatorStateInfo(0).normalizedTime;

                // raise exit power if needed
                if (m_exitClimbUp.m_maxExitPower > 0f && fClimbUpExitAnimTime >= m_exitClimbUp.m_maxExitPowerAnimTime)
                {
                    float fClampedInput = GetClampedInput();
                    m_curExitPower += Time.deltaTime * Time.deltaTime * m_exitClimbUp.m_maxExitPower * fClampedInput;
                }

                if (fClimbUpExitAnimTime >= 1f)
				{
					// leave control
					m_isClimbUpExit = false;
                    ReleaseControl(ExitType.ClimbExit);
				}

				return;
			}

            // do not exit too quickly (wait end of animation in all case)
            float fAnimTime = m_controlled.GetAnim().GetCurrentAnimatorStateInfo(0).normalizedTime;
            if ((Time.time - m_grabTime < m_minGrabTime) || fAnimTime < 1f)
            {
                return;
            }

            // snap to in air handle if grabbing from top and animation is ended (and stop physic animation)
            /*if (m_bIsGrabbingFromTop && m_controlled.GetAnim().GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                m_bPhysicAnim = false;
                m_bIsGrabbingFromTop = false;
                m_curGrabHandle = transform.TransformPoint(m_grabInAir.m_handle);
            }*/

			// check input for leaving this state is pushed or script is disabled
			float xValue = m_controlled.m_inputs.m_axisX.GetValue();
			float yValue = m_controlled.m_inputs.m_axisY.GetValue();

			if( !enabled ||
			   	(m_controlled.IsFacingRight() && xValue < -m_inputExitBack) ||
			   	(m_controlled.IsFacingRight() && xValue > m_inputExitFront) ||
			   	(!m_controlled.IsFacingRight() && xValue > m_inputExitBack) ||
			   	(!m_controlled.IsFacingRight() && xValue < -m_inputExitFront) ||
			   	(yValue < -m_inputExitDown) )
			{
                ReleaseControl(ExitType.Inputs);
			}
			// jump exit
			else if(m_jumpExit && m_controlled.m_jump.m_button.GetButtonDown())
			{
                ReleaseControl(ExitType.Jump);
			}
			else
			{
				// handle climb up exit
                if (m_exitClimbUp.m_enabled && m_exitClimbUp.m_button.IsSpecified() && m_exitClimbUp.m_button.GetButton())
				{
					m_isClimbUpExit = true;
					
					// launch player physic exit animation
                    InitPhysAnimation(transform.TransformPoint(m_grabInAir.m_handle));
                    m_controlled.PlayAnim(m_exitClimbUp.m_anim);
				}
			}
		}
	}

	void LateUpdate()
	{ 
		// handle physic animation after render update
        if (m_controlled && m_bPhysicAnim && m_controlled.GetPhysicAnim())
		{
            Vector2 curPhysicLocal = m_controlled.GetPhysicAnim().transform.localPosition;
            if (m_direction == Direction.Left)
            {
                curPhysicLocal.x = -curPhysicLocal.x;
            }

            curPhysicLocal.x += m_curExitPower;
            m_curGrabHandle = m_initPhysicAnimPos + curPhysicLocal;
		}        
	}

	void MatchGrabAnchor()
	{
		// use grab position as anchor point for character, start moving character (don't detect collisions during move)
        Vector2 v2Dist = m_curGrabHandle - (Vector2)m_controlled.transform.position;
		if(v2Dist.sqrMagnitude > 0.0001f)
		{
			m_controlled.GetRigidBody().velocity = v2Dist / Time.fixedDeltaTime;
		}
		else
		{
			m_controlled.GetRigidBody().velocity = Vector2.zero;
		}
	}

	public bool TryTakeControl(APCharacterController controlled, Collider2D otherCollider)
	{
		if(enabled && !m_controlled)
		{
            // check grabbing mode
            if (otherCollider == m_colliderFromTop && m_grabFromTop.m_enabled && m_grabFromTop.m_button.IsSpecified())
            {
                // make sure correct input is pushed while in trigger
                if (m_grabFromTop.m_button.GetButton())
                {
                    // flip player if not facing correctly
                    if (controlled.IsFacingRight() != (m_direction == Direction.Right))
                    {
                        controlled.GetMotor().Flip();
                    }
                    
                    StartGrab(controlled, m_grabFromTop.m_anim, m_grabFromTop.m_handle);
                    return true;
                }
            }
            else if (otherCollider == m_colliderAligned && m_grabInAir.m_enabled)
            {
                // make sure we do not grab same edge too quickly while in air
                if (Time.time - m_releaseTime < controlled.m_edgeGrab.m_minTimeBetweenEdgeGrabs)
                    return false;

                // check direction
                if (m_direction == Direction.Right && controlled.IsFacingRight() ||
                   m_direction == Direction.Left && !controlled.IsFacingRight())
                {
                    StartGrab(controlled, m_grabInAir.m_anim, m_grabInAir.m_handle);
                    return true;
                }
            }
		}

		return false;
	}

    void StartGrab(APCharacterController controlled, APAnimation anim, Vector2 grabHandle)
    {
        ClearRuntimeValues();

        m_controlled = controlled;
        m_controlled.IsControlled = true;

        // Init handle to grab
        m_curGrabHandle = transform.TransformPoint(grabHandle);
        MatchGrabAnchor();

        // start anim & reset velocity
        m_controlled.GetMotor().m_velocity = Vector2.zero;
        InitPhysAnimation(m_curGrabHandle);
        m_controlled.PlayAnim(anim);

        // launch events
        m_controlled.EventListeners.ForEach(e => e.OnEdgeGrabStart(this));
    }

    void InitPhysAnimation(Vector2 refPos)
    {
        if (m_controlled.GetPhysicAnim())
        {
            m_bPhysicAnim = true;
            m_initPhysicAnimPos = refPos;
        }
    }

	void ReleaseControl(ExitType EExitType)
	{
		if(m_controlled)
		{
            m_bPhysicAnim = false;

			// keep release time
            if(EExitType != ExitType.ClimbExit)
			    m_releaseTime = Time.time;

            // Leave control           
            m_controlled.EventListeners.ForEach(e => e.OnEdgeGrabEnd(this));
            m_controlled.IsControlled = false;            

			// exit with small impulse
            if (EExitType == ExitType.Jump)
			{
				APCharacterMotor motor = m_controlled.GetMotor();
				motor.m_velocity = m_jumpExitPower;

				float horAxis = m_controlled.m_inputs.m_axisX.GetValue();
				motor.m_velocity.x *= horAxis;
				
				// make sure to exit in right side
				if((motor.m_velocity.x > 0f && !motor.FaceRight) || (motor.m_velocity.x < 0f && motor.FaceRight))
				{
					motor.Flip();
				}
			}
            else if (EExitType == ExitType.ClimbExit)
            {
                float fClampedInput = GetClampedInput();

                // inject exit character velocity
                float fGroundSpeed = m_controlled.ComputeMaxGroundSpeed();
                m_controlled.GetMotor().m_velocity = Vector2.right * fClampedInput * fGroundSpeed;

                // limit exit input
                m_controlled.m_inputs.m_axisX.ResetValue(fClampedInput);
            }

            m_controlled = null;  
		}
	}

    float GetClampedInput()
    {                 
        float curInputX = m_controlled.m_inputs.m_axisX.GetValue();
        float fExitInputRatio = Mathf.Clamp01(m_exitClimbUp.m_maxExitInput);
        float fMaxInput = m_direction == Direction.Right ? Mathf.Min(Mathf.Max(0f, curInputX), fExitInputRatio) : Mathf.Max(Mathf.Min(0f, curInputX), -fExitInputRatio);
        return fMaxInput;
    }
}
