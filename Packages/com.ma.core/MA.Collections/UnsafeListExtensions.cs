// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace MA.Collections.Unsafe
{
    public static class UnsafeListExtensions
    {
        /// <summary>Returns true if the index is a valid for the list, false otherwise.</summary>
        /// <param name="list">The list to check the index against.</param>
        /// <param name="index">The index to check.</param>
        /// <returns>True if the index is valid, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidIndex<T>(this UnsafeList<T> list, int index) where T : unmanaged => index >= 0 && index < list.Length;

        /// <summary>Adds `count` uninitialized elements to the end of the list.</summary>
        /// <param name="list">The list to add elements to.</param>
        /// <param name="count">The number of elements to add.</param>
        /// <remarks>Resizes the list to `list.Length + count`.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddUninitialized<T>(ref this UnsafeList<T> list, int count) where T : unmanaged => list.Resize(list.Length + count);

        /// <summary>Fills the list with a value up to the length.</summary>
        /// <param name="list">The list to fill.</param>
        /// <param name="value">The value to fill the list with.</param>
        /// <param name="startIndex">The index to start filling from.</param>
        /// <param name="length">The number of elements to fill.</param>
        /// <remarks>If `length` is less than zero, the entire list is filled.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Fill<T>(this UnsafeList<T> list, T value, int startIndex = 0, int length = -1) where T : unmanaged
        {
            if (length < 0) length = list.Length - startIndex;
            UnsafeUtility.MemCpyReplicate(list.Ptr + startIndex, &value, UnsafeUtility.SizeOf<T>(), length);
        }

        /// <summary>Resizes the list to the given size, and fills all elements with the given value.</summary>
        /// <param name="list">The list to resize and fill.</param>
        /// <param name="initValue">The value to fill the list with.</param>
        /// <param name="count">The number of elements to initialize.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Initialize<T>(ref this UnsafeList<T> list, T initValue, int count)
            where T : unmanaged
        {
            list.Resize(count);
            list.Fill(initValue);
        }

        /// <summary>Reserves capacity in the list if the current capacity is less than the given count.</summary>
        /// <param name="list">The list to reserve capacity in.</param>
        /// <param name="count">The minimum capacity to reserve.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Reserve<T>(ref this UnsafeList<T> list, int count) where T : unmanaged
        {
            if (list.Capacity < count)
                list.Capacity = count;
        }

        /// <summary>Reserves an additional amount of capacity in the list.</summary>
        /// <param name="list">The list to reserve capacity in.</param>
        /// <param name="count">The additional capacity to reserve.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReserveAdditional<T>(ref this UnsafeList<T> list, int count) where T : unmanaged
        {
            int length = list.Length;
            if (list.Capacity < length + count)
                list.Capacity = length + count;
        }

        /// <summary>Copies elements from a <see cref="ReadOnlySpan{T}"/> into the list.</summary>
        /// <param name="self">The target list.</param>
        /// <param name="span">The <see cref="ReadOnlySpan{T}"/> from which elements are sourced.</param>
        /// <typeparam name="T">The type of elements.</typeparam>
        /// <remarks>The list is cleared before copying elements.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyFrom<T>(ref this UnsafeList<T> self, ReadOnlySpan<T> span)
            where T : unmanaged
        {
            self.Clear();
            if (span.Length > 0)
            {
                self.Resize(span.Length);
                span.CopyTo(self.AsSpan());
            }
        }
        
        /// <summary>Inserts the given value at the given index, shifting all elements after it to the right.</summary>
        /// <param name="list">The list to insert into.</param>
        /// <param name="index">The index to insert at.</param>
        /// <param name="value">The value to insert.</param>
        /// <param name="initialValue">The value to fill the list with if the index is greater than the length.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Insert<T>(ref this UnsafeList<T> list, int index, T value, T initialValue = default) where T : unmanaged
        {
            int length = list.Length;
            if (index == length)
            {
                list.Add(value);
            }
            else if (index > length)
            {
                list.Length = index;
                list.Add(value);

                for (int i = length; i < index; ++i)
                    list[i] = initialValue;
            }
            else
            {
                list.InsertRangeWithBeginEnd(index, index + 1);
                list[index] = value;
            }
        }

        /// <summary>Converts a <see cref="UnsafeList{T}"/> to a <see cref="NativeArray{T}"/> without copying the data.</summary>
        /// <param name="list">The list to convert.</param>
        /// <returns>A <see cref="NativeArray{T}"/> that points to the same memory as the list.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArray<T> AsNativeArray<T>(this UnsafeList<T> list)
            where T : unmanaged
        {
            unsafe
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle arraySafety = AtomicSafetyHandle.GetTempUnsafePtrSliceHandle();
                AtomicSafetyHandle.UseSecondaryVersion(ref arraySafety);
#endif
                NativeArray<T> array = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(list.Ptr, list.Length, Allocator.None);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref array, arraySafety);
#endif
                return array;
            }
        }
        
        /// <summary>Converts a <see cref="UnsafeList{T}"/> to a <see cref="UnsafeArray{T}"/> without copying the data.</summary>
        /// <param name="list">The list to convert.</param>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <returns>A <see cref="UnsafeArray{T}"/> that points to the same memory as the list.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe UnsafeArray<T> AsUnsafeArray<T>(this UnsafeList<T> list) where T : unmanaged 
            => new UnsafeArray<T> { Ptr = list.Ptr, Length = list.Length, Allocator = AllocatorManager.None };

        /// <summary>Converts a <see cref="UnsafeList{T}"/> to a <see cref="UnsafeArray{T}"/> without copying the data.</summary>
        /// <param name="list">The list to convert.</param>
        /// <param name="startIndex">The index to start the array at.</param>
        /// <param name="length">The length of the array.</param>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <returns>A <see cref="UnsafeArray{T}"/> that points to the same memory as the list.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe UnsafeArray<T> AsUnsafeArray<T>(this UnsafeList<T> list, int startIndex, int length) where T : unmanaged
        {
            CheckGetSubArrayArguments(list, startIndex, length);
            return new UnsafeArray<T> { Ptr = list.Ptr + startIndex, Length = length, Allocator = AllocatorManager.None };
        }

        /// <summary>Creates a <see cref="Span{T}"/> from a <see cref="NativeList{T}"/> starting at the specified index.</summary>
        /// <param name="list">The list to create the span from.</param>
        /// <returns>A <see cref="Span{T}"/> representing the specified range of the list.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Span<T> AsSpan<T>(this UnsafeList<T> list) where T : unmanaged => new Span<T>(list.Ptr, list.Length);

        /// <summary>Creates a <see cref="Span{T}"/> from a <see cref="NativeList{T}"/> starting at the specified index.</summary>
        /// <param name="list">The list to create the span from.</param>
        /// <param name="start">The index to start the span at.</param>
        /// <param name="length">The length of the span.</param>
        /// <returns>A <see cref="Span{T}"/> representing the specified range of the list.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Span<T> AsSpan<T>(this UnsafeList<T> list, int start, int length) where T : unmanaged
        {
            CheckGetSubArrayArguments(list, start, length);
            return new Span<T>(list.Ptr + start, length);
        }

        /// <summary>Creates a <see cref="ReadOnlySpan{T}"/> from a <see cref="NativeList{T}"/> starting at the specified index.</summary>
        /// <param name="list">The list to create the span from.</param>
        /// <returns>A <see cref="ReadOnlySpan{T}"/> representing the specified range of the list.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ReadOnlySpan<T> AsReadOnlySpan<T>(this UnsafeList<T> list) where T : unmanaged => new ReadOnlySpan<T>(list.Ptr, list.Length);

        /// <summary>Creates a <see cref="ReadOnlySpan{T}"/> from a <see cref="NativeList{T}"/> starting at the specified index.</summary>
        /// <param name="list">The list to create the span from.</param>
        /// <param name="start">The index to start the span at.</param>
        /// <param name="length">The length of the span.</param>
        /// <returns>A <see cref="ReadOnlySpan{T}"/> representing the specified range of the list.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ReadOnlySpan<T> AsReadOnlySpan<T>(this UnsafeList<T> list, int start, int length) where T : unmanaged
        {
            CheckGetSubArrayArguments(list, start, length);
            return new ReadOnlySpan<T>(list.Ptr + start, length);
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void CheckGetSubArrayArguments<T>(UnsafeList<T> array, int start, int length)
            where T : unmanaged
        {
            if (start < 0)
                throw new ArgumentOutOfRangeException(nameof(start), "start must be >= 0");
            if (start + length > array.Length)
                throw new ArgumentOutOfRangeException(nameof(length), $"sub array range {start}-{start + length - 1} is outside the range of the native array 0-{array.Length - 1}");
            if (start + length < 0)
                throw new ArgumentException($"sub array range {start}-{start + length - 1} caused an integer overflow and is outside the range of the native array 0-{array.Length - 1}");
        }
    }
}