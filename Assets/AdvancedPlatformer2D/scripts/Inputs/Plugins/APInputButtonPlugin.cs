/* Copyright (c) 2014 Advanced Platformer 2D */

using UnityEngine;

[System.Serializable]
public abstract class APInputButtonPlugin : MonoBehaviour
{
	public abstract bool GetButton();
	public abstract bool GetButtonDown();
	public abstract bool GetButtonUp();
}

