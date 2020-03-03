/* Copyright (c) 2014 Advanced Platformer 2D */

using UnityEngine;

[System.Serializable]
public abstract class APInputJoystickPlugin : MonoBehaviour
{
	public abstract float GetAxisX();	
	public abstract float GetAxisY();
}
