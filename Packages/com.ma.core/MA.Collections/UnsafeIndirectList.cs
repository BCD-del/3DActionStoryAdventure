// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Unity.Burst;
using Unity.Burst.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace MA.Collections.Unsafe
{
    /// <summary><see cref="Unity.Collections.LowLevel.Unsafe.UntypedUnsafeList"/></summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    unsafe struct UntypedUnsafeList
    {
#pragma warning disable 169
        [NativeDisableUnsafePtrRestriction] public void* Ptr;
        public int m_length;
        public int m_capacity;
        public AllocatorManager.AllocatorHandle Allocator;
        internal int obsolete_length;
        internal int obsolete_capacity;
#pragma warning restore 169
    }

    /// <summary>An unmanaged, resizable list.</summary>
    /// <remarks>This a <see cref="NativeList{T}"/> without the safety handle.</remarks>
    [StructLayout(LayoutKind.Sequential)]
    [DebuggerDisplay("Length = {Length}")]
    [DebuggerTypeProxy(typeof(UnsafeIndirectListDebugView<>))]
#if HAS_PACKAGE_UNITY_COLLECTIONS_2_0_0
    [GenerateTestsForBurstCompatibility(GenericTypeArguments = new[] { typeof(int) })]
#else
    [BurstCompatible]
#endif
    public unsafe struct UnsafeIndirectList<T>
        : INativeDisposable
            , IEnumerable<T> // Used by collection initializers.
        where T : unmanaged
    {
        [NativeDisableUnsafePtrRestriction] public UnsafeList<T>* List;

        /// <summary>Constructs a UnsafeIndirectList from a <see cref="AllocatorManager.AllocatorHandle"/></summary>
        /// <param name="initialCapacity">The initial capacity of the list.</param>
        /// <param name="allocator">The allocator to use.</param>
        /// <param name="options">Whether newly allocated bytes should be zeroed out.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeIndirectList(int initialCapacity, AllocatorManager.AllocatorHandle allocator, NativeArrayOptions options = NativeArrayOptions.UninitializedMemory)
        {
            initialCapacity = math.max(initialCapacity, 4); // Don't allow a capacity of 0.
            List = UnsafeList<T>.Create(initialCapacity, allocator, options);
        }

        /// <summary>Constructs a UnsafeIndirectList from a <see cref="AllocatorManager.AllocatorHandle"/></summary>
        /// <param name="allocator">The allocator to use.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeIndirectList(AllocatorManager.AllocatorHandle allocator)
            : this(4, allocator) { }

        /// <summary>Constructs a UnsafeIndirectList from a <see cref="UnsafeList{T}"/> pointer.</summary>
        /// <remarks>Allows modifying the list via typed methods.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeIndirectList(UnsafeList<T>* list)
        {
            List = list;
        }

        /// <summary>Returns the pointer to the internal list's buffer.</summary>
        public T* Ptr
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => List->Ptr;
        }

        /// <summary>The element at a given index.</summary>
        /// <param name="index">An index into this list.</param>
        /// <exception cref="IndexOutOfRangeException">Thrown if `index` is out of bounds.</exception>
        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                CheckIndexInRange(index, Length);
                return ref List->Ptr[AssumePositive(index)];
            }
        }

        /// <summary>Returns the last element of this list.</summary>
        public ref T Last
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                CheckIndexInRange(Length - 1, Length);
                return ref List->Ptr[Length - 1];
            }
        }

        /// <summary>The count of elements.</summary>
        /// <value>The current count of elements. Always less than or equal to the capacity.</value>
        /// <remarks>To decrease the memory used by a list, set <see cref="Capacity"/> after reducing the length of the list.</remarks>
        /// <param name="value>">The new length. If the new length is greater than the current capacity, the capacity is increased.</param>
        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => List != null ? AssumePositive(List->Length) : 0;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => List->Resize(value, NativeArrayOptions.ClearMemory);
        }

        /// <summary>The number of elements that fit in the current allocation.</summary>
        /// <value>The number of elements that fit in the current allocation.</value>
        /// <param name="value">The new capacity. Must be greater or equal to the length.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the new capacity is smaller than the length.</exception>
        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => List->Capacity;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => List->Capacity = value;
        }

        /// <summary>Whether this list is empty.</summary>
        /// <value>True if the list is empty or if the list has not been constructed.</value>
        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => !IsCreated || Length == 0;
        }

        /// <summary>Whether this list has been allocated (and not yet deallocated).</summary>
        /// <value>True if this list has been allocated (and not yet deallocated).</value>
        public bool IsCreated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => List != null;
        }

        /// <summary>Releases all resources (memory and safety handles).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (IsCreated)
                UnsafeList<T>.Destroy(List);

            List = null;
        }

        /// <summary>Creates and schedules a job that releases all resources (memory and safety handles) of this list.</summary>
        /// <param name="inputDeps">The dependency for the new job.</param>
        /// <returns>The handle of the new job. The job depends upon `inputDeps` and releases all resources of this list.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JobHandle Dispose(JobHandle inputDeps)
        {
            if (!IsCreated) return inputDeps;
            JobHandle jobHandle = new UnsafeIndirectListDisposeJob { Data = new UnsafeIndirectListDispose { ListData = (UntypedUnsafeList*)List } }.Schedule(inputDeps);
            List = null;
            return jobHandle;
        }

        /// <summary>Returns true if the index is a valid for the list, false otherwise.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsValidIndex(int index) => IsCreated && index >= 0 && index < Length;

        /// <summary>Ensures that the list has enough capacity to hold <paramref name="count"/> items.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reserve(int count)
        {
            if (count > List->Capacity)
                List->Capacity = count;
        }

        /// <summary>Ensures the list has enough capacity to hold an additional <paramref name="count"/> of items.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReserveAdditional(int count) => Reserve(Length + count);

        /// <summary>Appends an element to the end of this list.</summary>
        /// <param name="value">The value to add to the end of this list.</param>
        /// <remarks>Length is incremented by 1. Will not increase the capacity.</remarks>
        /// <exception cref="Exception">Thrown if incrementing the length would exceed the capacity.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddNoResize(T value) => List->AddNoResize(value);

        /// <summary>Appends elements from a buffer to the end of this list.</summary>
        /// <param name="ptr">The buffer to copy from.</param>
        /// <param name="count">The number of elements to copy from the buffer.</param>
        /// <remarks>Length is increased by the count. Will not increase the capacity.</remarks>
        /// <exception cref="Exception">Thrown if the increased length would exceed the capacity.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRangeNoResize(void* ptr, int count)
        {
            CheckArgPositive(count);
            List->AddRangeNoResize(ptr, count);
        }

        /// <summary>Appends the elements of another list to the end of this list.</summary>
        /// <param name="list">The other list to copy from.</param>
        /// <remarks>Length is increased by the length of the other list. Will not increase the capacity.</remarks>
        /// <exception cref="Exception">Thrown if the increased length would exceed the capacity.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRangeNoResize(UnsafeIndirectList<T> list) => List->AddRangeNoResize(*list.List);

        /// <summary>Appends an element to the end of this list.</summary>
        /// <param name="value">The value to add to the end of this list.</param>
        /// <remarks>Length is incremented by 1. If necessary, the capacity is increased.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(in T value) => List->Add(value);

        /// <summary>Appends the elements of an array to the end of this list.</summary>
        /// <param name="array">The array to copy from.</param>
        /// <remarks>Length is increased by the number of new elements. Does not increase the capacity.</remarks>
        /// <exception cref="Exception">Thrown if the increased length would exceed the capacity.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange(NativeArray<T> array) => AddRange(array.GetUnsafeReadOnlyPtr(), array.Length);

        /// <summary>Appends the elements of an span to the end of this list.</summary>
        /// <param name="span">The span to copy from.</param>
        /// <remarks>Length is increased by the number of new elements. Does not increase the capacity.</remarks>
        /// <exception cref="Exception">Thrown if the increased length would exceed the capacity.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange(ReadOnlySpan<T> span)
        {
            fixed (T* spanPtr = span)
                AddRange(spanPtr, span.Length);
        }

        /// <summary>Appends the elements of a buffer to the end of this list.</summary>
        /// <param name="ptr">The buffer to copy from.</param>
        /// <param name="count">The number of elements to copy from the buffer.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if count is negative.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange(void* ptr, int count)
        {
            CheckArgPositive(count);
            List->AddRange(ptr, AssumePositive(count));
        }

        /// <summary>Fill the list with a value up to the length.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fill(T value) => UnsafeUtility.MemCpyReplicate(List->Ptr, &value, UnsafeUtility.SizeOf<T>(), List->Length);

        /// <summary>Fill the list with zero up to the length.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillZero() => UnsafeUtility.MemClear(List->Ptr, UnsafeUtility.SizeOf<T>() * List->Length);

        /// <summary>Inserts the given value at the given index, shifting all elements after it to the right.
        /// All new elements are assigned `initialValue`.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Insert(int index, T value, T initialValue = default)
        {
            int length = Length;
            if (index == length)
            {
                Add(value);
            }
            else if (index > length)
            {
                Length = index;
                Add(value);

                for (int i = length; i < index; ++i)
                    List->Ptr[i] = initialValue;
            }
            else
            {
                InsertRangeWithBeginEnd(index, index + 1);
                List->Ptr[index] = value;
            }
        }

        /// <summary>Shifts elements toward the end of this list, increasing its length.</summary>
        /// <param name="begin">The index of the first element that will be shifted up.</param>
        /// <param name="end">The index where the first shifted element will end up.</param>
        /// <exception cref="ArgumentException">Thrown if `end &lt; begin`.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if `begin` or `end` are out of bounds.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InsertRangeWithBeginEnd(int begin, int end) => List->InsertRangeWithBeginEnd(AssumePositive(begin), AssumePositive(end));

        /// <summary>Pops the last element off this list.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Pop()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (Length == 0)
                throw new InvalidOperationException("Trying to pop from an empty list");
