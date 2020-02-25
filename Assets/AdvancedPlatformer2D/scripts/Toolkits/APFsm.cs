/* Copyright (c) 2014 Advanced Platformer 2D */
using System;

public enum APFsmStateEvent
{
	eEnter = 0,
	eUpdate,
	eLeave,
	
	_eMax
}

public abstract class APFsm
{	
	/// Virtual method called when state enter, update or leave occur
	public abstract void OnStateUpdate(APFsmStateEvent a_oEvent, uint a_oState);

	/// No state
	static uint mc_noState = uint.MaxValue;
	
	/// Default constructor
	public APFsm()
	{
        m_oCurrentState = mc_noState;
        m_oRequestedState = mc_noState;
        m_oPreviousState = mc_noState;
		m_fCurrentTime = 0f;
		m_fDeltaTime = 0f;
		m_bIsInUpdate = false;
        m_bChangeState = false;
	}
	
	/// Start the state machine update with a state
	public void StartFsm(uint a_newState)
	{
		SetRequestedState(a_newState);
	}
	
	/// Update the state machine
	public void UpdateFsm(float a_fDeltaTime)
	{
		m_bIsInUpdate = true;
		m_fDeltaTime = a_fDeltaTime;
		m_fCurrentTime += a_fDeltaTime;

		_SwitchStateIfNeeded();

		// Update current state
        if (m_oCurrentState != mc_noState)
		{
			_UpdateState();
		}
		
		_SwitchStateIfNeeded();
		m_bIsInUpdate = false;
	}
	
	/// Stop the state machine update
	public void StopFsm()
	{
        SetRequestedState(mc_noState);
	}
	
	/// Get the current state. 
	/// \warning Return last state, even if state has been asked to leave.
	public uint GetState() { return m_oCurrentState; }
	
	/// Get the previous state. 
	public uint GetPreviousState() { return m_oPreviousState; }
	
	/// Get the requested state. 
	public uint GetRequestedState() { return m_oRequestedState; }
	
	/// Set the next state.
	public void SetRequestedState(uint a_oState)
	{
        m_bChangeState = true;
        m_oRequestedState = a_oState;
		if(m_bIsInUpdate == false)
		{
			_SwitchStateIfNeeded();
		}
	}

    /// Check if new state transition is requested
    public bool IsNewStateRequested() { return m_bChangeState; }

	/// Get current state time
	public float GetFsmStateTime() { return m_fCurrentTime; }
	
	/// Get delta time
	public float GetFsmDeltaTime() { return m_fDeltaTime; }

	/// Set the current state
	void _SwitchStateIfNeeded()
	{
		if (m_bChangeState)
		{			
            // Leave the current state
		    _Leave();

			// Reset new state time
			m_fCurrentTime = 0f;
			
			// Load new state;
			_Enter();
			
			// Clear requested state
            m_oRequestedState = mc_noState;
            m_bChangeState = false;
		}
	}
	
	/// Enter new state (use in function SetState)
	void _Enter()
	{
		m_oPreviousState = m_oCurrentState;
		
		// Load the new state
		m_oCurrentState = m_oRequestedState;
		
		// Enter new state
        if (m_oCurrentState != mc_noState)
		{
			OnStateUpdate(APFsmStateEvent.eEnter, m_oCurrentState);
		}
	}
	
	/// Leave current state (use in function SetState)
	void _Leave()
	{
		// Leave current state
        if (m_oCurrentState != mc_noState)
		{
			OnStateUpdate(APFsmStateEvent.eLeave, m_oCurrentState);
		}
	}	

	void _UpdateState()
	{
		OnStateUpdate(APFsmStateEvent.eUpdate, m_oCurrentState);
	}

	/// The current state
	uint m_oCurrentState;

	/// The requested state
	uint m_oRequestedState;
	
	/// The Previous state
	uint m_oPreviousState;
	
	/// Current delta time
	float m_fDeltaTime;
	
	/// Current state time
	float m_fCurrentTime;
	
	/// Ensure SetState is not called during update
	bool m_bIsInUpdate;

    /// Request for a state change
    bool m_bChangeState;
}


