/* Copyright (c) 2014 Advanced Platformer 2D */


using UnityEngine;
using System.Collections;

[AddComponentMenu("Advanced Platformer 2D/Samples/APSampleBlob")]

// Sample for specific NPC Blob behavior
public class APSampleBlob : APHitable 
{
	////////////////////////////////////////////////////////
	// PUBLIC/HIGH LEVEL
	public int m_life = 2;   								// amount of life point for this npc
	public int m_touchDamage = 1;							// damage done when touching player
	public float m_jumpRatioInputReleased = 0.5f;			// jump ratio power when player jumping on npc while jump input is released
	public float m_jumpRatioInputPushed = 1f;				// jump ratio power when player jumping on npc while jump input is pushed

	public bool m_moveHorizontal = true;					// is moving left/right, otherwise down/up
	public float m_moveMinOffset = 5f;						// patrol min position
	public float m_moveMaxOffset = 5f;						// patrol end position
	public float m_moveSpeed = 5f;							// move speed
	public bool m_faceRightOrUp = true;						// initially moving right/up or not


	////////////////////////////////////////////////////////
	// PRIVATE/LOW LEVEL
	float m_minTimeBetweenTwoReceivedHits = 0.1f;
	float m_jumpHitPenetrationTolerance = 0.4f;
	float m_hitPenetrationTolerance = -0.1f;

	float m_startPosX;
	float m_startPosY;
	bool m_pause;
	bool m_shouldDisable;
	Animator m_anim;
	float m_lastHitTime;


	void Start () 
	{
		// init start position
		m_lastHitTime = 0f;
		m_startPosX = transform.position.x;
		m_startPosY = transform.position.y;
		m_anim = GetComponent<Animator>();
		m_pause = false;
		m_shouldDisable = false;
	}
	
	void FixedUpdate () 
	{
		// update position
		if(!m_pause)
		{
			if(m_moveHorizontal)
			{
				Vector2 curPos = transform.position;
				curPos.x += (m_faceRightOrUp ? m_moveSpeed : -m_moveSpeed) * Time.deltaTime;
				if(curPos.x >= m_startPosX + m_moveMaxOffset)
				{
					curPos.x = m_startPosX + m_moveMaxOffset;
					Flip();
				}
				else if(curPos.x <= m_startPosX - m_moveMinOffset)
				{
					curPos.x = m_startPosX - m_moveMinOffset;
					Flip();
				}		
				transform.position = curPos;
			}
			else
			{
				Vector2 curPos = transform.position;
				curPos.y += (m_faceRightOrUp ? m_moveSpeed : -m_moveSpeed) * Time.deltaTime;
				if(curPos.y >= m_startPosY + m_moveMaxOffset)
				{
					curPos.y = m_startPosY + m_moveMaxOffset;
					m_faceRightOrUp = !m_faceRightOrUp;
				}
				else if(curPos.y <= m_startPosY - m_moveMinOffset)
				{
					curPos.y = m_startPosY - m_moveMinOffset;
					m_faceRightOrUp = !m_faceRightOrUp;
				}		
				transform.position = curPos;
			}
		}
	}

	void LateUpdate()
	{
		if(m_shouldDisable)
		{
			gameObject.SetActive(false);
		}
	}

	// Flip horizontally
	void Flip()
	{
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
		m_faceRightOrUp = !m_faceRightOrUp;
	}

	// Pause/unpause movement
	void Pause()
	{
		m_pause = true;
	}

	void UnPause()
	{
		m_pause = false;
	}

	// return true if object is dead
	public bool IsDead() 
	{
		return m_life <= 0f;
	}

	// called when we have been hit by a melee attack
	override public bool OnMeleeAttackHit(APCharacterController character, APHitZone hitZone)
	{
		// Do nothing if player is currently blinking
		APSamplePlayer samplePlayer = character.GetComponent<APSamplePlayer>();
        if (samplePlayer && samplePlayer.IsGodModeEnabled())
			return false;

		return AddHitDamage(hitZone.m_damage);
	}

	// called when we have been hit by a bullet
	override public bool OnBulletHit(APCharacterController launcher, APBullet bullet) 
	{
		return AddHitDamage(bullet.m_hitDamage);
	}
	
	// called when character motor ray is touching us
	override public bool OnCharacterTouch(APCharacterController character, APCharacterMotor.RayType rayType, RaycastHit2D hit, float penetration,
	                                      APMaterial hitMaterial)
	{
		// Do nothing if dead
		if(IsDead())
			return false;

		// Do nothing in godmode
		APSamplePlayer samplePlayer = character.GetComponent<APSamplePlayer>();
        if (samplePlayer && samplePlayer.IsGodModeEnabled())
			return false;

		// check if jumping on us
		bool bHit = false;
		if((rayType == APCharacterMotor.RayType.Ground) && (penetration <= m_jumpHitPenetrationTolerance))
		{
			// make character jump
			float fRatio = m_jumpRatioInputReleased;
			if(character.m_jump.m_button.IsSpecified() && character.m_jump.m_button.GetButton())
			{
				fRatio = m_jumpRatioInputPushed;
			}
			
			if(fRatio > 0f)
			{
				character.Jump(character.m_jump.m_minHeight * fRatio, character.m_jump.m_maxHeight * fRatio);
			}

			// add hit to NPC
            if (samplePlayer)
            {
                AddHitDamage(samplePlayer.m_jumpDamage);
            }

			bHit = true;
		}
		else if(penetration <= m_hitPenetrationTolerance)
		{
			// add hit to character
            if (samplePlayer)
            {
                samplePlayer.OnHit(m_touchDamage, transform.position);
            }

			bHit = true;
		}

		// prevent checking hits too often
		if(bHit)
		{
			if(Time.time < m_lastHitTime + m_minTimeBetweenTwoReceivedHits)
				return false;
			
			// save current hit time
			m_lastHitTime = Time.time;
		}
		
		// always ignore contact
		return false;
	}
	
	// Add hit damage
	bool AddHitDamage(int hitAmount)
	{
		// do nothing if already dead, no damage or paused
		if (IsDead() || (hitAmount <= 0f) || m_pause)
			return false;
		
		// reduce life point
		m_life -= hitAmount;
		
		// handle death & callbacks
		if (m_life <= 0f)
		{
			m_life = 0;

			// launch die animation
			if(m_anim)
			{
				m_anim.Play("die", 0, 0f);
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