#endif

            T value = List->Ptr[Length - 1];
            RemoveAtSwapBack(Length - 1);
            return value;
        }

        /// <summary>Copies the last element of this list to the specified index. Decrements the length by 1.</summary>
        /// <remarks>Useful as a cheap way to remove an element from this list when you don't care about preserving order.</remarks>
        /// <param name="index">The index to overwrite with the last element.</param>
        /// <exception cref="IndexOutOfRangeException">Thrown if `index` is out of bounds.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAtSwapBack(int index) => List->RemoveAtSwapBack(AssumePositive(index));

        /// <summary>Copies the last *N* elements of this list to a range in this list. Decrements the length by *N*.</summary>
        /// <remarks>Copies the last `count` elements to the indexes `index` up to `index + count`.</remarks>
        /// <param name="index">The index of the first element to overwrite.</param>
        /// <param name="count">The number of elements to copy and remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if `index` is out of bounds, `count` is negative,
        /// or `index + count` exceeds the length.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveRangeSwapBack(int index, int count) => List->RemoveRangeSwapBack(AssumePositive(index), AssumePositive(count));

        /// <summary>Removes the element at an index, shifting everything above it down by one. Decrements the length by 1.</summary>
        /// <param name="index">The index of the item to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if `index` is out of bounds.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index) => List->RemoveAt(AssumePositive(index));

        /// <summary>Removes *N* elements in a range, shifting everything above the range down by *N*. Decrements the length by *N*.</summary>
        /// <param name="index">The index of the first element to remove.</param>
        /// <param name="count">The number of elements to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if `index` is out of bounds, `count` is negative, or `index + count` exceeds the length.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveRange(int index, int count) => List->RemoveRange(index, count);

        /// <summary>Sets the length to 0.</summary>
        /// <remarks>Does not change the capacity.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => List->Clear();

        /// <summary>Returns a subarray view of this array.</summary>
        /// <param name="start">The start index.</param>
        /// <param name="length">The number of elements.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray<T> GetSubArray(int start, int length)
        {
            CheckIndexCount(start, length);
            NativeArray<T> array = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(List->Ptr + start, length, Allocator.None);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref array, AtomicSafetyHandle.GetTempUnsafePtrSliceHandle());
