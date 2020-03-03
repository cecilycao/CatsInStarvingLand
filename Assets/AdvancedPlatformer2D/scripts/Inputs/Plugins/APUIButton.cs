/* Copyright (c) 2014 Advanced Platformer 2D */

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[AddComponentMenu("Advanced Platformer 2D/Inputs/UIButton")]

[System.Serializable]
public class APUIButton : APInputButtonPlugin
{
	public APButton m_button;

	bool m_wasPressed;
	bool m_down;
	bool m_up;

	public bool IsPushed()
	{
		return m_button != null && m_button.IsPressedEx();
	}

	void Awake()
	{
		m_wasPressed = false;
		m_down = false;
		m_up = false;
	}

	void Update()
	{
		bool bPushed = IsPushed();
		if(bPushed && !m_wasPressed)
		{
			m_down = true;
		}
		else if(!bPushed && m_wasPressed)
		{
			m_up = true;
		}

		m_wasPressed = bPushed;
	}

	void LateUpdate()
	{
		m_down = false;
		m_up = false;
	}

	override public bool GetButton()
	{
		return IsPushed() || m_down;
	}
	
	override public bool GetButtonDown()
	{
		return m_down;
	}
	
	override public bool GetButtonUp()
	{
		return m_up;
	}
}
