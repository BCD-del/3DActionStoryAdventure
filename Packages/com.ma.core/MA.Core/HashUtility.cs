// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using UnityEngine;

namespace MA.Core
{
    /// <summary>Utility class for hashing.</summary>
    public static unsafe class HashUtility
    {
        /// <summary>Converts a System.Guid to a Unity.Hash128.</summary>
        public static Hash128 ConvertSystemGuidToHash128(System.Guid guid)
        {
            Span<byte> bytes = stackalloc byte[16];

            ulong u64_0;
            ulong u64_1;

            if (guid.TryWriteBytes(bytes))
            {
                u64_0 = BitConverter.ToUInt64(bytes[..8]);
                u64_1 = BitConverter.ToUInt64(bytes[8..16]);
            }
            else
            {
                u64_0 = 0;
                u64_1 = 0;
            }

            return new Hash128(u64_0, u64_1);
        }

#if UNITY_EDITOR
        /// <summary>Converts a UnityEditor.GUID to a Unity.Hash128.</summary>
        public static Hash128 ConvertEditorGuidToHash128(UnityEditor.GUID guid) => *(Hash128*)&guid;

        /// <summary>Converts a UnityEditor.GUID to a Unity.Hash128.</summary>
        public static UnityEditor.GUID ConvertHash128ToEditorGuid(Hash128 hash) => *(UnityEditor.GUID*)&hash;
#endif
    }
}
