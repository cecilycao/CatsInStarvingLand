/* Copyright (c) 2014 Advanced Platformer 2D */

using UnityEngine;

[AddComponentMenu("Advanced Platformer 2D/Ranged Attack/Bullet")]
public class APBullet : MonoBehaviour
{
    ////////////////////////////////////////////////////////
    // PUBLIC/HIGH LEVEL
	public int m_hitDamage = 1;				// amount of hit point damage
	public float m_velocity = 10f;			// initial velocity when spawned (m/s)
	public float m_acceleration = 0f;		// acceleration over time (m/s²)
	public float m_lifeTime = 10f;			// if different than 0, time in seconds before dying
	public bool m_stopOnExplode = true;		// stop movement when exploding
    public bool m_explodeOnTouch = true;    // should the bullet explode on first touch
	public float m_destroyTimer = 0f;		// time after which object is destroyed when exploding
	public bool m_ignoreTrigger = true;		// ignore triggers
	public string m_animStateExplode;		// state of anim to launch when exploding
	public bool m_faceRight = true;			// tells if bullet is designed while facing right
   


	////////////////////////////////////////////////////////
	// PRIVATE/HIGH LEVEL
	Vector2 m_curVel;
	float m_spawnDuration;
	bool m_alive;
	APCharacterController m_launcher;
    Rigidbody2D m_rigidBody;
	Animator m_anim;

	public void Setup(APCharacterController launcher, Vector2 moveDir)
	{
		m_anim = GetComponent<Animator>();

		m_launcher = launcher;
		m_alive = true;
		m_spawnDuration = 0f;
		m_curVel = moveDir * m_velocity;
        m_rigidBody = GetComponent<Rigidbody2D>();
        if (m_rigidBody)
		{
            m_rigidBody.velocity = m_curVel;
		}

		// fix flipping
		if(launcher.GetMotor().FaceRight != m_faceRight)
		{
			Vector3 theScale = transform.localScale;
			theScale.x *= -1;
			transform.localScale = theScale;
		}

	}

	void FixedUpdate()
	{
		// update velocity if acceleration is set
		if(m_acceleration != 0f)
		{
			m_curVel += m_curVel * m_acceleration * Time.deltaTime;
            if (m_rigidBody)
			{
                m_rigidBody.velocity = m_curVel;
			}
		}

		// update position if not using rigid body
        if (!m_rigidBody)
		{
			Vector2 curPos = transform.position;
			curPos += m_curVel * Time.deltaTime;
			transform.position = curPos;
		}
	}

	void Update()
	{
		if(!m_alive)
			return;

		// destroy if timeout
		m_spawnDuration += Time.deltaTime;
		if(m_lifeTime > 0f && m_spawnDuration >= m_lifeTime)
		{
			m_alive = false;
			Object.Destroy(gameObject);
			return;
		}
	}

	public void OnTriggerEnter2D(Collider2D otherCollider)
	{
		// make sure we are not touching launcher itself
		if(m_alive && (otherCollider.gameObject != m_launcher.gameObject) && (otherCollider.isTrigger != m_ignoreTrigger))
		{
			// alert hitable if any
			APHitable hitable = otherCollider.GetComponent<APHitable>();
			if(hitable != null)
			{
				// ignore hit if requested
				if(!hitable.OnBulletHit(m_launcher, this))
					return;
			}

			// launch anim
			if(!string.IsNullOrEmpty(m_animStateExplode))
				m_anim.Play(m_animStateExplode);

			// stop move & destroy
            if (m_explodeOnTouch)
            {
                if (m_stopOnExplode)
                {
                    m_curVel = Vector2.zero;
                    if (m_rigidBody)
                    {
                        m_rigidBody.velocity = Vector2.zero;
                    }
                }

                m_alive = false;
                Object.Destroy(gameObject, m_destroyTimer);
            }
		}
	}
}
