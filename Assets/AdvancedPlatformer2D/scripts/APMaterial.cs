/* Copyright (c) 2014 Advanced Platformer 2D */

using UnityEngine;

[AddComponentMenu("Advanced Platformer 2D/Material")]
public class APMaterial : MonoBehaviour 
{
	////////////////////////////////////////////////////////
	// PUBLIC/HIGH LEVEL
	public enum BoolValue
	{
		Default = 0, 	// keep default default from character
		True, 			// override to true
		False 			// override to false
	}

	[System.Serializable]
	public class FloatValue
	{
		public bool m_override = false; 
		public float m_value = 0f;
	}

	// frictions
	public bool m_overrideFriction = false;		// use below friction settings if enabled
	public float m_dynFriction = 1f;			// dyn friction
	public float m_staticFriction = 1f;			// static friction

    // bounce
    public float m_groundBounciness = 0f;	    // bounce effect factor when touching ground

	// wall jump/slide
	public BoolValue m_wallJump = BoolValue.Default;	// wall jump override status
	public BoolValue m_wallSlide = BoolValue.Default;	// wall slide override status
	public FloatValue m_wallFriction;
}

