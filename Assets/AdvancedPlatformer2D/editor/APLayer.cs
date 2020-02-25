/* Copyright (c) 2014 Advanced Platformer 2D */

using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(APUnityLayer))]
public class APUnityLayerPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
    {
        EditorGUI.BeginProperty(_position, GUIContent.none, _property);
        SerializedProperty layerIndex = _property.FindPropertyRelative("m_layerIndex");
        if (layerIndex != null)
        {
            layerIndex.intValue = EditorGUI.LayerField(_position, _label, layerIndex.intValue);
        }
        EditorGUI.EndProperty();
    }
}
