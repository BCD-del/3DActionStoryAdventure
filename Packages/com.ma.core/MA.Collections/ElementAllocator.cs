// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine.Assertions;

namespace MA.Collections
{
    /// <summary>Manages efficient element allocation and deallocation within a contiguous block.</summary>
    /// <remarks>
    ///     Optimized for the case where all elements are of the same type.
    ///     Defers deallocation merging to a manual call to <see cref="MergeFree"/>.
    ///     Best suited for elements of the same type, that need deallocation deferred to a later time.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ElementAllocator : IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        struct Block : IComparable<Block>
        {
            public int Offset;
            public int Length;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int CompareTo(Block other) => Offset.CompareTo(other.Offset);
        }

        int m_AllocatedSize;
        int m_MaxAllocatedSize;
        int m_HighestAllocatedSize;
        int m_FirstValidBlockIndex;
        UnsafeList<Block> m_FreeBlocks;
        UnsafeList<Block> m_PendingDeallocations;

        /// <summary>Creates a new <see cref="ElementAllocator"/></summary>
        /// <param name="initialCapacity">The initial capacity of the allocator.</param>
        /// <param name="allocator">The allocator to use for internal data structures.</param>
        public ElementAllocator(int initialCapacity, AllocatorManager.AllocatorHandle allocator)
        {
            m_AllocatedSize = 0;
            m_MaxAllocatedSize = 0;
            m_HighestAllocatedSize = 0;
            m_FirstValidBlockIndex = 0;
            m_FreeBlocks = new UnsafeList<Block>(initialCapacity, allocator);
            m_PendingDeallocations = new UnsafeList<Block>(initialCapacity, allocator);
        }

        /// <summary>Disposes the allocator.</summary>
        public void Dispose()
        {
            if (!IsCreated)
                return;

            m_FreeBlocks.Dispose();
            m_PendingDeallocations.Dispose();
        }

