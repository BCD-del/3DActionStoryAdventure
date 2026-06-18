// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine.Assertions;

namespace MA.Core.Editor
{
    class SerializedOverridableValue
    {
        public SerializedProperty BaseProperty { get; private set; }
        public SerializedProperty OverrideState { get; private set; }
        public SerializedProperty Value { get; private set; }
        public Attribute[] Attributes { get; private set; }
        public Type ReferenceType { get; private set; }

        readonly object m_ReferenceValue;

        public SerializedOverridableValue(SerializedProperty property)
        {
            BaseProperty = property;
            OverrideState = property.FindPropertyRelative("OverrideState");
            Value = property.FindPropertyRelative("Value");
            // Find the actual property type, optional attributes & reference
            var path = property.propertyPath.Split('.');
            object obj = property.serializedObject.targetObject;
            FieldInfo field = null;

            foreach (var p in path)
            {
                field = obj.GetType().GetField(p, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;
                obj = field.GetValue(obj);
            }

            Assert.IsNotNull(field);

            BaseProperty = property.Copy();
            OverrideState = BaseProperty.FindPropertyRelative("OverrideState");
            Value = BaseProperty.FindPropertyRelative("Value");
            Attributes = field.GetCustomAttributes(false).Cast<Attribute>().ToArray();
            ReferenceType = obj.GetType();
            m_ReferenceValue = obj;
        }

        /// <summary>Gets and casts an attribute applied on the base <see cref="OverridableValue{T}"/>.</summary>
        public T GetAttribute<T>() where T : Attribute => (T)Attributes.FirstOrDefault(x => x is T);

        /// <summary>Gets and casts the underlying reference of type <typeparamref name="T"/>.</summary>
        public T GetObjectRef<T>() => (T)m_ReferenceValue;
    }
}
