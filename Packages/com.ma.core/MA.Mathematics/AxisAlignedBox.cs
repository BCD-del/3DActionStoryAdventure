// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using static Unity.Mathematics.math;
using float4x4 = Unity.Mathematics.float4x4;

namespace MA.Mathematics
{
    /// <summary>Represents a 3D axis aligned <see cref="AxisAlignedBox"/>.</summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct AxisAlignedBox : System.IEquatable<AxisAlignedBox>
    {
        /// <summary>Represents an empty <see cref="AxisAlignedBox"/>.</summary>
        public static readonly AxisAlignedBox Empty = new AxisAlignedBox(float3(float.PositiveInfinity), float3(float.NegativeInfinity));
        /// <summary>Represents an infinitely large <see cref="AxisAlignedBox"/>.</summary>
        public static readonly AxisAlignedBox Infinite = new AxisAlignedBox(float3(float.NegativeInfinity), float3(float.PositiveInfinity));

        /// <summary>The minimal point of the box. This is always equal to Center-Extents.</summary>
        public float3 Min;
        /// <summary>The maximal point of the box. This is always equal to Center+Extents.</summary>
        public float3 Max;

        /// <summary>Returns true if this box is empty.</summary>
        public readonly bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => any(Max <= Min);
        }

        /// <summary>The width of this box (x-axis).</summary>
        public readonly float Width
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => max(Max.x - Min.x, 0);
        }

        /// <summary>The height of this box (y-axis).</summary>
        public readonly float Height
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => max(Max.y - Min.y, 0);
        }

        /// <summary>The depth of this box (z-axis).</summary>
        public readonly float Depth
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => max(Max.z - Min.z, 0);
        }

        /// <summary>The volume of this box.</summary>
        public readonly float Volume
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Width * Height * Depth;
        }

        /// <summary>The minimum dimension of this box.</summary>
        public readonly float MinDim
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => cmin(Size);
        }

        /// <summary>The maximum dimension of this box.</summary>
        public readonly float MaxDim
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => cmax(Size);
        }

        /// <summary>The center of the <see cref="AxisAlignedBox"/>.</summary>
        public float3 Center
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => (Min + Max) * 0.5f;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                float3 extents = Extents;
                Min = value - extents;
                Max = value + extents;
            }
        }

        /// <summary>The extents of the <see cref="AxisAlignedBox"/>.</summary>
        /// <remarks>This is always half of the size of the Size.</remarks>
        public float3 Extents
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => (Max - Min) * 0.5f;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                float3 center = Center;
                Min = center - value;
                Max = center + value;
            }
        }

        /// <summary>The diagonal size of the box.</summary>
        /// <remarks>This is always twice as large as the Extents.</remarks>
        public float3 Size
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => Max - Min;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                float3 center = Center;
                Min = center - value * 0.5f;
                Max = center + value * 0.5f;
            }
        }

        /// <summary>Returns the length of the diagonal size.</summary>
        public readonly float DiagonalLength
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => length(Max - Min);
        }

        /// <summary>Returns the squared length of the diagonal size.</summary>
        public readonly float DiagonalLengthSq
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => lengthsq(Max - Min);
        }

        /// <summary>Returns the length of the extents.</summary>
        public readonly float Radius
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => length(Extents);
        }

        /// <summary>Returns the squared length of the extents.</summary>
        public readonly float RadiusSq
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => lengthsq(Extents);
        }

        /// <summary>Returns the total surface area of the box.</summary>
        public readonly float SurfaceArea
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => 2.0f * (Width * (Height + Depth) + Height * Depth);
        }

        /// <summary>Creates a new <see cref="AxisAlignedBox"/> at center and extent.</summary>
        /// <param name="center">The center of the box.</param>
        /// <param name="extent">The extent of the box.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AxisAlignedBox FromExtents(float3 center, float3 extent)
            => new AxisAlignedBox(center - extent, center + extent);

        /// <summary>Creates a new <see cref="AxisAlignedBox"/>.</summary>
        /// <param name="min">The minimum point of the box.</param>
        /// <param name="max">The maximum point of the box.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AxisAlignedBox(in float3 min, in float3 max)
        {
            Min = min;
            Max = max;
        }

        /// <summary>Creates a new <see cref="AxisAlignedBox"/> using the minimum and maximum values from points a, b and c.</summary>
        /// <param name="a">The first point.</param>
        /// <param name="b">The second point.</param>
        /// <param name="c">The third point.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AxisAlignedBox(in float3 a, in float3 b, in float3 c)
        {
            Min = new float3(
                cmin(float3(a.x, b.x, c.x)),
                cmin(float3(a.y, b.y, c.y)),
                cmin(float3(a.z, b.z, c.z)));
            Max = new float3(
                cmax(float3(a.x, b.x, c.x)),
                cmax(float3(a.y, b.y, c.y)),
                cmax(float3(a.z, b.z, c.z)));
        }

        /// <summary>Creates a new <see cref="AxisAlignedBox"/> from another <see cref="AxisAlignedBox"/>, optionally transformed by a transform function.</summary>
        /// <param name="box">The <see cref="AxisAlignedBox"/> to copy.</param>
        /// <param name="transform">The transform function to apply to each corner of the box.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AxisAlignedBox(in AxisAlignedBox box, System.Func<float3, float3> transform = null)
        {
            if (transform == null)
            {
                Min = box.Min;
                Max = box.Max;
                return;
            }

            Min = Max = transform(box.GetCornerAt(0));
            for (int i = 1; i < 8; ++i)
                Encapsulate(transform(box.GetCornerAt(i)));
        }

        /// <summary>Creates a <see cref="AxisAlignedBox"/> box from another <see cref="AxisAlignedBox"/>, transformed by a <see cref="LocalTransform"/>.</summary>
        /// <param name="box">The <see cref="AxisAlignedBox"/> to copy.</param>
        /// <param name="transform">The transform to apply to each corner of the box.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AxisAlignedBox(in AxisAlignedBox box, in LocalTransform transform)
        {
            Min = Max = transform.TransformPoint(box.GetCornerAt(0));
            for (int i = 1; i < 8; ++i)
                Encapsulate(transform.TransformPoint(box.GetCornerAt(i)));
        }

        /// <summary>Creates a new <see cref="AxisAlignedBox"/> from a <see cref="Bounds"/>.</summary>
        /// <param name="bounds">The bounds to copy.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AxisAlignedBox(in Bounds bounds)
        {
            Min = bounds.min;
            Max = bounds.max;
        }

        /// <summary>Creates a new <see cref="Sphere"/> that encapsulates this <see cref="AxisAlignedBox"/>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Sphere GetBoundingSphere() => new(Center, Radius);

        /// <summary>Returns the dimension of the <see cref="AxisAlignedBox"/> on the axis specified by axisIndex.</summary>
        /// <param name="axisIndex">The index of the axis.</param>
        /// <returns>The dimension of the <see cref="AxisAlignedBox"/> on the axis specified by axisIndex.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float Dimension(int axisIndex) => max(Max[axisIndex] - Min[axisIndex], 0);

        /// <summary>Corner point on the <see cref="AxisAlignedBox"/> identified by the given index.</summary>
        /// <remarks>Corners: [ (-x,-y), (x,-y), (x,y), (-x,y) ], -z, then +z</remarks>
        /// <param name="index">Index corner index in range 0-7</param>
        /// <returns>Corner point on the <see cref="AxisAlignedBox"/> identified by the given index.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float3 GetCornerAt(int index)
        {
            Assert.IsTrue(index is >= 0 and <= 7);
            float x = (((index & 1) != 0) ^ ((index & 2) != 0)) ? Max.x : Min.x;
            float y = ((index / 2) % 2 == 0) ? Min.y : Max.y;
            float z = (index < 4) ? Min.z : Max.z;
            return new float3(x, y, z);
        }

        /// <summary>Expands the <see cref="AxisAlignedBox"/> to contain point.</summary>
        /// <param name="point">The point to encapsulate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Encapsulate(in float3 point)
        {
            Min = min(Min, point);
            Max = max(Max, point);
        }

        /// <summary>Expands the <see cref="AxisAlignedBox"/> to contain another <see cref="AxisAlignedBox"/>.</summary>
        /// <param name="other">The <see cref="AxisAlignedBox"/> to encapsulate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Encapsulate(in AxisAlignedBox other)
        {
            Min = min(Min, other.Min);
            Max = max(Max, other.Max);
        }

        /// <summary>Returns true if this <see cref="AxisAlignedBox"/> contains point.</summary>
        /// <param name="point">The point to test.</param>
        /// <returns>True if this <see cref="AxisAlignedBox"/> contains point.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(in float3 point) => all(point >= Min & point <= Max);

        /// <summary>Returns true if this box contains another box.</summary>
        /// <param name="rhs">The <see cref="AxisAlignedBox"/> to test.</param>
        /// <returns>True if this <see cref="AxisAlignedBox"/> contains <paramref name="rhs"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(in AxisAlignedBox rhs) => all(Min <= rhs.Min & Max >= rhs.Max);

        /// <summary>Returns true if another <see cref="AxisAlignedBox"/> contains this <see cref="AxisAlignedBox"/>.</summary>
        /// <param name="rhs">The <see cref="AxisAlignedBox"/> to test.</param>
        /// <returns>True if <paramref name="rhs"/> contains this <see cref="AxisAlignedBox"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool IsInside(in AxisAlignedBox rhs) => rhs.Contains(this);

        /// <summary>Returns the closest point on this <see cref="AxisAlignedBox"/> to <paramref name="point"/>.</summary>
        /// <param name="point">The point to test.</param>
        /// <returns>A point on this <see cref="AxisAlignedBox"/> closest to <paramref name="point"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float3 GetClosestPointTo(in float3 point) => clamp(point, Min, Max);

        /// <summary>Returns the squared distance between this <see cref="AxisAlignedBox"/> and point.</summary>
        /// <param name="point">The point to test.</param>
        /// <returns>The squared distance between this <see cref="AxisAlignedBox"/> and point.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float DistanceSquared(in float3 point) => lengthsq(max(abs(point - Center), Extents) - Extents);

        /// <summary>Returns the squared distance between this <see cref="AxisAlignedBox"/> and other.</summary>
        /// <param name="other">The <see cref="AxisAlignedBox"/> to test.</param>
        /// <returns>The squared distance between this <see cref="AxisAlignedBox"/> and other.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float DistanceSquared(in AxisAlignedBox other) => lengthsq(max(0, abs(other.Center - Center) - (other.Extents + Extents)));

        /// <summary>Returns true if this <see cref="AxisAlignedBox"/> intersects sphere.</summary>
        /// <param name="center">The center of the sphere.</param>
        /// <param name="radiusSq">The squared radius of the sphere.</param>
        /// <returns>True if this <see cref="AxisAlignedBox"/>  intersects sphere.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool OverlapsSphereSq(in float3 center, float radiusSq)
        {
            if (IsEmpty) return false;
            float3 closest = min(max(center, Min), Max);
            float3 delta = center - closest;
            float distSq = lengthsq(delta);
            return distSq <= radiusSq;
        }

        /// <summary>Returns true if this <see cref="AxisAlignedBox"/> intersects sphere.</summary>
        /// <param name="center">The center of the sphere.</param>
        /// <param name="radius">The radius of the sphere.</param>
        /// <returns>True if this <see cref="AxisAlignedBox"/>  intersects sphere.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool OverlapsSphere(in float3 center, float radius) => OverlapsSphereSq(center, radius * radius);

        /// <summary>Returns true if this <see cref="AxisAlignedBox"/> intersects <see cref="Sphere"/>.</summary>
        /// <param name="sphere">The sphere to test.</param>
        /// <returns>True if this <see cref="AxisAlignedBox"/> intersects <see cref="Sphere"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool OverlapsSphere(in Sphere sphere) => OverlapsSphereSq(sphere.Center, sphere.Radius * sphere.Radius);

        /// <summary>Returns true if this <see cref="AxisAlignedBox"/> intersects another <see cref="AxisAlignedBox"/>.</summary>
        /// <param name="other">The <see cref="AxisAlignedBox"/> to test.</param>
        /// <returns>True if the <see cref="AxisAlignedBox"/>es intersect.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Overlaps(in AxisAlignedBox other) => all(Max >= other.Min & Min <= other.Max);

        /// <summary>Returns true if this <see cref="AxisAlignedBox"/>  intersects a <see cref="UnityEngine.Ray"/>.</summary>
        /// <param name="ray">The <see cref="UnityEngine.Ray"/> to test.</param>
        /// <param name="tolerance">The tolerance to use.</param>
        /// <returns>True if the <see cref="UnityEngine.Ray"/> intersects this <see cref="AxisAlignedBox"/> .</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Overlaps(in UnityEngine.Ray ray, float tolerance = MathConstants.ZeroTolerance)
        {
            if (IsEmpty) return false;

            float3 delta = (float3)ray.origin - Center;
            float3 extent = Extents + tolerance;

            float3 dir = ray.direction;
            float3 absDir = abs(dir);
            float3 absDelta = abs(delta);
            if (any(absDelta > extent & (delta * dir >= 0)))
                return false;

            float3 crossDelta = cross(dir, delta);
            float3 absCrossDelta = abs(crossDelta);
            float3 rhs = extent * absDir + extent * absDir;
            if (any(absCrossDelta > rhs))
                return false;

            return true;
        }

        /// <summary>Returns a <see cref="AxisAlignedBox"/> that is the intersection of this <see cref="AxisAlignedBox"/> and other.</summary>
        /// <param name="other">The <see cref="AxisAlignedBox"/> to intersect with.</param>
        /// <returns>A <see cref="AxisAlignedBox"/> that is the intersection of this <see cref="AxisAlignedBox"/> and other.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly AxisAlignedBox Intersect(in AxisAlignedBox other)
        {
            AxisAlignedBox intersection = new AxisAlignedBox(max(Min, other.Min), min(Max, other.Max));
            return intersection.Height <= 0f || intersection.Width <= 0f || intersection.Depth <= 0f
                ? Empty
                : intersection;
        }

        /// <summary>Returns an <see cref="AxisAlignedBox"/> translated by the given offset.</summary>
        /// <param name="offset">The offset to translate by.</param>
        /// <returns>An <see cref="AxisAlignedBox"/> translated by the given offset.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly AxisAlignedBox Translate(in float3 offset) => IsEmpty ? Empty : new AxisAlignedBox(Min + offset, Max + offset);

        /// <summary>Computes the corners of the <see cref="AxisAlignedBox"/>.</summary>
        /// <param name="vertices">The span to store the corners in.</param>
        /// <exception cref="ArgumentException">Thrown when vertices length is less than 8.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void ComputeCorners(Span<float3> vertices)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (vertices.Length < 8)
                throw new ArgumentException("Vertices must have a length of at least 8.", nameof(vertices));
