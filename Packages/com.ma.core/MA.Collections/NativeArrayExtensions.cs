// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using MA.Core.Bridge;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine.Jobs;

namespace MA.Collections
{
    /// <summary>Extension methods for <see cref="NativeArray{T}"/>.</summary>
    public static class NativeArrayExtensions
    {
        /// <summary>Resizes a native array. If an empty native array is passed, it will create a new one.</summary>
        /// <typeparam name="T">The type of the array.</typeparam>
        /// <param name="array">Target array to resize.</param>
        /// <param name="newSize">New size of native array to resize.</param>
        /// <param name="allocator">The allocator to use for the new array.</param>
        /// <param name="options">Clear memory options.</param>
        public static void Resize<T>(this ref NativeArray<T> array, int newSize, Allocator allocator, NativeArrayOptions options = NativeArrayOptions.ClearMemory) where T : struct
        {
            if (newSize == array.Length)
                return;

            NativeArray<T> newArray = new NativeArray<T>(newSize, allocator, options);
            if (array.IsCreated)
            {
                int copyLength = math.min(array.Length, newSize);
                if (copyLength > 0)
                    NativeArray<T>.Copy(array, newArray, copyLength);

                array.Dispose();
            }

            array = newArray;
        }

        /// <summary>Resizes a <see cref="TransformAccessArray"/>.</summary>
        /// <param name="array">Target array to resize.</param>
        /// <param name="newSize">The new size of the array.</param>
        /// <remarks>If an empty <see cref="TransformAccessArray"/> is passed, it will create a new one.</remarks>
        public static void Resize(this ref TransformAccessArray array, int newSize)
        {
            TransformAccessArray newArray = new TransformAccessArray(newSize);
            if (array.isCreated)
            {
                for (int i = 0; i < array.length; ++i)
                    newArray.Add(array[i]);

                array.Dispose();
            }
            array = newArray;
        }

        /// <summary>Returns a read-only sub-array of the specified length starting at the specified index.</summary>
        /// <param name="array">The readonly array to get a sub-array from.</param>
        /// <param name="start">The index to start the sub-array from.</param>
        /// <param name="length">The length of the sub-array.</param>
        public static unsafe NativeArray<T>.ReadOnly GetSubArray<T>(this NativeArray<T>.ReadOnly array, int start, int length) where T : struct
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (start < 0 || start >= array.Length)
                throw new ArgumentOutOfRangeException(nameof(start), $"Start index {start} is out of range (must be between 0 and {(array.Length - 1)}).");

            if (length < 0 || start + length > array.Length)
                throw new ArgumentOutOfRangeException(nameof(length), $"Length {length} is out of range (must be between 0 and {(array.Length - start)}).");
#endif

            NativeArray<T> nativeArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(array.GetUnsafeReadOnlyPtr(), array.Length, Allocator.None);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref nativeArray, AtomicSafetyHandle.Create()); // Assign a new handle
#endif

            return nativeArray.GetSubArray(start, length).AsReadOnly();
        }

        /// <summary>Fill the array with a value up to the length.</summary>
        /// <param name="array">The array to fill.</param>
        /// <param name="value">The value to fill the array with.</param>
        /// <param name="startIndex">The index to start filling the array from.</param>
        /// <param name="length">The number of elements to fill.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Fill<T>(this ref NativeArray<T> array, T value, int startIndex = 0, int length = -1) where T : unmanaged
        {
            if (length < 0) length = array.Length - startIndex;
            UnsafeUtility.MemCpyReplicate((T*)array.GetUnsafePtr() + startIndex, &value, UnsafeUtility.SizeOf<T>(), length);
        }

        /// <summary>Checks if an index is valid in a given NativeArray.</summary>
        /// <param name="array">The NativeArray to check.</param>
        /// <param name="index">The index to check.</param>
        /// <returns>True if index is valid, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidIndex<T>(in this NativeArray<T> array, int index) where T : struct => index >= 0 && index < array.Length;

        /// <summary>Checks if an index is valid in a given read-only NativeArray.</summary>
        /// <param name="readOnlyArray">The read-only NativeArray to check.</param>
        /// <param name="index">The index to check.</param>
        /// <returns>True if index is valid, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidIndex<T>(in this NativeArray<T>.ReadOnly readOnlyArray, int index) where T : struct => index >= 0 && index < readOnlyArray.Length;

        /// <summary>Converts a <see cref="Span{T}"/> to a <see cref="NativeArray{T}"/>.</summary>
        /// <param name="span">The Span to convert.</param>
        /// <param name="allocator">The allocator to use.</param>
        /// <returns>A NativeArray containing the same elements as the Span.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArray<T> ToNativeArray<T>(this Span<T> span, Allocator allocator) where T : unmanaged
        {
            NativeArray<T> array = new NativeArray<T>(span.Length, allocator, NativeArrayOptions.UninitializedMemory);
            array.CopyFrom(span);
            return array;
        }

        /// <summary>Converts a <see cref="ReadOnlySpan{T}"/> to a <see cref="NativeArray{T}"/>.</summary>
        /// <param name="span">The ReadOnlySpan to convert.</param>
        /// <param name="allocator">The allocator to use.</param>
        /// <returns>A NativeArray containing the same elements as the ReadOnlySpan.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArray<T> ToNativeArray<T>(this ReadOnlySpan<T> span, Allocator allocator) where T : unmanaged
        {
            NativeArray<T> array = new NativeArray<T>(span.Length, allocator, NativeArrayOptions.UninitializedMemory);
            array.CopyFrom(span);
            return array;
        }

