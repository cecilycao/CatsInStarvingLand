/* Copyright (c) 2014 Advanced Platformer 2D */
using UnityEngine;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(APCharacterMotor))]
[AddComponentMenu("Advanced Platformer 2D/CharacterController")]

public partial class APCharacterController : MonoBehaviour
{
	////////////////////////////////////////////////////////
	// PUBLIC/HIGH LEVEL

	public APCharacterMotor GetMotor()
	{
		return m_motor;
	}

	public Animator GetAnim()
	{
		return m_anim;
	}

	public Rigidbody2D GetRigidBody()
	{
		return m_rigidBody;
	}

	public float GetGroundSpeed()
	{
		return m_groundSpeed;
	}

	public bool IsCrouched()
	{
		return (GetState() == State.Crouch) || (GetRequestedState() == State.Crouch) || IsAttackingCrouched();
	}

	public bool IsOnGround()
	{
		return m_onGround;
	}

	public bool IsRunning()
	{
		return m_standardState == StandardState.Run;
	}

	public bool IsShifting()
	{
		return (GetState() == State.Shift) && m_shiftLaunched;
	}

	public GameObject GetPhysicAnim()
	{
		return m_physicAnim;
	}

	public APAttack GetAttack(string name)
	{
		return Array.Find<APAttack>(m_attacks.m_attacks, array => string.Compare(array.m_name, name, true) == 0);
	}


	public bool IsCarried() { return m_onGround && m_carrier != null && m_carrier.enabled; }
	public APCarrier GetCarrier() { return m_carrier; }

	public bool IsFacingRight() { return m_motor.m_faceRight; }

	////////////////////////////////////////////////////////
	// PRIVATE/LOW LEVEL

	// list of different states
	public enum State
	{
		Standard = 0,
		Crouch,
		WallJump,
		WallJumpInAir,
		Attack,
		Glide,
		WallSlide,
		Shift
	}

	public enum StandardState
	{
		Unknown = 0,
		Stopped,
		Run,
		Walk,
		InAir
	}

	// finite state machine of character controller
	class Fsm : APFsm
	{
		/// Virtual method called when state enter, update or leave occur
		public override void OnStateUpdate(APFsmStateEvent eEvent, uint a_oState)
		{
			switch ((State)a_oState)
			{
				case State.Standard:
				m_controller.StateStandard(eEvent);
				break;
				case State.Crouch:
				m_controller.StateCrouch(eEvent);
				break;
				case State.WallJump:
				m_controller.StateWallJump(eEvent);
				break;
				case State.WallJumpInAir:
				m_controller.StateWallJumpInAir(eEvent);
				break;
				case State.Attack:
				m_controller.StateAttack(eEvent);
				break;
				case State.Glide:
				m_controller.StateGlide(eEvent);
				break;
				case State.WallSlide:
				m_controller.StateWallSlide(eEvent);
				break;
				case State.Shift:
				m_controller.StateShift(eEvent);
				break;
			}
		}

		public APCharacterController m_controller;
	}

	//	private attributes
	APCharacterMotor m_motor;
	Rigidbody2D m_rigidBody;
	Animator m_anim;
	Fsm m_fsm = new Fsm();
	Collider2D[] m_overlapResult = new Collider2D[8];
	List<APCharacterEventListener> m_eventListeners = new List<APCharacterEventListener>(1);

	// basics
	float m_groundSpeed;
	bool m_isControlled;
	bool m_onGround;
	float m_fGroundAngleSigned;
	float m_speedFactor;
	bool m_sliding;  // used only for animation
	GameObject m_lastGround;
	bool m_bForceDefer;
	StandardState m_standardState;

	//	jump
	float m_lastJumpTime;
	float m_animAirTime;
	float m_airTime;
	int m_airJumpCount;
	bool m_deferJump;
	float m_deferJumpMinHeight;
	float m_deferJumpMaxHeight;
	float m_jumpMinHeight;
	float m_jumpMaxHeight;
	bool m_stopExtraJump;

	bool m_hasPushedJumpAxis;
	float m_timePushedJumpAxis;
	bool m_needReleaseJumpAxis;

	// crouch
	float m_crouchBoxOriginalSize;
	float m_crouchBoxOriginalCenter;
	bool m_needUncrouch;

	// carry
	APCarrier m_carrier;

	// wall jump
	bool m_wallJumpFlipDone;
	bool m_wallJumpAutoRotate;

	// attacks
	APAttack m_curAttack;
	bool m_attackCrouched;
	bool m_attackNoMove;
	APAttack.AttackContext m_attackContext;
	State m_attackExitState;

	// wall slide
	bool m_bWallSlideStick;
	float m_wallSlideExitPushTime;

	// glide
	int m_glideCount;

	// deferring
	bool m_deferImpulse;
	bool m_deferVelocity;
	Vector2 m_deferImpulsePower;
	Vector2 m_deferedVelocity;

	// shift
	Shift m_curShift;
	float m_shiftDuration;
	bool m_shiftLaunched;
	int m_airShiftCount;

	// ground align
	Vector3 m_groundAlignLastPos;
	int m_groundAlignFrameCount;
	Vector2 m_groundAlignAxis;

	// edge grab
	GameObject m_edgeGrabSensor;

	// Use this for initialization
	void Awake()
	{
		m_motor = GetComponent<APCharacterMotor>();
		m_anim = GetComponent<Animator>();
		m_fsm.m_controller = this;
		m_rigidBody = GetComponent<Rigidbody2D>();

		// Create edge grab sensor and link it to us
		if (m_edgeGrab.m_enabled)
		{
			m_edgeGrabSensor = new GameObject("EdgeGrabSensor");
			m_edgeGrabSensor.layer = m_edgeGrab.m_layer.LayerIndex;
			m_edgeGrabSensor.AddComponent<APEdgeGrabTrigger>().SetCharacter(this);
			CircleCollider2D edgeGrabCollider = m_edgeGrabSensor.AddComponent<CircleCollider2D>();
			edgeGrabCollider.isTrigger = true;
			edgeGrabCollider.radius = m_edgeGrab.m_sensorRadius;
			m_edgeGrabSensor.transform.parent = this.transform;
			m_edgeGrabSensor.transform.localPosition = Vector3.zero;
		}
	}

	void OnDisable()
	{
		m_rigidBody.velocity = Vector2.zero;
		m_rigidBody.angularVelocity = 0f;

		m_fsm.StopFsm();
	}

	void ClearRuntimeValues()
	{
		m_carrier = null;
		m_isControlled = false;
		m_onGround = false;
		m_animAirTime = m_animations.m_minAirTime;
		m_airTime = 0f;
		m_speedFactor = 1f;
		m_lastJumpTime = 0f;
		m_groundAlignAxis = transform.up;
		m_crouchBoxOriginalSize = 1f;
		m_crouchBoxOriginalCenter = 0f;
		m_groundAlignLastPos = transform.position;
		m_groundAlignFrameCount = 0;
		m_fGroundAngleSigned = 0f;
		m_groundSpeed = 0f;
		m_sliding = false;
		m_curAttack = null;
		m_attackContext = null;
		m_attackCrouched = false;
		m_attackNoMove = false;
		m_airJumpCount = 0;
		m_lastGround = null;
		m_deferJump = false;
		m_bForceDefer = true;
		m_standardState = StandardState.Unknown;
		m_deferJumpMinHeight = m_deferJumpMaxHeight = 0f;
		m_deferImpulse = false;
		m_deferVelocity = false;
		m_deferedVelocity = Vector2.zero;
		m_glideCount = 0;
		m_airShiftCount = 0;
		m_deferImpulsePower = Vector2.zero;
		m_needUncrouch = false;
		m_curShift = null;
		m_bWallSlideStick = false;
		m_wallSlideExitPushTime = 0f;
		m_stopExtraJump = false;
		m_jumpMinHeight = m_jumpMaxHeight = 0f;
		m_hasPushedJumpAxis = false;
		m_needReleaseJumpAxis = false;
		m_timePushedJumpAxis = 0f;
		m_anim.speed = 1f;

		// Make sure rigidbody does not move
		m_rigidBody.velocity = Vector2.zero;
		m_rigidBody.angularVelocity = 0f;

		SetState(State.Standard);
	}

	void OnEnable()
	{
		ResetController();

		// Reset initial attack reference
		foreach (APAttackSwitcher curSwitcher in m_attacks.m_switchers)
		{
			curSwitcher.m_character = this;
			curSwitcher.Reset();
		}
	}

	void ResetController()
	{
		ClearRuntimeValues();

		// collect collision info at first frame
		m_motor.ClearRuntimeValues();
		m_motor.Move();
		UpdateGroundStatus();
	}

	// Physic update
	void FixedUpdate()
	{
		if (APSettings.m_fixedUpdate)
		{
			UpdateController();
			RefreshInputs(false);
		}
	}
	// Update is called once per frame
	void Update()
	{
		if (APSettings.m_fixedUpdate)
		{
			RefreshInputs(true);
		}
		else
		{
			RefreshInputs(true);
			UpdateController();
			RefreshInputs(false);
		}
	}

	void LateUpdate()
	{
		if (m_isControlled)
			return;

		HandleAnimation();
	}

