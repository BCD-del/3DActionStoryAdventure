// Copyright © Magnetic Arcade. All Rights Reserved.

using System.Runtime.CompilerServices;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace MA.Mathematics
{
    /// <summary>Common constants used in math operations.</summary>
    public static class MathConstants
    {
        /// <summary>Zero tolerance for math operations - float 1e-6</summary>
        public const float ZeroTolerance = 1e-6f;
        /// <summary>3.14159...</summary>
        public const float PI = math.PI;
        /// <summary>0.5 * PI</summary>
        public const float HalfPI = 0.5f * math.PI;
        /// <summary>2.0 * PI</summary>
        public const float TwoPI = 2.0f * math.PI;
        /// <summary>4.0 * PI</summary>
        public const float FourPI = 4.0f * math.PI;
        /// <summary>1.0 / PI</summary>
        public const float InvPI = 1.0f / math.PI;
        /// <summary>1.0 / (2*PI)</summary>
        public const float InvTwoPI = 1.0f / TwoPI;
        /// <summary>sqrt(2)</summary>
        public const float Sqrt2 = 1.4142135623730950488016887242097f;
        /// <summary>1.0 / sqrt(2)</summary>
        public const float InvSqrt2 = 1.0f / Sqrt2;
        /// <summary>sqrt(3)</summary>
        public const float Sqrt3 = 1.7320508075688772935274463415059f;
        /// <summary>1.0 / sqrt(3)</summary>
        public const float InvSqrt3 = 1.0f / Sqrt3;
    }
    
    /// <summary>Generic math utility functions.</summary>
    public static class MathUtility
    {
        /// <summary>Fast method to detect if a float is negative.</summary>
        /// <param name="a">The float to check</param>
        /// <returns>True if the float is negative, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNegative(float a)
            => asuint(a) >= 0x80000000;
        
        /// <summary>Returns true if a and b are nearly equal.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool2 Nearly(float2 a, float2 b, float tolerance = MathConstants.ZeroTolerance) 
            => abs(b - a) <= tolerance;
        
        /// <summary>Returns true if a and b are nearly equal.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool3 Nearly(float3 a, float3 b, float tolerance = MathConstants.ZeroTolerance) 
            => abs(b - a) <= tolerance;
        
        /// <summary>Returns true if a and b are nearly equal.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool4 Nearly(float4 a, float4 b, float tolerance = MathConstants.ZeroTolerance) 
            => abs(b - a) <= tolerance;

        /// <summary>Returns true if a and b are nearly equal.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NearlyEquals(float a, float b, float tolerance = MathConstants.ZeroTolerance)
            => abs(b - a) <= tolerance;
        
        /// <summary>Returns true if a and b are nearly equal.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NearlyEquals(float2 a, float2 b, float tolerance = MathConstants.ZeroTolerance) 
            => all(abs(b - a) <= tolerance);
        
        /// <summary>Returns true if a and b are nearly equal.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NearlyEquals(float3 a, float3 b, float tolerance = MathConstants.ZeroTolerance) 
            => all(abs(b - a) <= tolerance);
        
        /// <summary>Returns true if a and b are nearly equal.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NearlyEquals(float4 a, float4 b, float tolerance = MathConstants.ZeroTolerance) 
            => all(abs(b - a) <= tolerance);
        
        /// <summary>Loops the value t, so that it is never larger than length and never smaller than 0.</summary>
        public static float Repeat(float t, float length) 
            => clamp(t - floor(t / length) * length, 0.0f, length);

        /// <summary>Snaps a value to the nearest grid multiple.</summary>
        /// <param name="value">The value to snap.</param>
        /// <param name="grid">The grid size.</param>
        /// <returns>The snapped value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GridSnap(float value, float grid) 
            => (grid == 0) ? value : (floor((value + (grid / 2f)) / grid) * grid);

        /// <summary>Divides two integers and rounds up.</summary>
        /// <param name="dividend">The number to divide.</param>
        /// <param name="divisor">The number to divide by.</param>
        /// <returns>The result of the division, rounded up.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int DivideAndRoundUp(int dividend, int divisor)
            => (dividend + divisor - 1) / divisor;
        
        /// <summary>Divides two integers and rounds down.</summary>
        /// <param name="dividend">The number to divide.</param>
        /// <param name="divisor">The number to divide by.</param>
        /// <returns>The result of the division, rounded down.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int DivideAndRoundDown(int dividend, int divisor) 
            => dividend / divisor;

        /// <summary>Divides two integers and rounds to nearest.</summary>
        /// <param name="dividend">The number to divide.</param>
        /// <param name="divisor">The number to divide by.</param>
        /// <returns>The result of the division, rounded to nearest.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int DivideAndRoundNearest(int dividend, int divisor) 
            => (dividend >= 0)
                ? (dividend + divisor / 2) / divisor
                : (dividend - divisor / 2 + 1) / divisor;

        /// <summary>Computes the ceiling of the base-2 logarithm of x.</summary>
        /// <param name="x">The value to compute the ceiling of the base-2 logarithm of.</param>
        /// <returns>The ceiling of the base-2 logarithm of x.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CeilLogTwo(ulong x)
            => 32 - lzcnt(x - 1);

        /// <summary>Calculates the next power of two greater than or equal to the input value.</summary>
        /// <param name="input">The input value.</param>
        /// <param name="alignPow2">The alignment value.</param>
        /// <returns>The next multiple of `alignment` greater than or equal to `input`.</returns>
        /// <remarks>Alignment must be a power of two.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int NextMultipleOf(int input, int alignPow2) 
            => (input + (alignPow2 - 1)) & (~(alignPow2 - 1));

        /// <summary>Returns the next multiple of `alignment` greater than or equal to `input`. (Alignment must be a power of two.)</summary>
        /// <param name="input">The input value.</param>
        /// <param name="alignPow2">The alignment value.</param>
        /// <returns>The next multiple of `alignment` greater than or equal to `input`.</returns>
        /// <remarks>Alignment must be a power of two.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong NextMultipleOf(ulong input, ulong alignPow2)
            => (input + (alignPow2 - 1)) & (~(alignPow2 - 1));

        /// <summary>Returns the next multiple of `alignment` greater than or equal to `input`.</summary>
        /// <param name="input">The input value.</param>
        /// <param name="alignment">The alignment value.</param>
        /// <returns>The next multiple of `alignment` greater than or equal to `input`.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int NextMultipleOfNonPow2(int input, int alignment) 
            => (input % alignment) == 0 ? input : ((input + alignment) - (input % alignment));
        
        /// <summary>Returns the next multiple of `alignment` greater than or equal to `input`.</summary>
        /// <param name="input">The input value.</param>
        /// <param name="alignment">The alignment value.</param>
        /// <returns>The next multiple of `alignment` greater than or equal to `input`.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong NextMultipleOfNonPow2(ulong input, ulong alignment) 
            => (input % alignment) == 0 ? input : ((input + alignment) - (input % alignment));
    }
}
