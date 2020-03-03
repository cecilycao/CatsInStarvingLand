/* Copyright (c) 2014 Advanced Platformer 2D */

using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu("Advanced Platformer 2D/HitZone")]
public class APHitZone : MonoBehaviour 
{
	////////////////////////////////////////////////////////
	// PUBLIC/HIGH LEVEL
	public float m_radius = 0.1f; 					// radius of hit zone
	public int m_damage = 1;						// hit damage points
	public bool m_active = false;					// should be false at init and updated in animation

	////////////////////////////////////////////////////////
	// PRIVATE/HIGH LEVEL
	List<APHitable> m_attackHits = new List<APHitable>(4);	// buffer used for melee attack
	public List<APHitable> attackHits
	{
		get
		{
			return m_attackHits;
		}
		set
		{
			m_attackHits = value;
		}
	}
}
