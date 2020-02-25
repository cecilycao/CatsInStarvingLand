/* Copyright (c) 2014 Advanced Platformer 2D */

using UnityEngine;

[System.Serializable]
public class APInputButton
{
	////////////////////////////////////////////////////////
	// PUBLIC/HIGH LEVEL	
	public string m_name;						// name of button in project input settings
	public APInputButtonPlugin m_plugin;		// use custom plugin instead of classic Unity input
    public string[] m_holders;					// additional buttons we must hold while testing button as pushed
    public string[] m_releasers;				// additional buttons we must release while testing button as pushed
    public APInputConstrainedAxis[] m_axis;		// additional axis constraints

	////////////////////////////////////////////////////////
	// PRIVATE/LOW LEVEL
	bool m_bDown = false;
	bool m_bUp = false;
	float m_timeDown = 0f;

	[System.NonSerialized]
	public float m_downPushDuration = 0.1f;		// time during which down button status can lasts even if player has released button

	public APInputButton() 
	{
		m_bDown = false;
		m_bUp = false;
		m_timeDown = 0f;
	}

	// check if button is setup or not
	public bool IsSpecified()
	{
		return !string.IsNullOrEmpty(m_name) || (m_plugin != null);
	}

	// check if all specified holders are pushed
	public bool Holders()
	{
		foreach(string sHolder in m_holders)
		{
			if(!string.IsNullOrEmpty(sHolder) && !Input.GetButton(sHolder))
			{
				return false;
			}
		}
		return true;
	}

	// check if all specified releasers are not pushed
	public bool Releasers()
	{
		foreach(string sReleaser in m_releasers)
		{
			if(!string.IsNullOrEmpty(sReleaser) && Input.GetButton(sReleaser))
			{
				return false;
			}
		}
		return true;
	}

    // check if all specified axis constraints are valid
    public bool AllAxisConstraintsValid()
    {
        foreach (APInputConstrainedAxis sConstraint in m_axis)
        {
            if (!sConstraint.IsConstraintValid())
            {
                return false;
            }
        }
        return true;
    }

	// check button current state
	public bool GetButton()
	{
        bool bRet = string.IsNullOrEmpty(m_name) ? false : Input.GetButton(m_name) && Holders() && Releasers() && AllAxisConstraintsValid();
		if(m_plugin != null)
		{
			bRet |= m_plugin.GetButton();
		}
		return bRet;
	}

	// check if button has been pressed down at last frame
	public bool GetButtonDown()
	{
		return m_bDown;
	}

	// check if button has been released at last frame
	public bool GetButtonUp()
	{
		return m_bUp;
	}

	public void Refresh(bool bSet, bool bAutoFire = false)
	{
		if(bSet)
		{
			if(InternalGetButtonDown())
			{
				m_bDown = true;
				m_timeDown = Time.time;
			}

			if(InternalGetButtonUp())
			{
				m_bUp = true;
			}
		}
		else
		{
			if((Time.time >= m_timeDown + m_downPushDuration) && (!bAutoFire || !GetButton()))
			{
				m_bDown = false;
			}
			
			m_bUp = false;
		}
	}

	bool InternalGetButtonDown()
	{
        bool bRet = string.IsNullOrEmpty(m_name) ? false : Input.GetButtonDown(m_name) && Holders() && Releasers() && AllAxisConstraintsValid();
        if (m_plugin != null)
        {
            bRet |= m_plugin.GetButtonDown();
        }
        return bRet;
        return false;
	}

	bool InternalGetButtonUp()
	{
		bool bRet = string.IsNullOrEmpty(m_name) ? false : Input.GetButtonUp(m_name);
		if(m_plugin != null)
		{
			bRet |= m_plugin.GetButtonUp();
		}
		return bRet;
	}
}

