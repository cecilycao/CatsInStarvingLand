/* Copyright (c) 2014 Advanced Platformer 2D */

using UnityEngine;

[AddComponentMenu("Advanced Platformer 2D/Inputs/PlayMakerButton")]

[System.Serializable]
public class APPlayMakerButton : APInputButtonPlugin
{
	bool m_pushed;
	uint m_pushFrameCount;
	bool m_down;
	bool m_up;

	public bool pushed {
		get {
			return m_pushed;
		}
		set {
			m_pushed = value;
		}
	}

	void Awake()
	{
		m_pushFrameCount = 0;
		m_pushed = false;
		m_down = false;
		m_up = false;
	}

	void Update()
	{
		m_up = false;
		m_down = false;

		if(m_pushed)
		{
			if(m_pushFrameCount == 0)
			{
				m_down = true;
			}

			m_pushFrameCount++;

			// clear values each frame
			m_pushed = false;
		}
		else if(m_pushFrameCount != 0)
		{
			m_pushFrameCount = 0;
			m_up = true;
		}
	}

	override public bool GetButton()
	{
		return m_pushed || m_down;
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

