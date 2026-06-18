// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Animations;
using static Unity.Mathematics.math;
using float3 = Unity.Mathematics.float3;
using float3x3 = Unity.Mathematics.float3x3;
using quaternion = Unity.Mathematics.quaternion;

namespace MA.Mathematics
{
	/// <summary>Implements a container for euler rotation.</summary>
	/// <remarks>All rotation values are stored in degrees.</remarks>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct EulerRotation : IEquatable<EulerRotation>
    {
        /// <summary>Zero value.</summary>
        public static readonly EulerRotation Zero = new EulerRotation(0, 0, 0);

        /// <summary>Rotation around the right axis (around X axis), Looking up and down (0=Straight Ahead, +Up, -Down)</summary>
        public float Pitch;
        /// <summary>Rotation around the up axis (around Y axis), Running in circles 0=East, +North, -South.</summary>
        public float Yaw;
        /// <summary>Rotation around the forward axis (around Z axis), Tilting your head, 0=Straight, +Clockwise, -CCW.</summary>
        public float Roll;

        /// <summary>Constructs a new rotation given the pitch, yaw and roll.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EulerRotation(float pitch, float yaw, float roll)
        {
            Pitch = pitch;
            Yaw   = yaw;
            Roll  = roll;
        }

        /// <summary>Constructs a new rotation given a vector containing the pitch, yaw and roll values.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EulerRotation(float3 euler)
            : this(euler.x, euler.y, euler.z)
        {
        }

        /// <summary>Convert a vector of floating-point Euler angles (in degrees) into a EulerRotation.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EulerRotation EulerDegrees(float3 eulerDegrees) => new EulerRotation(eulerDegrees);

        /// <summary>Convert a vector of floating-point Euler angles (in radians) into a EulerRotation.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EulerRotation EulerRadians(float3 eulerRadians) => new EulerRotation(degrees(eulerRadians));

        /// <summary>Returns a new EulerRotation with its orientation corresponding to the direction in which the normal points.</summary>
        /// <note>Sets Yaw and Pitch to the proper numbers, and sets Roll to zero because the roll can't be determined from a normal.</note>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EulerRotation FromNormal(float3 normal)
        {
            EulerRotation eulerRotation;
            eulerRotation.Pitch = degrees(atan2(-normal.y, sqrt(normal.x * normal.x + normal.z * normal.z)));
            eulerRotation.Yaw   = degrees(-atan2(-normal.x, normal.z));
            eulerRotation.Roll  = 0;
            return eulerRotation;
        }

        /// <summary>Returns a new EulerRotation from a 3x3 rotation matrix.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EulerRotation FromRotationMatrix(float3x3 m)
        {
            m.GetScaledAxes(out float3 xAxis, out float3 yAxis, out float3 zAxis);

            EulerRotation newEulerRotation = new EulerRotation(
                degrees(atan2(zAxis.z, sqrt(lengthsq(zAxis.x) + lengthsq(zAxis.y)))), 
                degrees(atan2(zAxis.y, zAxis.x)), 0);

            float3 sxAxis = newEulerRotation.ToRotationMatrix().GetScaledAxis(Axis.X);
            newEulerRotation.Roll = degrees(atan2(dot(yAxis, sxAxis), dot(xAxis, sxAxis)));
            return newEulerRotation;
        }

        /// <summary>Converts a quaternion into a rotator.</summary>
        public static EulerRotation FromQuaternion(in quaternion q)
        {
            float x = q.value.x;
            float y = q.value.y;
            float z = q.value.z;
            float w = q.value.w;
            float xx = x * x;
            float yy = y * y;
            float zz = z * z;
            float singularityTest = y * z - w * x;
            float yawY = 2.0f * (w * y + z * x);
            float yawX = (1.0f - 2.0f * (xx+ yy));

            // reference
            // http://en.wikipedia.org/wiki/Conversion_between_quaternions_and_Euler_angles
            // http://www.euclideanspace.com/maths/geometry/rotations/conversions/quaternionToEuler/

            // this value was found from experience, the above websites recommend different values
            // but that isn't the case for us, so I went through different testing, and finally found the case
            // where both of world lives happily.
            const float singularityThreshold = 0.4999995f;
            float pitch, yaw, roll;

            switch (singularityTest)
            {
                case < -singularityThreshold:
                    pitch = -90.0f;
                    yaw   = degrees(atan2(yawY, yawX));
                    roll  = NormalizeAxis(-yaw - (2.0f * degrees(atan2(z, w))));
                    break;
                case > singularityThreshold:
                    pitch = 90.0f;
                    yaw   = degrees(atan2(yawY, yawX));
                    roll  = NormalizeAxis(yaw - (2.0f * degrees(atan2(z, w))));
                    break;
                default:
                    pitch = degrees(-asin(2.0f * singularityTest));
                    yaw   = degrees(atan2(yawY, yawX));
                    roll  = degrees(atan2(2.0f * (w*z + x*y), (1.0f - 2.0f * (zz + xx))));
                    break;
            }

            return new EulerRotation(pitch, yaw, roll);
        }

        /// <summary>Checks whether rotator is nearly zero within specified tolerance, when treated as an orientation.
        /// This means that Rotator3(0, 0, 360) is "zero", because it is the same final orientation as the zero rotator.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool IsNearlyZero(float tolerance = MathConstants.ZeroTolerance)
        {
            return abs(NormalizeAxis(Pitch)) <= tolerance &&
                   abs(NormalizeAxis(Yaw))   <= tolerance &&
                   abs(NormalizeAxis(Roll))  <= tolerance;
        }

        /// <summary>Checks whether this has exactly zero rotation, when treated as an orientation.
        /// This means that Rotator3(0, 0, 360) is "zero", because it is the same final orientation as the zero rotator.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool IsZero() => ClampAxis(Pitch) == 0.0f && ClampAxis(Yaw) == 0.0f && ClampAxis(Roll) == 0.0f;

        /// <summary>Adds to each component of the rotator.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(float deltaPitch, float deltaYaw, float deltaRoll)
        {
            Yaw   += deltaYaw;
            Pitch += deltaPitch;
            Roll  += deltaRoll;
        }

        /// <summary>Returns the inverse of the rotator.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly EulerRotation Inversed() => FromQuaternion(inverse(ToQuaternion()));

        /// <summary>Get the rotation, snapped to specified degree segments.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly EulerRotation GridSnap(in EulerRotation rotationGrid)
        {
            return new EulerRotation(
                MathUtility.GridSnap(Pitch, rotationGrid.Pitch),
                MathUtility.GridSnap(Yaw,   rotationGrid.Yaw),
                MathUtility.GridSnap(Roll,  rotationGrid.Roll));
        }

        /// <summary>Convert a rotation into a unit vector facing in its direction.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float3 ToUnitVector()
        {
	        // Remove winding and clamp to [-360, 360]
            float pitchNoWinding = fmod(Pitch, 360.0f);
            float yawNoWinding   = fmod(Yaw, 360.0f);

            sincos(radians(pitchNoWinding) * 0.5f, out float sinP, out float cosP);
            sincos(radians(yawNoWinding)   * 0.5f, out float sinY, out float cosY);

	        return float3(sinP, cosP * cosY, cosP * sinY);
        }

        /// <summary>Get Rotation as a quaternion.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly quaternion ToQuaternion()
        {
            float p = fmod(Pitch, 360.0f);
            float y = fmod(Yaw,   360.0f);
            float r = fmod(Roll,  360.0f);
            return quaternion.Euler(radians(float3(p, y, r)));
        }

        /// <summary>Get Rotation as a rotation matrix.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float3x3 ToRotationMatrix() => float3x3.EulerXYZ(radians(AsFloat3()));

        /// <summary>Convert a EulerRotation into floating-point Euler angles (in degrees). EulerRotation now stored in degrees.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float3 AsFloat3() => float3(Pitch, Yaw, Roll);

        /// <summary>Rotate a vector rotated by this rotator.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float3 RotateVector(float3 v) => mul(ToRotationMatrix(), v);

        /// <summary>Returns the vector rotated by the inverse of this rotator.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float3 UnrotateVector(float3 v) => mul(transpose(ToRotationMatrix()), v);

        /// <summary>Gets the rotation values so they fall within the range [0,360]</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly EulerRotation Clamped() => new EulerRotation(ClampAxis(Pitch), ClampAxis(Yaw), ClampAxis(Roll));

        /// <summary>In-place normalize, removes all winding and creates the "shortest route" rotation.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Normalize()
        {
            Pitch = NormalizeAxis(Pitch);
            Yaw   = NormalizeAxis(Yaw);
            Roll  = NormalizeAxis(Roll);
        }

        /// <summary>Create a copy of this rotator and normalize, removes all winding and creates the "shortest route" rotation.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly EulerRotation Normalized()
        {
            EulerRotation rot = this;
            rot.Normalize();
            return rot;
        }

        /// <summary>Create a copy of this rotator and denormalize, clamping each axis to 0 - 360.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly EulerRotation Denormalized()
        {
            EulerRotation rot = this;
            rot.Pitch = ClampAxis(rot.Pitch);
            rot.Yaw	  = ClampAxis(rot.Yaw);
            rot.Roll  = ClampAxis(rot.Roll);
            return rot;
        }

        /// <summary>Returns the float element at a specified index.</summary>
        public unsafe ref float this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                if ((uint)index >= 3)
                    throw new ArgumentException("index must be between[0...2]");
#endif
                fixed (float* array = &Pitch) { return ref array[index]; }
            }
        }

        /// <summary>Get or set a specific component of the vector, given a specific axis by enum</summary>
        public float this[Axis axis]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get
            {
                return axis switch
                {
                    Axis.X => Roll,
                    Axis.Y => Pitch,
                    Axis.Z => Yaw,
                    _ => 0.0f
                };
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                switch (axis)
                {
                    case Axis.X:
                        Roll = value;
                        break;
                    case Axis.Y:
                        Pitch = value;
                        break;
                    case Axis.Z:
                        Yaw = value;
                        break;
                }
            }
        }

        /// <summary>Utility to check if there are any non-finite values (NaN or Inf) in this EulerRotation.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool ContainsNaN() => (isnan(Pitch) || isnan(Yaw) || isnan(Roll));

        /// <summary>Utility lerp between two rotators.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EulerRotation Lerp(in EulerRotation a, in EulerRotation b, float alpha) => a + (b - a).Normalized() * alpha;

        /// <summary>Similar to Lerp, but does not take the shortest path. Allows interpolation over more than 180 degrees.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EulerRotation LerpRange(in EulerRotation a, in EulerRotation b, float alpha) => (a * (1.0f - alpha) + b * alpha).Normalized();

        /// <summary>Clamps an angle to the range of [0, 360).</summary>
        /// <remarks>Input will be treated as in the range [0, 360) and output will be in the range [0, 360).</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ClampAxis(float angle)
        {
            angle %= 360.0f;
            if (angle < 0.0f)
                angle += 360.0f;

            return angle;
        }

        /// <summary>Clamps an angle to the range of (-180, 180].</summary>
        /// <remarks>Input will be treated as in the range [0, 360) and output will be in the range (-180, 180].</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float NormalizeAxis(float angle)
        {
            angle = ClampAxis(angle);
            if (angle > 180.0f)
                angle -= 360.0f;

            return angle;
        }

        /// <summary>Returns a string representation.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => $"EulerRotation({Pitch}, {Yaw}, {Roll})";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator EulerRotation(float f) => new EulerRotation(f, f, f);

        /// <summary>Returns the result of a component-wise addition operation on two rotators.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EulerRotation operator +(in EulerRotation lhs, in EulerRotation rhs) => new EulerRotation(lhs.Pitch + rhs.Pitch, lhs.Yaw + rhs.Yaw, lhs.Roll + rhs.Roll);

        /// <summary>Returns the result of a component-wise subtraction operation on two rotators.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EulerRotation operator -(in EulerRotation lhs, in EulerRotation rhs) => new EulerRotation(lhs.Pitch - rhs.Pitch, lhs.Yaw - rhs.Yaw, lhs.Roll - rhs.Roll);

        /// <summary>Returns the result of a component-wise scale operation.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EulerRotation operator *(in EulerRotation lhs, float scale) => new EulerRotation(lhs.Pitch * scale, lhs.Yaw * scale, lhs.Roll * scale);

        /// <summary>Checks whether two rotators are equal within specified tolerance, when treated as an orientation.</summary>
        /// <remarks>This means that EulerRotation(0, 0, 360).Equals(EulerRotation(0,0,0)) is true, because they represent the same final orientation.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(in EulerRotation rhs, float tolerance)
        {
            return (abs(NormalizeAxis(Pitch - rhs.Pitch)) <= tolerance) &&
                   (abs(NormalizeAxis(Yaw   - rhs.Yaw))   <= tolerance) &&
                   (abs(NormalizeAxis(Roll  - rhs.Roll))  <= tolerance);
        }

        /// <summary>Returns true if this rotator is equal to another rotator.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(EulerRotation rhs) => Pitch == rhs.Pitch && Yaw == rhs.Yaw && Roll == rhs.Roll;

        /// <summary>Returns true if this rotator is equal to another object.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override bool Equals(object o) => o is EulerRotation converted && Equals(converted);

        /// <summary>Returns the hash code for this rotator.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override int GetHashCode()
        {
	        unchecked
	        {
		        int hash = 17;
		        hash = hash * 31 + Pitch.GetHashCode();
		        hash = hash * 31 + Yaw.GetHashCode();
		        hash = hash * 31 + Roll.GetHashCode();
		        return hash;
	        }
        }

        /// <summary>Returns the result of a component-wise equality operation on two rotators.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in EulerRotation lhs, in EulerRotation rhs) => lhs.Equals(rhs);

        /// <summary>Returns the result of a component-wise not equal operation on two rotators.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in EulerRotation lhs, in EulerRotation rhs) => !lhs.Equals(rhs);
    }

    public static class RotatorHelpers
    {
        /// <summary>Shortcut <see cref="EulerRotation.FromNormal"/></summary>
        public static EulerRotation ToOrientationRotation(this in float3 v) => EulerRotation.FromNormal(v);

        /// <summary>Shortcut <see cref="EulerRotation.FromNormal"/></summary>
        public static EulerRotation ToOrientationRotation(this in Vector3 v) => EulerRotation.FromNormal(v);

        /// <summary>Shortcut <see cref="EulerRotation.FromQuaternion"/></summary>
        public static EulerRotation ToRotator(this in quaternion q) => EulerRotation.FromQuaternion(q);

        /// <summary>Shortcut <see cref="EulerRotation.FromQuaternion"/></summary>
        public static EulerRotation ToRotator(this in Quaternion q) => EulerRotation.FromQuaternion(q);
    }
}
