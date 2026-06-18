// Copyright © Magnetic Arcade. All Rights Reserved.

using UnityEditor;
using UnityEngine;

namespace MA.Mathematics.Editor
{
    [CustomPropertyDrawer(typeof(IntervalMinAttribute))]
    sealed class IntervalMinDrawer : PropertyDrawer
    {
        static readonly string s_InvalidTypeMessage = L10n.Tr("Use IntervalMin with Interval.");
        IntervalMinAttribute MinAttribute => attribute as IntervalMinAttribute;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            EditorGUI.BeginChangeCheck();
            IntervalGUI.IntervalField(position, property, label);
            if (EditorGUI.EndChangeCheck())
            {
                SerializedProperty min = property.FindPropertyRelative("Min");
                SerializedProperty max = property.FindPropertyRelative("Max");
                if (min is { propertyType: SerializedPropertyType.Float } &&
                    max is { propertyType: SerializedPropertyType.Float })
                {
                    min.floatValue = Mathf.Max(MinAttribute.Min, min.floatValue);
                    max.floatValue = Mathf.Max(MinAttribute.Min, max.floatValue);
                }
                else if (min is { propertyType: SerializedPropertyType.Integer } &&
                         max is { propertyType: SerializedPropertyType.Integer })
                {
                    min.intValue = Mathf.Max((int)MinAttribute.Min, min.intValue);
                    max.intValue = Mathf.Max((int)MinAttribute.Min, max.intValue);
                }
                else
                {
                    EditorGUI.LabelField(position, label.text, s_InvalidTypeMessage);
                }
            }
            EditorGUI.EndProperty();
        }
    }

    [CustomPropertyDrawer(typeof(IntervalClampAttribute))]
    sealed class IntervalClampDrawer : PropertyDrawer
    {
        static readonly string s_InvalidTypeMessage = L10n.Tr("Use IntervalClamp with Interval.");
        IntervalClampAttribute ClampAttribute => attribute as IntervalClampAttribute;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            IntervalGUI.IntervalField(position, property, label);
            if (EditorGUI.EndChangeCheck())
            {
                SerializedProperty min = property.FindPropertyRelative("Min");
                SerializedProperty max = property.FindPropertyRelative("Max");

                if (min is { propertyType: SerializedPropertyType.Float } &&
                    max is { propertyType: SerializedPropertyType.Float })
                {
                    min.floatValue = Mathf.Max(ClampAttribute.Min, min.floatValue);
                    min.floatValue = Mathf.Min(ClampAttribute.Max, min.floatValue);

                    max.floatValue = Mathf.Max(ClampAttribute.Min, max.floatValue);
                    max.floatValue = Mathf.Min(ClampAttribute.Max, max.floatValue);
                }
                else if (min is { propertyType: SerializedPropertyType.Integer } &&
                         max is { propertyType: SerializedPropertyType.Integer })
                {
                    min.intValue = Mathf.Max((int)ClampAttribute.Min, min.intValue);
                    min.intValue = Mathf.Min((int)ClampAttribute.Max, min.intValue);

                    max.intValue = Mathf.Max((int)ClampAttribute.Min, max.intValue);
                    max.intValue = Mathf.Min((int)ClampAttribute.Max, max.intValue);
                }
                else
                {
                    EditorGUI.LabelField(position, label.text, s_InvalidTypeMessage);
                }
            }
            EditorGUI.EndProperty();
        }
    }

    [CustomPropertyDrawer(typeof(IntervalRangeAttribute))]
    sealed class IntervalRangeDrawer : PropertyDrawer
    {
        static readonly string s_InvalidTypeMessage = L10n.Tr("Use IntervalRange with a float Interval.");
        IntervalRangeAttribute RangeAttribute => attribute as IntervalRangeAttribute;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            
            SerializedProperty min = property.FindPropertyRelative("Min");
            SerializedProperty max = property.FindPropertyRelative("Max");
            
            if (min is { propertyType: SerializedPropertyType.Float } &&
                max is { propertyType: SerializedPropertyType.Float })
            {
                float minValue = min.floatValue;
                float maxValue = max.floatValue;
                EditorGUI.MinMaxSlider(position, label, ref minValue, ref maxValue, RangeAttribute.Min, RangeAttribute.Max);
                min.floatValue = minValue;
                max.floatValue = maxValue;
            }
            else
            {
                EditorGUI.LabelField(position, label.text, s_InvalidTypeMessage);
            }
            
            EditorGUI.EndProperty();
        }
    }

    [CustomPropertyDrawer(typeof(Interval))]
    public class IntervalPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            IntervalGUI.IntervalField(position, property, label);
            EditorGUI.EndProperty();
        }
    }

    public static class IntervalGUI
    {
        static readonly string s_InvalidTypeMessage = L10n.Tr("Property is not an Interval type.");

        static readonly GUIContent[] s_MinMaxLabels = new GUIContent[2]
        {
            EditorGUIUtility.TrTextContent("Min", "Min"),
            EditorGUIUtility.TrTextContent("Max", "Max")
        };

        public static void IntervalField(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty min = property.FindPropertyRelative("Min");
            SerializedProperty max = property.FindPropertyRelative("Max");

            if (min is { propertyType: SerializedPropertyType.Float } &&
                max is { propertyType: SerializedPropertyType.Float })
            {
                EditorGUI.MultiPropertyField(position, s_MinMaxLabels, min, label);
            }
            else if (min is { propertyType: SerializedPropertyType.Integer } &&
                     max is { propertyType: SerializedPropertyType.Integer })
            {
                EditorGUI.MultiPropertyField(position, s_MinMaxLabels, min, label);
            }
            else
            {
                EditorGUI.LabelField(position, label.text, s_InvalidTypeMessage);
            }
        }
    }
}