	void UpdateController()
	{
		// Do nothing if game is paused
		if (Time.deltaTime == 0f)
			return;

		// Update inputs filtering values
		HandleInputFilter();

		// Carry is always active
		HandleCarry();

		// Handle buffered requests before
		m_bForceDefer = false;
		if (m_deferJump)
		{
			Jump(m_deferJumpMinHeight, m_deferJumpMaxHeight);
			m_deferJump = false;
		}
		if (m_deferImpulse)
		{
			AddImpulse(m_deferImpulsePower);
			m_deferImpulse = false;
			m_deferImpulsePower = Vector2.zero;
		}
		if (m_deferVelocity)
		{
			SetVelocity(m_deferedVelocity);
			m_deferVelocity = false;
			m_deferedVelocity = Vector2.zero;
		}

		// For now do nothing if controlled
		if (!IsControlled)
		{
			// Update states
			m_fsm.UpdateFsm(Time.deltaTime);

			// Restore deferring mode
			m_bForceDefer = true;

			// Finally move character with its current velocity
			m_motor.Move();
		}

		// Update our own touch ground status
		UpdateGroundStatus();
		RefreshStandardState();

		if (m_groundAlign.m_groundAlign && m_groundAlignFrameCount > 0)
		{
			m_groundAlignFrameCount--;
		}

		HandleAttackSwitchers();
	}

	void RefreshInputs(bool bSet)
	{
		// used to handle properly inputs within physic frame
		m_jump.m_button.Refresh(bSet);
		m_glide.m_button.Refresh(bSet);
		m_inputs.m_runButton.Refresh(bSet);
		m_groundAlign.m_oneWayGroundDownJumpButton.Refresh(bSet);

		// Handle jump with vertical axis
		if (bSet)
		{
			bool bIsPushing = IsPushingJumpAxis();
			if (m_needReleaseJumpAxis)
			{
				m_needReleaseJumpAxis = bIsPushing;
			}
			else if (bIsPushing)
			{
				m_hasPushedJumpAxis = true;
				m_timePushedJumpAxis = Time.time;
				m_needReleaseJumpAxis = true;
			}
		}
		else
		{
			if ((Time.time >= m_timePushedJumpAxis + m_jump.m_button.m_downPushDuration))
			{
				m_hasPushedJumpAxis = false;
			}
		}

		if (m_attacks.m_enabled)
		{
			foreach (APAttack curAttack in m_attacks.m_attacks)
			{
				curAttack.m_button.Refresh(bSet, curAttack.m_autoFire);
			}
		}

		if (m_attacks.m_enableAttackSwitchers)
		{
			foreach (APAttackSwitcher curSwitcher in m_attacks.m_switchers)
			{
				if(curSwitcher.IsActive())
				{
					APAttack curAttack = curSwitcher.GetCurrentAttack();
					curSwitcher.m_launchAttack.Refresh(bSet, curAttack != null ? curAttack.m_autoFire : false);
					curSwitcher.m_setPreviousAttack.Refresh(bSet);
					curSwitcher.m_setNextAttack.Refresh(bSet);
				}
			}
		}

		if (m_shift.m_enabled)
		{
			foreach (Shift curShift in m_shift.m_shifts)
			{
				curShift.m_button.Refresh(bSet);
			}
		}
	}

	void StateStandard(APFsmStateEvent eEvent)
	{
		switch (eEvent)
		{
			case APFsmStateEvent.eEnter:
			{
			}
			break;

			case APFsmStateEvent.eUpdate:
			{
				ApplyGravity();
				HandleCrouch();
				HandleHorizontalMove();
				HandleGlide();
				HandleWallSlide();
				HandleWallJump();
				HandleJump();
				HandleAttack();
				HandleShift();
				HandleAutoRotate();
				HandleStandardAnimation();
			}
			break;

			case APFsmStateEvent.eLeave:
			{
			}
			break;
		}
	}

	void StateGlide(APFsmStateEvent eEvent)
	{
		switch (eEvent)
		{
			case APFsmStateEvent.eEnter:
			{
				m_glideCount++;
				m_animAirTime = m_animations.m_minAirTime;
				PlayAnim(m_animations.m_glide, true);

				m_eventListeners.ForEach(e => e.OnGlideStart());
			}
			break;

			case APFsmStateEvent.eUpdate:
			{
				if (m_fsm.GetFsmStateTime() >= m_glide.m_maxDuration || !m_glide.m_button.GetButton() || m_onGround)
				{
					SetState(State.Standard);
				}

				ApplyGravity();
				HandleHorizontalMove();
				HandleWallSlide();
				HandleWallJump();
				HandleJump();
				HandleAttack();
				HandleShift();
				HandleAutoRotate();
			}
			break;

			case APFsmStateEvent.eLeave:
			{
				m_eventListeners.ForEach(e => e.OnGlideEnd());
			}
			break;
		}
	}

	void StateWallSlide(APFsmStateEvent eEvent)
	{
		switch (eEvent)
		{
			case APFsmStateEvent.eEnter:
			{
				m_wallSlideExitPushTime = 0f;
				m_bWallSlideStick = false;
			}
			break;

			case APFsmStateEvent.eUpdate:
			{
				float fWallFriction = 0f;
				if (ShouldWallSlide(out fWallFriction))
				{
					float stateTime = m_fsm.GetFsmStateTime();
					if (stateTime >= m_wallSlide.m_minTime)
					{
						// stick to wall if needed and launch animation
						if (!m_bWallSlideStick)
						{
							m_wallSlideExitPushTime = 0f;
							m_bWallSlideStick = true;
							PlayAnim(m_animations.m_wallSlide, true);
							m_eventListeners.ForEach(e => e.OnWallSlideStart());
						}

						// handle friction
						float fVerticalVel = ApplyDamping(m_motor.m_velocity.y, fWallFriction);
						m_motor.m_velocity.y = fVerticalVel;
					}
				}
				else
				{
					// leave state if no more respecting conditions
					SetState(State.Standard);
				}

				ApplyGravity();

				if (!m_bWallSlideStick)
					HandleHorizontalMove();

				HandleWallJump();

				if (!m_bWallSlideStick)
					HandleAutoRotate();
			}
			break;

			case APFsmStateEvent.eLeave:
			{
				m_bWallSlideStick = false;
				m_eventListeners.ForEach(e => e.OnWallSlideEnd());
			}
			break;
		}
	}

	void StateCrouch(APFsmStateEvent eEvent)
	{
		switch (eEvent)
		{
			case APFsmStateEvent.eEnter:
			{
				// go back to last crouch frame if coming from attack
				State ePrevState = GetPreviousState();

				if (IsAnimationSet(m_animations.m_crouch))
				{
					PlayAnim(m_animations.m_crouch, ePrevState == State.Attack ? 1f : 0f);
				}
				else
				{
					PlayAnim(m_animations.m_walkCrouch);
				}
				m_eventListeners.ForEach(e => e.OnCrouchStart());
			}
			break;

			case APFsmStateEvent.eUpdate:
			{
				ApplyGravity();
				HandleCrouch();
				HandleHorizontalMove();
				HandleJump();
				HandleAttack();

				if (m_basic.m_enableCrouchedRotate)
					HandleAutoRotate();

				if (CanMoveCrouched())
					HandleStandardAnimation();
			}
			break;

			case APFsmStateEvent.eLeave:
			{
				// restore collision box
				Vector2 orgSize = new Vector2(m_motor.GetBoxCollider().size.x, m_crouchBoxOriginalSize);
				Vector2 orgCenter = new Vector2(m_motor.GetBoxCollider().offset.x, m_crouchBoxOriginalCenter);
				m_motor.GetBoxCollider().size = orgSize;
				m_motor.GetBoxCollider().offset = orgCenter;

				// and motor
				m_motor.Scale = Vector2.one;
				m_motor.ScaleOffset = Vector2.zero;

				m_eventListeners.ForEach(e => e.OnCrouchEnd());
			}
			break;
		}
	}

	void StateWallJump(APFsmStateEvent eEvent)
	{
		switch (eEvent)
		{
			case APFsmStateEvent.eEnter:
			{
				PlayAnim(m_animations.m_wallJump, true);

				m_wallJumpAutoRotate = m_basic.m_autoRotate;
				m_airJumpCount = 0;
				m_glideCount = 0;
				m_airShiftCount = 0;
				m_lastJumpTime = Time.time;
				m_jumpMaxHeight = 0f;
				m_jumpMinHeight = 0f;
				m_stopExtraJump = true;
				m_animAirTime = m_animations.m_minAirTime;

				// prevent auto rotate for a while
				if (m_wallJumpAutoRotate)
					m_basic.m_autoRotate = false;

				m_eventListeners.ForEach(e => e.OnWallJumpStart());
			}
			break;

			case APFsmStateEvent.eUpdate:
			{
				// cancel any velocity
				m_motor.m_velocity = Vector2.zero;

				// wait for end of timer before effective jump
				if (m_fsm.GetFsmStateTime() >= m_wallJump.m_timeBeforeJump)
				{
					m_motor.m_velocity.y = m_wallJump.m_jumpPower.y;

					// Apply horizontal boost power (thanks to Claudio!)
					if ((m_motor.FaceRight && m_inputs.m_axisX.GetValue() < 0) ||
						(!m_motor.FaceRight && m_inputs.m_axisX.GetValue() > 0))
					{
						m_motor.m_velocity.x = (m_wallJump.m_jumpPower.x + m_wallJump.m_horizontalBooster) * (m_motor.FaceRight ? -1f : 1f);
					}
					else
					{
						m_motor.m_velocity.x = (m_wallJump.m_jumpPower.x) * (m_motor.FaceRight ? -1f : 1f);
					}

					SetState(State.WallJumpInAir);
				}
			}
			break;

			case APFsmStateEvent.eLeave:
			{
				// restore initial auto rotate in all cases
				m_basic.m_autoRotate = m_wallJumpAutoRotate;
				m_eventListeners.ForEach(e => e.OnWallJumpEnd());
			}
			break;
		}
	}

