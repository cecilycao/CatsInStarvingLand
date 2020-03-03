/* Copyright (c) 2014 Advanced Platformer 2D */


using UnityEngine;
using System.Collections;

[AddComponentMenu("Advanced Platformer 2D/Samples/APSampleCrate")]

// Sample for specific explodable Crate behavior
// first hit = damage
// second hit = explode
public class APSampleCrate : APHitable 
{
	////////////////////////////////////////////////////////
	// PRIVATE/LOW LEVEL
	int m_hitCount;
	bool m_shouldDisable;
	Animator m_anim;
	bool m_shiftHit;

	void Start () 
	{
		m_hitCount = 2;
		m_anim = GetComponent<Animator>();
		m_shouldDisable = false;
		m_shiftHit = false;
	}

	void FixedUpdate()
	{
		// handle deferred hit
		if(m_shiftHit)
		{
			HandleHit();
			m_shiftHit = false;
		}
	}
	
	void LateUpdate()
	{
		if(m_shouldDisable)
		{
			gameObject.SetActive(false);
		}
	}

	// return true if object is dead
	bool IsDead() 
	{
		return m_hitCount <= 0;
	}
	
	// called when we have been hit by a melee attack
	override public bool OnMeleeAttackHit(APCharacterController character, APHitZone hitZone)
	{
		return HandleHit();
	}

	// called when we have been hit by a bullet
	override public bool OnBulletHit(APCharacterController launcher, APBullet bullet) 
	{
		return HandleHit();
	}

	// called when character is touching us with a ray
	override public bool OnCharacterTouch(APCharacterController launcher, APCharacterMotor.RayType rayType, RaycastHit2D hit, 
	                                     float penetration, APMaterial hitMaterial) 
	{
		// ignore contacts for exploded crates
		if(IsDead())
			return false;

		// check if we are touching with vertical down shift attack 
		if(!m_shiftHit && launcher.IsShifting() && launcher.GetMotor().m_velocity.y < 0f)
		{
			// handle different penetration in function of ray type
			if((rayType == APCharacterMotor.RayType.Ground && penetration < 0.1f) || (rayType != APCharacterMotor.RayType.Ground && penetration < 0f))
			{
				// defer hit (as this callback may be called for many rays)
				m_shiftHit = true;
			}
		}

		// ignore all contacts after a shift hit
		if(m_shiftHit)
		  return false;

		// always allow contact with crate
		return true; 
	}

	// call to handle one hit
	bool HandleHit()
	{
		// do nothing if already dead
		if (IsDead())
			return false;

		// reduce hit count
		m_hitCount--;
		
		// handle death & callbacks
		if (m_hitCount <= 0)
		{
			m_hitCount = 0;
			
			// launch die animation
			if(m_anim)
			{
				m_anim.Play("explode", 0, 0f);
			}
		}
		else
		{
			// launch hit animation
			if(m_anim)
			{
				m_anim.Play("hit", 0, 0f);
			}
		}

		return true;
	}
	
	// Disable object at next frame
	void Disable()
	{
		m_shouldDisable = true;
	}
}
