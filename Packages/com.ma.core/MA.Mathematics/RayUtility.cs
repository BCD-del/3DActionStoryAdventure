// Copyright © Magnetic Arcade. All Rights Reserved.

using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace MA.Mathematics
{
    public static class RayUtility
    {
        /// <summary>Transform a ray by a matrix.</summary>
        /// <param name="ray">The ray to transform.</param>
        /// <param name="matrix">The transformation matrix.</param>
        /// <returns>A transformed ray.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Ray TransformBy(this Ray ray, float4x4 matrix)
        {
            return new Ray
            {
                origin = math.transform(matrix, ray.origin),
                direction = math.rotate(matrix, ray.direction)
            };
        }
        
        /// <summary>Transform a ray by a local transform.</summary>
        /// <param name="ray">The ray to transform.</param>
        /// <param name="transform">The transformation.</param>
        /// <returns>A transformed ray.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Ray TransformBy(this Ray ray, LocalTransform transform)
        {
            return new Ray
            {
                origin = transform.TransformPoint(ray.origin),
                direction = transform.TransformDirection(ray.direction)
            };
        }
        
        /// <summary>Inverse transform a ray by a matrix.</summary>
        /// <param name="ray">The ray to transform.</param>
        /// <param name="matrix">The transformation matrix.</param>
        /// <returns>An inverse transformed ray.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Ray InverseTransformBy(this Ray ray, float4x4 matrix)
        {
            float4x4 inverseMatrix = math.inverse(matrix);
            return new Ray
            {
                origin = math.transform(inverseMatrix, ray.origin),
                direction = math.rotate(inverseMatrix, ray.direction)
            };
        }
        
        /// <summary>Inverse transform a ray by a local transform.</summary>
        /// <param name="ray">The ray to transform.</param>
        /// <param name="transform">The transformation.</param>
        /// <returns>An inverse transformed ray.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Ray InverseTransformBy(this Ray ray, LocalTransform transform)
        {
            return new Ray
            {
                origin = transform.InverseTransformPoint(ray.origin),
                direction = transform.InverseTransformDirection(ray.direction)
            };
        }
        
        /// <summary>Get the end point of a ray at a given distance.</summary>
        /// <param name="ray">The ray to get the end point of.</param>
        /// <param name="distance">The distance from the origin of the ray.</param>
        /// <returns>The end point of the ray at the given distance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 GetEndPoint(this Ray ray, float distance) => ray.origin + ray.direction * distance;

        /// <summary>Get the end point of a raycast hit.</summary>
        /// <param name="hit">The raycast hit to get the end point of.</param>
        /// <returns>The end point of the raycast hit.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 GetEndPoint(this RaycastHit hit) => hit.point + hit.normal * hit.distance;

        /// <summary>Get the end point of a raycast hit.</summary>
        /// <param name="raycastCommand">The raycast command to get the end point of.</param>
        /// <returns>The end point of the raycast hit.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 GetEndPoint(this RaycastCommand raycastCommand) => raycastCommand.from + raycastCommand.direction * raycastCommand.distance;
    }
}