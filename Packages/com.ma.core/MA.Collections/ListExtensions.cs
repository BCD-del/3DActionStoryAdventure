// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MA.Collections.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace MA.Collections
{
    /// <summary>Extension methods for <see cref="System.Collections.Generic.List{V}"/>.</summary>
    public static class ListExtensions
    {
        /// <summary>Creates a new <see cref="UnsafeArray{T}"/> from the list.</summary>
        /// <param name="list">The source list.</param>
        /// <param name="allocator">The allocator to use for the new array.</param>
        /// <typeparam name="T">The type of elements.</typeparam>
        /// <returns>A new <see cref="UnsafeArray{T}"/> containing the elements of the list.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeArray<T> ToUnsafeArray<T>(this List<T> list, AllocatorManager.AllocatorHandle allocator) where T : unmanaged
        {
            UnsafeArray<T> array = new UnsafeArray<T>(list.Count, allocator);
            list.CopyTo(ref array);
            return array;
        }

        /// <summary>Copies unmanaged elements from a <see cref="List{T}"/> into the list.</summary>
        /// <param name="self">The target list.</param>
        /// <param name="other">The <see cref="List{T}"/> from which elements are sourced.</param>
        /// <typeparam name="T">The type of elements.</typeparam>
        /// <remarks>The list is cleared before copying elements.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void CopyFrom<T>(this List<T> self, List<T> other) where T : unmanaged
        {
            fixed (T* src = other.GetInternalArray())
                CopyFrom(self, src, other.Count);
        }

        /// <summary>Copies elements from a <see cref="NativeArray{T}"/> into the list.</summary>
        /// <param name="self">The target list.</param>
        /// <param name="array">The <see cref="NativeArray{T}"/> from which elements are sourced.</param>
        /// <typeparam name="T">The type of elements.</typeparam>
        /// <remarks>The list is cleared before copying elements.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void CopyFrom<T>(this List<T> self, NativeArray<T> array) where T : unmanaged
            => CopyFrom(self, (T*)array.GetUnsafeReadOnlyPtr(), array.Length);

        /// <summary>Copies elements from a <see cref="UnsafeIndirectList{T}"/> into the list.</summary>
        /// <param name="self">The target list.</param>
        /// <param name="indirectList">The <see cref="UnsafeIndirectList{T}"/> from which elements are sourced.</param>
        /// <typeparam name="T">The type of elements.</typeparam>
        /// <remarks>The list is cleared before copying elements.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void CopyFrom<T>(this List<T> self, UnsafeIndirectList<T> indirectList) where T : unmanaged
            => CopyFrom(self, indirectList.Ptr, indirectList.Length);

        /// <summary>Copies elements from a <see cref="UnsafeArray{T}"/> into the list.</summary>
        /// <param name="self">The target list.</param>
        /// <param name="array">The <see cref="UnsafeArray{T}"/> from which elements are sourced.</param>
        /// <typeparam name="T">The type of elements.</typeparam>
        /// <remarks>The list is cleared before copying elements.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void CopyFrom<T>(this List<T> self, UnsafeArray<T> array) where T : unmanaged
            => CopyFrom(self, array.Ptr, array.Length);

        /// <summary>Copies unmanaged elements from a source pointer, using <see cref="UnsafeUtility.MemCpy"/></summary>
        /// <param name="list">The target list.</param>
        /// <param name="src">The source pointer.</param>
        /// <param name="srcLength">The number of elements to copy.</param>
        /// <typeparam name="T">The type of elements.</typeparam>
        /// <remarks>The list is cleared before copying elements.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void CopyFrom<T>(this List<T> list, T* src, int srcLength)
            where T : unmanaged
        {
            if (srcLength == 0)
            {
                list.Clear();
                return;
            }

            list.Resize(srcLength);

            T[] internalArray = list.GetInternalArray();
            fixed (void* dst = internalArray)
            {
                UnsafeUtility.MemCpy(dst, src, srcLength * UnsafeUtility.SizeOf<T>());
            }
        }

        /// <summary>Copies elements from the list into a <see cref="UnsafeArray{T}"/>.</summary>
        /// <param name="self">The source list.</param>
        /// <param name="array">The <see cref="UnsafeArray{T}"/> to copy elements into.</param>
        /// <typeparam name="T">The type of elements.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void CopyTo<T>(this List<T> self, ref UnsafeArray<T> array) where T : unmanaged
        {
            UnsafeArray<T>.CheckCopyLengths(self.Count, array.Length);
            CopyTo(self, array.Ptr, array.Length);
        }

        /// <summary>Copies elements from the list into a <see cref="UnsafeIndirectList{T}"/>.</summary>
        /// <param name="self">The source list.</param>
        /// <param name="indirectList">The <see cref="UnsafeIndirectList{T}"/> to copy elements into.</param>
        /// <typeparam name="T">The type of elements.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void CopyTo<T>(this List<T> self, UnsafeIndirectList<T> indirectList) where T : unmanaged
        {
            indirectList.Resize(self.Count);
            CopyTo(self, indirectList.Ptr, indirectList.Length);
        }

        /// <summary>Copies unmanaged elements from a list into a destination pointer, using <see cref="UnsafeUtility.MemCpy"/>.</summary>
        /// <param name="list">The source list.</param>
        /// <param name="dst">The destination pointer.</param>
        /// <param name="dstLength">The number of elements to copy.</param>
        /// <typeparam name="T">The type of elements.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void CopyTo<T>(this List<T> list, T* dst, int dstLength)
            where T : unmanaged
        {
            if (dstLength == 0)
                return;

            T[] array = list.GetInternalArray();
            fixed (void* src = array)
            {
                UnsafeUtility.MemCpy(dst, src, dstLength * UnsafeUtility.SizeOf<T>());
            }
        }

        /// <summary>Inserts an item into the list in sorted order.</summary>
        /// <remarks>Items must implement <see cref="IComparable{T}"/>.</remarks>
        /// <param name="list">The target list.</param>
        /// <param name="item">The item to insert.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddSorted<T>(this List<T> list, T item)
            where T : IComparable<T>
        {
            if (list.Count == 0)
            {
                // Empty list
                list.Add(item);
                return;
            }

            if (list[^1].CompareTo(item) <= 0)
            {
                // Item is greater than the last item in the list
                list.Add(item);
                return;
            }

            if (list[0].CompareTo(item) >= 0)
            {
                // Item is less than the first item in the list, insert it at the start
                list.Insert(0, item);
                return;
            }

            // Find the index to insert the item at
            int index = list.BinarySearch(item);
            if (index < 0)
                index = ~index;

            // Insert the item
            list.Insert(index, item);
        }

        /// <summary>Inserts an item into the list in sorted order using a custom comparer.</summary>
        /// <remarks>Items must implement <see cref="IComparable{T}"/>.</remarks>
        /// <param name="list">The target list.</param>
        /// <param name="item">The item to insert.</param>
        /// <param name="comparer">The comparer to use.</param>
        /// <typeparam name="T">The type of elements.</typeparam>
        /// <typeparam name="C">The type of comparer.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddSorted<T, C>(this List<T> list, T item, C comparer) where C : IComparer<T>
        {
            if (list.Count == 0)
            {
                // Empty list
                list.Add(item);
                return;
            }

            if (comparer.Compare(list[^1], item) <= 0)
            {
                // Item is greater than the last item in the list
                list.Add(item);
                return;
            }

            if (comparer.Compare(list[0], item) >= 0)
            {
                // Item is less than the first item in the list, insert it at the start
                list.Insert(0, item);
                return;
            }

            // Find the index to insert the item at
            // TODO: Boxing allocation?
            int index = list.BinarySearch(item, comparer);
            if (index < 0)
                index = ~index;

            // Insert the item
            list.Insert(index, item);
        }

        /// <summary>Ensures that the list has enough capacity to hold <paramref name="count"/> items.</summary>
        /// <remarks>This method is faster than <see cref="List{T}.Capacity"/> because it only allocates memory if the list is too small.</remarks>
        /// <param name="list">The target list.</param>
        /// <param name="count">The number of items the list must be able to hold.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Reserve<T>(this List<T> list, int count)
        {
            if (count > list.Capacity)
            {
                if (list.Capacity > 0)
                    list.Capacity = count + 3 * count / 8 + 16; // 3/8 growth factor
                else
                    list.Capacity = Mathf.Max(4, count);
            }
        }

        /// <summary>Ensures the list has enough capacity to hold an additional <paramref name="count"/> of items.</summary>
        /// <param name="list">The target list.</param>
        /// <param name="count">The number of additional items the list must be able to hold.</param>
        /// <typeparam name="T">The type of elements.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReserveAdditional<T>(this List<T> list, int count) => Reserve(list, list.Count + count);

        /// <summary>Resizes the list to the specified size using an optimized Unity method.</summary>
        /// <param name="list">The target list to resize.</param>
        /// <param name="size">The new size of the list.</param>
        /// <typeparam name="T">The type of elements.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Resize<T>(this List<T> list, int size)
        {
            if (list.Capacity < size)
                list.Capacity = math.ceilpow2(math.max(size, 16));

            NoAllocHelpers.ResizeList(list, size);
        }

        /// <summary>Resets and resizes the list to a specified size using a default value.</summary>
        /// <param name="list">The target list to reset and resize.</param>
        /// <param name="size">The new size of the list.</param>
        /// <param name="defaultValue">The default value for new elements.</param>
        /// <typeparam name="T">The type of elements.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Reset<T>(this List<T> list, int size, T defaultValue)
        {
            Resize(list, size);

            int count = list.Count;
            if (size > count)
            {
                for (int i = count; i < size; i++)
                    list.Add(defaultValue);
            }
        }

        /// <summary>Checks if an index is valid for the list.</summary>
        /// <param name="list">The list to check against.</param>
        /// <param name="index">The index to check.</param>
        /// <typeparam name="T">The type of elements.</typeparam>
        /// <returns>True if the index is valid; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidIndex<T>(this List<T> list, int index) => index >= 0 && index < list.Count;

        /// <summary>Returns a reference to an element at the specified index.</summary>
        /// <param name="list">The list containing the element.</param>
        /// <param name="index">The index of the element.</param>
        /// <typeparam name="T">The type of elements.</typeparam>
        /// <returns>A reference to the element at the specified index.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T ElementAt<T>(this List<T> list, int index) => ref NoAllocHelpers.ExtractArrayFromListT(list)[index];

        /// <summary>Returns the internal array of the list.</summary>
        /// <param name="list">The source list.</param>
        /// <typeparam name="T">The type of elements.</typeparam>
        /// <returns>The internal array of the list.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] GetInternalArray<T>(this List<T> list) => NoAllocHelpers.ExtractArrayFromListT(list);

        /// <summary>Returns a <see cref="Span{T}"/> that representation of the list.</summary>
        /// <param name="list">The source list.</param>
        /// <typeparam name="T">The type of elements.</typeparam>
        /// <returns>A Span representing the list.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> AsSpan<T>(this List<T> list) => new Span<T>(NoAllocHelpers.ExtractArrayFromListT(list), 0, list.Count);

        /// <summary>Returns a <see cref="Span{T}"/> that representation of the list.</summary>
        /// <param name="list">The source list.</param>
        /// <param name="index">The starting index of the portion.</param>
        /// <param name="count">The number of elements in the portion.</param>
        /// <typeparam name="T">The type of elements.</typeparam>
        /// <returns>A Span representing the portion of the list.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> AsSpan<T>(this List<T> list, int index, int count) => new Span<T>(NoAllocHelpers.ExtractArrayFromListT(list), index, count);

        /// <summary>Returns a <see cref="ReadOnlySpan{T}"/> that representation of the list.</summary>
        /// <param name="list">The source list.</param>
        /// <typeparam name="T">The type of elements.</typeparam>
        /// <returns>A ReadOnlySpan representing the list.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> AsReadOnlySpan<T>(this List<T> list) => new ReadOnlySpan<T>(NoAllocHelpers.ExtractArrayFromListT(list), 0, list.Count);

        /// <summary>Returns a <see cref="ReadOnlySpan{T}"/> that representation of the list.</summary>
        /// <param name="list">The source list.</param>
        /// <param name="index">The starting index of the portion.</param>
        /// <param name="count">The number of elements in the portion.</param>
        /// <typeparam name="T">The type of elements.</typeparam>
        /// <returns>A ReadOnlySpan representing the portion of the list.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> AsReadOnlySpan<T>(this List<T> list, int index, int count) => new ReadOnlySpan<T>(NoAllocHelpers.ExtractArrayFromListT(list), index, count);

        /// <summary>Copies elements from a <see cref="ReadOnlySpan{T}"/> to a <see cref="List{T}"/>.</summary>
        /// <param name="list">The target list to copy elements to.</param>
        /// <param name="span">The span to copy elements from.</param>
        /// <typeparam name="T">The type of elements.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyFrom<T>(this List<T> list, ReadOnlySpan<T> span)
        {
            if (span.Length == 0)
            {
                list.Clear();
            }
            else
            {
                list.Resize(span.Length);
                span.CopyTo(list.GetInternalArray());
            }
        }

        /// <summary>Copies elements from a <see cref="List{T}"/> to a <see cref="Span{T}"/>.</summary>
        /// <param name="list">The target list to copy elements from.</param>
        /// <param name="span">The span to copy elements to.</param>
        /// <typeparam name="T">The type of elements.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<T>(this List<T> list, Span<T> span)
        {
            if (span.Length < list.Count)
                throw new ArgumentException("Destination span is too small.");

            if (list.Count == 0)
                return;

            list.AsSpan().CopyTo(span);
        }

        /// <summary>Adds unmanaged elements from a <see cref="ReadOnlySpan{T}"/> to the list.</summary>
        /// <param name="list">The target list.</param>
        /// <param name="span">The span to add elements from.</param>
        /// <typeparam name="T">The type of elements.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddSpan<T>(this List<T> list, ReadOnlySpan<T> span)
        {
            if (span.Length == 0)
                return;

            int start = list.Count;
            int end = list.Count + span.Length;
            list.Resize(end);
            span.CopyTo(list.AsSpan(start, span.Length));
        }
    }
}
