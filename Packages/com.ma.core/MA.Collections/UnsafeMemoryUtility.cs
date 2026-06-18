// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;

namespace MA.Collections.Unsafe
{
    /// <summary>Utility functions for working with unsafe memory.</summary>
    public static class UnsafeMemoryUtility
    {
        /// <summary>The maximum size of memory that can be allocated.</summary>
        public const long MaximumRamSizeInBytes = 1L << 40; // a terabyte

        /// <summary>Allocates memory.</summary>
        /// <param name="allocator">The allocator to use.</param>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <returns>A pointer to the allocated memory.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe T* Allocate<T>(AllocatorManager.AllocatorHandle allocator) where T : unmanaged
            => Resize<T>(allocator, null, 0, 1);
        
        /// <summary>Allocates memory.</summary>
        /// <param name="count">The number of elements to allocate.</param>
        /// <param name="allocator">The allocator to use.</param>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <returns>A pointer to the allocated memory.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe T* Allocate<T>(AllocatorManager.AllocatorHandle allocator, long count) where T : unmanaged 
            => Resize<T>(allocator, null, 0, count);
        
        /// <summary>Allocates memory.</summary>
        /// <param name="allocator">The allocator to use.</param>
        /// <param name="size"></param>
        /// <param name="align"></param>
        /// <returns>A pointer to the allocated memory.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void* Allocate(AllocatorManager.AllocatorHandle allocator, long size, int align) 
            => Resize(allocator, null, 0, 1, size, align);
        
        /// <summary>Re-allocates memory.</summary>
        /// <param name="allocator">The allocator to use.</param>
        /// <param name="oldPointer">The pointer to the old memory.</param>
        /// <param name="oldCount">The number of elements in the old memory.</param>
        /// <param name="newCount">The number of elements in the new memory.</param>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <returns>The pointer to the new memory.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe T* Resize<T>(AllocatorManager.AllocatorHandle allocator, T* oldPointer, long oldCount, long newCount) where T : unmanaged 
            => (T*)Resize(allocator, oldPointer, oldCount, newCount, UnsafeUtility.SizeOf<T>(), UnsafeUtility.AlignOf<T>());

