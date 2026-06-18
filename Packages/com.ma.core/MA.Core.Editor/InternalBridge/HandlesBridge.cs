// Copyright © Magnetic Arcade. All Rights Reserved.

using UnityEditor;
using UnityEngine;

namespace MA.Core.Editor.Bridge
{
    static class HandlesBridge
    {
        internal static bool IsSceneCameraFiltered(this Camera camera)
            => Handles.GetCameraFilterMode(camera) == Handles.CameraFilterMode.ShowFiltered;
        
        internal static void ShowSceneViewLabel(Vector3 position, GUIContent content)
            => Handles.Label(position, content);
        
        internal static void LockHandlePosition()
            => Tools.LockHandlePosition();
        
        internal static void LockHandlePosition(Vector3 position)
            => Tools.LockHandlePosition(position);
        
        internal static void UnlockHandlePosition()
            => Tools.UnlockHandlePosition();
    }
}