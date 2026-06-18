// Copyright © Magnetic Arcade. All Rights Reserved.

using System.Runtime.CompilerServices;
using Unity.Collections;

namespace MA.Core.Bridge
{
    static class CollectionsBridge
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Allocator GetAllocatorLabel<T>(in this NativeArray<T> array) where T : struct => array.m_AllocatorLabel;
    }
}