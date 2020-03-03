/* Copyright (c) 2014 Advanced Platformer 2D */
using UnityEngine;

[System.Serializable]
public class APAnimation
{   
    public string m_name = null;
	public int m_layer = 0;

	public int GetAnimHash()
	{
		if((m_animHash == 0) && !string.IsNullOrEmpty(m_name))
		{
			m_animHash = Animator.StringToHash(m_name);
		}
		return m_animHash;
	}

	public bool IsValid()
	{
		return GetAnimHash() != 0;
	}

	private int m_animHash = 0;
}
