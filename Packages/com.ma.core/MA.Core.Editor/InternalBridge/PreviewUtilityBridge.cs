// Copyright © Magnetic Arcade. All Rights Reserved.

using UnityEditor;
using UnityEngine;

namespace MA.Core.Editor.Bridge
{
    static class PreviewRenderUtilityBridge
    {
        internal static Vector2 Drag2D(Vector2 scrollPosition, Rect position) 
            => PreviewGUI.Drag2D(scrollPosition, position);
        
        internal static void DrawPreview(Rect r, Texture texture)
            => PreviewRenderUtility.DrawPreview(r, texture);
        
        internal static RenderTexture GetRenderTexture(this PreviewRenderUtility previewRenderUtility)
            => previewRenderUtility.renderTexture;
        
        internal static void AddManagedGameObject(this PreviewRenderUtility previewRenderUtility, GameObject go) 
            => previewRenderUtility.AddManagedGO(go);
    }
}