        /// <summary>Returns true if the allocator has been created.</summary>
        public bool IsCreated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_FreeBlocks.IsCreated && m_PendingDeallocations.IsCreated;
        }

        /// <summary>Returns the current allocated size.</summary>
        public int AllocatedSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_AllocatedSize;
        }

        /// <summary>Returns the peak size that the allocator has reached.</summary>
        /// <remarks>This is useful for determining a capacity for external data structures.</remarks>
        public int MaxAllocatedSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_MaxAllocatedSize;
        }

        /// <summary>Returns the number of free elements.</summary>
        public int AvailableBlocks
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_FreeBlocks.Length + m_PendingDeallocations.Length;
        }

        /// <summary>Returns the number of pending deallocations that will be processed on the next <see cref="MergeFree"/>.</summary>
        public int PendingFreeBlockCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_PendingDeallocations.Length;
        }

        /// <summary>Clears all allocations, resetting the allocator.</summary>
        public void Clear()
        {
            m_AllocatedSize = 0;
            m_MaxAllocatedSize = 0;
            m_HighestAllocatedSize = 0;
            m_FirstValidBlockIndex = 0;
            m_FreeBlocks.Clear();
            m_PendingDeallocations.Clear();
        }

        /// <summary>Allocates a group of elements of the specified count.</summary>
        /// <param name="count">Number of elements to allocate.</param>
        /// <returns>Returns the index of the allocated element block.</returns>
        public int Allocate(int count = 1)
        {
            // Find a free block that can accommodate the requested length
            int freeBlockIndex = FindFreeBlock(count, m_FirstValidBlockIndex);

            // If no suitable block was found, merge pending deallocations and try again
            if (freeBlockIndex == -1 && !m_PendingDeallocations.IsEmpty)
            {
                MergeFree();
                freeBlockIndex = FindFreeBlock(count, m_FirstValidBlockIndex);
            }

            m_AllocatedSize += count;

            // If a free block is found, allocate within it.
            if (freeBlockIndex != -1)
            {
                Block freeBlock = m_FreeBlocks[freeBlockIndex];

                // Remove count from the free block
                m_FreeBlocks[freeBlockIndex] = new Block { Offset = freeBlock.Offset + count, Length = freeBlock.Length - count };

                // Update the index for first available block
                if (freeBlock.Length == count && m_FirstValidBlockIndex == freeBlockIndex)
                {
                    m_FirstValidBlockIndex = freeBlockIndex + 1;
                }

                return freeBlock.Offset;
            }

            // Allocate a new block at the end.
            int newBlockOffset = m_MaxAllocatedSize;
            m_MaxAllocatedSize += count;
            m_HighestAllocatedSize = math.max(m_HighestAllocatedSize, m_MaxAllocatedSize);
            return newBlockOffset;
        }

        /// <summary>Frees an element block starting at the given offset and length, and merges adjacent free blocks.</summary>
        /// <remarks>Does not merge adjacent free blocks, must be done manually with <see cref="MergeFree"/>.</remarks>
        /// <param name="index">Start index of the element block to free.</param>
        /// <param name="count">Number of elements to free.</param>
        public void Free(int index, int count = 1)
        {
            CheckIndexCount(index, count);
            m_PendingDeallocations.Add(new Block { Offset = index, Length = count });
            m_AllocatedSize -= count;
        }

        /// <summary>Merges adjacent free element blocks.</summary>
        /// <remarks>Must be called manually after a series of <see cref="Free"/> calls.</remarks>
        public void MergeFree()
        {
            if (m_PendingDeallocations.IsEmpty && m_FirstValidBlockIndex == 0)
                return;

            // Sort the Newly free list by block start, the existing free list is already sorted by construction
            NativeSortExtension.Sort(m_PendingDeallocations.Ptr, m_PendingDeallocations.Length);

            // Alternate free list, used during consolidation to avoid N^2 worst case
            UnsafeList<Block> mergedFreeBlocks = new UnsafeList<Block>(m_FreeBlocks.Length, AllocatorManager.Temp);
            int prevEndOffset = -1;
            int pendingIndex = 0;

            // Joint loop and merge over both free list and newly freed and fuse all adjacent, copy into new free list (to avoid compaction)
            for (int i = 0; i < m_FreeBlocks.Length || pendingIndex < m_PendingDeallocations.Length; )
            {
                Block currentBlock = i < m_FreeBlocks.Length ? m_FreeBlocks[i] : new Block { Offset = int.MaxValue, Length = 0 };
                Assert.IsTrue(pendingIndex < m_PendingDeallocations.Length || currentBlock.Offset < int.MaxValue);

                // Consume the next new alloc if it is before the next old one
                if (pendingIndex < m_PendingDeallocations.Length && m_PendingDeallocations[pendingIndex].Offset < currentBlock.Offset)
                {
                    currentBlock = m_PendingDeallocations[pendingIndex++];
                }
                else
                {
                    // Otherwise advance the old allocs
                    ++i;
                }

                Assert.IsTrue(currentBlock.Offset < int.MaxValue);

                // Merge adjacent blocks
                if (currentBlock.Length > 0)
                {
                    if (prevEndOffset == currentBlock.Offset)
                    {
                        mergedFreeBlocks.Ptr[mergedFreeBlocks.Length-1].Length += currentBlock.Length;
                    }
                    else
                    {
                        mergedFreeBlocks.Add(currentBlock);
                    }

                    prevEndOffset = mergedFreeBlocks[^1].Length + mergedFreeBlocks[^1].Offset;
                }
            }

            // Trim last block
            if (!mergedFreeBlocks.IsEmpty && mergedFreeBlocks[^1].Offset + mergedFreeBlocks[^1].Length == m_MaxAllocatedSize)
            {
                m_MaxAllocatedSize -= mergedFreeBlocks[^1].Length;
                mergedFreeBlocks.RemoveAtSwapBack(mergedFreeBlocks.Length-1);
            }

            // Update free block list and reset pending deallocations
            m_FreeBlocks.CopyFrom(mergedFreeBlocks);
            m_PendingDeallocations.Clear();
            m_FirstValidBlockIndex = 0;
        }

        /// <summary>Checks if a specific element index is free.</summary>
        /// <param name="index">Index of the element to check.</param>
        /// <returns>True if the element is free; otherwise, false.</returns>
        public bool IsElementFree(int index)
        {
            // If outside the max size, it is considered free as the allocator can grow at will
            if (index >= m_MaxAllocatedSize) return true;

            for (int i = 0; i < m_FreeBlocks.Length; i++)
            {
                ref readonly Block freeBlock = ref m_FreeBlocks.Ptr[i];
                if (index >= freeBlock.Offset && index < freeBlock.Offset + freeBlock.Length)
                    return true;
            }

            return false;
        }

        /// <summary>Finds a free block of the specified length, starting the search at a given index.</summary>
        /// <param name="count">Number of elements to find.</param>
        /// <param name="startIndex">Index to start the search at.</param>
        /// <returns>Index of the found block, or -1 if not found.</returns>
        int FindFreeBlock(int count, int startIndex)
        {
            for (int i = startIndex; i < m_FreeBlocks.Length; i++)
            {
                Block freeBlock = m_FreeBlocks[i];
                if (freeBlock.Length >= count)
                    return i;
            }

            return -1;
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void CheckIndexCount(int index, int count)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (index < 0 || index + count > m_MaxAllocatedSize)
                throw new IndexOutOfRangeException($"ElementAllocator: Index {index} with count {count} is out of range.");

            if (count > m_AllocatedSize)
                throw new IndexOutOfRangeException($"ElementAllocator: Count {count} is greater than the allocated size {m_AllocatedSize}.");
#endif
        }
    }
}