        /// <summary>Re-allocates memory.</summary>
        /// <param name="allocator">The allocator to use.</param>
        /// <param name="oldPointer">The pointer to the old memory.</param>
        /// <param name="oldCount">The number of elements in the old memory.</param>
        /// <param name="newCount">The number of elements in the new memory.</param>
        /// <param name="size"></param>
        /// <param name="align"></param>
        /// <returns>The pointer to the new memory.</returns>
        public static unsafe void* Resize(AllocatorManager.AllocatorHandle allocator, void* oldPointer, long oldCount, long newCount, long size, int align)
        {
            int alignment = math.max(JobsUtility.CacheLineSize, align);
            
            if (IsCustom(allocator))
                return CustomResize(allocator, oldPointer, oldCount, newCount, size, alignment);
            
            void* newPointer = default;
            if (newCount > 0)
            {
                long bytesToAllocate = newCount * size;
                CheckByteCountIsReasonable(bytesToAllocate);
#if UNITY_2022_3_OR_NEWER
                newPointer = UnsafeUtility.MallocTracked(bytesToAllocate, alignment, allocator.ToAllocator, 0);
#else
                newPointer = UnsafeUtility.Malloc(bytesToAllocate, alignment, allocator.ToAllocator);
#endif
                if (oldCount > 0)
                {
                    long copyCount = math.min(oldCount, newCount);
                    long bytesToCopy = copyCount * size;
                    CheckByteCountIsReasonable(bytesToCopy);
                    UnsafeUtility.MemCpy(newPointer, oldPointer, bytesToCopy);
                }
            }

            if (oldCount > 0)
            {
#if UNITY_2022_3_OR_NEWER
                UnsafeUtility.FreeTracked(oldPointer, allocator.ToAllocator);
#else
                UnsafeUtility.Free(oldPointer, allocator.ToAllocator);
#endif
            }
            
            return newPointer;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe void* CustomResize(AllocatorManager.AllocatorHandle allocator, void* oldPointer, long oldCount, long newCount, long size, int align)
        {
            AllocatorManager.Block block = default;
            block.Range.Allocator = allocator;
            block.Range.Items = (int)newCount;
            block.Range.Pointer = (IntPtr)oldPointer;
            block.BytesPerItem = (int)size;
            block.Alignment = align;
            block.AllocatedItems = (int)oldCount;
            int error = AllocatorManager.Try(ref block);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (error != 0) throw new ArgumentException("failed to allocate");
#endif
            return (void*)block.Range.Pointer;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool IsCustom(AllocatorManager.AllocatorHandle allocator) => (int) allocator.Index >= AllocatorManager.FirstUserIndex;

        /// <summary>Frees memory allocated with <see cref="Allocate"/>.</summary>
        /// <param name="ptr">The pointer to the memory to free.</param>
        /// <param name="allocator">The allocator handle used to allocate the memory.</param>
        /// <typeparam name="T">The type of the elements.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Free<T>(AllocatorManager.AllocatorHandle allocator, T* ptr) where T : unmanaged 
        {
            if (ptr == null) return;
            Resize(allocator, ptr, 1, 0);
        }

        /// <summary>Frees memory allocated with <see cref="Allocate"/>.</summary>
        /// <param name="ptr">The pointer to the memory to free.</param>
        /// <param name="allocator">The allocator handle used to allocate the memory.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Free(AllocatorManager.AllocatorHandle allocator, void* ptr)
        {
            if (ptr == null) return;
            Resize(allocator, ptr, 1, 0, 1, 1);
        }

        /// <summary>Clears memory.</summary>
        /// <param name="dst">The destination pointer.</param>
        /// <param name="size">The number of bytes to clear.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Clear(void* dst, long size)
            => UnsafeUtility.MemClear(dst, size);

        /// <summary>Clears memory.</summary>
        /// <param name="dst">The destination pointer.</param>
        /// <param name="count">The number of elements to clear.</param>
        /// <typeparam name="T">The type of the elements.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Clear<T>(T* dst, long count) where T : unmanaged 
            => UnsafeUtility.MemClear(dst, count * sizeof(T));

        /// <summary>Copies memory from one location to another.</summary>
        /// <param name="dst">The destination pointer.</param>
        /// <param name="src">The source pointer.</param>
        /// <param name="count">The number of elements to copy.</param>
        /// <typeparam name="T">The type of the elements.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Copy<T>(T* dst, T* src, long count) where T : unmanaged 
            => Copy((void*)dst, (void*)src, sizeof(T) * count);
        
        /// <summary>Copies memory from one location to another.</summary>
        /// <param name="dst">The destination pointer.</param>
        /// <param name="src">The source pointer.</param>
        /// <param name="sizeInBytes">The number of elements to copy.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Copy(IntPtr dst, IntPtr src, long sizeInBytes)
            => Copy((void*)dst, (void*)src, sizeInBytes);

        /// <summary>Copies memory from one location to another.</summary>
        /// <param name="dst">The destination pointer.</param>
        /// <param name="src">The source pointer.</param>
        /// <param name="sizeInBytes">The number of elements to copy.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Copy(void* dst, void* src, long sizeInBytes) 
            => UnsafeUtility.MemCpy(dst, src, sizeInBytes);
        
        /// <summary>Fills memory with a value.</summary>
        /// <param name="dst">The destination pointer.</param>
        /// <param name="value">The value to fill the memory with.</param>
        /// <param name="count">The number of elements to fill.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Replicate<T>(T* dst, T value, int count) where T : unmanaged 
            => UnsafeUtility.MemCpyReplicate(dst, &value, sizeof(T), count);
        
        /// <summary>Fills memory with a value.</summary>
        /// <param name="dst">The destination pointer.</param>
        /// <param name="value">The value to fill the memory with.</param>
        /// <param name="count">The number of elements to fill.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Set<T>(T* dst, byte value, long count) where T : unmanaged 
            => UnsafeUtility.MemSet(dst, value, count * sizeof(T));
        
        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void CheckByteCountIsReasonable(long size)
        {
            if (size < 0)
                throw new InvalidOperationException($"Attempted to operate on {size} bytes of memory: negative size");
            if (size > MaximumRamSizeInBytes)
                throw new InvalidOperationException($"Attempted to operate on {size} bytes of memory: size too big");
        }
    }
}