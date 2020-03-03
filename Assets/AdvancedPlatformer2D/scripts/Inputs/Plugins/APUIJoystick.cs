/* Copyright (c) 2014 Advanced Platformer 2D */

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[AddComponentMenu("Advanced Platformer 2D/Inputs/UIJoystick")]

[System.Serializable]
public class APUIJoystick : APInputJoystickPlugin
{
	public APButton m_buttonLeft;
	public APButton m_buttonRight;
	public APButton m_buttonUp;
	public APButton m_buttonDown;

	public override float GetAxisX() 
	{
		if(m_buttonLeft == null || m_buttonRight == null)
			return 0f;

		return m_buttonLeft.IsPressedEx() ? -1f : m_buttonRight.IsPressedEx() ? 1f : 0f;
	}

	public override float GetAxisY()
	{
		if(m_buttonDown == null || m_buttonUp == null)
			return 0f;

		return m_buttonDown.IsPressedEx() ? -1f : m_buttonUp.IsPressedEx() ? 1f : 0f;
	}
}
