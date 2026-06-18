// Copyright © Magnetic Arcade. All Rights Reserved.

using System.Runtime.CompilerServices;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using float3 = Unity.Mathematics.float3;

namespace MA.Mathematics
{
    /// <summary>Vector math utilities.</summary>
    public static class VectorUtility
    {
        /// <summary>Returns the angle in degrees between vector a and b.</summary>
        /// <param name="a">The first vector</param>
        /// <param name="b">The second vector</param>
        /// <returns>The angle in degrees between vector a and b</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AngleDegrees(float2 a, float2 b) 
            => degrees(AngleRadians(a, b));

        /// <summary>Returns the angle in radians between vector a and b.</summary>
        /// <param name="a">The first vector</param>
        /// <param name="b">The second vector</param>
        /// <returns>The angle in radians between vector a and b</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AngleRadians(float2 a, float2 b)
        {
            float d = dot(a, b);
            float clampedDot = (d < -1f ? -1f : (d > 1f ? 1f : d));
            return acos(clampedDot);
        }
        
        /// <summary>Returns the angle in degrees between vector a and b.</summary>
        /// <param name="a">The first vector</param>
        /// <param name="b">The second vector</param>
        /// <returns>The angle in degrees between vector a and b</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AngleDegrees(float3 a, float3 b) 
            => degrees(AngleRadians(a, b));
        
        /// <summary>Returns the angle in radians between vector a and b.</summary>
        /// <param name="a">The first vector</param>
        /// <param name="b">The second vector</param>
        /// <returns>The angle in radians between vector a and b</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AngleRadians(float3 a, float3 b)
        {
            float d = dot(a, b);
            float clampedDot = (d < -1f ? -1f : (d > 1f ? 1f : d));
            return acos(clampedDot);
        }
        
        /// <summary>Increase or decreases the length of a vector by a given amount.</summary>
        /// <param name="vector">The vector to modify</param>
        /// <param name="size">The amount to increase or decrease the length of the vector</param>
        /// <returns>The vector with the new length</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 AddLength(float3 vector, float size)
        {
            // Get the vector length
            float magnitude = length(vector);
            // Calculate new vector length
            float newMagnitude = magnitude + size;
            // Calculate the ratio of the new length to the old length
            float scale = newMagnitude / magnitude;
            // Scale the vector
            vector *= scale;
            // Return the scaled vector
            return vector;
        }

        /// <summary>Create a vector of direction `vector` with length `newLength`.</summary>
        /// <param name="vector">The vector to modify</param>
        /// <param name="newLength">The new length of the vector</param>
        /// <returns>The vector with the new length</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 SetLength(float3 vector, float newLength) => normalizesafe(vector) * newLength;

        /// <summary>Clamp the length of a vector to a `maxLength`.</summary>
        /// <param name="vector">The vector to clamp</param>
        /// <param name="maxLength">The maximum length of the vector</param>
        /// <returns>The clamped vector</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 ClampLength(float3 vector, float maxLength)
        {
            float sqLength = lengthsq(vector);
            if (sqLength <= maxLength * maxLength)
                return vector;

            float length = sqrt(sqLength);
            return (vector / length) * maxLength;
        }
        
        /// <summary>Calculates two vectors perpendicular to input normal, as efficiently as possible.</summary>
        /// <param name="normal">The normal vector</param>
        /// <param name="outPerp1">The first perpendicular vector</param>
        /// <param name="outPerp2">The second perpendicular vector</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MakePerpendicularVectors(float3 normal, out float3 outPerp1, out float3 outPerp2)
        {
            // Duff et al method, from https://graphics.pixar.com/library/OrthonormalB/paper.pdf
            if (normal.z < 0f)
            {
                float a = 1f / (1f - normal.z);
                float b = normal.x * normal.y * a;
                outPerp1.x = 1f - normal.x * normal.x * a;
                outPerp1.y = -b;
                outPerp1.z = normal.x;
                outPerp2.x = b;
                outPerp2.y = normal.y * normal.y * a - 1f;
                outPerp2.z = -normal.y;
            }
            else
            {
                float a = 1f / (1f + normal.z);
                float b = -normal.x * normal.y * a;
                outPerp1.x = 1f - normal.x * normal.x * a;
                outPerp1.y = b;
                outPerp1.z = -normal.x;
                outPerp2.x = b;
                outPerp2.y = 1f - normal.y * normal.y * a;
                outPerp2.z = -normal.y;
            }
        }

