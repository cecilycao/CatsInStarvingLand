/* Copyright (c) 2014 Advanced Platformer 2D */


using UnityEngine;
using System.Collections;

[AddComponentMenu("Advanced Platformer 2D/Samples/APSampleFallingPlatform")]

// Sample falling platform when player lands on it
public class APSampleFallingPlatform : APHitable
{
	////////////////////////////////////////////////////////
	// PUBLIC/HIGH LEVEL
	public float m_timeBeforeFall = 2f;   					// timer before platform starts falling
	public float m_fallingPower = 10f;						// power at which platform is falling
	public float m_maxFallLength = 50f;						// maximum distance to move when falling, then stop


	////////////////////////////////////////////////////////
	// PRIVATE/LOW LEVEL
	float m_startPosY;
	float m_velY;
	bool m_hasBeenTouched;
	bool m_isFalling;
	float m_touchedTime;
	bool m_pause;

	void Start () 
	{
		// init start position
		m_startPosY = transform.position.y;
		m_velY = 0f;
		m_hasBeenTouched = false;
		m_pause = false;
		m_isFalling = false;
		m_touchedTime = 0f;
	}
	
	void FixedUpdate () 
	{
		if(m_pause)
			return;

		// go to fall if needed
		if(!m_isFalling && m_hasBeenTouched && (Time.time >= m_touchedTime + m_timeBeforeFall))
		{
			m_isFalling = true;
		}

		// simulate fall
		if(m_isFalling)
		{
			m_velY += -m_fallingPower * Time.deltaTime;

			Vector2 curPos = transform.position;
			curPos.y += m_velY * Time.deltaTime;
			transform.position = curPos;

			// stop when max length reached
			if(Mathf.Abs(curPos.y - m_startPosY) >= m_maxFallLength)
			{
				m_pause = true;
			}
		}
	}

	// called when character motor ray is touching us
	override public bool OnCharacterTouch(APCharacterController character, APCharacterMotor.RayType rayType, RaycastHit2D hit, float penetration,
	                                      APMaterial hitMaterial)
	{
		if(m_pause || m_hasBeenTouched || penetration > 0.1f)
			return true;

		// enable timer if player is touching us with feet only
		if(!m_hasBeenTouched && (rayType == APCharacterMotor.RayType.Ground))
		{
			m_hasBeenTouched = true;
			m_touchedTime = Time.time;
		}

		// always keep contact
		return true;
	}
}
