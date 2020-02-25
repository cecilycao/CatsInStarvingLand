/* Copyright (c) 2014 Advanced Platformer 2D */

using UnityEngine;

[RequireComponent(typeof(ETCJoystick))]
[AddComponentMenu("Advanced Platformer 2D/Inputs/EasyTouchJoystick")]

[System.Serializable]
public class APEasyTouchJoystick : APInputJoystickPlugin
{
	ETCJoystick m_joystick;		

	void Awake()
	{
        m_joystick = GetComponent<ETCJoystick>();
	}

	override public float GetAxisX()
	{
        return m_joystick.axisX.axisValue;
	}

	override public float GetAxisY()
	{
        return m_joystick.axisY.axisValue;
	}
}

