/* Copyright (c) 2014 Advanced Platformer 2D */

using UnityEngine;

[AddComponentMenu("Advanced Platformer 2D/Ranged Attack/Ammo")]
public class APAmmo : MonoBehaviour
{
    ////////////////////////////////////////////////////////
    // PUBLIC/HIGH LEVEL
	public int m_ammoCount = 10;		// number of ammo
	public int m_maxAmmoCount = 50;		// max number of ammo

	public void AddAmmo(int ammo)
	{
		m_ammoCount += ammo;
		m_ammoCount = Mathf.Clamp(m_ammoCount, 0, m_maxAmmoCount);
	}

	public string GetAmmoString()
	{
		return m_ammoCount.ToString();
	}
}
