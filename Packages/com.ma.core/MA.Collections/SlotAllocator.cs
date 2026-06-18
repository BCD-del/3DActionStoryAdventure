// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MA.Collections.Unsafe;
using MA.Mathematics;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Assertions;

namespace MA.Collections
{
    /// <summary>Represents a slot allocator.</summary>
    /// <remarks>Allocates slots using a bit list, very memory efficient.</remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct SlotAllocator: IDisposable
    {
        internal UnsafeBitList m_Slots;
        internal int m_NextFreeSlot;
        internal int m_MaxAllocatedSlot;
        internal int m_AllocatedCount;
        AllocatorManager.AllocatorHandle m_Allocator;

        /// <summary>Gets the maximum in-use slot count.</summary>
        public int MaxAllocatedSlot
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_MaxAllocatedSlot;
        }

        /// <summary>Gets the number of allocated slots.</summary>
        public int AllocatedCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_AllocatedCount;
        }

        /// <summary>Returns true if the slot allocator is empty; otherwise, false.</summary>
        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_AllocatedCount == 0;
        }

        /// <summary>Initializes a new instance of the <see cref="SlotAllocator"/>.</summary>
        /// <param name="capacity">The capacity of the slot allocator.</param>
        /// <param name="allocator">The allocator handle.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SlotAllocator(int capacity, AllocatorManager.AllocatorHandle allocator)
        {
            capacity = math.max(64, MathUtility.NextMultipleOf(capacity, 64));
            m_Allocator = allocator;
            m_Slots = new UnsafeBitList(capacity, allocator);
            m_NextFreeSlot = 0;
            m_AllocatedCount = 0;
            m_MaxAllocatedSlot = 0;
        }

        /// <summary>Initializes a new instance of the <see cref="SlotAllocator"/> with the specified active slots.</summary>
        /// <param name="slots">The active slots to initialize the slot allocator with.</param>
        /// <param name="allocator">The allocator handle.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SlotAllocator(ReadOnlySpan<int> slots, AllocatorManager.AllocatorHandle allocator)
        {
            m_Allocator = allocator;
            m_MaxAllocatedSlot = 0;
            for (int i = 0; i < slots.Length; i++)
                m_MaxAllocatedSlot = math.max(m_MaxAllocatedSlot, slots[i]);

            int capacity = math.max(64, MathUtility.NextMultipleOf(m_MaxAllocatedSlot + 1, 64));
            m_Slots = new UnsafeBitList(capacity, allocator);
            m_NextFreeSlot = 0;
            m_AllocatedCount = 0;

            for (int i = 0; i < slots.Length; i++)
            {
                int slot = slots[i];
                m_Slots[slot] = true;
                m_AllocatedCount++;
            }

            m_NextFreeSlot = m_MaxAllocatedSlot > 0 ? m_Slots.FindFirst(false) : 0;
        }

        /// <summary>Disposes the slot allocator.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            m_Slots.Dispose();
            m_AllocatedCount = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Grow()
        {
            int newCapacity = math.max(64, MathUtility.NextMultipleOf(m_Slots.Length * 2, 64));
            m_Slots.Resize(newCapacity);
        }

        /// <summary>Clears the slot allocator.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            m_Slots.Clear();
            m_NextFreeSlot = 0;
            m_AllocatedCount = 0;
            m_MaxAllocatedSlot = 0;
        }

        /// <summary>Reserves space in the slot allocator.</summary>
        /// <param name="capacity">The capacity to reserve.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reserve(int capacity)
        {
            if (m_Slots.Length < capacity)
                m_Slots.Resize(capacity);
        }

        /// <summary>Checks if the slot exists.</summary>
        /// <param name="slot">The slot to check.</param>
        /// <returns>True if the slot exists; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Exists(int slot) => m_Slots.IsValidIndex(slot) && m_Slots[slot];

        /// <summary>Allocates a slot.</summary>
        /// <returns>The allocated slot.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Allocate()
        {
            if (m_AllocatedCount == m_Slots.Length)
                Grow();

            int slot = m_Slots.FindAndSetFirstZeroBit(m_NextFreeSlot);
            if (slot == -1) slot = m_Slots.Add(true);

            m_NextFreeSlot = slot;
            m_MaxAllocatedSlot = math.max(m_MaxAllocatedSlot, slot);
            m_AllocatedCount++;
            return slot;
        }

        /// <summary>Frees the slot.</summary>
        /// <param name="slot">The slot to free.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Free(int slot)
        {
            Assert.IsTrue(Exists(slot), "Slot is not been allocated.");

            m_Slots[slot] = false;
            m_NextFreeSlot = math.min(m_NextFreeSlot, slot);
            if (slot == m_MaxAllocatedSlot)
                m_MaxAllocatedSlot = m_Slots.FindLast(true);

            m_AllocatedCount--;
            if (m_AllocatedCount == 0)
                m_MaxAllocatedSlot = 0;
        }

        /// <summary>Gets the enumerator of the slot allocator.</summary>
        /// <returns>The enumerator of the slot allocator.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() => new Enumerator(m_Slots);

        /// <summary>Enumerates allocated slots.</summary>
        public unsafe struct Enumerator : IEnumerator<int>
        {
            BitUtility.SetBitEnumerator m_SetBitEnumerator;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(UnsafeBitList slots)
            {
                m_SetBitEnumerator = new BitUtility.SetBitEnumerator(slots.Ptr, 0, slots.Length);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                return m_SetBitEnumerator.MoveNext();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset()
            {
                m_SetBitEnumerator.Reset();
            }

            public int Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => m_SetBitEnumerator.Current;
            }

            object System.Collections.IEnumerator.Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => Current;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                m_SetBitEnumerator.Dispose();
            }
        }
    }
}