        /// <summary>Normalizes the vector v if length is greater than epsilon, and returns the original length.</summary>
        /// <param name="v">The vector to normalize</param>
        /// <param name="tolerance">The tolerance</param>
        /// <returns>pre-normalized length of v</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float NormalizeLength(ref float2 v, float tolerance = 0)
        {
            float length = math.length(v);
            if (length > tolerance)
            {
                float invLength = 1f / length;
                v.x *= invLength;
                v.y *= invLength;
                return length;
            }
            v.x = v.y = 0;
            return 0;
        }

        /// <summary>Normalizes the vector v if length is greater than epsilon, and returns the original length.</summary>
        /// <param name="v">The vector to normalize</param>
        /// <param name="tolerance">The tolerance</param>
        /// <returns>pre-normalized length of v</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float NormalizeLength(ref float3 v, float tolerance = 0)
        {
            float length = math.length(v);
            if (length > tolerance)
            {
                float invLength = 1f / length;
                v.x *= invLength;
                v.y *= invLength;
                v.z *= invLength;
                return length;
            }
            v.x = v.y = v.z = 0;
            return 0;
        }

        /// <summary>Calculates angle between vFrom and vTo after projection onto plane with normal defined by planeN</summary>
        /// <param name="vFrom">The from vector</param>
        /// <param name="vTo">The to vector</param>
        /// <param name="planeN">The plane normal</param>
        /// <returns>The angle between vFrom and vTo after projection onto plane with normal defined by planeN</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float PlaneAngleSignedDegrees(float3 vFrom, float3 vTo, float3 planeN)
        {
            float3 from = normalizesafe(vFrom - dot(vFrom, planeN) * planeN);
            float3 to = normalizesafe(vTo - dot(vTo, planeN) * planeN);
            float3 c = cross(from, to);
            if (lengthsq(c) < MathConstants.ZeroTolerance)
            {
                // vectors are parallel
                return dot(from, to) < 0 ? 180f : 0f;
            }
            float sign = dot(c, planeN) < 0 ? -1f : 1f;
            return sign * AngleDegrees(from, to);
        }

        /// <summary>Calculates angle between vFrom and vTo after projection onto plane with normal defined by planeN</summary>
        /// <param name="a">The first point</param>
        /// <param name="b">The second point</param>
        /// <param name="c">The third point</param>
        /// <returns>Greater than 0 if C is to the left of the line from A to B, less than 0 if to the right, or 0 if on the line</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Orient(float2 a, float2 b, float2 c) 
            => dot((b - a), (c - a));

        /// <summary>Returns a right-perpendicular vector to v, ie v rotated 90 degrees clockwise.</summary>
        /// <param name="v">The vector to rotate</param>
        /// <returns>A right-perpendicular vector to v, ie v rotated 90 degrees clockwise</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 PerpendicularClockwise(float2 v) 
            => float2(v.y, -v.x);

        /// <summary>Calculates the normal of a triangle defined by v0, v1, and v2</summary>
        /// <param name="v0">The first vertex</param>
        /// <param name="v1">The second vertex</param>
        /// <param name="v2">The third vertex</param>
        /// <returns>A normalized vector that is perpendicular to triangle v0,v1,v2 (triangle normal)</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 Normal(float3 v0, float3 v1, float3 v2)
        {
            float3 edge1 = normalizesafe(v1 - v0);
            float3 edge2 = normalizesafe(v2 - v0);
            float3 cross = math.cross(edge1, edge2); // Reverse order if left-handed
            return normalizesafe(cross);
        }

