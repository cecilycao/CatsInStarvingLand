/* Copyright (c) 2014 Advanced Platformer 2D */
using UnityEngine;

public class APCharacterEventListener : MonoBehaviour
{
	public virtual void OnTouchGround() {}
	public virtual void OnLeaveGround() {}
	public virtual void OnJump() {}

	public virtual void OnGlideStart() {}
	public virtual void OnGlideEnd() {}

	public virtual void OnWallSlideStart() {}
	public virtual void OnWallSlideEnd() {}

	public virtual void OnCrouchStart() {}
	public virtual void OnCrouchEnd() {}

	public virtual void OnWallJumpStart() {}
	public virtual void OnWallJumpEnd() {}

	public virtual void OnAttackStart(APAttack attack) {}
	public virtual void OnAttackBulletFired(APAttack attack, APBullet bullet) {}
    public virtual void OnAttackMeleeHit(APAttack attack, APHitable hitObject) { }
	public virtual void OnAttackEnd(APAttack attack) {}

	// Called when a select is made to specified attack
	public virtual void OnSelectAttack(APAttack attack) {}

	public virtual void OnShiftStart() {}
	public virtual void OnShiftEnd() {}

	public virtual void OnEdgeGrabStart(APEdgeGrab grabObject) {}
	public virtual void OnEdgeGrabEnd(APEdgeGrab grabObject) {}

	public virtual void OnLadderCatch(APLadder ladder) {}
	public virtual void OnLadderRelease(APLadder ladder) {}

	public virtual void OnRailingsCatch(APRailings railings) {}
	public virtual void OnRailingsRelease(APRailings railings) {}

}