#if !HAS_PACKAGE_UNITY_COLLECTIONS_2_0_0
        /// <summary>Converts a <see cref="NativeArray{T}"/> to a <see cref="Span{T}"/>.</summary>
        /// <param name="array">The NativeArray to convert.</param>
        /// <returns>A Span containing the same elements as the NativeArray.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Span<T> AsSpan<T>(this NativeArray<T> array) where T : unmanaged => new Span<T>((void*)array.GetUnsafePtr(), array.Length);

        /// <summary>Converts a <see cref="NativeArray{T}"/> to a <see cref="Span{T}"/>, starting at the specified index.</summary>
        /// <param name="array">The NativeArray to convert.</param>
        /// <param name="start">The index to start at.</param>
        /// <param name="length">The number of elements in the Span.</param>
        /// <returns>A Span containing the a subset of the elements in the NativeArray.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Span<T> AsSpan<T>(this NativeArray<T> array, int start, int length) where T : unmanaged => new Span<T>((T*)array.GetUnsafePtr() + start, length);

        /// <summary>Converts a <see cref="NativeArray{T}.ReadOnly"/> to a <see cref="ReadOnlySpan{T}"/>.</summary>
        /// <param name="array">The NativeArray to convert.</param>
        /// <returns>A ReadOnlySpan containing the same elements as the NativeArray.ReadOnly.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ReadOnlySpan<T> AsReadOnlySpan<T>(this NativeArray<T>.ReadOnly array) where T : unmanaged => new ReadOnlySpan<T>((void*)array.GetUnsafeReadOnlyPtr(), array.Length);

        /// <summary>Converts a <see cref="NativeArray{T}"/> to a <see cref="ReadOnlySpan{T}"/>.</summary>
        /// <param name="array">The NativeArray to convert.</param>
        /// <returns>A ReadOnlySpan containing the same elements as the NativeArray.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ReadOnlySpan<T> AsReadOnlySpan<T>(this NativeArray<T> array) where T : unmanaged => new ReadOnlySpan<T>((void*)array.GetUnsafeReadOnlyPtr(), array.Length);

        /// <summary>Converts a <see cref="NativeArray{T}"/> to a <see cref="ReadOnlySpan{T}"/>, starting at the specified index.</summary>
        /// <param name="array">The NativeArray to convert.</param>
        /// <param name="start">The index to start at.</param>
        /// <param name="length">The number of elements in the ReadOnlySpan.</param>
        /// <returns>A ReadOnlySpan containing the a subset of the elements in the NativeArray.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ReadOnlySpan<T> AsReadOnlySpan<T>(this NativeArray<T> array, int start, int length) where T : unmanaged => new ReadOnlySpan<T>((T*)array.GetUnsafeReadOnlyPtr() + start, length);
