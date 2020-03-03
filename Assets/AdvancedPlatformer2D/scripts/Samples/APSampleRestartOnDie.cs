/* Copyright (c) 2014 Advanced Platformer 2D */


using UnityEngine;
using System.Collections;

[RequireComponent(typeof(APSamplePlayer))]
[AddComponentMenu("Advanced Platformer 2D/Samples/APSampleRestartOnDie")]

// Sample for falling platform
public class APSampleRestartOnDie : MonoBehaviour
{
	////////////////////////////////////////////////////////
	// PUBLIC/HIGH LEVEL
	public float m_minHeight = -100f;   					// height at which player is considered dead

	////////////////////////////////////////////////////////
	// PRIVATE/LOW LEVEL
	APSamplePlayer m_player;

	void Start()
	{
		m_player = GetComponent<APSamplePlayer>();
	}

	void Update()
	{
		if(m_player && (transform.position.y <= m_minHeight))
		{
			m_player.Kill();
		}
	}
}
