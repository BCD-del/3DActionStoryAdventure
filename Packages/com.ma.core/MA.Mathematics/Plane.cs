// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;
using float3 = Unity.Mathematics.float3;

namespace MA.Mathematics
{
    /// <summary>Representation of a plane in 3D space.</summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Plane : IEquatable<Plane>
    {
        /// <summary>The normal vector of the plane.</summary>
        public float3 Normal;
        /// <summary>The distance of the Plane along its normal from the origin.</summary>
        public float Distance;
        
        /// <summary>Get the origin of this plane.</summary>
        public readonly float3 Origin
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Normal * Distance;
        }

        /// <summary>Checks if this plane is valid (ie: if it has a non-zero normal).</summary>
        public readonly bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => !Normal.NearlyZero();
        }

        /// <summary>Returns a plane facing in the opposite direction.</summary>
        public readonly Plane Flipped
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(-Normal, -Distance);
        }

        /// <summary>Constructs a new plane from a normal and a point.</summary>
        /// <param name="normal">The Plane's normal vector.</param>
        /// <param name="point">A point on the Plane.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Plane(float3 normal, float3 point)
        {
            Normal = normalizesafe(normal);
            Distance = -dot(Normal, point);
        }
        
        /// <summary>Constructs a new plane.</summary>
        /// <param name="normal">The Plane's normal vector.</param>
        /// <param name="distance">The Plane's distance from the origin along its normal vector.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Plane(float3 normal, float distance)
        {
            Normal = normalizesafe(normal);
            Distance = distance;
        }
        
        /// <summary>Constructs a new plane.</summary>
        /// <param name="x">The X-component of the normal.</param>
        /// <param name="y">The Y-component of the normal.</param>
        /// <param name="z">The Z-component of the normal.</param>
        /// <param name="d">The distance of the Plane along its normal from the origin.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Plane(float x, float y, float z, float d)
        {
            Normal = float3(x, y, z);
            Distance = d;
        }
        
        /// <summary>Constructs a new Plane that contains the three given points.</summary>
        /// <param name="a">The first point defining the Plane.</param>
        /// <param name="b">The second point defining the Plane.</param>
        /// <param name="c">The third point defining the Plane.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Plane(float3 a, float3 b, float3 c)
        {
            Normal = normalizesafe(cross(b - a, c - a));
            Distance = -dot(Normal, a);
        }

        /// <summary>Sets a plane using a point that lies within it along with a normal to orient it.</summary>
        /// <param name="normal">The plane's normal vector.</param>
        /// <param name="point">A point that lies on the plane.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetNormalAndPosition(float3 normal, float3 point)
        {
            Normal = normalizesafe(normal);
            Distance = -dot(Normal, point);
        }

        /// <summary>Returns the normalized plane.</summary>
        /// <returns>The normalized plane.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Plane Normalized()
        {
            float normalLength = length(Normal);
            return normalLength.NearlyEquals(1.0f) ? this : new Plane(Normal / normalLength, Distance / normalLength);
        }

        /// <summary>Returns the dot product of a specified value and the normal vector of this plane plus the distance value of the plane.</summary>
        /// <param name="value">The input vector.</param>
        /// <returns>The resulting value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float DotCoordinate(float3 value) => dot(Normal, value) + Distance;

        /// <summary>Returns the dot product of a specified value and the normal vector of this plane.</summary>
        /// <param name="value">The input vector.</param>
        /// <returns>The resulting dot product.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float DotNormal(float3 value) => dot(Normal, value);
        
        /// <summary>Compute d = Dot(N,P)-c where N is the plane normal and c is the plane constant. This is a signed distance.
        /// The sign of the return value is positive if the point is on the positive side of the plane, negative if
        /// the point is on the negative side, and zero if the point is on the plane.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float DistanceToPoint(float3 p) => dot(Normal, p) + Distance;

        /// <summary>Returns a copy of the given plane that is moved in space by the given translation.</summary>
        /// <param name="translation">The offset in space to move the plane with.</param>
        /// <returns>The translated plane.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Plane Translate(float3 translation) => new Plane(Normal, Distance + dot(Normal, translation));
        
        /// <summary>Get the result of transforming the plane by a matrix.</summary>
        /// <param name="m">The transformation matrix.</param>
        /// <returns>The transformed plane.</returns>
        /// <remarks>This plane must already be normalized, so that its <see cref="Normal"/> vector is of unit length, before this method is called.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Plane TransformBy(in float4x4 m)
        {
            float4x4 im = inverse(m);
            float x = Normal.x, y = Normal.y, z = Normal.z, w = Distance;
            
            return new Plane( 
                x * im.c0[0] + y * im.c0[1] + z * im.c0[2] + w * im.c0[3],
                x * im.c1[0] + y * im.c1[1] + z * im.c1[2] + w * im.c1[3],
                x * im.c2[0] + y * im.c2[1] + z * im.c2[2] + w * im.c2[3],
                x * im.c3[0] + y * im.c3[1] + z * im.c3[2] + w * im.c3[3]);
        }
        
        /// <summary>Transforms a normalized plane by a quaternion rotation.</summary>
        /// <param name="rotation">The quaternion rotation to apply to the plane.</param>
        /// <returns>A new plane that results from applying the rotation.</returns>
        /// <remarks>This plane must already be normalized, so that its <see cref="Normal"/> vector is of unit length, before this method is called.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Plane Rotate(in quaternion rotation)
        {
            float3x3 m = float3x3(rotation);
            float x = Normal.x, y = Normal.y, z = Normal.z;

            return new Plane(
                x * m.c0[0] + y * m.c1[0] + z * m.c2[0],
                x * m.c0[1] + y * m.c1[1] + z * m.c2[1],
                x * m.c0[2] + y * m.c1[2] + z * m.c2[2],
                Distance);
        }

        /// <summary>Is a point on the positive side of the plane?</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool SameSide(float3 p0, float3 p1)
        {
            float d0 = DistanceToPoint(p0);
            float d1 = DistanceToPoint(p1);
            return (d0 > 0.0 && d1 > 0.0) || (d0 <= 0.0 && d1 <= 0.0);
        }

        /// <summary>For a given point returns the closest point on the plane.</summary>
        /// <param name="point">The point to project onto the plane.</param>
        /// <returns>A point on the plane that is closest to point.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float3 GetClosestPoint(float3 point)
        {
            float d = dot(Normal, point) + Distance;
            return point - Normal * d;
        }

        /// <summary>Returns a value indicating the point's position relative to the plane.</summary>
        /// <param name="point">The point to check.</param>
        /// <returns>-1 if the point is on the negative side of the plane, 1 if it is on the positive side, and 0 if it is on the plane.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int WhichSide(float3 point)
        {
            float distance = DistanceToPoint(point);
            return distance switch
            {
                < 0 => -1,
                > 0 => +1,
                _ => 0
            };
        }
        
        /// <summary>Intersects a ray with the plane.</summary>
        /// <param name="origin">The origin of the ray.</param>
        /// <param name="direction">The direction of the ray.</param>
        /// <param name="enter">The distance along the ray at which the intersection occurs.</param>
        /// <returns>True if the ray intersects the plane, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Raycast(float3 origin, float3 direction, out float enter)
        {
            float nDot = -dot(origin, Normal) - Distance;
            if (abs(nDot) < MathConstants.ZeroTolerance)
            {
                enter = 0.0f;
                return false;
            }

            float vDot = dot(direction, Normal);
            enter = nDot / vDot;
            return enter > 0.0f;
        }
        
        /// <summary>Intersects a ray with the plane.</summary>
        /// <param name="ray">The ray to intersect with the plane.</param>
        /// <param name="enter">The distance along the ray at which the intersection occurs.</param>
        /// <returns>True if the ray intersects the plane, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Raycast(Ray ray, out float enter) => Raycast(ray.origin, ray.direction, out enter);

        /// <summary>Compute intersection of line with plane.</summary>
        /// <param name="lineOrigin">The origin of the line.</param>
        /// <param name="lineDirection">The direction of the line.</param>
        /// <param name="intersectionPoint">The point of intersection.</param>
        /// <returns>True if the line intersects the plane, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool FindLineIntersection(float3 lineOrigin, float3 lineDirection, out float3 intersectionPoint)
        {
            if (Raycast(lineOrigin, lineDirection, out float t))
            {
                intersectionPoint = lineOrigin + t * lineDirection;
                return true;
            }

            intersectionPoint = Mathf.Infinity;
            return false;
        }

        /// <summary>Compute the intersection of three planes.</summary>
        /// <param name="p0">The first plane.</param>
        /// <param name="p1">The second plane.</param>
        /// <param name="p2">The third plane.</param>
        /// <param name="intersectionPoint">The point of intersection.</param>
        /// <returns>True if the planes intersect, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IntersectPlanes3(Plane p0, Plane p1, Plane p2, out float3 intersectionPoint)
        {
            float3 n0 = p0.Normal;
            float3 n1 = p1.Normal;
            float3 n2 = p2.Normal;

            float determinant = dot(cross(n0, n1), n2);
            if (abs(determinant) < MathConstants.ZeroTolerance)
            {
                intersectionPoint = float3.zero;
                return false;
            }

            intersectionPoint = ((cross(n2, n1) * p0.Distance) +
                                 (cross(n0, n2) * p1.Distance) -
                                 (cross(n0, n1) * p2.Distance)) / determinant;
            return true;
        }

        /// <summary>True if this plane is equal to another plane.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(Plane other) => this == other;

        /// <summary>True if this plane is equal to another object.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override bool Equals(object o) => o is Plane converted && Equals(converted);

        /// <summary>Returns a hash code for this plane.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override int GetHashCode() => float4(Normal, Distance).GetHashCode();
        
        /// <summary>Returns a string representation.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override string ToString() => $"Plane(Normal={Normal}, Distance={Distance})";

        /// <summary>Compare two planes for equality.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Plane lhs, Plane rhs) => all(lhs.Normal == rhs.Normal) && lhs.Distance == rhs.Distance;

        /// <summary>Compare two planes for inequality.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Plane lhs,Plane rhs) => any(lhs.Normal != rhs.Normal) || lhs.Distance != rhs.Distance;

        /// <summary>Converts a float4 to a plane.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Plane(float4 rhs) => new Plane { Normal = rhs.xyz, Distance = rhs.w };

        /// <summary>Converts a plane to a float4.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator float4(Plane rhs) => float4(rhs.Normal, rhs.Distance);

        /// <summary>Converts a <see cref="UnityEngine.Plane"/> to a <see cref="Plane"/></summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator UnityEngine.Plane(Plane rhs) => new UnityEngine.Plane { normal = rhs.Normal, distance = rhs.Distance };

        /// <summary>Converts a <see cref="Plane"/> to a <see cref="UnityEngine.Plane"/></summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Plane(UnityEngine.Plane rhs) => new Plane { Normal = rhs.normal, Distance = rhs.distance };
    }
}
