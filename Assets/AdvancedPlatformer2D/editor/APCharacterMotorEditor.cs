/* Copyright (c) 2014 Advanced Platformer 2D */

using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(APCharacterMotor))]
[CanEditMultipleObjects]
public class APCharacterMotorEditor : Editor
{
	static GUIContent m_buildContent = new GUIContent("Rebuild", "Rebuild rays using current properties and assigned collider");

	enum direction
	{
		left = 0,
		right,
		up,
		down
	}

	public override void OnInspectorGUI () 
	{
		serializedObject.Update();

		this.DrawDefaultInspector();
		this.DrawAutoBuilder();

		serializedObject.ApplyModifiedProperties();
	}

	public void OnSceneGUI () 
	{
		APCharacterMotor oMotor = (APCharacterMotor)target;
		if(oMotor.m_DrawRays)
		{
			DrawRays(oMotor, oMotor.m_RaysGround, Color.blue, direction.down);
			DrawRays(oMotor, oMotor.m_RaysFront, Color.red, oMotor.m_faceRight ? direction.right : direction.left);
			DrawRays(oMotor, oMotor.m_RaysBack, Color.magenta, oMotor.m_faceRight ? direction.left : direction.right);
			DrawRays(oMotor, oMotor.m_RaysUp, Color.cyan, direction.up);
		}
	}

	void DrawRays (APCharacterMotor oMotor, APCharacterMotor.CharacterRay2D[] rays, Color color, direction direction) 
	{
		Transform rayTransform = oMotor.transform;

		for(int i = 0; i < rays.Length; i++)
		{
			APCharacterMotor.CharacterRay2D curRay = rays[i];
			Vector3 pointPos = oMotor.GetRayPositionWs(curRay);
			Handles.color = color;
			Vector3 newPos = Handles.FreeMoveHandle(pointPos, Quaternion.identity, 0.03f, Vector3.zero, Handles.DotHandleCap);
			if(newPos != pointPos)
			{
				Undo.RecordObject(oMotor, "Move Ray Point");
				curRay.m_position = rayTransform.InverseTransformPoint(newPos);
				UpdateRayPenetrationOnMove(oMotor, curRay, newPos, direction);

				// mark object as dirty
				EditorUtility.SetDirty(oMotor);
			}

			// Draw ray line
			Vector3 rayDir = rayTransform.up;
			float fPenScale = rayTransform.lossyScale.y * oMotor.Scale.y;
			switch(direction)
			{
			case direction.left: 	rayDir = -rayTransform.right; fPenScale = rayTransform.lossyScale.x * oMotor.Scale.x; break;
			case direction.right: 	rayDir = rayTransform.right; fPenScale = rayTransform.lossyScale.x * oMotor.Scale.x; break;
			case direction.down: 	rayDir = -rayTransform.up;	break;
			}

			float fPen = curRay.m_penetration * Mathf.Abs(fPenScale);
			Handles.DrawLine(pointPos, pointPos + (Vector3)rayDir * (fPen + curRay.m_extraDistance));
			Handles.CircleHandleCap(0, pointPos + (Vector3)rayDir * fPen, Quaternion.identity, 0.03f, EventType.Repaint);
		}
	}

	void UpdateRayPenetrationOnMove(APCharacterMotor oMotor, APCharacterMotor.CharacterRay2D rayToUpdate, Vector2 newPosWs, direction direction)
	{
		// Get characters box in world space if available
		Transform rayTransform = oMotor.transform;
		Vector2 newPosLs = rayTransform.InverseTransformPoint(newPosWs);

		// for now we support boxcollider2d only !
		System.Type colliderType = oMotor.GetComponent<Collider2D>().GetType();
		if(oMotor.GetComponent<Collider2D>() && (colliderType == typeof(UnityEngine.BoxCollider2D)) )
		{
			UnityEngine.BoxCollider2D boxCollider = (UnityEngine.BoxCollider2D)oMotor.GetComponent<Collider2D>();
			Vector2 centerLs = boxCollider.offset;

			// compute distance from ray position to collider border
			float fNewPenetration = 0f;
			switch(direction)
			{
			case direction.left: 	fNewPenetration = newPosLs.x - (centerLs.x - 0.5f * boxCollider.size.x); break;
			case direction.right: 	fNewPenetration = (centerLs.x + 0.5f * boxCollider.size.x) - newPosLs.x; break;
			case direction.up: 		fNewPenetration = (centerLs.y + 0.5f * boxCollider.size.y) - newPosLs.y; break;
			case direction.down: 	fNewPenetration = newPosLs.y - (centerLs.y - 0.5f * boxCollider.size.y); break;
			}

			fNewPenetration = Mathf.Max(0f, fNewPenetration);
			rayToUpdate.m_penetration = fNewPenetration;// / Mathf.Abs(rayTransform.lossyScale.y); // remove any scale as is it applied at runtime
		}
	}

	void DrawAutoBuilder()
	{
		//m_autoBuilderFold = EditorGUILayout.Foldout(m_autoBuilderFold, m_autoBuilderText);
		//if(m_autoBuilderFold)
		{
			// handle button
			GUILayout.BeginHorizontal();
			GUILayout.Space(15);
			if(GUILayout.Button(m_buildContent, EditorStyles.miniButton, GUILayout.MaxWidth(50f)))
			{
				RebuildRays();
			}
			GUILayout.EndVertical();

			// prevent bad values
			SerializedProperty autoBuilderProp = serializedObject.FindProperty("m_autoBuilder");
			if(autoBuilderProp.isExpanded)
			{
				SerializedProperty rayCountX = autoBuilderProp.FindPropertyRelative("m_rayCountX");
				if(rayCountX.intValue < 2)
					rayCountX.intValue = 2;

				SerializedProperty rayCountY = autoBuilderProp.FindPropertyRelative("m_rayCountY");
				if(rayCountY.intValue < 2)
					rayCountY.intValue = 2;
			}
		}
	}

