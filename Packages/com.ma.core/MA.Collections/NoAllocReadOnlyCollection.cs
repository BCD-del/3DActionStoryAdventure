// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MA.Collections
{
    /// <summary>Read only list that doesn't generate garbage when used in a foreach.</summary>
    /// <typeparam name="T">The list element type</typeparam>
    public readonly struct NoAllocReadOnlyList<T> : IEnumerable<T>
    {
        readonly List<T> m_Source;

        /// <summary>Construct a new instance.</summary>
        /// <param name="source">The source list</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NoAllocReadOnlyList(List<T> source) => m_Source = source;

        /// <summary>The number of list elements.</summary>
        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Source.Count;
        }

        /// <summary>Look up a list element by index.</summary>
        /// <param name="index">The list index to look up.</param>
        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Source[index];
        }

        /// <summary>Check if the list contains a specific element.</summary>
        /// <param name="item">The item to search for.</param>
        /// <returns>True if the element is found in the list, or false if not.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T item) => m_Source.Contains(item);

        /// <summary>Get an enumerator interface to the list.</summary>
        /// <returns>A list enumerator.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<T>.Enumerator GetEnumerator() => m_Source.GetEnumerator();

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
            => throw new NotSupportedException($"To avoid boxing, do not cast {nameof(NoAllocReadOnlyList<T>)} to IEnumerable<T>.");
        IEnumerator IEnumerable.GetEnumerator()
            => throw new NotSupportedException($"To avoid boxing, do not cast {nameof(NoAllocReadOnlyList<T>)} to IEnumerable.");
        
        /// <summary>Casts a list to a read only list.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NoAllocReadOnlyList<T>(List<T> source) => new NoAllocReadOnlyList<T>(source);
    }
    
    /// <summary>Read only dictionary that doesn't generate garbage when used in a foreach.</summary>
    /// <typeparam name="TKey">The dictionary key type</typeparam>
    /// <typeparam name="TValue">The dictionary value type</typeparam>
    public readonly struct NoAllocReadOnlyDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>,
        IEnumerable,
        IReadOnlyCollection<KeyValuePair<TKey, TValue>>
    {
        readonly Dictionary<TKey, TValue> m_Source;

        /// <summary>Construct a new instance.</summary>
        /// <param name="source">The source dictionary.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NoAllocReadOnlyDictionary(Dictionary<TKey, TValue> source) => m_Source = source;

        /// <summary>The number of dictionary elements.</summary>
        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Source.Count;
        }

        /// <summary>Look up a dictionary element by key.</summary>
        public TValue this[TKey key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Source[key];
        }

        /// <summary>Get the dictionary keys.</summary>
        public Dictionary<TKey, TValue>.KeyCollection Keys
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Source.Keys;
        }

        /// <summary>Get the dictionary values.</summary>
        public Dictionary<TKey, TValue>.ValueCollection Values
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Source.Values;
        }

        /// <summary>Check if the dictionary contains a specific key.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(TKey key) => m_Source.ContainsKey(key);

        /// <summary>Try to get a dictionary value by key.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(TKey key, out TValue value) => m_Source.TryGetValue(key, out value);

        /// <summary>Get an enumerator interface to the dictionary.</summary>
        /// <returns>A dictionary enumerator</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<TKey, TValue>.Enumerator GetEnumerator() => m_Source.GetEnumerator();

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
            => throw new NotSupportedException($"To avoid boxing, do not cast {nameof(NoAllocReadOnlyDictionary<TKey, TValue>)} to IEnumerable<T>.");
        IEnumerator IEnumerable.GetEnumerator()
            => throw new NotSupportedException($"To avoid boxing, do not cast {nameof(NoAllocReadOnlyDictionary<TKey, TValue>)} to IEnumerable.");
        
        /// <summary>Casts a dictionary to a read only dictionary.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NoAllocReadOnlyDictionary<TKey, TValue>(Dictionary<TKey, TValue> source) => new NoAllocReadOnlyDictionary<TKey, TValue>(source);
    }
}