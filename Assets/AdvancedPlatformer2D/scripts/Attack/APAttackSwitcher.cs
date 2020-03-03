/* Copyright (c) 2014 Advanced Platformer 2D */

using UnityEngine;

[System.Serializable]
public class APAttackSwitcher
{
    ////////////////////////////////////////////////////////
    // PUBLIC/HIGH LEVEL
    public bool m_enabled = true;
    public bool m_loop = true;                          // loop on attack list when switching
    public APInputButton m_launchAttack;		        // button to use for the current active attack
    public APInputButton m_setPreviousAttack;			// button to switch to previous attack
    public APInputButton m_setNextAttack;			    // button to switch to the next attack
	public int m_currentAttackIndex = 0;				// current active attack index
	public string[] m_attacks;						    // list of attacks names
	
	public APCharacterController m_character { get; set; }
	APAttack m_currentAttack = null;

	public void Reset()
	{
		_RefreshAttack();

		m_setPreviousAttack.m_downPushDuration = 0f;
		m_setNextAttack.m_downPushDuration = 0f;		
	}

	public bool IsActive()
	{
		return m_enabled && m_attacks.Length > 0;
	}

	public void Manage()
	{
		if(m_setNextAttack.GetButtonDown())
		{
			SetNextAttack();
		}
		else if(m_setPreviousAttack.GetButtonDown())
		{
			SetPreviousAttack();
		}
	}

	public void RefreshAnimations()
	{
		if(m_currentAttack == null)
		{
			return;
		}

		// Refresh current attack animations
		m_currentAttack.RefreshAnimations(m_character);
	}
	
	public void SetNextAttack()
	{
		_Increment(1);
	}

	public void SetPreviousAttack()
	{
		_Increment(-1);
	}

	public APAttack GetCurrentAttack()
	{
		return m_currentAttack;
	}

	void _RefreshAttack()
	{
		if(m_currentAttackIndex < m_attacks.Length)
		{
			m_currentAttack = m_character.GetAttack(m_attacks[m_currentAttackIndex]);
		}
		else
		{
			m_currentAttack = null;
		}
	}

	void _Increment(int incr)
	{
		int previousAttackIndex = m_currentAttackIndex;
		m_currentAttackIndex += incr;
		if(m_loop)
		{
			if(m_currentAttackIndex >= m_attacks.Length)
			{
				m_currentAttackIndex = 0;
			}
			else if(m_currentAttackIndex < 0)
			{
				m_currentAttackIndex = m_attacks.Length - 1;
			}
		}
		else
		{
			m_currentAttackIndex = Mathf.Clamp(m_currentAttackIndex, 0, m_attacks.Length - 1);
		}

		if(previousAttackIndex != m_currentAttackIndex)
		{
			_RefreshAttack();

			// We switched to new attack, call listeners
			APAttack newAttack = GetCurrentAttack();
			m_character.EventListeners.ForEach(e => e.OnSelectAttack(newAttack));
		}
	}
}
