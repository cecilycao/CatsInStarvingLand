/* Copyright (c) 2014 Advanced Platformer 2D */

using UnityEngine;

[AddComponentMenu("Advanced Platformer 2D/Inputs/PlayMakerJoystick")]

[System.Serializable]
public class APPlayMakerJoystick : APInputJoystickPlugin
{
	float m_axisX;
	float m_axisY;

	public float axisX {
		get {
			return m_axisX;
		}
		set {
			m_axisX = value;
		}
	}

	public float axisY {
		get {
			return m_axisY;
		}
		set {
			m_axisY = value;
		}
	}

	void Awake()
	{
		m_axisX = 0f;
		m_axisY = 0f;
	}

	void Update()
	{
		// clear values each frame
		m_axisX = 0f;
		m_axisY = 0f;
	}

	override public float GetAxisX()
	{
		return m_axisX;
	}

	override public float GetAxisY()
	{
		return m_axisY;
	}
}

