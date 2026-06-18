// Copyright © Magnetic Arcade. All Rights Reserved.

using MA.Core.Bridge;
using UnityEngine;
using UnityEngine.UIElements;

namespace MA.Core.Editor
{
    static class VisualElementEditorExtensions
    {
        public static void SetIsCompositeRoot(this VisualElement element, bool value) 
            => VisualElementEditorBridge.SetIsCompositeRoot(element, value);
        
        public static Rect GetBoundingBox(this VisualElement element)
            => VisualElementEditorBridge.GetBoundingBox(element);
    }
}