#endif

        /// <summary>Copies the contents of a <see cref="Span{T}"/> into the specified <see cref="NativeArray{T}"/>.</summary>
        /// <remarks>The length of the <see cref="Span{T}"/> must be equal to the length of the <see cref="NativeArray{T}"/>.</remarks>
        /// <param name="array">The NativeArray to copy into.</param>
        /// <param name="span">The Span to copy from.</param>
        /// <typeparam name="T">The type of elements in the NativeArray and Span.</typeparam>
        /// <exception cref="ArgumentException">Thrown if the length of the Span is not equal to the length of the NativeArray.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void CopyFrom<T>(this NativeArray<T> array, Span<T> span)
            where T : unmanaged
        {
            CheckCopyLengths(array.Length, span.Length);
            fixed (T* ptr = span)
            {
                UnsafeUtility.MemCpy(array.GetUnsafePtr(), ptr, span.Length * UnsafeUtility.SizeOf<T>());
            }
        }

        /// <summary>Copies the contents of a <see cref="ReadOnlySpan{T}"/> into the specified <see cref="NativeArray{T}"/>.</summary>
        /// <remarks>The length of the <see cref="ReadOnlySpan{T}"/> must be equal to the length of the <see cref="NativeArray{T}"/>.</remarks>
        /// <param name="array">The NativeArray to copy into.</param>
        /// <param name="span">The ReadOnlySpan to copy from.</param>
        /// <typeparam name="T">The type of elements in the NativeArray and ReadOnlySpan.</typeparam>
        /// <exception cref="ArgumentException">Thrown if the length of the ReadOnlySpan is not equal to the length of the NativeArray.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void CopyFrom<T>(this NativeArray<T> array, ReadOnlySpan<T> span)
            where T : unmanaged
        {
            CheckCopyLengths(array.Length, span.Length);
            fixed (T* ptr = span)
            {
                UnsafeUtility.MemCpy(array.GetUnsafePtr(), ptr, span.Length * UnsafeUtility.SizeOf<T>());
            }
        }

        /// <summary>Gets a reference to an element of a NativeArray at a specific index.</summary>
        /// <param name="array">The NativeArray to get an element from.</param>
        /// <param name="index">The index of the element.</param>
        /// <returns>A reference to the element at the specified index.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ref T ElementAt<T>(this NativeArray<T> array, int index) where T : struct
        {
            CheckElement(index, array.Length);
            return ref UnsafeUtility.ArrayElementAsRef<T>(array.GetUnsafePtr(), index);
        }

        /// <summary>Gets a read-only reference to an element of a NativeArray at a specific index.</summary>
        /// <param name="array">The NativeArray to get an element from.</param>
        /// <param name="index">The index of the element.</param>
        /// <returns>A read-only reference to the element at the specified index.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ref readonly T ReadOnlyElementAt<T>(this NativeArray<T> array, int index) where T : struct
        {
            CheckElement(index, array.Length);
            return ref UnsafeUtility.ArrayElementAsRef<T>(array.GetUnsafeReadOnlyPtr(), index);
        }

        /// <summary>Returns a typed pointer to the <see cref="NativeArray{T}"/>.</summary>
        /// <seealso cref="NativeArrayUnsafeUtility.GetUnsafePtr{T}(NativeArray{T})"/>
        /// <returns>A typed pointer to the <see cref="NativeArray{T}"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe T* GetUnsafePtrT<T>(this NativeArray<T> array) where T : unmanaged => (T*)array.GetUnsafePtr();

        /// <summary>Returns a `ReadOnly` typed pointer to the <see cref="NativeArray{T}"/>.</summary>
        /// <seealso cref="NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr{T}(NativeArray{T})"/>
        /// <returns>A `ReadOnly` typed pointer to the <see cref="NativeArray{T}"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe T* GetUnsafeReadOnlyPtrT<T>(this NativeArray<T> array) where T : unmanaged => (T*)array.GetUnsafeReadOnlyPtr();

        /// <summary>Returns a pointer to the element at the specified index.</summary>
        /// <seealso cref="NativeArrayUnsafeUtility.GetUnsafePtr{T}(NativeArray{T})"/>
        /// <returns>A pointer to the element at the specified index.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe T* GetUnsafePtrAt<T>(this NativeArray<T> array, int offset) where T : unmanaged => (T*)array.GetUnsafePtr() + offset;

        /// <summary>Returns a `ReadOnly` pointer to the element at the specified index.</summary>
        /// <seealso cref="NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr{T}(NativeArray{T})"/>
        /// <returns>A `ReadOnly` pointer to the element at the specified index.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe T* GetUnsafeReadOnlyPtrAt<T>(this NativeArray<T> array, int offset) where T : unmanaged => (T*)array.GetUnsafeReadOnlyPtr() + offset;

        /// <summary>Returns a `ReadOnly` pointer to the element at the specified index.</summary>
        /// <seealso cref="NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr{T}(NativeArray{T})"/>
        /// <returns>A `ReadOnly` pointer to the element at the specified index.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe T* GetUnsafeReadOnlyPtrAt<T>(this NativeArray<T>.ReadOnly array, int offset) where T : unmanaged => (T*)array.GetUnsafeReadOnlyPtr() + offset;

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void CheckElement(int index, int length)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (index >= length)
                throw new IndexOutOfRangeException($"Index {index} is out of range (must be between 0 and {(length - 1)}).");
#endif
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void CheckCopyLengths(int srcLength, int dstLength)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (srcLength != dstLength)
                throw new ArgumentException("source and destination length must be the same");
#endif
        }
    }
}
