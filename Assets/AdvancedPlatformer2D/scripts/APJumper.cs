/* Copyright (c) 2014 Advanced Platformer 2D */

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
[AddComponentMenu("Advanced Platformer 2D/Jumper")]

public class APJumper : MonoBehaviour 
{
	////////////////////////////////////////////////////////
	// PUBLIC/HIGH LEVEL		
	public enum EImpulseMode
	{
		Impulse,
		Jump
	}

	public EImpulseMode m_mode = EImpulseMode.Impulse;		// mode of impulse
	public float m_impulsePower = 20f; 						// power of impulse when impulse mode is selected
	public float m_impulseDirection = 90f;					// direction of impulse in degrees when impulse mode is selected

	public float m_jumpMinHeight = 3f;						// jump min height height jump mode is used
	public float m_jumpMaxHeight = 4f;						// jump max height height jump mode is used

	public string m_animation = "Impulse";					// animation to play when impulse is launched
		

	////////////////////////////////////////////////////////
	// PRIVATE/LOW LEVEL
	bool m_playing;
	Animator m_anim;
	int m_animHash;

	void Awake()
	{
		m_anim = GetComponent<Animator>();
	}

	void ClearRuntimeValues()
	{
		m_playing = false;
		m_animHash = Animator.StringToHash(m_animation);
	}
	
	void Start () 
	{
		ClearRuntimeValues();
	}

	// called when character is entering this collectable
	public void OnTriggerEnter2D(Collider2D otherCollider)
	{
		// do nothing if not ebabled
		if(!enabled)
			return;

		if(m_playing)
		{
			// update playing state
			float fAnimTime = m_anim.GetCurrentAnimatorStateInfo(0).normalizedTime;
			if (fAnimTime >= 1f)
			{
				m_playing = false;
			}
			else
			{
				// do not allow impulse during play
				return;
			}
		}

		APCharacterController character = otherCollider.GetComponent<APCharacterController>();
		if(character != null)
		{
			m_playing = true;
			m_anim.Play(m_animHash, 0, 0f);

			// launch impulse/jump
			if(m_mode == EImpulseMode.Impulse)
			{
				float fAngle = -Mathf.Deg2Rad * m_impulseDirection;
				Vector2 v2ImpulseDir = new Vector2(Mathf.Cos(fAngle), -Mathf.Sin(fAngle));
				v2ImpulseDir = transform.TransformDirection(v2ImpulseDir);

				Vector2 charVel = character.GetVelocity();
				float velAlongImpulse = Vector2.Dot(charVel, v2ImpulseDir);
				charVel += v2ImpulseDir * (m_impulsePower - velAlongImpulse);
				character.SetVelocity(charVel);
			}
			else
			{
				character.Jump(m_jumpMinHeight, m_jumpMaxHeight);
			}
		}
	}
}