#endif
            return array;
        }

        /// <summary>Transfers ownership of the <see cref="NativeList{T}"/> to a <see cref="NativeArray{T}"/>.</summary>
        /// <remarks>This is useful for returning a read-only array from a function that requires a list to operate on.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray<T> TransferOwnershipToNativeArray()
        {
            NativeArray<T> array = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(List->Ptr, List->Length, List->Allocator.ToAllocator);
            List->Allocator = AllocatorManager.None;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            Dispose();
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref array, AtomicSafetyHandle.Create()); // Assign a new handle
#endif
            return array;
        }

        /// <summary>Returns a native array that aliases the content of this list.</summary>
        /// <returns>A native array that aliases the content of this list.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray<T> AsArray() => GetSubArray(0, List->Length);

        /// <summary>Returns a native array that aliases the content of this list.</summary>
        /// <returns>A native array that aliases the content of this list.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeArray<T> AsUnsafeArray() => new UnsafeArray<T>(List->Ptr, List->Length);

        /// <summary>Returns a managed array that contains a copy of this list's content.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToArray() => Length == 0 ? Array.Empty<T>() : AsArray().ToArray();

        /// <summary>Returns a <see cref="Span{T}"/> that aliases the content of this list.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan() => new Span<T>(List->Ptr, List->Length);

        /// <summary>Returns a <see cref="ReadOnlySpan{T}"/> that aliases the content of this list.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan() => new ReadOnlySpan<T>(List->Ptr, List->Length);

        /// <summary>Returns an array that aliases this list. The length of the array is updated when the length of this array is updated in a prior job.</summary>
        /// <remarks>Useful when a job populates a list that is then used by another job.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray<T> AsDeferredJobArray()
        {
            byte* buffer = (byte*)List;
            // We use the first bit of the pointer to infer that the array is in list mode
            // Thus the job scheduling code will need to patch it.
            buffer += 1;
            NativeArray<T> array = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(buffer, 0, Allocator.Invalid);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref array, AtomicSafetyHandle.Create());
