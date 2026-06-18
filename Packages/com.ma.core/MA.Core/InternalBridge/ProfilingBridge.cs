// Copyright © Magnetic Arcade. All Rights Reserved.

using System.Runtime.CompilerServices;
using Unity.Profiling;
using Unity.Profiling.LowLevel.Unsafe;

namespace MA.Core.Bridge
{
    static class ProfilingBridge
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetName(in ProfilerMarker marker)
        {
            string name = ""; 
            marker.GetName(ref name);
            return name;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ProfilerCategory GetAnyCategory() => ProfilerCategory.Any;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ProfilerCategory GetGPUCategory() => ProfilerCategory.GPU;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ProfilerRecorderHandle GetRecorderHandle(ProfilerMarker marker) => ProfilerRecorderHandle.Get(marker);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ProfilerRecorderHandle GetRecorderHandle(ProfilerCategory category, string name) => ProfilerRecorderHandle.Get(category, name);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong GetHandle(in ProfilerRecorder recorder) => recorder.handle;
    }
}