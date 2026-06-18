// Copyright © Magnetic Arcade. All Rights Reserved.

using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine.Animations;
using static Unity.Mathematics.math;
using float3x3 = Unity.Mathematics.float3x3;
using float4 = Unity.Mathematics.float4;
using float4x4 = Unity.Mathematics.float4x4;
using quaternion = Unity.Mathematics.quaternion;

namespace MA.Mathematics
{
    /// <summary>A set of utility extensions for Unity.Mathematics matrix types.</summary>
    public static class MatrixUtility
    {
        /// <summary>Computes the "forward" direction in a transformation matrix's reference frame.</summary>
        /// <remarks>This method assumes that <paramref name="m"/> is an affine transformation matrix without shear.</remarks>
        /// <param name="m">A transformation matrix.</param>
        /// <returns>
        /// A vector pointing in the "forward" direction of <paramref name="m"/>'s reference frame.
        /// By Unity's convention, this is positive Z axis. This vector does not necessarily have unit length.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 Forward(in this float4x4 m) => m.c2.xyz;

        /// <summary>Computes the "back" direction in a transformation matrix's reference frame.</summary>
        /// <remarks>This method assumes that <paramref name="m"/> is an affine transformation matrix without shear.</remarks>
        /// <param name="m">A transformation matrix.</param>
        /// <returns>
        /// A vector pointing in the "back" direction of <paramref name="m"/>'s reference frame.
        /// By Unity's convention, this is negative Z axis. This vector does not necessarily have unit length.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 Back(in this float4x4 m) => -Forward(m);

        /// <summary>Computes the "up" direction in a transformation matrix's reference frame.</summary>
        /// <remarks>
        /// This method assumes that <paramref name="m"/> is an affine transformation matrix without shear.
        /// </remarks>
        /// <param name="m">A transformation matrix.</param>
        /// <returns>
        /// A vector pointing in the "up" direction of <paramref name="m"/>'s reference frame.
        /// By Unity's convention, this is positive Y axis. This vector does not necessarily have unit length.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 Up(in this float4x4 m) => m.c1.xyz;

        /// <summary>Computes the "down" direction in a transformation matrix's reference frame.</summary>
        /// <remarks>This method assumes that <paramref name="m"/> is an affine transformation matrix without shear.</remarks>
        /// <param name="m">A transformation matrix.</param>
        /// <returns>
        /// A vector pointing in the "down" direction of <paramref name="m"/>'s reference frame.
        /// By Unity's convention, this is negative Y axis. This vector does not necessarily have unit length.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 Down(in this float4x4 m) => -Up(m);

        /// <summary>Computes the "right" direction in a transformation matrix's reference frame.</summary>
        /// <remarks>This method assumes that <paramref name="m"/> is an affine transformation matrix without shear.</remarks>
        /// <param name="m">A transformation matrix.</param>
        /// <returns>
        /// A vector pointing in the "right" direction of <paramref name="m"/>'s reference frame.
        /// By Unity's convention, this is positive X axis. This vector does not necessarily have unit length.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 Right(in this float4x4 m) => new float3(m.c0.x, m.c0.y, m.c0.z);

        /// <summary>Computes the "left" direction in a transformation matrix's reference frame.</summary>
        /// <remarks>This method assumes that <paramref name="m"/> is an affine transformation matrix without shear.</remarks>
        /// <param name="m">A transformation matrix.</param>
        /// <returns>
        /// A vector pointing in the "left" direction of <paramref name="m"/>'s reference frame.
        /// By Unity's convention, this is negative X axis. This vector does not necessarily have unit length.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 Left(in this float4x4 m) => -Right(m);

        /// <summary>Extracts the translation from a transformation matrix.</summary>
        /// <remarks>This method assumes that <paramref name="m"/> is an affine transformation matrix without shear.</remarks>
        /// <param name="m">A transformation matrix.</param>
        /// <returns>A vector containing the translation applied by the provided transformation matrix.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 Translation(in this float4x4 m) => new float3(m.c3.x, m.c3.y, m.c3.z);

        /// <summary>Adds a translation to the matrix.</summary>
        /// <remarks>This method assumes that <paramref name="m"/> is an affine transformation matrix without shear.</remarks>
        /// <param name="m">A transformation matrix.</param>
        /// <param name="translation">A 3D translation</param>
        /// <returns>A new transformation matrix with the translation applied.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float4x4 Translate(this in float4x4 m, float3 translation)
        {
            float4x4 result = m;
            result.c3.xyz += translation;
            return m;
        }

