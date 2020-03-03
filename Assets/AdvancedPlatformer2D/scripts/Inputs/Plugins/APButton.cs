/* Copyright (c) 2014 Advanced Platformer 2D */

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[AddComponentMenu("Advanced Platformer 2D/Inputs/Button")]

[System.Serializable]
public class APButton : Button
{
	// Just to make a protected function as public...
	public bool IsPressedEx() { return IsPressed(); }
}
