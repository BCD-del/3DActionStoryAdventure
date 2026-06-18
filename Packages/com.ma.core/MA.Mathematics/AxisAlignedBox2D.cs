// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using static Unity.Mathematics.math;

namespace MA.Mathematics
{
    /// <summary>Represents a 3D axis aligned <see cref="AxisAlignedBox2D"/>.</summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct AxisAlignedBox2D : System.IEquatable<AxisAlignedBox2D>
    {
        /// <summary>Represents an empty <see cref="AxisAlignedBox2D"/>.</summary>
        public static readonly AxisAlignedBox2D Empty = new AxisAlignedBox2D(float2(float.PositiveInfinity), float2(float.NegativeInfinity));
        /// <summary>Represents an infinitely large <see cref="AxisAlignedBox2D"/>.</summary>
        public static readonly AxisAlignedBox2D Infinite = new AxisAlignedBox2D(float2(float.NegativeInfinity), float2(float.PositiveInfinity));

        /// <summary>The minimal point of the box. This is always equal to Center-Extents.</summary>
        public float2 Min;
        /// <summary>The maximal point of the box. This is always equal to Center+Extents.</summary>
        public float2 Max;

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

        /// <summary>The center of the <see cref="AxisAlignedBox2D"/>.</summary>
        public float2 Center
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => (Min + Max) * 0.5f;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                float2 extents = Extents;
                Min = value - extents;
                Max = value + extents;
            }
        }

        /// <summary>The extents of the <see cref="AxisAlignedBox2D"/>.</summary>
        /// <remarks>This is always half of the size of the Size.</remarks>
        public float2 Extents
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => (Max - Min) * 0.5f;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                float2 center = Center;
                Min = center - value;
                Max = center + value;
            }
        }

        /// <summary>The diagonal size of the box.</summary>
        /// <remarks>This is always twice as large as the Extents.</remarks>
        public float2 Size
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => Max - Min;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                float2 center = Center;
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
            get => Width * Height;
        }

        /// <summary>Creates a new <see cref="AxisAlignedBox2D"/> at center and extent.</summary>
        /// <param name="center">The center of the box.</param>
        /// <param name="extent">The extent of the box.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AxisAlignedBox2D FromExtents(in float2 center, in float2 extent) => new AxisAlignedBox2D(center - extent, center + extent);

        /// <summary>Creates a new <see cref="AxisAlignedBox2D"/>.</summary>
        /// <param name="min">The minimum point of the box.</param>
        /// <param name="max">The maximum point of the box.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AxisAlignedBox2D(in float2 min, in float2 max)
        {
            Min = min;
            Max = max;
        }

        /// <summary>Creates a new <see cref="AxisAlignedBox2D"/> from another <see cref="AxisAlignedBox2D"/>, optionally transformed by a transform function.</summary>
        /// <param name="box">The <see cref="AxisAlignedBox2D"/> to copy.</param>
        /// <param name="transform">The transform function to apply to each corner of the box.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AxisAlignedBox2D(in AxisAlignedBox2D box, System.Func<float2, float2> transform = null)
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

        /// <summary>Returns the dimension of the <see cref="AxisAlignedBox2D"/> on the axis specified by axisIndex.</summary>
        /// <param name="axisIndex">The index of the axis.</param>
        /// <returns>The dimension of the <see cref="AxisAlignedBox2D"/> on the axis specified by axisIndex.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float Dimension(int axisIndex) => max(Max[axisIndex] - Min[axisIndex], 0);

        /// <summary>Corner point on the <see cref="AxisAlignedBox2D"/> identified by the given index.</summary>
        /// <remarks>Corners: [ (-x,-y), (x,-y), (x,y), (-x,y) ], -z, then +z</remarks>
        /// <param name="index">Index corner index in range 0-7</param>
        /// <returns>Corner point on the <see cref="AxisAlignedBox2D"/> identified by the given index.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float2 GetCornerAt(int index)
        {
            Assert.IsTrue(index is >= 0 and <= 3);
            float x = (((index & 1) != 0) ^ ((index & 2) != 0)) ? Max.x : Min.x;
            float y = ((index / 2) % 2 == 0) ? Min.y : Max.y;
            return new float2(x, y);
        }

        /// <summary>Returns the closest point on this <see cref="AxisAlignedBox"/> to <paramref name="point"/>.</summary>
        /// <param name="point">The point to test.</param>
        /// <returns>A point on this <see cref="AxisAlignedBox"/> closest to <paramref name="point"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float2 GetClosestPointTo(in float2 point) => clamp(point, Min, Max);

        /// <summary>Expands the <see cref="AxisAlignedBox2D"/> to contain point.</summary>
        /// <param name="point">The point to encapsulate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Encapsulate(in float2 point)
        {
            Min = min(Min, point);
            Max = max(Max, point);
        }

        /// <summary>Expands the <see cref="AxisAlignedBox2D"/> to contain another <see cref="AxisAlignedBox2D"/>.</summary>
        /// <param name="other">The <see cref="AxisAlignedBox2D"/> to encapsulate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Encapsulate(in AxisAlignedBox2D other)
        {
            Min = min(Min, other.Min);
            Max = max(Max, other.Max);
        }

        /// <summary>Returns true if this <see cref="AxisAlignedBox2D"/> contains point.</summary>
        /// <param name="point">The point to test.</param>
        /// <returns>True if this <see cref="AxisAlignedBox2D"/> contains point.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(in float2 point) => all(point >= Min & point <= Max);

        /// <summary>Returns true if this box contains another box.</summary>
        /// <param name="rhs">The <see cref="AxisAlignedBox2D"/> to test.</param>
        /// <returns>True if this <see cref="AxisAlignedBox2D"/> contains <paramref name="rhs"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(in AxisAlignedBox2D rhs) => all(Min <= rhs.Min & Max >= rhs.Max);

        /// <summary>Returns true if another <see cref="AxisAlignedBox2D"/> contains this <see cref="AxisAlignedBox2D"/>.</summary>
        /// <param name="rhs">The <see cref="AxisAlignedBox2D"/> to test.</param>
        /// <returns>True if <paramref name="rhs"/> contains this <see cref="AxisAlignedBox2D"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool IsInside(in AxisAlignedBox2D rhs) => rhs.Contains(this);

        /// <summary>Returns true if this <see cref="AxisAlignedBox2D"/> intersects circle.</summary>
        /// <param name="sphereCenter">The center of the circle.</param>
        /// <param name="sphereRadiusSq">The squared radius of the circle.</param>
        /// <returns>True if this <see cref="AxisAlignedBox2D"/>  intersects circle.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Overlaps(in float2 sphereCenter, float sphereRadiusSq)
        {
            if (IsEmpty) return false;
            float2 closest = min(max(sphereCenter, Min), Max);
            float2 delta = sphereCenter - closest;
            float distSq = lengthsq(delta);
            return distSq <= sphereRadiusSq;
        }

        /// <summary>Returns true if this <see cref="AxisAlignedBox2D"/> intersects another <see cref="AxisAlignedBox2D"/>.</summary>
        /// <param name="other">The <see cref="AxisAlignedBox2D"/> to test.</param>
        /// <returns>True if the <see cref="AxisAlignedBox2D"/>es intersect.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Overlaps(in AxisAlignedBox2D other) => all(Max >= other.Min & Min <= other.Max);

        /// <summary>Returns a <see cref="AxisAlignedBox2D"/> that is the intersection of this <see cref="AxisAlignedBox2D"/> and other.</summary>
        /// <param name="other">The <see cref="AxisAlignedBox2D"/> to intersect with.</param>
        /// <returns>A <see cref="AxisAlignedBox2D"/> that is the intersection of this <see cref="AxisAlignedBox2D"/> and other.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly AxisAlignedBox2D Intersect(in AxisAlignedBox2D other)
        {
            AxisAlignedBox2D intersection = new AxisAlignedBox2D(max(Min, other.Min), min(Max, other.Max));
            return intersection.Height <= 0f || intersection.Width <= 0f ? Empty : intersection;
        }

        /// <summary>Returns an <see cref="AxisAlignedBox2D"/> translated by the given offset.</summary>
        /// <param name="offset">The offset to translate by.</param>
        /// <returns>An <see cref="AxisAlignedBox2D"/> translated by the given offset.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly AxisAlignedBox2D Translate(in float2 offset) => IsEmpty ? Empty : new AxisAlignedBox2D(Min + offset, Max + offset);

        /// <summary>Returns the squared distance between this <see cref="AxisAlignedBox2D"/> and point.</summary>
        /// <param name="point">The point to test.</param>
        /// <returns>The squared distance between this <see cref="AxisAlignedBox2D"/> and point.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float DistanceSquared(in float2 point) => lengthsq(max(abs(point - Center), Extents) - Extents);

        /// <summary>Returns the squared distance between this <see cref="AxisAlignedBox2D"/> and other.</summary>
        /// <param name="other">The <see cref="AxisAlignedBox2D"/> to test.</param>
        /// <returns>The squared distance between this <see cref="AxisAlignedBox2D"/> and other.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float DistanceSquared(in AxisAlignedBox2D other) => lengthsq(max(0, abs(other.Center - Center) - (other.Extents + Extents)));

        /// <summary>Expand the bounds by increasing its size by radius.</summary>
        /// <param name="radius">The radius to expand by.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Expand(float radius)
        {
            Max += radius;
            Min -= radius;
        }

        /// <summary>Creats a 3D <see cref="AxisAlignedBox"/> from this <see cref="AxisAlignedBox2D"/>.</summary>
        /// <param name="minY">The minimum y value of the 3D box.</param>
        /// <param name="maxY">The maximum y value of the 3D box.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly AxisAlignedBox ToAxisAlignedBox(float minY, float maxY) => new AxisAlignedBox(new float3(Min.x, minY, Min.y), new float3(Max.x, maxY, Max.y));

        /// <summary>Returns a string representation.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => $"AxisAlignedBox2D(Min={Min}, Max={Max})";

        /// <summary>Returns a <see cref="AxisAlignedBox2D"/> that encapsulates <param name="lhs"></param> and <paramref name="rhs"/>.</summary>
        /// <param name="lhs">The source <see cref="AxisAlignedBox2D"/>.</param>
        /// <param name="rhs">The point to encapsulate.</param>
        /// <returns>A <see cref="AxisAlignedBox2D"/> that encapsulates <param name="lhs"></param> and <paramref name="rhs"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AxisAlignedBox2D operator +(AxisAlignedBox2D lhs, float2 rhs)
        {
            lhs.Encapsulate(rhs);
            return lhs;
        }

        /// <summary>Returns a <see cref="AxisAlignedBox2D"/> that encapsulates <param name="lhs"></param> and <paramref name="rhs"/>.</summary>
        /// <param name="lhs">The source <see cref="AxisAlignedBox2D"/>.</param>
        /// <param name="rhs">The <see cref="AxisAlignedBox2D"/> to encapsulate.</param>
        /// <returns>A <see cref="AxisAlignedBox2D"/> that encapsulates <param name="lhs"></param> and <paramref name="rhs"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AxisAlignedBox2D operator +(AxisAlignedBox2D lhs, AxisAlignedBox2D rhs)
        {
            lhs.Encapsulate(rhs);
            return lhs;
        }

        /// <summary>Determines another <see cref="AxisAlignedBox2D"/> is equal to this instance.</summary>
        /// <param name="other">The <see cref="AxisAlignedBox2D"/> to compare with.</param>
        /// <returns>True this <see cref="AxisAlignedBox2D"/> is equal to other.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(AxisAlignedBox2D other) => this == other;

        /// <summary>Determines whether this <see cref="AxisAlignedBox2D"/> is equal to an <see cref="object"/>.</summary>
        /// <param name="o">The object to compare.</param>
        /// <returns>True if the <see cref="object"/> is an <see cref="AxisAlignedBox2D"/> and is equal to this instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override bool Equals(object o) => o is AxisAlignedBox2D converted && Equals(converted);

        /// <summary>Returns a hash code for this <see cref="AxisAlignedBox2D"/>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override int GetHashCode()
            => (int)(csum(asuint(Min) * uint2(0x713BD06Fu, 0x753AD6ADu) +
                          asuint(Max) * uint2(0xB5D0BF63u, 0xF9102C5Fu)) + 0x4FC93C25u);

        /// <summary>Returns true if two <see cref="AxisAlignedBox2D"/> are equal.</summary>
        /// <param name="lhs">The first <see cref="AxisAlignedBox2D"/> to compare.</param>
        /// <param name="rhs">The second <see cref="AxisAlignedBox2D"/> to compare.</param>
        /// <returns>True if the two <see cref="AxisAlignedBox2D"/> are equal.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(AxisAlignedBox2D lhs, AxisAlignedBox2D rhs) => all(lhs.Min == rhs.Min) && all(lhs.Max == rhs.Max);

        /// <summary>Returns true if two <see cref="AxisAlignedBox2D"/> are not equal.</summary>
        /// <param name="lhs">The first <see cref="AxisAlignedBox2D"/> to compare.</param>
        /// <param name="rhs">The second <see cref="AxisAlignedBox2D"/> to compare.</param>
        /// <returns>True if the two <see cref="AxisAlignedBox2D"/> are not equal.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(AxisAlignedBox2D lhs, AxisAlignedBox2D rhs) => any(lhs.Min != rhs.Min) || any(lhs.Max != rhs.Max);

        /// <summary>Implicitly converts a <see cref="AxisAlignedBox2D"/> to a <see cref="Rect"/>.</summary>
        /// <param name="box">The <see cref="AxisAlignedBox2D"/> to convert.</param>
        /// <returns>A <see cref="Rect"/> representing the same box.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Rect(AxisAlignedBox2D box) => new Rect(box.Center, box.Size);

        /// <summary>Implicitly converts a <see cref="Rect"/> to a <see cref="AxisAlignedBox2D"/>.</summary>
        /// <param name="rect">The <see cref="Rect"/> to convert.</param>
        /// <returns>A <see cref="AxisAlignedBox2D"/> representing the same box.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AxisAlignedBox2D(Rect rect) => new AxisAlignedBox2D(rect.min, rect.max);
    }
}
