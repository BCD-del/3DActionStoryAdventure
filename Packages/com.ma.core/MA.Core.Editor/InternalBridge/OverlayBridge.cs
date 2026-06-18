// Copyright © Magnetic Arcade. All Rights Reserved.

using UnityEditor.Overlays;
using UnityEngine.UIElements;

namespace MA.Core.Editor.InternalBridge
{
    static class OverlayBridge
    {
        internal static VisualElement GetRootVisualElement(this Overlay overlay)
            => overlay.rootVisualElement;

        internal static bool IsCollapsed(this Overlay overlay)
            => overlay.collapsed;
    }
}
