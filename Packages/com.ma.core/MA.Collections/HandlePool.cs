// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;

namespace MA.Collections
{
    /// <summary>Manages a pool of versioned handles, allowing for efficient allocation, deallocation, and reuse of indices.</summary>
    /// <remarks>This structure is particularly useful for managing resources that need stable, reusable identifiers.</remarks>
    /// <typeparam name="TExternalIndex">The type of the index used in the handles, must be unmanaged.</typeparam>
    [DebuggerTypeProxy(typeof(HandlePoolDebugView<>))]
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct HandlePool<TExternalIndex> : IDisposable 
        where TExternalIndex : unmanaged
    {
        [NativeDisableUnsafePtrRestriction] internal TExternalIndex* m_ExternalIndices;
        [NativeDisableUnsafePtrRestriction] internal int* m_NextFreeIndices;
        [NativeDisableUnsafePtrRestriction] internal int* m_Versions;
        internal int m_Capacity;
        internal int m_NextFreeIndex;
        internal int m_AllocatedCount;
        internal readonly AllocatorManager.AllocatorHandle m_AllocatorHandle;
        
        /// <summary>Get the number of handles in the pool.</summary>
        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Capacity;
        }
        
        /// <summary>Get the maximum allowed index of the pool.</summary>
        /// <remarks>This index can never decrease, any index below or equal to this value could be valid.</remarks>
        public int MaxIndex
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Capacity - 1;
        }

        /// <summary>Get the number of allocated handles.</summary>
        public int AllocatedCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_AllocatedCount;
        }

        /// <summary>Get the number of free handles.</summary>
        public int FreeCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Capacity - m_AllocatedCount;
        }

        /// <summary>True if the pool is created.</summary>
        public bool IsCreated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_NextFreeIndices != null;
        }

        /// <summary>Initializes a new instance of the <see cref="HandlePool{TExternalIndex}"/> struct.</summary>
        /// <param name="capacity">Initial capacity of the pool.</param>
        /// <param name="allocator">The allocator to use for memory management.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HandlePool(int capacity, AllocatorManager.AllocatorHandle allocator)
        {
            m_Capacity = 0;
            m_AllocatorHandle = allocator;
            m_ExternalIndices = null;
            m_NextFreeIndices = null;
            m_Versions = null;
            m_NextFreeIndex = 0;
            m_AllocatedCount = 0;
            Reserve(math.max(capacity, 8));
        }
        
        /// <summary>Releases all resources used by the <see cref="HandlePool{TExternalIndex}"/>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (m_ExternalIndices == null)
                return;
            
            AllocatorManager.Free(m_AllocatorHandle, m_ExternalIndices);
            m_ExternalIndices = null;
            m_NextFreeIndices = null;
            m_Versions = null;
            m_NextFreeIndex = 0;
            m_AllocatedCount = 0;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IncreaseCapacity() => Reserve(m_Capacity * 2);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void InitializeHandles(int startIndex)
        {
            for (int i = startIndex; i < m_Capacity; i++)
            {
                m_NextFreeIndices[i] = i + 1;
                m_ExternalIndices[i] = default;
                m_Versions[i] = 0;
            }
            
            m_NextFreeIndices[m_Capacity - 1] = -1;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void AllocateHandles(TExternalIndex* indices, int count, Handle* outHandles)
        {
            for (int i = 0; i < count; i++)
            {
                int nextFreeIndex = m_NextFreeIndices[m_NextFreeIndex];
                if (nextFreeIndex == -1)
                {
                    IncreaseCapacity();
                    nextFreeIndex = m_NextFreeIndices[m_NextFreeIndex];
                }
                
                m_ExternalIndices[m_NextFreeIndex] = indices[i];
                
                int handleVersion = m_Versions[m_NextFreeIndex];
                if (handleVersion < 1)
                    m_Versions[m_NextFreeIndex] = handleVersion = 1;
                
                Handle* handle = outHandles + i;
                handle->Index = m_NextFreeIndex;
                handle->Version = handleVersion;
                
                m_NextFreeIndices[m_NextFreeIndex] = nextFreeIndex;
                m_NextFreeIndex = nextFreeIndex;
                
                m_AllocatedCount++;
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void DeallocateHandles(Handle* handles, int count)
        {
            int freeIndex = m_NextFreeIndex;
            
            for (int i = 0; i < count; i++)
            {
                int handleIndex = handles[i].Index;
                if (m_Versions[handleIndex] != handles[i].Version)
                    continue;
                
                m_Versions[handleIndex]++;
                m_ExternalIndices[handleIndex] = default;
                m_NextFreeIndices[handleIndex] = freeIndex;
                freeIndex = handleIndex;
                m_AllocatedCount--;
            }
            
            m_NextFreeIndex = freeIndex;
        }

        /// <summary>Reserves a minimum capacity for the pool, expanding it if necessary.</summary>
        /// <remarks>Capacity can only be increased, not decreased.</remarks>
        /// <param name="newCapacity">The minimum capacity to ensure.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reserve(int newCapacity)
        {
            if (newCapacity <= m_Capacity)
                return;
            
            int oldCapacity = m_Capacity;
            m_Capacity = newCapacity;
            
            void* oldBlock = m_ExternalIndices;
            TExternalIndex* oldExternalIndices = m_ExternalIndices;
            int* oldNextFreeIndices = m_NextFreeIndices;
            int* oldVersions = m_Versions;
                
            int newTotalSize = m_Capacity * sizeof(TExternalIndex) + m_Capacity * sizeof(int) + m_Capacity * sizeof(int);
            void* newBlock = AllocatorManager.Allocate(m_AllocatorHandle, newTotalSize, JobsUtility.CacheLineSize);
            
            m_ExternalIndices = (TExternalIndex*)newBlock;
            m_NextFreeIndices = (int*)(m_ExternalIndices + m_Capacity);
            m_Versions = m_NextFreeIndices + m_Capacity;
                
            if (oldCapacity > 0)
            {
                UnsafeUtility.MemCpy(m_ExternalIndices, oldExternalIndices, oldCapacity * sizeof(TExternalIndex));
                UnsafeUtility.MemCpy(m_NextFreeIndices, oldNextFreeIndices, oldCapacity * sizeof(int));
                UnsafeUtility.MemCpy(m_Versions, oldVersions, oldCapacity * sizeof(int));
                AllocatorManager.Free(m_AllocatorHandle, oldBlock);
            }
            
            InitializeHandles(math.max(0, oldCapacity - 1));
        }
        
        /// <summary>Reset the pool, invalidating all handles.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            InitializeHandles(0);
            m_NextFreeIndex = 0;
            m_AllocatedCount = 0;
        }

        /// <summary>Get the handle at an index.</summary>
        /// <param name="handleIndex">The index of the handle.</param>
        /// <returns>The handle at the index.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Handle GetHandleAt(int handleIndex)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (handleIndex < 0 || handleIndex >= m_Capacity)
                throw new ArgumentOutOfRangeException(nameof(handleIndex), "Handle index is out of bounds.");
#endif
            
            return new Handle(handleIndex, m_Versions[handleIndex]);
        }
        
        /// <summary>Check if an handle is valid.</summary>
        /// <param name="handle">The handle to check.</param>
        /// <returns>True if the handle is valid.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Exists(Handle handle)
        {
            return handle is { IsCreated: true, Index: >= 0 } && 
                   handle.Index < m_Capacity && 
                   m_Versions[handle.Index] == handle.Version;
        }
        
        /// <summary>Allocates a new handle and associates it with the given data.</summary>
        /// <param name="index">The index to associate with the newly allocated handle.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Handle Allocate(TExternalIndex index)
        {
            Handle handle = default;
            AllocateHandles(&index, 1, &handle);
            return handle;
        }
        
        /// <summary>Allocates a set of new handles and associates them with the given indices.</summary>
        /// <param name="indices">The indices to associate with the newly allocated handles.</param>
        /// <param name="allocator">The allocator to use for memory management.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray<Handle> Allocate(ReadOnlySpan<TExternalIndex> indices, Allocator allocator)
        {
            NativeArray<Handle> handles = new NativeArray<Handle>(indices.Length, allocator);
            fixed (TExternalIndex* indicesPtr = indices)
                AllocateHandles(indicesPtr, indices.Length, (Handle*)handles.GetUnsafePtr());
            return handles;
        }
        
        /// <summary>Allocates a set of new handles and associates them with the given indices.</summary>
        /// <param name="indices">The indices to associate with the newly allocated handles.</param>
        /// <param name="outHandles">The output handles to be allocated.</param>
        /// <exception cref="ArgumentException">Thrown when the length of the handles and indices do not match.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(ReadOnlySpan<TExternalIndex> indices, Span<Handle> outHandles)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (outHandles.Length != indices.Length)
                throw new ArgumentException("Handles and indices must have the same length.");
#endif
            
            fixed (TExternalIndex* indicesPtr = indices)
            fixed (Handle* handlesPtr = outHandles)
                AllocateHandles(indicesPtr, indices.Length, handlesPtr);
        }

        /// <summary>Allocates a set of new handles and associates them with the given indices.</summary>
        /// <param name="indices">A pointer to the indices to associate with the newly allocated handles.</param>
        /// <param name="outHandles">A pointer to the output handles to be allocated.</param>
        /// <param name="length">The length of the indices and handles.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(TExternalIndex* indices, Handle* outHandles, int length)
        {
            AllocateHandles(indices, length, outHandles);
        }
        
        /// <summary>Destroy a handle.</summary>
        /// <param name="handle">The handle to destroy.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Free(Handle handle)
        {
            DeallocateHandles(&handle, 1);
        }

        /// <summary>Destroy multiple handles.</summary>
        /// <param name="handles">The handles to destroy.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Free(ReadOnlySpan<Handle> handles)
        {
            fixed (Handle* handlesPtr = handles)
                DeallocateHandles(handlesPtr, handles.Length);
        }

        /// <summary>Destroy multiple handles.</summary>
        /// <param name="ptr">The pointer to the handles to destroy.</param>
        /// <param name="length">The number of handles to destroy.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Free(Handle* ptr, int length) => DeallocateHandles(ptr, length);
        
        /// <summary>Get the array index of a handle.</summary>
        /// <param name="handle">The handle to get the array index of.</param>
        /// <returns>The array index of the handle.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TExternalIndex GetIndex(Handle handle)
        {
            EnsureHandleExists(handle);
            return m_ExternalIndices[handle.Index];
        }

        /// <summary>Try to get the array index of an handle.</summary>
        /// <param name="handle">The handle to get the array index of.</param>
        /// <param name="index">The index to get from the handle.</param>
        /// <returns>True if the handle is valid.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetIndex(Handle handle, out TExternalIndex index)
        {
            if (Exists(handle))
            {
                index = m_ExternalIndices[handle.Index];
                return true;
            }
            else
            {
                index = default;
                return false;
            }
        }
        
        /// <summary>Update the array index of an handle.</summary>
        /// <param name="handle">The handle to update.</param>
        /// <param name="newIndex">The new index to set.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateIndex(Handle handle, in TExternalIndex newIndex)
        {
            EnsureHandleExists(handle);
            m_ExternalIndices[handle.Index] = newIndex;
        }
        
        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        void EnsureHandleExists(Handle handle)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (!Exists(handle))
                throw new InvalidOperationException($"{typeof(HandlePool<TExternalIndex>).Name}: Handle: {handle} does not exist.");
