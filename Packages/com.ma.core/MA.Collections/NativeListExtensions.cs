// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using MA.Collections.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace MA.Collections
{
    /// <summary>Extension methods for <see cref="NativeList{T}"/>.</summary>
    public static class NativeListExtensions
    {
        /// <summary>Returns true if the index is a valid for the list, false otherwise.</summary>
        /// <param name="list">The list to check the index against.</param>
        /// <param name="index">The index to check.</param>
        /// <returns>True if the index is valid, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidIndex<T>(in this NativeList<T> list, int index) where T : unmanaged => index >= 0 && index < list.Length;

        /// <summary>Adds `count` uninitialized elements to the end of the list.</summary>
        /// <param name="list">The list to add elements to.</param>
        /// <param name="count">The number of elements to add.</param>
        /// <remarks>Resizes the list to `list.Length + count`.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddUninitialized<T>(this NativeList<T> list, int count) where T : unmanaged => list.Resize(list.Length + count, NativeArrayOptions.UninitializedMemory);
        
        /// <summary>Fills the list with a value up to the length.</summary>
        /// <param name="list">The list to fill.</param>
        /// <param name="value">The value to fill the list with.</param>
        /// <param name="startIndex">The index to start filling from.</param>
        /// <param name="length">The number of elements to fill.</param>
        /// <remarks>If `length` is less than zero, the entire list is filled.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Fill<T>(this NativeList<T> list, T value, int startIndex = 0, int length = -1) where T : unmanaged
        {
            if (length < 0) length = list.Length - startIndex;
            UnsafeUtility.MemCpyReplicate((T*)list.GetUnsafePtr() + startIndex, &value, UnsafeUtility.SizeOf<T>(), length);
        }
        
        /// <summary>Resizes the list to the given size, and fills all elements with the given value.</summary>
        /// <param name="list">The list to resize and fill.</param>
        /// <param name="initValue">The value to fill the list with.</param>
        /// <param name="count">The number of elements to initialize.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Initialize<T>(this NativeList<T> list, in T initValue, int count) where T : unmanaged
        {
            list.ResizeUninitialized(count);
            list.Fill(initValue);
        }

        /// <summary>Reserves capacity in the list if the current capacity is less than the given count.</summary>
        /// <param name="list">The list to reserve capacity in.</param>
        /// <param name="count">The minimum capacity to reserve.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Reserve<T>(this NativeList<T> list, int count) where T : unmanaged
        {
            if (list.Capacity < count)
                list.Capacity = count;
        }

        /// <summary>Reserves an additional amount of capacity in the list.</summary>
        /// <param name="list">The list to reserve capacity in.</param>
        /// <param name="count">The additional capacity to reserve.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReserveAdditional<T>(this NativeList<T> list, int count) where T : unmanaged
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
        public static unsafe void CopyFrom<T>(this NativeList<T> self, ReadOnlySpan<T> span) where T : unmanaged
        {
            self.Clear();
            if (span.Length > 0)
            {
                self.Resize(span.Length, NativeArrayOptions.UninitializedMemory);
                fixed (void* otherPtr = span)
                    UnsafeUtility.MemCpy(self.GetUnsafePtr(), otherPtr, span.Length * UnsafeUtility.SizeOf<T>());
            }
        }
        
        /// <summary>Returns a <see cref="UnsafeIndirectList{T}"/> representing the list.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe UnsafeIndirectList<T> AsUnsafeIndirectList<T>(this NativeList<T> list) where T : unmanaged => new UnsafeIndirectList<T> { List = list.GetUnsafeList() };

        /// <summary>Transfers ownership of the <see cref="NativeList{T}"/> to a <see cref="NativeArray{T}"/>.</summary>
        /// <remarks>This is useful for returning a read-only array from a function that requires a list to operate on.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe NativeArray<T> TransferOwnershipToNativeArray<T>(ref this NativeList<T> list)
            where T : unmanaged
        {
            UnsafeList<T>* unsafeList = list.GetUnsafeList();
            NativeArray<T> array = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(unsafeList->Ptr, unsafeList->Length, unsafeList->Allocator.ToAllocator);
            unsafeList->Allocator = AllocatorManager.None;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            list.Dispose();                                                                         // Release the old safety handle
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref array, AtomicSafetyHandle.Create()); // Assign a new handle
#endif
            return array;
        }
        
        /// <summary>Creates a <see cref="Span{T}"/> from a <see cref="NativeList{T}"/> starting at the specified index.</summary>
        /// <param name="list">The list to create the span from.</param>
        /// <returns>A <see cref="Span{T}"/> representing the specified range of the list.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Span<T> AsSpan<T>(this NativeList<T> list) where T : unmanaged => new Span<T>(list.GetUnsafePtr(), list.Length);

        /// <summary>Creates a <see cref="Span{T}"/> from a <see cref="NativeList{T}"/> starting at the specified index.</summary>
        /// <param name="list">The list to create the span from.</param>
        /// <param name="start">The index to start the span at.</param>
        /// <param name="length">The length of the span.</param>
        /// <returns>A <see cref="Span{T}"/> representing the specified range of the list.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Span<T> AsSpan<T>(this NativeList<T> list, int start, int length) where T : unmanaged
        {
            CheckGetSubArrayArguments(list, start, length);
            return new Span<T>((T*)list.GetUnsafeReadOnlyPtr() + start, length);
        }

        /// <summary>Creates a <see cref="ReadOnlySpan{T}"/> from a <see cref="NativeList{T}"/> starting at the specified index.</summary>
        /// <param name="list">The list to create the span from.</param>
        /// <returns>A <see cref="ReadOnlySpan{T}"/> representing the specified range of the list.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ReadOnlySpan<T> AsReadOnlySpan<T>(this NativeList<T> list) where T : unmanaged => new ReadOnlySpan<T>((T*)list.GetUnsafeReadOnlyPtr(), list.Length);

        /// <summary>Creates a <see cref="ReadOnlySpan{T}"/> from a <see cref="NativeList{T}"/> starting at the specified index.</summary>
        /// <param name="list">The list to create the span from.</param>
        /// <param name="start">The index to start the span at.</param>
        /// <param name="length">The length of the span.</param>
        /// <returns>A <see cref="ReadOnlySpan{T}"/> representing the specified range of the list.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ReadOnlySpan<T> AsReadOnlySpan<T>(this NativeList<T> list, int start, int length) where T : unmanaged
        {
            CheckGetSubArrayArguments(list, start, length);
            return new ReadOnlySpan<T>((T*)list.GetUnsafeReadOnlyPtr() + start, length);
        }

        /// <summary>Returns a pointer to the element at the specified index.</summary>
        /// <seealso cref="NativeListUnsafeUtility.GetUnsafePtr{T}"/>
        /// <param name="list">The list to get the pointer from.</param>
        /// <param name="index">The index of the element to get the pointer to.</param>
        /// <returns>A pointer to the element at the specified index.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe T* GetUnsafePtrAt<T>(this NativeList<T> list, int index) where T : unmanaged
        {
            CheckIndexInRange(index, list.Length);
            return (T*)list.GetUnsafePtr() + index;
        }

        /// <summary>Returns a `ReadOnly` pointer to the element at the specified index.</summary>
        /// <seealso cref="NativeListUnsafeUtility.GetUnsafeReadOnlyPtr{T}"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe T* GetUnsafeReadOnlyPtrAt<T>(this NativeList<T> list, int index) where T : unmanaged
        {
            CheckIndexInRange(index, list.Length);
            return (T*)list.GetUnsafeReadOnlyPtr() + index;
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void CheckGetSubArrayArguments<T>(NativeList<T> array, int start, int length) where T : unmanaged
        {
            if (start < 0)
                throw new ArgumentOutOfRangeException(nameof(start), "start must be >= 0");
            if (start + length > array.Length)
                throw new ArgumentOutOfRangeException(nameof(length), $"sub array range {start}-{start + length - 1} is outside the range of the native array 0-{array.Length - 1}");
            if (start + length < 0)
                throw new ArgumentException($"sub array range {start}-{start + length - 1} caused an integer overflow and is outside the range of the native array 0-{array.Length - 1}");
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void CheckGetSubArrayArguments<T>(UnsafeList<T> array, int start, int length) where T : unmanaged
        {
            if (start < 0)
                throw new ArgumentOutOfRangeException(nameof(start), "start must be >= 0");
            if (start + length > array.Length)
                throw new ArgumentOutOfRangeException(nameof(length), $"sub array range {start}-{start + length - 1} is outside the range of the native array 0-{array.Length - 1}");
            if (start + length < 0)
                throw new ArgumentException($"sub array range {start}-{start + length - 1} caused an integer overflow and is outside the range of the native array 0-{array.Length - 1}");
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void CheckIndexInRange(int index, int length)
        {
            // This checks both < 0 and >= Length with one comparison
            if ((uint)index >= (uint)length)
                throw new IndexOutOfRangeException($"Index {index} is out of range in container of '{length}' Length.");
        }
    }
}
