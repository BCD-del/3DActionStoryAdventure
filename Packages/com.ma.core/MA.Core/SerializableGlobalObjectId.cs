// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace MA.Core
{
    /// <summary>Runtime version of the editor type <seealso cref="UnityEditor.GlobalObjectId"/>. These types need to be binary compatible.</summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct SerializableGlobalObjectId : IEquatable<SerializableGlobalObjectId>, IComparable<SerializableGlobalObjectId>
    {
        /// <summary>Null id.</summary>
        public static readonly SerializableGlobalObjectId Null = default;

        /// <summary>Unique identifier within a scene</summary>
        public long SceneObjectIdentifier0;
        
        /// <summary>Unused.</summary>
        public long SceneObjectIdentifier1;
        
        /// <summary>Asset GUID.</summary>
        public Hash128 AssetGUID;
        
        /// <summary>Identifier type.</summary>
        public int IdentifierType;
        
        /// <summary>True if the id is valid.</summary>
        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this != Null;
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(SerializableGlobalObjectId other)
        {
            int ac = AssetGUID.CompareTo(other.AssetGUID);
            if (ac != 0) return ac;
            return SceneObjectIdentifier0.CompareTo(other.SceneObjectIdentifier0);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(SerializableGlobalObjectId other) => SceneObjectIdentifier0 == other.SceneObjectIdentifier0 && AssetGUID.Equals(other.AssetGUID);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj) => obj is SerializableGlobalObjectId other && Equals(other);

        /// <summary>Returns the hash code of this id.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => unchecked((SceneObjectIdentifier0.GetHashCode() * 397) ^ AssetGUID.GetHashCode());

        /// <summary>Converts the id to a string representation.</summary>
        /// <returns>The string representation of the id in the form $"{AssetGUID}-{SceneObjectIdentifier0}.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => $"{AssetGUID}-{SceneObjectIdentifier0}";

        /// <summary>Compares two ids for equality.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(SerializableGlobalObjectId lhs, SerializableGlobalObjectId rhs) => lhs.Equals(rhs);
        
        /// <summary>Compares two ids for inequality.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(SerializableGlobalObjectId lhs, SerializableGlobalObjectId rhs) => !lhs.Equals(rhs);

#if UNITY_EDITOR
        /// <summary>Implicitly converts a <seealso cref="UnityEditor.GlobalObjectId"/> to a <seealso cref="SerializableGlobalObjectId"/>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator SerializableGlobalObjectId(UnityEditor.GlobalObjectId editorGlobalObjectId) 
            => UnsafeUtility.As<UnityEditor.GlobalObjectId, SerializableGlobalObjectId>(ref editorGlobalObjectId);

        /// <summary>Implicitly converts a <seealso cref="SerializableGlobalObjectId"/> to a <seealso cref="UnityEditor.GlobalObjectId"/>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator UnityEditor.GlobalObjectId(SerializableGlobalObjectId serializableGlobalObjectId) 
            => UnsafeUtility.As<SerializableGlobalObjectId, UnityEditor.GlobalObjectId>(ref serializableGlobalObjectId);
#endif
    }
}
