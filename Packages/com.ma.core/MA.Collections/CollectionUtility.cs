// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Collections;

namespace MA.Collections
{
    static class CollectionUtility
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool ShouldDeallocate(AllocatorManager.AllocatorHandle allocator)
        {
            // Allocator.Invalid == container is not initialized.
            // Allocator.None    == container is initialized, but container doesn't own data.
            return allocator.ToAllocator > Allocator.None;
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void CheckAllocator(AllocatorManager.AllocatorHandle allocator)
        {
            if (!ShouldDeallocate(allocator))
                throw new ArgumentException($"Allocator {allocator} must not be None or Invalid");
        }
    }
}