	void StateWallJumpInAir(APFsmStateEvent eEvent)
	{
		switch (eEvent)
		{
			case APFsmStateEvent.eEnter:
			{
				m_wallJumpAutoRotate = m_basic.m_autoRotate;
				m_wallJumpFlipDone = false;

				// prevent auto rotate for a while
				if (m_wallJumpAutoRotate)
					m_basic.m_autoRotate = false;
			}
			break;

			case APFsmStateEvent.eUpdate:
			{
				// go back to standard state as soon as touching ground
				if (m_onGround)
				{
					SetState(State.Standard);
				}
				else
				{
					// flip when needed
					bool bFlipEnabled = (m_wallJump.m_timeBeforeFlip >= 0f);
					if (!m_wallJumpFlipDone && bFlipEnabled && (m_fsm.GetFsmStateTime() >= m_wallJump.m_timeBeforeFlip))
					{
						m_motor.Flip();
						m_wallJumpFlipDone = true;
					}

					// enable back auto rotate if needed
					if (m_wallJumpAutoRotate && !m_basic.m_autoRotate && (m_fsm.GetFsmStateTime() >= m_wallJump.m_disableAutoRotateTime) && (m_wallJumpFlipDone || !bFlipEnabled))
					{
						m_basic.m_autoRotate = true;
					}

					// leave state if work is ended and no new state is requested
					if ((!bFlipEnabled || m_wallJumpFlipDone) && (!m_wallJumpAutoRotate || m_basic.m_autoRotate))
					{
						SetState(State.Standard);
					}

					ApplyGravity();
					HandleHorizontalMove();
					HandleAutoRotate();
				}
			}
			break;

			case APFsmStateEvent.eLeave:
			{
				// restore initial auto rotate in all cases
				m_basic.m_autoRotate = m_wallJumpAutoRotate;
			}
			break;
		}
	}

	void HandleAttack()
	{
		// check if attack allowed
		if (m_attacks.m_enabled && (IsCrouched() || (GetState() == State.Standard)) && !IsNewStateRequested())
		{
			// Handle current active attacks
			if(m_attacks.m_enableAttackSwitchers)
			{
				foreach (APAttackSwitcher curSwitcher in m_attacks.m_switchers)
				{
					if (curSwitcher.IsActive() && curSwitcher.m_launchAttack.GetButtonDown())
					{
						APAttack curAttack = curSwitcher.GetCurrentAttack();
						if (curAttack != null && curAttack.IsActive())
						{
							if (InitAttack(curAttack))
							{
								SetState(State.Attack);
								return;
							}
						}
					}
				}
			}

			// parse list of attack, check if one is launched with immediate button
			for (int i = 0; i < m_attacks.m_attacks.Length; i++)
			{
				APAttack curAttack = m_attacks.m_attacks[i];
				if (curAttack.m_enabled && curAttack.m_button.GetButtonDown())
				{
					if (InitAttack(curAttack))
					{
						SetState(State.Attack);
						return;
					}
				}
			}			
		}
	}

	bool IsCurrentAttackFiringAgain()
	{
		// Handle current active attacks
		if(m_attacks.m_enableAttackSwitchers)
		{
			foreach (APAttackSwitcher curSwitcher in m_attacks.m_switchers)
			{
				if (curSwitcher.IsActive() && (curSwitcher.GetCurrentAttack() == m_curAttack) && curSwitcher.m_launchAttack.GetButtonDown())
				{
					return true;
				}
			}
		}

		return m_curAttack.m_button.GetButtonDown();
	}

	void HandleAttackSwitchers()
	{
		if(m_attacks.m_enableAttackSwitchers && (GetState() == State.Standard))
		{
			foreach (APAttackSwitcher curSwitcher in m_attacks.m_switchers)
			{
				if (curSwitcher.IsActive())
				{
					curSwitcher.Manage();
				}
			}
		}
	}

	void RefreshAttackSwitchersAnimations()
	{
		if(m_attacks.m_enableAttackSwitchers && (GetState() == State.Standard))
		{
			foreach (APAttackSwitcher curSwitcher in m_attacks.m_switchers)
			{
				if (curSwitcher.IsActive())
				{
					curSwitcher.RefreshAnimations();
				}
			}
		}
	}

	bool IsAttackAimActive()
	{
		APAttack curAttack = GetCurrentAttack();
		if(curAttack != null)
		{
			return curAttack.m_useAimAnimations;
		}
		return false;
	}

	// Force attack to be launched
	public void LaunchAttack(APAttack attackToLaunch)
	{
		if (InitAttack(attackToLaunch))
		{
			SetState(State.Attack);
			return;
		}
	}

	bool InitAttack(APAttack newAttack)
	{
		m_attackContext = newAttack.GetCurrentContext(this);		

		// do nothing if context is disabled
		if (m_attackContext == null || !m_attackContext.m_enabled)
		{
			return false;
		}

		// handle stop on ground
		m_attackNoMove = m_attackContext.m_stopOnGround;

		// launch attack
		m_curAttack = newAttack;
		m_attackCrouched = IsCrouched();

		return true;
	}

	void StateAttack(APFsmStateEvent eEvent)
	{
		switch (eEvent)
		{
			case APFsmStateEvent.eEnter:
			{
				PlayAnim(m_attackContext.m_animFire, true);

				// clear buffer of hits & disable hit zones at init
				foreach (APHitZone curHitZone in m_curAttack.m_hitZones)
				{
					curHitZone.attackHits.Clear();
				}

				// Initialize exit state
				if (GetPreviousState() != State.Attack)
				{
					m_attackExitState = GetPreviousState();
				}

				m_eventListeners.ForEach(e => e.OnAttackStart(m_curAttack));
			}
			break;

			case APFsmStateEvent.eUpdate:
			{
				// update state
				ApplyGravity();
				HandleCrouch();
				HandleHorizontalMove();
				HandleGlide();
				HandleWallJump();
				HandleJump();
				HandleShift();
				HandleAutoRotate();				
				HandleStandardAnimation();

				// Compute all hits for current attack
				foreach (APHitZone curHitZone in m_curAttack.m_hitZones)
				{
					if (curHitZone.m_active && curHitZone.gameObject.activeInHierarchy)
					{
						Vector2 wsPos = curHitZone.transform.position;
						int hitCount = Physics2D.OverlapCircleNonAlloc(wsPos, curHitZone.m_radius, m_overlapResult, m_motor.m_rayLayer);
						for (int i = 0; i < hitCount; i++)
						{
							Collider2D curHit = m_overlapResult[i];

							// notify only hitable objects and not already hit by this hit zone
							APHitable hitable = curHit.GetComponent<APHitable>();
							if (hitable != null && !curHitZone.attackHits.Contains(hitable))
							{
								// alert hitable
								if (hitable.OnMeleeAttackHit(this, curHitZone))
								{
									m_eventListeners.ForEach(e => e.OnAttackMeleeHit(m_curAttack, hitable));
									curHitZone.attackHits.Add(hitable);
								}
							}
						}
					}
				}
								
				if (!IsNewStateRequested())
				{
					// check if animation has looped
					float fAnimTime = m_anim.GetCurrentAnimatorStateInfo(m_attackContext.m_animFire.m_layer).normalizedTime;
					if (fAnimTime >= 1f)
					{
						// stay in same state if player is still firing, restart current state
						if (IsCurrentAttackFiringAgain() && InitAttack(m_curAttack))
						{
							SetState(State.Attack);
						}
						else
						{
							SetState(m_attackExitState);
						}
					}
				}
			}
			break;

			case APFsmStateEvent.eLeave:
			{
				m_eventListeners.ForEach(e => e.OnAttackEnd(m_curAttack));
			}
			break;
		}
	}

	// called when ranged attack must launch a bullet
	void FireRangedAttack()
	{
		if (IsAttacking() && (m_attackContext != null) && (m_curAttack != null))
		{
			m_curAttack.FireBullet(this, m_attackContext);
		}
	}

	public APAttack GetCurrentAttack()
	{
		return IsAttacking() ? m_curAttack : null;
	}

	public bool IsAttacking()
	{
		State curState = GetState();
		State reqState = GetRequestedState();
		return (curState == State.Attack) || (reqState == State.Attack);
	}

