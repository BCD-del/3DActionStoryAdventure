// Copyright © Magnetic Arcade. All Rights Reserved.

using Unity.Profiling;
using UnityEngine.Rendering;

namespace MA.Core
{
    public static class CommandBufferExtensions
    {
#if !UNITY_2022_3_OR_NEWER
        public static void BeginSample(this CommandBuffer commandBuffer, ProfilerMarker marker) 
            => commandBuffer.BeginSample(marker.GetName());
        
        public static void EndSample(this CommandBuffer commandBuffer, ProfilerMarker marker) 
            => commandBuffer.BeginSample(marker.GetName());
#endif
    }
}