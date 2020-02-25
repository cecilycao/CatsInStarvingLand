/* Copyright (c) 2014 Advanced Platformer 2D */
using UnityEngine;
using System.Collections.Generic;


public partial class APCharacterController : MonoBehaviour
{
	////////////////////////////////////////////////////////
	// PUBLIC/HIGH LEVEL
	[System.Serializable]
	public class Inputs
	{
		public APInputJoystick m_axisX = new APInputJoystick("Horizontal", true);
		public APInputJoystick m_axisY = new APInputJoystick("Vertical", false);
		public APInputButton m_runButton;
	}

	public enum eAutoMove
	{
		Disabled,
		Right,
		Left
	}

	[System.Serializable]
	public class Settings
	{
		public bool m_alwaysRun = true;							// run always enabled
		public float m_walkSpeed = 5f;							// speed when walking (in m/s)
		public float m_runSpeed = 8f;							// speed when running (in m/s)
		public float m_walkSpeedBackward = 0f;					// speed when walking backward (in m/s), same as m_walkSpeed if zero
		public float m_runSpeedBackward = 0f;					// speed when running backward (in m/s), same as m_runSpeed if zero
		public float m_acceleration = 30f;						// in m/s²
		public float m_deceleration = 20f;						// in m/s²
		public bool m_stopOnRotate = true;						// directly stop when rotating on ground
		public float m_frictionDynamic = 1f;					// friction when accelerating
		public float m_frictionStatic = 1f;						// friction when releasing input / moving in opposite direction
		public bool m_autoRotate = true;						// enable auto rotate
		public float m_gravity = 50f;							// in m/s²
		public float m_airPower = 10f;							// acceleration when in air
		public float m_groundFlipMaxSpeed = -1f;				// maximum allowed speed on ground for flipping player
		public float m_airFlipMaxSpeed = -1f;					// maximum allowed speed in air for flipping player
		public float m_maxAirSpeed = 8f;						// max horizontal speed in air
		public float m_maxFallSpeed = 20f;						// max fall speed
        public bool m_enableCrouch = true;                      // enable crouching or not
        public float m_crouchSpeed = 3f;                        // moving crouch speed
        public float m_crouchSpeedBackward = 0f;                // speed when moving crouched backward (in m/s), same as m_crouchSpeed if zero
        public float m_crouchSizePercent = 0.5f;				// collision box reduce size percent when crouched
		public float m_uncrouchMinSpeed = 0f;					// used to get unstuck from a crouch
		public bool m_enableCrouchedRotate = true;				// enable rotate while crouched
		public eAutoMove m_autoMove = eAutoMove.Disabled;		// force player to always move in a direction
		public string m_haltAutoMove;							// input for pausing the auto move
		public float m_edgeSlideFactor = 0f;					// edge fall move factor (0=disabled, 1=max push)
		public int m_edgeSlideMinRayOut = 2;					// minimum number of ray to be out of ground for edge sliding

		// maxspeed factor in function of current ground slope angle (negative values for down slope, positive value for up)
		// NB : factor must be in [0,1] range
		public AnimationCurve m_slopeSpeedMultiplier = new AnimationCurve(new Keyframe(-90, 1), new Keyframe(0, 1), new Keyframe(45, 1), new Keyframe(70, 1), new Keyframe(70, 0), new Keyframe(90, 0));
	}

	[System.Serializable]
	public class GroundAlign
	{
		public bool m_groundAlign = false;				    // align player with ground normal
		public bool m_jumpAlign = false;				    // is jump direction aligned with ground normal or not
		public bool m_alignAirMove = false;				    // align air move direction with player direction
		public bool m_forceInAirVerticalAlign = true;	    // make sure player is always aligned with vertical axis while in air
		public float m_maxAngle = 80f;					    // maximum angle at which player can rotate (0=vertical, 90=horizontal, 180=upanddown)
        public float m_rotateSpeed = 360f;                  // rotation speed for ground align (deg/s)
		public LayerMask m_oneWayLayer = 0;				    // collision layer for "oneway" grounds
		public bool m_oneWayDownJump = false;			    // enable one way ground down jumping
        public APInputButton m_oneWayGroundDownJumpButton;  // used for down jumping while on OneWayGround only
	}

