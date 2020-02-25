/* Copyright (c) 2014 Advanced Platformer 2D */

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(APCharacterController))]
[CanEditMultipleObjects]
public class APCharacterControllerEditor : Editor
{
	bool m_bShow = false;
	List<bool> m_bShowBulletSpawnsStand = new List<bool>(8);
	List<bool> m_bShowBulletSpawnsRun = new List<bool>(8);
	List<bool> m_bShowBulletSpawnsInAir = new List<bool>(8);
	List<bool> m_bShowBulletSpawnsCrouched = new List<bool>(8);

	List<bool> m_bShowBulletSpawnsFront = new List<bool>(8);
	List<bool> m_bShowBulletSpawnsUp = new List<bool>(8);
	List<bool> m_bShowBulletSpawnsDown = new List<bool>(8);
	List<bool> m_bShowBulletSpawnsFrontUp = new List<bool>(8);
	List<bool> m_bShowBulletSpawnsFrontDown = new List<bool>(8);

	public override void OnInspectorGUI () 
	{
		base.OnInspectorGUI();

		serializedObject.Update();

		SerializedProperty prop = serializedObject.FindProperty("m_attacks");
		m_bShow = prop != null && prop.isExpanded;
		if(m_bShow)
		{
			SerializedProperty propChilds = prop.FindPropertyRelative("m_attacks");
			m_bShow = propChilds != null && propChilds.isExpanded;
			if(m_bShow)
			{
				m_bShowBulletSpawnsStand.Clear();
				m_bShowBulletSpawnsRun.Clear();
				m_bShowBulletSpawnsInAir.Clear();
				m_bShowBulletSpawnsCrouched.Clear();

				m_bShowBulletSpawnsFront.Clear();
				m_bShowBulletSpawnsUp.Clear();
				m_bShowBulletSpawnsDown.Clear();
				m_bShowBulletSpawnsFrontUp.Clear();
				m_bShowBulletSpawnsFrontDown.Clear();

				int attackCount = propChilds.arraySize;
				for(int i = 0; i < attackCount; i++)
				{
					SerializedProperty propChild = propChilds.GetArrayElementAtIndex(i);
					m_bShowBulletSpawnsStand.Add(propChild.FindPropertyRelative("m_contextStand").isExpanded);
					m_bShowBulletSpawnsRun.Add(propChild.FindPropertyRelative("m_contextRun").isExpanded);
					m_bShowBulletSpawnsInAir.Add(propChild.FindPropertyRelative("m_contextInAir").isExpanded);
					m_bShowBulletSpawnsCrouched.Add(propChild.FindPropertyRelative("m_contextCrouched").isExpanded);

					m_bShowBulletSpawnsFront.Add(propChild.FindPropertyRelative("m_contextAimFront").isExpanded);
					m_bShowBulletSpawnsUp.Add(propChild.FindPropertyRelative("m_contextAimDown").isExpanded);
					m_bShowBulletSpawnsDown.Add(propChild.FindPropertyRelative("m_contextAimUp").isExpanded);
					m_bShowBulletSpawnsFrontUp.Add(propChild.FindPropertyRelative("m_contextAimFrontUp").isExpanded);
					m_bShowBulletSpawnsFrontDown.Add(propChild.FindPropertyRelative("m_contextAimFrontDown").isExpanded);
				}
			}
		}

        serializedObject.ApplyModifiedProperties();
	}

	public void OnSceneGUI () 
	{
		APCharacterController oController = (APCharacterController)target;

		// draw all melee attack zones and allow to move them when selecting controller
		if (oController.m_attacks != null && oController.m_attacks.m_enabled)
		{
            foreach (APAttack curAttack in oController.m_attacks.m_attacks)
			{
				foreach (APHitZone curZone in curAttack.m_hitZones)
				{
					if (curZone.m_active && curZone.gameObject.activeInHierarchy)
					{
						Vector3 pointPos = curZone.transform.position;
						Color color = Color.green;
						color.a = 0.5f;
						Handles.color = color;
						Vector3 newPos = Handles.FreeMoveHandle(pointPos, Quaternion.identity, curZone.m_radius * 2f, Vector3.zero, Handles.SphereHandleCap);
						if (newPos != pointPos)
						{
							Undo.RecordObject(curZone.transform, "Move Hit Zone");
							curZone.transform.position = newPos;

							// mark object as dirty
							EditorUtility.SetDirty(curZone);
						}
					}
				}
			}
		}

		// draw all ranged attack spawning bullets and allow to move them when selecting controller
        if (oController.m_attacks != null && oController.m_attacks.m_enabled && m_bShow)
		{
            for (int i = 0; i < oController.m_attacks.m_attacks.Length; i++)
			{
                APAttack curAttack = oController.m_attacks.m_attacks[i];
				DrawStartPos(curAttack.m_contextStand, m_bShowBulletSpawnsStand[i]);
				DrawStartPos(curAttack.m_contextRun, m_bShowBulletSpawnsRun[i]);
				DrawStartPos(curAttack.m_contextInAir, m_bShowBulletSpawnsInAir[i]);
				DrawStartPos(curAttack.m_contextCrouched, m_bShowBulletSpawnsCrouched[i]);

				DrawStartPos(curAttack.m_contextAimFront, m_bShowBulletSpawnsFront[i]);
				DrawStartPos(curAttack.m_contextAimDown, m_bShowBulletSpawnsUp[i]);
				DrawStartPos(curAttack.m_contextAimUp, m_bShowBulletSpawnsDown[i]);
				DrawStartPos(curAttack.m_contextAimFrontUp, m_bShowBulletSpawnsFrontUp[i]);
				DrawStartPos(curAttack.m_contextAimFrontDown, m_bShowBulletSpawnsFrontDown[i]);
			}
		}
	}

	public void DrawStartPos(APAttack.AttackContext context, bool bDraw)
	{
		if(bDraw && context.m_enabled)
		{
			APCharacterController oController = (APCharacterController)target;
			Vector3 pointPos = oController.transform.TransformPoint(context.m_bulletStartPosition);
			Color color = Color.cyan;
			color.a = 0.5f;
			Handles.color = color;
			Vector3 newPos = Handles.FreeMoveHandle(pointPos, Quaternion.identity, 0.1f, Vector3.zero, Handles.SphereHandleCap);
			if (GUI.changed)
			{
				Undo.RecordObject(oController, "Move Bullet Position");
				context.m_bulletStartPosition = oController.transform.InverseTransformPoint(newPos);
				
				// mark object as dirty
				EditorUtility.SetDirty(oController);
			}

			// draw direction arrow
			float fAngle = Mathf.Deg2Rad * context.m_bulletDirection;
			Vector2 v2MoveDir = new Vector2(Mathf.Cos(fAngle), -Mathf.Sin(fAngle));

			APCharacterMotor oMotor = oController.GetComponent<APCharacterMotor>();
			if(oMotor && ((oMotor.m_faceRight && v2MoveDir.x < 0f) || (!oMotor.m_faceRight && v2MoveDir.x > 0f)))
			{
				v2MoveDir.x = -v2MoveDir.x;
			}

			v2MoveDir = oController.transform.TransformDirection(v2MoveDir);

			Quaternion rot = Quaternion.LookRotation(v2MoveDir);
			Handles.ArrowHandleCap(0, pointPos, rot, 0.5f, EventType.Repaint);
		}
	}
}