        /// <summary>Extracts the rotation from a transformation matrix.</summary>
        /// <remarks>This method assumes that <paramref name="m"/> is an affine transformation matrix without shear.</remarks>
        /// <param name="m">A transformation matrix.</param>
        /// <returns>A normalized quaternion containing the rotation applied by the provided transformation matrix.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion Rotation(in this float4x4 m) => new quaternion(orthonormalize(new float3x3(m)));

        /// <summary>Extracts the rotation from a transformation matrix.</summary>
        /// <remarks>This method assumes that <paramref name="m"/> is an affine transformation matrix without shear.</remarks>
        /// <param name="m">A transformation matrix.</param>
        /// <returns>A normalized quaternion containing the rotation applied by the provided transformation matrix.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion Rotation(this in float3x4 m) => new quaternion(orthonormalize(new float3x3(m.c0, m.c1, m.c2)));
        
        /// <summary>Extracts the rotation from a transformation matrix.</summary>
        /// <remarks>This method assumes that <paramref name="m"/> is an affine transformation matrix without shear.</remarks>
        /// <param name="m">A transformation matrix.</param>
        /// <returns>A normalized quaternion containing the rotation applied by the provided transformation matrix.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion Rotation(this in float3x3 m) => new quaternion(orthonormalize(m));

        /// <summary>Extracts the scale from a transformation matrix.</summary>
        /// <remarks>This method assumes that <paramref name="m"/> is an affine transformation matrix without shear.</remarks>
        /// <param name="m">A transformation matrix.</param>
        /// <returns>A vector containing the scale applied by the provided transformation matrix.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 Scale(in this float4x4 m) => new float3(length(m.c0.xyz), length(m.c1.xyz), length(m.c2.xyz));

        /// <summary>Extracts the scale from a transformation matrix.</summary>
        /// <remarks>This method assumes that <paramref name="m"/> is an affine transformation matrix without shear.</remarks>
        /// <param name="m">A transformation matrix.</param>
        /// <returns>A vector containing the scale applied by the provided transformation matrix.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 Scale(this in float3x4 m) => new float3(length(m.c0), length(m.c1), length(m.c2));
        
        /// <summary>Extracts the scale from a transformation matrix.</summary>
        /// <remarks>This method assumes that <paramref name="m"/> is an affine transformation matrix without shear.</remarks>
        /// <param name="m">A transformation matrix.</param>
        /// <returns>A vector containing the scale applied by the provided transformation matrix.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 Scale(this in float3x3 m) => new float3(length(m.c0), length(m.c1), length(m.c2));

        /// <summary>Returns true if the matrix has a non-uniform scale.</summary>
        /// <param name="m">A transformation matrix.</param>
        /// <returns>True if the matrix has a non-uniform scale, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsScaleNonUniform(this in float4x4 m)
        {
            float3 scale = m.Scale();
            return (!scale.x.NearlyEquals(scale.y) ||
                    !scale.x.NearlyEquals(scale.z) ||
                    !scale.y.NearlyEquals(scale.z));
        }

        // ------------------ Coordinate system conversion

        /// <summary>Transforms a 3D point by a 4x4 transformation matrix.</summary>
        /// <remarks>This method assumes that <paramref name="m"/> is an affine transformation matrix.</remarks>
        /// <param name="m">A transformation matrix.</param>
        /// <param name="p">A 3D position</param>
        /// <returns>A vector containing the transformed point.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 TransformPoint(in this float4x4 m, in float3 p) => mul(m, new float4(p, 1)).xyz;

        /// <summary>Transforms a 3D direction by a 4x4 transformation matrix.</summary>
        /// <remarks>This method assumes that <paramref name="m"/> is an affine transformation matrix.</remarks>
        /// <param name="m">A transformation matrix.</param>
        /// <param name="d">A vector representing a direction in 3D space. This vector does not need to be normalized.</param>
        /// <returns>A vector containing the transformed direction. This vector will not necessarily be unit-length.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 TransformDirection(in this float4x4 m, in float3 d) => rotate(m, d);