	public bool IsAttackingStopped()
	{
		return IsAttacking() && m_attackNoMove;
	}

	public bool IsAttackingCrouched()
	{
		return IsAttacking() && m_attackCrouched;
	}

	public void SetState(State newState)
	{
		m_fsm.SetRequestedState((uint)newState);
	}

	public State GetState()
	{
		return (State)m_fsm.GetState();
	}

	public State GetRequestedState()
	{
		return (State)m_fsm.GetRequestedState();
	}

	public State GetPreviousState()
	{
		return (State)m_fsm.GetPreviousState();
	}

	void ApplyGravity()
	{
		float fFactor = (GetState() == State.Glide && m_motor.m_velocity.y <= 0f) ? m_glide.m_gravityFactor : 1f;
		if (!m_onGround)
		{
			float fDiff = m_basic.m_gravity * Time.deltaTime * fFactor;
			if (m_motor.m_velocity.y > -m_basic.m_maxFallSpeed)
			{
				m_motor.m_velocity.y = Mathf.Max(-m_basic.m_maxFallSpeed, m_motor.m_velocity.y - fDiff);
			}
		}
	}

	void HandleInputFilter()
	{
		m_inputs.m_axisX.Update(Time.deltaTime);
		m_inputs.m_axisY.Update(Time.deltaTime);

		// handle auto move here
		if (m_basic.m_autoMove != eAutoMove.Disabled)
		{
			bool bHalt = !string.IsNullOrEmpty(m_basic.m_haltAutoMove) && Input.GetButton(m_basic.m_haltAutoMove);
			m_inputs.m_axisX.SetForcedValue(true, bHalt ? 0f : (m_basic.m_autoMove == eAutoMove.Right ? 1f : -1f));
		}
		else
		{
			m_inputs.m_axisX.SetForcedValue(false, 0f);
		}
	}

	void UpdateGroundStatus()
	{
		// first check if we are on ground
		bool bCurrentGroundStatus = false;
		if (m_motor.TouchGround)
		{
			if (!m_onGround)
			{
				// snap on ground if we are close enough and if velocity is toward ground normal
				float fVelDotGround = Vector2.Dot(m_motor.m_velocity, m_motor.GetGroundNormal());
				if ((fVelDotGround < m_advanced.m_maxVerticalVelForSnap) && (m_motor.GetDistanceToGround() < 0.01f))
				{
					bCurrentGroundStatus = true;
				}
			}
			else
			{
				// previously on ground, keep snap on ground if normal or distance is ok and no jump at previous frame
				// NB : ground snapping with angle is enabled only with same ground game object
				Vector2 groundNormal = m_motor.GetGroundNormalLs();
				if ((m_lastJumpTime == 0f) && ((m_motor.GetDistanceToGround() < 0.01f) || (m_motor.GetDistanceToGround() < m_advanced.m_groundSnapMaxHeight && groundNormal.y > Mathf.Cos(m_advanced.m_groundSnapAngle) && m_lastGround == m_motor.GetGroundGameObject())))
				{
					bCurrentGroundStatus = true;
				}
			}
		}

		// we are on ground !
		if (bCurrentGroundStatus)
		{
			m_lastGround = m_motor.GetGroundGameObject();

			// snap on ground only in required state
			if (!IsControlled && (GetState() == State.Standard || IsCrouched()))
			{
				transform.position -= transform.up * m_motor.GetDistanceToGround();
			}

			// handle ground alignment
			if (!IsControlled && m_groundAlign.m_groundAlign && (m_groundAlignFrameCount <= 0))
			{
				Vector3 v3PosOffset = transform.position - m_groundAlignLastPos;
				if (v3PosOffset.sqrMagnitude > m_advanced.m_groundAlignUpdatePosThreshold * m_advanced.m_groundAlignUpdatePosThreshold)
				{
					m_groundAlignFrameCount = m_advanced.m_groundAlignUpdateFrame;
					m_groundAlignLastPos = transform.position;

					// compute smoothed ground normal
					Vector2 v2SmoothGroundNormal = Vector2.zero;
					uint normalCount = 0;
					foreach (APCharacterMotor.CharacterRay2D curRay in m_motor.m_RaysGround)
					{
						if (curRay.m_hitInfo.m_collider && curRay.m_hitInfo.m_penetration < m_advanced.m_groundAlignPenetrationThreshold)
						{
							v2SmoothGroundNormal += curRay.m_hitInfo.m_normal;
							normalCount++;
						}
					}

					if (normalCount > 0)
					{
						v2SmoothGroundNormal /= normalCount;
					}
					else
					{
						v2SmoothGroundNormal = m_motor.GetGroundNormal();
					}

					// update ground align axis only if delta angle is valid
					float fAngle = Vector2.Angle(m_groundAlignAxis, v2SmoothGroundNormal);

					// check angle delta
					if ((fAngle >= m_advanced.m_groundAlignDeltaAngleMin && fAngle <= m_advanced.m_groundAlignDeltaAngleMax))
					{
						// check absolute angle
						float fNewAngle = Vector2.Angle(Vector2.up, v2SmoothGroundNormal);
						if (fNewAngle <= m_groundAlign.m_maxAngle)
						{
							// update new ground axis
							m_groundAlignAxis = v2SmoothGroundNormal;
						}
					}
				}
			}

			// clear vertical velocity if we just touched ground
			if (bCurrentGroundStatus != m_onGround)
			{
				if (!IsControlled)
				{
					m_motor.m_velocity.y = 0f;
				}

				m_animAirTime = 0f;
				m_airJumpCount = 0;
				m_lastJumpTime = 0f;
				m_airTime = 0f;
				m_glideCount = 0;
				m_airShiftCount = 0;
			}
		}
		else
		{
			m_lastGround = null;
			m_animAirTime += Time.deltaTime;
			m_airTime += Time.deltaTime;

			if (!IsControlled && m_groundAlign.m_forceInAirVerticalAlign)
			{
				m_groundAlignAxis = Vector2.up;
			}
		}

		if (!IsControlled)
		{
			AlignGroundAxis();
		}

		// update current status
		if (m_onGround != bCurrentGroundStatus)
		{
			m_onGround = bCurrentGroundStatus;

			if (m_onGround)
			{
				m_eventListeners.ForEach(e => e.OnTouchGround());
			}
			else if (m_lastJumpTime == 0f)
			{
				m_eventListeners.ForEach(e => e.OnLeaveGround());
			}
		}
	}

	void AlignGroundAxis()
	{
		// update angular velocity to reach current ground align axis
		float fAngle = Vector2.Angle(transform.up, m_groundAlignAxis);
		float angleSign = 1f;
		if (Vector3.Cross(transform.up, m_groundAlignAxis).z < 0f)
		{
			angleSign = -1f;
		}

		m_rigidBody.angularVelocity = angleSign * Mathf.Min(fAngle / Time.deltaTime, m_groundAlign.m_rotateSpeed);
	}

	void HandleAutoRotate()
	{
		if (m_basic.m_autoRotate)
		{
			// handle max speed
			float fFlipMaxSpeed = m_onGround ? m_basic.m_groundFlipMaxSpeed : m_basic.m_airFlipMaxSpeed;
			if (fFlipMaxSpeed >= 0f)
			{
				float threshold = fFlipMaxSpeed + 0.001f;
				float fVelHorizontal = Vector2.Dot(m_motor.m_velocity, transform.right);
				if ((m_motor.FaceRight && fVelHorizontal > threshold) || (!m_motor.FaceRight && fVelHorizontal < -threshold))
					return;
			}

			// standard behavior
			if (m_inputs.m_axisX.GetValue() > 0f && !m_motor.FaceRight)
			{
				m_motor.Flip();
			}
			else if (m_inputs.m_axisX.GetValue() < 0f && m_motor.FaceRight)
			{
				m_motor.Flip();
			}
		}
	}

	bool GetInputRunning()
	{
		return m_basic.m_alwaysRun || (!m_inputs.m_runButton.IsSpecified() ? false : m_inputs.m_runButton.GetButton());
	}

	public float ComputeMaxSpeed()
	{
		if (!m_onGround)
		{
			return m_basic.m_maxAirSpeed;
		}
		else
		{
			return ComputeMaxGroundSpeed();
		}
	}

	public float ComputeMaxGroundSpeed()
	{
		float fSpeed = m_basic.m_walkSpeed;
		float fSpeedBackward = m_basic.m_walkSpeedBackward;
		if (IsCrouched())
		{
			fSpeed = m_basic.m_crouchSpeed;
			fSpeedBackward = m_basic.m_crouchSpeedBackward;
		}
		else if (GetInputRunning())
		{
			fSpeed = m_basic.m_runSpeed;
			fSpeedBackward = m_basic.m_runSpeedBackward;
		}

		return (!m_basic.m_autoRotate && IsMovingBackward() && fSpeedBackward > 0f) ? fSpeedBackward : fSpeed;
	}

	bool CanMoveCrouched()
	{
		return IsAnimationSet(m_animations.m_walkCrouch) && (m_basic.m_crouchSpeed > 0f);
	}

