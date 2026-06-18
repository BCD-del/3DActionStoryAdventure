// Copyright © Magnetic Arcade. All Rights Reserved.

using UnityEditor;
using UnityEngine;

namespace MA.Core.Editor.Bridge
{
    static class SceneViewBridge
    {
        internal static Rect GetCameraViewport(this SceneView sceneView) 
            => sceneView.cameraViewport;
    }
}