#endif
        }
    }

    sealed class HandlePoolDebugView<TIndex>
        where TIndex : unmanaged
    {
        HandlePool<TIndex> m_Pool;

        public HandlePoolDebugView(HandlePool<TIndex> pool) 
            => m_Pool = pool;
        
        public int Capacity 
            => m_Pool.Capacity;
        
        public unsafe TIndex[] Indices
        {
            get
            {
                TIndex[] externalIndices = new TIndex[m_Pool.Capacity];
                fixed (TIndex* externalIndicesPtr = externalIndices)
                    UnsafeUtility.MemCpy(externalIndicesPtr, m_Pool.m_ExternalIndices, m_Pool.Capacity * sizeof(TIndex));
                return externalIndices;
            }
        }

        public unsafe int[] FreeIndices
        {
            get
            {
                int[] freeIndices = new int[m_Pool.Capacity];
                fixed (int* freeIndicesPtr = freeIndices)
                    UnsafeUtility.MemCpy(freeIndicesPtr, m_Pool.m_NextFreeIndices, m_Pool.Capacity * sizeof(int));
                return freeIndices;
            }
        }
        
        public unsafe int[] Versions
        {
            get
            {
                int[] versions = new int[m_Pool.Capacity];
                fixed (int* versionsPtr = versions)
                    UnsafeUtility.MemCpy(versionsPtr, m_Pool.m_Versions, m_Pool.Capacity * sizeof(int));
                return versions;
            }
        }
    }
}