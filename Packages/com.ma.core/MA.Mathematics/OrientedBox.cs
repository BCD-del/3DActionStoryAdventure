// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using float3 = Unity.Mathematics.float3;

namespace MA.Mathematics
{
    /// <summary>A box with an orientation.</summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct OrientedBox : IEquatable<OrientedBox>
    {
        /// <summary>A unit box with its center at the origin.</summary>
        public static readonly OrientedBox UnitCentered = new OrientedBox(float3.zero, 0.5f);
        
        /// <summary>A unit box with its minimum corner at the origin.</summary>
        public static readonly OrientedBox UnitPositive = new OrientedBox(0.5f, 0.5f);
        
        /// <summary>The frame of reference for the box.</summary>
        public LocalFrame Frame;
        
        /// <summary>The extents of the box.</summary>
        public float3 Extents;
        
        /// <summary>Constructs a new <see cref="OrientedBox"/>.</summary>
        /// <param name="frame">The frame of reference for the box.</param>
        /// <param name="extents">The extents of the box.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public OrientedBox(in LocalFrame frame, float3 extents)
        {
            Frame = frame;
            Extents = extents;
        }
        
        /// <summary>Constructs a new <see cref="OrientedBox"/>.</summary>
        /// <param name="position">The position of the box.</param>
        /// <param name="extents">The extents of the box.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public OrientedBox(float3 position, float3 extents)
        {
            Frame = new LocalFrame(position);
            Extents = extents;
        }
        
        /// <summary>Constructs a new <see cref="OrientedBox"/>.</summary>
        /// <param name="position">The position of the box.</param>
        /// <param name="rotation">The rotation of the box.</param>
        /// <param name="extents">The extents of the box.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public OrientedBox(float3 position, quaternion rotation, float3 extents)
        {
            Frame = new LocalFrame(position, rotation);
            Extents = extents;
        }
        
        /// <summary>Creates a new <see cref="OrientedBox"/> from a <see cref="AxisAlignedBox"/>.</summary>
        /// <param name="box">The axis-aligned box to construct the oriented box from.</param>
        /// <returns>The constructed <see cref="OrientedBox"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static OrientedBox FromAxisAlignedBox(in AxisAlignedBox box) 
            => new OrientedBox(box.Center, box.Extents);
        
        /// <summary>The center of the box.</summary>
        public float3 Center
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => Frame.Position;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Frame.Position = value;
        }
        
