/* Copyright (c) 2014 Advanced Platformer 2D */

using UnityEngine;

[System.Serializable]
public class APInputJoystick
{
	////////////////////////////////////////////////////////
	// PUBLIC/HIGH LEVEL	
	public string m_name = "axis";					// name of axis in project input settings
	public float m_acceleration = 10f;				// acceleration of axis
	public float m_deceleration = 10f;				// deceleration of axis
	public bool m_snap = true;						// snap value to 0 when changin axis direction
	public bool m_forceDigital = true;				// is input analog value forced to digital (only -1, 0 and 1 values)
	public float m_deadZone = 0.3f;					// dead zone value (analog values < to this value are set to zero)
	public APInputJoystickPlugin m_plugin;			// use custom plugin for raw value instead of classic Unity input

	[HideInInspector]
	public bool m_horizontal = true;				// is axis horizontal or not

	////////////////////////////////////////////////////////
	// PRIVATE/LOW LEVEL
	float m_value = 0f;
	float m_forcedValue = float.MinValue;

	public APInputJoystick(string sName, bool bHorizontal) 
	{
		m_name = sName; 
		m_horizontal = bHorizontal;
	}

    // Reset current value
    public void ResetValue(float fValue)
    {
        m_value = Mathf.Clamp(fValue, -1f, 1f);
    }

	public void Update(float dt)
	{
		m_value = Update(m_value, GetValue(), m_snap, m_acceleration, m_deceleration, dt);
		m_value = Mathf.Clamp(m_value, -1f, 1f);
	}

	static public float Update(float fCurValue, float fNewVal, bool bSnap, float fAccel, float fDecel, float dt)
	{
		float fSignNew = Mathf.Sign(fNewVal);
		float fSignCur = Mathf.Sign(fCurValue);
		
		// handle snap
		if(bSnap && (fNewVal != 0f) && (fSignNew != fSignCur))
		{
			fCurValue = 0f;
		}

		// handle deceleration first
		float fDiffValue = fNewVal - fCurValue;
		if(fCurValue >= 0f)
		{
			if(fDiffValue >= 0f)
			{
				// accel
				fDiffValue = Mathf.Min(fDiffValue, fAccel * dt);
			}
			else
			{
				if(fDiffValue < -fCurValue)
				{
					// we are changing sign in one frame, handle this special case
					if(fCurValue > fDecel * dt)
					{
						fDiffValue = -fDecel * dt;

					}
					else
					{
						fDiffValue = Mathf.Max(fDiffValue + fCurValue, -fAccel * dt) - fCurValue;
					}
				}
				else
				{
					// decel
					fDiffValue = Mathf.Max(fDiffValue, -fDecel * dt);
				}
			}
		}
		else
		{
			if(fDiffValue <= 0f)
			{
				// accel
				fDiffValue = Mathf.Max(fDiffValue, -fAccel * dt);
			}
			else
			{
				if(fDiffValue > -fCurValue)
				{
					// we are changing sign in one frame, handle this special case
					if(fCurValue < -fDecel * dt)
					{
						fDiffValue = fDecel * dt;

					}
					else
					{
						fDiffValue = Mathf.Min(fDiffValue + fCurValue, fAccel * dt) - fCurValue;
					}
				}
				else
				{
					// decel
					fDiffValue = Mathf.Min(fDiffValue, fDecel * dt);
				}
			}
		}

		// final update
		return fCurValue + fDiffValue;
	}
	
	public float GetSmoothValue()
	{
		return m_value;
	}

	public float GetValue()
	{
		float fValue = 0f;
		if(m_forcedValue == float.MinValue)
		{
			// get from Unity engine
			if(!string.IsNullOrEmpty(m_name))
				fValue = Input.GetAxisRaw(m_name);

			// get from plugin if any, keep highest absolute value
			if(m_plugin != null)
			{
				float fPluginValue = m_horizontal ? m_plugin.GetAxisX() : m_plugin.GetAxisY();
				if(Mathf.Abs(fPluginValue) > Mathf.Abs(fValue))
					fValue = fPluginValue;
			}

			// handle dead zone
			if(Mathf.Abs(fValue) <= m_deadZone)
			{
				fValue = 0f;
			}

			// handle non analog filtering
			if(m_forceDigital && fValue != 0f)
			{
				fValue = Mathf.Sign(fValue);
			}
		}
		else
		{
			fValue = m_forcedValue;
		}
		return fValue;
	}

    // Force a static value all the time
	public void SetForcedValue(bool enabled, float value)
	{
		if(enabled)
		{
			m_forcedValue = value;
		}
		else
		{
			m_forcedValue = float.MinValue;
		}
	}

	public bool isHorizontal {
		get {
			return m_horizontal;
		}
	}
}

