// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace MA.Collections
{
    /// <summary>A hash map that allows multiple values per key.</summary>
    /// <typeparam name="TKey">The type of the keys in the map.</typeparam>
    /// <typeparam name="TValue">The type of the values in the map.</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    public sealed class MultiHashMap<TKey, TValue> : IEnumerable<MultiHashMap<TKey, TValue>.KeyValue>
    {
        TValue[] m_Values;
        TKey[] m_Keys;
        int[] m_Next;
        int[] m_Buckets;
        int m_Count;
        int m_KeyCapacity;
        int m_BucketCapacityMask;
        int m_AllocatedIndexLength;
        int m_FirstFreeIndex;

        /// <summary>Initializes a new instance of the <see cref="MultiHashMap{TKey, TValue}"/> class.</summary>
        /// <param name="capacity">The initial capacity of the hash map.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MultiHashMap(int capacity = 16)
        {
            m_KeyCapacity = math.min(capacity, 16);
            int bucketCapacity = math.ceilpow2(capacity);
            m_BucketCapacityMask = bucketCapacity - 1;

            m_Values = new TValue[m_KeyCapacity];
            m_Keys = new TKey[m_KeyCapacity];
            m_Next = new int[m_KeyCapacity];
            m_Buckets = new int[bucketCapacity];

            Clear();
        }

        /// <summary>Gets or sets the capacity of the hash map.</summary>
        /// <value>The number of key-value pairs that fit in the current allocation.</value>
        /// <param name="value">A new capacity. Must be larger than the current capacity.</param>
        /// <exception cref="InvalidOperationException">Thrown if `value` is less than the current capacity.</exception>
        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_KeyCapacity;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Realloc(value, GetBucketSize(value));
        }

        /// <summary>Whether this hash map is empty.</summary>
        /// <value>True if this hash map is empty or the hash map has not been constructed.</value>
        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Count == 0;
        }

        /// <summary>Returns the current number of key-value pairs in this hash map.</summary>
        /// <remarks>Key-value pairs with matching keys are counted as separate, individual pairs.</remarks>
        /// <returns>The current number of key-value pairs in this hash map.</returns>
        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Count;
        }

        /// <summary>Re-allocates the hash map to the specified capacity.</summary>
        /// <param name="newCapacity">The new capacity of the hash map.</param>
        /// <param name="newBucketCapacity">The new capacity of the buckets.</param>
        void Realloc(int newCapacity, int newBucketCapacity)
        {
            newBucketCapacity = math.ceilpow2(newBucketCapacity);
            if (m_KeyCapacity == newCapacity && (m_BucketCapacityMask + 1) == newBucketCapacity)
                return;

            TValue[] newValues = new TValue[newCapacity];
            TKey[] newKeys = new TKey[newCapacity];
            int[] newNext = new int[newCapacity];
            int[] newBuckets = new int[newBucketCapacity];

            Array.Copy(m_Values, newValues, m_KeyCapacity);
            Array.Copy(m_Keys, newKeys, m_KeyCapacity);
            Array.Copy(m_Next, newNext, m_KeyCapacity);

            for (int emptyNext = m_KeyCapacity; emptyNext < newCapacity; ++emptyNext)
                newNext[emptyNext] = -1;

            // re-hash the buckets, first clear the new bucket list, then insert all values from the old list
            for (int bucket = 0; bucket < newBucketCapacity; ++bucket)
                newBuckets[bucket] = -1;

            for (int bucket = 0; bucket <= m_BucketCapacityMask; ++bucket)
            {
                while (m_Buckets[bucket] >= 0)
                {
                    int curEntry = m_Buckets[bucket];
                    m_Buckets[bucket] = newNext[curEntry];
                    int newBucket = m_Keys[curEntry].GetHashCode() & (newBucketCapacity - 1);
                    newNext[curEntry] = newBuckets[newBucket];
                    newBuckets[newBucket] = curEntry;
                }
            }

            if (m_AllocatedIndexLength > m_KeyCapacity)
                m_AllocatedIndexLength = m_KeyCapacity;

            m_Values = newValues;
            m_Keys = newKeys;
            m_Next = newNext;
            m_Buckets = newBuckets;
            m_KeyCapacity = newCapacity;
            m_BucketCapacityMask = newBucketCapacity - 1;
        }

        /// <summary>Removes all key-value pairs.</summary>
        /// <remarks>Does not change the capacity.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Clear()
        {
            UnsafeUtility.MemSet(UnsafeUtility.AddressOf(ref m_Buckets[0]), 0xFF, (m_BucketCapacityMask + 1) * sizeof(int));
            UnsafeUtility.MemSet(UnsafeUtility.AddressOf(ref m_Next[0]), 0xFF, m_KeyCapacity * sizeof(int));

            m_FirstFreeIndex = -1;
            m_AllocatedIndexLength = 0;
        }

        /// <summary>Adds a new key-value pair.</summary>
        /// <remarks>If a key-value pair with this key is already present, an additional separate key-value pair is added.</remarks>
        /// <param name="key">The key to add.</param>
        /// <param name="item">The value to add.</param>
        public void Add(TKey key, TValue item)
        {
            if (m_AllocatedIndexLength >= m_KeyCapacity && m_FirstFreeIndex < 0)
                Realloc(GrowCapacity(m_KeyCapacity), GetBucketSize(m_KeyCapacity));

            int index = m_FirstFreeIndex;
            if (index >= 0)
            {
                m_FirstFreeIndex = m_Next[index];
            }
            else
            {
                index = m_AllocatedIndexLength++;
            }

            if (index < 0 || index >= m_KeyCapacity)
                throw new InvalidOperationException("Internal HashMap error");

            m_Keys[index] = key;
            m_Values[index] = item;

            int bucket = key.GetHashCode() & m_BucketCapacityMask;
            m_Next[index] = m_Buckets[bucket];
            m_Buckets[bucket] = index;
            m_Count++;
        }

        /// <summary>Adds a range of key-value pairs.</summary>
        /// <remarks>If a key-value pair with a key is already present, an additional separate key-value pair is added.</remarks>
        /// <param name="keyValues">The key-value pairs to add.</param>
        public void AddRange(ReadOnlySpan<KeyValuePair<TKey, TValue>> keyValues)
        {
            int freeCount = m_KeyCapacity - m_AllocatedIndexLength;
            int requiredCount = keyValues.Length;
            if (requiredCount > freeCount)
                Realloc(GrowCapacity(m_KeyCapacity + requiredCount), GetBucketSize(m_KeyCapacity + requiredCount));

            for (int i = 0; i < keyValues.Length; ++i)
            {
                int index = m_FirstFreeIndex;
                if (index >= 0)
                {
                    m_FirstFreeIndex = m_Next[index];
                }
                else
                {
                    index = m_AllocatedIndexLength++;
                }

                if (index < 0 || index >= m_KeyCapacity)
                    throw new InvalidOperationException("Internal HashMap error");

                m_Keys[index] = keyValues[i].Key;
                m_Values[index] = keyValues[i].Value;

                int bucket = keyValues[i].Key.GetHashCode() & m_BucketCapacityMask;
                m_Next[index] = m_Buckets[bucket];
                m_Buckets[bucket] = index;
            }

            m_Count += keyValues.Length;
        }

        /// <summary>Removes a key and its associated value(s).</summary>
        /// <param name="key">The key to remove.</param>
        /// <returns>The number of removed key-value pairs. If the key was not present, returns 0.</returns>
        public int Remove(TKey key)
        {
            if (m_KeyCapacity == 0)
                return 0;

            int bucket = key.GetHashCode() & m_BucketCapacityMask;
            int prev = -1;
            int entryIndex = m_Buckets[bucket];
            int removed = 0;
            EqualityComparer<TKey> kc = EqualityComparer<TKey>.Default;

            while (entryIndex >= 0 && entryIndex < m_KeyCapacity)
            {
                if (kc.Equals(m_Keys[entryIndex], key))
                {
                    ++removed;

                    if (prev < 0)
                    {
                        m_Buckets[bucket] = m_Next[entryIndex];
                    }
                    else
                    {
                        m_Next[prev] = m_Next[entryIndex];
                    }

                    int nextIndex = m_Next[entryIndex];
                    m_Next[entryIndex] = m_FirstFreeIndex;
                    m_FirstFreeIndex = entryIndex;
                    entryIndex = nextIndex;
                }
                else
                {
                    prev = entryIndex;
                    entryIndex = m_Next[entryIndex];
                }
            }

            m_Count -= removed;

            return removed;
        }
        
        /// <summary>Removes all key-value pairs with a particular key and a particular value.</summary>
        /// <remarks>Removes all key-value pairs which have a particular key and which *also have* a particular value. In other words: (key *AND* value) rather than (key *OR* value).</remarks>
        /// <param name="key">The key of the key-value pairs to remove.</param>
        /// <param name="value">The value of the key-value pairs to remove.</param>
        public int Remove(TKey key, TValue value)
        {
            if (m_KeyCapacity == 0)
                return 0;

            int bucket = key.GetHashCode() & m_BucketCapacityMask;
            int prev = -1;
            int entryIndex = m_Buckets[bucket];
            int removed = 0;
            EqualityComparer<TKey> kc = EqualityComparer<TKey>.Default;
            EqualityComparer<TValue> vc = EqualityComparer<TValue>.Default;

            while (entryIndex >= 0 && entryIndex < m_KeyCapacity)
            {
                if (kc.Equals(m_Keys[entryIndex], key) && vc.Equals(m_Values[entryIndex], value))
                {
                    ++removed;

                    if (prev < 0)
                    {
                        m_Buckets[bucket] = m_Next[entryIndex];
                    }
                    else
                    {
                        m_Next[prev] = m_Next[entryIndex];
                    }

                    int nextIndex = m_Next[entryIndex];
                    m_Next[entryIndex] = m_FirstFreeIndex;
                    m_FirstFreeIndex = entryIndex;
                    entryIndex = nextIndex;
                }
                else
                {
                    prev = entryIndex;
                    entryIndex = m_Next[entryIndex];
                }
            }

            m_Count -= removed;
            return removed;
        }
        
        /// <summary>Returns true if a given key is present in this hash map.</summary>
        /// <param name="key">The key to look up.</param>
        /// <returns>True if the key was present in this hash map.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(in TKey key) => TryGetFirstValue(key, out _, out _);

        /// <summary>Sets a new value for an existing key-value pair.</summary>
        /// <param name="item">The new value.</param>
        /// <param name="it">The iterator representing a key-value pair.</param>
        /// <returns>True if a value was overwritten.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SetValue(TValue item, in Iterator it)
        {
            if (it.EntryIndex < 0 || it.EntryIndex >= m_KeyCapacity)
                return false;

            m_Values[it.EntryIndex] = item;
            return true;
        }

        /// <summary>An iterator over all values associated with an individual key in a multi hash map.</summary>
        /// <remarks>The iteration order over the values associated with a key is an implementation detail. Do not rely upon any particular ordering.</remarks>
        public struct Iterator
        {
            internal TKey Key;
            internal int NextEntryIndex;
            internal int EntryIndex;

            /// <summary>
            /// Returns the entry index.
            /// </summary>
            /// <returns>The entry index.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int GetEntryIndex() => EntryIndex;
        }

        /// <summary>Gets an iterator for a key.</summary>
        /// <param name="key">The key.</param>
        /// <param name="item">Outputs the associated value represented by the iterator.</param>
        /// <param name="it">Outputs an iterator.</param>
        /// <returns>True if the key was present.</returns>
        public bool TryGetFirstValue(TKey key, out TValue item, out Iterator it)
        {
            it.Key = key;

            if (m_AllocatedIndexLength <= 0)
            {
                it.EntryIndex = it.NextEntryIndex = -1;
                item = default;
                return false;
            }

            // First find the slot based on the hash
            int bucket = key.GetHashCode() & m_BucketCapacityMask;
            it.EntryIndex = it.NextEntryIndex = m_Buckets[bucket];
            return TryGetNextValue(out item, ref it);
        }

        /// <summary>Advances an iterator to the next value associated with its key.</summary>
        /// <param name="item">Outputs the next value.</param>
        /// <param name="it">A reference to the iterator to advance.</param>
        /// <returns>True if the key was present and had another value.</returns>
        public bool TryGetNextValue(out TValue item, ref Iterator it)
        {
            int entryIdx = it.NextEntryIndex;
            it.NextEntryIndex = -1;
            it.EntryIndex = -1;
            item = default;
            if (entryIdx < 0 || entryIdx >= m_KeyCapacity)
            {
                return false;
            }

            while (!m_Keys[entryIdx].Equals(it.Key))
            {
                entryIdx = m_Next[entryIdx];
                if (entryIdx < 0 || entryIdx >= m_KeyCapacity)
                {
                    return false;
                }
            }

            it.NextEntryIndex = m_Next[entryIdx];
            it.EntryIndex = entryIdx;

            // Read the value
            item = m_Values[entryIdx];

            return true;
        }

        /// <summary>Returns the number of values associated with a given key.</summary>
        /// <param name="key">The key to look up.</param>
        /// <returns>The number of values associated with the key. Returns 0 if the key was not present.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CountValuesForKey(TKey key)
        {
            if (!TryGetFirstValue(key, out _, out Iterator iterator))
                return 0;

            int count = 1;
            while (TryGetNextValue(out _, ref iterator))
                count++;

            return count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool MoveNextSearch(ref int bucketIndex, ref int nextIndex, out int index)
        {
            for (int i = bucketIndex; i <= m_BucketCapacityMask; ++i)
            {
                int idx = m_Buckets[i];
                if (idx != -1)
                {
                    index = idx;
                    bucketIndex = i + 1;
                    nextIndex = m_Next[idx];
                    return true;
                }
            }

            index = -1;
            bucketIndex = m_BucketCapacityMask + 1;
            nextIndex = -1;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool MoveNext(ref int bucketIndex, ref int nextIndex, out int index)
        {
            if (nextIndex != -1)
            {
                index = nextIndex;
                nextIndex = m_Next[nextIndex];
                return true;
            }

            return MoveNextSearch(ref bucketIndex, ref nextIndex, out index);
        }
        
        /// <summary>Gets the keys of this hash map.</summary>
        /// <param name="result">The list to add the keys to.</param>
        public void GetAllKeys(List<TKey> result)
        {
            for (int i = 0, count = 0; i <= m_BucketCapacityMask && count < m_Count; ++i)
            {
                int bucket = m_Buckets[i];

                while (bucket != -1)
                {
                    result.Add(m_Keys[bucket]);
                    bucket = m_Next[bucket];
                }
            }
        }
        
        /// <summary>Gets the keys of this hash map.</summary>
        /// <returns>The keys of this hash map as an array.</returns>
        public TKey[] GetKeyArray()
        {
            TKey[] result = new TKey[m_Count];
            for (int i = 0, count = 0; i <= m_BucketCapacityMask && count < m_Count; ++i)
            {
                int bucket = m_Buckets[i];

                while (bucket != -1)
                {
                    result[count++] = m_Keys[bucket];
                    bucket = m_Next[bucket];
                }
            }

            return result;
        }
        
        static HashSet<TKey> s_UniqueKeysBuffer = new();
        
        /// <summary>Gets the unique keys of this hash map.</summary>
        /// <returns>The keys of this hash map as an array.</returns>
        public TKey[] GetUniqueKeyArray()
        {
            s_UniqueKeysBuffer.Clear();
            
            for (int i = 0, count = 0; i <= m_BucketCapacityMask && count < m_Count; ++i)
            {
                int bucket = m_Buckets[i];

                while (bucket != -1)
                {
                    s_UniqueKeysBuffer.Add(m_Keys[bucket]);
                    bucket = m_Next[bucket];
                }
            }

            return s_UniqueKeysBuffer.ToArray();
        }

        /// <summary>Gets the values of this hash map.</summary>
        /// <param name="result">The list to add the values to.</param>
        public void GetAllValues(List<TValue> result)
        {
            for (int i = 0, count = 0, max = m_Count, capacityMask = m_BucketCapacityMask
                 ; i <= capacityMask && count < max
                 ; ++i
                )
            {
                int bucket = m_Buckets[i];

                while (bucket != -1)
                {
                    result[count++] = m_Values[bucket];
                    bucket = m_Next[bucket];
                }
            }
        }
        
        /// <summary>Gets the values of this hash map.</summary>
        /// <returns>The values of this hash map as an array.</returns>
        public TValue[] GetValueArray()
        {
            TValue[] result = new TValue[m_Count];
            for (int i = 0, count = 0, max = m_Count, capacityMask = m_BucketCapacityMask
                 ; i <= capacityMask && count < max
                 ; ++i
                )
            {
                int bucket = m_Buckets[i];

                while (bucket != -1)
                {
                    result[count++] = m_Values[bucket];
                    bucket = m_Next[bucket];
                }
            }

            return result;
        }
        
        /// <summary>Returns an enumerator over the values of an individual key.</summary>
        /// <param name="key">The key to get an enumerator for.</param>
        /// <returns>An enumerator over the values of a key.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetValuesForKey(TKey key) => new() { Hashmap = this, Key = key, IsFirst = true };

        /// <summary>An enumerator over the values of an individual key in a multi hash map.</summary>
        public struct Enumerator : IEnumerator<TValue>
        {
            internal MultiHashMap<TKey, TValue> Hashmap;
            internal TKey Key;
            internal bool IsFirst;

            TValue m_Value;
            Iterator m_Iterator;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose() { }

            /// <summary>Advances the enumerator to the next value of the key.</summary>
            /// <returns>True if <see cref="Current"/> is valid to read after the call.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                // Avoids going beyond the end of the collection.
                if (IsFirst)
                {
                    IsFirst = false;
                    return Hashmap.TryGetFirstValue(Key, out m_Value, out m_Iterator);
                }

                return Hashmap.TryGetNextValue(out m_Value, ref m_Iterator);
            }

            /// <summary>Resets the enumerator to its initial state.</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset() => IsFirst = true;

            /// <summary>Returns this enumerator.</summary>
            /// <returns>This enumerator.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Enumerator GetEnumerator() { return this; }

            /// <summary>The current value.</summary>
            /// <value>The current value.</value>
            public TValue Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => m_Value;
            }

            object IEnumerator.Current => Current;
        }

        /// <summary>Returns an enumerator over the key-value pairs of this hash map.</summary>
        /// <remarks>A key with *N* values is visited by the enumerator *N* times.</remarks>
        /// <returns>An enumerator over the key-value pairs of this hash map.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyValueEnumerator GetEnumerator() => new(this);

        /// <summary>This method is not implemented. Use <see cref="GetEnumerator"/> instead.</summary>
        /// <returns>Throws NotImplementedException.</returns>
        /// <exception cref="NotImplementedException">Method is not implemented.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator<KeyValue> IEnumerable<KeyValue>.GetEnumerator() => throw new NotImplementedException();

        /// <summary>This method is not implemented. Use <see cref="GetEnumerator"/> instead.</summary>
        /// <returns>Throws NotImplementedException.</returns>
        /// <exception cref="NotImplementedException">Method is not implemented.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();

        /// <summary>An enumerator over the key-value pairs of a multi hash map.</summary>
        /// <remarks>A key with *N* values is visited by the enumerator *N* times.</remarks>
        public struct KeyValueEnumerator : IEnumerator<KeyValue>
        {
            internal MultiHashMap<TKey, TValue> m_Map;
            internal int m_Index;
            internal int m_BucketIndex;
            internal int m_NextIndex;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public KeyValueEnumerator(MultiHashMap<TKey, TValue> map)
            {
                m_Map = map;
                m_Index = -1;
                m_BucketIndex = 0;
                m_NextIndex = -1;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext() => m_Map.MoveNext(ref m_BucketIndex, ref m_NextIndex, out m_Index);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset()
            {
                m_Index = -1;
                m_BucketIndex = 0;
                m_NextIndex = -1;
            }

            public KeyValue Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => new KeyValue { m_Map = m_Map, m_Index = m_Index };
            }

            object IEnumerator.Current => Current;
        }

        /// <summary>A key-value pair in a multi hash map.</summary>
        /// <remarks>Used for enumerators.</remarks>
        public struct KeyValue
        {
            internal MultiHashMap<TKey, TValue> m_Map;
            internal int m_Index;
            internal int m_Next;

            /// <summary>An invalid KeyValue.</summary>
            /// <value>In a hash map enumerator's initial state, its <see cref="UnsafeParallelHashMap{TKey,TValue}.Enumerator.Current"/> value is Null.</value>
            public static KeyValue Null => new KeyValue { m_Index = -1 };

            /// <summary>The key.</summary>
            /// <remarks>If this KeyValue is Null, returns the default of TKey.</remarks>
            public TKey Key
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => m_Index != -1 ? m_Map.m_Keys[m_Index] : default;
            }

            /// <summary>Value of key/value pair. </summary>
            public ref TValue Value
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    if (m_Index == -1)
                        throw new ArgumentException("must be valid");

                    return ref m_Map.m_Values[m_Index];
                }
            }

            /// <summary>Gets the key and the value.</summary>
            /// <param name="key">Outputs the key. If this KeyValue is Null, outputs the default of TKey.</param>
            /// <param name="value">Outputs the value. If this KeyValue is Null, outputs the default of TValue.</param>
            /// <returns>True if the key-value pair is valid.</returns>
            public bool GetKeyValue(out TKey key, out TValue value)
            {
                if (m_Index != -1)
                {
                    key = m_Map.m_Keys[m_Index];
                    value = m_Map.m_Values[m_Index];
                    return true;
                }

                key = default;
                value = default;
                return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int GetBucketSize(int capacity) => capacity * 2;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int GrowCapacity(int capacity) => capacity == 0 ? 1 : capacity * 2;
    }
}