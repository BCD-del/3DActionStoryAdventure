// Copyright © Magnetic Arcade. All Rights Reserved.

using UnityEditor;
using UnityEngine;

namespace MA.Core.Editor
{
    [CustomPropertyDrawer(typeof(OverridableValue<>))]
    class OverridableValuePropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.wideMode ? EditorGUIUtility.singleLineHeight : EditorGUIUtility.singleLineHeight * 2;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Split the total position into two columns
            const float checkboxWidth = 26;
            const float checkboxMarginY = 1;
            Rect checkboxRect = new Rect(position.x - checkboxWidth, position.y - checkboxMarginY, checkboxWidth, position.height + checkboxMarginY * 2);
            Rect labelRect = new Rect(position.x, position.y, position.width, position.height);

            // Draw the override state checkbox and the value property field on the same line
            SerializedProperty overrideState = property.FindPropertyRelative("OverrideState");
            overrideState.boolValue = EditorGUI.Toggle(checkboxRect, GUIContent.none, overrideState.boolValue, EditorStyles.toggle);

            EditorGUI.BeginDisabledGroup(!overrideState.boolValue);
            Rect valueRect = EditorGUI.PrefixLabel(labelRect, label);
            EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("Value"), GUIContent.none);
            EditorGUI.EndDisabledGroup();

            EditorGUI.EndProperty();
        }
    }
}
