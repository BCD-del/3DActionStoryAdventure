// Copyright © Magnetic Arcade. All Rights Reserved.

using UnityEditor;
using UnityEngine;

namespace MA.Core.Editor
{
    [CustomPropertyDrawer(typeof(SerializableGuid))]
    class SerializableGuidPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.wideMode ? EditorGUIUtility.singleLineHeight : EditorGUIUtility.singleLineHeight * 2;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty guidLow = property.FindPropertyRelative("m_GuidLow");
            SerializedProperty guidHigh = property.FindPropertyRelative("m_GuidHigh");
            
#if UNITY_2022_2_OR_NEWER
            SerializableGuid guid = new SerializableGuid(guidLow.ulongValue, guidHigh.ulongValue);
#else
            SerializableGuid guid = new SerializableGuid((ulong)guidLow.longValue, (ulong)guidHigh.longValue);
#endif
            
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.TextField(position, label, guid.ToString());
            EditorGUI.EndDisabledGroup();
            EditorGUI.EndProperty();
        }
    }
}