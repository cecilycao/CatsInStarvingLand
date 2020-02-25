/* Copyright (c) 2014 Advanced Platformer 2D */

using UnityEngine;
using System.Collections;

[AddComponentMenu("Advanced Platformer 2D/Samples/APSampleAudio")]

// Sample GUI handler
public class APSampleAudio : APCharacterEventListener 
{
	////////////////////////////////////////////////////////
	// PUBLIC/HIGH LEVEL
	public AudioSource m_jump;

	////////////////////////////////////////////////////////
	// PRIVATE/LOW LEVEL
	APCharacterController m_character;

	void Awake()
	{
		// get character from whole scene
		m_character = GameObject.FindObjectOfType<APCharacterController>();
	}

	void Start () 
	{
		// register event listener to character
		if(m_character)
		{
			m_character.EventListeners.Add(this);
		}
	}

	public override void OnJump() 
	{
		// Play jump audio on jump event
		m_jump.Play();
	}

}
