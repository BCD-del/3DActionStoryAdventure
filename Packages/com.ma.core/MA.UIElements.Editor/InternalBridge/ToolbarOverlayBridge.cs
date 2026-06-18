// Copyright © Magnetic Arcade. All Rights Reserved.

using System.Runtime.CompilerServices;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;

namespace MA.UIElements.Editor.Bridge
{
    public static class ToolbarOverlayBridge
    {
        const string k_CollapsedIconButton = "unity-overlay-collapsed-dropdown__icon";
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IssueRebuild(ToolbarOverlay overlay) 
            => overlay.RebuildContent();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Texture2D GetCollapsedIcon(ToolbarOverlay overlay)
        {
            _ = overlay.collapsedButtonRect; // This just forces the collapsedContent to be created                                                                                                            
            var label = overlay.rootVisualElement.Q<Label>(null, new[] { k_CollapsedIconButton });
            return label?.style.backgroundImage.value.texture;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetCollapsedIcon(ToolbarOverlay overlay, Texture2D value)
        {
            if (value != null)
            {
                _ = overlay.collapsedButtonRect; // This just forces the collapsedContent to be created
                var label = overlay.rootVisualElement.Q<Label>(null, new[] { k_CollapsedIconButton });
                if (label != null)
                {
                    label.text = null;
                    label.style.backgroundImage = value;
                }
            }
        }
    }
}
