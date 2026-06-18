// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace MA.Mathematics
{
    /// <summary>The result of the intersection.</summary>
    public enum FrustumIntersectResult : byte
    {
        /// <summary>The intersection failed, the test was completely outside the volume.</summary>
        Outside,
        /// <summary>The intersection was positive and completely inside the volume.</summary>
        Inside,
        /// <summary>The intersection was positive and partially inside the volume.</summary>
        Partial
    }

    /// <summary>Used for fast intersection tests with axis-aligned boxes.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct FrustumSIMDPacket
    {
        public float4 Nx;
        public float4 Ny;
        public float4 Nz;
        public float4 D;
        public float4 AbsNx;
        public float4 AbsNy;
        public float4 AbsNz;

        public FrustumSIMDPacket(ReadOnlySpan<Plane> planes, int offset, int limit)
        {
            Plane p0 = planes[math.min(offset + 0, limit)];
            Plane p1 = planes[math.min(offset + 1, limit)];
            Plane p2 = planes[math.min(offset + 2, limit)];
            Plane p3 = planes[math.min(offset + 3, limit)];
            Nx = new float4(p0.Normal.x, p1.Normal.x, p2.Normal.x, p3.Normal.x);
            Ny = new float4(p0.Normal.y, p1.Normal.y, p2.Normal.y, p3.Normal.y);
            Nz = new float4(p0.Normal.z, p1.Normal.z, p2.Normal.z, p3.Normal.z);
            D = new float4(p0.Distance, p1.Distance, p2.Distance, p3.Distance);
            AbsNx = math.abs(Nx);
            AbsNy = math.abs(Ny);
            AbsNz = math.abs(Nz);
        }
    }

    /// <summary>Utility for working with frustum planes.</summary>
    public static class FrustumUtility
    {
        // --- Initialization ---

        /// <summary>Creates a Frustum from a Camera.</summary>
        /// <param name="frustumPlanes">The output frustum planes.</param>
        /// <param name="camera">The camera to create the frustum from.</param>
        /// <param name="normalize">Whether to normalize the planes.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InitializeFromCamera(Span<Plane> frustumPlanes, Camera camera, bool normalize = true)
            => InitializeFromViewProjection(frustumPlanes, camera.projectionMatrix * camera.worldToCameraMatrix, normalize);

        /// <summary>Creates a new frustum from a view-projection matrix.</summary>
        /// <param name="frustumPlanes">The output frustum planes.</param>
        /// <param name="viewProjectionMatrix">The view-projection matrix to create the frustum from.</param>
        /// <param name="normalize">Whether to normalize the planes.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InitializeFromViewProjection(Span<Plane> frustumPlanes, in float4x4 viewProjectionMatrix, bool normalize = true)
        {
            float4x4 m = math.transpose(viewProjectionMatrix);

            float4 l = m.c3 + m.c0;
            float4 r = m.c3 - m.c0;
            float4 b = m.c3 + m.c1;
            float4 t = m.c3 - m.c1;
            float4 n = m.c3 + m.c2;
            float4 f = m.c3 - m.c2;

            if (normalize)
            {
                l *= 1.0f / math.length(l.xyz);
                r *= 1.0f / math.length(r.xyz);
                b *= 1.0f / math.length(b.xyz);
                t *= 1.0f / math.length(t.xyz);
                n *= 1.0f / math.length(n.xyz);
                f *= 1.0f / math.length(f.xyz);
            }

            frustumPlanes[0] = l;
            frustumPlanes[1] = r;
            frustumPlanes[2] = b;
            frustumPlanes[3] = t;
            frustumPlanes[4] = n;
            frustumPlanes[5] = f;
        }

        /// <summary>Calculates a view projection matrix from a frustum.</summary>
        /// <param name="frustumPlanes">The frustum planes.</param>
        /// <returns>The view projection matrix.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float4x4 ComputeViewProjection(ReadOnlySpan<Plane> frustumPlanes)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (frustumPlanes.Length != 6)
                throw new InvalidOperationException("Must have 6 planes to calculate view projection matrix.");
#endif

            // Extract the frustum's right, up, and forward vectors from the first three planes
            float3 right = -frustumPlanes[0].Normal;
            float3 up = frustumPlanes[1].Normal;
            float3 forward = -frustumPlanes[2].Normal;

            // Extract the frustum's position from the fourth plane
            float3 position = -frustumPlanes[3].Normal * frustumPlanes[3].Distance;

            // Construct the frustum's rotation matrix using the extracted vectors
            float4x4 rotation = default;
            rotation.c0 = math.float4(right, 0.0f);
            rotation.c1 = math.float4(up, 0.0f);
            rotation.c2 = math.float4(forward, 0.0f);

            // Extract the frustum's near and far clip distances from the fifth and sixth planes
            float near = -frustumPlanes[4].Distance;
            float far = frustumPlanes[5].Distance;

            // Construct the frustum's projection matrix using the extracted clip distances
            float4x4 projection = float4x4.OrthoOffCenter(-1, 1, -1, 1, near, far);

            // Multiply the rotation and projection matrices to get the final frustum matrix
            float4x4 frustumMatrix = math.mul(rotation, projection);

            // Translate the frustum matrix by the extracted position
            frustumMatrix.c3 = new float4(position.x, position.y, position.z, 1);

            return frustumMatrix;
        }

        /// <summary>Gets the corners of the frustum.</summary>
        /// <param name="planes">The frustum planes.</param>
        /// <param name="vertices">The output vertices.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ComputeCorners(ReadOnlySpan<Plane> planes, Span<float3> vertices)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (planes.Length != 6)
                throw new InvalidOperationException("Must have 6 planes to calculate corners.");
            if (vertices.Length != 8)
                throw new InvalidOperationException("Must have 8 vertices to calculate corners.");
#endif

            Plane.IntersectPlanes3(planes[0], planes[3], planes[4], out vertices[0]); // Near bottom left
            Plane.IntersectPlanes3(planes[1], planes[3], planes[4], out vertices[1]); // Near bottom right
            Plane.IntersectPlanes3(planes[0], planes[2], planes[4], out vertices[2]); // Near top left
            Plane.IntersectPlanes3(planes[1], planes[2], planes[4], out vertices[3]); // Near top right
            Plane.IntersectPlanes3(planes[0], planes[3], planes[5], out vertices[4]); // Far bottom left
            Plane.IntersectPlanes3(planes[1], planes[3], planes[5], out vertices[5]); // Far bottom right
            Plane.IntersectPlanes3(planes[0], planes[2], planes[5], out vertices[6]); // Far top left
            Plane.IntersectPlanes3(planes[1], planes[2], planes[5], out vertices[7]); // Far top right
        }

        /// <summary>Calculates 4 corners in world space from the given view projection matrix.</summary>
        /// <param name="invViewProjectionMatrix">The inverse view-projection matrix.</param>
        /// <param name="z">The plane distance.</param>
        /// <param name="vertices">A span of 4 float3s to store the corners in world space.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void ComputeCorners(in float4x4 invViewProjectionMatrix, float z, Span<float3> vertices)
        {
            Span<float3> clipSpaceFrustumCorners = stackalloc float3[4]
            {
                new float3(-1, -1, z),
                new float3( 1, -1, z),
                new float3(-1,  1, z),
                new float3( 1,  1, z),
            };

            for (int i = 0; i < 4; ++i)
            {
                float4 projected = math.mul(invViewProjectionMatrix, new float4(clipSpaceFrustumCorners[i], 1.0f));
                vertices[i] = projected.xyz * (1.0f / projected.w);
            }
        }

        /// <summary>Calculates the bounds of the given frustum planes.</summary>
        /// <param name="frustumPlanes">The frustum planes.</param>
        /// <returns>The bounds of the frustum.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AxisAlignedBox ComputeBounds(ReadOnlySpan<Plane> frustumPlanes)
        {
            Span<float3> corners = stackalloc float3[8];
            ComputeCorners(frustumPlanes, corners);

            AxisAlignedBox bounds = AxisAlignedBox.Empty;
            for (int i = 0; i < 8; ++i)
                bounds.Encapsulate(corners[i]);

            return bounds;
        }

        // --- Intersection ---

        /// <summary>Calculates the required packet count for a given number of planes.</summary>
        /// <param name="planeCount">The number of planes.</param>
        /// <returns>The plane packet count.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ComputeSIMDPacketCount(int planeCount) => (planeCount + 3) >> 2;

        /// <summary>Builds the packet planes for use with fast intersection tests.</summary>
        /// <param name="planes">The input planes.</param>
        /// <param name="packets">The output packets.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InitializeSIMDPackets(ReadOnlySpan<Plane> planes, Span<FrustumSIMDPacket> packets)
        {
            // for (int i = 0; i < packets.Length; i++)
            // {
            //     ref FrustumPlanePacket packet = ref packets[i];
            //     packet = new FrustumPlanePacket(planes, i * 4, planes.Length);
            // }
            // return;

            for (int i = 0; i < planes.Length; i++)
            {
                ref FrustumSIMDPacket packet = ref packets[i >> 2];
                int element = i & 3;
                packet.Nx[element] = planes[i].Normal.x;
                packet.Ny[element] = planes[i].Normal.y;
                packet.Nz[element] = planes[i].Normal.z;
                packet.D[element]  = planes[i].Distance;
                packet.AbsNx[element] = math.abs(packet.Nx[element]);
                packet.AbsNy[element] = math.abs(packet.Ny[element]);
                packet.AbsNz[element] = math.abs(packet.Nz[element]);
            }

            // Populate the remaining planes with values that are always "in"
            for (int i = planes.Length; i < 4 * packets.Length; ++i)
            {
                ref FrustumSIMDPacket packet = ref packets[i >> 2];
                int element = i & 3;
                packet.Nx[element] = 1.0f;
                packet.Ny[element] = 0.0f;
                packet.Nz[element] = 0.0f;
                // This value was before hardcoded to 32786.0f.
                // It was causing the culling system to discard the rendering of entities having a X coordinate approximately less than -32786.
                // We could not find anything relying on this number, so the value has been increased to 1 billion
                packet.D[element] = 1e9f;
                packet.AbsNx[element] = 1.0f;
                packet.AbsNy[element] = 0.0f;
                packet.AbsNz[element] = 0.0f;
            }
        }

        /// <summary>Intersection test with a sphere.</summary>
        /// <param name="planes">The frustum planes.</param>
        /// <param name="center">The center of the sphere.</param>
        /// <param name="radius">The radius of the sphere.</param>
        /// <returns>The result of the intersection test.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FrustumIntersectResult IntersectSphere(ReadOnlySpan<Plane> planes, float3 center, float radius)
        {
            int count = 0;

            for (int i = 0; i < planes.Length; i++)
            {
                float d = math.dot(planes[i].Normal, center) + planes[i].Distance;
                if (d < -radius)
                    return FrustumIntersectResult.Outside;

                if (d > radius)
                    count++;
            }

            return (count == planes.Length) ? FrustumIntersectResult.Inside : FrustumIntersectResult.Partial;
        }

        /// <summary>Intersection test with a sphere.</summary>
        /// <param name="planes">The frustum planes.</param>
        /// <param name="sphere">The sphere to test.</param>
        /// <returns>The result of the intersection test.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FrustumIntersectResult IntersectSphere(ReadOnlySpan<Plane> planes, in Sphere sphere) => IntersectSphere(planes, sphere.Center, sphere.Radius);

        /// <summary>Intersection test with a sphere.</summary>
        /// <param name="planes">The frustum planes.</param>
        /// <param name="sphere">The sphere to test.</param>
        /// <returns>The result of the intersection test.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FrustumIntersectResult IntersectSphere(ReadOnlySpan<Plane> planes, in BoundingSphere sphere) => IntersectSphere(planes, sphere.position, sphere.radius);

        /// <summary>Intersection test with an axis-aligned box.</summary>
        /// <param name="planes">The frustum planes.</param>
        /// <param name="center">The center of the box.</param>
        /// <param name="extents">The extents of the box.</param>
        /// <returns>The result of the intersection test.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FrustumIntersectResult IntersectBounds(ReadOnlySpan<Plane> planes, in float3 center, in float3 extents)
        {
            int count = 0;

            for (int i = 0; i < planes.Length; i++)
            {
                float3 normal = planes[i].Normal;
                float distance = math.dot(normal, center) + planes[i].Distance;
                float radius = math.dot(extents, math.abs(normal));
                if (distance + radius <= 0)
                    return FrustumIntersectResult.Outside;

                if (distance > radius)
                    count++;
            }

            return (count == planes.Length) ? FrustumIntersectResult.Inside : FrustumIntersectResult.Partial;
        }

        /// <summary>Intersection test with a <see cref="UnityEngine.Bounds"/></summary>
        /// <param name="planes">The frustum planes.</param>
        /// <param name="bounds">The bounds to test.</param>
        /// <returns>The result of the intersection test.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FrustumIntersectResult IntersectBounds(ReadOnlySpan<Plane> planes, in Bounds bounds) => IntersectBounds(planes, bounds.center, bounds.extents);

        /// <summary>Intersection test with an <see cref="AxisAlignedBox"/></summary>
        /// <param name="planes">The frustum planes.</param>
        /// <param name="bounds">The bounds to test.</param>
        /// <returns>The result of the intersection test.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FrustumIntersectResult IntersectBounds(ReadOnlySpan<Plane> planes, in AxisAlignedBox bounds) => IntersectBounds(planes, bounds.Center, bounds.Extents);

        /// <summary>SIMD optimized intersection test with a translated axis-aligned box using pre-calculated plane packets.</summary>
        /// <param name="packets">The frustum plane packets, calculated using <see cref="InitializeSIMDPackets"/>.</param>
        /// <param name="center">The center of the box.</param>
        /// <param name="extents">The extents of the box.</param>
        /// <returns>The result of the intersection test.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FrustumIntersectResult IntersectBoundsSIMD(ReadOnlySpan<FrustumSIMDPacket> packets, in float3 center, in float3 extents)
        {
            float4 cx = center.xxxx;
            float4 cy = center.yyyy;
            float4 cz = center.zzzz;

            float4 ex = extents.xxxx;
            float4 ey = extents.yyyy;
            float4 ez = extents.zzzz;

            int4 outCounts = 0;
            int4 inCounts = 0;

            for (int i = 0; i < packets.Length; i++)
            {
                FrustumSIMDPacket packet = packets[i];
                float4 distances = packet.Nx * cx + packet.Ny * cy + packet.Nz * cz + packet.D;
                float4 radii = packet.AbsNx * ex + packet.AbsNy * ey + packet.AbsNz * ez;

                inCounts += (int4)(distances >= radii);
                outCounts += (int4)(distances + radii < 0);
            }

            int inCount = math.csum(inCounts);
            int outCount = math.csum(outCounts);
            if (outCount != 0)
                return FrustumIntersectResult.Outside;
            else
                return (inCount == 4 * packets.Length) ? FrustumIntersectResult.Inside : FrustumIntersectResult.Partial;
        }

        /// <summary>SIMD optimized intersection test with a translated axis-aligned box using pre-calculated plane packets.</summary>
        /// <param name="packets">The frustum plane packets, calculated using <see cref="InitializeSIMDPackets"/>.</param>
        /// <param name="center">The center of the box.</param>
        /// <param name="extents">The extents of the box.</param>
        /// <returns>The result of the intersection test.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool OverlapsBoundsSIMD(ReadOnlySpan<FrustumSIMDPacket> packets, in float3 center, in float3 extents)
        {
            float4 cx = center.xxxx;
            float4 cy = center.yyyy;
            float4 cz = center.zzzz;

            float4 ex = extents.xxxx;
            float4 ey = extents.yyyy;
            float4 ez = extents.zzzz;

            bool4 isCulled = new bool4(false);

            for (int i = 0; i < packets.Length; i++)
            {
                FrustumSIMDPacket packet = packets[i];
                float4 distances = packet.Nx * cx + packet.Ny * cy + packet.Nz * cz + packet.D;
                float4 radii = packet.AbsNx * ex + packet.AbsNy * ey + packet.AbsNz * ez;
                isCulled |= (distances + radii < float4.zero);
            }

            return !math.any(isCulled);
        }

        /// <summary>SIMD optimized intersection test with a <see cref="UnityEngine.Bounds"/> using pre-calculated plane packets.</summary>
        /// <param name="packets">The frustum plane packets, calculated using <see cref="InitializeSIMDPackets"/>.</param>
        /// <param name="bounds">The bounds to test.</param>
        /// <returns>The result of the intersection test.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FrustumIntersectResult IntersectBoundsSIMD(ReadOnlySpan<FrustumSIMDPacket> packets, in Bounds bounds) => IntersectBoundsSIMD(packets, bounds.center, bounds.extents);

        /// <summary>SIMD optimized intersection test with a <see cref="AxisAlignedBox"/> using pre-calculated plane packets.</summary>
        /// <param name="packets">The frustum plane packets, calculated using <see cref="InitializeSIMDPackets"/>.</param>
        /// <param name="box">The axis-aligned box to test.</param>
        /// <returns>The result of the intersection test.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FrustumIntersectResult IntersectBoundsSIMD(ReadOnlySpan<FrustumSIMDPacket> packets, in AxisAlignedBox box) => IntersectBoundsSIMD(packets, box.Center, box.Extents);

        /// <summary>Intersects a triangle with a frustum using SIMD operations to determine if the triangle is inside, outside, or partially inside the frustum.</summary>
        /// <param name="planes">Span of planes representing the frustum.</param>
        /// <param name="packets">SIMD packets containing the plane coefficients optimized for SIMD operations.</param>
        /// <param name="a">First vertex of the triangle.</param>
        /// <param name="b">Second vertex of the triangle.</param>
        /// <param name="c">Third vertex of the triangle.</param>
        /// <returns>The intersection result as an enum (Inside, Outside, Partial).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FrustumIntersectResult IntersectTriangleSIMD(ReadOnlySpan<Plane> planes, ReadOnlySpan<FrustumSIMDPacket> packets, float3 a, float3 b, float3 c)
        {
            float4 ax = a.xxxx, ay = a.yyyy, az = a.zzzz;
            float4 bx = b.xxxx, by = b.yyyy, bz = b.zzzz;
            float4 cx = c.xxxx, cy = c.yyyy, cz = c.zzzz;
            bool aInside = true, bInside = true, cInside = true;

            for (int i = 0; i < packets.Length; i++)
            {
                FrustumSIMDPacket packet = packets[i];

                // Compute signed distances from the triangle vertices to the plane
                float4 da = packet.Nx * ax + packet.Ny * ay + packet.Nz * az + packet.D;
                float4 db = packet.Nx * bx + packet.Ny * by + packet.Nz * bz + packet.D;
                float4 dc = packet.Nx * cx + packet.Ny * cy + packet.Nz * cz + packet.D;

                // Determine if vertices are outside the plane
                bool4 aOutside = da < 0;
                bool4 bOutside = db < 0;
                bool4 cOutside = dc < 0;

                aInside &= !math.any(aOutside);
                bInside &= !math.any(bOutside);
                cInside &= !math.any(cOutside);

                // Early exit if all vertices are outside on the same plane
                if (math.all(aOutside & bOutside & cOutside))
                    return FrustumIntersectResult.Outside;
            }

            // If any vertex is inside, the triangle is at least partially inside
            if (aInside || bInside || cInside)
                return (aInside && bInside && cInside) ? FrustumIntersectResult.Inside : FrustumIntersectResult.Partial;

            // Maximum possible intersections considering a single triangle intersecting multiple frustum planes
            const int MaxIntersections = 15;

            Span<float3> vertices = stackalloc float3[MaxIntersections];
            Span<bool> inside = stackalloc bool[MaxIntersections];
            Span<float3> intersections = stackalloc float3[MaxIntersections];

            int verticesCount = 3;
            vertices[0] = a;
            vertices[1] = b;
            vertices[2] = c;

            for (int i = 0; i < planes.Length; i++)
            {
                Plane plane = planes[i];
                bool insidePlane = false;
                bool outsidePlane = false;

                for (int j = 0; j < verticesCount; j++)
                {
                    float d = math.dot(plane.Normal, vertices[j]) + plane.Distance;
                    if (d >= 0)
                    {
                        insidePlane = true;
                        inside[j] = true;
                    }
                    else
                    {
                        outsidePlane = true;
                        inside[j] = false;
                    }
                }

                if (!insidePlane)
                    // All vertices are outside this plane
                    return FrustumIntersectResult.Outside;

                if (!outsidePlane)
                    // All vertices are inside this plane, skip
                    continue;

                // Generate new vertices at the intersections of the edges crossing the plane
                int intersectionsCount = 0;
                int lastVertexIndex = verticesCount - 1;
                float3 lastVertex = vertices[lastVertexIndex];
                bool lastInside = inside[lastVertexIndex];

                for (int j = 0; j < verticesCount; j++)
                {
                    float3 vertex = vertices[j];
                    bool insideVertex = inside[j];

                    if (insideVertex != lastInside)
                    {
                        // Check if the edge crosses the plane
                        if (plane.FindLineIntersection(lastVertex, vertex - lastVertex, out float3 intersection))
                        {
                            intersections[intersectionsCount++] = intersection;
                        }
                        if (insideVertex)
                            intersections[intersectionsCount++] = vertex;
                    }
                    else if (insideVertex)
                    {
                        intersections[intersectionsCount++] = vertex;
                    }

                    lastVertex = vertex;
                    lastInside = insideVertex;
                }

                if (intersectionsCount == 0)
                    return FrustumIntersectResult.Outside;

                for (int k = 0; k < intersectionsCount; k++)
                {
                    inside[k] = false;
                }

                intersections[..intersectionsCount].CopyTo(vertices);
                verticesCount = intersectionsCount;
                intersections.Clear();
            }

            return FrustumIntersectResult.Partial;
        }

        // --- Utility ---

        /// <summary>Creates a the left frustum plane from a view-projection matrix.</summary>
        /// <param name="m">The view-projection matrix.</param>
        /// <returns>The left frustum plane.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Plane GetFrustumLeftPlane(this float4x4 m)
        {
            return MakeFrustumPlane(
                m.c0[3] + m.c0[0],
                m.c1[3] + m.c1[0],
                m.c1[3] + m.c1[0],
                m.c3[3] + m.c3[0]);
        }

        /// <summary>Creates a the right frustum plane from a view-projection matrix.</summary>
        /// <param name="m">The view-projection matrix.</param>
        /// <returns>The right frustum plane.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Plane GetFrustumRightPlane(this float4x4 m)
        {
            return MakeFrustumPlane(
                m.c0[3] - m.c0[0],
                m.c1[3] - m.c1[0],
                m.c1[3] - m.c1[0],
                m.c3[3] - m.c3[0]);
        }

        /// <summary>Creates a the bottom frustum plane from a view-projection matrix.</summary>
        /// <param name="m">The view-projection matrix.</param>
        /// <returns>The bottom frustum plane.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Plane GetFrustumBottomPlane(this float4x4 m)
        {
            return MakeFrustumPlane(
                m.c0[3] + m.c0[1],
                m.c1[3] + m.c1[1],
                m.c1[3] + m.c1[1],
                m.c3[3] + m.c3[1]);
        }

        /// <summary>Creates a the top frustum plane from a view-projection matrix.</summary>
        /// <param name="m">The view-projection matrix.</param>
        /// <returns>The top frustum plane.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Plane GetFrustumTopPlane(this float4x4 m)
        {
            return MakeFrustumPlane(
                m.c0[3] - m.c0[1],
                m.c1[3] - m.c1[1],
                m.c1[3] - m.c1[1],
                m.c3[3] - m.c3[1]);
        }

        /// <summary>Creates a the near frustum plane from a view-projection matrix.</summary>
        /// <param name="m">The view-projection matrix.</param>
        /// <returns>The near frustum plane.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Plane GetFrustumNearPlane(this float4x4 m)
        {
            return MakeFrustumPlane(
                m.c0[3] + m.c0[2],
                m.c1[3] + m.c1[2],
                m.c1[3] + m.c1[2],
                m.c3[3] + m.c3[2]);
        }

        /// <summary>Creates a the far frustum plane from a view-projection matrix.</summary>
        /// <param name="m">The view-projection matrix.</param>
        /// <returns>The far frustum plane.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Plane GetFrustumFarPlane(this float4x4 m)
        {
            return MakeFrustumPlane(
                m.c0[3] - m.c0[2],
                m.c1[3] - m.c1[2],
                m.c1[3] - m.c1[2],
                m.c3[3] - m.c3[2]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static Plane MakeFrustumPlane(float x, float y, float z, float w) => MakeFrustumPlane(new float4(x, y, z, w));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static Plane MakeFrustumPlane(in float4 plane)
        {
            float length = math.length(plane.xyz);
            if (length > MathConstants.ZeroTolerance)
                return plane * (1.0f / length);
            else
                return plane;
        }
    }
}