#endif

            vertices[0] = float3(Min);
            vertices[1] = float3(Min.x, Min.y, Max.z);
            vertices[2] = float3(Min.x, Max.y, Min.z);
            vertices[3] = float3(Max.x, Min.y, Min.z);
            vertices[4] = float3(Max.x, Max.y, Min.z);
            vertices[5] = float3(Max.x, Min.y, Max.z);
            vertices[6] = float3(Min.x, Max.y, Max.z);
            vertices[7] = float3(Max);
        }

        /// <summary>Gets a <see cref="AxisAlignedBox"/> transformed by a rotation matrix.</summary>
        /// <param name="m">The matrix to transform by.</param>
        /// <remarks>The resulting AABB encapsulates the transformed AABB which may not be axis aligned after the transformation.</remarks>
        /// <returns>A <see cref="AxisAlignedBox"/> transformed by a matrix.</returns>
        /// <seealso cref="Unity.Mathematics.Geometry.Math.Transform(float3x3, Unity.Mathematics.Geometry.MinMaxAABB)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly AxisAlignedBox RotateBy(in float3x3 m)
        {
            if (IsEmpty)
                return Empty;

            float3 t1 = m.c0.xyz * Min.xxx;
            float3 t2 = m.c0.xyz * Max.xxx;
            bool3 minMask = t1 < t2;
            AxisAlignedBox rotated = new AxisAlignedBox(select(t2, t1, minMask), select(t2, t1, !minMask));

            t1 = m.c1.xyz * Min.yyy;
            t2 = m.c1.xyz * Max.yyy;
            minMask = t1 < t2;
            rotated.Min += select(t2, t1, minMask);
            rotated.Max += select(t2, t1, !minMask);

            t1 = m.c2.xyz * Min.zzz;
            t2 = m.c2.xyz * Max.zzz;
            minMask = t1 < t2;
            rotated.Min += select(t2, t1, minMask);
            rotated.Max += select(t2, t1, !minMask);

            return rotated;
        }

        /// <summary>Gets a <see cref="AxisAlignedBox"/> transformed by a matrix.</summary>
        /// <param name="m">The matrix to transform by.</param>
        /// <returns>A <see cref="AxisAlignedBox"/> transformed by a matrix.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly AxisAlignedBox TransformBy(in float4x4 m)
        {
            if (IsEmpty)
                return Empty;

            float4 min = new float4(Min, 0);
            float4 max = new float4(Max, 0);

            float4 extent = (max - min) * 0.5f;
            float4 center = (max + min) * 0.5f;

            float4 newExtent = abs(extent.xxxx * m.c0);
            newExtent += abs(extent.yyyy * m.c1);
            newExtent += abs(extent.zzzz * m.c2);

            float4 newCenter = center.xxxx * m.c0;
            newCenter += center.yyyy * m.c1;
            newCenter += center.zzzz * m.c2;
            newCenter += m.c3;

            float4 newMin = (newCenter - newExtent);
            float4 newMax = (newCenter + newExtent);

            return new AxisAlignedBox(newMin.xyz, newMax.xyz);
        }

        /// <summary>Gets a <see cref="AxisAlignedBox"/> transformed by transform.</summary>
        /// <param name="transform">The transform to transform by.</param>
        /// <returns>A <see cref="AxisAlignedBox"/> transformed by transform.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly AxisAlignedBox TransformBy(in LocalTransform transform)
        {
            return TransformBy(transform.ToMatrix());
            // // Rotate each axis individually and find their new positions in the rotated space.
            // float3 extent = Extents;
            // float3 x = rotate(transform.Rotation, new float3(extent.x, 0, 0));
            // float3 y = rotate(transform.Rotation, new float3(0, extent.y, 0));
            // float3 z = rotate(transform.Rotation, new float3(0, 0, extent.z));
            //
            // // Find the new max corner by summing the rotated axes.  Absolute value of each axis
            // // since we are trying to find the max corner.
            // float3 newExtent = abs(x) + abs(y) + abs(z);
            // float3 newCenter = transform.TransformPoint(Center);
            //
            // return new AxisAlignedBox(newCenter - newExtent, newCenter + newExtent);
        }

        /// <summary>Gets a <see cref="AxisAlignedBox"/> transformed by a matrix.</summary>
        /// <param name="matrix">The matrix to inverse transform by.</param>
        /// <returns>A <see cref="AxisAlignedBox"/> inverse transformed by a matrix.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly AxisAlignedBox InverseTransformBy(in float4x4 matrix) => TransformBy(inverse(matrix));

        /// <summary>Gets a <see cref="AxisAlignedBox"/> transformed by a transform.</summary>
        /// <param name="transform">The transform to inverse transform by.</param>
        /// <returns>A <see cref="AxisAlignedBox"/> inverse transformed by a transform.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly AxisAlignedBox InverseTransformBy(in LocalTransform transform) => InverseTransformBy(transform.ToMatrix());

        /// <summary>Returns the current world <see cref="AxisAlignedBox"/> transformed and projected to screen space.</summary>
        /// <param name="projectionMatrix">The projection matrix to transform by.</param>
        /// <returns>The current world <see cref="AxisAlignedBox"/> transformed and projected to screen space.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly AxisAlignedBox TransformProjectBy(in float4x4 projectionMatrix)
        {
            if (IsEmpty) return Empty;

            Span<float3> corners = stackalloc float3[8];
            ComputeCorners(corners);

            AxisAlignedBox projectedBox = Empty;
            for (int i = 0; i < corners.Length; i++)
            {
                float4 projectedVertex = mul(projectionMatrix, float4(corners[i], 1.0f));
                projectedBox += projectedVertex.xyz * rcp(projectedVertex.w);
            }

            return projectedBox;
        }

        /// <summary>Expand the bounds by increasing its size by radius.</summary>
        /// <param name="radius">The radius to expand by.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Expand(float radius)
        {
            Max += radius;
            Min -= radius;
        }

        /// <summary>Returns a string representation.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
            => $"AxisAlignedBox(Min={Min}, Max={Max})";

        /// <summary>Returns a string representation of the float3 using a specified format and culture-specific format information.</summary>
        /// <param name="format">Format string to use during string formatting.</param>
        /// <param name="formatProvider">Format provider to use during string formatting.</param>
        /// <returns>String representation of the value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(string format, IFormatProvider formatProvider)
            => $"AxisAlignedBox(Min={Min.ToString(format, formatProvider)}, Max={Max.ToString(format, formatProvider)})";

        /// <summary>Returns a <see cref="AxisAlignedBox"/> that encapsulates <param name="lhs"></param> and <paramref name="rhs"/>.</summary>
        /// <param name="lhs">The source <see cref="AxisAlignedBox"/>.</param>
        /// <param name="rhs">The point to encapsulate.</param>
        /// <returns>A <see cref="AxisAlignedBox"/> that encapsulates <param name="lhs"></param> and <paramref name="rhs"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AxisAlignedBox operator +(AxisAlignedBox lhs, float3 rhs)
        {
            lhs.Encapsulate(rhs);
            return lhs;
        }

        /// <summary>Returns a <see cref="AxisAlignedBox"/> that encapsulates <param name="lhs"></param> and <paramref name="rhs"/>.</summary>
        /// <param name="lhs">The source <see cref="AxisAlignedBox"/>.</param>
        /// <param name="rhs">The <see cref="AxisAlignedBox"/> to encapsulate.</param>
        /// <returns>A <see cref="AxisAlignedBox"/> that encapsulates <param name="lhs"></param> and <paramref name="rhs"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AxisAlignedBox operator +(AxisAlignedBox lhs, AxisAlignedBox rhs)
        {
            lhs.Encapsulate(rhs);
            return lhs;
        }

        /// <summary>Determines another <see cref="AxisAlignedBox"/> is equal to this instance.</summary>
        /// <param name="other">The <see cref="AxisAlignedBox"/> to compare with.</param>
        /// <returns>True this <see cref="AxisAlignedBox"/> is equal to other.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(AxisAlignedBox other) => this == other;

        /// <summary>Determines whether this <see cref="AxisAlignedBox"/> is equal to an <see cref="object"/>.</summary>
        /// <param name="o">The object to compare.</param>
        /// <returns>True if the <see cref="object"/> is an <see cref="AxisAlignedBox"/> and is equal to this instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override bool Equals(object o) => o is AxisAlignedBox converted && Equals(converted);

        /// <summary>Returns a hash code for this <see cref="AxisAlignedBox"/>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override int GetHashCode()
            => (int)(csum(asuint(Min) * uint3(0x713BD06Fu, 0x753AD6ADu, 0xD19764C7u) +
                          asuint(Max) * uint3(0xB5D0BF63u, 0xF9102C5Fu, 0x9881FB9Fu)) + 0x4FC93C25u);

        /// <summary>Returns true if two <see cref="AxisAlignedBox"/> are equal.</summary>
        /// <param name="lhs">The first <see cref="AxisAlignedBox"/> to compare.</param>
        /// <param name="rhs">The second <see cref="AxisAlignedBox"/> to compare.</param>
        /// <returns>True if the two <see cref="AxisAlignedBox"/> are equal.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(AxisAlignedBox lhs, AxisAlignedBox rhs) => all(lhs.Min == rhs.Min) && all(lhs.Max == rhs.Max);

        /// <summary>Returns true if two <see cref="AxisAlignedBox"/> are not equal.</summary>
        /// <param name="lhs">The first <see cref="AxisAlignedBox"/> to compare.</param>
        /// <param name="rhs">The second <see cref="AxisAlignedBox"/> to compare.</param>
        /// <returns>True if the two <see cref="AxisAlignedBox"/> are not equal.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(AxisAlignedBox lhs, AxisAlignedBox rhs) => any(lhs.Min != rhs.Min) || any(lhs.Max != rhs.Max);

        /// <summary>Returns a <see cref="Bounds"/> representation of an <see cref="AxisAlignedBox"/>.</summary>
        /// <param name="rhs">The box to convert.</param>
        /// <returns>A <see cref="Bounds"/> representation of an <see cref="AxisAlignedBox"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Bounds(in AxisAlignedBox rhs) => new Bounds(rhs.Center, rhs.Size);

        /// <summary>Returns an <see cref="AxisAlignedBox"/> representation of a <see cref="Bounds"/>.</summary>
        /// <param name="rhs">The bounds to convert.</param>
        /// <returns>An <see cref="AxisAlignedBox"/> representation of a <see cref="Bounds"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AxisAlignedBox(in Bounds rhs) => new AxisAlignedBox(rhs.min, rhs.max);
    }

    public static class AxisAlignedBoxHelpers
    {
        /// <summary>Returns an AxisAlignedBox representation of a Bounds.</summary>
        /// <param name="bounds">The bounds to convert.</param>
        /// <returns>An AxisAlignedBox representation of a Bounds.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AxisAlignedBox AsAxisAlignedBox(this Bounds bounds) => bounds;

        /// <summary>Returns an AxisAlignedBox representation of a Renderer's local bounds.</summary>
        /// <param name="renderer">The renderer to get the bounds from.</param>
        /// <returns>An AxisAlignedBox representation of a Renderer's local bounds.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AxisAlignedBox GetLocalAxisAlignedBox(this Renderer renderer) => renderer.localBounds;

        /// <summary>Returns an AxisAlignedBox representation of a Renderer's world bounds.</summary>
        /// <param name="renderer">The renderer to get the bounds from.</param>
        /// <returns>An AxisAlignedBox representation of a Renderer's world bounds.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AxisAlignedBox GetWorldAxisAlignedBox(this Renderer renderer) => renderer.bounds;

        /// <summary>Returns an AxisAlignedBox representation of a Mesh's local bounds.</summary>
        /// <param name="mesh">The mesh to get the bounds from.</param>
        /// <returns>An AxisAlignedBox representation of a Mesh's local bounds.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AxisAlignedBox GetLocalAxisAlignedBox(this Mesh mesh) => mesh.bounds;
    }
}
