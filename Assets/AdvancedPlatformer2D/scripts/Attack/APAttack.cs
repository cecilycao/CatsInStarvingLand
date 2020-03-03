/* Copyright (c) 2014 Advanced Platformer 2D */

using UnityEngine;

[System.Serializable]
public class APAttack
{
    ////////////////////////////////////////////////////////
    // PUBLIC/HIGH LEVEL
    public string m_name;                           // you can use a name to identify your attack
    public bool m_enabled = true;			        // activation status
	public APInputButton m_button;				    // button to use for this attack
	public APAmmo m_ammoBox;						// ammo object used to count remaining ammo for this attack
	public bool m_infiniteAmmo = false;			    // ignore ammo count?
	public bool m_autoFire = false;				    // enable/disable autofire
	public APBullet m_bullet;					    // reference to bullet prefab to launch
    public APHitZone[] m_hitZones;				    // list of hit zones for hit detection
	public bool m_useAimAnimations = false;			// tells if this attack use aim context or not

	public AttackContext m_contextStand; 			// when attacking while stopped
	public AttackContext m_contextRun; 				// when attacking while running
	public AttackContext m_contextCrouched; 		// when attacking in crouched position
	public AttackContext m_contextInAir; 			// when attacking while in air

	public AttackContextAim m_contextAimFront;
	public AttackContextAim m_contextAimDown;
	public AttackContextAim m_contextAimUp;
	public AttackContextAim m_contextAimFrontUp;
	public AttackContextAim m_contextAimFrontDown;

	[System.Serializable]
	public class AttackContext
	{
        public bool m_enabled = false;                  // activation status
		public APAnimation m_anim;						// animation used when in current context
		public APAnimation m_animFire;					// animation used when firing in current context
		public Vector2 m_bulletStartPosition;           // spawn position of bullet
		public float m_bulletDirection;		            // launch direction
        public bool m_stopOnGround = false;             // should we stop moving while on ground during an attack
    }

	[System.Serializable]
	public class AttackContextAim : AttackContext
	{
		public APAnimation m_animRun;					// animation used when running while aiming
    }

	public bool CanFireBullet()
	{
		return (m_ammoBox != null && m_ammoBox.m_ammoCount > 0 || m_infiniteAmmo) && (m_bullet != null);
	}

	public bool IsActive()
	{
		return m_enabled && (m_bullet == null) || CanFireBullet();
	}

	public void FireBullet(APCharacterController launcher, AttackContext context)
	{
		if (CanFireBullet())
		{
			if ( !m_infiniteAmmo )
			{
				m_ammoBox.AddAmmo(-1);
			}

			// make sure move horizontal direction is valid
			bool bFaceRight = launcher.GetMotor().m_faceRight;
			float fAngle = Mathf.Deg2Rad * context.m_bulletDirection;
			Vector2 v2MoveDir = new Vector2(Mathf.Cos(fAngle), -Mathf.Sin(fAngle));
			if(bFaceRight && v2MoveDir.x < 0f || !bFaceRight && v2MoveDir.x > 0f)
			{
				v2MoveDir.x = -v2MoveDir.x;
			}

			// spawn and launch bullet (add player velocity before spawn)
			Vector2 pointPos = launcher.transform.TransformPoint(context.m_bulletStartPosition);
			pointPos = pointPos + (Time.deltaTime * launcher.GetMotor().m_velocity);

			float bulletSign = m_bullet.m_faceRight ? 1f : -1f;
			Quaternion rot = Quaternion.Euler(0f, 0f, bFaceRight != m_bullet.m_faceRight ? bulletSign * context.m_bulletDirection : -bulletSign * context.m_bulletDirection);
			APBullet newBullet = (APBullet)UnityEngine.Object.Instantiate(m_bullet, pointPos, rot);
			newBullet.enabled = true;

			// init bullet
			v2MoveDir = launcher.transform.TransformDirection(v2MoveDir);
			newBullet.Setup(launcher, v2MoveDir);

			// launch listeners
			launcher.EventListeners.ForEach(e => e.OnAttackBulletFired(this, newBullet));
		}
	}

	public void RefreshAnimations(APCharacterController character)
	{
		AttackContext ctx = GetCurrentContext(character);
		if(ctx != null && ctx.m_enabled)
		{
			// Handle aim sub context properly
			AttackContextAim ctxAim = ctx as AttackContextAim;
			if(ctxAim != null)
			{
				character.PlayAnim(character.IsRunning() && ctxAim.m_animRun.IsValid() ? ctxAim.m_animRun : ctxAim.m_anim);
			}
			else
			{
				// keep default set of animation if not overrided by each attack
				if(ctx.m_anim.IsValid())
				{
					character.PlayAnim(ctx.m_anim);
				}
			}
		}
	}

	public AttackContext GetCurrentContext(APCharacterController character)
	{
		if(m_useAimAnimations)
		{
			// Check inputs and update animation according to this
			AttackContext ctx = m_contextAimFront;
			bool bFront = Mathf.Abs(character.m_inputs.m_axisX.GetValue()) > 0f;
			bool bUp = character.m_inputs.m_axisY.GetValue() > 0f;
			bool bDown = character.m_inputs.m_axisY.GetValue() < 0f;
			if (bFront)
			{
				if (bUp)
				{
					ctx = m_contextAimFrontUp;
				}
				else if(bDown)
				{
					ctx = m_contextAimFrontDown;
				}
			}
			else if (bUp)
			{
				ctx = m_contextAimUp;
			}
			else if(bDown)
			{
				ctx = m_contextAimDown;
			}
			return ctx;
		}
		else
		{
			bool bCrouched = character.IsCrouched();
			if (bCrouched)
			{
				return m_contextCrouched;
			}
			else if (character.IsOnGround())
			{
				if (!character.IsRunning())
				{
					return m_contextStand;
				}
				else
				{
					return m_contextRun;
				}
			}
			else
			{
				return m_contextInAir;
			}
		}
	}
}
