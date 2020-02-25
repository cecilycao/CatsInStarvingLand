/* Copyright (c) 2014 Advanced Platformer 2D */

using UnityEngine;
using System.Collections;

[AddComponentMenu("Advanced Platformer 2D/Carrier")]

public class APCarrier : MonoBehaviour
{ 
	////////////////////////////////////////////////////////
	// PUBLIC/HIGH LEVEL
	public bool m_animationMode = true;

	// Compute velocity at point in world space
    public Vector2 ComputeVelocityAt(Vector2 worldPoint)
    {
        // put point in local space
		Vector3 localPoint = (Vector3)worldPoint - transform.position;
        return m_linearVel + (Vector2)Vector3.Cross(m_angularVel, localPoint);
    }


	////////////////////////////////////////////////////////
	// PRIVATE/LOW LEVEL
	void Start ()
	{
		m_prevPos = transform.position;
		m_prevRot = transform.rotation;
		m_linearVel = Vector2.zero;
        m_angularVel = Vector3.zero;
		m_bNextUpdate = false;
		m_bUpdate = false;
	}

	void FixedUpdate ()
	{
        if (!m_animationMode)
        {
            UpdateVelocities();
        } 
        else
        {
			if(m_bNextUpdate)
			{
				m_bNextUpdate = false;
				transform.position = m_nextPos;
				transform.rotation = m_nextRot;
			}

            UpdateVelocities();

            // new position will be updated after this call
			m_bUpdate = true;
        }
	}

  	void Update ()
   	{
		if (m_animationMode && m_bUpdate)
        {
            // differ to next physic frame
			m_bUpdate = false;
			m_bNextUpdate = true;
			m_nextPos = transform.position;
			m_nextRot = transform.rotation;
            transform.position = m_prevPos;
            transform.rotation = m_prevRot;
        }
	}

	void UpdateVelocities()
	{
		// Linear velocity
		Vector2 fDiff = (Vector2)transform.position - m_prevPos;
		if (fDiff.sqrMagnitude > 1e-4f)
		{
			m_linearVel = (fDiff) / (Time.fixedDeltaTime);			
		}

		// Angular velocity
		float fAngle = Quaternion.Angle(m_prevRot, transform.rotation);
		if (fAngle > 0.1f)
		{
			Vector3 forwardA = m_prevRot * Vector3.up;
			Vector3 forwardB = transform.rotation * Vector3.up;
			if (Vector3.Cross(forwardA, forwardB).z < 0f)
			{
				fAngle = -fAngle;
			}

			m_angularVel.z = Mathf.Deg2Rad * fAngle / Time.fixedDeltaTime;			
		}

		m_prevRot = transform.rotation;
		m_prevPos = transform.position;
	}


	bool m_bNextUpdate;
	bool m_bUpdate;
	Vector2 m_prevPos;
	Quaternion m_prevRot;	

	Vector2 m_nextPos;
	Quaternion m_nextRot;

	Vector2 m_linearVel;
    Vector3 m_angularVel;
}

