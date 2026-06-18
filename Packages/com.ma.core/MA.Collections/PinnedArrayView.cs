// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MA.Collections.Unsafe;
using Unity.Burst.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace MA.Collections
{
    /// <summary>A view of a pinned managed array of unmanaged types.</summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeContainer]
    [DebuggerDisplay("Length = {Length}")]
    [DebuggerTypeProxy(typeof(PinnedArrayDebugView<>))]
#if HAS_PACKAGE_UNITY_COLLECTIONS_2_0_0
    [GenerateTestsForBurstCompatibility(GenericTypeArguments = new[] { typeof(int) })]
#else
    [BurstCompatible]
#endif
    public unsafe struct PinnedArrayView<T>
        : INativeDisposable
            , IEnumerable<T> // Used by collection initializers.
        where T : unmanaged
    {
        ulong m_ItemsHandle;
        [NativeDisableUnsafePtrRestriction] internal T* m_Items;
        int m_Length;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        internal AtomicSafetyHandle m_Safety;
#if !REMOVE_DISPOSE_SENTINEL
        [NativeSetClassTypeToNullOnSchedule] internal DisposeSentinel m_DisposeSentinel;
#endif
#endif

        /// <summary>Constructs a <see cref="PinnedArrayView{T}"/> from a managed array.</summary>
        public PinnedArrayView(T[] array, int length, int startIndex = 0)
        {
            // Pinning the array will prevent the GC from moving it. This should pin the parent list as well.
            m_Items = (T*)UnsafeUtility.PinGCArrayAndGetDataAddress(array, out m_ItemsHandle) + startIndex;
            m_Length = math.min(length, array.Length);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
#if REMOVE_DISPOSE_SENTINEL
            var safety = AtomicSafetyHandle.Create();
#else
            DisposeSentinel.Create(out AtomicSafetyHandle safety, out DisposeSentinel sentinel, 2, Allocator.None);
            m_DisposeSentinel = sentinel;
#endif
            m_Safety = safety;
#endif
        }

        /// <summary>Constructs a <see cref="PinnedArrayView{T}"/> from a managed array.</summary>
        public PinnedArrayView(T[] array) : this(array, array.Length) { }

        /// <summary>Constructs a <see cref="PinnedArrayView{T}"/> from a <see cref="List{T}"/>.</summary>
        public PinnedArrayView(List<T> list) : this(list.GetInternalArray(), list.Count) { }

        /// <summary>Constructs a <see cref="PinnedArrayView{T}"/> from a <see cref="LeanList{T}"/>.</summary>
        public PinnedArrayView(LeanList<T> list) : this(list.InternalArray, list.Count) { }

        /// <summary>Returns true if the index is a valid for the list, false otherwise.</summary>
        /// <param name="index">The index to check.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool IsValidIndex(int index) => IsCreated && index >= 0 && index < m_Length;

        /// <summary>The element at a given index.</summary>
        /// <param name="index">An index into this list.</param>
        /// <value>The value to store at the `index`.</value>
        /// <exception cref="IndexOutOfRangeException">Thrown if `index` is out of bounds.</exception>
        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
#endif
                return UnsafeUtility.ReadArrayElement<T>(m_Items, index);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            [WriteAccessRequired]
            set
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
                UnsafeUtility.WriteArrayElement(m_Items, index, value);
            }
        }

        /// <summary>Returns true if the <see cref="PinnedArrayView{T}"/> has been created and not yet disposed.</summary>
        public readonly bool IsCreated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Items != null;
        }

        /// <summary>The pointer to the first element.</summary>
        public T* Ptr
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Items;
        }

        /// <summary>The number of elements.</summary>
        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
#endif
                return m_Length;
            }
        }

        /// <summary>Returns a reference to the element at the specified index.</summary>
        /// <param name="index">The index of the element to return.</param>
        /// <returns>A reference to the element at the specified index.</returns>
        public ref T ElementAt(int index)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
            return ref m_Items[index];
        }

        /// <summary>Fill the list with a value up to the length.</summary>
        /// <param name="value">The value to fill the list with.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fill(T value)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
            UnsafeUtility.MemCpyReplicate(m_Items, &value, UnsafeUtility.SizeOf<T>(), Length);
        }

        /// <summary>Fill the list with zero value up to the length.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillZero()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
            UnsafeUtility.MemClear(m_Items, UnsafeUtility.SizeOf<T>() * Length);
        }

        /// <summary>Releases all resources (memory and safety handles).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (m_ItemsHandle != 0)
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                DisposeSentinel.Dispose(ref m_Safety, ref m_DisposeSentinel);
#endif
                UnsafeUtility.ReleaseGCObject(m_ItemsHandle);
            }

            m_ItemsHandle = 0;
            m_Items = null;
            m_Length = 0;
        }

        /// <summary>Creates and schedules a job that releases all resources (memory and safety handles) of this list.</summary>
        /// <param name="inputDeps">The dependency for the new job.</param>
        /// <returns>The handle of the new job. The job depends upon `inputDeps` and releases all resources (memory and safety handles) of this list.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JobHandle Dispose(JobHandle inputDeps)
        {
            JobHandle jobHandle = inputDeps;

            if (m_ItemsHandle != 0)
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
#if !REMOVE_DISPOSE_SENTINEL
                DisposeSentinel.Clear(ref m_DisposeSentinel);
#endif
                jobHandle = new DisposeGCHandleJob { Handle = m_ItemsHandle, Safety = m_Safety }.Schedule(inputDeps);
                AtomicSafetyHandle.Release(m_Safety);
#else
                jobHandle = new DisposeGCHandleJob { Handle = m_ItemsHandle }.Schedule(inputDeps);
#endif
            }

            m_ItemsHandle = 0;
            m_Items = null;
            m_Length = 0;
            return jobHandle;
        }

        /// <summary>Returns a native array that aliases the content of this list.</summary>
        /// <returns>A native array that aliases the content of this list.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly NativeArray<T> AsArray()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckGetSecondaryDataPointerAndThrow(m_Safety);
            AtomicSafetyHandle arraySafety = m_Safety;
            AtomicSafetyHandle.UseSecondaryVersion(ref arraySafety);