	void HandleHorizontalMove()
	{
		m_sliding = false;

		float maxSpeed = ComputeMaxSpeed();
		float absAxisX = Mathf.Abs(m_inputs.m_axisX.GetValue());
		bool bCrouched = IsCrouched();
		bool bCanMove = (bCrouched && CanMoveCrouched()) || GetState() == State.Standard;

		// compute horizontal velocity from input
		float fMoveDir = m_inputs.m_axisX.GetValue() != 0f ? Mathf.Sign(m_inputs.m_axisX.GetValue()) : (m_motor.FaceRight ? 1f : -1f);

		// compute slope factor
		m_speedFactor = 1f;
		bool downSlide = false;
		if (m_onGround)
		{
			float fGroundAngle = Mathf.Rad2Deg * Mathf.Acos(m_motor.GetGroundNormal().y);
			m_fGroundAngleSigned = fMoveDir != Mathf.Sign(m_motor.GetGroundNormal().x) ? fGroundAngle : -fGroundAngle;

			// handle auto down slide here
			if (m_downSlopeSliding.m_enabled && bCanMove && (absAxisX == 0f) && (fGroundAngle >= m_downSlopeSliding.m_slopeMinAngle))
			{
				// - force player to slide down
				fMoveDir = Mathf.Sign(m_motor.GetGroundNormal().x);
				absAxisX = m_downSlopeSliding.m_slidingPower;
				m_sliding = true; // this like a slide according to animation !
				downSlide = true;

				// stop any invalid velocity
				m_motor.m_velocity.x = fMoveDir > 0f ? Mathf.Max(0, m_motor.m_velocity.x) : Mathf.Min(0, m_motor.m_velocity.x);
			}
			else
			{
				m_speedFactor = Mathf.Clamp01(m_basic.m_slopeSpeedMultiplier.Evaluate(m_fGroundAngleSigned));
			}
		}

		// add edge slide velocity if needed
		if (m_onGround && bCanMove && !downSlide && (m_basic.m_edgeSlideFactor > absAxisX))
		{
			float fPushDir = HandleEdgeSlide();
			if (fPushDir != 0f)
			{
				absAxisX = Mathf.Min(m_basic.m_edgeSlideFactor, 1f);
				fMoveDir = fPushDir;
			}
		}

		// Compute move direction
		Vector2 hrzMoveDir = new Vector2(fMoveDir, 0f);
		float hrzMoveLength = absAxisX * maxSpeed * m_speedFactor;

		// align this velocity on ground plane if we touch the ground 		
		bool bAttacking = IsAttackingStopped();
		if (m_onGround)
		{
			float fDot = Vector2.Dot(hrzMoveDir, m_motor.GetGroundNormal());
			if (Mathf.Abs(fDot) > float.Epsilon)
			{
				Vector3 perpAxis = Vector3.Cross(hrzMoveDir, m_motor.GetGroundNormal());
				hrzMoveDir = Vector3.Cross(m_motor.GetGroundNormal(), perpAxis);
				hrzMoveDir.Normalize();
			}

			// cancel input if needed
			if (!downSlide && (bCrouched && !CanMoveCrouched() || bAttacking))
			{
				hrzMoveLength = 0f;
			}
		}
		else if (m_groundAlign.m_alignAirMove)
		{
			hrzMoveDir = transform.TransformDirection(hrzMoveDir);
		}

		// handle dynamic
		if (m_onGround)
		{
			float fDynFriction, fStaticFriction;
			ComputeFrictions(out fDynFriction, out fStaticFriction);

			// update sliding status
			bool bDynSliding = !(bCrouched && !CanMoveCrouched()) && !bAttacking && absAxisX > 0f;
			if ((bDynSliding && fDynFriction < 1f) || (!bDynSliding && fStaticFriction < 1f))
			{
				m_sliding = true;
			}

			float fVelOnMove = Vector2.Dot(m_motor.m_velocity, hrzMoveDir);
			float fDirLength = fVelOnMove;
			if (m_sliding && !downSlide)
			{
				// dynamic is different while sliding
				if (bDynSliding)
				{
					float fDiffMax = maxSpeed - fVelOnMove;
					if (fDiffMax > 0f)
					{
						fDirLength = fVelOnMove + Mathf.Min(fDiffMax, absAxisX * fDynFriction * Time.deltaTime * 20f);
					}
				}
				else
				{
					fDirLength = ApplyDamping(fVelOnMove, fStaticFriction * 5f);
				}
			}
			else
			{
				// raise deceleration for crouched/attack
				float fDecel = (!downSlide && (bCrouched || bAttacking)) ? m_basic.m_deceleration * 2f : m_basic.m_deceleration;
				fDirLength = APInputJoystick.Update(fVelOnMove, hrzMoveLength, m_basic.m_stopOnRotate, m_basic.m_acceleration, fDecel, Time.deltaTime);
			}

			// handle case where uncrouch speed is requested
			if (m_needUncrouch && Mathf.Abs(fDirLength) < m_basic.m_uncrouchMinSpeed)
			{
				fDirLength = m_basic.m_uncrouchMinSpeed;
				m_needUncrouch = false;
			}

			ClampValueWithDamping(ref fDirLength, m_advanced.m_maxVelDamping, -maxSpeed, maxSpeed);
			m_motor.m_velocity = hrzMoveDir * (fDirLength);

			// update ground speed
			m_groundSpeed = m_motor.m_velocity.magnitude;
		}
		else
		{
			// in air dynamic
			float fVelOnMove = Vector2.Dot(m_motor.m_velocity, hrzMoveDir);
			float fDiffVel = (hrzMoveLength - fVelOnMove);
			float fMaxAccel = m_basic.m_airPower * Time.deltaTime * (GetState() == State.Glide ? m_glide.m_lateralMoveFactor : 1f);
			fDiffVel = Mathf.Clamp(fDiffVel, -fMaxAccel, fMaxAccel);

			m_motor.m_velocity += hrzMoveDir * fDiffVel;

			// TODO : fix clamping when in air ground align is enabled
			ClampValueWithDamping(ref m_motor.m_velocity.x, m_advanced.m_maxAirVelDamping, -m_basic.m_maxAirSpeed, m_basic.m_maxAirSpeed);
			ClampValueWithDamping(ref m_motor.m_velocity.y, m_advanced.m_maxAirVelDamping, -m_basic.m_maxFallSpeed, float.MaxValue);
			m_groundSpeed = 0f;
		}
	}

	void ComputeFrictions(out float fDynFriction, out float fStaticFriction)
	{
		fDynFriction = 0f;
		fStaticFriction = 0f;

		// compute material override friction, keep highest value if different grounds are touched
		bool bOverride = false;
		for (int i = 0; i < m_motor.m_RaysGround.Length; i++)
		{
			if (m_motor.m_RaysGround[i].m_hitInfo.m_collider)
			{
				APMaterial groundMat = m_motor.m_RaysGround[i].m_hitInfo.m_material;
				if (groundMat != null && groundMat.m_overrideFriction)
				{
					bOverride = true;
					fDynFriction = Mathf.Max(fDynFriction, groundMat.m_dynFriction);
					fStaticFriction = Mathf.Max(fStaticFriction, groundMat.m_staticFriction);
				}
			}
		}

		// keep default value if no override
		if (!bOverride)
		{
			fDynFriction = m_basic.m_frictionDynamic;
			fStaticFriction = m_basic.m_frictionStatic;
		}
	}

	void ClampValueWithDamping(ref float fValue, float fDamping, float fMin, float fMax)
	{
		if (fValue > fMax)
		{
			fValue = Mathf.Max(fMax, ApplyDamping(fValue, fDamping));
		}
		else if (fValue < fMin)
		{
			fValue = Mathf.Min(fMin, ApplyDamping(fValue, fDamping));
		}
	}

	float ApplyDamping(float fValue, float fDampingValue)
	{
		float fDamping = Mathf.Exp(-fDampingValue * Time.deltaTime);
		return fValue * fDamping;
	}

	void HandleCarry()
	{
		m_carrier = null;
		if (m_onGround)
		{
			for (int i = 0; i < m_motor.m_RaysGround.Length; i++)
			{
				if (m_motor.m_RaysGround[i].m_hitInfo.m_collider)
				{
					APCarrier curCarrier = m_motor.m_RaysGround[i].m_hitInfo.m_collider.GetComponent<APCarrier>();
					if (curCarrier != null)
					{
						// use prefer index if carrier is already assigned
						if ((m_carrier == null) || (i == m_advanced.m_carryRayIndex))
						{
							m_carrier = curCarrier;
						}
					}
				}
			}
		}
	}

	void HandleGlide()
	{
		if (m_glide.m_enabled && !IsNewStateRequested() && !m_onGround)
		{
			// additional glides must release input
			bool bGlideInput = m_glideCount == 0 ? m_glide.m_button.GetButton() : m_glide.m_button.GetButtonDown();
			if (bGlideInput && (m_glideCount < m_glide.m_maxCount) && (m_airTime >= m_glide.m_minAirTimeBeforeGlide))
			{
				SetState(State.Glide);
			}
		}
	}

