// Copyright © Magnetic Arcade. All Rights Reserved.

// #define ENABLE_CORE_COLLECTIONS_CHECKS

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace MA.Collections
{
    /// <summary>Provides a lightweight list with direct reference access to its items and access to its internal array.</summary>
    /// <remarks>Is directly convertable to and from a <see cref="List{T}"/>.</remarks>
    [DebuggerDisplay("Count = {Count}")]
    [Serializable]
    [DebuggerTypeProxy(typeof(LeanListDebugView<>))]
    [StructLayout(LayoutKind.Sequential)]
    public sealed class LeanList<T>
        : IEnumerable<T>
        , ISerializationCallbackReceiver
    {
        [SerializeField] internal T[] m_Items;
        [SerializeField] internal int m_Count;
        [SerializeField] internal int m_Version; // Currently unused, keeps in sync with List<T> for compatibility.

        /// <summary>Constructs a LeanList. The list is initially empty and has a capacity of zero.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LeanList()
        {
            m_Items = System.Array.Empty<T>();
            m_Count = 0;
        }

        /// <summary>Constructs a LeanList with a given initial capacity.</summary>
        /// <param name="capacity">Initial capacity of the list.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LeanList(int capacity) : this()
        {
            Capacity = capacity;
        }

        /// <summary>Constructs a LeanList with an initial array and range of elements to copy.</summary>
        /// <param name="values">Array to copy elements from.</param>
        /// <param name="startIndex">Start index in the array to begin copying.</param>
        /// <param name="count">Number of elements to copy from the array.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LeanList(T[] values, int startIndex, int count) : this()
        {
            CopyFrom(values, startIndex, count);
        }

        /// <summary>Constructs a LeanList with an initial array.</summary>
        /// <param name="values">Array to initialize the list with.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LeanList(T[] values) : this(values, 0, values.Length) { }
        
        /// <summary>Method that is called before object serialization.</summary>
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            TrimExcess(); // Trim excess before serializing
        }

        /// <summary>Method that is called after object deserialization.</summary>
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
        }
        
        /// <summary>Direct access to the underlying array. Use with caution.</summary>
        public T[] InternalArray
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Items;
        }

        /// <summary>Gets or set the number of elements contained in the list.</summary>
        /// <remarks>When setting the count, it will expand or reduce the list as needed.</remarks> 
        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Count;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (value != m_Count)
                {
                    if (value > m_Items.Length)
                    {
                        Resize(value);
                    }
                    else
                    {
                        m_Count = value;
                    }
                }
            }
        }

        /// <summary>Gets or sets the total number of elements the internal data structure can hold without resizing.</summary>
        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Items.Length;
            set
            {
                if (value != m_Items.Length)
                {
                    if (value > 0)
                    {
                        int newCapacity = math.max(value, 4);
                        newCapacity = math.ceilpow2(newCapacity);
                        if (newCapacity == m_Items.Length)
                        {
                            return;
                        }
                    
                        System.Array.Resize(ref m_Items, newCapacity);
                        m_Count = math.min(m_Count, newCapacity);
                    }
                    else
                    {
                        m_Items = System.Array.Empty<T>();
                        m_Count = 0;
                    }
                }
            }
        }

        /// <summary>Gets or sets the element at the specified index with a reference return.</summary>
        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                CheckIndex(index);
                return ref m_Items[index];
            }
        }

        /// <summary>Returns true if the list contains no elements, false otherwise.</summary>
        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Count == 0;
        }
        
        /// <summary>Determines if the given index is within the valid range of the list's elements.</summary>
        /// <param name="index">The index to validate.</param>
        /// <returns>True if the index is valid, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsValidIndex(int index) => index >= 0 && index < m_Count;

        /// <summary>Reserves capacity for the list to fit a given number of elements.</summary>
        /// <param name="count">The number of elements to reserve capacity for.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reserve(int count) => Capacity = count;

        /// <summary>Reserves additional capacity for the list to fit an extra number of elements.</summary>
        /// <param name="count">The additional number of elements to reserve capacity for.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReserveAdditional(int count) => Capacity = m_Count + count;

        /// <summary>Sets the list's length without initializing the new elements.</summary>
        /// <param name="newCount">The new length of the list.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize(int newCount)
        {
            if (newCount > m_Items.Length)
                Capacity = newCount;

            m_Count = newCount;
        }

        /// <summary>Sets the count to 0.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => m_Count = 0;

        /// <summary>Resets the list to its initial state. This is equivalent to setting the count to 0 and the capacity to 0.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetEmpty()
        {
            m_Items = System.Array.Empty<T>();
            m_Count = 0;
        }

        /// <summary>Sets the capacity to match the count.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TrimExcess()
        {
            if (m_Count < m_Items.Length)
            {
                if (m_Count > 0)
                {
                    System.Array.Resize(ref m_Items, m_Count);
                }
                else
                {
                    m_Items = System.Array.Empty<T>();
                }
            }
        }
        
        /// <summary>Returns the index of the first occurrence of a value, or -1 if not found.</summary>
        /// <param name="value">The value to locate in the list.</param>
        /// <returns>The index of the first occurrence or -1 if not found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(in T value) => Array.IndexOf(m_Items, value, 0, m_Count);
        
        /// <summary>Checks if the list contains a specific value.</summary>
        /// <param name="value">The value to check for.</param>
        /// <returns>True if the list contains the value, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(in T value) => IndexOf(value) != -1;

        /// <summary>Adds an element to the list, resizing if necessary.</summary>
        /// <param name="value">The value to add.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(in T value)
        {
            if (m_Count == m_Items.Length)
                Capacity = m_Count + 1;

            m_Items[m_Count++] = value;
        }
        
        /// <summary>Adds an element without resizing the list.</summary>
        /// <param name="value">The value to add.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddNoResize(T value)
        {
            m_Items[m_Count++] = value;
        }

        /// <summary>Adds a range of elements from an array to the list.</summary>
        /// <param name="array">The array containing the elements.</param>
        /// <param name="count">The number of elements to add.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange(T[] array, int count)
        {
            int index = m_Count;
            if (m_Count + count > Capacity)
            {
                Resize(m_Count + count);
            }
            else
            {
                m_Count += count;
            }
            
            Array.Copy(array, 0, m_Items, index, count);
        }
        
        /// <summary>Inserts an item into the list in sorted order using a custom comparer.</summary>
        /// <remarks>Items must implement <see cref="IComparable{T}"/>.</remarks>
        /// <param name="item">The item to insert.</param>
        /// <param name="comparer">The comparer to use.</param>
        /// <typeparam name="T">The type of elements.</typeparam>
        /// <typeparam name="TComparer">The type of comparer.</typeparam>
        public void AddSorted<TComparer>(T item, TComparer comparer) where TComparer : IComparer<T>
        {
            if (m_Count == 0)
            {
                // Empty list, just add the item.
                Add(item);
                return;
            }

            if (comparer.Compare(this[^1], item) <= 0)
            {
                // Item is greater than the last item in the list, just add it.
                Add(item);
                return;
            }

            if (comparer.Compare(this[0], item) >= 0)
            {
                // Item is less than the first item in the list, insert it at the start.
                Insert(item, 0);
                return;
            }

            // Find the index to insert the item at.
            int index = BinarySearch(item, comparer);
            if (index < 0)
                index = ~index;

            // Insert the item.
            Insert(item, index);
        }
        
        /// <summary>Finds the index of an element in the list using a binary search.</summary>
        /// <param name="value">The value to search for.</param>
        /// <param name="comp">The comparer to use.</param>
        /// <typeparam name="TComparer">The type of comparer.</typeparam>
        /// <returns>The index if the value was found using the proved comparer, otherwise -1.</returns>
        public int BinarySearch<TComparer>(T value, TComparer comp) where TComparer : IComparer<T>
        {
            var offset = 0;

            for (var l = m_Count; l != 0; l >>= 1)
            {
                var idx = offset + (l >> 1);
                var curr = m_Items[idx];
                var r = comp.Compare(value, curr);
                if (r == 0)
                {
                    return idx;
                }

                if (r > 0)
                {
                    offset = idx + 1;
                    --l;
                }
            }

            return ~offset;
        }
        
        /// <summary>Inserts an element at a specific index, resizing if necessary.</summary>
        /// <param name="value">The value to insert.</param>
        /// <param name="index">The index to insert at.</param>
        public void Insert(T value, int index)
        {
            if (m_Count == m_Items.Length)
                Capacity = m_Count + 1;

            Array.Copy(m_Items, index, m_Items, index + 1, m_Count - index);
            m_Items[index] = value;
            m_Count++;
        }
        
        /// <summary>Removes and returns the last element from the list.</summary>
        /// <returns>The last element from the list.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Pop()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (m_Count == 0)
                throw new InvalidOperationException("Cannot pop from an empty list.");
