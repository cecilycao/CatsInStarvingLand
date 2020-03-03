/* Copyright (c) 2014 Advanced Platformer 2D */

using UnityEngine;

[RequireComponent(typeof(APCharacterController))]
[AddComponentMenu("Advanced Platformer 2D/TargetAnimation")]

[System.Serializable]
public class APTargetAnim : MonoBehaviour
{
    ////////////////////////////////////////////////////////
    // PUBLIC/HIGH LEVEL
    public APAnimation m_targetFront;
	public APAnimation m_targetFrontRun;
	public APAnimation m_targetUp;
	public APAnimation m_targetDown;
	public APAnimation m_targetFrontUp;
	public APAnimation m_targetFrontDown;

	 ////////////////////////////////////////////////////////
    // PRIVATE
	private APCharacterController m_player;

	void Awake()
	{
		m_player = GetComponent<APCharacterController>();
	}

	void Update()
	{
		// Early exits
		if(!m_player || !m_player.enabled || m_player.IsControlled)
		{
			return;
		}

		// Check if player is in valid state
		if(m_player.GetState() != APCharacterController.State.Standard)
			return;

		// Check inputs and update animation according to this
		APAnimation anim = m_targetFront;
		bool bFront = Mathf.Abs(m_player.m_inputs.m_axisX.GetValue()) > 0f;
		bool bUp = m_player.m_inputs.m_axisY.GetValue() > 0f;
		bool bDown = m_player.m_inputs.m_axisY.GetValue() < 0f;
		if (bFront)
		{
			if (bUp)
			{
				anim = m_targetFrontUp;
			}
			else if(bDown)
			{
				anim = m_targetFrontDown;
			}
		}
		else if (bUp)
		{
			anim = m_targetUp;
		}
		else if(bDown)
		{
			anim = m_targetDown;
		}

		// Use front running animation if needed
		if((anim == m_targetFront) && m_player.IsRunning() && (m_targetFrontRun.GetAnimHash() != 0))
		{
			anim = m_targetFrontRun;
		}

		m_player.PlayAnim(anim);
	}   
}
