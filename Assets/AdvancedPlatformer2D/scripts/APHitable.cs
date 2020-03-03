/* Copyright (c) 2014 Advanced Platformer 2D */

using UnityEngine;

[AddComponentMenu("Advanced Platformer 2D/Hitable")]
public class APHitable : MonoBehaviour 
{
    // called when we have been hit by a melee attack
	// - launcher : character controller launching the attack
	// - hitZone : hitzone detecting the hit
	// - return false if you want engine to ignore the hit
	virtual public bool OnMeleeAttackHit(APCharacterController launcher, APHitZone hitZone) { return false; }

	// called when we have been hit by a bullet
	// - launcher : character controller launching the bullet
	// - bullet : reference to bullet touching us
	// - return false if you want engine to ignore the hit, true if you want bullet to be destroyed
	virtual public bool OnBulletHit(APCharacterController launcher, APBullet bullet) { return false; }

	// called when character is touching us with a ray
	// - motor : character controller touching the hitable
	// - rayType : type of ray detecting the hit
	// - hit : hit info
	// - penetration : penetration distance (from player box surface to hit point, can be negative)
	// - hitMaterial : material of hit collided (can be null)
	// - return false if contact should be ignored by engine, true otherwise
	virtual public bool OnCharacterTouch(APCharacterController launcher, APCharacterMotor.RayType rayType, RaycastHit2D hit, 
	                                     float penetration, APMaterial hitMaterial) { return true; }
}