        /// <summary>Calculates an un-normalized direction that is parallel to normal of triangle v0,v1,v2</summary>
        /// <param name="v0">The first vertex</param>
        /// <param name="v1">The second vertex</param>
        /// <param name="v2">The third vertex</param>
        /// <returns>An un-normalized direction that is parallel to normal of triangle v0,v1,v2</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 NormalDirection(float3 v0, float3 v1, float3 v2) 
            => cross((v1 - v0), (v2 - v0));

        /// <summary>Calculates the area of 3D triangle v0,v1,v2</summary>
        /// <param name="v0">The first vertex</param>
        /// <param name="v1">The second vertex</param>
        /// <param name="v2">The third vertex</param>
        /// <returns>The area of 3D triangle v0,v1,v2</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Area(float3 v0, float3 v1, float3 v2)
        {
            float3 edge1 = v1 - v0;
            float3 edge2 = v2 - v0;
            float3 cross = math.cross(edge1, edge2); // Reverse order if left-handed
            return 0.5f * length(cross);
        }

        /// <summary>Calculates the signed area of 2D triangle v0,v1,v2</summary>
        /// <param name="v0">The first vertex</param>
        /// <param name="v1">The second vertex</param>
        /// <param name="v2">The third vertex</param>
        /// <returns>The signed area of 2D triangle v0,v1,v2</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Area(float2 v0, float2 v1, float2 v2)
        {
            float2 edge1 = v1 - v0;
            float2 edge2 = v2 - v0;
            float crossZ = dot(edge1, edge2); // Reverse order if left-handed
            return 0.5f * abs(crossZ);
        }

        /// <summary>Calculates the signed area of 2D triangle v0,v1,v2</summary>
        /// <param name="v0">The first vertex</param>
        /// <param name="v1">The second vertex</param>
        /// <param name="v2">The third vertex</param>
        /// <returns>The signed area of 2D triangle v0,v1,v2</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SignedArea(float2 v0, float2 v1, float2 v2) 
            => 0.5f * ((v0.x * v1.y - v0.y * v1.x) + (v1.x * v2.y - v1.y * v2.x) + (v2.x * v0.y - v2.y * v0.x));

        /// <summary>Calculates the minimum component index of a vector.</summary>
        /// <param name="v">The vector to check</param>
        /// <returns>The index of the minimum component of a vector</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOfMinComponent(float2 v)
            => v.x < v.y ? 0 : 1;
        
        /// <summary>Calculates the minimum component index of a vector.</summary>
        /// <param name="v">The vector to check</param>
        /// <returns>The index of the minimum component of a vector</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOfMinComponent(float3 v)
            => v.x < v.y ? ((v.x < v.z) ? 0 : 2) : ((v.y < v.z) ? 1 : 2);
        
        /// <summary>Calculates the minimum component index of a vector.</summary>
        /// <param name="v">The vector to check</param>
        /// <returns>The index of the minimum component of a vector</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOfMinComponent(float4 v) 
            => cmax(select(new int4(0, 1, 2, 3), new int4(-1), cmin(v) < v));
        
        /// <summary>Calculates the maximum component index of a vector.</summary>
        /// <param name="v">The vector to check</param>
        /// <returns>The index of the maximum component of a vector</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOfMaxComponent(float2 v) 
            => v.x > v.y ? 0 : 1;
        
        /// <summary>Calculates the maximum component index of a vector.</summary>
        /// <param name="v">The vector to check</param>
        /// <returns>The index of the maximum component of a vector</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOfMaxComponent(float3 v)
            => v.x > v.y ? ((v.x > v.z) ? 0 : 2) : ((v.y > v.z) ? 1 : 2);
        
        /// <summary>Calculates the maximum component index of a vector.</summary>
        /// <param name="v">The vector to check</param>
        /// <returns>The index of the maximum component of a vector</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOfMaxComponent(float4 v)
            => cmax(select(new int4(0, 1, 2, 3), new int4(-1), cmax(v) > v));
    }
}
