// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Unity.Burst;
using Unity.Burst.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace MA.Collections
{
    /// <summary>An unmanaged, resizable untyped list.</summary>
    [DebuggerDisplay("Length = {Length}, Capacity = {Capacity}, IsCreated = {IsCreated}, IsEmpty = {IsEmpty}")]
    [DebuggerTypeProxy(typeof(UnsafeUntypedListTDebugView))]
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct UnsafeUntypedList : INativeDisposable
    {
        /// <summary>Pointer to the internal buffer.</summary>
        [NativeDisableUnsafePtrRestriction] public void* Ptr;
        /// <summary>The size of each element in bytes.</summary>
        public readonly int ElementSize;
        /// <summary>The alignment of each element in bytes.</summary>
        public readonly int ElementAlignment;
        /// <summary>The internal count of elements.</summary>
        public int m_length;
        /// <summary>The internal capacity of elements.</summary>
        public int m_capacity;
        /// <summary>The allocator to use.</summary>
        public AllocatorManager.AllocatorHandle Allocator;

        /// <summary>The number of elements.</summary>
        /// <value>The number of elements.</value>
        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => AssumePositive(m_length);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (value > Capacity)
                {
                    Resize(value);
                }
                else
                {
                    m_length = value;
                }
            }
        }

        /// <summary>The number of elements that can fit in the internal buffer.</summary>
        /// <value>The number of elements that can fit in the internal buffer.</value>
        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => AssumePositive(m_capacity);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SetCapacity(value);
        }

        /// <summary>Returns true if the index is valid for this list.</summary>
        /// <param name="index">The index to check.</param>
        /// <returns>True if the index is valid.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsValidIndex(int index) => index >= 0 && index < m_length;

        /// <summary>Gets the element at a given index.</summary>
        /// <param name="index">The index to access. Must be in the range of [0..Length).</param>
        /// <typeparam name="T">The type of the value to get, must match the element type of the list.</typeparam>
        /// <returns>The element at the index.</returns>
        public T GetElement<T>(int index) where T : unmanaged
        {
            CheckElementSize(sizeof(T));
            CheckIndexInRange(index, m_length);
            return *(T*)((byte*)Ptr + index * ElementSize);
        }

        /// <summary>Sets the element at a given index.</summary>
        /// <param name="index">The index to access. Must be in the range of [0..Length).</param>
        /// <param name="value">The value to set.</param>
        /// <typeparam name="T">The type of the value to set, must match the element type of the list.</typeparam>
        public void SetElement<T>(int index, T value) where T : unmanaged
        {
            CheckElementSize(sizeof(T));
            CheckIndexInRange(index, m_length);
            *(T*)((byte*)Ptr + index * ElementSize) = value;
        }

        /// <summary>Returns a reference to the element at a given index.</summary>
        /// <param name="index">The index to access. Must be in the range of [0..Length).</param>
        /// <returns>A reference to the element at the index.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T ElementAt<T>(int index) where T : unmanaged
        {
            CheckIndexInRange(index, m_length);
            return ref UnsafeUtility.AsRef<T>((byte*)Ptr + index * ElementSize);
        }

        /// <summary>Initializes and returns an instance of UnsafeUntypedList.</summary>
        /// <param name="ptr">An existing byte array to set as the internal buffer.</param>
        /// <param name="elementSize"></param>
        /// <param name="length">The length.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeUntypedList(void* ptr, int elementSize, int length) : this()
        {
            Ptr = ptr;
            m_length = length;
            m_capacity = length;
            ElementSize = elementSize;
            Allocator = AllocatorManager.None;
        }

        /// <summary>Initializes and returns an instance of UnsafeUntypedList.</summary>
        /// <param name="initialCapacity">The initial capacity of the list.</param>
        /// <param name="elementAlignment"></param>
        /// <param name="allocator">The allocator to use.</param>
        /// <param name="options">Whether newly allocated bytes should be zeroed out.</param>
        /// <param name="elementSize"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeUntypedList(int initialCapacity, int elementSize, int elementAlignment, AllocatorManager.AllocatorHandle allocator, NativeArrayOptions options = NativeArrayOptions.UninitializedMemory)
        {
            Ptr = null;
            m_length = 0;
            m_capacity = 0;
            ElementSize = elementSize;
            ElementAlignment = elementAlignment;
            Allocator = allocator;

            SetCapacity(math.max(initialCapacity, 1));

            if (options == NativeArrayOptions.ClearMemory && Ptr != null)
            {
                UnsafeUtility.MemClear(Ptr, Capacity * ElementSize);
            }
        }

        /// <summary>Initializes and returns an instance of UnsafeUntypedList with the same data as another list.</summary>
        /// <param name="other">The list to copy data from.</param>
        /// <param name="allocator">The allocator to use.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeUntypedList(UnsafeUntypedList other, AllocatorManager.AllocatorHandle allocator) : this()
        {
            Ptr = null;
            m_length = 0;
            m_capacity = 0;
            ElementSize = other.ElementSize;
            ElementAlignment = other.ElementAlignment;
            Allocator = allocator;

            SetCapacity(other.Capacity);

            if (other.Ptr != null)
            {
                UnsafeUtility.MemCpy(Ptr, other.Ptr, other.Length * ElementSize);
                m_length = other.Length;
            }
        }

        /// <summary>Returns a new list.</summary>
        /// <param name="initialCapacity">The initial capacity of the list.</param>
        /// <param name="allocator">The allocator to use.</param>
        /// <param name="options">Whether newly allocated bytes should be zeroed out.</param>
        /// <returns>A pointer to the new list.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeUntypedList* Create(int initialCapacity, int elementSize, int elementAlignment, AllocatorManager.AllocatorHandle allocator, NativeArrayOptions options = NativeArrayOptions.UninitializedMemory)
        {
            UnsafeUntypedList* listData = AllocatorManager.Allocate<UnsafeUntypedList>(allocator.Handle);
            *listData = new UnsafeUntypedList(initialCapacity, elementSize, elementAlignment, allocator.Handle, options);
            return listData;
        }

        /// <summary>Destroys the list.</summary>
        /// <param name="listData">The list to destroy.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Destroy(UnsafeUntypedList* listData)
        {
            CheckNull(listData);
            AllocatorManager.AllocatorHandle allocator = listData->Allocator;
            listData->Dispose();
            AllocatorManager.Free(allocator, listData, sizeof(UnsafeUntypedList), UnsafeUtility.AlignOf<UnsafeUntypedList>(), 1);
        }

        /// <summary>Whether the list is empty.</summary>
        /// <value>True if the list is empty or the list has not been constructed.</value>
        public readonly bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => !IsCreated || m_length == 0;
        }

        /// <summary>Whether this list has been allocated (and not yet deallocated).</summary>
        /// <value>True if this list has been allocated (and not yet deallocated).</value>
        public readonly bool IsCreated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Ptr != null;
        }

        /// <summary>Releases all resources (memory).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (!IsCreated)
                return;

            if (CollectionUtility.ShouldDeallocate(Allocator))
            {
                AllocatorManager.Free(Allocator, Ptr, ElementSize, ElementAlignment, m_capacity);
                Allocator = AllocatorManager.Invalid;
            }

            Ptr = null;
            m_length = 0;
            m_capacity = 0;
        }

        /// <summary>Creates and schedules a job that frees the memory of this list. </summary>
        /// <param name="inputDeps">The dependency for the new job.</param>
        /// <returns>The handle of the new job. The job depends upon `inputDeps` and frees the memory of this list.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JobHandle Dispose(JobHandle inputDeps)
        {
            if (!IsCreated)
            {
                return inputDeps;
            }

            if (CollectionUtility.ShouldDeallocate(Allocator))
            {
                JobHandle jobHandle = new UnsafeUntypedListDisposeJob
                {
                    Ptr = Ptr,
                    ElementSize = ElementSize,
                    ElementAlignment = ElementAlignment,
                    Capacity = m_capacity,
                    Allocator = Allocator
                }.Schedule(inputDeps);

                Ptr = null;
                Allocator = AllocatorManager.Invalid;

                return jobHandle;
            }

            Ptr = null;

            return inputDeps;
        }

        /// <summary>Sets the length to 0.</summary>
        /// <remarks>Does not change the capacity.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            m_length = 0;
        }

        /// <summary>Sets the length, expanding the capacity if necessary.</summary>
        /// <param name="length">The new length.</param>
        /// <param name="options">Whether newly allocated bytes should be zeroed out.</param>
        public void Resize(int length, NativeArrayOptions options = NativeArrayOptions.UninitializedMemory)
        {
            int oldLength = m_length;
            if (length > Capacity)
                SetCapacity(length);

            m_length = length;

            if (options == NativeArrayOptions.ClearMemory && oldLength < length)
            {
                int num = length - oldLength;
                byte* ptr = (byte*)Ptr;
                UnsafeUtility.MemClear(ptr + oldLength * ElementSize, num * ElementSize);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ResizeExact(int newCapacity)
        {
            newCapacity = math.max(0, newCapacity);

            void* newPointer = null;

            if (newCapacity > 0)
            {
                newPointer = AllocatorManager.Allocate(Allocator, ElementSize, ElementAlignment, newCapacity);

                if (Ptr != null && m_capacity > 0)
                {
                    int itemsToCopy = math.min(newCapacity, Capacity);
                    int bytesToCopy = itemsToCopy * ElementSize;
                    UnsafeUtility.MemCpy(newPointer, Ptr, bytesToCopy);
                }
            }

            AllocatorManager.Free(Allocator, Ptr, ElementSize, ElementAlignment, newCapacity);

            Ptr = newPointer;
            m_capacity = newCapacity;
            m_length = math.min(m_length, newCapacity);
        }

        /// <summary>Sets the capacity.</summary>
        /// <param name="capacity">The new capacity.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCapacity(int capacity)
        {
            int newCapacity = math.max(capacity, CollectionHelper.CacheLineSize / ElementSize);
            newCapacity = math.ceilpow2(newCapacity);

            if (newCapacity == Capacity)
                return;

            ResizeExact(newCapacity);
        }

        /// <summary>Sets the capacity to match the length.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TrimExcess()
        {
            if (Capacity != m_length)
            {
                ResizeExact(m_length);
            }
        }

        /// <summary>Adds an element to the end of this list.</summary>
        /// <remarks>Increments the length by 1. Never increases the capacity.</remarks>
        /// <param name="value">The value to add to the end of the list.</param>
        /// <exception cref="InvalidOperationException">Thrown if incrementing the length would exceed the capacity.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddNoResize<T>(T value) where T : unmanaged
        {
            CheckElementSize(sizeof(T));
            CheckNoResizeHasEnoughCapacity(1);
            UnsafeUtility.WriteArrayElement(Ptr, m_length, value);
            m_length += 1;
        }

        /// <summary>Copies elements from a buffer to the end of this list.</summary>
        /// <remarks>Increments the length by `count`. Never increases the capacity.</remarks>
        /// <param name="ptr">The buffer to copy from.</param>
        /// <param name="count">The number of elements to copy from the buffer.</param>
        /// <exception cref="InvalidOperationException">Thrown if the increased length would exceed the capacity.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRangeNoResize(void* ptr, int count)
        {
            CheckNoResizeHasEnoughCapacity(count);
            void* dst = (byte*)Ptr + m_length * ElementSize;
            UnsafeUtility.MemCpy(dst, ptr, count * ElementSize);
            m_length += count;
        }

        /// <summary>Copies the elements of another list to the end of this list.</summary>
        /// <param name="list">The other list to copy from.</param>
        /// <remarks>Increments the length by the length of the other list. Never increases the capacity.</remarks>
        /// <exception cref="InvalidOperationException">Thrown if the increased length would exceed the capacity.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRangeNoResize(UnsafeUntypedList list)
        {
            CheckElementSize(list.ElementSize);
            AddRangeNoResize(list.Ptr, AssumePositive(list.Length));
        }

        /// <summary>Adds an element to the end of the list.</summary>
        /// <param name="value">The value to add to the end of this list.</param>
        /// <remarks>Increments the length by 1. Increases the capacity if necessary.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add<T>(in T value) where T : unmanaged
        {
            CheckElementSize(sizeof(T));
            int idx = m_length;
            if (m_length < m_capacity)
            {
                UnsafeUtility.WriteArrayElement(Ptr, idx, value);
                m_length++;
                return;
            }

            Resize(idx + 1);
            UnsafeUtility.WriteArrayElement(Ptr, idx, value);
        }

        /// <summary>Copies the elements of a buffer to the end of this list.</summary>
        /// <param name="ptr">The buffer to copy from.</param>
        /// <param name="count">The number of elements to copy from the buffer.</param>
        /// <remarks>Increments the length by `count`. Increases the capacity if necessary.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange(void* ptr, int count)
        {
            int index = m_length;

            if (m_length + count > Capacity)
                Resize(m_length + count);
            else
                m_length += count;

            void* dst = (byte*)Ptr + index * ElementSize;
            UnsafeUtility.MemCpy(dst, ptr, count * ElementSize);
        }

        /// <summary>Copies the elements of another list to the end of the list.</summary>
        /// <param name="list">The list to copy from.</param>
        /// <remarks>The length is increased by the length of the other list. Increases the capacity if necessary.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange(UnsafeUntypedList list) => AddRange(list.Ptr, list.Length);

        /// <summary>Appends value count times to the end of this list.</summary>
        /// <param name="value">The value to add to the end of this list.</param>
        /// <param name="count">The number of times to replicate the value.</param>
        /// <remarks>Length is incremented by count. If necessary, the capacity is increased.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddReplicate<T>(in T value, int count) where T : unmanaged
        {
            CheckElementSize(sizeof(T));
            int index = m_length;
            if (m_length + count > Capacity)
                Resize(m_length + count);
            else
                m_length += count;

            fixed (void* valuePtr = &value)
                UnsafeUtility.MemCpyReplicate((byte*)Ptr + (index * ElementSize), valuePtr, ElementSize, count);
        }

        /// <summary>Shifts elements toward the end of this list, increasing its length.</summary>
        /// <param name="begin">The index of the first element that will be shifted up.</param>
        /// <param name="end">The index where the first shifted element will end up.</param>
        /// <exception cref="ArgumentException">Thrown if `end &lt; begin`.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if `begin` or `end` are out of bounds.</exception>
        public void InsertRangeWithBeginEnd(int begin, int end)
        {
            CheckBeginEndNoLength(begin, end);

            // Because we've checked begin and end in `CheckBeginEnd` above, we can now
            // assume they are positive.
            begin = AssumePositive(begin);
            end = AssumePositive(end);

            int items = end - begin;
            if (items < 1)
                return;

            int oldLength = m_length;
            if (m_length + items > Capacity)
                Resize(m_length + items);
            else
                m_length += items;

            int itemsToCopy = oldLength - begin;
            if (itemsToCopy < 1)
                return;

            int bytesToCopy = itemsToCopy * ElementSize;
            byte* ptr = (byte*)Ptr;
            byte* dest = ptr + end * ElementSize;
            byte* src = ptr + begin * ElementSize;
            UnsafeUtility.MemMove(dest, src, bytesToCopy);
        }

        /// <summary>Shifts elements toward the end of this list, increasing its length.</summary>
        /// <param name="index">The index of the first element that will be shifted up.</param>
        /// <param name="count">The number of elements to insert.</param>
        /// <exception cref="ArgumentException">Thrown if `count` is negative.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if `index` is out of bounds.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InsertRange(int index, int count) => InsertRangeWithBeginEnd(index, index + count);

        /// <summary>Copies the last element of this list to the specified index. Decrements the length by 1.</summary>
        /// <remarks>Useful as a cheap way to remove an element from this list when you don't care about preserving order.</remarks>
        /// <param name="index">The index to overwrite with the last element.</param>
        /// <exception cref="IndexOutOfRangeException">Thrown if `index` is out of bounds.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAtSwapBack(int index) => RemoveRangeSwapBack(index, 1);

        /// <summary>Copies the last *N* elements of this list to a range in this list. Decrements the length by *N*.</summary>
        /// <param name="index">The index of the first element to overwrite.</param>
        /// <param name="count">The number of elements to copy and remove.</param>
        /// <exception cref="IndexOutOfRangeException">Thrown if `index` is out of bounds</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if `count` is negative,
        /// or `index + count` exceeds the length.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveRangeSwapBack(int index, int count)
        {
            CheckIndexCount(index, count);

            index = AssumePositive(index);
            count = AssumePositive(count);

            if (count > 0)
            {
                int copyFrom = math.max(m_length - count, index + count);
                void* dst = (byte*)Ptr + index * ElementSize;
                void* src = (byte*)Ptr + copyFrom * ElementSize;
                UnsafeUtility.MemCpy(dst, src, (m_length - copyFrom) * ElementSize);
                m_length -= count;
            }
        }

        /// <summary>Removes the element at an index, shifting everything above it down by one. Decrements the length by 1.</summary>
        /// <param name="index">The index of the element to remove.</param>
        /// <remarks>If you don't care about preserving the order of the elements, <see cref="RemoveAtSwapBack(int)"/> is a more efficient way to remove elements.</remarks>
        /// <exception cref="IndexOutOfRangeException">Thrown if `index` is out of bounds.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index)
        {
            CheckIndexInRange(index, m_length);

            index = AssumePositive(index);

            void* dst = (byte*)Ptr + (index * ElementSize);
            void* src = (byte*)dst + (1 * ElementSize);
            m_length--;

            UnsafeUtility.MemMove(dst, src, (m_length - index) * ElementSize);
        }

        /// <summary>
        /// Removes *N* elements in a range, shifting everything above the range down by *N*. Decrements the length by *N*.
        /// </summary>
        /// <param name="index">The index of the first element to remove.</param>
        /// <param name="count">The number of elements to remove.</param>
        /// <remarks>
        /// If you don't care about preserving the order of the elements, `RemoveRangeSwapBackWithBeginEnd`
        /// is a more efficient way to remove elements.
        /// </remarks>
        /// <exception cref="IndexOutOfRangeException">Thrown if `index` is out of bounds</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if `count` is negative,
        /// or `index + count` exceeds the length.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveRange(int index, int count)
        {
            CheckIndexCount(index, count);

            index = AssumePositive(index);
            count = AssumePositive(count);

            if (count > 0)
            {
                int copyFrom = math.min(index + count, m_length);
                void* dst = (byte*)Ptr + index * ElementSize;
                void* src = (byte*)Ptr + copyFrom * ElementSize;
                UnsafeUtility.MemCpy(dst, src, (m_length - copyFrom) * ElementSize);
                m_length -= count;
            }
        }

        /// <summary>Returns a span representing the elements of this list.</summary>
        /// <typeparam name="T">The type of the elements in the list.</typeparam>
        /// <returns>The span representing the elements of this list.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the size of `T` does not match the size of the elements in the list.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan<T>() where T : unmanaged
        {
            CheckElementSize(sizeof(T));
            return new Span<T>((T*)Ptr, Length);
        }

        /// <summary>Returns a readonly span representing the elements of this list.</summary>
        /// <typeparam name="T">The type of the elements in the list.</typeparam>
        /// <returns>The readonly span representing the elements of this list.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the size of `T` does not match the size of the elements in the list.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> ReadOnlySpan<T>() where T : unmanaged
        {
            CheckElementSize(sizeof(T));
            return new ReadOnlySpan<T>((T*)Ptr, Length);
        }

        /// <summary>Returns a read only of this list.</summary>
        /// <returns>A read only of this list.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeUntypedList.ReadOnly AsReadOnly() => new UnsafeUntypedList.ReadOnly(Ptr, ElementSize, Length);

        /// <summary>A read only for an UnsafeUntypedList.</summary>
        /// <remarks>Use <see cref="AsReadOnly"/> to create a read only for a list.</remarks>
        public struct ReadOnly
        {
            /// <summary> The internal buffer of the list.</summary>
            [NativeDisableUnsafePtrRestriction] public readonly void* Ptr;
            public readonly int ElementSize;
            /// <summary>The number of elements.</summary>
            public readonly int Length;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal ReadOnly(void* ptr, int elementSize, int length)
            {
                Ptr = ptr;
                ElementSize = elementSize;
                Length = length;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool IsValidIndex(int index) => index >= 0 && index < Length;

            /// <summary>Returns an enumerator over the elements of the list.</summary>
            /// <returns>An enumerator over the elements of the list.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public UnsafeUntypedList.Enumerator<T> GetEnumerator<T>() where T : unmanaged => new() { m_Ptr = (T*)Ptr, m_Length = Length, m_Index = -1 };
        }

        /// <summary>Returns a parallel writer of this list.</summary>
        /// <returns>A parallel writer of this list.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeUntypedList.ParallelWriter AsParallelWriter() => new((UnsafeUntypedList*)UnsafeUtility.AddressOf(ref this));

        /// <summary>A parallel writer for an UnsafeUntypedList.</summary>
        /// <remarks>
        /// Use <see cref="AsParallelWriter"/> to create a parallel writer for a list.
        /// </remarks>
        public struct ParallelWriter
        {
            /// <summary>The data of the list.</summary>
            public readonly void* Ptr
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ListData->Ptr;
            }

            /// <summary>The UnsafeUntypedList to write to.</summary>
            [NativeDisableUnsafePtrRestriction]
            public UnsafeUntypedList* ListData;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal ParallelWriter(UnsafeUntypedList* listData)
            {
                ListData = listData;
            }

            /// <summary>Adds an element to the end of the list.</summary>
            /// <param name="value">The value to add to the end of the list.</param>
            /// <remarks>Increments the length by 1. Never increases the capacity.</remarks>
            /// <exception cref="InvalidOperationException">Thrown if incrementing the length would exceed the capacity.</exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void AddNoResize<T>(T value) where T : unmanaged
            {
                ListData->CheckElementSize(sizeof(T));
                int idx = Interlocked.Increment(ref ListData->m_length) - 1;
                ListData->CheckNoResizeHasEnoughCapacity(idx, 1);
                UnsafeUtility.WriteArrayElement(ListData->Ptr, idx, value);
            }

            /// <summary>Copies elements from a buffer to the end of the list.</summary>
            /// <param name="ptr">The buffer to copy from.</param>
            /// <param name="count">The number of elements to copy from the buffer.</param>
            /// <remarks>Increments the length by `count`. Never increases the capacity.</remarks>
            /// <exception cref="InvalidOperationException">Thrown if the increased length would exceed the capacity.</exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void AddRangeNoResize(void* ptr, int count)
            {
                int idx = Interlocked.Add(ref ListData->m_length, count) - count;
                ListData->CheckNoResizeHasEnoughCapacity(idx, count);
                void* dst = (byte*)ListData->Ptr + idx * ListData->ElementSize;
                UnsafeUtility.MemCpy(dst, ptr, count * ListData->ElementSize);
            }

            /// <summary>Copies the elements of another list to the end of this list.</summary>
            /// <param name="list">The other list to copy from.</param>
            /// <remarks>Increments the length by the length of the other list. Never increases the capacity.</remarks>
            /// <exception cref="InvalidOperationException">Thrown if the increased length would exceed the capacity.</exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void AddRangeNoResize(UnsafeUntypedList list) => AddRangeNoResize(list.Ptr, list.Length);
        }

        /// <summary>Copies all elements of specified container to this container.</summary>
        /// <param name="other">An container to copy into this container.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom<T>(in NativeArray<T> other) where T : unmanaged
        {
            CheckElementSize(sizeof(T));
            Resize(other.Length);
            UnsafeUtility.MemCpy(Ptr, other.GetUnsafeReadOnlyPtr(), ElementSize * other.Length);
        }

        /// <summary>Copies all elements of specified container to this container.</summary>
        /// <param name="other">An container to copy into this container.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(in UnsafeUntypedList other)
        {
            CheckElementSize(other.ElementSize);
            Resize(other.Length);
            UnsafeUtility.MemCpy(Ptr, other.Ptr, ElementSize * other.Length);
        }

        /// <summary>Returns an enumerator over the elements of the list.</summary>
        /// <returns>An enumerator over the elements of the list.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeUntypedList.Enumerator<T> GetEnumerator<T>() where T : unmanaged
        {
            CheckElementSize(sizeof(T));
            return new Enumerator<T> { m_Ptr = (T*)Ptr, m_Length = Length, m_Index = -1 };
        }

        /// <summary>An enumerator over the elements of a list.</summary>
        /// <remarks>In an enumerator's initial state, <see cref="Current"/> is invalid. The first <see cref="MoveNext"/> call advances the enumerator to the first element of the list.</remarks>
        public struct Enumerator<T> : IEnumerator<T> where T : unmanaged
        {
            internal T* m_Ptr;
            internal int m_Length;
            internal int m_Index;

            /// <summary>Does nothing.</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose() { }

            /// <summary>Advances the enumerator to the next element of the list.</summary>
            /// <remarks>The first `MoveNext` call advances the enumerator to the first element of the list. Before this call, `Current` is not valid to read.</remarks>
            /// <returns>True if `Current` is valid to read after the call.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext() => ++m_Index < m_Length;

            /// <summary>Resets the enumerator to its initial state.</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset() => m_Index = -1;

            /// <summary>The current element.</summary>
            /// <value>The current element.</value>
            public T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => m_Ptr[m_Index];
            }

            object IEnumerator.Current => Current;
        }

        /// <summary>Tell Burst that an integer can be assumed to map to an always positive value.</summary>
        /// <param name="value">The integer that is always positive.</param>
        /// <returns>Returns `x`, but allows the compiler to assume it is always positive.</returns>
        [return: AssumeRange(0, int.MaxValue)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int AssumePositive(int value) => value;

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void CheckIndexInRange(int index, int length)
        {
            if (index < 0)
                throw new IndexOutOfRangeException($"Index {index} must be positive.");
            if (index >= length)
                throw new IndexOutOfRangeException($"Index {index} is out of range in container of '{length}' Length.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        internal static void CheckNull(void* listData)
        {
            if (listData == null)
            {
                throw new InvalidOperationException("UnsafeUntypedList has yet to be created or has been destroyed!");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        void CheckIndexCount(int index, int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException($"Value for count {count} must be positive.");
            }

            if (index < 0)
            {
                throw new IndexOutOfRangeException($"Value for index {index} must be positive.");
            }

            if (index >= Length)
            {
                throw new IndexOutOfRangeException($"Value for index {index} is out of bounds.");
            }

            if (index + count > Length)
            {
                throw new ArgumentOutOfRangeException($"Value for count {count} is out of bounds.");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        void CheckBeginEndNoLength(int begin, int end)
        {
            if (begin > end)
            {
                throw new ArgumentException($"Value for begin {begin} index must less or equal to end {end}.");
            }

            if (begin < 0)
            {
                throw new ArgumentOutOfRangeException($"Value for begin {begin} must be positive.");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        void CheckBeginEnd(int begin, int end)
        {
            CheckBeginEndNoLength(begin, end);

            if (begin > Length)
            {
                throw new ArgumentOutOfRangeException($"Value for begin {begin} is out of bounds.");
            }

            if (end > Length)
            {
                throw new ArgumentOutOfRangeException($"Value for end {end} is out of bounds.");
            }
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void CheckElementSize(int elementSize)
        {
            if (elementSize != ElementSize)
                throw new ArgumentException($"Element size {elementSize} does not match the element size of the list ({ElementSize}).");
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void CheckNoResizeHasEnoughCapacity(int length)
        {
            CheckNoResizeHasEnoughCapacity(length, Length);
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void CheckNoResizeHasEnoughCapacity(int length, int index)
        {
            if (Capacity < index + length)
            {
                throw new InvalidOperationException($"AddNoResize assumes that list capacity is sufficient (Capacity {Capacity}, Length {Length}), requested length {length}!");
            }
        }
    }

    [BurstCompile]
    unsafe struct UnsafeUntypedListDisposeJob : IJob
    {
        [NativeDisableUnsafePtrRestriction] public void* Ptr;
        public int ElementSize;
        public int ElementAlignment;
        public int Capacity;
        public AllocatorManager.AllocatorHandle Allocator;

        public void Execute()
        {
            AllocatorManager.Free(Allocator, Ptr, ElementSize, ElementAlignment, Capacity);
        }
    }

    /// <summary>Provides extension methods for UnsafeUntypedList.</summary>
    public static unsafe class UnsafeUntypedListExtensions
    {
        /// <summary>Finds the index of the first occurrence of a particular value in this list.</summary>
        /// <typeparam name="T">The type of elements in this list.</typeparam>
        /// <param name="list">This list.</param>
        /// <param name="value">A value to locate.</param>
        /// <returns>The zero-based index of the first occurrence of the value if it is found. Returns -1 if no occurrence is found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T>(this UnsafeUntypedList list, T value) where T : unmanaged, IEquatable<T>
            => Unity.Collections.NativeArrayExtensions.IndexOf<T, T>((T*)list.Ptr, list.Length, value);

        /// <summary>Returns true if a particular value is present in this list.</summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="list">This list.</param>
        /// <param name="value">The value to locate.</param>
        /// <returns>True if the value is present in this list.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains<T>(this UnsafeUntypedList list, T value) where T : unmanaged, IEquatable<T>
            => list.IndexOf(value) != -1;

        /// <summary>Finds the index of the first occurrence of a particular value in the list.</summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="list">This reader of the list.</param>
        /// <param name="value">A value to locate.</param>
        /// <returns>The zero-based index of the first occurrence of the value if it is found. Returns -1 if no occurrence is found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T>(this UnsafeUntypedList.ReadOnly list, T value) where T : unmanaged, IEquatable<T>
            => Unity.Collections.NativeArrayExtensions.IndexOf<T, T>((T*)list.Ptr, list.Length, value);

        /// <summary>Returns true if a particular value is present in the list.</summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="list">This reader of the list.</param>
        /// <param name="value">The value to locate.</param>
        /// <returns>True if the value is present in the list.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains<T>(this UnsafeUntypedList.ReadOnly list, T value) where T : unmanaged, IEquatable<T>
            => list.IndexOf(value) != -1;

        /// <summary>Returns true if this container and another have equal length and content.</summary>
        /// <typeparam name="T">The type of the source container's elements.</typeparam>
        /// <param name="container">The container to compare for equality.</param>
        /// <param name="other">The other container to compare for equality.</param>
        /// <returns>True if the containers have equal length and content.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ArraysEqual<T>(this UnsafeUntypedList container, in UnsafeUntypedList other)
        {
            if (container.Length != other.Length)
                return false;
            if (container.ElementSize != other.ElementSize)
                return false;
            if (container.ElementAlignment != other.ElementAlignment)
                return false;
            if (container.Ptr == other.Ptr)
                return true;

            return UnsafeUtility.MemCmp(container.Ptr, other.Ptr, container.Length * container.ElementSize) == 0;
        }

    }

    sealed unsafe class UnsafeUntypedListTDebugView
    {
        UnsafeUntypedList Data;

        public UnsafeUntypedListTDebugView(UnsafeUntypedList data)
        {
            Data = data;
        }

        public byte[] Bytes
        {
            get
            {
                byte[] result = new byte[Data.Length * Data.ElementSize];
                for (int i = 0; i < result.Length; ++i)
                    result[i] = ((byte*)Data.Ptr)[i];
                return result;
            }
        }

        public int ElementSize => Data.ElementSize;

        public int ElementAlignment => Data.ElementAlignment;

        public int Length => Data.Length;
    }
}
