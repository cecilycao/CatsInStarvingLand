/* Copyright (c) 2014 Advanced Platformer 2D */

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(APJumper))]
[CanEditMultipleObjects]
public class APJumperEditor : Editor
{
	public override void OnInspectorGUI () 
	{
		serializedObject.Update();
		
		APJumper jumper = (APJumper)target;

		EditorGUILayout.PropertyField(serializedObject.FindProperty("m_animation"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("m_mode"));

		if(jumper.m_mode == APJumper.EImpulseMode.Impulse)
		{
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_impulsePower"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_impulseDirection"));
		}
		else
		{
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_jumpMinHeight"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_jumpMaxHeight"));
		}

		serializedObject.ApplyModifiedProperties();
	}

	public void OnSceneGUI () 
	{
		APJumper oJumper = (APJumper)target;

		if (oJumper.enabled)
		{
			// draw direction arrow
			Color color = Color.cyan;
			color.a = 0.5f;
			Handles.color = color;

			float fAngle = oJumper.m_mode == APJumper.EImpulseMode.Impulse ? -Mathf.Deg2Rad * oJumper.m_impulseDirection : -Mathf.Deg2Rad * 90f;
			Vector2 v2ImpulseDir = new Vector2(Mathf.Cos(fAngle), -Mathf.Sin(fAngle));

			if(oJumper.m_mode == APJumper.EImpulseMode.Impulse)
			{
				v2ImpulseDir = oJumper.transform.TransformDirection(v2ImpulseDir);
			}

			//Vector3 dir = new Vector3(-oJumper.m_impulseDirection, 90f, 0f);
			Quaternion rot = Quaternion.LookRotation(v2ImpulseDir);
			Handles.ArrowHandleCap(0, oJumper.transform.position, rot, 0.5f, EventType.Repaint);
		}
	}
}
