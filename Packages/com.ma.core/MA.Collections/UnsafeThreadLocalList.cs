// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs.LowLevel.Unsafe;

namespace MA.Collections.Unsafe
{
    /// <summary>An unmanaged, resizable list.</summary>
    /// <remarks>This a <see cref="NativeList{T}"/> without the safety handle.</remarks>
    [StructLayout(LayoutKind.Sequential)]
    [DebuggerTypeProxy(typeof(UnsafeIndirectListDebugView<>))]
#if HAS_PACKAGE_UNITY_COLLECTIONS_2_0_0
    [GenerateTestsForBurstCompatibility(GenericTypeArguments = new[] { typeof(int) })]
#else
    [BurstCompatible]
#endif
    public unsafe struct UnsafeThreadLocalList<T> : IDisposable
        where T : unmanaged
    {
        /// <summary>The number of threads that can be used.</summary>
        public const int NumThreads = JobsUtility.MaxJobThreadCount;
        
        /// <summary>The list of thread local lists.</summary>
        public UnsafeList<UnsafeIndirectList<T>> Lists;
        
        /// <summary>The number of elements in the list.</summary>
        public int Length => Lists.Length;
        
        /// <summary>Create a new thread local list.</summary>
        /// <param name="initialCapacity">The initial capacity of each list.</param>
        /// <param name="allocator">The allocator to use for the lists.</param>
        public UnsafeThreadLocalList(int initialCapacity, AllocatorManager.AllocatorHandle allocator)
        {
            Lists = new UnsafeList<UnsafeIndirectList<T>>(NumThreads, allocator);
            Lists.Resize(NumThreads);

            for (int i = 0; i < NumThreads; ++i)
            {
                Lists[i] = new UnsafeIndirectList<T>(initialCapacity, allocator);
            }
        }
        
        /// <summary>Clears all lists.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            for (int i = 0; i < NumThreads; ++i)
                Lists[i].Clear();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeIndirectList<T> GetThreadLocalList(int threadIndex) => Lists[threadIndex];

        /// <summary>Returns a reference to the thread local list for the given thread index.</summary>
        /// <param name="threadIndex">The thread index.</param>
        public ref UnsafeIndirectList<T> this[int threadIndex]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Lists.Ptr[threadIndex];
        }

        /// <summary>Disposes all thread local lists.</summary>
        public void Dispose()
        {
            for (int i = 0; i < Lists.Length; i++)
                Lists[i].Dispose();
            
            Lists.Dispose();
        }
    }
}