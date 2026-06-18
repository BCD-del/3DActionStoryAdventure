// Copyright © Magnetic Arcade. All Rights Reserved.

using System.Runtime.CompilerServices;
using MA.UIElements.Editor.Bridge;
using UnityEditor.Overlays;
using UnityEngine;

namespace MA.UIElements.Editor
{
    public static class ToolbarOverlayExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IssueRebuild(this ToolbarOverlay overlay) 
            => ToolbarOverlayBridge.IssueRebuild(overlay);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Texture2D GetCollapsedIcon(this ToolbarOverlay overlay)
            => ToolbarOverlayBridge.GetCollapsedIcon(overlay);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetCollapsedIcon(this ToolbarOverlay overlay, Texture2D value)
            => ToolbarOverlayBridge.SetCollapsedIcon(overlay, value);
    }
}