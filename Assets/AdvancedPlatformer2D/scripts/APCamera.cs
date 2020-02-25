/* Copyright (c) 2014 Advanced Platformer 2D */

using UnityEngine;
using System.Collections;

[AddComponentMenu("Advanced Platformer 2D/Camera")]

public class APCamera : MonoBehaviour 
{
    public enum FaceLeadMode
    {
        Input,
        CharacterSpeed
    };

	////////////////////////////////////////////////////////
	// PUBLIC/HIGH LEVEL
	public float m_marginX = 1f;				                 // Max distance between camera and player on X axis
	public float m_marginY = 2f;				                 // Max distance between camera and player on Y axis
	public Vector2 m_offset = Vector2.zero;		                 // static offset
	public float m_faceLead = 0f;				                 // face leading distance
	public float m_faceLeadPower = 5f;			                 // face leading power
    public FaceLeadMode m_faceLeadMode = FaceLeadMode.Input;    // Mode of the face leading
    public bool m_NoBackward = false;                            // prevent camera from going backward

	// speed curve in fonction of distance
	public AnimationCurve m_speed = new AnimationCurve(new Keyframe(0, 15), new Keyframe(15, 50));

	public bool m_clampEnabled = false;			    // Enable position clamping
	public Vector2 m_minPos = Vector2.zero;		    // Camera min position
	public Vector2 m_maxPos = Vector2.zero;		    // Camera max position
    public float m_clampUpdateSpeed = 100f;		    // Max speed at which clamp values can be changed

	public Transform player;								// Reference to the player's transform.
	public APParallaxScrolling[] m_parallaxScrollings;		// list of object to handle for parallax scrolling

	////////////////////////////////////////////////////////
	// PRIVATE/LOW LEVEL
	APCharacterController m_character;
	float m_dynOffset;
    Vector2 m_curMinPos;
    Vector2 m_curMaxPos;

	void Awake ()
	{
        m_curMinPos = m_minPos;
        m_curMaxPos = m_maxPos;
		m_dynOffset = 0f;
		m_character = player.GetComponent<APCharacterController>();
		for(int i = 0; i < m_parallaxScrollings.Length; i++)
		{
			m_parallaxScrollings[i].Init(this);
		}
	}

	void FixedUpdate()
	{
		if(APSettings.m_fixedUpdate)
		{
			UpdateCamera();
		}
	}

	void LateUpdate ()
	{
		if(!APSettings.m_fixedUpdate)
		{
			UpdateCamera();
		}
	}

	void UpdateCamera ()
	{
		float camX = transform.position.x;
		float camY = transform.position.y;
		float fOffsetX = 0f;
		float fOffsetY = 0f;
		Vector2 v2Diff = player.position - transform.position;
		v2Diff += m_offset;

		// Handle face leading here
		if(m_character)
		{
            float fMinSpeed = 0.1f;
			if((m_faceLeadMode == FaceLeadMode.Input && m_character.m_inputs.m_axisX.GetValue() > 0f) ||
               (m_faceLeadMode == FaceLeadMode.CharacterSpeed && m_character.GetMotor().m_velocity.x > fMinSpeed))
			{
				m_dynOffset += Time.deltaTime *  m_faceLeadPower;
				m_dynOffset = Mathf.Min(m_faceLead, m_dynOffset);
			}
            else if (!m_NoBackward && ((m_faceLeadMode == FaceLeadMode.Input && m_character.m_inputs.m_axisX.GetValue() < 0f) ||
                    (m_faceLeadMode == FaceLeadMode.CharacterSpeed && m_character.GetMotor().m_velocity.x < -fMinSpeed)))
			{   
				m_dynOffset -= Time.deltaTime *  m_faceLeadPower;
				m_dynOffset = Mathf.Max(-m_faceLead, m_dynOffset);
			}

			v2Diff.x += m_dynOffset;
		}

		/*
		Vector3 camPosOffset = transform.position - new Vector3(m_offset.x + m_dynOffset, m_offset.y, 0f);
		Debug.DrawLine(camPosOffset + new Vector3(-m_marginX, -m_marginY, 0f), 
		               camPosOffset + new Vector3(-m_marginX, m_marginY, 0f));

		Debug.DrawLine(camPosOffset + new Vector3(-m_marginX, m_marginY, 0f), 
		               camPosOffset + new Vector3(m_marginX, m_marginY, 0f));

		Debug.DrawLine(camPosOffset + new Vector3(m_marginX, m_marginY, 0f), 
		               camPosOffset + new Vector3(m_marginX, -m_marginY, 0f));

		Debug.DrawLine(camPosOffset + new Vector3(m_marginX, -m_marginY, 0f), 
		               camPosOffset + new Vector3(-m_marginX, -m_marginY, 0f));
		*/

		// handle x margin
		if(v2Diff.x > m_marginX)
		{
			fOffsetX = (v2Diff.x - m_marginX);
		}
		else if(v2Diff.x < -m_marginX)
		{
			fOffsetX = (v2Diff.x + m_marginX);
		}


		// handle y margin
		if(v2Diff.y > m_marginY)
		{
			fOffsetY = (v2Diff.y - m_marginY);
		}
		else if(v2Diff.y < -m_marginY)
		{
			fOffsetY = (v2Diff.y + m_marginY);
		}

        // Prevent backward move if requested
        if (m_NoBackward)
        {
            fOffsetX = Mathf.Max(0f, fOffsetX);
        }

		// update dynamic
		float fAbsOffsetX = Mathf.Abs(fOffsetX);
		float fSpeedX = m_speed.Evaluate(fAbsOffsetX);
		camX = Move(camX, camX + fOffsetX, fSpeedX);

		float fAbsOffsetY = Mathf.Abs(fOffsetY);
		float fSpeedY = m_speed.Evaluate(fAbsOffsetY);
		camY = Move(camY, camY + fOffsetY, fSpeedY);


		// clamp if needed
		if(m_clampEnabled)
		{
            // smooth transition when min/max position has changed
            if (!m_curMinPos.Equals(m_minPos))
            {
                m_curMinPos.x = Move(m_curMinPos.x, m_minPos.x, m_clampUpdateSpeed);
                m_curMinPos.y = Move(m_curMinPos.y, m_minPos.y, m_clampUpdateSpeed);
            }

            if (!m_curMaxPos.Equals(m_maxPos))
            {
                m_curMaxPos.x = Move(m_curMaxPos.x, m_maxPos.x, m_clampUpdateSpeed);
                m_curMaxPos.y = Move(m_curMaxPos.y, m_maxPos.y, m_clampUpdateSpeed);
            }

            // Effective clamp
            camX = Mathf.Clamp(camX, m_curMinPos.x, m_maxPos.x);
            camY = Mathf.Clamp(camY, m_curMinPos.y, m_maxPos.y);
		}

		// Final update
		SetCameraPosition(camX, camY);
	}

	void SetCameraPosition(float fPosX, float fPosY)
	{
		transform.position = new Vector3(fPosX, fPosY, transform.position.z);
		
		// Update parallax scrollings
		for(int i = 0; i < m_parallaxScrollings.Length; i++)
		{
			m_parallaxScrollings[i].Update();
		}
	}

	float Move(float fFrom, float fTo, float fSpeed)
	{
		float fDiff = fTo - fFrom;
		float fOffset = fSpeed * Time.deltaTime;
		if(fOffset > Mathf.Abs(fDiff))
		{
			fOffset = fDiff;
		}
		else
		{
			fOffset *= Mathf.Sign(fDiff);
		}

		return fFrom + fOffset;
	}
}