	void HandleJump()
	{
		// Handle OneWayGround down jump first
		if (m_groundAlign.m_oneWayDownJump && m_groundAlign.m_oneWayGroundDownJumpButton.GetButtonDown() && GetMotor().IgnoreOnOneWayGround())
		{
			Uncrouch();
			return;
		}

		// check if we should jump
		if (m_jump.m_enabled && HasPushedJump() && (Time.time - m_lastJumpTime > m_advanced.m_minTimeBetweenTwoJumps))
		{
			if (m_onGround || (m_jump.m_airJumpCount > m_airJumpCount))
			{
				// make sure we can jump if crouched
				if (!IsCrouched() || CanUncrouch())
				{
					if (!m_onGround)
					{
						m_airJumpCount++;
					}

					Uncrouch();
					Jump(m_jump.m_minHeight, m_jump.m_maxHeight);

					// Launch jump event
					m_eventListeners.ForEach(e => e.OnJump());
					return;
				}
			}
		}

		// handle extra jumping
		if (m_lastJumpTime > 0f && !m_stopExtraJump)
		{
			m_stopExtraJump = !IsPushingJump() || m_motor.TouchHead || m_onGround;
			if (!m_stopExtraJump)
			{
				float fJumpVerticalSpeed = ComputeJumpVerticalSpeed(m_jumpMinHeight);
				if ((fJumpVerticalSpeed > Mathf.Epsilon) && (Time.time < m_lastJumpTime + (m_jumpMaxHeight - m_jumpMinHeight) / fJumpVerticalSpeed))
				{
					// remove gravity
					m_motor.m_velocity += Vector2.up * m_basic.m_gravity * Time.deltaTime;
				}
			}
		}
	}

	bool IsPushingJumpAxis()
	{
		return m_inputs.m_axisY.GetValue() > 0.1f;
	}

	bool IsPushingJump()
	{
		return m_jump.m_button.GetButton() || (m_jump.m_jumpWithVerticalAxis && IsPushingJumpAxis());
	}

	bool HasPushedJump()
	{
		return m_jump.m_button.GetButtonDown() || (m_jump.m_jumpWithVerticalAxis && m_hasPushedJumpAxis);
	}

	// add an impulse to character and leave its current state
	public void AddImpulse(Vector2 impulse)
	{
		if (m_bForceDefer)
		{
			m_deferImpulse = true;
			m_deferImpulsePower += impulse;
		}
		else
		{
			LeaveAnyState();
			m_onGround = false;
			m_motor.m_velocity += impulse;
		}
	}

	// set character velocity and leave its current state
	public void SetVelocity(Vector2 velocity)
	{
		if (m_bForceDefer)
		{
			m_deferVelocity = true;
			m_deferedVelocity = velocity;
		}
		else
		{
			LeaveAnyState();
			m_onGround = false;
			m_motor.m_velocity = velocity;
		}
	}

	// get character velocity
	public Vector2 GetVelocity()
	{
		return m_motor.m_velocity;
	}

	// force character to jump immediately at minimum height * specified ratio
	public void Jump(float fMinHeight, float fMaxHeight)
	{
		// handle case where jump is requested inside a callback, buffer action in this case
		if (m_bForceDefer)
		{
			m_deferJump = true;
			m_deferJumpMinHeight = fMinHeight;
			m_deferJumpMaxHeight = fMaxHeight;
		}
		else
		{
			m_lastJumpTime = Time.time;
			m_animAirTime = m_animations.m_minAirTime;
			m_jumpMinHeight = fMinHeight;
			m_jumpMaxHeight = fMaxHeight;
			m_stopExtraJump = false; // to stop high jump as soon as button is released

			float fJumpSpeed = ComputeJumpVerticalSpeed(m_jumpMinHeight);
			if (m_groundAlign.m_jumpAlign && m_onGround)
			{
				float fDot = Vector2.Dot(m_motor.GetGroundNormal(), m_motor.m_velocity);
				m_motor.m_velocity += m_motor.GetGroundNormal() * (fJumpSpeed - fDot);
			}
			else
			{
				m_motor.m_velocity.y = fJumpSpeed;
			}

			// inject carrier horizontal velocity
			if (IsCarried())
			{
				m_motor.m_velocity.x += m_carrier.ComputeVelocityAt(transform.position).x;
			}

			// inject air jump addition horizontal impulse
			if (!m_onGround && Mathf.Abs(m_inputs.m_axisX.GetValue()) > 0f)
			{
				m_motor.m_velocity.x = IsFacingRight() ? Mathf.Max(m_jump.m_airJumpHorizontalPower, m_motor.m_velocity.x) :
						Mathf.Min(-m_jump.m_airJumpHorizontalPower, m_motor.m_velocity.x);
			}

			// launch jump animation (use inair if empty)
			PlayAnim(!IsAnimationSet(m_animations.m_jump) ? m_animations.m_inAir : m_animations.m_jump, true);

			// make sure to go back to Standard state
			LeaveAnyState();
			m_onGround = false;
		}
	}

	public void LeaveAnyState()
	{
		// make sure to go back to Standard state
		State curState = GetState();
		if ((curState != State.Standard) && (!IsNewStateRequested() || GetRequestedState() != State.Standard))
		{
			if (IsCrouched())
			{
				// crouch special case
				if (CanUncrouch())
				{
					Uncrouch();
				}
			}
			else
			{
				SetState(State.Standard);
			}
		}
	}

	void HandleWallJump()
	{
		// early exit
		if (!m_jump.m_button.IsSpecified() || !HasPushedJump() || IsCrouched() || m_onGround || IsNewStateRequested())
			return;

		// check if touching wall
		bool bHit = false;
		uint iHitCount = 0;
		APMaterial.BoolValue bMaterialSnap = APMaterial.BoolValue.Default;

		for (int i = 0; i < m_motor.m_RaysFront.Length; i++)
		{
			// check if this ray should be tested
			bool bTestRay = (m_wallJump.m_rayIndexes.Length == 0);
			for (int j = 0; j < m_wallJump.m_rayIndexes.Length; j++)
			{
				if (m_wallJump.m_rayIndexes[j] == i)
				{
					bTestRay = true;
					break;
				}
			}

			if (bTestRay && m_motor.m_RaysFront[i].m_hitInfo.m_collider)
			{
				iHitCount++;

				// use hit material value, true has priority
				APMaterial hitMat = m_motor.m_RaysFront[i].m_hitInfo.m_material;
				if (hitMat != null)
				{
					if (hitMat.m_wallJump == APMaterial.BoolValue.True)
					{
						bMaterialSnap = APMaterial.BoolValue.True;
					}
					else if (hitMat.m_wallJump == APMaterial.BoolValue.False && bMaterialSnap != APMaterial.BoolValue.True)
					{
						bMaterialSnap = APMaterial.BoolValue.False;
					}
				}
			}
		}

		if (m_wallJump.m_rayIndexes.Length == 0)
			bHit = (iHitCount == m_motor.m_RaysFront.Length);
		else
			bHit = (iHitCount == m_wallJump.m_rayIndexes.Length);


		// effective jump
		bool bCanJump = bHit && (bMaterialSnap != APMaterial.BoolValue.False) && (bMaterialSnap == APMaterial.BoolValue.True || m_wallJump.m_enabled);
		if (bCanJump && (Time.time - m_lastJumpTime > m_advanced.m_minTimeBetweenTwoJumps))
		{
			// put in wall jump state
			SetState(State.WallJump);
		}
	}

	void HandleWallSlide()
	{
		// early exit
		if (IsNewStateRequested())
		{
			return;
		}

		// check conditions
		float wallFriction;
		if (ShouldWallSlide(out wallFriction))
		{
			SetState(State.WallSlide);
		}
	}

