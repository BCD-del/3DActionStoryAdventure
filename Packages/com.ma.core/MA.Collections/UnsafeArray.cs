// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Burst.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace MA.Collections.Unsafe
{
    /// <summary>An unmanaged array.</summary>
    /// <remarks>This a <see cref="NativeArray{T}"/> without the safety handle.</remarks>
    [StructLayout(LayoutKind.Sequential)]
    [DebuggerDisplay("Length = {Length}")]
    [DebuggerTypeProxy(typeof(UnsafeArrayDebugView<>))]
#if HAS_PACKAGE_UNITY_COLLECTIONS_2_0_0
    [GenerateTestsForBurstCompatibility(GenericTypeArguments = new[] { typeof(int) })]
#else
    [BurstCompatible]
#endif
    public unsafe struct UnsafeArray<T>
        : INativeDisposable
        , IEnumerable<T> // Used by collection initializers.
        where T : unmanaged
    {
        /// <summary>The internal unsafe pointer of the array.</summary>
        [NativeDisableUnsafePtrRestriction] public T* Ptr;

        /// <summary>The count of elements.</summary>
        public int Length;

        /// <summary>The allocator used to allocate the array.</summary>
        public AllocatorManager.AllocatorHandle Allocator;

        /// <summary>Constructs a UnsafeArray from a <see cref="AllocatorManager.AllocatorHandle"/></summary>
        /// <param name="count">The initial capacity of the array.</param>
        /// <param name="allocator">The allocator to use.</param>
        /// <param name="options">Whether newly allocated bytes should be zeroed out.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeArray(int count, AllocatorManager.AllocatorHandle allocator, NativeArrayOptions options = NativeArrayOptions.ClearMemory)
        {
            Ptr = (T*)AllocatorManager.Allocate(allocator, UnsafeUtility.SizeOf<T>(), UnsafeUtility.AlignOf<T>(), count);
            Length = count;
            Allocator = allocator;

            if (options == NativeArrayOptions.ClearMemory)
            {
                UnsafeUtility.MemClear(Ptr, UnsafeUtility.SizeOf<T>() * Length);
            }
        }

        /// <summary>Constructs a UnsafeArray{T} from a pointer and a length.</summary>
        /// <param name="buffer">The pointer to the buffer.</param>
        /// <param name="length">The length of the buffer.</param>
        /// <param name="allocator">The allocator that was used to allocate the buffer.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeArray(T* buffer, int length, AllocatorManager.AllocatorHandle allocator)
        {
            Ptr = buffer;
            Length = length;
            Allocator = allocator;
        }

        /// <summary>Constructs a UnsafeArray{T} from a pointer and a length.</summary>
        /// <param name="buffer">The pointer to the buffer.</param>
        /// <param name="length">The length of the buffer.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeArray(T* buffer, int length)
            : this(buffer, length, AllocatorManager.None)
        {
        }

        /// <summary>Returns true if the index is a valid for the array, false otherwise.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsValidIndex(int index) => IsCreated && index >= 0 && index < Length;

        /// <summary>The element at a given index.</summary>
        /// <param name="index">An index into this array.</param>
        /// <value>The value to store at the `index`.</value>
        /// <exception cref="IndexOutOfRangeException">Thrown if `index` is out of bounds.</exception>
        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                CheckIndexInRange(index, Length);
                return ref Ptr[AssumePositive(index)];
            }
        }

        /// <summary>Fill the array with a value up to the length.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fill(T value) => UnsafeUtility.MemCpyReplicate(Ptr, &value, UnsafeUtility.SizeOf<T>(), Length);

        /// <summary>Fill the array with a value up to the length.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillRange(T value, int startIndex, int length = -1)
        {
            if (length < 0) length = Length - startIndex;
            CheckIndexCount(startIndex, length);
            UnsafeUtility.MemCpyReplicate(Ptr + startIndex, &value, UnsafeUtility.SizeOf<T>(), length);
        }

        /// <summary>Fill the array with zero up to the length.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillZero() => UnsafeUtility.MemClear(Ptr, UnsafeUtility.SizeOf<T>() * Length);

        /// <summary>Whether this array is empty.</summary>
        /// <value>True if the array is empty or if the array has not been constructed.</value>
        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => !IsCreated || Length == 0;
        }

        /// <summary>Whether this array has been allocated (and not yet deallocated).</summary>
        /// <value>True if this array has been allocated (and not yet deallocated).</value>
        public bool IsCreated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Ptr != null;
        }

        /// <summary>Releases all resources (memory and safety handles).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (!IsCreated)
                return;

            if (CollectionUtility.ShouldDeallocate(Allocator))
            {
                AllocatorManager.Free(Allocator, Ptr);
                Allocator = AllocatorManager.Invalid;
            }

            Ptr = null;
        }

        /// <summary>Creates and schedules a job that releases all resources (memory and safety handles) of this array.</summary>
        /// <param name="inputDeps">The dependency for the new job.</param>
        /// <returns>The handle of the new job. The job depends upon `inputDeps` and releases all resources (memory and safety handles) of this array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JobHandle Dispose(JobHandle inputDeps)
        {
            if (!IsCreated)
                return inputDeps;

            if (CollectionUtility.ShouldDeallocate(Allocator))
            {
                inputDeps = new UnsafeArrayDisposeJob { Data = new UnsafeArrayDispose { Ptr = Ptr, Allocator = Allocator } }.Schedule(inputDeps);
                Allocator = AllocatorManager.Invalid;
            }

            Ptr = null;
            return inputDeps;
        }

        /// <summary>Returns a subarray view of this array.</summary>
        /// <param name="start">The start index.</param>
        /// <param name="length">The number of elements.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeArray<T> GetSubArray(int start, int length)
        {
            CheckIndexCount(start, length);
            return new UnsafeArray<T>
            {
                Ptr = Ptr + start,
                Length = length,
                Allocator = AllocatorManager.None
            };
        }

        /// <summary>Returns a native array that aliases the content of this array.</summary>
        /// <returns>A native array that aliases the content of this array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray<T> AsNativeArray()
        {
            NativeArray<T> array = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(Ptr, Length, Unity.Collections.Allocator.None);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref array, AtomicSafetyHandle.Create());
