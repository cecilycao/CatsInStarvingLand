/* Copyright (c) 2014 Advanced Platformer 2D */


using UnityEngine;
using System.Collections;

[AddComponentMenu("Advanced Platformer 2D/Samples/APSampleSpikes")]

// Sample for Spikes object
public class APSampleSpikes : MonoBehaviour
{
	////////////////////////////////////////////////////////
	// PUBLIC/HIGH LEVEL
	public int m_touchDamage = 1;						// damage done when touching player

	////////////////////////////////////////////////////////
	// PRIVATE/LOW LEVEL
	float m_minTimeBetweenTwoReceivedHits = 0.1f;
	float m_lastHitTime;

    // public AudioClip[] otherClip; //make an arrayed variable (so you can attach more than one sound)
    public AudioClip[] otherClip; //make an arrayed variable (so you can attach more than one sound)


    void Start () 
	{
		m_lastHitTime = 0f;
	}

	// called when character is entering this collectable
	public void OnTriggerEnter2D(Collider2D otherCollider)
	{
		APSamplePlayer character = otherCollider.GetComponent<APSamplePlayer>();
		if(character && !character.IsGodModeEnabled())
		{
			// prevent checking hits too often
			if(Time.time < m_lastHitTime + m_minTimeBetweenTwoReceivedHits)
				return;
			
			// save current hit time
			m_lastHitTime = Time.time;
			
			// add hit to character
			character.OnHit(m_touchDamage, transform.position);
            PlaySound(0);


        }
	}

    // called when character is inside this collectable , after godMode
    public void OnTriggerStay2D(Collider2D otherCollider)
    {
        APSamplePlayer character = otherCollider.GetComponent<APSamplePlayer>();
        if (character && !character.IsGodModeEnabled())
        {
            // prevent checking hits too often
            if (Time.time < m_lastHitTime + m_minTimeBetweenTwoReceivedHits)
                return;

            // save current hit time
            m_lastHitTime = Time.time;

            // add hit to character
            character.OnHit(m_touchDamage, transform.position);
        }
    }

    // Play sound from variable
    public void PlaySound(int i)
    {
        //Assign random sound from variable
        //  GetComponent<AudioSource>().clip = otherClip[Random.Range(0, otherClip.length)];

        if (otherClip.Length != 0)

        {
            GetComponent<AudioSource>().clip = otherClip[i];
            GetComponent<AudioSource>().Play();
        }
    }
}
