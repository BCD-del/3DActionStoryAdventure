// Copyright © Magnetic Arcade. All Rights Reserved.

using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace MA.Core
{
    /// <summary>Utility functions for Morton codes.</summary>
    /// <remarks>https://fgiesen.wordpress.com/2009/12/13/decoding-morton-codes/</remarks>
    public static class MortonUtility
    {
        /// <summary>Encode a 2D point into a 2D Morton code.</summary>
        /// <remarks>Assumes the point coordinates are less than 2^16, and not negative.</remarks>
        /// <param name="x">The x coordinate of the point.</param>
        /// <param name="y">The y coordinate of the point.</param>
        /// <returns>The 2D Morton code of the point.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int EncodeMorton2(int x, int y) 
            => (Part1By1(y) << 1) + Part1By1(x);

        /// <summary>Encode a 3D point into a 3D Morton code.</summary>
        /// <remarks>Assumes the point coordinates are less than 2^10, and not negative.</remarks>
        /// <param name="x">The x coordinate of the point.</param>
        /// <param name="y">The y coordinate of the point.</param>
        /// <param name="z">The z coordinate of the point.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int EncodeMorton3(int x, int y, int z)
            => (Part1By2(z) << 2) + (Part1By2(y) << 1) + Part1By2(x);

        /// <summary>Decode a 2D X coordinate from a 2D Morton code.</summary>
        /// <param name="code">The 2D Morton code to decode.</param>
        /// <returns>The x coordinate of the point.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int DecodeMorton2X(int code) 
            => Compact1By1(code >> 0);

        /// <summary>Decode a 2D Y coordinate from a 2D Morton code.</summary>
        /// <param name="code">The 2D Morton code to decode.</param>
        /// <returns>The y coordinate of the point.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int DecodeMorton2Y(int code) 
            => Compact1By1(code >> 1);

        /// <summary>Decode a 3D X coordinate from a 3D Morton code.</summary>
        /// <param name="code">The 3D Morton code to decode.</param>
        /// <returns>The x coordinate of the point.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int DecodeMorton3X(int code) 
            => Compact1By2(code >> 0);

        /// <summary>Decode a 3D Y coordinate from a 3D Morton code.</summary>
        /// <param name="code">The 3D Morton code to decode.</param>
        /// <returns>The y coordinate of the point.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int DecodeMorton3Y(int code)
            => Compact1By2(code >> 1);

        /// <summary>Decode a 3D Z coordinate from a 3D Morton code.</summary>
        /// <param name="code">The 3D Morton code to decode.</param>
        /// <returns>The z coordinate of the point.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int DecodeMorton3Z(int code)
            => Compact1By2(code >> 2);

        /// <summary>Insert a 0 bit after each of the 16 low bits of x.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Part1By1(int x)
        {
            x &= 0x0000ffff;                  // x = ---- ---- ---- ---- fedc ba98 7654 3210
            x = (x ^ (x <<  8)) & 0x00ff00ff; // x = ---- ---- fedc ba98 ---- ---- 7654 3210
            x = (x ^ (x <<  4)) & 0x0f0f0f0f; // x = ---- fedc ---- ba98 ---- 7654 ---- 3210
            x = (x ^ (x <<  2)) & 0x33333333; // x = --fe --dc --ba --98 --76 --54 --32 --10
            x = (x ^ (x <<  1)) & 0x55555555; // x = -f-e -d-c -b-a -9-8 -7-6 -5-4 -3-2 -1-0
            return x;
        }
        
        /// <summary>Insert two 0 bits after each of the 10 low bits of x.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Part1By2(uint x)
        {
            x &= 0x000003ff;                  // x = ---- ---- ---- ---- ---- --98 7654 3210
            x = (x ^ (x << 16)) & 0xff0000ff; // x = ---- --98 ---- ---- ---- ---- 7654 3210
            x = (x ^ (x <<  8)) & 0x0300f00f; // x = ---- --98 ---- ---- 7654 ---- ---- 3210
            x = (x ^ (x <<  4)) & 0x030c30c3; // x = ---- --98 ---- 76-- --54 ---- 32-- --10
            x = (x ^ (x <<  2)) & 0x09249249; // x = ---- 9--8 --7- -6-- 5--4 --3- -2-- 1--0
            return x;
        }

        /// <summary>Insert two 0 bits after each of the 10 low bits of x.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Part1By2(int x) => (int)Part1By2(math.asuint(x));
        
        /// <summary>Inverse of Part1By1 - "delete" all odd-indexed bits.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Compact1By1(int x)
        {
            x &= 0x55555555;                  // x = -f-e -d-c -b-a -9-8 -7-6 -5-4 -3-2 -1-0
            x = (x ^ (x >>  1)) & 0x33333333; // x = --fe --dc --ba --98 --76 --54 --32 --10
            x = (x ^ (x >>  2)) & 0x0f0f0f0f; // x = ---- fedc ---- ba98 ---- 7654 ---- 3210
            x = (x ^ (x >>  4)) & 0x00ff00ff; // x = ---- ---- fedc ba98 ---- ---- 7654 3210
            x = (x ^ (x >>  8)) & 0x0000ffff; // x = ---- ---- ---- ---- fedc ba98 7654 3210
            return x;
        }
        
        /// <summary>Inverse of Part1By2 - "delete" all bits not at positions divisible by 3.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Compact1By2(uint x)
        {
            x &= 0x09249249;                  // x = ---- 9--8 --7- -6-- 5--4 --3- -2-- 1--0
            x = (x ^ (x >>  2)) & 0x030c30c3; // x = ---- --98 ---- 76-- --54 ---- 32-- --10
            x = (x ^ (x >>  4)) & 0x0300f00f; // x = ---- --98 ---- ---- 7654 ---- ---- 3210
            x = (x ^ (x >>  8)) & 0xff0000ff; // x = ---- --98 ---- ---- ---- ---- 7654 3210
            x = (x ^ (x >> 16)) & 0x000003ff; // x = ---- ---- ---- ---- ---- --98 7654 3210
            return x;
        }

        /// <summary>Inverse of Part1By2 - "delete" all bits not at positions divisible by 3.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Compact1By2(int x) => (int)Compact1By2(math.asuint(x));
    }
}