        /// <summary>Transforms a 3D rotation by a 4x4 transformation matrix.</summary>
        /// <remarks>This method assumes that <paramref name="m"/> is an affine transformation matrix without shear.</remarks>
        /// <param name="m">A transformation matrix.</param>
        /// <param name="q">A quaternion representing a 3D rotation. This quaternion does not need to be normalized.</param>
        /// <returns>A quaternion containing the transformed rotation. This quaternion will normalized if the input quaternion is normalized.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion TransformRotation(in this float4x4 m, in quaternion q) => mul(new quaternion(orthonormalize(new float3x3(m))), q);

        /// <summary>Transforms a 3D point by the inverse of a 4x4 transformation matrix.</summary>
        /// <remarks>This method assumes that <paramref name="m"/> is an affine transformation matrix.</remarks>
        /// <param name="m">A transformation matrix.</param>
        /// <param name="p">A 3D position</param>
        /// <returns>A vector containing the transformed point.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 InverseTransformPoint(in this float4x4 m, in float3 p) => mul(inverse(m), new float4(p, 1)).xyz;

        /// <summary>Transforms a 3D direction by the inverse of a 4x4 transformation matrix.</summary>
        /// <remarks> This method assumes that <paramref name="m"/> is an affine transformation matrix.</remarks>
        /// <param name="m">A transformation matrix.</param>
        /// <param name="d">A vector representing a direction in 3D space. This vector does not need to be normalized.</param>
        /// <returns> A vector containing the transformed direction. This vector will not necessarily be unit-length.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 InverseTransformDirection(in this float4x4 m, in float3 d) => rotate(inverse(m), d);

        /// <summary>Transforms a 3D rotation by the inverse of a 4x4 transformation matrix.</summary>
        /// <remarks>This method assumes that <paramref name="m"/> is an affine transformation matrix without shear.</remarks>
        /// <param name="m">A transformation matrix.</param>
        /// <param name="q">A quaternion representing a 3D rotation. This quaternion does not need to be normalized.</param>
        /// <returns>A quaternion containing the transformed rotation. This quaternion will be normalized if the input quaternion is normalized.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion InverseTransformRotation(in this float4x4 m, in quaternion q) => mul(new quaternion(orthonormalize(inverse(new float3x3(m)))), q);

        /// <summary>Returns an orthonormalized version of the matrix.</summary>
        /// <param name="m">A transformation matrix.</param>
        /// <returns>An orthonormalized version of the matrix.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float4x4 Orthonormalized(this in float4x4 m)
        {
            float3x3 orthonormalized = new float3x3(m);
            orthonormalized = orthonormalize(orthonormalized);
            
            float4x4 result = m;
            result.c0.xyz = orthonormalized.c0;
            result.c1.xyz = orthonormalized.c1;
            result.c2.xyz = orthonormalized.c2;
            
            return result;
        }

        /// <summary>Returns an orthonormalized version of the matrix.</summary>
        /// <param name="m">A rotation matrix.</param>
        /// <returns>An orthonormalized version of the matrix.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3x3 Orthonormalized(this in float3x3 m) => orthonormalize(m);
        
        /// <summary>Returns one of the scaled axes of the matrix.</summary>
        /// <param name="m">A transformation matrix.</param>
        /// <param name="axis">The axis to retrieve.</param>
        /// <returns>The scaled axis of the matrix.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 GetScaledAxis(this in float4x4 m, Axis axis)
        {
            return axis switch
            {
                Axis.X => m[0].xyz,
                Axis.Y => m[1].xyz,
                Axis.Z => m[2].xyz,
                _ => 0
            };
        }

        /// <summary>Returns one of the scaled axes of the matrix.</summary>
        /// <param name="m">A rotation matrix.</param>
        /// <param name="axis">The axis to retrieve.</param>
        /// <returns>The scaled axis of the matrix.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 GetScaledAxis(this in float3x3 m, Axis axis)
        {
            return axis switch
            {
                Axis.X => m[0],
                Axis.Y => m[1],
                Axis.Z => m[2],
                _ => 0
            };
        }

        /// <summary>Returns one of the normalized scaled axes of the matrix.</summary>
        /// <param name="m">A transformation matrix.</param>
        /// <param name="axis">The axis to retrieve.</param>
        /// <returns>The normalized scaled axis of the matrix.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 GetUnitAxis(this in float3x3 m, Axis axis) => normalizesafe(m.GetScaledAxis(axis));

