/* Copyright (c) 2014 Advanced Platformer 2D */


using UnityEngine;
using System.Collections;

[AddComponentMenu("Advanced Platformer 2D/Samples/APSampleMovingObject")]

// Sample script for moving object
public class APSampleMovingObject : MonoBehaviour 
{
	////////////////////////////////////////////////////////
	// PUBLIC/HIGH LEVEL
	public bool m_moveHorizontal = true;				// is moving left/right, otherwise down/up
	public float m_minOffset = 5f;						// move min offset
	public float m_maxOffset = 5f;						// move max offset
	public float m_speedMax = 5f;						// max move speed (m/s)M
	public float m_acceleration = 50f;					// move acceleration (m/s²)
	public bool m_moveRightOrUp = true;					// moving initially right/up or not
	public bool m_flip = false;							// flip sprite horizontally when reaching a limit


	////////////////////////////////////////////////////////
	// PRIVATE/LOW LEVEL
	float m_startPosX;
	float m_startPosY;
	float m_curSpeed;
    Rigidbody2D m_rigidBody;


	void Start () 
	{
		// init start position
		m_startPosX = transform.position.x;
		m_startPosY = transform.position.y;
		m_curSpeed = 0f;
        m_rigidBody = GetComponent<Rigidbody2D>();

        // make sure to disable carrier automatic mode if any
        APCarrier pCarrier = GetComponent<APCarrier>();
        if (pCarrier != null)
        {
            pCarrier.m_animationMode = false;
        }
	}
	
	void FixedUpdate () 
	{
		// update speed
		m_curSpeed += (m_moveRightOrUp ? m_acceleration : -m_acceleration) * Time.deltaTime;
		m_curSpeed = Mathf.Clamp(m_curSpeed, -m_speedMax, m_speedMax);

		// update position
		Vector2 curPos = transform.position;
		float fMoveDelta = m_curSpeed * Time.deltaTime;
		if(m_moveHorizontal)
		{
			if(curPos.x + fMoveDelta >= m_startPosX + m_maxOffset)
			{
				fMoveDelta = curPos.x - (m_startPosX + m_maxOffset);
				m_curSpeed = 0f;
				m_moveRightOrUp = !m_moveRightOrUp;
				Flip();
			}
			else if(curPos.x + fMoveDelta <= m_startPosX - m_minOffset)
			{
				fMoveDelta = curPos.x - (m_startPosX - m_minOffset);
				m_curSpeed = 0f;
				m_moveRightOrUp = !m_moveRightOrUp;
				Flip();
			}
		}
		else
		{
			if(curPos.y + fMoveDelta >= m_startPosY + m_maxOffset)
			{
				fMoveDelta = curPos.y - (m_startPosY + m_maxOffset);
				m_curSpeed = 0f;
				m_moveRightOrUp = !m_moveRightOrUp;
			}
			else if(curPos.y + fMoveDelta <= m_startPosY - m_minOffset)
			{
				fMoveDelta = curPos.y - (m_startPosY - m_minOffset);
				m_curSpeed = 0f;
				m_moveRightOrUp = !m_moveRightOrUp;
			}		
		}

		// handle physic or position update
        if (m_rigidBody != null)
		{
			float fSpeed = fMoveDelta / Time.deltaTime;
			Vector2 newVel = m_moveHorizontal ? new Vector2(fSpeed, 0f) : new Vector2(0f, fSpeed);
            m_rigidBody.velocity = newVel;
		}
		else
		{
			if(m_moveHorizontal)
			{
				curPos.x += fMoveDelta;
			}
			else
			{
				curPos.y += fMoveDelta;
			}
			transform.position = curPos;
		}
	}

	// Flip horizontally
	void Flip()
	{
		if(m_flip)
		{
			Vector3 theScale = transform.localScale;
			theScale.x *= -1;
			transform.localScale = theScale;
		}
	}
}