	void RebuildRays()
	{
		for(int targetId = 0; targetId < targets.Length; targetId++)
		{
			// Get characters box if available
			APCharacterMotor oMotor = (APCharacterMotor)targets[targetId];
			Undo.RecordObject(oMotor, "RebuildRays");
			Transform rayTransform = oMotor.transform;

			// for now we support boxcollider2d only !
			System.Type colliderType = oMotor.GetComponent<Collider2D>().GetType();
			if(oMotor.GetComponent<Collider2D>() && (colliderType == typeof(UnityEngine.BoxCollider2D)) )
			{
				UnityEngine.BoxCollider2D boxCollider = (UnityEngine.BoxCollider2D)oMotor.GetComponent<Collider2D>();

				// build list of horizontal rays
				APCharacterMotor.CharacterRay2D[] raysUp = new APCharacterMotor.CharacterRay2D[oMotor.m_autoBuilder.m_rayCountX];
				APCharacterMotor.CharacterRay2D[] raysDown = new APCharacterMotor.CharacterRay2D[oMotor.m_autoBuilder.m_rayCountX];
				APCharacterMotor.CharacterRay2D[] raysFront = new APCharacterMotor.CharacterRay2D[oMotor.m_autoBuilder.m_rayCountY];
				APCharacterMotor.CharacterRay2D[] raysBack = new APCharacterMotor.CharacterRay2D[oMotor.m_autoBuilder.m_rayCountY];

				// rays up/down
				{
					float boxWidthX = boxCollider.size.x * oMotor.m_autoBuilder.m_rayXBoxScale.x;
					float boxWidthY = boxCollider.size.y * oMotor.m_autoBuilder.m_rayXBoxScale.y;
					float fOffsetX = -boxWidthX * 0.5f + boxCollider.offset.x;
					float fOffsetY = -boxWidthY * 0.5f + boxCollider.offset.y;
                    float fSpaceX = oMotor.m_autoBuilder.m_rayCountX > 1 ? boxWidthX / (oMotor.m_autoBuilder.m_rayCountX - 1) : 0f;

					for(int i = 0; i < oMotor.m_autoBuilder.m_rayCountX; i++)
					{
						APCharacterMotor.CharacterRay2D curRayUp = new APCharacterMotor.CharacterRay2D();
						curRayUp.m_position.x = fOffsetX + i * fSpaceX;
						curRayUp.m_position.y = fOffsetY + boxWidthY;
						curRayUp.m_extraDistance = oMotor.m_autoBuilder.m_extraDistanceUp;
						UpdateRayPenetrationOnMove(oMotor, curRayUp, rayTransform.TransformPoint(curRayUp.m_position), direction.up);

						APCharacterMotor.CharacterRay2D curRayDown = new APCharacterMotor.CharacterRay2D();
						curRayDown.m_position.x = curRayUp.m_position.x;
						curRayDown.m_position.y = fOffsetY;
						curRayDown.m_extraDistance = oMotor.m_autoBuilder.m_extraDistanceDown;
						UpdateRayPenetrationOnMove(oMotor, curRayDown, rayTransform.TransformPoint(curRayDown.m_position), direction.down);

						raysUp[i] = curRayUp;
						raysDown[i] = curRayDown;
					}
				}

				// rays front/back
				{
					float boxWidthX = boxCollider.size.x * oMotor.m_autoBuilder.m_rayYBoxScale.x;
					float boxWidthY = boxCollider.size.y * oMotor.m_autoBuilder.m_rayYBoxScale.y;
					float fOffsetX = -boxWidthX * 0.5f + boxCollider.offset.x;
					float fOffsetY = -boxWidthY * 0.5f + boxCollider.offset.y;
                    float fSpaceY = oMotor.m_autoBuilder.m_rayCountY > 1 ? boxWidthY / (oMotor.m_autoBuilder.m_rayCountY - 1) : 0f;

					for(int i = 0; i < oMotor.m_autoBuilder.m_rayCountY; i++)
					{
						APCharacterMotor.CharacterRay2D curRayFront = new APCharacterMotor.CharacterRay2D();
						curRayFront.m_position.x = oMotor.m_faceRight ? fOffsetX + boxWidthX : fOffsetX;
						curRayFront.m_position.y = fOffsetY + i * fSpaceY;
						curRayFront.m_extraDistance = oMotor.m_autoBuilder.m_extraDistanceFront;
						UpdateRayPenetrationOnMove(oMotor, curRayFront, rayTransform.TransformPoint(curRayFront.m_position), oMotor.m_faceRight ? direction.right : direction.left);
						
						APCharacterMotor.CharacterRay2D curRayBack = new APCharacterMotor.CharacterRay2D();
						curRayBack.m_position.x = oMotor.m_faceRight ? fOffsetX : fOffsetX + boxWidthX;
						curRayBack.m_position.y = curRayFront.m_position.y;
						curRayBack.m_extraDistance = oMotor.m_autoBuilder.m_extraDistanceBack;
						UpdateRayPenetrationOnMove(oMotor, curRayBack, rayTransform.TransformPoint(curRayBack.m_position), oMotor.m_faceRight ? direction.left : direction.right);
						
						raysFront[i] = curRayFront;
						raysBack[i] = curRayBack;
					}
				}

				oMotor.m_RaysUp = raysUp;
				oMotor.m_RaysGround = raysDown;
				oMotor.m_RaysFront = raysFront;
				oMotor.m_RaysBack = raysBack;

				// mark object as dirty
				EditorUtility.SetDirty(oMotor);
			}
		}
	}
}
