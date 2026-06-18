// Copyright © Magnetic Arcade. All Rights Reserved.

using System.Runtime.CompilerServices;
using MA.Core.Bridge;
using Unity.Profiling;
using Unity.Profiling.LowLevel.Unsafe;

namespace MA.Core
{
    public static class ProfilingUtility
    {
        /// <summary>Gets the GPU category.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ProfilerCategory GetAnyCategory() => ProfilingBridge.GetAnyCategory();
        
        /// <summary>Gets the GPU category.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ProfilerCategory GetGPUCategory() => ProfilingBridge.GetGPUCategory();
        
        /// <summary>Gets the recorder handle of the specified marker.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ProfilerRecorderHandle GetRecorderHandle(ProfilerMarker marker) => ProfilingBridge.GetRecorderHandle(marker);
        
        /// <summary>Gets the recorder handle of the specified category and name.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ProfilerRecorderHandle GetRecorderHandle(ProfilerCategory category, string name) => ProfilingBridge.GetRecorderHandle(category, name);
        
        /// <summary>Starts a GPU recorder with the specified marker.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ProfilerRecorder StartGPURecorder(ProfilerMarker marker)
        {
            ProfilerRecorderHandle handle = GetRecorderHandle(GetAnyCategory(), GetName(marker));
            return new ProfilerRecorder(handle, options: (ProfilerRecorderOptions)217);
        }

        /// <summary>Gets the handle of the recorder.</summary>
        /// <param name="recorder">The recorder.</param>
        /// <returns>The handle of the recorder.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong GetHandle(this in ProfilerRecorder recorder) => ProfilingBridge.GetHandle(recorder);

        /// <summary>Gets the name of the marker.</summary>
        /// <param name="marker">The marker.</param>
        /// <returns>The name of the marker.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetName(this in ProfilerMarker marker) => ProfilingBridge.GetName(marker);
    }
}
