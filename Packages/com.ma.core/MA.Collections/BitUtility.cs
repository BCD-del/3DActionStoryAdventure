// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using MA.Mathematics;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace MA.Collections
{
    [BurstCompile]
    public static unsafe class BitUtility
    {
        /// <summary>Returns the integer equivalent of the specified boolean value.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FromBool(bool value) => value ? 1 : 0;

        /// <summary>Aligns the specified value to the next power of two.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int AlignDown(int value, int alignPow2) => value & ~(alignPow2 - 1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int AlignUp(int value, int alignPow2) => AlignDown(value + alignPow2 - 1, alignPow2);

        /// <summary>Returns number of leading zeros in the binary representations of a <see cref="byte"/> value.</summary>
        /// <seealso cref="math.lzcnt(uint)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Lzcnt(byte value) => math.lzcnt((uint)value) - 24;

        /// <summary>Returns number of trailing zeros in the binary representations of a <see cref="byte"/> value.</summary>
        /// <seealso cref="math.lzcnt(uint)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Tzcnt(byte value) => math.min(8, math.tzcnt((uint)value));

        /// <summary>Returns number of leading zeros in the binary representations of a <see cref="ushort"/> value.</summary>
        /// <seealso cref="math.tzcnt(uint)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Lzcnt(ushort value) => math.lzcnt((uint)value) - 16;

        /// <summary>Returns number of trailing zeros in the binary representations of a <see cref="ushort"/> value.</summary>
        /// <seealso cref="math.tzcnt(uint)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Tzcnt(ushort value) => math.min(16, math.tzcnt((uint)value));

        /// <summary>Gets the mask for the last word in a <see cref="ulong"/> bit array of the specified length.</summary>
        /// <param name="length">The length of the bit array.</param>
        /// <returns>The mask for the last word.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong GetLastULongMask(int length) => ~0ul >> (64 - length % 64) % 64;

        /// <summary>Replaces the bits in the input with the specified value using the specified mask.</summary>
        /// <param name="input">The input value.</param>
        /// <param name="bit">The index of the first bit to replace.</param>
        /// <param name="mask">The mask to use for the replacement.</param>
        /// <param name="value">The value to replace the bits with.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ReplaceBits(ulong input, int bit, ulong mask, ulong value)
        {
            ulong tmp0 = (value & mask) << bit;
            ulong tmp1 = input & ~(mask << bit);
            return tmp0 | tmp1;
        }

        /// <summary>Extracts the bits from the input using the specified mask.</summary>
        /// <param name="input">The input value.</param>
        /// <param name="bit">The index of the first bit to extract.</param>
        /// <param name="mask">The mask to use for the extraction.</param>
        /// <returns>The extracted bits.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ExtractBits(ulong input, int bit, ulong mask)
        {
            ulong tmp0 = input >> bit;
            return tmp0 & mask;
        }

        /// <summary>Checks if the specified bit is set in the bit array.</summary>
        /// <param name="ulongs">A pointer to the bits stored in <see cref="ulong"/> ulongs.</param>
        /// <param name="maxBits">The length of the bit array.</param>
        /// <param name="bitIndex">The index of the bit to check.</param>
        /// <exception cref="ArgumentException">Thrown if the bit index is out of range.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSet(ulong* ulongs, int maxBits, int bitIndex)
        {
            CheckIndexCount(maxBits, bitIndex, 1);
            int ulongIndex = bitIndex >> 6;
            int shift = bitIndex & 0x3f;
            ulong mask = 1ul << shift;
            return 0ul != (ulongs[ulongIndex] & mask);
        }

        /// <summary>Sets the specified bit in the bit array to the specified value.</summary>
        /// <param name="ulongs">A pointer to the bits stored in <see cref="ulong"/> ulongs.</param>
        /// <param name="maxBits">The length of the bit array.</param>
        /// <param name="bitIndex">The index of the bit to set.</param>
        /// <param name="value">The value to set the bit to.</param>
        /// <exception cref="IndexOutOfRangeException">Thrown if the bit index is out of range.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Set(ulong* ulongs, int maxBits, int bitIndex, bool value)
        {
            CheckIndexCount(maxBits, bitIndex, 1);
            int ulongIndex = bitIndex >> 6;
            int shift = bitIndex & 0x3f;
            ulong mask = 1ul << shift;
            ulong bits = (ulongs[ulongIndex] & ~mask) | ((ulong)FromBool(value) << shift);
            ulongs[ulongIndex] = bits;
        }

        /// <summary>Atomically sets the bit at the specified index to the specified value.</summary>
        /// <param name="ulongs">A pointer to the bits stored in <see cref="ulong"/> ulongs.</param>
        /// <param name="maxBits">The length of the bit array.</param>
        /// <param name="index">The index of the bit to set.</param>
        /// <param name="value">The value to set the bit to.</param>
        /// <exception cref="IndexOutOfRangeException">Thrown if the bit index is out of range.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetAtomic(long* ulongs, int maxBits, int index, bool value)
        {
            CheckIndexCount(maxBits, index, 1);
            int ulongIndex = index >> 6;

            ulong bit = 1ul << (index & 0x3f);
            long test = (long)(~bit);
            long mask = value ? (long)bit : 0;

            long oldChunk, newChunk;
            do
            {
                oldChunk = Interlocked.Read(ref ulongs[ulongIndex]);
                newChunk = (oldChunk & test) | mask;
            } while (Interlocked.CompareExchange(ref ulongs[ulongIndex], newChunk, oldChunk) != oldChunk);
        }

        /// <summary>Sets the specified bits in the bit array to the specified value using the specified mask.</summary>
        /// <param name="ulongs">A pointer to the bits stored in <see cref="ulong"/> ulongs.</param>
        /// <param name="maxBits">The length of the bit array.</param>
        /// <param name="bit">The index of the first bit to get.</param>
        /// <param name="numBits">The number of bits to get.</param>
        /// <returns>The bits extracted from the bit array.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown if the bit index is out of range.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong GetBits(ulong* ulongs, int maxBits, int bit, int numBits = 1)
        {
            CheckRange(maxBits, bit, numBits);

            int idxB = bit >> 6;
            int shiftB = bit & 0x3f;

            if (shiftB + numBits <= 64)
            {
                ulong mask = 0xfffffffffffffffful >> (64 - numBits);
                return ExtractBits(ulongs[idxB], shiftB, mask);
            }

            int end = math.min(bit + numBits, maxBits);
            int idxE = (end - 1) >> 6;
            int shiftE = end & 0x3f;

            ulong maskB = 0xfffffffffffffffful >> shiftB;
            ulong valueB = ExtractBits(ulongs[idxB], shiftB, maskB);

            ulong maskE = 0xfffffffffffffffful >> (64 - shiftE);
            ulong valueE = ExtractBits(ulongs[idxE], 0, maskE);

            return (valueE << (64 - shiftB)) | valueB;
        }

        /// <summary>Sets a range of bits to 0 or 1.</summary>
        /// <remarks>The range runs from index `pos` up to (but not including) `pos + numBits`.</remarks>
        /// <param name="ulongs">A pointer to the bits stored in <see cref="ulong"/> ulongs.</param>
        /// <param name="maxBits">The length of the bit array.</param>
        /// <param name="bit">Index of the first bit to set.</param>
        /// <param name="value">True for 1, false for 0.</param>
        /// <param name="numBitsToSet">Number of bits to set.</param>
        /// <exception cref="IndexOutOfRangeException">Thrown if pos is out of bounds or if numBits is less than 1.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetBits(ulong* ulongs, int maxBits, int bit, bool value, int numBitsToSet)
        {
            if (numBitsToSet == 0)
                return;

            CheckRange(maxBits, bit, numBitsToSet);

            const ulong ulongMask = 0xfffffffffffffffful;

            // Work out which ulong index to set from, and how many
            int startIndex = bit / 64;
            int count      = (bit + numBitsToSet + (64 - 1)) / 64 - startIndex;

            // Work out masks for the start/end of the sequence
            ulong startMask  = ulongMask << (bit & 0x3f);
            ulong endMask    = ulongMask >> (64 - (bit + numBitsToSet) % 64) % 64;

            ulong* ptr = ulongs + startIndex;
            if (count == 1)
            {
                ulong mask = startMask & endMask;
                if (value)
                    ptr[0] |= mask;
                else
                    ptr[0] &= ~mask;
            }
            else
            {
                if (value)
                {
                    ptr[0] |= startMask;
                    for (int i = 1; i < count - 1; i++)
                        ptr[i] = ulongMask;

                    ptr[count - 1] |= endMask;
                }
                else
                {
                    ptr[0] &= ~startMask;
                    for (int i = 1; i < count - 1; i++)
                        ptr[i] = 0;

                    ptr[count - 1] &= ~endMask;
                }
            }
        }

        /// <summary>Copies bits of a ulong to bits in this array.</summary>
        /// <remarks>
        /// The destination bits in this array run from index `pos` up to (but not including) `pos + numBits`.
        /// No exception is thrown if `pos + numBits` exceeds the length.
        ///
        /// The lowest bit of the ulong is copied to the first destination bit; the second-lowest bit of the ulong is
        /// copied to the second destination bit; and so forth.
        /// </remarks>
        /// <param name="ulongs">A pointer to the bits stored in <see cref="ulong"/> ulongs.</param>
        /// <param name="maxBits">The length of the bit array.</param>
        /// <param name="bit">Index of the first bit to set.</param>
        /// <param name="value">Unsigned long from which to copy bits.</param>
        /// <param name="numBitsToSet">Number of bits to set (must be between 1 and 64).</param>
        /// <exception cref="ArgumentException">Thrown if pos is out of bounds or if numBits is not between 1 and 64.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetBits(ulong* ulongs, int maxBits, int bit, ulong value, int numBitsToSet = 1)
        {
            CheckArgsUlong(maxBits, bit, numBitsToSet);

            int idxB = bit >> 6;
            int shiftB = bit & 0x3f;

            if (shiftB + numBitsToSet <= 64)
            {
                ulong mask = 0xfffffffffffffffful >> (64 - numBitsToSet);
                ulongs[idxB] = ReplaceBits(ulongs[idxB], shiftB, mask, value);

                return;
            }

            int end = math.min(bit + numBitsToSet, maxBits);
            int idxE = (end - 1) >> 6;
            int shiftE = end & 0x3f;

            ulong maskB = 0xfffffffffffffffful >> shiftB;
            ulongs[idxB] = ReplaceBits(ulongs[idxB], shiftB, maskB, value);

            ulong valueE = value >> (64 - shiftB);
            ulong maskE = 0xfffffffffffffffful >> (64 - shiftE);
            ulongs[idxE] = ReplaceBits(ulongs[idxE], 0, maskE, valueE);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void CopyUlong(ulong* dstChunks, int dstLength, int dstPos, ulong* srcChunks, int srcLength, int srcPos, int numBits)
            => SetBits(dstChunks, dstLength, dstPos, GetBits(srcChunks, srcLength, srcPos, numBits), numBits);

        /// <summary>
        /// Copies a range of bits from an array to a range of bits in this array.
        /// </summary>
        /// <remarks>
        /// The bits to copy in the source array run from index srcPos up to (but not including) `srcPos + numBits`.
        /// The bits to set in the destination array run from index dstPos up to (but not including) `dstPos + numBits`.
        ///
        /// It's fine if source and destination array are one and the same, even if the ranges overlap, but the result in the overlapping region is undefined.
        /// </remarks>
        /// <param name="dstChunks">The pointer to the destination bit array.</param>
        /// <param name="dstLength">The length of the destination bit array.</param>
        /// <param name="dstPos">Index of the first bit to set.</param>
        /// <param name="srcChunks">The pointer to the source bit array.</param>
        /// <param name="srcLength">The length of the source bit array.</param>
        /// <param name="srcPos">Index of the first bit to copy.</param>
        /// <param name="numBits">The number of bits to copy.</param>
        /// <exception cref="ArgumentException">Thrown if either `dstPos + numBits` or `srcBitArray + numBits` exceed the length of this array.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Copy(ulong* dstChunks, int dstLength, int dstPos, ulong* srcChunks, int srcLength, int srcPos, int numBits)
        {
            if (numBits == 0)
                return;

            CheckArgsCopy(dstLength, dstPos, srcLength, srcPos, numBits);

            if (numBits <= 64) // 1x CopyUlong
            {
                CopyUlong(dstChunks, dstLength, dstPos, srcChunks, srcLength, srcPos, numBits);
            }
            else if (numBits <= 128) // 2x CopyUlong
            {
                CopyUlong(dstChunks, dstLength, dstPos, srcChunks, srcLength, srcPos, 64);
                numBits -= 64;

                if (numBits > 0)
                {
                    CopyUlong(dstChunks, dstLength, dstPos + 64, srcChunks, srcLength, srcPos + 64, numBits);
                }
            }
            else if ((dstPos & 7) == (srcPos & 7)) // aligned copy
            {
                int dstPosInBytes = AlignUp(dstPos, 8) >> 3;
                int srcPosInBytes = AlignUp(srcPos, 8) >> 3;
                int numPreBits = dstPosInBytes * 8 - dstPos;

                if (numPreBits > 0)
                {
                    CopyUlong(dstChunks, dstLength, dstPos, srcChunks, srcLength, srcPos, numPreBits);
                }

                int numBitsLeft = numBits - numPreBits;
                int numBytes = numBitsLeft / 8;

                if (numBytes > 0)
                {
                    UnsafeUtility.MemMove((byte*)dstChunks + dstPosInBytes, (byte*)srcChunks + srcPosInBytes, numBytes);
                }

                int numPostBits = numBitsLeft & 7;

                if (numPostBits > 0)
                {
                    CopyUlong(dstChunks, dstLength, (dstPosInBytes + numBytes) * 8, srcChunks, srcLength, (srcPosInBytes + numBytes) * 8, numPostBits);
                }
            }
            else // unaligned copy
            {
                int dstPosAligned = AlignUp(dstPos, 64);
                int numPreBits = dstPosAligned - dstPos;

                if (numPreBits > 0)
                {
                    CopyUlong(dstChunks, dstLength, dstPos, srcChunks, srcLength, srcPos, numPreBits);
                    numBits -= numPreBits;
                    dstPos += numPreBits;
                    srcPos += numPreBits;
                }

                for (; numBits >= 64; numBits -= 64, dstPos += 64, srcPos += 64)
                {
                    dstChunks[dstPos >> 6] = GetBits(srcChunks, srcLength, srcPos, 64);
                }

                if (numBits > 0)
                {
                    CopyUlong(dstChunks, dstLength, dstPos, srcChunks, srcLength, srcPos, numBits);
                }
            }
        }

        /// <summary>Returns true if the two bit arrays are equal.</summary>
        /// <param name="aChunks">The pointer to the first bit array.</param>
        /// <param name="bChunks">The pointer to the second bit array.</param>
        /// <param name="maxBits">The length of the bit arrays.</param>
        /// <returns>True if the two bit-arrays are equal; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AreEqual(ulong* aChunks, ulong* bChunks, int maxBits)
        {
            int ulongCount = MathUtility.DivideAndRoundUp(maxBits, 64);
            int lastUlongIndex = ulongCount - 1;
            ulong lastULongMask = GetLastULongMask(maxBits);

            for (int i = 0; i < ulongCount; i++)
            {
                ulong a = aChunks[i];
                ulong b = bChunks[i];

                if (i == lastUlongIndex)
                {
                    a &= lastULongMask;
                    b &= lastULongMask;
                }

                if (a != b)
                    return false;
            }

            return true;
        }

        /// <summary>Returns the hash code for the specified bit array.</summary>
        /// <param name="ulongs">A pointer to the bits stored in <see cref="ulong"/> ulongs.</param>
        /// <param name="maxBits">The length of the bit array.</param>
        /// <returns>The hash code for the bit array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Hash(ulong* ulongs, int maxBits)
        {
            int ulongCount = MathUtility.DivideAndRoundUp(maxBits, 64);
            int lastChunkIndex = ulongCount - 1;
            ulong lastChunkMask = GetLastULongMask(maxBits);

            ulong hash = (ulong)ulongCount;

            unchecked
            {
                for (int i = 0; i < ulongCount; i++)
                {
                    ulong value = ulongs[i];
                    if (i == lastChunkIndex)
                        value &= lastChunkMask;
                    hash ^= value;
                }
            }

            return hash.GetHashCode();
        }

        /// <summary>Counts the number of bits set in the bit array.</summary>
        /// <param name="ulongs">A pointer to the bits stored in <see cref="ulong"/> ulongs.</param>
        /// <param name="maxBits">The length of the bit array.</param>
        /// <param name="bit">The starting position to count bits from.</param>
        /// <param name="numBits">The number of bits to count.</param>
        /// <returns>The number of bits set in the specified range.</returns>
        /// <exception cref="ArgumentException">Thrown if the bit index or count is out of range.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountBits(ulong* ulongs, int maxBits, int bit, int numBits = 1)
        {
            CheckRange(maxBits, bit, numBits);
            int end = math.min(bit + numBits, maxBits);

            int idxB = bit >> 6;
            int shiftB = bit & 0x3f;

            int idxE = (end - 1) >> 6;
            int shiftE = end & 0x3f;

            ulong maskB = 0xfffffffffffffffful << shiftB;
            ulong maskE = 0xfffffffffffffffful >> (64 - shiftE);

            if (idxB == idxE)
            {
                ulong mask = maskB & maskE;
                return math.countbits(ulongs[idxB] & mask);
            }

            int count = math.countbits(ulongs[idxB] & maskB);

            for (int idx = idxB + 1; idx < idxE; ++idx)
            {
                count += math.countbits(ulongs[idx]);
            }

            count += math.countbits(ulongs[idxE] & maskE);

            return count;
        }

        /// <summary>Searches for the specified boolean value in the bit array, starting from the specified index.</summary>
        /// <param name="ulongs">A pointer to the bits stored in <see cref="ulong"/> ulongs.</param>
        /// <param name="maxBits">The length of the bit array.</param>
        /// <param name="value">The value to search for.</param>
        /// <param name="startIndex">The index to start searching from.</param>
        /// <returns>The index of the first occurrence of the specified value, or -1 if the value is not found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FindFirst(ulong* ulongs, int maxBits, bool value, int startIndex = 0)
        {
            if (startIndex >= maxBits)
                return -1;

            // Create a mask to filter out bits before the startIndex.
            ulong mask = ~0ul << (startIndex % 64);

            // Determine the test value based on whether we are searching for true or false.
            ulong test = value ? 0ul : ~0ul;

            // Calculate the number of 64-bit words (qwords) and the starting qword index.
            int ulongCount = MathUtility.DivideAndRoundUp(maxBits, 64);
            int ulongIndex = startIndex >> 6;

            // Iterate through the qwords and compare them with the test value.
            while (ulongIndex < ulongCount && (ulongs[ulongIndex] & mask) == (test & mask))
            {
                ++ulongIndex;
                mask = ~0ul; // Reset the mask to compare all bits in subsequent qwords.
            }

            // If a matching qword is found, search for the first set (1) bit within that qword.
            if (ulongIndex < ulongCount)
            {
                // If we're looking for a false, then we flip the bits - then we only need to find the first one bit.
                ulong bits = (value ? ulongs[ulongIndex] : ~ulongs[ulongIndex]) & mask;
                int lowestBitIndex = math.tzcnt(bits) + (ulongIndex << 6); // Convert bit index to global index.

                // Check if the found bit index is within the valid range of the bit array.
                if (lowestBitIndex < maxBits)
                    return lowestBitIndex;
            }

            // Return -1 if the value is not found in the specified range.
            return -1;
        }

        /// <summary>Searches for the last occurrence of the specified boolean value in the bit array.</summary>
        /// <param name="ulongs">A pointer to the bits stored in <see cref="ulong"/> ulongs.</param>
        /// <param name="maxBits">The length of the bit array.</param>
        /// <param name="value">The value to search for.</param>
        /// <returns>The index of the last occurrence of the specified value, or -1 if the value is not found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FindLast(ulong* ulongs, int maxBits, bool value)
        {
            // Get the correct mask for the last word
            ulong mask = GetLastULongMask(maxBits);

            // Iterate over the array until we see a word with a zero bit.
            int ulongIndex = MathUtility.DivideAndRoundUp(maxBits, 64);

            ulong test = value ? 0ul : ~0ul;
            for (;;)
            {
                if (ulongIndex == 0)
                    return -1;

                --ulongIndex;

                if ((ulongs[ulongIndex] & mask) != (test & mask))
                    break;

                mask = ~0u;
            }

            // Flip the bits, then we only need to find the first one bit -- easy.
            ulong bits = (value ? ulongs[ulongIndex] : ~ulongs[ulongIndex]) & mask;
            int bit = 63 - math.lzcnt(bits);
            int result = bit + (ulongIndex << 6);
            return result;
        }

        /// <summary>Finds the first zero bit in the bit array starting from the specified index and sets it to true.</summary>
        /// <param name="ulongs">A pointer to the bits stored in <see cref="ulong"/> ulongs.</param>
        /// <param name="maxBits">The length of the bit array.</param>
        /// <param name="startIndex">The index to start searching from.</param>
        /// <returns>The index of the first zero bit found and set to true, or -1 if no zero bit is found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FindAndSetFirstZeroBit(ulong* ulongs, int maxBits, int startIndex = 0)
        {
            // Find the index of the first zero bit.
            int index = FindFirst(ulongs, maxBits, false, startIndex);
            // If a zero bit is found, set it to true.
            if (index != -1) Set(ulongs, maxBits, index, true);
            return index;
        }

        /// <summary>Finds the last zero bit in the bit array and sets it to true.</summary>
        /// <param name="ulongs">A pointer to the bits stored in <see cref="ulong"/> ulongs.</param>
        /// <param name="maxBits">The length of the bit array.</param>
        /// <returns>The index of the last zero bit found and set to true, or -1 if no zero bit is found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FindAndSetLastZeroBit(ulong* ulongs, int maxBits)
        {
            // Find the index of the last zero bit.
            int index = FindLast(ulongs, maxBits, false);
            // If a zero bit is found, set it to true.
            if (index != -1) Set(ulongs, maxBits, index, true);
            return index;
        }

        /// <summary>Provides an enumerator to iterate through the indices of set bits in an UnsafeBitList.</summary>
        /// <param name="ulongs">A pointer to the bits stored in <see cref="ulong"/> ulongs.</param>
        /// <param name="maxBits">The length of the bit array.</param>
        /// <returns>An enumerator to iterate through the set bit indices.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SetBitEnumerator GetSetBitIndexEnumerator(ulong* ulongs, int maxBits) => new(ulongs, 0, maxBits);

        /// <summary>Provides an enumerator to iterate through the indices of set bits in an UnsafeBitList.</summary>
        /// <param name="ulongs">A pointer to the bits stored in <see cref="ulong"/> ulongs.</param>
        /// <param name="maxBits">The length of the bit array.</param>
        /// <param name="startBit">The index to start searching from.</param>
        /// <param name="count">The number of bits to search.</param>
        /// <returns>An enumerator to iterate through the set bit indices.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SetBitEnumerator GetSetBitIndexEnumerator(ulong* ulongs, int maxBits, int startBit, int count)
        {
            CheckRange(maxBits, startBit, count);
            return new SetBitEnumerator(ulongs, startBit, count);
        }

        /// <summary>Enumerator for iterating through the set bit indices in an UnsafeBitList.</summary>
        public struct SetBitEnumerator : IEnumerator<int>, IEnumerable<int>
        {
            ulong* m_BitArray;
            int m_StartBit;
            int m_EndBit;
            int m_ULongIndex;
            ulong m_Mask;
            ulong m_UnscannedBitMask;
            int m_CurrentBit;
            int m_BaseBitIndex;

            /// <summary>Initializes a new instance of the SetEnumerator struct.</summary>
            /// <param name="bitArray">A pointer to the bits stored in <see cref="ulong"/> ulongs.</param>
            /// <param name="startBit">The index to start searching from.</param>
            /// <param name="bitCount">The number of bits to search.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public SetBitEnumerator(ulong* bitArray, int startBit, int bitCount)
            {
                m_BitArray = bitArray;
                m_StartBit = startBit;
                m_EndBit = startBit + bitCount;
                m_CurrentBit = -1; // Initialize to -1 to indicate an invalid state.
                m_ULongIndex = 0;
                m_BaseBitIndex = 0;
                m_UnscannedBitMask = 0;
                m_Mask = 0;
            }

            /// <summary>Disposes of the enumerator.</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose() { }

            /// <summary>Resets the enumerator to the beginning.</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset()
            {
                m_CurrentBit = -1; // Reset to -1 to indicate an invalid state.
                m_Mask = 0;
            }

            /// <summary>Moves to the next set bit in the UnsafeBitArray.</summary>
            /// <returns>True if a next set bit is found; otherwise, false.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                if (m_CurrentBit == -1)
                {
                    // If this is the first call, initialize the search.
                    m_ULongIndex = m_StartBit >> 6;
                    m_BaseBitIndex = m_StartBit & ~63;
                    m_UnscannedBitMask = ulong.MaxValue << (m_StartBit & 63);
                    FindFirstSetBit();
                }
                else
                {
                    // Mark the current bit as visited.
                    m_UnscannedBitMask &= ~m_Mask;
                    FindFirstSetBit();
                }

                return m_CurrentBit < m_EndBit;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void FindFirstSetBit()
            {
                // Find the first set bit that hasn't been visited yet.
                // Calculate the index of the last ulong (64-bit value) in the bit array.
                int lastULongIndex = (m_EndBit - 1) >> 6;

                // Advance to the next ulong if the current one has no remaining bits
                ulong remainingBitMask = m_BitArray[m_ULongIndex] & m_UnscannedBitMask;
                while (remainingBitMask == 0)
                {
                    ++m_ULongIndex;
                    m_BaseBitIndex += 64;

                    if (m_ULongIndex > lastULongIndex)
                    {
                        // We've advanced past the end of the bit array
                        m_CurrentBit = m_EndBit;
                        return;
                    }

                    remainingBitMask = m_BitArray[m_ULongIndex];
                    m_UnscannedBitMask = ulong.MaxValue;
                }

                // Unset the lowest set bit in the remainingBitMask to isolate it
                ulong newRemainingBitMask = remainingBitMask & (remainingBitMask - 1);

                // XOR the newRemainingBitMask with the original mask to find the lowest-set bit
                m_Mask = newRemainingBitMask ^ remainingBitMask;

                // Calculate the index of the lowest set bit and adjust it based on the base bit index
                m_CurrentBit = m_BaseBitIndex + math.tzcnt(m_Mask);

                // Check if we've accidentally iterated off the end of an array but are still within the same ulong
                if (m_CurrentBit >= m_EndBit)
                {
                    m_CurrentBit = m_EndBit;
                }
            }

            /// <summary>Gets the current set bit index.</summary>
            public int Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => m_CurrentBit;
            }

            /// <summary>Gets the current set bit index as an object.</summary>
            object IEnumerator.Current => m_CurrentBit;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public SetBitEnumerator GetEnumerator() => this;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            IEnumerator<int> IEnumerable<int>.GetEnumerator() => this;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void CheckRange(int maxBits, int startIndex, int count)
        {
            if (startIndex < 0 || startIndex >= maxBits || count < 1)
                throw new IndexOutOfRangeException($"BitUtility: invalid arguments: begin {startIndex} (must be 0-{maxBits - 1}), count {count} (must be greater than 0).");

            if (startIndex + count > maxBits)
                throw new ArgumentException($"BitUtility: invalid arguments: Out of bounds - begin {startIndex}, count {count}, Length {maxBits}.");
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void CheckArgsCopy(int dstMaxBits, int dstPos, int srcMaxBits, int srcPos, int numBits)
        {
            if (srcPos + numBits > srcMaxBits)
                throw new ArgumentException($"BitUtility: invalid arguments: Out of bounds - source position {srcPos}, numBits {numBits}, source bit array Length {srcMaxBits}.");

            if (dstPos + numBits > dstMaxBits)
                throw new ArgumentException($"BitUtility: invalid arguments: Out of bounds - destination position {dstPos}, numBits {numBits}, destination bit array Length {dstMaxBits}.");
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void CheckArgsUlong(int maxBits, int bit, int numBits)
        {
            CheckIndexCount(maxBits, bit, numBits);

            if (numBits is < 1 or > 64)
                throw new ArgumentException($"BitUtility: invalid arguments: numBits {numBits} (must be 1-64).");

            if (bit + numBits > maxBits)
                throw new ArgumentException($"BitUtility: invalid arguments: Out of bounds bit {bit}, numBits {numBits}, Length {maxBits}.");
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void CheckIndexCount(int maxBits, int bit, int numBits)
        {
            if (bit < 0 || bit >= maxBits || numBits < 1)
                throw new IndexOutOfRangeException($"BitUtility: invalid arguments: bit {bit} (must be 0-{maxBits - 1}), numBits {numBits} (must be greater than 0).");
        }
    }
}