#endif
            return array;
        }

        /// <summary>Returns an array containing a copy of this list's content.</summary>
        /// <param name="allocator">The allocator to use.</param>
        /// <returns>An array containing a copy of this list's content.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray<T> ToArray(AllocatorManager.AllocatorHandle allocator)
        {
            NativeArray<T> result = CollectionHelper.CreateNativeArray<T>(Length, allocator, NativeArrayOptions.UninitializedMemory);
            result.CopyFrom(AsArray());
            return result;
        }

        /// <summary>Returns an enumerator over the elements of this list.</summary>
        /// <returns>An enumerator over the elements of this list.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() => new Enumerator(this);

        /// <summary>Returns an enumerator over the elements of this list.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>Returns an enumerator over the elements of this list.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

        /// <summary>Overwrites the elements of this list with the elements of an equal-length array.</summary>
        /// <param name="array">An array to copy into this list.</param>
        /// <exception cref="ArgumentException">Thrown if the array and list have unequal length.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(NativeArray<T> array) => CopyFrom(array.GetUnsafeReadOnlyPtrT(), array.Length);

        /// <summary>Overwrites the elements of this list with the elements of an equal-length array.</summary>
        /// <param name="array">An array to copy into this list.</param>
        /// <exception cref="ArgumentException">Thrown if the array and list have unequal length.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(UnsafeArray<T> array) => CopyFrom(array.Ptr, array.Length);

        /// <summary>Overwrites the elements of this list with the elements of an equal-length array.</summary>
        /// <param name="list">An array to copy into this list.</param>
        /// <exception cref="ArgumentException">Thrown if the array and list have unequal length.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(UnsafeList<T> list) => CopyFrom(list.Ptr, list.Length);

        /// <summary>Overwrites the elements of this list with the elements of an equal-length array.</summary>
        /// <param name="list">An array to copy into this list.</param>
        /// <exception cref="ArgumentException">Thrown if the array and list have unequal length.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(UnsafeIndirectList<T> list) => CopyFrom(list.Ptr, list.Length);

        /// <summary>Overwrites the elements of this list with the elements of an equal-length array.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(void* src, int srcCount)
        {
            Clear();

            if (srcCount > 0)
            {
                Resize(srcCount, NativeArrayOptions.UninitializedMemory);
                UnsafeUtility.MemCpy(List->Ptr, src, srcCount * UnsafeUtility.SizeOf<T>());
            }
        }

        /// <summary>Sets the length of this list, increasing the capacity if necessary.</summary>
        /// <param name="length">The new length of this list.</param>
        /// <param name="options">Whether to clear any newly allocated bytes to all zeroes.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize(int length, NativeArrayOptions options = NativeArrayOptions.ClearMemory) => List->Resize(length, options);

        /// <summary>Sets the length of this list, increasing the capacity if necessary.</summary>
        /// <remarks>Does not clear newly allocated bytes.</remarks>
        /// <param name="length">The new length of this list.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResizeUninitialized(int length) => Resize(length, NativeArrayOptions.UninitializedMemory);

        /// <summary>Sets the capacity.</summary>
        /// <param name="capacity">The new capacity.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCapacity(int capacity) => List->SetCapacity(capacity);

        /// <summary>Sets the capacity to match the length.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TrimExcess() => List->TrimExcess();

        public struct Enumerator : IEnumerator<T>
        {
            [NativeDisableUnsafePtrRestriction] readonly T* m_Buffer;
            readonly int m_Length;
            int m_Index;
            T m_Value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Enumerator(in UnsafeIndirectList<T> list)
            {
                m_Buffer = list.List->Ptr;
                m_Length = list.List->Length;
                m_Index = -1;
                m_Value = default;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose() { }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                m_Index++;
                if (m_Index < m_Length)
                {
                    m_Value = m_Buffer[m_Index];
                    return true;
                }

                m_Value = default;
                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset()
            {
                m_Index = -1;
            }

            public T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => m_Value;
            }

            object IEnumerator.Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => Current;
            }
        }

        /// <summary>Returns a parallel writer of this list.</summary>
        /// <returns>A parallel writer of this list.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ParallelWriter AsParallelWriter() => new ParallelWriter(List);

        /// <summary>A parallel writer for a UnsafeIndirectList.</summary>
        /// <remarks>Use <see cref="AsParallelWriter"/> to create a parallel writer for a list.</remarks>
        [NativeContainerIsAtomicWriteOnly]
