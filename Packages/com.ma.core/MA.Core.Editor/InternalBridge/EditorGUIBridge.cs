// Copyright © Magnetic Arcade. All Rights Reserved.

using UnityEditor;
using UnityEngine;

namespace MA.Core.Editor.Bridge
{
    static class EditorGUIBridge
    {
        public static Rect MultiFieldPrefixLabel(Rect totalPosition, int id, GUIContent label, int columns) 
            => EditorGUI.MultiFieldPrefixLabel(totalPosition, id, label, columns);
        
        public static float GetLabelWidth(GUIContent label, float prefixLabelWidth = -1f) 
            => EditorGUI.GetLabelWidth(label, prefixLabelWidth);
    }
}