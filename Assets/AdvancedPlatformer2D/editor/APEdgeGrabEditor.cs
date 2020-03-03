/* Copyright (c) 2014 Advanced Platformer 2D */

using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(APEdgeGrab))]
[CanEditMultipleObjects]
public class APEdgeGrabEditor : Editor
{
	public void OnSceneGUI () 
	{
		APEdgeGrab oGrab = (APEdgeGrab)target;
		
		// draw handle
		if (oGrab.enabled)
		{
            if (oGrab.m_grabFromTop.m_enabled)
                UpdateGrabPosition(oGrab, ref oGrab.m_grabFromTop.m_handle, Color.cyan, oGrab.m_grabFromTop.m_handleRadius);

            if (oGrab.m_grabInAir.m_enabled)
                UpdateGrabPosition(oGrab, ref oGrab.m_grabInAir.m_handle, Color.magenta, oGrab.m_grabInAir.m_handleRadius);
		}
	}

	void UpdateGrabPosition(APEdgeGrab oGrab, ref Vector3 grabHandle, Color color, float radius)
	{
		Vector3 pointPos = oGrab.transform.TransformPoint(grabHandle);
		color.a = 0.5f;
		Handles.color = color;
        Vector3 newPos = Handles.FreeMoveHandle(pointPos, Quaternion.identity, radius * 2f, Vector3.zero, Handles.SphereHandleCap);
		if (newPos != pointPos)
		{
			Undo.RecordObject(oGrab, "Move grab handle");
			grabHandle = oGrab.transform.InverseTransformPoint(newPos);
			
			// mark object as dirty
			EditorUtility.SetDirty(oGrab);
		}
	}
}
