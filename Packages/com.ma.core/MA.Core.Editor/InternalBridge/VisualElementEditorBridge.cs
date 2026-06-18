// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MA.Core.Bridge
{
    static class VisualElementEditorBridge
    {
        delegate void SetIsCompositeRootDelegate(VisualElement element, bool value);
        static SetIsCompositeRootDelegate s_SetIsCompositeRoot;
        
        delegate Rect GetBoundingBoxDelegate(VisualElement element);
        static GetBoundingBoxDelegate s_GetBoundingBox;
        
        [InitializeOnLoadMethod]
        static void Initialize()
        {
            s_SetIsCompositeRoot ??= (SetIsCompositeRootDelegate)Delegate.CreateDelegate(typeof(SetIsCompositeRootDelegate),
                typeof(VisualElement).GetProperty("isCompositeRoot", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                    .SetMethod);
            
            s_GetBoundingBox ??= (GetBoundingBoxDelegate)Delegate.CreateDelegate(typeof(GetBoundingBoxDelegate),
                typeof(VisualElement).GetProperty("boundingBox", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                    .GetMethod);
        }
        
        public static void SetIsCompositeRoot(VisualElement element, bool value)
            => s_SetIsCompositeRoot(element, value);
        
        public static Rect GetBoundingBox(VisualElement element)
            => s_GetBoundingBox(element);
    }
}