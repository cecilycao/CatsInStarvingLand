/* Copyright (c) 2014 Advanced Platformer 2D */


using UnityEngine;
using System.Collections;

[AddComponentMenu("Advanced Platformer 2D/Samples/APSampleLoadLevel")]

// Sample collectibles
public class APSampleLoadLevel : MonoBehaviour 
{
	////////////////////////////////////////////////////////
	// PUBLIC/HIGH LEVEL
	public string m_levelName;   						// level name to load
	public float m_timeToLoad = 0f;						// defer loading time

	////////////////////////////////////////////////////////
	// PRIVATE/LOW LEVEL
	bool m_launched;

	void Start()
	{
		m_launched = false;
	}

	// called when character is entering this collectable
	public void OnTriggerEnter2D(Collider2D otherCollider)
	{
		APSamplePlayer player = otherCollider.GetComponent<APSamplePlayer>();
		if(!m_launched && player && player.GetGUI() && !string.IsNullOrEmpty(m_levelName))
		{
			m_launched = true;
			player.GetGUI().LoadLevel(m_levelName, m_timeToLoad);
		}
	}
}