#endif
            return array;
        }

        /// <summary>Returns a managed array that contains a copy of this list's content.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToArray()
        {
            if (!IsCreated || Length == 0)
                return Array.Empty<T>();

            return AsNativeArray().ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeArray<U> Reinterpret<U>() where U : unmanaged
        {
            CheckReinterpretSize<U>();
            return new UnsafeArray<U>((U*)Ptr, Length);
        }

        /// <summary>Returns a <see cref="Span{T}"/> that aliases the content of this array.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan() => new Span<T>(Ptr, Length);

        /// <summary>Returns a <see cref="ReadOnlySpan{T}"/> that aliases the content of this array.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan() => new ReadOnlySpan<T>(Ptr, Length);

        /// <summary>Returns a <see cref="ReadOnlySpan{T}"/> that aliases the content of this array.</summary>
        /// <param name="index">The start index.</param>
        /// <param name="count">The number of elements.</param>
        /// <exception cref="IndexOutOfRangeException">Thrown if the index or count is out of bounds.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan(int index, int count)
        {
            CheckIndexCount(index, count);
            return new ReadOnlySpan<T>(Ptr + index, count);
        }

        /// <summary>Implicitly converts this array to a <see cref="Span{T}"/> that aliases the internal buffer of this array.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Span<T>(UnsafeArray<T> array) => array.AsSpan();

        /// <summary>Implicitly converts this array to a <see cref="ReadOnlySpan{T}"/> that aliases the internal buffer of this array.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<T>(UnsafeArray<T> array) => array.AsReadOnlySpan();

        /// <summary>Returns an array containing a copy of this array's content.</summary>
        /// <param name="allocator">The allocator to use.</param>
        /// <returns>An array containing a copy of this array's content.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray<T> ToArray(AllocatorManager.AllocatorHandle allocator)
        {
            NativeArray<T> result = CollectionHelper.CreateNativeArray<T>(Length, allocator, NativeArrayOptions.UninitializedMemory);
            result.CopyFrom(AsNativeArray());
            return result;
        }

        /// <summary>Returns an enumerator over the elements of this array.</summary>
        /// <returns>An enumerator over the elements of this array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() => new Enumerator(this);

        /// <summary>Returns an enumerator over the elements of this array.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>Returns an enumerator over the elements of this array.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

        /// <summary>Overwrites the elements of this array with the elements of an equal-length array.</summary>
        /// <param name="array">An array to copy into this array.</param>
        /// <exception cref="ArgumentException">Thrown if the array and array have unequal length.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(UnsafeArray<T> array)
        {
            CheckSufficientCapacity(Length, array.Length);
            UnsafeUtility.MemCpy(Ptr, array.Ptr, UnsafeUtility.SizeOf<T>() * array.Length);
        }

        /// <summary>Overwrites the elements of this array with the elements of an equal-length array.</summary>
        /// <param name="array">An array to copy into this array.</param>
        /// <exception cref="ArgumentException">Thrown if the array and array have unequal length.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(NativeArray<T> array)
        {
            CheckSufficientCapacity(Length, array.Length);
            UnsafeUtility.MemCpy(Ptr, array.GetUnsafePtr(), UnsafeUtility.SizeOf<T>() * array.Length);
        }

        /// <summary>Overwrites the elements of this array with the elements of an equal-length array.</summary>
        /// <param name="span">A span to copy into this array.</param>
        /// <exception cref="ArgumentException">Thrown if the array and array have unequal length.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(ReadOnlySpan<T> span)
        {
            CheckSufficientCapacity(Length, span.Length);
            fixed (T* spanPtr = span)
            {
                UnsafeUtility.MemCpy(Ptr, spanPtr, UnsafeUtility.SizeOf<T>() * span.Length);
            }
        }

        /// <summary>The enumerator for a UnsafeArray.</summary>
        public struct Enumerator : IEnumerator<T>
        {
            [NativeDisableUnsafePtrRestriction]
            readonly T*  m_Buffer;
            readonly int m_Length;
            int m_Index;
            T m_Value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Enumerator(in UnsafeArray<T> array)
            {
                m_Buffer = array.Ptr;
                m_Length = array.Length;
                m_Index = -1;
                m_Value = default;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
            }

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

        /// <summary>Returns a read-only view of the UnsafeArray.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnly AsReadOnly() => new ReadOnly(Ptr, Length);

        /// <summary>A read-only view of a UnsafeArray.</summary>
        [StructLayout(LayoutKind.Sequential)]
        [DebuggerDisplay("Length = {Length}")]
        public struct ReadOnly : IEnumerable<T>
        {
            [NativeDisableUnsafePtrRestriction]
            internal T*  m_Buffer;
            internal int m_Length;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnly(T* buffer, int length)
            {
                m_Buffer = buffer;
                m_Length = length;
            }

            public readonly T* Buffer
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => m_Buffer;
            }

            public int Length
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => m_Length;
            }

            public bool IsCreated
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => m_Buffer != null;
            }

            public ref readonly T this[int index]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    CheckIndexInRange(index, m_Length);
                    return ref m_Buffer[AssumePositive(index)];
                }
            }


            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public UnsafeArray<T>.ReadOnly GetSubArray(int start, int length)
            {
                CheckIndexCount(start, length);
                return new UnsafeArray<T>.ReadOnly
                {
                    m_Buffer = m_Buffer + start,
                    m_Length = length
                };
            }

            /// <summary>Returns true if the index is a valid for the list, false otherwise.</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool IsValidIndex(int index) => m_Buffer != null && index >= 0 && index < m_Length;

            // This method does not copy T, but returns a readonly T.
            // It is marked as unsafe because the value returned by this method can become invalid at any time, for example, if the container was disposed.
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ref readonly T UnsafeElementAt(int index) => ref m_Buffer[index];

            /// <summary>Returns a native array that aliases the content of this array.</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeArray<T> AsArray()
            {
                NativeArray<T> array = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(m_Buffer, m_Length, Unity.Collections.Allocator.None);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref array, AtomicSafetyHandle.Create());
