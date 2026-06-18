// Copyright © Magnetic Arcade. All Rights Reserved.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

#if HAS_PACKAGE_UNITY_COLLECTIONS_2_0_0
using RewindableAllocator = Unity.Collections.RewindableAllocator;
#else
using RewindableAllocator = MA.Collections.Internal.RewindableAllocator;
#endif

namespace MA.Collections
{
    /// <summary>A thread local allocator that can be rewound.</summary>
    public unsafe struct ThreadLocalAllocator
    {
        /// <summary>The initial size of each allocator.</summary>
        public const int InitialSize = 1024 * 1024;

        /// <summary>The allocator used for the thread local allocators.</summary>
        public const Allocator Allocator = Unity.Collections.Allocator.Persistent;

        /// <summary>The number of threads that can be used.</summary>
        public const int NumThreads = JobsUtility.MaxJobThreadCount;

        /// <summary>A padded allocator that can be used in a thread local allocator.</summary>
        [StructLayout(LayoutKind.Explicit, Size = JobsUtility.CacheLineSize)]
        public struct PaddedAllocator
        {
            /// <summary>The allocator.</summary>
            [FieldOffset(0)]
            public AllocatorHelper<RewindableAllocator> Allocator;

            /// <summary>Whether the allocator has been used since the last rewind.</summary>
            [FieldOffset(16)]
            public bool UsedSinceRewind;

            /// <summary>Initializes the allocator.</summary>
            public void Initialize(int initialSize)
            {
                Allocator = new AllocatorHelper<RewindableAllocator>(AllocatorManager.Persistent);
                Allocator.Allocator.Initialize(initialSize);
            }
        }

        /// <summary>The list of allocators.</summary>
        public UnsafeList<PaddedAllocator> Allocators;

        /// <summary>Creates a new thread local allocator.</summary>
        public ThreadLocalAllocator(int expectedUsedCount = -1, int initialSize = InitialSize)
        {
            // Note, the comparison is <= as on 32-bit builds this size will be smaller, which is fine.
            Debug.Assert(sizeof(AllocatorHelper<RewindableAllocator>) <= 16,
                         $"PaddedAllocator's Allocator size has changed. The type layout needs adjusting.");
            Debug.Assert(sizeof(PaddedAllocator) >= JobsUtility.CacheLineSize,
                         $"Thread local allocators should be on different cache lines. Size: {sizeof(PaddedAllocator)}, Cache Line: {JobsUtility.CacheLineSize}");

            if (expectedUsedCount < 0)
                expectedUsedCount = math.max(0, JobsUtility.JobWorkerCount + 1);

            Allocators = new UnsafeList<PaddedAllocator>(NumThreads, Allocator, NativeArrayOptions.ClearMemory);
            Allocators.Resize(NumThreads);

            for (int i = 0; i < NumThreads; ++i)
            {
                Allocators.ElementAt(i).Initialize(i < expectedUsedCount ? initialSize : 1);
            }
        }

        /// <summary>Rewinds the allocators.</summary>
        public void Rewind()
        {
            // Profiler.BeginSample("RewindAllocators");
            for (int i = 0; i < NumThreads; ++i)
            {
                ref PaddedAllocator allocator = ref Allocators.ElementAt(i);
                if (allocator.UsedSinceRewind)
                {
                    // Profiler.BeginSample("Rewind");
                    Allocators.ElementAt(i).Allocator.Allocator.Rewind();
                    // Profiler.EndSample();
                }
                allocator.UsedSinceRewind = false;
            }
            // Profiler.EndSample();
        }

        /// <summary>Disposes the allocators.</summary>
        public void Dispose()
        {
            for (int i = 0; i < NumThreads; ++i)
            {
                Allocators.ElementAt(i).Allocator.Allocator.Dispose();
                Allocators.ElementAt(i).Allocator.Dispose();
            }

            Allocators.Dispose();
        }

        /// <summary>Gets the allocator for the given thread.</summary>
        public RewindableAllocator* ThreadAllocator(int threadIndex)
        {
            ref PaddedAllocator allocator = ref Allocators.ElementAt(threadIndex);
            allocator.UsedSinceRewind = true;
            return (RewindableAllocator*)UnsafeUtility.AddressOf(ref allocator.Allocator.Allocator);
        }

        /// <summary>The allocator for the main thread.</summary>
        public RewindableAllocator* GeneralAllocator
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ThreadAllocator(Allocators.Length - 1);
        }
    }
}
