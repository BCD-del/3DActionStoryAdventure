// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MA.Mathematics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace MA.Collections.Unsafe
{
    /// <summary>Represents an unsafe bit list with dynamic resizing capabilities.</summary>
    /// <remarks>This data structure allows efficient manipulation of individual bits.</remarks>
    [DebuggerDisplay("Length = {Length}, IsCreated = {IsCreated}")]
    [DebuggerTypeProxy(typeof(UnsafeBitListDebugView))]
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct UnsafeBitList : IDisposable, IEquatable<UnsafeBitList>
    {
        [NativeDisableUnsafePtrRestriction] ulong* m_Ptr;
        int m_Length;
        int m_Capacity;
        AllocatorManager.AllocatorHandle m_Allocator;

        /// <summary>Initializes a new instance of the <see cref="UnsafeBitList"/> class with the specified capacity and allocator.</summary>
        /// <param name="length">The initial capacity of the bit list.</param>
        /// <param name="allocator">The allocator to use for memory management.</param>
        /// <param name="options"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeBitList(int length, AllocatorManager.AllocatorHandle allocator, NativeArrayOptions options = NativeArrayOptions.ClearMemory)
        {
            m_Allocator = allocator;
            m_Ptr = null;
            m_Length = 0;
            m_Capacity = 0;

            Resize(length, options);
        }

        /// <summary>Initializes a new instance of the <see cref="UnsafeBitList"/> class with the contents of another bit list.</summary>
        /// <param name="other">The bit list to copy bits from.</param>
        /// <param name="allocator">The allocator to use for memory management.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeBitList(UnsafeBitList other, AllocatorManager.AllocatorHandle allocator)
            : this(other.Length, allocator, NativeArrayOptions.UninitializedMemory)
        {
            CopyFrom(other);
        }

        /// <summary>Releases the resources used by the <see cref="UnsafeBitList"/>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (CollectionUtility.ShouldDeallocate(m_Allocator))
            {
                AllocatorManager.Free(m_Allocator, m_Ptr);
                m_Allocator = AllocatorManager.Invalid;
            }

            m_Ptr = null;
            m_Length = 0;
        }

        /// <summary>Creates and schedules a job that will dispose this array.</summary>
        /// <param name="inputDeps">The handle of a job which the new job will depend upon.</param>
        /// <returns>The handle of a new job that will dispose this array. The new job depends upon inputDeps.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JobHandle Dispose(JobHandle inputDeps)
        {
            if (CollectionUtility.ShouldDeallocate(m_Allocator))
            {
                var jobHandle = new UnsafeArrayDisposeJob { Data = new UnsafeArrayDispose { Ptr = m_Ptr, Allocator = m_Allocator } }.Schedule(inputDeps);

                m_Ptr = null;
                m_Allocator = AllocatorManager.Invalid;

                return jobHandle;
            }

            m_Ptr = null;

            return inputDeps;
        }

        /// <summary>Gets a pointer to the underlying bit array data.</summary>
        public readonly ulong* Ptr
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Ptr;
        }

        /// <summary>Gets or sets the number of bits in the bit list.</summary>
        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => m_Length;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Resize(value, NativeArrayOptions.ClearMemory);
        }

        /// <summary>Gets the number of chunks in the bit list.</summary>
        public readonly int ULongLength
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => MathUtility.DivideAndRoundUp(m_Length, 64);
        }

        /// <summary>Gets or sets the capacity of the bit list.</summary>
        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => m_Capacity;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Reserve(value);
        }

        /// <summary>Returns true if the <see cref="UnsafeBitList"/> is empty.</summary>
        public readonly bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => !IsCreated || m_Length == 0;
        }

        /// <summary>Gets a value indicating whether the <see cref="UnsafeBitList"/> is created and initialized.</summary>
        public readonly bool IsCreated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Ptr != null;
        }

        /// <summary>Gets or sets the bit at the specified index.</summary>
        /// <param name="bitIndex">The index of the bit to get or set.</param>
        /// <returns>The value of the bit at the specified index.</returns>
        public bool this[int bitIndex]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => BitUtility.IsSet(m_Ptr, m_Length, bitIndex);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => BitUtility.Set(m_Ptr, m_Length, bitIndex, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Realloc(int capacityInBits)
        {
            int newCapacity = BitUtility.AlignUp(capacityInBits, 64);
            int sizeInBytes = newCapacity / 8;

            ulong* newPointer = null;

            if (sizeInBytes > 0)
            {
                newPointer = (ulong*)AllocatorManager.Allocate(m_Allocator, sizeInBytes, 16);

                if (m_Capacity > 0)
                {
                    int itemsToCopy = math.min(newCapacity, m_Capacity);
                    int bytesToCopy = itemsToCopy / 8;
                    UnsafeUtility.MemCpy(newPointer, m_Ptr, bytesToCopy);
                }
            }

            AllocatorManager.Free(m_Allocator, m_Ptr);

            m_Ptr = newPointer;
            m_Capacity = newCapacity;
            m_Length = math.min(m_Length, newCapacity);
        }

        /// <summary>Sets the length, expanding the capacity if necessary.</summary>
        /// <param name="numBits">The new length in bits.</param>
        /// <param name="options">Whether newly allocated data should be zeroed out.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize(int numBits, NativeArrayOptions options = NativeArrayOptions.ClearMemory)
        {
            CollectionUtility.CheckAllocator(m_Allocator);

            int minCapacity = math.max(numBits, 1);
            if (minCapacity > m_Capacity)
            {
                SetCapacity(minCapacity);
            }

            int oldLength = m_Length;
            m_Length = numBits;

            if (options == NativeArrayOptions.ClearMemory && oldLength < m_Length)
            {
                SetRange(oldLength, m_Length - oldLength, false);
            }
        }

        /// <summary>Resizes the bit list to the specified length without initializing added bits.</summary>
        /// <param name="newLength">The new length of the bit list.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResizeUninitialized(int newLength) => Resize(newLength, NativeArrayOptions.UninitializedMemory);

        /// <summary>Sets the capacity to match what it would be if it had been originally initialized with all its entries.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TrimExcess() => SetCapacity(m_Length);

        /// <summary>Initializes the bit list with the specified value and length.</summary>
        /// <param name="value">The value to initialize the bits with.</param>
        /// <param name="length">The length of the bit list.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Initialize(bool value, int length)
        {
            Resize(length);
            if (m_Length > 0)
                BitUtility.SetBits(m_Ptr, m_Length, 0, value, m_Length);
        }

        /// <summary>Sets the capacity.</summary>
        /// <param name="capacityInBits">The new capacity.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCapacity(int capacityInBits)
        {
            if (m_Capacity == capacityInBits)
                return;

            CheckCapacityInRange(capacityInBits, m_Length);
            Realloc(capacityInBits);
        }

        /// <summary>Atomically sets the bit at the specified index to the specified value.</summary>
        /// <param name="index">The index of the bit to set.</param>
        /// <param name="value">The value to set the bit to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetAtomic(int index, bool value) => BitUtility.SetAtomic((long*)m_Ptr, m_Length, index, value);

        /// <summary>Sets a range of bits to the specified value.</summary>
        /// <param name="index">The starting index of the range to set.</param>
        /// <param name="count">The number of bits to set.</param>
        /// <param name="value">The value to set the bits to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetRange(int index, int count, bool value) => BitUtility.SetBits(m_Ptr, m_Length, index, value, count);

        /// <summary>Sets a range of bits to the specified value.</summary>
        /// <param name="value">The value to set the bits to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetAll(bool value) => BitUtility.SetBits(m_Ptr, m_Length, 0, value, m_Length);

        /// <summary>Checks whether the specified index is a valid index within the bit list.</summary>
        /// <param name="index">The index to check for validity.</param>
        /// <returns><c>true</c> if the index is valid (within the range of the bit list), otherwise <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsValidIndex(int index) => index >= 0 && index < m_Length;

        /// <summary>Clears all bits in the bit list.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => m_Length = 0;

        /// <summary>Reserves capacity for the bit list.</summary>
        /// <param name="capacity">The desired capacity.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reserve(int capacity) => SetCapacity(capacity);

        /// <summary>Adds a new bit to the end of the bit list and returns its index.</summary>
        /// <param name="value">The value of the new bit.</param>
        /// <returns>The index of the newly added bit.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Add(bool value)
        {
            int index = AddUninitialized(1);
            this[index] = value;
            return index;
        }

        /// <summary>Adds uninitialized bits to the end of the bit list and returns the index of the first added bit.</summary>
        /// <param name="count">The number of bits to add.</param>
        /// <returns>The index of the first added bit.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int AddUninitialized(int count)
        {
            int index = m_Length;
            if (count > 0) ResizeUninitialized(index + count);
            return index;
        }

        /// <summary>Inserts a bit at the specified index with the given value.</summary>
        /// <param name="index">The index at which to insert the bit.</param>
        /// <param name="value">The value of the inserted bit.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Insert(int index, bool value) => InsertRange(index, 1, value);

        /// <summary>Inserts a range of bits at the specified index with the given value.</summary>
        /// <param name="index">The index at which to insert the bits.</param>
        /// <param name="count">The number of bits to insert.</param>
        /// <param name="value">The value of the inserted bits.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InsertRange(int index, int count, bool value)
        {
            InsertUninitialized(index, count);
            BitUtility.SetBits(m_Ptr, m_Length, index, value, count);
        }

        /// <summary>Inserts uninitialized bits at the specified index.</summary>
        /// <param name="index">The index at which to insert the bits.</param>
        /// <param name="count">The number of bits to insert.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InsertUninitialized(int index, int count)
        {
            if (count > 0)
            {
                int oldLength = m_Length;
                AddUninitialized(count);

                int shiftCount = oldLength - index;
                if (shiftCount > 0)
                {
                    BitUtility.Copy(m_Ptr, m_Length, index + count, m_Ptr, m_Length, index, shiftCount);
                }
            }
        }

        /// <summary>Removes a bit at the specified index.</summary>
        /// <param name="index">The index of the bit to remove.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index) => RemoveAtRange(index, 1);

        /// <summary>Removes a range of bits starting from the specified index.</summary>
        /// <param name="index">The starting index of the range to remove.</param>
        /// <param name="count">The number of bits to remove.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAtRange(int index, int count)
        {
            if (index + count != m_Length)
            {
                int shiftCount = m_Length - index - count;
                BitUtility.Copy(m_Ptr, m_Length, index, m_Ptr, m_Length, index + count, shiftCount);
            }

            m_Length -= count;
        }

        /// <summary>Removes a bit at the specified index by swapping it with the last bit.</summary>
        /// <param name="index">The index of the bit to remove.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAtSwapBack(int index) => RemoveRangeSwapBack(index, 1);

        /// <summary>Removes a range of bits starting from the specified index by swapping them with bits from the end.</summary>
        /// <param name="index">The starting index of the range to remove.</param>
        /// <param name="count">The number of bits to remove.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveRangeSwapBack(int index, int count)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (!IsValidIndex(index))
                throw new ArgumentOutOfRangeException($"Index {index} is out of range in container of '{m_Length}' Length.");
            if (index + count > m_Length)
                throw new ArgumentOutOfRangeException($"Index {index} + Count {count} is out of range in container of '{m_Length}' Length.");
#endif

            if (index < m_Length - count)
            {
                for (int i = 0; i < count; ++i)
                    this[index + i] = this[m_Length - count + i];
            }

            m_Length -= count;
        }

        /// <summary>Copies all bits from a source pointer to the bit list.</summary>
        /// <param name="srcChunks">The source pointer to copy bits from.</param>
        /// <param name="srcLength">The number of bits to copy from the source pointer.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(ulong* srcChunks, int srcLength)
        {
            Clear();
            Resize(srcLength);
            int chunkCount = MathUtility.DivideAndRoundUp(srcLength, 64);
            UnsafeUtility.MemCpy(m_Ptr, srcChunks, chunkCount * sizeof(ulong));
        }

        /// <summary>Copies all bits from a source bit list to the bit list.</summary>
        /// <param name="src">The source bit list to copy bits from.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the source bit list is invalid.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(UnsafeBitList src)
        {
            Clear();
            Resize(src.Length);
            int chunkCount = MathUtility.DivideAndRoundUp(src.Length, 64);
            UnsafeUtility.MemCpy(m_Ptr, src.Ptr, chunkCount * sizeof(ulong));
        }

        /// <summary>Counts the number of set (true) bits within the specified range of the bit list.</summary>
        /// <param name="startIndex">The index at which to start counting bits (default is 0).</param>
        /// <param name="count">The number of bits to count (default is 1).</param>
        /// <returns>The count of set bits within the specified range.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CountBits(int startIndex = 0, int count = -1)
        {
            if (count < 0) count = m_Length - startIndex;
            return BitUtility.CountBits(m_Ptr, m_Length, startIndex, count);
        }

        /// <summary>Searches for the specified boolean value in the bit list, starting from the specified index.</summary>
        /// <param name="value">The boolean value to search for.</param>
        /// <param name="startIndex">The index at which to start the search (default is 0).</param>
        /// <returns>The index of the first occurrence of the specified value within the bit list, starting from the specified index, or -1 if the value is not found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int FindFirst(bool value, int startIndex = 0) => BitUtility.FindFirst(m_Ptr, m_Length, value, startIndex);

        /// <summary>Searches for the last occurrence of the specified boolean value in the bit list.</summary>
        /// <param name="value">The boolean value to search for.</param>
        /// <returns>The index of the last occurrence of the specified value within the bit list, or -1 if the value is not found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int FindLast(bool value) => BitUtility.FindLast(m_Ptr, m_Length, value);

        /// <summary>Determines whether the bit list contains the specified boolean value.</summary>
        /// <param name="value">The boolean value to check for.</param>
        /// <returns><c>true</c> if the bit list contains the specified value; otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(bool value) => BitUtility.FindFirst(m_Ptr, m_Length, value) != -1;

        /// <summary>Finds the first zero bit in the bit list starting from the specified index and sets it to true.</summary>
        /// <param name="startIndex">The index at which to start the search (default is 0).</param>
        /// <returns>The index of the first zero bit found and set to true, or -1 if no zero bit is found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int FindAndSetFirstZeroBit(int startIndex = 0) => BitUtility.FindAndSetFirstZeroBit(m_Ptr, m_Length, startIndex);

        /// <summary>Finds the last zero bit in the bit list and sets it to true.</summary>
        /// <returns>The index of the last zero bit found and set to true, or -1 if no zero bit is found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int FindAndSetLastZeroBit() => BitUtility.FindAndSetLastZeroBit(m_Ptr, m_Length);

        /// <summary>Provides an enumerator to iterate through the indices of set bits in an UnsafeBitList.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitUtility.SetBitEnumerator SetBitEnumerator() => BitUtility.GetSetBitIndexEnumerator(m_Ptr, m_Length);

        /// <summary>Provides an enumerator to iterate through the indices of set bits in an UnsafeBitList.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitUtility.SetBitEnumerator SetBitEnumerator(int startIndex, int count) => BitUtility.GetSetBitIndexEnumerator(m_Ptr, m_Length, startIndex, count);

        /// <summary>Tests whether two UnsafeBitList instances are equal.</summary>
        /// <param name="other">The UnsafeBitList to compare with the current instance.</param>
        /// <returns>True if the two UnsafeBitList instances are equal; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(UnsafeBitList other)
        {
            if (m_Ptr == other.m_Ptr) return true;
            if (m_Ptr == null || other.m_Ptr == null) return false;
            return m_Length == other.m_Length && BitUtility.AreEqual(m_Ptr, other.m_Ptr, m_Length);
        }

        /// <summary>Tests if the UnsafeBitList is equal to the specified object.</summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns>True if the object is an UnsafeBitList and is equal to the current instance; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj) => obj is UnsafeBitList other && Equals(other);

        /// <summary>Returns the hash code for the UnsafeBitList.</summary>
        /// <returns>The hash code for the UnsafeBitList.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            if (m_Ptr == null || Length == 0) return 0;
            return BitUtility.Hash(m_Ptr, m_Length);
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void CheckCapacityInRange(int capacity, int length)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException($"Capacity {capacity} must be positive.");

            if (capacity < length)
                throw new ArgumentOutOfRangeException($"Capacity {capacity} is out of range in container of '{length}' Length.");
        }
    }

    sealed class UnsafeBitListDebugView
    {
        UnsafeBitList m_Data;

        public UnsafeBitListDebugView(UnsafeBitList data)
        {
            m_Data = data;
        }

        public bool[] Bits
        {
            get
            {
                var array = new bool[m_Data.Length];
                for (int i = 0; i < m_Data.Length; ++i)
                {
                    array[i] = m_Data[i];
                }
                return array;
            }
        }
    }
}