        /// <summary>Returns one of the normalized scaled axes of the matrix.</summary>
        /// <param name="m">A rotation matrix.</param>
        /// <param name="axis">The axis to retrieve.</param>
        /// <returns>The normalized scaled axis of the matrix.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 GetUnitAxis(this in float4x4 m, Axis axis) => normalizesafe(m.GetScaledAxis(axis));
        
        /// <summary>Returns the scaled axes of the matrix.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetScaledAxes(this in float4x4 m, out float3 x, out float3 y, out float3 z)
        {
            x = m.c0.xyz;
            y = m.c1.xyz;
            z = m.c2.xyz;
        }

        /// <summary>Returns the scaled axes of the matrix.</summary>
        /// <param name="m">A rotation matrix.</param>
        /// <param name="x">The output X axis of the matrix.</param>
        /// <param name="y">The output Y axis of the matrix.</param>
        /// <param name="z">The output Z axis of the matrix.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetScaledAxes(this in float3x3 m, out float3 x, out float3 y, out float3 z)
        {
            x = m.c0;
            y = m.c1;
            z = m.c2;
        }
        
        /// <summary>Returns the normalized scaled axes of the matrix.</summary>
        /// <param name="m">A transformation matrix.</param>
        /// <param name="x">The output X axis of the matrix.</param>
        /// <param name="y">The output Y axis of the matrix.</param>
        /// <param name="z">The output Z axis of the matrix.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetUnitAxes(this in float4x4 m, out float3 x, out float3 y, out float3 z)
        {
            m.GetScaledAxes(out x, out y, out z);
            x = normalizesafe(x);
            y = normalizesafe(y);
            z = normalizesafe(z);
        }

        /// <summary>Returns the normalized scaled axes of the matrix.</summary>
        /// <param name="m">A rotation matrix.</param>
        /// <param name="x">The output X axis of the matrix.</param>
        /// <param name="y">The output Y axis of the matrix.</param>
        /// <param name="z">The output Z axis of the matrix.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetUnitAxes(this in float3x3 m, out float3 x, out float3 y, out float3 z)
        {
            m.GetScaledAxes(out x, out y, out z);
            x = normalizesafe(x);
            y = normalizesafe(y);
            z = normalizesafe(z);
        }

        /// <summary>Returns the upper left 3x3 of the transformation matrix.</summary>
        /// <param name="m">A transformation matrix.</param>
        /// <returns>The upper left 3x3 of the matrix.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3x3 UpperLeft(this in float4x4 m) => new float3x3(m);

        /// <summary>Sets each row of the matrix.</summary>
        /// <param name="m">A transformation matrix.</param>
        /// <param name="x">The X row.</param>
        /// <param name="y">The Y row.</param>
        /// <param name="z">The Z row.</param>
        /// <param name="w">The W row.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetRows(this ref float4x4 m, out float4 x, out float4 y, out float4 z, out float4 w)
        {
            x = float4(m[0][0], m[1][0], m[2][0], m[3][0]);
            y = float4(m[0][1], m[1][1], m[2][1], m[3][1]);
            z = float4(m[0][2], m[1][2], m[2][2], m[3][2]);
            w = float4(m[0][3], m[1][3], m[2][3], m[3][3]);
        }

        /// <summary>Sets the 3x3 rows of the matrix.</summary>
        /// <param name="m">A transformation matrix.</param>
        /// <param name="x">The X row.</param>
        /// <param name="y">The Y row.</param>
        /// <param name="z">The Z row.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetRows(this ref float4x4 m, in float3 x, in float3 y, in float3 z)
        {
            m.c0.xyz = float3(x[0], y[0], z[0]);
            m.c1.xyz = float3(x[1], y[1], z[1]);
            m.c2.xyz = float3(x[2], y[2], z[2]);
        }

        /// <summary>Sets the rows of the rotation matrix.</summary>
        /// <param name="m">A rotation matrix.</param>
        /// <param name="x">The X row.</param>
        /// <param name="y">The Y row.</param>
        /// <param name="z">The Z row.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetRows(this ref float3x3 m, in float3 x, in float3 y, in float3 z)
        {
            m.c0.xyz = float3(x[0], y[0], z[0]);
            m.c1.xyz = float3(x[1], y[1], z[1]);
            m.c2.xyz = float3(x[2], y[2], z[2]);
        }
    }
}