#endif
            
            T value = m_Items[m_Count - 1];
            m_Count--;
            return value;
        }

        /// <summary>Removes an element at a specific index and replaces it with the last element.</summary>
        /// <param name="index">The index of the element to remove.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAtSwapBack(int index) => RemoveRangeSwapBack(index, 1);

        /// <summary>Removes a range of elements and replaces them with the last elements in the list.</summary>
        /// <param name="index">The starting index of the range to remove.</param>
        /// <param name="count">The number of elements to remove.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveRangeSwapBack(int index, int count)
        {
            CheckIndexCount(index, count);
            if (count > 0)
            {
                // Determine the start index to copy items from the end.
                int sourceIndex = math.max(m_Count - count, index + count);
                // Number of items to copy from the end to the spot where items were removed.
                int itemsToMove = m_Count - sourceIndex;
                // Copy items from the end of the list to fill the gap.
                Array.Copy(m_Items, sourceIndex, m_Items, index, itemsToMove);
                // Update list count.
                m_Count -= count;
            }
        }

        /// <summary>Removes the element at a specific index.</summary>
        /// <param name="index">The index of the element to remove.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index)
        {
            m_Count--;
            if (index < m_Count) 
            {
                Array.Copy(m_Items, index + 1, m_Items, index, m_Count - index);
            }
        }

        /// <summary>Removes a range of elements from the list.</summary>
        /// <param name="index">The starting index of the range to remove.</param>
        /// <param name="count">The number of elements to remove.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveRange(int index, int count)
        {
            CheckIndexCount(index, count);
            if (count > 0)
            {
                m_Count -= count;
                if (index < m_Count) 
                {
                    Array.Copy(m_Items, index + 1, m_Items, index, m_Count - index);
                }
            }
        }
        
        /// <summary>Fills a range in the list with a specific value.</summary>
        /// <param name="value">The value to fill with.</param>
        /// <param name="startIndex">The starting index for the fill operation.</param>
        /// <param name="count">The number of elements to fill.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fill(in T value, int startIndex, int count) => Array.Fill(m_Items, value, startIndex, count);
        
        /// <summary>Fills the entire list with a specific value.</summary>
        /// <param name="value">The value to fill with.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fill(in T value) => Array.Fill(m_Items, value, 0, m_Count);

        /// <summary>Replaces elements in the list with those from a specified array segment.</summary>
        /// <param name="array">The source array.</param>
        /// <param name="startIndex">The start index in the source array.</param>
        /// <param name="length">The number of elements to copy.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(T[] array, int startIndex, int length)
        {
            Resize(length);
            Array.Copy(array, startIndex, m_Items, 0, length);
        }

        /// <summary>Replaces elements in the list with those from a specified array.</summary>
        /// <param name="array">The source array.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(T[] array) => CopyFrom(array, 0, array.Length);

        /// <summary>Replaces elements in the list with those from another LeanList.</summary>
        /// <param name="array">The source LeanList.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(LeanList<T> array) => CopyFrom(array.m_Items, 0, array.m_Count);
        
        /// <summary>Returns a mutable memory span representing the list's items.</summary>
        /// <returns>A span that represents the list's items.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan() => new Span<T>(m_Items, 0, m_Count);
        
        /// <summary>Returns a read-only memory span representing the list's items.</summary>
        /// <returns>A read-only span that represents the list's items.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan() => new ReadOnlySpan<T>(m_Items, 0, m_Count);

        /// <summary>Returns an enumerator over the elements of the list.</summary>
        /// <returns>An enumerator for the list.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() => new Enumerator(m_Items, m_Count);

        /// <summary>Returns an enumerator over the elements of the list.</summary>
        /// <returns>An enumerator for the list.</returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>Returns an enumerator over the elements of the list.</summary>
        /// <returns>An enumerator for the list.</returns>
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

        /// <summary>An enumerator over the elements of a list.</summary>
        public struct Enumerator : IEnumerator<T>
        {
            readonly T[] m_Items;
            readonly int m_Count;
            int m_Index;

            /// <summary>Constructs an enumerator over the elements of an array.</summary>
            /// <param name="items">The array to enumerate over.</param>
            /// <param name="count">The number of elements in the array to enumerate over.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Enumerator(T[] items, int count)
            {
                m_Items = items;
                m_Count = count;
                m_Index = -1;
            }

            /// <summary>Does nothing.</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose() { }

            /// <summary>Advances the enumerator to the next element of the list.</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext() => ++m_Index < m_Count;

            /// <summary>Resets the enumerator to its initial state.</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset() => m_Index = -1;

            /// <summary>The current element.</summary>
            public T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => m_Items[m_Index];
            }

            object IEnumerator.Current => Current;
        }
        
        /// <summary>Checks if the given index is valid for the list.</summary>
        /// <param name="index">The index to check.</param>
        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void CheckIndex(int index)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (index < 0 || index >= m_Count)
                throw new ArgumentOutOfRangeException($"Value for index {index} is out of bounds.");
