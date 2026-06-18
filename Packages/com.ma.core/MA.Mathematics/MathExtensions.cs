// Copyright © Magnetic Arcade. All Rights Reserved.

using System.Runtime.CompilerServices;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace MA.Mathematics
{
    /// <summary>A set of utility extensions for the Unity.Mathematics library.</summary>
    public static class MathExtensions
    {
        /// <summary>Converts a `bool` to an `int`.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int AsInt(this bool b) => b ? 1 : 0;
        
        /// <summary>Converts a `bool` to an `uint`.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint AsUnt(this bool b) => b ? 1U : 0U;
        
        /// <summary>Converts a `bool` to an `long`.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long AsLong(this bool b) => b ? 1L : 0L;
        
        /// <summary>Converts a `bool` to an `ulong`.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong AsUlong(this bool b) => b ? 1UL : 0UL;
        
        /// <summary>Verbose addition of two vectors.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 Translate(this float3 v, float3 translation) => v + translation;

        /// <summary>
        /// The purpose of this function is to find two vectors that are orthogonal (perpendicular) to each other and to the
        /// input vector `v`, such that one of these vectors aligns with the component of `v` that has the largest absolute value.
        /// The resulting vectors axis1 and axis2 can then be used as a basis for a coordinate system centered on `v`.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CalculatePerpendicularAxes(this float3 v, out float3 axis1, out float3 axis2)
        {
            float nx = abs(v.x);
            float ny = abs(v.y);
            float nz = abs(v.z);

            // Find best basis vectors.
            if (nz > nx && nz > ny)	axis1 = float3(1, 0, 0);
            else					axis1 = float3(0, 0, 1);

            float3 tmp = axis1 - v * dot(axis1, v);
            axis1 = normalizesafe(tmp);
            axis2 = cross(axis1, v);
        }

        /// <summary>Checks if a vector is normalized within a certain tolerance.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNormalized(this in float2 v, float tolerance = MathConstants.ZeroTolerance)
            => abs((v.x * v.x + v.y * v.y) - 1f) <= tolerance;

        /// <summary>Checks if a vector is normalized within a certain tolerance.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNormalized(this float3 v, float tolerance = MathConstants.ZeroTolerance) 
            => abs((v.x * v.x + v.y * v.y + v.z * v.z) - 1f) <= tolerance;

        /// <summary>Checks if a vector equals another vector within a certain tolerance.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NearlyEquals(this float f, float other, float tolerance = MathConstants.ZeroTolerance) 
            => MathUtility.NearlyEquals(f, other, tolerance);

        /// <summary>Checks if a vector equals another vector within a certain tolerance.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NearlyEquals(this in float2 v, float2 other, float tolerance = MathConstants.ZeroTolerance) 
            => MathUtility.NearlyEquals(v, other, tolerance);

        /// <summary>Checks if a vector equals another vector within a certain tolerance.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NearlyEquals(this float3 v, float3 other, float tolerance = MathConstants.ZeroTolerance)
            => MathUtility.NearlyEquals(v, other, tolerance);

        /// <summary>Checks if a vector equals another vector within a certain tolerance.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NearlyEquals(this in float4 v, float4 other, float tolerance = MathConstants.ZeroTolerance) 
            => MathUtility.NearlyEquals(v, other, tolerance);

        /// <summary>Checks if a matrix equals another vector within a certain tolerance.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NearlyEquals(this in float2x2 m, float2x2 other, float tolerance = MathConstants.ZeroTolerance)
            => m.c0.NearlyEquals(other.c0, tolerance) &&
               m.c1.NearlyEquals(other.c1, tolerance);

        /// <summary>Checks if a matrix equals another vector within a certain tolerance.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NearlyEquals(this in float2x3 m, float2x3 other, float tolerance = MathConstants.ZeroTolerance) 
            => m.c0.NearlyEquals(other.c0, tolerance) &&
               m.c1.NearlyEquals(other.c1, tolerance) &&
               m.c2.NearlyEquals(other.c2, tolerance);

        /// <summary>Checks if a matrix equals another vector within a certain tolerance.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NearlyEquals(this in float2x4 m, float2x4 other, float tolerance = MathConstants.ZeroTolerance)
            => m.c0.NearlyEquals(other.c0, tolerance) &&
               m.c1.NearlyEquals(other.c1, tolerance) &&
               m.c2.NearlyEquals(other.c2, tolerance) &&
               m.c3.NearlyEquals(other.c3, tolerance);

        /// <summary>Checks if a matrix equals another vector within a certain tolerance.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NearlyEquals(this in float3x2 m, float3x2 other, float tolerance = MathConstants.ZeroTolerance)
            => m.c0.NearlyEquals(other.c0, tolerance) &&
               m.c1.NearlyEquals(other.c1, tolerance);

        /// <summary>Checks if a matrix equals another vector within a certain tolerance.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NearlyEquals(this in float3x3 m, float3x3 other, float tolerance = MathConstants.ZeroTolerance) 
            => m.c0.NearlyEquals(other.c0, tolerance) &&
               m.c1.NearlyEquals(other.c1, tolerance) &&
               m.c2.NearlyEquals(other.c2, tolerance);

        /// <summary>Checks if a matrix equals another vector within a certain tolerance.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NearlyEquals(this in float3x4 m, float3x4 other, float tolerance = MathConstants.ZeroTolerance)
            => m.c0.NearlyEquals(other.c0, tolerance) &&
               m.c1.NearlyEquals(other.c1, tolerance) &&
               m.c2.NearlyEquals(other.c2, tolerance);

        /// <summary>Checks if a matrix equals another vector within a certain tolerance.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NearlyEquals(this in float4x2 m, float4x2 other, float tolerance = MathConstants.ZeroTolerance) 
            => m.c0.NearlyEquals(other.c0, tolerance) &&
               m.c1.NearlyEquals(other.c1, tolerance);

        /// <summary>Checks if a matrix equals another vector within a certain tolerance.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NearlyEquals(this in float4x3 m, float4x3 other, float tolerance = MathConstants.ZeroTolerance) 
            => m.c0.NearlyEquals(other.c0, tolerance) &&
               m.c1.NearlyEquals(other.c1, tolerance) &&
               m.c2.NearlyEquals(other.c2, tolerance);

        /// <summary>Checks if a matrix equals another vector within a certain tolerance.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NearlyEquals(this in float4x4 m, float4x4 other, float tolerance = MathConstants.ZeroTolerance)
            => m.c0.NearlyEquals(other.c0, tolerance) &&
               m.c1.NearlyEquals(other.c1, tolerance) &&
               m.c2.NearlyEquals(other.c2, tolerance) &&
               m.c3.NearlyEquals(other.c3, tolerance);

        /// <summary>Checks if a vector is nearly zero within a certain tolerance.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NearlyZero(this in float2 v, float tolerance = MathConstants.ZeroTolerance) 
            => all(abs(v) <= tolerance);

        /// <summary>Checks if a vector is nearly zero within a certain tolerance.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NearlyZero(this float3 v, float tolerance = MathConstants.ZeroTolerance) 
            => all(abs(v) <= tolerance);

        /// <summary>Checks if a vector is nearly zero within a certain tolerance.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NearlyZero(this in float4 v, float tolerance = MathConstants.ZeroTolerance) 
            => all(abs(v) <= tolerance);

        /// <summary>Checks if a matrix is nearly zero within a certain tolerance.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NearlyZero(this in float2x2 v, float tolerance = MathConstants.ZeroTolerance) 
            => all(abs(v.c0) <= tolerance) &&
               all(abs(v.c1) <= tolerance);

        /// <summary>Checks if a matrix is nearly zero within a certain tolerance.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NearlyZero(this in float2x3 v, float tolerance = MathConstants.ZeroTolerance) 
            => all(abs(v.c0) <= tolerance) &&
               all(abs(v.c1) <= tolerance) &&
               all(abs(v.c2) <= tolerance);

        /// <summary>Checks if a matrix is nearly zero within a certain tolerance.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NearlyZero(this in float2x4 v, float tolerance = MathConstants.ZeroTolerance) 
            => all(abs(v.c0) <= tolerance) &&
               all(abs(v.c1) <= tolerance) &&
               all(abs(v.c2) <= tolerance) &&
               all(abs(v.c3) <= tolerance);

        /// <summary>Checks if a matrix is nearly zero within a certain tolerance.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NearlyZero(this in float3x3 v, float tolerance = MathConstants.ZeroTolerance)
            => all(abs(v.c0) <= tolerance) &&
               all(abs(v.c1) <= tolerance) &&
               all(abs(v.c2) <= tolerance);

        /// <summary>Checks if a matrix is nearly zero within a certain tolerance.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NearlyZero(this in float3x4 v, float tolerance = MathConstants.ZeroTolerance)
            => all(abs(v.c0) <= tolerance) &&
               all(abs(v.c1) <= tolerance) &&
               all(abs(v.c2) <= tolerance) &&
               all(abs(v.c3) <= tolerance);

        /// <summary>Checks if a matrix is nearly zero within a certain tolerance.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NearlyZero(this in float4x2 v, float tolerance = MathConstants.ZeroTolerance)
            => all(abs(v.c0) <= tolerance) &&
               all(abs(v.c1) <= tolerance);

        /// <summary>Checks if a matrix is nearly zero within a certain tolerance.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NearlyZero(this in float4x3 v, float tolerance = MathConstants.ZeroTolerance)
            => all(abs(v.c0) <= tolerance) &&
               all(abs(v.c1) <= tolerance) &&
               all(abs(v.c2) <= tolerance);

        /// <summary>Checks if a matrix is nearly zero within a certain tolerance.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NearlyZero(this in float4x4 v, float tolerance = MathConstants.ZeroTolerance) 
            => all(abs(v.c0) <= tolerance) &&
               all(abs(v.c1) <= tolerance) &&
               all(abs(v.c2) <= tolerance) &&
               all(abs(v.c3) <= tolerance);

        /// <summary>Utility function for finding the index of a value in an int vector.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf(this in int2 v, int value) 
            => v[0] == value ? 0 :
               v[1] == value ? 1 : -1;

        /// <summary>Utility function for finding the index of a value in an int vector.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf(this in int3 v, int value) 
            => v[0] == value ? 0 :
               v[1] == value ? 1 :
               v[2] == value ? 2 : -1;

        /// <summary>Utility function for finding the index of a value in an int vector.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf(this in int4 v, int value) 
            => v[0] == value ? 0 :
               v[1] == value ? 1 :
               v[2] == value ? 2 :
               v[3] == value ? 3 : -1;

        /// <summary>Utility function for checking if an int vector contains a value.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains(this in int2 v, int value) 
            => v.IndexOf(value) != -1;

        /// <summary>Utility function for checking if an int vector contains a value.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains(this in int3 v, int value) 
            => v.IndexOf(value) != -1;

        /// <summary>Utility function for checking if an int vector contains a value.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains(this in int4 v, int value) 
            => v.IndexOf(value) != -1;
    }
}