#if HAS_PACKAGE_UNITY_COLLECTIONS_2_0_0
        [GenerateTestsForBurstCompatibility(GenericTypeArguments = new[] { typeof(int) })]
#else
        [BurstCompatible]
#endif
        public struct ParallelWriter
        {
            /// <summary>The data of the list.</summary>
            public readonly void* Ptr => ListData->Ptr;

            /// <summary>The internal unsafe list.</summary>
            /// <value>The internal unsafe list.</value>
            [NativeDisableUnsafePtrRestriction] public UnsafeList<T>* ListData;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal ParallelWriter(UnsafeList<T>* listData) => ListData = listData;

            public bool IsCreated
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ListData != null;
            }

            /// <summary>Appends an element to the end of this list.</summary>
            /// <param name="value">The value to add to the end of this list.</param>
            /// <remarks>Increments the length by 1 unless doing so would exceed the current capacity.</remarks>
            /// <exception cref="Exception">Thrown if adding an element would exceed the capacity.</exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void AddNoResize(T value)
            {
                int idx = Interlocked.Increment(ref ListData->m_length) - 1;
                CheckSufficientCapacity(ListData->Capacity, idx + 1);
                UnsafeUtility.WriteArrayElement(ListData->Ptr, idx, value);
            }

            /// <summary>Appends elements from a buffer to the end of this list.</summary>
            /// <param name="ptr">The buffer to copy from.</param>
            /// <param name="count">The number of elements to copy from the buffer.</param>
            /// <remarks>Increments the length by `count` unless doing so would exceed the current capacity.</remarks>
            /// <exception cref="Exception">Thrown if adding the elements would exceed the capacity.</exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void AddRangeNoResize(void* ptr, int count)
            {
                CheckArgPositive(count);

                int idx = Interlocked.Add(ref ListData->m_length, count) - count;
                CheckSufficientCapacity(ListData->Capacity, idx + count);

                int sizeOf = sizeof(T);
                void* dst = (byte*)ListData->Ptr + idx * sizeOf;
                UnsafeUtility.MemCpy(dst, ptr, count * sizeOf);
            }

            /// <summary>Appends the elements of another list to the end of this list.</summary>
            /// <param name="list">The other list to copy from.</param>
            /// <remarks>Increments the length of this list by the length of the other list unless doing so would exceed the current
            /// capacity.</remarks>
            /// <exception cref="Exception">Thrown if adding the elements would exceed the capacity.</exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void AddRangeNoResize(UnsafeList<T> list) => AddRangeNoResize(list.Ptr, list.Length);

            /// <summary>Appends the elements of another list to the end of this list.</summary>
            /// <param name="list">The other list to copy from.</param>
            /// <remarks>Increments the length of this list by the length of the other list unless doing so would exceed the current
            /// capacity.</remarks>
            /// <exception cref="Exception">Thrown if adding the elements would exceed the capacity.</exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void AddRangeNoResize(UnsafeIndirectList<T> list) => AddRangeNoResize(*list.List);
        }

        /// <summary>Tell Burst that an integer can be assumed to map to an always positive value.</summary>
        /// <param name="value">The integer that is always positive.</param>
        /// <returns>Returns `x`, but allows the compiler to assume it is always positive.</returns>
        [return: AssumeRange(0, int.MaxValue)]
        internal static int AssumePositive(int value) => value;

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void CheckIndexCount(int index, int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException($"Value for count {count} must be positive.");
            if (index < 0)
                throw new IndexOutOfRangeException($"Value for index {index} must be positive.");
            if (index >= Length)
                throw new IndexOutOfRangeException($"Value for index {index} is out of bounds.");
            if (index + count > Length)
                throw new ArgumentOutOfRangeException($"Value for count {count} is out of bounds.");
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void CheckIndexInRange(int index, int length)
        {
            if (index < 0)
                throw new IndexOutOfRangeException($"Index {index} must be positive.");
            if (index >= length)
                throw new IndexOutOfRangeException($"Index {index} is out of range in container of '{length}' Length.");
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void CheckSufficientCapacity(int capacity, int length)
        {
            if (capacity < length)
                throw new Exception($"Length {length} exceeds capacity Capacity {capacity}");
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void CheckArgPositive(int value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException($"Value {value} must be positive.");
        }
    }

#if HAS_PACKAGE_UNITY_COLLECTIONS_2_0_0
    [GenerateTestsForBurstCompatibility]
#else
    [BurstCompatible]
#endif
    unsafe struct UnsafeIndirectListDispose
    {
        [NativeDisableUnsafePtrRestriction] public UntypedUnsafeList* ListData;

        public void Dispose()
        {
            UnsafeList<int>* listData = (UnsafeList<int>*)ListData;
            UnsafeList<int>.Destroy(listData);
        }
    }

    [BurstCompile]
#if HAS_PACKAGE_UNITY_COLLECTIONS_2_0_0
    [GenerateTestsForBurstCompatibility]
#else
    [BurstCompatible]
#endif
    struct UnsafeIndirectListDisposeJob : IJob
    {
        internal UnsafeIndirectListDispose Data;

        public void Execute()
        {
            Data.Dispose();
        }
    }

    sealed class UnsafeIndirectListDebugView<T> where T : unmanaged
    {
        UnsafeIndirectList<T> m_Array;

        public UnsafeIndirectListDebugView(UnsafeIndirectList<T> array)
        {
            m_Array = array;
        }

        public T[] Items => m_Array.AsArray().ToArray();
    }

    /// <summary>Provides unsafe utility methods for UnsafeIndirectList.</summary>
#if HAS_PACKAGE_UNITY_COLLECTIONS_2_0_0
    [GenerateTestsForBurstCompatibility]
#else
    [BurstCompatible]
#endif
    public static unsafe class UnsafeIndirectListUnsafeUtility
    {
        /// <summary>Returns the index of the first occurrence of a value in a list.</summary>
        /// <param name="list">The list to search.</param>
        /// <param name="value">The value to search for.</param>
        /// <returns>The index of the first occurrence of the value in the list, or -1 if the value is not found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T>(this UnsafeIndirectList<T> list, T value) where T : unmanaged, IEquatable<T>
            => global::Unity.Collections.NativeArrayExtensions.IndexOf<T, T>(list.List->Ptr, list.Length, value);

        /// <summary>Returns true if the list contains a specified value, false otherwise.</summary>
        /// <param name="list">The list to search.</param>
        /// <param name="value">The value to search for.</param>
        /// <returns>True if the list contains the value, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains<T>(this UnsafeIndirectList<T> list, T value) where T : unmanaged, IEquatable<T>
            => list.IndexOf(value) >= 0;

        /// <summary>Finds the first index of an element using a binary search.</summary>
        /// <param name="list">The list to search.</param>
        /// <param name="value">The value to search for.</param>
        /// <returns>The index of the first occurrence of the value in the list, or -1 if the value is not found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BinarySearch<T>(this UnsafeIndirectList<T> list, T value) where T : unmanaged, IEquatable<T>, IComparable<T>
            => NativeSortExtension.BinarySearch(list.List->Ptr, list.Length, value);

        /// <summary>Adds an element to the list in sorted order.</summary>
        /// <param name="list">The list to add to.</param>
        /// <param name="item">The item to add.</param>
        /// <returns>The index where the item was added.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int AddSorted<T>(this UnsafeIndirectList<T> list, T item) where T : unmanaged, IComparable<T>
        {
            int index = NativeSortExtension.BinarySearch(list.List->Ptr, list.Length, item);
            if (index >= 0)
            {
                list.Insert(index, item);
            }
            else
            {
                index = list.Length;
                list.Add(item);
            }

            return index;
        }

        /// <summary>Adds an element to the list in sorted order.</summary>
        /// <param name="list">The list to add to.</param>
        /// <param name="item">The item to add.</param>
        /// <param name="comparer">The comparer to use.</param>
        /// <returns>The index where the item was added.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int AddSorted<T, U>(this UnsafeIndirectList<T> list, T item, U comparer)
            where T : unmanaged
            where U : IComparer<T>
        {
            int index = NativeSortExtension.BinarySearch(list.List->Ptr, list.Length, item, comparer);
            if (index >= 0)
            {
                list.Insert(index, item);
            }
            else
            {
                index = list.Length;
                list.Add(item);
            }

            return index;
        }

        /// <summary>Removes the first occurrence of a value from the list.</summary>
        /// <param name="list">The list to remove from.</param>
        /// <param name="value">The value to remove.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Remove<T>(this UnsafeIndirectList<T> list, T value) where T : unmanaged, IEquatable<T>
        {
            int index = list.IndexOf(value);
            if (index >= 0)
                list.RemoveAt(index);
        }

        /// <summary>Removes the first occurrence of a value from the list, using RemoveAtSwapBack.</summary>
        /// <param name="list">The list to remove from.</param>
        /// <param name="value">The value to remove.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveSwapBack<T>(this UnsafeIndirectList<T> list, T value) where T : unmanaged, IEquatable<T>
        {
            int index = list.IndexOf(value);
            if (index >= 0)
                list.RemoveAtSwapBack(index);
        }
    }
}
