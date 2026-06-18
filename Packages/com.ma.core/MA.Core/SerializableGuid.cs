// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace MA.Core
{
    /// <summary>A serializable version of <see cref="System.Guid"/>.</summary>
    [Serializable]
    public struct SerializableGuid : IEquatable<SerializableGuid>, IComparable<SerializableGuid>
    {
        /// <summary>Used to represent <see cref="System.Guid.Empty"/> (that is, a GUID whose values are all zeros).</summary>
        public static readonly SerializableGuid Empty = new SerializableGuid(0, 0);

        /// <summary>The low part of the GUID.</summary>
        [SerializeField] ulong m_GuidLow;
        /// <summary>The high part of the GUID.</summary>
        [SerializeField] ulong m_GuidHigh;

        /// <summary>Constructs a new SerializableGuid from a <see cref="Guid"/>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SerializableGuid(Guid guid)
        {
            GuidUtility.Decompose(guid, out m_GuidLow, out m_GuidHigh);
        }

        /// <summary>Constructs a new SerializableGuid from a <see cref="Hash128"/>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe SerializableGuid(Hash128 guid)
        {
            m_GuidLow = ((ulong*)&guid)[0];
            m_GuidHigh = ((ulong*)&guid)[1];
        }

        /// <summary>Constructs a new SerializableGuid from a <see cref="uint4"/>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SerializableGuid(uint4 guid)
        {
            m_GuidLow = ((ulong)guid.x << 32) | guid.y;
            m_GuidHigh = ((ulong)guid.z << 32) | guid.w;
        }

        /// <summary>Constructs a <see cref="SerializableGuid"/> from two 64-bit <c>ulong</c>s.</summary>
        /// <param name="guidLow">The low 8 bytes of the <see cref="Guid"/>.</param>
        /// <param name="guidHigh">The high 8 bytes of the <see cref="Guid"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SerializableGuid(ulong guidLow, ulong guidHigh)
        {
            m_GuidLow = guidLow;
            m_GuidHigh = guidHigh;
        }

#if UNITY_EDITOR
        /// <summary>Constructs a <see cref="SerializableGuid"/> from a <c>GUID</c>.</summary>
        /// <param name="guid">The editor GUID.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SerializableGuid(UnityEditor.GUID guid) : this(HashUtility.ConvertEditorGuidToHash128(guid)) { }
#endif

        /// <summary>Returns true if this is a valid <see cref="Guid"/> (not empty).</summary>
        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this != Empty;
        }

        /// <summary>Returns a <see cref="Guid"/> version of this <see cref="SerializableGuid"/>.</summary>
        public Guid Guid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GuidUtility.Compose(m_GuidLow, m_GuidHigh);
        }

        /// <summary>Returns a <see cref="Hash128"/> version of this <see cref="SerializableGuid"/>.</summary>
        public Hash128 Hash128
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new Hash128(m_GuidLow, m_GuidHigh);
        }

        /// <summary>Tests for equality.</summary>
        /// <param name="rhs">The other <see cref="SerializableGuid"/> to compare against.</param>
        /// <returns>`True` if every field in <paramref name="rhs"/> is equal to this <see cref="SerializableGuid"/>, otherwise
        /// false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(SerializableGuid rhs)
        {
            return m_GuidLow == rhs.m_GuidLow &&
                   m_GuidHigh == rhs.m_GuidHigh;
        }

        /// <summary>Creates a new <see cref="SerializableGuid"/> from a new <c>Guid</c>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SerializableGuid NewGuid() => new SerializableGuid(Guid.NewGuid());

        /// <summary>Converts a <see cref="Guid"/> to a <see cref="SerializableGuid"/>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator SerializableGuid(Guid from) => new SerializableGuid(from);

        /// <summary>Converts a <see cref="Hash128"/> to a <see cref="SerializableGuid"/>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator SerializableGuid(Hash128 from) => new SerializableGuid(from);

#if UNITY_EDITOR
        public UnityEditor.GUID EditorGUID => HashUtility.ConvertHash128ToEditorGuid(Hash128);
        
        /// <summary>Converts a <see cref="UnityEditor.GUID"/> to a <see cref="SerializableGuid"/>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator SerializableGuid(UnityEditor.GUID from) => new SerializableGuid(from);
#endif

        /// <summary>Generates a string representation of the <c>Guid</c>. Same as <see cref="Guid"/><c>.ToString()</c>.</summary>
        /// <returns>A string representation of the <c>Guid</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => Guid.ToString();

        /// <summary>Generates a string representation of the <c>Guid</c>. Same as <see cref="Guid"/><c>.ToString(format)</c>.</summary>
        /// <param name="format">A single format specifier that indicates how to format the value of the <c>Guid</c>.</param>
        /// <returns>A string representation of the <c>Guid</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(string format) => Guid.ToString(format);

        /// <summary>Generates a string representation of the <c>Guid</c>. Same as <see cref="Guid"/><c>.ToString(format, provider)</c>.</summary>
        /// <param name="format">A single format specifier that indicates how to format the value of the <c>Guid</c>.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <returns>A string representation of the <c>Guid</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(string format, IFormatProvider provider) => Guid.ToString(format, provider);
        

        /// <summary>Compares this <see cref="SerializableGuid"/> to another for sorting purposes.</summary>
        /// <param name="other">The other <see cref="SerializableGuid"/> to compare against.</param>
        /// <returns>
        /// A negative value if this <see cref="SerializableGuid"/> is less than <paramref name="other"/>, zero if they are equal,
        /// or a positive value if this <see cref="SerializableGuid"/> is greater than <paramref name="other"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(SerializableGuid other) => m_GuidHigh < other.m_GuidHigh ? m_GuidHigh.CompareTo(other.m_GuidHigh) : m_GuidLow.CompareTo(other.m_GuidLow);

        /// <summary>Tests for equality.</summary>
        /// <param name="obj">The `object` to compare against.</param>
        /// <returns>`True` if <paramref name="obj"/> is of type <see cref="SerializableGuid"/> and
        /// <see cref="Equals(SerializableGuid)"/> also returns `true`; otherwise `false`.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj) => obj is SerializableGuid converted && Equals(converted);

        /// <summary>Generates a hash suitable for use with containers like `HashSet` and `Dictionary`.</summary>
        /// <returns>A hash code generated from this object's fields.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            // XOR all the bits of the GUID 32 bits at a time.
            return (int)(m_GuidLow & uint.MaxValue) ^
                   (int)(m_GuidLow >> 32) ^
                   (int)(m_GuidHigh & uint.MaxValue) ^
                   (int)(m_GuidHigh >> 32);
        }

        /// <summary>Tests for equality. Same as <see cref="Equals(SerializableGuid)"/>.</summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(SerializableGuid lhs, SerializableGuid rhs) => lhs.Equals(rhs);

        /// <summary>Tests for inequality. Same as `!`<see cref="Equals(SerializableGuid)"/>.</summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is not equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(SerializableGuid lhs, SerializableGuid rhs) => !lhs.Equals(rhs);
    }
}
