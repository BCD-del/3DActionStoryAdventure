// Copyright © Magnetic Arcade. All Rights Reserved.
// ReSharper disable InconsistentNaming

using System.Runtime.CompilerServices;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using float3 = Unity.Mathematics.float3;
using quaternion = Unity.Mathematics.quaternion;

namespace MA.Mathematics
{
    /// <summary>A set of utility extensions for the Unity.Mathematics quaternion type.</summary>
    public static class QuaternionUtility
    {
        public static quaternion FromToRotation(float3 fromDirection, float3 toDirection)
        {
            float3 cross = math.cross(fromDirection, toDirection);
            float dot = math.dot(fromDirection, toDirection);
            return new quaternion(cross.x, cross.y, cross.z, dot + length(fromDirection) * length(toDirection));
        }
        
        /// <summary>Returns a 3x3 matrix representation of the quaternion.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3x3 ToFloat3x3(this in quaternion q) => float3x3(q);

        /// <summary>Returns a 4x4 matrix representation of the quaternion.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float4x4 ToFloat4x4(this in quaternion q)
        {
            float3x3 rot = q.ToFloat3x3();
            return float4x4(
                new float4(rot.c0, 0),
                new float4(rot.c1, 0),
                new float4(rot.c2, 0),
                new float4(0, 0, 0, 1));
        }

        /// <summary>Calculates the rotational difference from A to B</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion Subtract(this in quaternion b, in quaternion a) 
            => mul(inverse(a), b);

        /// <summary>Adds rotation B to rotation A.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion Add(this in quaternion a, in quaternion b) 
            => mul(a, b);
        
        /// <summary>Calculates the euler angles of the quaternion in radians.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 Euler(this in quaternion q) 
            => new float3(GetPitch(q), GetYaw(q), GetRoll(q));
        
        /// <summary>Calculates the euler angles of the quaternion in radians.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 EulerDegrees(this in quaternion q) 
            => degrees(new float3(GetPitch(q), GetYaw(q), GetRoll(q)));

        /// <summary>Calculates the pitch angle of the quaternion in radians.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetPitch(this in quaternion q) 
            => atan2(2f*q.value.x*q.value.w - 2f*q.value.y*q.value.z, 1f - 2f*q.value.x*q.value.x - 2f*q.value.z*q.value.z);

        /// <summary>Calculates the yaw angle of the quaternion in radians.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetYaw(this in quaternion q)
            => atan2(2f* q.value.y*q.value.w - 2f*q.value.x*q.value.z, 1f - 2f*q.value.y*q.value.y - 2f*q.value.z*q.value.z);

        /// <summary>Calculates the roll angle of the quaternion in radians.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetRoll(this in quaternion q) 
            => asin(2.0f*q.value.x*q.value.y + 2.0f*q.value.z*q.value.w);
        
        /// <summary>Calculates the angle and axis of the quaternion.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ToAngleAxis(this in quaternion q, out float angle, out float3 axis)
        {
            angle = 2.0f * acos(q.value.w);
            axis = normalizesafe(q.value.xyz, forward());
        }

        /// <summary>Calculates the x axis represented by the quaternion.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 AxisX(this in quaternion q)
        {
            float twoY  = 2.0f * q.value.y; float twoZ  = 2.0f * q.value.z;
            float twoWy = twoY * q.value.w; float twoWz = twoZ * q.value.w;
            float twoXy = twoY * q.value.x; float twoXz = twoZ * q.value.x;
            float twoYy = twoY * q.value.y; float twoZz = twoZ * q.value.z;
            return float3(1.0f - (twoYy + twoZz), twoXy + twoWz, twoXz - twoWy);
        }

        /// <summary>Calculates the y axis represented by the quaternion.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 AxisY(this in quaternion q)
        {
            float twoX  = 2.0f * q.value.x; float twoY  = 2.0f * q.value.y; float twoZ  = 2.0f * q.value.z;
            float twoWx = twoX * q.value.w; float twoWz = twoZ * q.value.w; float twoXx = twoX * q.value.x;
            float twoXy = twoY * q.value.x; float twoYz = twoZ * q.value.y; float twoZz = twoZ * q.value.z;
            return float3(twoXy - twoWz, 1.0f - (twoXx + twoZz), twoYz + twoWx);
        }

        /// <summary>Calculates the z axis represented by the quaternion.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 AxisZ(this in quaternion q)
        {
            float twoX  = 2.0f * q.value.x; float twoY  = 2.0f * q.value.y; float twoZ  = 2.0f * q.value.z;
            float twoWx = twoX * q.value.w; float twoWy = twoY * q.value.w; float twoXx = twoX * q.value.x;
            float twoXz = twoZ * q.value.x; float twoYy = twoY * q.value.y; float twoYZ = twoZ * q.value.y;
            return float3(twoXz + twoWy, twoYZ - twoWx, 1.0f - (twoXx + twoYy));
        }

        /// <summary>Calculates each axis represented by the quaternion.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetAxes(this in quaternion q, out float3 xAxis, out float3 yAxis, out float3 zAxis)
        {
            float twoX  = 2.0f * q.value.x; float twoY  = 2.0f * q.value.y; float twoZ  = 2.0f * q.value.z;
            float twoWx = twoX * q.value.w; float twoWy = twoY * q.value.w; float twoWz = twoZ * q.value.w;
            float twoXx = twoX * q.value.x; float twoXy = twoY * q.value.x; float twoXz = twoZ * q.value.x;
            float twoYy = twoY * q.value.y; float twoYZ = twoZ * q.value.y; float twoZz = twoZ * q.value.z;
            xAxis = float3(1.0f - (twoYy + twoZz), twoXy + twoWz, twoXz - twoWy);
            yAxis = float3(twoXy - twoWz, 1.0f - (twoXx + twoZz), twoYZ + twoWx);
            zAxis = float3(twoXz + twoWy, twoYZ - twoWx, 1.0f - (twoXx + twoYy));
        }
    }
}
