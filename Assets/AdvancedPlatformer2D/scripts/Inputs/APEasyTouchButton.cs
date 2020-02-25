/* Copyright (c) 2014 Advanced Platformer 2D */

using UnityEngine;

[RequireComponent(typeof(ETCButton))]
[AddComponentMenu("Advanced Platformer 2D/Inputs/EasyTouchButton")]

[System.Serializable]
public class APEasyTouchButton : APInputButtonPlugin
{
    ETCButton m_button;

	void Awake()
	{
        m_button = GetComponent<ETCButton>();
	}

	override public bool GetButton()
	{
        return (m_button.axis.axisState == ETCAxis.AxisState.Down || m_button.axis.axisState == ETCAxis.AxisState.Press);
	}
	
	override public bool GetButtonDown()
	{
        return m_button.axis.axisState == ETCAxis.AxisState.Down;
	}
	
	override public bool GetButtonUp()
	{
        return m_button.axis.axisState == ETCAxis.AxisState.Up;
	}
}