	bool ShouldWallSlide(out float wallFriction)
	{
		wallFriction = 0f;

		// early exit
		if (IsCrouched() || m_onGround || (!m_bWallSlideStick && m_motor.m_velocity.y > -m_wallSlide.m_minSpeed))
		{
			return false;
		}

		bool bMoveForward = IsMovingForward();
		bool bMoveBackward = IsMovingBackward();
		if (!m_bWallSlideStick)
		{
			// not still stick, exit if not pushing inputs forward
			if (m_wallSlide.m_mustMoveForwardToStick && !bMoveForward)
			{
				return false;
			}
		}

		// check if touching front wall, save its friction value if any
		bool bWallSlide = false;
		uint iHitCount = 0;
		bool bUseWallFriction = false;
		APMaterial.BoolValue bMaterialWallSlide = APMaterial.BoolValue.Default;
		float fWallFriction = 0f;

		for (int i = 0; i < m_motor.m_RaysFront.Length; i++)
		{
			// check if this ray should be tested
			bool bTestRay = (m_wallSlide.m_rayIndexes.Length == 0);
			foreach (int curRayIndex in m_wallSlide.m_rayIndexes)
			{
				if (curRayIndex == i)
				{
					bTestRay = true;
					break;
				}
			}

			if (bTestRay && m_motor.m_RaysFront[i].m_hitInfo.m_collider)
			{
				iHitCount++;

				// use collider material value if any, keep highest friction
				APMaterial hitMat = m_motor.m_RaysFront[i].m_hitInfo.m_material;
				if (hitMat != null)
				{
					if (hitMat.m_wallFriction.m_override)
					{
						bUseWallFriction = true;
						fWallFriction = Mathf.Max(fWallFriction, hitMat.m_wallFriction.m_value);
					}

					if (hitMat.m_wallSlide == APMaterial.BoolValue.True)
					{
						bMaterialWallSlide = APMaterial.BoolValue.True;
					}
					else if (hitMat.m_wallSlide == APMaterial.BoolValue.False && bMaterialWallSlide != APMaterial.BoolValue.True)
					{
						bMaterialWallSlide = APMaterial.BoolValue.False;
					}
				}
			}
		}

		if (m_wallSlide.m_rayIndexes.Length == 0)
			bWallSlide = (iHitCount == m_motor.m_RaysFront.Length);
		else
			bWallSlide = (iHitCount == m_wallSlide.m_rayIndexes.Length);

		// make sure material allows it if any
		bWallSlide &= (bMaterialWallSlide != APMaterial.BoolValue.False) && (bMaterialWallSlide == APMaterial.BoolValue.True || m_wallSlide.m_enabled);

		// update friction and result
		wallFriction = bUseWallFriction ? fWallFriction : m_wallSlide.m_friction;

		// check if pushing in opposite direction if currently sticked
		if (bWallSlide && m_bWallSlideStick)
		{
			if ((m_wallSlide.m_mustMoveBackwardToUnstick && bMoveBackward) || (!m_wallSlide.m_mustMoveBackwardToUnstick && !bMoveForward && m_wallSlide.m_mustMoveForwardToStick))
			{
				m_wallSlideExitPushTime += Time.deltaTime;
				if (m_wallSlideExitPushTime >= m_wallSlide.m_exitPushTime)
					return false;
			}
			else
			{
				// clear exit push timer
				m_wallSlideExitPushTime = 0f;
			}
		}

		return bWallSlide;
	}

	bool GetInputCrouch()
	{
		return m_basic.m_enableCrouch && (m_inputs.m_axisY.GetValue() < -0.5f);
	}

	void HandleCrouch()
	{
		// Do not change state if currently switching or no crouch animation
		if (IsNewStateRequested() || (!IsAnimationSet(m_animations.m_crouch) && !IsAnimationSet(m_animations.m_walkCrouch)))
			return;

		m_needUncrouch = false;

		bool bCrouched = IsCrouched();
		bool bInputCrouch = GetInputCrouch();

		// crouch if needed
		if (!bCrouched && m_onGround && bInputCrouch)
		{
			Crouch();
		}
		else if (bCrouched && !bInputCrouch)
		{
			// uncrouch as soon as we can
			if (CanUncrouch())
			{
				Uncrouch();
			}
			else
			{
				m_needUncrouch = true;
			}
		}
	}

	bool CanUncrouch()
	{
		// make sure expanded box does not collide
		Vector2 orgSize = new Vector2(m_motor.GetBoxCollider().size.x, m_crouchBoxOriginalSize);
		Vector2 orgCenter = new Vector2(m_motor.GetBoxCollider().offset.x, m_crouchBoxOriginalCenter);

		float fScale = 0.75f; // global scale to allow tolerance
		float fBoxTop = (orgCenter + 0.5f * orgSize * m_motor.m_autoBuilder.m_rayYBoxScale.y).y; // use AutoBuilder setting for top height detection
		Vector2 boxA = m_motor.GetBoxCollider().offset + Vector2.Scale(m_motor.GetBoxCollider().size * fScale, new Vector2(-0.5f, 0.5f));
		Vector2 boxB = new Vector2(boxA.x + m_motor.GetBoxCollider().size.x * fScale, fBoxTop);
		boxA = transform.TransformPoint(boxA);
		boxB = transform.TransformPoint(boxB);

		if (m_advanced.m_debugDraw)
		{
			Debug.DrawLine(boxA, new Vector2(boxA.x, boxB.y), Color.red);
			Debug.DrawLine(new Vector2(boxA.x, boxB.y), boxB, Color.red);
			Debug.DrawLine(boxB, new Vector2(boxB.x, boxA.y), Color.red);
			Debug.DrawLine(new Vector2(boxB.x, boxA.y), boxA, Color.red);
		}

		int hitCount = Physics2D.OverlapAreaNonAlloc(boxA, boxB, m_overlapResult, m_motor.m_rayLayer);
		for (int i = 0; i < hitCount; i++)
		{
			// ignore character himself
			if (m_overlapResult[i] == GetComponent<Collider2D>())
				continue;

			// ignore triggers & handle ignored colliders
			if (m_overlapResult[i].isTrigger || m_motor.IsIgnoredCollider(m_overlapResult[i]))
				continue;

			// ignore oneway grounds
			if (m_motor.IsOneWayGround(m_overlapResult[i]))
				continue;

			// all other colliders prevent uncrouch
			return false;
		}
		return true;
	}

	void Crouch()
	{
		if (!IsCrouched())
		{
			// reduce collision box (and stay on same base plane)
			Vector2 curBoxSize = m_motor.GetBoxCollider().size;
			Vector2 curBoxCenter = m_motor.GetBoxCollider().offset;
			m_crouchBoxOriginalSize = curBoxSize.y;
			m_crouchBoxOriginalCenter = curBoxCenter.y;

			Vector2 newSize = new Vector2(curBoxSize.x, curBoxSize.y * m_basic.m_crouchSizePercent);
			m_motor.GetBoxCollider().size = newSize;

			// compute new box center
			Vector2 newCenter = curBoxCenter;
			float fOffset = curBoxCenter.y - curBoxSize.y * 0.5f;
			newCenter.y -= fOffset;
			newCenter.y *= m_basic.m_crouchSizePercent;
			newCenter.y += fOffset;
			m_motor.GetBoxCollider().offset = newCenter;

			// do the same for the rays
			m_motor.Scale = new Vector2(1f, m_basic.m_crouchSizePercent);
			m_motor.ScaleOffset = new Vector2(0f, fOffset);

			// play anim
			SetState(State.Crouch);
		}
	}

	void Uncrouch()
	{
		if (IsCrouched())
		{
			SetState(State.Standard);
		}
	}

	float ComputeJumpVerticalSpeed(float targetJumpHeight)
	{
		// estimation of speed for required height
		return Mathf.Sqrt(2 * targetJumpHeight * m_basic.m_gravity);
	}

	void HandleShift()
	{
		// Early exit
		if (!m_shift.m_enabled || IsNewStateRequested())
			return;

		// Handle max shift in air
		if (!m_onGround && m_airShiftCount >= m_shift.m_maxCountInAir)
			return;

		// Check if a shift button is pushed
		foreach (Shift curShift in m_shift.m_shifts)
		{
			if (curShift.m_button.GetButtonDown() && (!curShift.m_inAirOnly || !m_onGround))
			{
				// go to shift state
				m_curShift = curShift;
				SetState(State.Shift);
				return;
			}
		}
	}

	void StateShift(APFsmStateEvent eEvent)
	{
		switch (eEvent)
		{
			case APFsmStateEvent.eEnter:
			{
				// stop & setup
				if (!m_onGround)
					m_airShiftCount++;

				m_motor.m_velocity = Vector2.zero;
				m_shiftLaunched = false;
				m_shiftDuration = m_curShift.m_moveSpeed > Mathf.Epsilon ? m_curShift.m_pauseTime + m_curShift.m_moveLength / m_curShift.m_moveSpeed : 0f;
				PlayAnim(m_curShift.m_anim, true);

				m_eventListeners.ForEach(e => e.OnShiftStart());
			}
			break;

			case APFsmStateEvent.eUpdate:
			{
				// launch animation
				if (!m_shiftLaunched && m_fsm.GetFsmStateTime() >= m_curShift.m_pauseTime)
				{
					m_shiftLaunched = true;
				}

				// move
				if (m_shiftLaunched)
				{
					float fAngle = -Mathf.Deg2Rad * m_curShift.m_moveAngle;
					Vector2 v2MoveDir = new Vector2(Mathf.Cos(fAngle), -Mathf.Sin(fAngle));
					if (IsFacingRight() && v2MoveDir.x < 0f || !IsFacingRight() && v2MoveDir.x > 0f)
					{
						v2MoveDir.x = -v2MoveDir.x;
					}

					v2MoveDir = transform.TransformDirection(v2MoveDir);
					m_motor.m_velocity = v2MoveDir * m_curShift.m_moveSpeed;

					bool bLeave = false;
					if (m_curShift.m_stopOnHit)
					{
						bLeave = CheckOppositeHit(m_motor.m_RaysBack) ||
								CheckOppositeHit(m_motor.m_RaysFront) ||
								CheckOppositeHit(m_motor.m_RaysGround) ||
								CheckOppositeHit(m_motor.m_RaysUp);
					}

					// exit if needed
					if ((m_fsm.GetFsmStateTime() >= m_shiftDuration + m_curShift.m_pauseTime) || bLeave)
					{
						m_motor.m_velocity = v2MoveDir * m_curShift.m_exitSpeed;
						SetState(State.Standard);
					}
				}
			}
			break;

			case APFsmStateEvent.eLeave:
			{
				m_eventListeners.ForEach(e => e.OnShiftEnd());
			}
			break;
		}
	}