#endif
            NativeArray<T> array = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(m_Items, m_Length, Allocator.None);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref array, arraySafety);
#endif
            return array;
        }

        /// <summary>Returns a native array that aliases the content of this list.</summary>
        /// <returns>A native array that aliases the content of this list.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly UnsafeArray<T> AsUnsafeArray() => new UnsafeArray<T>(m_Items, m_Length);

        /// <summary>Returns a <see cref="Span{T}"/> that aliases the content of this list.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<T> AsSpan()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
            return new Span<T>(m_Items, m_Length);
        }

        /// <summary>Returns a <see cref="ReadOnlySpan{T}"/> that aliases the content of this list.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<T> AsReadOnlySpan()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
#endif
            return new ReadOnlySpan<T>(m_Items, m_Length);
        }

        /// <summary>Returns an array containing a copy of this list's content.</summary>
        /// <param name="allocator">The allocator to use.</param>
        /// <returns>An array containing a copy of this list's content.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly NativeArray<T> ToNativeArray(AllocatorManager.AllocatorHandle allocator)
        {
            NativeArray<T> result = CollectionHelper.CreateNativeArray<T>(m_Length, allocator, NativeArrayOptions.UninitializedMemory);
            result.CopyFrom(AsArray());
            return result;
        }

        /// <summary>Compares this <see cref="PinnedArrayView{T}"/> with another for equality.</summary>
        /// <param name="other">The other <see cref="PinnedArrayView{T}"/> to compare with this <see cref="PinnedArrayView{T}"/>.</param>
        /// <returns>True if the two <see cref="PinnedArrayView{T}"/>s are equal, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(PinnedArrayView<T> other) => m_Items == other.m_Items && m_Length == other.m_Length;

        /// <summary>Compares this <see cref="PinnedArrayView{T}"/> with an object for equality.</summary>
        /// <param name="obj">The object to compare with this <see cref="PinnedArrayView{T}"/>.</param>
        /// <returns>True if the object is a <see cref="PinnedArrayView{T}"/> equal to this <see cref="PinnedArrayView{T}"/>, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is PinnedArrayView<T> list && Equals(list);
        }

        /// <summary>Returns the hash code for this list.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override int GetHashCode() => unchecked(((int)m_Items * 397) ^ m_Length);

        /// <summary>Compares two <see cref="PinnedArrayView{T}"/> for equality.</summary>
        /// <param name="left">The first <see cref="PinnedArrayView{T}"/> to compare.</param>
        /// <param name="right">The second <see cref="PinnedArrayView{T}"/> to compare.</param>
        /// <returns>True if the <see cref="PinnedArrayView{T}"/>s are equal, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(PinnedArrayView<T> left, PinnedArrayView<T> right) => left.Equals(right);

        /// <summary>Compares two <see cref="PinnedArrayView{T}"/> for inequality.</summary>
        /// <param name="left">The first <see cref="PinnedArrayView{T}"/> to compare.</param>
        /// <param name="right">The second <see cref="PinnedArrayView{T}"/> to compare.</param>
        /// <returns>True if the <see cref="PinnedArrayView{T}"/>s are not equal, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(PinnedArrayView<T> left, PinnedArrayView<T> right) => !left.Equals(right);

        /// <summary>Returns an enumerator over the elements of this list.</summary>
        /// <returns>An enumerator over the elements of this list.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Enumerator GetEnumerator() => new Enumerator(this);

        /// <summary>Returns an enumerator over the elements of this list.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>Returns an enumerator over the elements of this list.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

        /// <summary>Overwrites the elements of this list with the elements of an equal-length array.</summary>
        /// <param name="array">An array to copy into this list.</param>
        /// <exception cref="ArgumentException">Thrown if the array and list have unequal length.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(NativeArray<T> array) => AsArray().CopyFrom(array);

        public struct Enumerator : IEnumerator<T>
        {
            PinnedArrayView<T> m_List;
            int m_Index;
            T m_Value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Enumerator(in PinnedArrayView<T> list)
            {
                m_List = list;
                m_Index = -1;
                m_Value = default;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose() { }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                m_Index++;
                if (m_Index < m_List.Length)
                {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                    AtomicSafetyHandle.CheckReadAndThrow(m_List.m_Safety);
#endif
                    m_Value = UnsafeUtility.ReadArrayElement<T>(m_List.m_Items, m_Index);
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

        /// <summary>Returns a parallel reader of this list.</summary>
        /// <returns>A parallel reader of this list.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly NativeArray<T>.ReadOnly AsParallelReader() => AsArray().AsReadOnly();

        /// <summary>Tell Burst that an integer can be assumed to map to an always positive value.</summary>
        /// <param name="value">The integer that is always positive.</param>
        /// <returns>Returns `x`, but allows the compiler to assume it is always positive.</returns>
        [return: AssumeRange(0, int.MaxValue)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int AssumePositive(int value) => value;

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

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void CheckBeginEnd(int begin, int end)
        {
            if (begin > end)
                throw new ArgumentException($"Value for begin {begin} index must less or equal to end {end}.");
            if (begin < 0)
                throw new ArgumentOutOfRangeException($"Value for begin {begin} must be positive.");
            if (begin > Length)
                throw new ArgumentOutOfRangeException($"Value for begin {begin} is out of bounds.");
            if (end > Length)
                throw new ArgumentOutOfRangeException($"Value for end {end} is out of bounds.");
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void CheckIndexCount(int index, int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException($"Value for cound {count} must be positive.");
            if (index < 0)
                throw new ArgumentOutOfRangeException($"Value for index {index} must be positive.");
            if (index > Length)
                throw new ArgumentOutOfRangeException($"Value for index {index} is out of bounds.");
            if (index + count > Length)
                throw new ArgumentOutOfRangeException($"Value for count {count} is out of bounds.");
        }
    }

    /// <summary>DebuggerTypeProxy for <see cref="PinnedArrayView{T}"/></summary>
    sealed unsafe class PinnedArrayDebugView<T> where T : unmanaged
    {
        PinnedArrayView<T> m_List;

        public PinnedArrayDebugView(PinnedArrayView<T> list) => m_List = list;

        public T[] Items
        {
            get
            {
                if (!m_List.IsCreated)
                    return default;

                T[] array = new T[m_List.Length];
                fixed (void* arrayPtr = array)
                    UnsafeUtility.MemCpy(arrayPtr, m_List.m_Items, UnsafeUtility.SizeOf<T>() * m_List.Length);

                return array;
            }
        }
    }

    /// <summary>A simple job for freeing a pointer with a safety handle.</summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeContainer]
    public struct DisposeGCHandleJob : IJob
    {
        public ulong Handle;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        public AtomicSafetyHandle Safety;
#endif

        public void Execute()
        {
            UnsafeUtility.ReleaseGCObject(Handle);
        }
    }

    /// <summary>Provides unsafe utility methods for PinnedArray.</summary>
#if HAS_PACKAGE_UNITY_COLLECTIONS_2_0_0
    [GenerateTestsForBurstCompatibility]
#else
    [BurstCompatible]
#endif
    public static unsafe class PinnedArrayUnsafeUtility
    {
        /// <summary>Returns a pointer to this view's internal buffer.</summary>
        /// <remarks>Performs a job safety check for read-write access.</remarks>
        /// <param name="view">The view.</param>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <returns>A pointer to this list's internal buffer.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* GetUnsafePtr<T>(this PinnedArrayView<T> view) where T : unmanaged
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckReadAndThrow(view.m_Safety);
#endif
            return view.m_Items;
        }

        /// <summary>Returns a pointer to this view's internal buffer.</summary>
        /// <remarks>Performs a job safety check for read-write access.</remarks>
        /// <param name="arrayView">The view.</param>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <returns>A pointer to this list's internal buffer.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* GetUnsafePtrT<T>(this PinnedArrayView<T> arrayView) where T : unmanaged
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckReadAndThrow(arrayView.m_Safety);
#endif
            return arrayView.m_Items;
        }

        /// <summary>Returns a pointer to this view's internal buffer.</summary>
        /// <remarks>Performs a job safety check for read-only access.</remarks>
        /// <param name="view">The view.</param>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <returns>A pointer to this list's internal buffer.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* GetUnsafeReadOnlyPtr<T>(this PinnedArrayView<T> view) where T : unmanaged
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(view.m_Safety);
#endif
            return view.m_Items;
        }

        /// <summary>Returns a pointer to this view's internal buffer.</summary>
        /// <remarks>Performs a job safety check for read-only access.</remarks>
        /// <param name="view">The view.</param>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <returns>A pointer to this list's internal buffer.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* GetUnsafeReadOnlyPtrT<T>(this PinnedArrayView<T> view) where T : unmanaged
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(view.m_Safety);
#endif
            return view.m_Items;
        }
    }
}