        /// <summary>The x-axis of the box.</summary>
        public readonly float3 Right
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Frame.Right;
        }
        
        /// <summary>The y-axis of the box.</summary>
        public readonly float3 Up
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Frame.Up;
        }
        
        /// <summary>The z-axis of the box.</summary>
        public readonly float3 Forward
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Frame.Forward;
        }
        
        /// <summary>The diagonal vector of the box.</summary>
        public readonly float3 Diagonal
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Frame.PointAt(Extents) - Frame.PointAt(-Extents);
        }
        
        /// <summary>The radius of the box.</summary>
        public readonly float BoundingRadius
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => length(Diagonal) * 0.5f;
        }
        
        /// <summary>The volume of the box.</summary>
        public readonly float Volume
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => max(Extents.x * Extents.y * Extents.z, 0.0f) * 8f;
        }
        
        /// <summary>The surface area of the box.</summary>
        public readonly float SurfaceArea
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => max(Extents.x * Extents.y + Extents.x * Extents.z + Extents.y * Extents.z, 0.0f) * 8f;
        }
        
        /// <summary>Returns the box as an axis-aligned box.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly AxisAlignedBox ToAxisAlignedBox()
        {
            float3 min = float.MaxValue;
            float3 max = float.MinValue;
            
            for (int i = 0; i < 8; i++)
            {
                float3 corner = GetCornerAt(i);
                min = math.min(min, corner);
                max = math.max(max, corner);
            }
            
            return new AxisAlignedBox(min, max);
        }
        
        /// <summary>Transforms the box by a matrix.</summary>
        /// <param name="matrix">The matrix to transform the box by.</param>
        /// <returns>The transformed box.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly OrientedBox Transform(in float4x4 matrix)
        {
            // Transform the frame (position and orientation).
            LocalFrame transformedFrame = Frame.Transform(matrix);
            // Scale the extents by the scale of the matrix.
            float3 scaledExtents = Extents * matrix.Scale();
            // Create a new OrientedBox with the transformed frame and scaled extents.
            return new OrientedBox(transformedFrame, scaledExtents);
        }

        /// <summary>Transforms the box by a <see cref="LocalTransform"/> </summary>
        /// <param name="transform">The transform to transform the box by.</param>
        /// <returns>The transformed box.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly OrientedBox Transform(in LocalTransform transform)
        {
            // Transform the frame (position and orientation).
            LocalFrame transformedFrame = Frame.Transform(transform);
            // Scale the extents by the scale of the transform.
            float3 scaledExtents = Extents * abs(transform.Scale);
            // Create a new OrientedBox with the transformed frame and scaled extents.
            return new OrientedBox(transformedFrame, scaledExtents);
        }

        /// <summary>Checks if the box contains a point.</summary>
        /// <param name="point">The point to check.</param>
        /// <returns>True if the box contains the point, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(float3 point)
        {
            float3 localPoint = Frame.ToFramePoint(point);
            return all(abs(localPoint) <= Extents);
        }

        /// <summary>Checks if the oriented box overlaps a axis-aligned box.</summary>
        /// <param name="aabb">The axis-aligned box to check.</param>
        /// <returns>True if the oriented box overlaps the axis-aligned box, false otherwise.</returns>
        public readonly bool Overlaps(in AxisAlignedBox aabb)
        {
            // First perform fast AABB test
            if (!aabb.Overlaps(ToAxisAlignedBox()))  
                return false;
            
            // Perform complex OBB test
            
            Span<float3> oobCorners = stackalloc float3[8]; 
            ComputeCorners(oobCorners);
            
            Span<float3> aabbCorners = stackalloc float3[8]; 
            aabb.ComputeCorners(aabbCorners);
            
            // Cache the box axes
            float3x3 axes = default;
            axes[0] = Right;
            axes[1] = Up;
            axes[2] = Forward;
            
            for (int i = 0; i < 3; i++)
            {
                float2 aProj = ProjectCorners(oobCorners, axes[i]);
                float2 bProj = ProjectCorners(aabbCorners, axes[i]);

                // Separating axis found, no overlap
                if (aProj.y < bProj.x || bProj.y < aProj.x)
                    return false;
            }

            // No separating axis found, boxes overlap
            return true;
        }

        static float2 ProjectCorners(in Span<float3> corners, float3 axis)
        {
            float min = dot(axis, corners[0]);
            float max = min;
            
            for (int i = 1; i < 8; i++)
            {
                float proj = dot(axis, corners[i]);
                if      (proj < min) min = proj;
                else if (proj > max) max = proj;
            }

            return new float2(min, max);
        }

        /// <summary>Checks if a plane overlaps the box.</summary>
        /// <param name="plane">The plane to check.</param>
        /// <returns>True if the plane overlaps the box, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Overlaps(Plane plane) => Overlaps(plane, Right, Up, Forward);

        /// <summary>Checks if a plane overlaps the box.</summary>
        /// <param name="plane">The plane to check.</param>
        /// <param name="right">The right vector of the box.</param>
        /// <param name="up">The up vector of the box.</param>
        /// <param name="forward">The forward vector of the box.</param>
        /// <returns>True if the plane overlaps the box, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Overlaps(Plane plane, float3 right, float3 up, float3 forward)
        {
            // Max projection of the half-diagonal onto the normal (always positive).
            float maxHalfDiagProj = Extents.x * abs(dot(plane.Normal, right)) + 
                                    Extents.y * abs(dot(plane.Normal, up)) + 
                                    Extents.z * abs(dot(plane.Normal, forward));

            // Positive distance -> center in front of the plane.
            // Negative distance -> center behind the plane (outside).
            float centerToPlaneDist = dot(plane.Normal, Center) + plane.Distance;

            // outside = maxHalfDiagProj < -centerToPlaneDist
            // outside = maxHalfDiagProj + centerToPlaneDist < 0
            // overlap = overlap && !outside
            return (maxHalfDiagProj + centerToPlaneDist >= 0);
        }
        
        /// <summary>Checks if a frustum intersects with the box.</summary>
        /// <param name="frustumPlanes">The frustum to check.</param>
        /// <returns>The result of the intersection test.</returns>
        public readonly unsafe FrustumIntersectResult Intersect(Span<Plane> frustumPlanes)
        {
            // First perform fast sphere test.
            if (FrustumUtility.IntersectSphere(frustumPlanes, Center, BoundingRadius) == FrustumIntersectResult.Outside)
                return FrustumIntersectResult.Outside;
            
            // Cache the box axes.
            float3 right = Right;
            float3 up = Up;
            float3 forward = Forward;
            
            // Test the OBB against frustum planes. Frustum planes are inward-facing.
            // The OBB is outside if it's entirely behind one of the frustum planes.
            // See "Real-Time Rendering", 3rd Edition, 16.10.2.
            if (!Overlaps(frustumPlanes[0], right, up, forward)) return FrustumIntersectResult.Outside;
            if (!Overlaps(frustumPlanes[1], right, up, forward)) return FrustumIntersectResult.Outside;
            if (!Overlaps(frustumPlanes[2], right, up, forward)) return FrustumIntersectResult.Outside;
            if (!Overlaps(frustumPlanes[3], right, up, forward)) return FrustumIntersectResult.Outside;
            if (!Overlaps(frustumPlanes[4], right, up, forward)) return FrustumIntersectResult.Outside;
            if (!Overlaps(frustumPlanes[5], right, up, forward)) return FrustumIntersectResult.Outside;

            // Test the frustum corners against OBB planes. The OBB planes are outward-facing.
            // The frustum is outside if all of its corners are entirely in front of one of the OBB planes.
            // See "Correct Frustum Culling" by Inigo Quilez.
            // We can exploit the symmetry of the box by only testing against 3 planes rather than 6.
            Span<Plane> planes = stackalloc Plane[3];
            planes[0].Normal = right;
            planes[0].Distance = Extents.x;
            planes[1].Normal = up;
            planes[1].Distance = Extents.y;
            planes[2].Normal = forward;
            planes[2].Distance = Extents.z;
            
            // Get the frustum corners.
            Span<float3> frustumCorners = stackalloc float3[8];
            FrustumUtility.ComputeCorners(frustumPlanes, frustumCorners);
            bool fullyInside = true; 

            for (int i = 0; i < 3; i++)
            {
                int cornersInside = 0; 

                for (int j = 0; j < 8; ++j)
                {
                    // Check if the corner is inside the plane.
                    if (dot(planes[i].Normal, frustumCorners[j] - Center) >= -planes[i].Distance)
                    {
                        cornersInside++;
                    }
                    else
                    {
                        break;
                    }
                }
                
                // If not all corners are inside this plane, the OBB cannot be fully inside the frustum.
                if (cornersInside != 8)
                {
                    fullyInside = false;
                    break; // Early out - no need to check other planes.
                }
            }
    
            return fullyInside ? FrustumIntersectResult.Inside : FrustumIntersectResult.Partial;
        }

        /// <summary>Corner point on the box identified by the given index.</summary>
        /// <remarks>Corners: [ (-x,-y), (x,-y), (x,y), (-x,y) ], -z, then +z</remarks>
        /// <param name="index">Index corner index in range 0-7</param>
        /// <returns>Corner point on the box identified by the given index.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float3 GetCornerAt(int index)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if ((uint)index >= 8)
                throw new IndexOutOfRangeException($"Index {index} is out of range (0, 7)");
