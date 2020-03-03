/* Copyright (c) 2014 Advanced Platformer 2D */

using UnityEngine;
using System.Collections;

[AddComponentMenu("Advanced Platformer 2D/Samples/APSampleDoor")]

// Sample to move a door when all listed npcs are dead
public class APSampleDoor : MonoBehaviour 
{
	////////////////////////////////////////////////////////
	// PUBLIC/HIGH LEVEL
	public APSampleBlob[] m_blobs;		// list of blobs to kill before open
    public string AnimationToActivate;
	////////////////////////////////////////////////////////
	// PRIVATE/LOW LEVEL
	bool m_isOpened;

	// Use this for initialization
	void Start () 
	{
		m_isOpened = false;
	} 
	
	// Update is called once per frame
	void Update () 
	{ 
		if(m_isOpened || m_blobs.Length == 0)
			return;

		// this is not really optimal to check blob status everyframe
		// we should add a callbacks system to be warned when each blob is dead
		// but this is just a sample :)
		int blobDeadCount = 0;
		foreach(APSampleBlob curBlob in m_blobs)
		{
			if(curBlob.IsDead())
			{
				blobDeadCount++;
			}
		}

		if(blobDeadCount == m_blobs.Length)
		{
			// open the door
			m_isOpened = true;

			if(!string.IsNullOrEmpty(AnimationToActivate))
			{
				GetComponent<Animator>().Play(AnimationToActivate);
			}
		}
	}
}
