// Copyright © Magnetic Arcade. All Rights Reserved.

using UnityEngine;

namespace MA.Core.Bridge
{
    static class CameraInternalBridge
    {
        public static ulong GetSceneCullingMask(this Camera camera)
            => camera.sceneCullingMask;
    }
}