#endif
            
            float x = (((index & 1) != 0) ^ ((index & 2) != 0)) ? Extents.x : -Extents.x;
            float y = ((index / 2) % 2 == 0) ? -Extents.y : Extents.y;
            float z = (index < 4) ? -Extents.z : Extents.z;
            return Frame.PointAt(x, y, z);
        }
        
        /// <summary>Gets the corners of the box.</summary>
        /// <param name="vertices">The span to store the corners in.</param>
        /// <exception cref="ArgumentException">Thrown when the span length is less than 8.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void ComputeCorners(Span<float3> vertices)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (vertices.Length < 8)
                throw new ArgumentException("Vertices must have a length of at least 8", nameof(vertices));
#endif
            
            (float x, float y, float z) = (Extents.x, Extents.y, Extents.z);
            vertices[0] = Frame.PointAt(-x, -y, -z);
            vertices[1] = Frame.PointAt( x, -y, -z);
            vertices[2] = Frame.PointAt( x,  y, -z);
            vertices[3] = Frame.PointAt(-x,  y, -z);
            vertices[4] = Frame.PointAt(-x, -y,  z);
            vertices[5] = Frame.PointAt( x, -y,  z);
            vertices[6] = Frame.PointAt( x,  y,  z);
            vertices[7] = Frame.PointAt(-x,  y,  z);
        }
        
        /// <summary>Calculates the squared distance from the box to a point.</summary>
        /// <param name="point">The point to check.</param>
        /// <returns>The squared distance from the box to a point.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float DistanceSquared(float3 point)
        {
            float3 localPoint = Frame.ToFramePoint(point);
            float3 closestPoint = abs(localPoint) - Extents;
            return lengthsq(max(0.0f, closestPoint));
        }
        
        /// <summary>Calculates the signed distance from the box to a point.</summary>
        /// <remarks>Positive if the point is outside the box, negative if inside.</remarks>
        /// <param name="point">The point to check.</param>
        /// <returns>The signed distance from the box to a point.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float SignedDistance(float3 point)
        {
            float3 localPoint = Frame.ToFramePoint(point);
            float3 closestPoint = abs(localPoint) - Extents;
            float maxComponent = cmax(closestPoint);
            return maxComponent < 0.0f ? maxComponent : length(max(0.0f, closestPoint));
        }
       
        /// <summary>Calculates the closest point on the box to a point.</summary>
        /// <param name="point">The point to check.</param>
        /// <returns>The closest point on the box to a point.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float3 ClosesPoint(float3 point)
        {
            float3 localPoint = Frame.ToFramePoint(point);
            float3 closestPoint = clamp(localPoint, -Extents, Extents);
            return Frame.FromFramePoint(closestPoint);
        }

        /// <summary>Returns a value that indicates whether the current <see cref="T:MA.Core.OrientedBox" /> is equal to another <see cref="T:MA.Core.OrientedBox" />.</summary>
        /// <param name="rhs">The other <see cref="T:MA.Core.OrientedBox" /> to compare against.</param>
        /// <returns><c>true</c> if the <paramref name="rhs" /> and current instance are equal; otherwise, <c>false</c>.</returns>
        public bool Equals(OrientedBox rhs) => Frame.Equals(rhs.Frame) && Extents.Equals(rhs.Extents);

        /// <summary>Returns a value that indicates whether the current <see cref="T:MA.Core.OrientedBox" /> is equal to a specified object.</summary>
        public override bool Equals(object o) => o is OrientedBox converted && Equals(converted);

        /// <summary>Returns a hash code for the current <see cref="T:MA.Core.OrientedBox" />.</summary>
        public override int GetHashCode() => unchecked((Frame.GetHashCode() * 397) ^ Extents.GetHashCode());

        /// <summary>Returns a string representation.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => $"OrientedBox(Frame={Frame}, Extents={Extents})";

        /// <summary>Returns a value that indicates whether the values of two <see cref="T:MA.Core.OrientedBox" /> objects are equal.</summary>
        /// <param name="lhs">The first value to compare.</param>
        /// <param name="rhs">The second value to compare.</param>
        /// <returns>true if the <paramref name="lhs" /> and <paramref name="rhs" /> parameters have the same value; otherwise, false.</returns>
        public static bool operator ==(OrientedBox lhs, OrientedBox rhs) => lhs.Equals(rhs);

        /// <summary>Returns a value that indicates whether two <see cref="T:MA.Core.OrientedBox" /> objects have different values.</summary>
        /// <param name="lhs">The first value to compare.</param>
        /// <param name="rhs">The second value to compare.</param>
        /// <returns>true if <paramref name="lhs" /> and <paramref name="rhs" /> are not equal; otherwise, false.</returns>
        public static bool operator !=(OrientedBox lhs, OrientedBox rhs) => !lhs.Equals(rhs);
    }
}