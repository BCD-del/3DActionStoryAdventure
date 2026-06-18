// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace MA.Collections
{
    /// <summary>Provides helper methods to avoid memory allocations.</summary>
    public static class NoAllocHelpers
    {
        /// <summary>Resizes a list to the given size.</summary>
        /// <param name="list">The list to resize.</param>
        /// <param name="size">The new size of the list.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ResizeList<T>(List<T> list, int size)
        {
            Debug.Assert(list.Capacity >= size);
            ListPrivateFieldAccess<T> privateFieldAccess = UnsafeUtility.As<List<T>, ListPrivateFieldAccess<T>>(ref list);
            privateFieldAccess._size = size;
            ++privateFieldAccess._version;
        }

        /// <summary>Resizes a list to the given size.</summary>
        /// <param name="list">The list to resize.</param>
        /// <param name="array">The new array to use for the list.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReplaceInternalArray<T>(List<T> list, T[] array)
        {
            ListPrivateFieldAccess<T> privateFieldAccess = UnsafeUtility.As<List<T>, ListPrivateFieldAccess<T>>(ref list);
            privateFieldAccess._items = array;
            privateFieldAccess._size = Mathf.Min(privateFieldAccess._size, array.Length);
            ++privateFieldAccess._version;
        }

        /// <summary>Ensures that a list has a certain number of elements.</summary>
        /// <param name="list">The list to ensure the size of.</param>
        /// <param name="count">The desired size of the list.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureListElemCount<T>(List<T> list, int count)
        {
            if (list == null)
                throw new ArgumentNullException(nameof (list));
            if (count < 0)
                throw new ArgumentException("invalid size to resize.", nameof (list));
            
            list.Clear();
            if (list.Capacity < count)
                list.Capacity = count;
            if (count == list.Count)
                return;
            
            ListPrivateFieldAccess<T> privateFieldAccess = UnsafeUtility.As<List<T>, ListPrivateFieldAccess<T>>(ref list);
            privateFieldAccess._size = count;
            ++privateFieldAccess._version;
        }

        /// <summary>Extracts the internal array from a list.</summary>
        /// <param name="list">The list to extract the array from.</param>
        /// <returns>The internal array of the list.</returns>
        /// <typeparam name="T">The type of the list.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] ExtractArrayFromListT<T>(List<T> list)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));
            
            ListPrivateFieldAccess<T> privateFieldAccess = UnsafeUtility.As<List<T>, ListPrivateFieldAccess<T>>(ref list);
            return privateFieldAccess._items;
        }
        
        /// <summary>A helper class for accessing the private fields of a list.</summary>
        /// <remarks>Ensure that the layout of this class matches the layout of the <see cref="List{T}"/> class.</remarks>
        class ListPrivateFieldAccess<T>
        {
            internal T[] _items;
            internal int _size;
            internal int _version;
        }
    }
}