	[System.Serializable]
	public class DownSlopeSliding
	{
		public bool m_enabled = false;		// enable or not down slope sliding
		public float m_slidingPower = 1f;	// power to make player move downside
		public float m_slopeMinAngle = 20f;	// min slope angle at which sliding occurs (in degrees)
	}

	[System.Serializable]
	public class Advanced
	{
        public float m_groundSnapAngle = 60f;					// down slope max angle at which player can be snapped on ground => ground snapping is applied only within same game object
        public float m_groundSnapMaxHeight = 0.2f;				// max height for ground snapping to prevent vertical teleports => ground snapping is applied only within same game object
		public float m_maxVerticalVelForSnap = 0.01f;			// max vertical velocity for ground snapping if not previously on ground
		public float m_minTimeBetweenTwoJumps = 0.3f;			// minimum time between 2 consecutive jumps
		public float m_maxVelDamping = 1f;						// damping when max velocity is reached
		public float m_maxAirVelDamping = 1f;					// damping when max velocity is reached while in air
		public uint m_carryRayIndex = 0;						// used only when different carriers are detected, same ray is used each time
		public float m_groundAlignDeltaAngleMin = 1f;	        // delta angle in degrees from which ground align is updated (to prevent imprecisions issues)
		public float m_groundAlignDeltaAngleMax = 60f;	        // max delta angle in degrees at which ground align is not updated
        public float m_groundAlignUpdatePosThreshold = 0.2f;    // minimum move offset at which ground align can be updated ((to prevent imprecisions issues)
		public float m_groundAlignPenetrationThreshold = 0.5f;  // maximum penetration for a ray to be considered for ground normal computing
        public int m_groundAlignUpdateFrame = 2;                // means that ground align is update every count of frame specified by this value (to prevent imprecisions issues)
		public bool m_debugDraw = false;						// enable debug drawing
	}

	[System.Serializable]
	public class Animations
	{
		public APAnimation m_stand;
		public APAnimation m_run;
		public APAnimation m_runBackward;
		public APAnimation m_walk;
		public APAnimation m_walkBackward;
        public APAnimation m_walkCrouch;
        public APAnimation m_walkCrouchBackward;
        public APAnimation m_crouch;
		public APAnimation m_jump;
		public APAnimation m_inAir;
		public APAnimation m_wallJump;
		public APAnimation m_wallSlide;
		public APAnimation m_glide;
		public float m_minAirTime = 0f;				// min time in air before launching InAir animation
		public bool m_walkAnimFromInput = true;		// use input filtered value for animation, otherwise use ground speed
		public bool m_stopOnFrontWall = true;		// stop walking/running when touching front wall

		// animation speed in function of input filtered value, 0 means go to stand
		public AnimationCurve m_animFromInput = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
		// animation speed in function of ground speed
		public AnimationCurve m_animFromSpeed = new AnimationCurve(new Keyframe(0, 0), new Keyframe(8, 1));
	}

	[System.Serializable]
	public class SettingsJump
	{
		public bool m_enabled = true;					    // enabled status
		public APInputButton m_button;					    // disabled if empty
        public bool m_jumpWithVerticalAxis = false;         // allows to use vertical axis also in order to jump
		public float m_minHeight = 3f;					    // min jump height
		public float m_maxHeight = 3f;					    // max jump height if pushing jump input
		public int m_airJumpCount = 0;					    // number of additional jump you can make while in air
		public float m_airJumpHorizontalPower = 8f;		    // horizontal impulse applied on air jumps
	}

	[System.Serializable]
	public class SettingsWallJump
	{
		public bool m_enabled = false;
		public Vector2 m_jumpPower = new Vector2(10f, 8f);	// power of jump
		public float m_horizontalBooster = 5.0f;			// extra horizontal jump power 
		public float m_timeBeforeJump = 0.1f;				// time to be snapped on wall before jumping
		public float m_timeBeforeFlip = 0.1f;				// time before flipping after jump (negative to disable)
		public float m_disableAutoRotateTime = 0.3f;		// min time after wall jump to prevent autorotate
		public int[] m_rayIndexes;							// list of ray indexes to use for wall detection, empty means all rays
	}