#endif
                return array;
            }

            /// <summary>Returns a <see cref="UnsafeArray{U}.ReadOnly"/> that aliases the content of this array, casted to a different type.</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public UnsafeArray<U>.ReadOnly Reinterpret<U>() where U : unmanaged => new UnsafeArray<U>.ReadOnly((U*)m_Buffer, m_Length);

            /// <summary>Read-only enumerator for a UnsafeArray.ReadOnly.</summary>
            public struct ReadOnlyEnumerator : IEnumerator<T>
            {
                readonly ReadOnly m_Array;
                int m_Index;
                T m_Value;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public ReadOnlyEnumerator(in ReadOnly array)
                {
                    m_Array = array;
                    m_Index = -1;
                    m_Value = default;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Dispose()
                {
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public bool MoveNext()
                {
                    m_Index++;
                    if (m_Index < m_Array.m_Length)
                    {
                        m_Value = m_Array.m_Buffer[m_Index];
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

                // Let NativeArray indexer check for out of range.
                public T Current
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => m_Value;
                }

                object IEnumerator.Current => Current;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnlyEnumerator GetEnumerator() => new ReadOnlyEnumerator(this);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly ReadOnlySpan<T> AsReadOnlySpan() => new ReadOnlySpan<T>(m_Buffer, m_Length);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator ReadOnlySpan<T>(in ReadOnly source) => source.AsReadOnlySpan();

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

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void CheckCopyLengths(int srcLength, int dstLength)
        {
            if (srcLength != dstLength)
                throw new ArgumentException("source and destination length must be the same");
        }

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
        void CheckReinterpretSize<U>() where U : unmanaged
        {
            var tSize = UnsafeUtility.SizeOf<T>();
            var uSize = UnsafeUtility.SizeOf<U>();

            var byteLen = ((long)Length) * tSize;
            var uLen = byteLen / uSize;

            if (uLen * uSize != byteLen)
                throw new InvalidOperationException($"Types {typeof(T)} (array length {Length}) and {typeof(U)} cannot be aliased due to size constraints. The size of the types and lengths involved must line up.");
        }
    }

#if HAS_PACKAGE_UNITY_COLLECTIONS_2_0_0
    [GenerateTestsForBurstCompatibility]
#else
    [BurstCompatible]
#endif
    unsafe struct UnsafeArrayDispose
    {
        [NativeDisableUnsafePtrRestriction] public void* Ptr;

        public AllocatorManager.AllocatorHandle Allocator;

        public void Dispose() => AllocatorManager.Free(Allocator, Ptr);
    }

    [BurstCompile]
#if HAS_PACKAGE_UNITY_COLLECTIONS_2_0_0
    [GenerateTestsForBurstCompatibility]
#else
    [BurstCompatible]
#endif
    struct UnsafeArrayDisposeJob : IJob
    {
        internal UnsafeArrayDispose Data;

        public void Execute() => Data.Dispose();
    }

    sealed class UnsafeArrayDebugView<T> where T : unmanaged
    {
        UnsafeArray<T> m_Array;

        public UnsafeArrayDebugView(UnsafeArray<T> array) => m_Array = array;

        public T[] Items => m_Array.AsNativeArray().ToArray();
    }

    public static unsafe class UnsafeArrayExtensions
    {
        /// <summary>Resizes an array. If an empty array is passed, it will create a new one.</summary>
        /// <typeparam name="T">The type of the array.</typeparam>
        /// <param name="array">Target array to resize.</param>
        /// <param name="newSize">New size of native array to resize.</param>
        /// <param name="allocator">The allocator to use for the new array.</param>
        /// <param name="options">The options to use when creating the new array.</param>
        public static void Resize<T>(this ref UnsafeArray<T> array, int newSize, AllocatorManager.AllocatorHandle allocator, NativeArrayOptions options = NativeArrayOptions.ClearMemory) where T : unmanaged
        {
            if (newSize == array.Length)
                return;

            UnsafeArray<T> newArray = new UnsafeArray<T>(newSize, allocator, options);
            if (array.IsCreated)
            {
                int copyLength = math.min(array.Length, newSize);
                if (copyLength > 0) UnsafeUtility.MemCpy(newArray.Ptr, array.Ptr, array.Length * UnsafeUtility.SizeOf<T>());
                array.Dispose();
            }

            array = newArray;
        }

        /// <summary>Returns true if a particular value is present in this array.</summary>
        /// <typeparam name="T">The type of elements in this array.</typeparam>
        /// <typeparam name="U">The value type.</typeparam>
        /// <param name="array">The array to search.</param>
        /// <param name="value">The value to locate.</param>
        /// <returns>True if the value is present in this array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains<T, U>(this UnsafeArray<T> array, U value) where T : unmanaged, IEquatable<U>
        {
            return IndexOf<T, U>(array.Ptr, array.Length, value) != -1;
        }

        /// <summary>Finds the index of the first occurrence of a particular value in this array.</summary>
        /// <typeparam name="T">The type of elements in this array.</typeparam>
        /// <typeparam name="U">The value type.</typeparam>
        /// <param name="array">The array to search.</param>
        /// <param name="value">The value to locate.</param>
        /// <returns>The index of the first occurrence of value in this array. Returns -1 if no occurrence is found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T, U>(this UnsafeArray<T> array, U value) where T : unmanaged, IEquatable<U>
        {
            return IndexOf<T, U>(array.Ptr, array.Length, value);
        }

        /// <summary>Returns true if a particular value is present in this array.</summary>
        /// <typeparam name="T">The type of elements in this array.</typeparam>
        /// <typeparam name="U">The value type.</typeparam>
        /// <param name="array">The array to search.</param>
        /// <param name="value">The value to locate.</param>
        /// <returns>True if the value is present in this array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains<T, U>(this UnsafeArray<T>.ReadOnly array, U value) where T : unmanaged, IEquatable<U>
        {
            return IndexOf<T, U>(array.m_Buffer, array.m_Length, value) != -1;
        }

        /// <summary>Finds the index of the first occurrence of a particular value in this array.</summary>
        /// <typeparam name="T">The type of elements in this array.</typeparam>
        /// <typeparam name="U">The type of value to locate.</typeparam>
        /// <param name="array">The array to search.</param>
        /// <param name="value">The value to locate.</param>
        /// <returns>The index of the first occurrence of the value in this array. Returns -1 if no occurrence is found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T, U>(this UnsafeArray<T>.ReadOnly array, U value) where T : unmanaged, IEquatable<U>
        {
            return IndexOf<T, U>(array.m_Buffer, array.m_Length, value);
        }

        /// <summary>Finds the index of the first occurrence of a particular value in a buffer.</summary>
        /// <typeparam name="T">The type of elements in the buffer.</typeparam>
        /// <typeparam name="U">The value type.</typeparam>
        /// <param name="ptr">A buffer.</param>
        /// <param name="length">Number of elements in the buffer.</param>
        /// <param name="value">The value to locate.</param>
        /// <returns>The index of the first occurrence of the value in the buffer. Returns -1 if no occurrence is found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T, U>(void* ptr, int length, U value) where T : unmanaged, IEquatable<U>
        {
            for (int i = 0; i != length; i++)
            {
                if (UnsafeUtility.ReadArrayElement<T>(ptr, i).Equals(value))
                    return i;
            }
            return -1;
        }

        /// <summary>Performs a binary search on the array.</summary>
        /// <param name="array">The array to search.</param>
        /// <param name="value">The value to locate.</param>
        /// <returns>The index of the value in the array. Returns -1 if the value is not found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BinarySearch<T>(this UnsafeArray<T> array, T value) where T : unmanaged, IEquatable<T>, IComparable<T>
            => NativeSortExtension.BinarySearch(array.Ptr, array.Length, value);
    }
}