#endif
        }
        
        /// <summary>Checks if the given index and count are valid for the list.</summary>
        /// <param name="index">The starting index.</param>
        /// <param name="count">The number of elements to check.</param>
        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void CheckIndexCount(int index, int count)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (index < 0 || count < 0 || index + count > m_Count)
                throw new ArgumentOutOfRangeException($"Value for index {index} and/or count {count} are out of bounds.");
#endif
        }
        
        /// <summary>Checks if the given begin and end indices are valid for the list.</summary>
        /// <param name="begin">The starting index.</param>
        /// <param name="end">The ending index.</param>
        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void CheckBeginEnd(int begin, int end)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (begin < 0 || begin > end || end > m_Count)
                throw new ArgumentOutOfRangeException($"Value for begin {begin} and/or end {end} are out of bounds.");
#endif
        }
    }

    class LeanListDebugView<T>
    {
        LeanList<T> m_List;
        
        public LeanListDebugView(LeanList<T> list) => m_List = list;

        public T[] Items => m_List.m_Items[0..m_List.m_Count];
        
        public int Count => m_List.m_Count;
    }
    
    /// <summary>Provides extension methods for LeanList.</summary>
    public static class LeanListExtensions
    {
        /// <summary>Adds a range of items to the end of the LeanList from a pointer to unmanaged memory.</summary>
        /// <remarks>The list will be resized if necessary.</remarks>
        /// <param name="this">The list to add to.</param>
        /// <param name="src">Pointer to the first element in the range.</param>
        /// <param name="count">The number of elements in the range.</param>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        public static unsafe void AddRange<T>(this LeanList<T> @this, T* src, int count) where T : unmanaged
        {
            int startIndex = @this.m_Count;
            if (@this.m_Count + count > @this.Capacity)
            {
                @this.Resize(@this.m_Count + count);
            }
            else
            {
                @this.m_Count += count;
            }

            fixed (T* ptr = @this.m_Items)
            {
                UnsafeUtility.MemCpy(ptr + startIndex, src, count * sizeof(T));
            }
        }
        
        /// <summary>Copies the elements of another list to the end of the list.</summary>
        /// <param name="this">The list to add to.</param>
        /// <param name="unsafeList">The unsafe list to copy from.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void AddRange<T>(this LeanList<T> @this, UnsafeList<T> unsafeList) where T : unmanaged 
            => @this.AddRange(unsafeList.Ptr, unsafeList.Length);
        
        /// <summary>Copies the elements of another list to the end of the list.</summary>
        /// <param name="this">The list to add to.</param>
        /// <param name="nativeArray">The native array to copy from.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void AddRange<T>(this LeanList<T> @this, NativeArray<T> nativeArray) where T : unmanaged 
            => @this.AddRange((T*)nativeArray.GetUnsafeReadOnlyPtr(), nativeArray.Length);
        
        /// <summary>Resizes the list to match the length of the provided span and then overwrites the elements of this list with the elements from the span.</summary>
        /// <param name="this">The list to copy to.</param>
        /// <param name="span">The span to copy from.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void CopyFrom<T>(this LeanList<T> @this, ReadOnlySpan<T> span) where T : unmanaged
        {
            @this.Resize(span.Length);

            if (span.Length > 0)
            {
                fixed (T* dst = @this.m_Items)
                fixed (T* src = span)
                {
                    UnsafeUtility.MemCpy(dst, src, sizeof(T) * span.Length);
                }
            }
        }

        /// <summary>Overwrites the elements of this list with the elements of an equal-length array.</summary>
        /// <param name="this">The list to copy to.</param>
        /// <param name="nativeArray">The native array to copy from.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyFrom<T>(this LeanList<T> @this, NativeArray<T> nativeArray) where T : unmanaged
        {
            @this.Resize(nativeArray.Length);
            NativeArray<T>.Copy(nativeArray, @this.m_Items, nativeArray.Length);
        }

        /// <summary>Overwrites the elements of this list with the elements of an equal-length array.</summary>
        /// <param name="this">The list to copy from.</param>
        /// <param name="nativeArray">The native array to copy to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<T>(this LeanList<T> @this, NativeArray<T> nativeArray) where T : unmanaged
            => NativeArray<T>.Copy(@this.m_Items, nativeArray, @this.m_Count);

        /// <summary>Overwrites the elements of this list with the elements of an equal-length array.</summary>
        /// <param name="this">The list to copy to.</param>
        /// <param name="nativeList">The native list to copy from.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<T>(this LeanList<T> @this, NativeList<T> nativeList) where T : unmanaged
        {
            nativeList.ResizeUninitialized(@this.m_Count);
            NativeArray<T>.Copy(@this.m_Items, nativeList.AsArray(), @this.m_Count);
        }

        /// <summary>Returns true if a particular value matches the predicate in this list.</summary>
        /// <param name="this">The list to search.</param>
        /// <param name="predicate">The predicate to match.</param>
        /// <returns>True if a value matches the predicate, false otherwise.</returns>
        public static bool Any<T>(this LeanList<T> @this, Predicate<T> predicate) where T : unmanaged
        {
            for (int i = 0; i < @this.Count; i++)
            {
                if (predicate(@this[i]))
                    return true;
            }

            return false;
        }

        /// <summary>Returns true if this list and another have equal length and content.</summary>
        /// <param name="this">The list to compare.</param>
        /// <param name="other">The list to compare against.</param>
        /// <returns>True if the lists are equal, false otherwise.</returns>
        public static bool ListsEqual<T>(this LeanList<T> @this, LeanList<T> other) where T : unmanaged, IEquatable<T>
        {
            if (@this.Count != other.Count)
                return false;

            for (int i = 0; i != @this.Count; i++)
            {
                if (!@this[i].Equals(other[i]))
                    return false;
            }

            return true;
        }
    }
}