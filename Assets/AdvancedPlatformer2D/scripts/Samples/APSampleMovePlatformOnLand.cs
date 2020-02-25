/* Copyright (c) 2014 Advanced Platformer 2D */


using UnityEngine;
using System.Collections;

[AddComponentMenu("Advanced Platformer 2D/Samples/APSampleMovePlatformOnLand")]

// Sample moving platform when player lands on it
public class APSampleMovePlatformOnLand: APHitable
{
	////////////////////////////////////////////////////////
	// PUBLIC/HIGH LEVEL
	public float m_timeBeforeStartsMove = 2f;   			// timer before platform starts moving
	public string m_anim = "Move";							// move state

	////////////////////////////////////////////////////////
	// PRIVATE/LOW LEVEL
	bool m_moving;
	bool m_hasTouched;
	float m_touchTime;
    Animation m_animation;

	void Start () 
	{
		m_moving = false;
		m_hasTouched = false;
		m_touchTime = 0f;
        m_animation = GetComponent<Animation>();

        // make sure to enable carrier automatic mode if any
        APCarrier pCarrier = GetComponent<APCarrier>();
        if (pCarrier != null)
        {
            pCarrier.m_animationMode = true;
        }
	}

	void Update()
	{
		if(m_hasTouched && !m_moving)
		{
			m_touchTime += Time.deltaTime;
			if(m_touchTime >= m_timeBeforeStartsMove)
			{
				m_moving = true;
                m_animation.Play(m_anim);
			}
		}
	}

	// called when character motor ray is touching us
	override public bool OnCharacterTouch(APCharacterController character, APCharacterMotor.RayType rayType, RaycastHit2D hit, float penetration,
	                                      APMaterial hitMaterial)
	{
		if(m_moving || m_hasTouched || penetration > 0.1f)
			return true;

		// enable timer if player is touching us with feet only
		if(rayType == APCharacterMotor.RayType.Ground)
		{
			m_hasTouched = true;
			m_touchTime = 0f;
		}

		// always keep contact
		return true;
	}
}
