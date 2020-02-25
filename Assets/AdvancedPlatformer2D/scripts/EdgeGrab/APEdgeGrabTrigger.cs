/* Copyright (c) 2014 Advanced Platformer 2D */

using UnityEngine;
using System.Collections.Generic;

public class APEdgeGrabTrigger : MonoBehaviour 
{
    public void SetCharacter(APCharacterController character)
    {
        m_character = character;
    }

    void HandleEdgeGrab(Collider2D otherCollider)
    {
        // Early exit
        if (!m_character || !m_character.m_edgeGrab.m_enabled || m_character.IsNewStateRequested() || m_character.IsControlled)
            return;

        APEdgeGrab compEdgeGrab = otherCollider.GetComponent<APEdgeGrab>();
        if (compEdgeGrab == null)
            return;

        // List of allowed from state
        APCharacterController.State[] allowedStates = { APCharacterController.State.Crouch, APCharacterController.State.Standard, APCharacterController.State.Attack,
                                                          APCharacterController.State.Glide, APCharacterController.State.WallSlide, APCharacterController.State.Shift };
        APCharacterController.State curState = m_character.GetState();
        bool bAllowed = false;
        foreach (APCharacterController.State testState in allowedStates)
        {
            if (curState == testState)
            {
                bAllowed = true;
                break;
            }
        }

        if (bAllowed)
        {
            // we are touching a edge grab zone, give control if allowed
            compEdgeGrab.TryTakeControl(m_character, otherCollider);
        }
    }	

    // called when trigger is entering a collectable
    public void OnTriggerStay2D(Collider2D otherCollider)
    {
        HandleEdgeGrab(otherCollider);
    }

    // called when trigger is entering a collectable
    public void OnTriggerEnter2D(Collider2D otherCollider)
    {
        HandleEdgeGrab(otherCollider);
    }

    private APCharacterController m_character;
}
