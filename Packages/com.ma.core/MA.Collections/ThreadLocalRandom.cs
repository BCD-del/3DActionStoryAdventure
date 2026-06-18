// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using MA.Mathematics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Random = Unity.Mathematics.Random;

namespace MA.Collections
{
    /// <summary>Represents a thread-local random generator.</summary>
    public struct ThreadLocalRandom : IDisposable
    {
        /// <summary>The number of threads that can be used.</summary>
        public const int NumThreads = JobsUtility.MaxJobThreadCount;

        /// <summary>The thread-local randoms.</summary>
        NativeArray<Random> m_ThreadLocalRandoms;
        
        /// <summary>The index of the current thread.</summary>
        [NativeSetThreadIndex] int m_ThreadIndex;

        /// <summary>Initializes a new instance of the <see cref="ThreadLocalRandom"/> struct.</summary>
        /// <param name="allocator">The allocator used for memory allocation.</param>
        public ThreadLocalRandom(Allocator allocator)
        {
            m_ThreadLocalRandoms = new NativeArray<Random>(NumThreads, allocator);
            m_ThreadIndex = -1;
        }

        /// <summary>Disposes of the resources used by the ThreadLocalRandom instance.</summary>
        public void Dispose() => m_ThreadLocalRandoms.Dispose();

        /// <summary>Schedules the disposal of the resources used by the ThreadLocalRandom instance.</summary>
        /// <param name="inputDeps">The job handle that must complete before the resources are disposed.</param>
        /// <returns>The job handle that represents the disposal of the resources.</returns>
        public JobHandle Dispose(JobHandle inputDeps) => m_ThreadLocalRandoms.Dispose(inputDeps);

        /// <summary>Creates a new instance of <see cref="ThreadLocalRandom"/> with a specified seed.</summary>
        /// <param name="seed">The seed value for the random generator.</param>
        /// <param name="allocator">The allocator used for memory allocation.</param>
        /// <returns>A new <see cref="ThreadLocalRandom"/> instance.</returns>
        public static ThreadLocalRandom Create(uint seed, Allocator allocator)
        {
            ThreadLocalRandom threadLocalRandom = new ThreadLocalRandom(allocator);
            threadLocalRandom.Reset(seed);
            return threadLocalRandom;
        }

        /// <summary>Creates a new instance of <see cref="ThreadLocalRandom"/> using an existing <see cref="Random"/> instance.</summary>
        /// <param name="random">A reference to the existing Random instance.</param>
        /// <param name="allocator">The allocator used for memory allocation.</param>
        /// <returns>A new <see cref="ThreadLocalRandom"/> instance.</returns>
        public static ThreadLocalRandom Create(ref Random random, Allocator allocator)
        {
            ThreadLocalRandom threadLocalRandom = new ThreadLocalRandom(allocator);
            threadLocalRandom.Reset(ref random);
            return threadLocalRandom;
        }

        /// <summary>Gets the current Random instance associated with the current thread.</summary>
        public unsafe ref Random Current => ref UnsafeUtility.AsRef<Random>(m_ThreadLocalRandoms.GetUnsafePtrAt(m_ThreadIndex));

        /// <summary>Initializes the thread-local randoms using a specified seed.</summary>
        /// <param name="seed">The seed value for the random generator.</param>
        public void Reset(uint seed)
        {
            for (int i = 0; i < NumThreads; i++)
                m_ThreadLocalRandoms[i] = Random.CreateFromIndex(seed + (uint)i);
        }

        /// <summary>Initializes the thread-local randoms using an existing <see cref="Random"/> instance.</summary>
        /// <param name="random">A reference to the existing Random instance.</param>
        public void Reset(ref Random random)
        {
            for (int i = 0; i < NumThreads; i++)
                m_ThreadLocalRandoms[i] = random.NextRandom();
        }
    }
}