	bool CheckOppositeHit(APCharacterMotor.CharacterRay2D[] rays)
	{
		foreach (APCharacterMotor.CharacterRay2D ray in rays)
		{
			if (ray.m_hitInfo.m_collider && ray.m_hitInfo.m_penetration < 0.01f)
			{
				// make sure normal is in opposite direction of movement
				if (Vector2.Dot(ray.m_hitInfo.m_normal, m_motor.m_velocity) < 0f)
				{
					return true;
				}
			}
		}
		return false;
	}

	float HandleEdgeSlide()
	{
		if (m_basic.m_edgeSlideMinRayOut > 0)
		{
			APCharacterMotor oMotor = GetMotor();
			GameObject groundObject = m_motor.GetGroundGameObject();

			int iRayOutCount = 0;
			float farthestRay = 0;
			Vector2 slideDir = Vector2.zero;
			foreach (APCharacterMotor.CharacterRay2D curRay in oMotor.m_RaysGround)
			{
				if (curRay.m_hitInfo.m_collider == null || (curRay.m_hitInfo.m_collider.gameObject != groundObject))
				{
					iRayOutCount++;

					// keep farthest ray from player
					Vector2 rayDir = oMotor.GetRayPositionWs(curRay) - (Vector2)transform.position;
					rayDir.y = 0f;
					float dirLength = rayDir.sqrMagnitude;
					if (dirLength > farthestRay + Mathf.Epsilon)
					{
						farthestRay = dirLength;
						slideDir = rayDir;
					}
				}
			}

			if (iRayOutCount >= m_basic.m_edgeSlideMinRayOut)
			{
				return Mathf.Sign(slideDir.x);
			}
		}

		return 0f;
	}

	public bool IsControlled
	{
		get
		{
			return m_isControlled;
		}
		set
		{
			// reset runtime values when no more controlled
			if (m_isControlled != value)
			{
				if (!value)
				{
					ResetController();
				}
				else
				{
					// clear any velocity when giving hand and leave current state
					m_rigidBody.velocity = Vector2.zero;
					m_rigidBody.angularVelocity = 0f;

					m_fsm.StopFsm();
				}
			}

			m_isControlled = value;
		}
	}

	bool IsTouchingFrontWall()
	{
		foreach (APCharacterMotor.CharacterRay2D curRay in GetMotor().m_RaysFront)
		{
			if (curRay.m_hitInfo.m_collider != null && curRay.m_hitInfo.m_penetration < 0.01f)
			{
				// make sure this is a blocking wall
				if (curRay.m_hitInfo.m_normal.y < 0.1f)
					return true;
			}
		}
		return false;
	}

	public bool IsMovingForward()
	{
		return (m_inputs.m_axisX.GetValue() > 0.5f && m_motor.m_faceRight) || (m_inputs.m_axisX.GetValue() < -0.5f && !m_motor.m_faceRight);
	}

	public bool IsMovingBackward()
	{
		return (m_inputs.m_axisX.GetValue() > 0.5f && !m_motor.m_faceRight) || (m_inputs.m_axisX.GetValue() < -0.5f && m_motor.m_faceRight);
	}

	void HandleAnimation()
	{
		// setup some scenaric values
		m_anim.SetFloat("VerticalSpeed", Vector2.Dot(m_motor.m_velocity, transform.up));
		m_anim.SetBool("OnGround", m_onGround);
		m_anim.SetFloat("GroundSpeed", m_groundSpeed);
		m_anim.SetFloat("GroundAngleSigned", m_fGroundAngleSigned);
		m_anim.SetInteger("State", (int)GetState());
	}

	void RefreshStandardState()
	{
		m_standardState = StandardState.Unknown;		

		if (m_onGround || m_animAirTime < m_animations.m_minAirTime)
		{
			// Special case were attacking stopped
			if(IsAttacking() && m_attackContext.m_stopOnGround)
			{
				m_standardState = StandardState.Stopped;
				m_anim.SetFloat("AnimWalkSpeed", 0f);
				return;
			}

			// check if moving against a wall
			bool bStopOnWall = m_animations.m_stopOnFrontWall && IsTouchingFrontWall();

			// mode from input
			float fFilteredInput = Mathf.Abs(m_inputs.m_axisX.GetSmoothValue());
			if (m_animations.m_walkAnimFromInput)
			{
				float fSpeed = m_animations.m_animFromInput.Evaluate(fFilteredInput);
				if (fSpeed < 1e-2f || bStopOnWall)
				{					
					m_standardState = StandardState.Stopped;
					m_anim.SetFloat("AnimWalkSpeed", 0f);
				}
				else
				{
					bool bShouldRun = GetInputRunning();
					if (bShouldRun)
					{
						m_standardState = StandardState.Run;
					}
					else
					{
						m_standardState = StandardState.Walk;
					}
					m_anim.SetFloat("AnimWalkSpeed", Mathf.Abs(fSpeed));
				}
			}
			else
			{
				// compute speed on ground/in air
				float fGroundSpeed = m_onGround ? m_motor.ComputeVelocityOnGround().magnitude : Mathf.Abs(m_motor.m_velocity.x);

				float fSpeedFromInput = m_animations.m_animFromInput.Evaluate(fFilteredInput);
				float fSpeedFromGround = m_animations.m_animFromSpeed.Evaluate(fGroundSpeed);
				if (bStopOnWall || (fSpeedFromInput < 1e-3f && m_sliding) || (!m_sliding && fSpeedFromGround < 1e-3f && m_inputs.m_axisX.GetValue() == 0f))
				{
					m_standardState = StandardState.Stopped;
					m_anim.SetFloat("AnimWalkSpeed", 0f);
				}
				else
				{
					bool bShouldRun = GetInputRunning();
					if (bShouldRun)
					{
						m_standardState = StandardState.Run;
					}
					else
					{
						m_standardState = StandardState.Walk;
					}

					float fSpeed = 0f;
					if (m_sliding)
					{
						fSpeed = fSpeedFromInput;
					}
					else
					{
						fSpeed = fSpeedFromGround;
					}

					m_anim.SetFloat("AnimWalkSpeed", Mathf.Abs(fSpeed));
				}
			}
		}
		else
		{
			m_standardState = StandardState.InAir;
		}
	}

	void HandleStandardAnimation()
	{
		// Lets new state handling animation
		if(IsNewStateRequested())
		{
			return;
		}

		// Do nothing if current active attack overrides standard animations
		if(!IsAttacking() || IsAttackAimActive())
		{
			switch(m_standardState)
			{
				case StandardState.Stopped:
				{
					if(!IsCrouched())
					{
						PlayAnim(m_animations.m_stand);				
					}
				}
				break;

				case StandardState.Walk:
				case StandardState.Run:
				{
					if (IsCrouched())
					{
						// wait for crouch anim to end
						if(IsAnimationPlaying(m_animations.m_crouch) && IsAnimationEnded(m_animations.m_crouch))
						{
							UpdateStandardAnim(m_animations.m_walkCrouch, m_animations.m_walkCrouchBackward);
						}
					}
					else if (m_standardState == StandardState.Run)
					{
						UpdateStandardAnim(m_animations.m_run, m_animations.m_runBackward);
					}
					else
					{
						UpdateStandardAnim(m_animations.m_walk, m_animations.m_walkBackward);
					}
				}
				break;

				case StandardState.InAir:
				{
					PlayAnim(m_animations.m_inAir);
				}
				break;
			}

			RefreshAttackSwitchersAnimations();
		}
	}

	private void UpdateStandardAnim(APAnimation forwardAnim, APAnimation backwardAnim)
	{
		if (m_animations.m_walkAnimFromInput && !m_basic.m_autoRotate && IsMovingBackward())
		{
			PlayAnim(backwardAnim);
		}
		else
		{
			PlayAnim(forwardAnim);
		}
	}
	
	public void PlayAnim(APAnimation anim, bool forceRestart = false)
	{
		int animHash = anim.GetAnimHash();
		if((animHash != 0) && (forceRestart || !IsAnimationPlaying(anim)))
		{
			m_anim.Play(animHash, anim.m_layer, forceRestart ? 0f : float.NegativeInfinity);
		}
	}

	public void PlayAnim(APAnimation anim, float normalizedTime)
	{
		int animHash = anim.GetAnimHash();
		if(animHash != 0)
		{
			m_anim.Play(animHash, anim.m_layer, normalizedTime);
		}
	}

	public bool IsAnimationPlaying(APAnimation anim)
	{
		AnimatorStateInfo layerState = m_anim.GetCurrentAnimatorStateInfo(anim.m_layer);
		return layerState.shortNameHash == anim.GetAnimHash();
	}

	public bool IsAnimationEnded(APAnimation anim)
	{
		AnimatorStateInfo layerState = m_anim.GetCurrentAnimatorStateInfo(anim.m_layer);
		return layerState.normalizedTime >= 1f;
	}

	public bool IsAnimationSet(APAnimation anims)
	{
		return anims.GetAnimHash() != 0;
	}

	public bool IsNewStateRequested()
	{
		return m_fsm.IsNewStateRequested();
	}

	public List<APCharacterEventListener> EventListeners
	{
		get
		{
			return m_eventListeners;
		}
	}
}