	[System.Serializable]
	public class SettingsWallSlide
	{
		public bool m_enabled = false;
		public float m_friction = 8f;						// friction of the wall when sliding on it
		public float m_minSpeed = 0.1f;						// minimum down speed for enabling wall sliding
		public float m_minTime = 0f;						// minimum time to move along a wall before launching state
		public bool m_mustMoveForwardToStick = true;		// must move against a wall in order to slide on it
		public bool m_mustMoveBackwardToUnstick = false;	// must move in opposite direction in order to unstick from wall
		public float m_exitPushTime = 0.5f;					// amount of time opposite wall direction must be pushed for unsticking from wall slide
		public int[] m_rayIndexes;							// list of ray indexes to use for wall detection, empty means all rays
	}

	[System.Serializable]
	public class SettingsAttacks
	{
		public bool m_enabled = false;
		public APAttack[] m_attacks;					// list of attacks
		public bool m_enableAttackSwitchers = false;	
        public APAttackSwitcher[] m_switchers;			// list of attack switchers
	}

	[System.Serializable]
	public class SettingsGlide
	{
		public bool m_enabled = false;					// enabled status
		public APInputButton m_button;					// disabled if empty
		public float m_gravityFactor = 0.5f;			// factor to apply to gravity (in [0,1] range)
		public float m_lateralMoveFactor = 0.5f;		// factor to apply to lateral move (in [0,1] range)
		public float m_maxDuration = 3f;				// max duration a glide can occur
		public int m_maxCount = 1;						// number of glides you can do before touching back the ground
		public float m_minAirTimeBeforeGlide = 0.1f;	// minimum time while in air before glide is allowed
	}

	[System.Serializable]
	public class Shift
	{
		public APInputButton m_button;				// button for this action
		public float m_moveAngle = 0f;				// move angle (0 = horizontal, 90 = up, -90 = down)
		public float m_pauseTime = 0.5f;			// pause character for this duration before shifting
		public float m_moveLength = 2f;				// shift length (in meters)
		public float m_moveSpeed = 3f;				// shift speed (m/s)
		public float m_exitSpeed = 0f;				// character speed when leaving state
		public bool m_inAirOnly = true;				// allow shift only while in air
		public bool m_stopOnHit = true;				// should state exit on first hit
		public APAnimation m_anim;					// animation state to launch
	}

	[System.Serializable]
	public class SettingsShift
	{
		public bool m_enabled = false;				// enabled status
		public int m_maxCountInAir = 1;				// max consecutive shifts while in air
		public Shift[] m_shifts;					// list of shifts
	}

	[System.Serializable]
	public class SettingsEdgeGrab
	{
		public bool m_enabled = false;				    // enabled status
        public APUnityLayer m_layer;                    // layer used for detecting grab
        public float m_sensorRadius = 0.1f;             // radius of sensor for detecting edge grabs
        public float m_minTimeBetweenEdgeGrabs = 0.1f;	// minimum time between two consecutive grabs
	}

    public GameObject m_physicAnim;					// reference to physic animation node (needed for physic animation)
	public Inputs m_inputs;							// inputs handler (common for all interactive objects, we may add one by object if needed...)
	public Settings m_basic;						// basic settings
	public DownSlopeSliding m_downSlopeSliding;		// down slope sliding settings
	public GroundAlign m_groundAlign;				// align player rotation against contact normal
	public Animations m_animations;					// settings for animations
	public SettingsJump m_jump;						// jump settings
	public SettingsWallJump m_wallJump;				// wall jump settings
	public SettingsWallSlide m_wallSlide;			// wall slide settings
	public APLadder.Settings m_ladder;				// settings for ladders (can be overriden for each ladder)
	public APRailings.Settings m_railings;			// settings for railings (can be overriden for each railings)    
    public SettingsAttacks m_attacks; 	            // settings for attacks
	public SettingsGlide m_glide;					// settings for glide
	public SettingsShift m_shift;					// settings for shift
	public SettingsEdgeGrab m_edgeGrab;				// settings for edge grabbing

	public Advanced m_advanced;						// advanced low level settings
}