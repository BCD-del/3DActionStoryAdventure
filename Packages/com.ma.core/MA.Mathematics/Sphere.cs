// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

namespace MA.Mathematics
{
    /// <summary>Represents a sphere in 3D space.</summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Sphere : IEquatable<Sphere>
    {
        /// <summary>Center of the sphere</summary>
        public float3 Center;
        /// <summary>Radius of the sphere</summary>
        public float Radius;

        /// <summary>Diameter of sphere.</summary>
        public readonly float Diameter
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => 2f * Radius;
        }
        /// <summary>Circumference of sphere</summary>
        public readonly float Circumference
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => 2f * PI * Radius;
        }
        /// <summary>Area of sphere</summary>
        public readonly float Area
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => CalculateArea(Radius);
        }
        /// <summary>Volume of sphere</summary>
        public readonly float Volume
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => CalculateVolume(Radius);
        }

        /// <summary>Returns the bounds of the sphere.</summary>
        public readonly AxisAlignedBox Bounds
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => AxisAlignedBox.FromExtents(Center, Radius);
        }

        /// <summary>Constructs a new sphere.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Sphere(float3 center, float radius)
        {
            Center = center;
            Radius = radius;
        }

        /// <summary>Returns true if this sphere contains the given `point`.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(float3 point) => distancesq(Center, point) <= lengthsq(Radius);

        /// <summary>Returns true if this sphere contains the given `other` sphere.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(in Sphere other) => (distance(Center, other.Center) + other.Radius) <= Radius;

        /// <summary>Returns true if this sphere is inside the given `other` sphere.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool IsInside(in Sphere other) => other.Contains(this);

        /// <summary>Test whether this sphere intersects another sphere.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Intersects(in Sphere other, float tolerance = MathConstants.ZeroTolerance) 
            => lengthsq(Center - other.Center) <= lengthsq(max(0.0f, other.Radius + Radius + tolerance));

        /// <summary>Returns the minimum squared distance from `point` to the sphere's surface. for points outside sphere, 0 for points inside.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float DistanceSquared(in float3 point) => lengthsq(max(SignedDistance(point), 0f));

        /// <summary>Returns the signed distance from Point to Sphere surface.</summary>
        /// <remarks>Points inside sphere return negative distance.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float SignedDistance(in float3 point) => distance(Center, point) - Radius;

        /// <summary>Returns a sphere transformed by a matrix.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Sphere Translate(in float3 translation)
        {
            Sphere result;
            result.Center = Center + translation;
            result.Radius = Radius;
            return result;
        }

        /// <summary>Returns a sphere transformed by a matrix.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Sphere TransformBy(in float4x4 matrix)
        {
            Sphere result;
            result.Center = transform(matrix, Center);

            float3 xAxis = matrix.c0.xyz;
            float3 yAxis = matrix.c1.xyz;
            float3 zAxis = matrix.c2.xyz;
            result.Radius = sqrt(max(dot(xAxis, xAxis), max(dot(yAxis, yAxis), dot(zAxis, zAxis)))) * Radius;

            return result;
        }

        /// <summary>Returns a sphere transformed by a transform.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Sphere TransformBy(in LocalTransform transform)
        {
            Sphere result;
            result.Center = transform.TransformPoint(Center);
            result.Radius = transform.GetMaximumAxisScale() * Radius;
            return result;
        }

        /// <summary>Returns a sphere transformed by a transform.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Sphere InverseTransformBy(in LocalTransform transform)
        {
            Sphere result;
            result.Center = transform.InverseTransformPoint(Center);
            result.Radius = Radius / transform.GetMaximumAxisScale();
            return result;
        }
        
        /// <summary>Returns a float4 representation of the sphere.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float4 AsFloat4() => new float4(Center, Radius);

        /// <summary>Area of sphere with given Radius</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float CalculateArea(float radius) => 4.0f * PI * radius * radius;

        /// <summary>Volume of sphere with given Radius</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float CalculateVolume(float radius) => (4.0f / 3.0f) * PI * radius * radius * radius;

        /// <summary>Returns true if this sphere is equal to another sphere.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(Sphere other) => this == other;

        /// <summary>Returns true if this sphere is equal to another object.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override bool Equals(object obj) => obj is Sphere other && Equals(other);

        /// <summary>Returns the hash code for this sphere.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override int GetHashCode() => float4(Center, Radius).GetHashCode();

        /// <summary>Returns a string representation.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => $"Sphere({Center}, {Radius})";

        /// <summary>Returns the result of a component-wise equality operation on two spheres.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in Sphere lhs, in Sphere rhs) => all(lhs.Center == rhs.Center) && lhs.Radius == rhs.Radius;

        /// <summary>Returns the result of a component-wise not equal operation on two spheres.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in Sphere lhs, in Sphere rhs) => any(lhs.Center != rhs.Center) || lhs.Radius != rhs.Radius;
        
        /// <summary>Implicitly converts a sphere to a bounding sphere.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator BoundingSphere(Sphere sphere) => new BoundingSphere(sphere.Center, sphere.Radius);
        
        /// <summary>Implicitly converts a bounding sphere to a sphere.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Sphere(BoundingSphere sphere) => new Sphere(sphere.position, sphere.radius);
    }
}
