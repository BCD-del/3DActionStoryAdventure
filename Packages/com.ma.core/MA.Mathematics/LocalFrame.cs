// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine.Assertions;
using static Unity.Mathematics.math;
using float3 = Unity.Mathematics.float3;
using quaternion = Unity.Mathematics.quaternion;

namespace MA.Mathematics
{
	/// <summary>Represents a 3D coordinate frame, consisting of a position and rotation.</summary>
	/// <remarks>The representation is the same as a <see cref="LocalTransform"/>, except that a <see cref="LocalFrame"/> has no scale.</remarks>
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
    public struct LocalFrame : IEquatable<LocalFrame>
    {
	    /// <summary>Origin of the frame.</summary>
        public float3 Position;
        /// <summary>Rotation of the frame. Think of this as the rotation of the unit X/Y/Z axes to the 3D frame axes.</summary>
        public quaternion Rotation;
        
        /// <summary>X axis of frame (axis 0)</summary>
        public readonly float3 Right
        {
	        [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Rotation.AxisX();
        }
        /// <summary>Y axis of frame (axis 1)</summary>
        public readonly float3 Up
        {
	        [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Rotation.AxisY();
        }
        /// <summary>Z axis of frame (axis 2)</summary>
        public readonly float3 Forward
        {
	        [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Rotation.AxisZ();
        }

        /// <summary>Construct a frame from the given position and quaternion rotation.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LocalFrame(float3 position, in quaternion rotation)
        {
	        Position = position;
	        Rotation = rotation;
        }

	    /// <summary>Construct a frame at the given origin aligned to the unit axes.</summary>
	    [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LocalFrame(float3 position) : this(position, quaternion.identity) { }
	    
        /// <summary>Construct a frame with the forward axis aligned to a target axis.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
	    public LocalFrame(float3 position, float3 forward) : this(position, quaternion.LookRotationSafe(up(), forward)) { }
        
        /// <summary>Construct a frame from X/Y/Z axis vectors. Vectors must be mutually orthogonal.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LocalFrame(float3 position, float3 x, in float3 y, in float3 z) : this(position, new quaternion(float3x3(x, y, z))) { }

	    /// <summary>AxisIndex, index of axis of frame, either 0, 1, or 2.</summary>
	    [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float3 GetAxis(int axisIndex)
        {
	        switch (axisIndex)
	        {
		        case 0:
			        return Rotation.AxisX();
		        case 1:
			        return Rotation.AxisY();
		        case 2:
			        return Rotation.AxisZ();
		        default:
			        Assert.IsTrue(false, "Invalid axis index");
			        return 0;
	        }
        }

	    /// <summary>Conversion of this Frame to Transform.</summary>
	    [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly LocalTransform ToTransform() => new LocalTransform(Position, Rotation, 1);

	    /// <summary>Point at distances along the frame axes.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float3 PointAt(float x, float y, float z) => rotate(Rotation, float3(x, y, z)) + Position;

	    /// <summary>Point at distances along the frame axes.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float3 PointAt(in float3 point) => rotate(Rotation, float3(point.x, point.y, point.z)) + Position;

	    /// <summary>Point transformed into local coordinate system of the Frame.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float3 ToFramePoint(in float3 point) => rotate(inverse(Rotation), point - Position);

	    /// <summary>Point transformed from local coordinate system of the Frame into world coordinate system.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float3 FromFramePoint(in float3 point) => rotate(Rotation, point) + Position;

	    /// <summary>Vector transformed into local coordinate system the Frame.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float3 ToFrameVector(in float3 vector) => rotate(inverse(Rotation), vector);

	    /// <summary>Vector transformed from local coordinate system of Frame into world coordinate system.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float3 FromFrameVector(in float3 vector) => rotate(Rotation, vector);

	    /// <summary>Quaternion transformed into local coordinate system of the Frame.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly quaternion ToFrame(in quaternion quat) => mul(inverse(Rotation), quat);

	    /// <summary>Quaternion transformed from local coordinate system of Frame into world coordinate system.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly quaternion FromFrame(in quaternion quat) => mul(Rotation, quat);

	    /// <summary>Frame transformed into local coordinate system of this Frame.
        ///  </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly LocalFrame ToFrame(in LocalFrame localFrame) => new LocalFrame(ToFramePoint(localFrame.Position), ToFrame(localFrame.Rotation));

	    /// <summary>
        ///		Input Frame transformed from local coordinate system of this Frame into world coordinate system.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly LocalFrame FromFrame(in LocalFrame localFrame) => new LocalFrame(FromFramePoint(localFrame.Position), FromFrame(localFrame.Rotation));

	    /// <summary>Project a 3D point into plane and convert to UV coordinates in that plane.</summary>
	    /// <param name="position">The 3D point to project onto the plane.</param>
	    /// <param name="planeNormalAxis">Which plane to project onto, identified by perpendicular normal. Default is 2, ie normal is Z, plane is (X,Y).</param>
	    /// <returns>2D coordinates on a UV plane, relative to origin.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly float2 ToPlaneUV(in float3 position, int planeNormalAxis = 2)
		{
			int axis0 = 0, axis1 = 1;
			switch (planeNormalAxis)
			{
				case 0:
					axis0 = 2;
					break;
				case 1:
					axis1 = 2;
					break;
			}
			float3 localPos = position - Position;
			float u = dot(localPos, GetAxis(axis0));
			float v = dot(localPos, GetAxis(axis1));
			return float2(u, v);
		}
        /// <summary>Map a point from local UV plane coordinates to the corresponding 3D point in one of the planes of the frame.</summary>
	    /// <param name="localUV">The 2D coordinates in UV plane.</param>
	    /// <param name="planeNormalAxis">Which plane to map to, identified by perpendicular normal. Default is 2, ie normal is Z, plane is (X,Y).</param>
	    /// <returns>The 3D point on the plane, including the frame position.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly float3 FromPlaneUV(in float2 localUV, int planeNormalAxis = 2)
		{
			float3 planePos = float3(localUV[0], localUV[1], 0);
			switch (planeNormalAxis)
			{
				case 0:
					planePos[0] = 0; planePos[2] = localUV[0];
					break;
				case 1:
					planePos[1] = 0; planePos[2] = localUV[1];
					break;
			}
			return mul(Rotation, planePos) + Position;
		}

	    /// <summary>Project a point onto one of the planes of the frame.</summary>
	    /// <param name="position">The 3D point to project onto the plane.</param>
	    /// <param name="planeNormalAxis">PlaneNormalAxis which plane to project onto, identified by perpendicular normal. Default is 2, ie normal is Z, plane is (X,Y).</param>
	    /// <returns>The 3D point on the plane, including the frame position.</returns>
	    [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly float3 ToPlane(in float3 position, int planeNormalAxis = 2)
		{
			float3 normal = GetAxis(planeNormalAxis);
			float3 localVec = position - Position;
			float signedDist = dot(localVec, normal);
			return position - signedDist * normal;
		}

		/// <summary>Rotate this frame by given quaternion.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly LocalFrame Rotate(in quaternion q)
		{
			quaternion newRotation = mul(q, Rotation);
			return all(normalizesafe(newRotation).value > 0) ? new LocalFrame(Position, newRotation) : this;
		}

		/// <summary>Transform this frame by the given transform.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly LocalFrame Transform(in LocalTransform transform)
		{
			LocalFrame result = this;
			result.Position = transform.TransformPoint(Position);
			return result.Rotate(transform.Rotation);
		}

		/// <summary>Transform this frame by the given transform.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly LocalFrame Transform(in float4x4 matrix)
		{
			LocalFrame result = this;
			result.Position = transform(matrix, Position);
			return result.Rotate(matrix.Rotation());
		}

		/// <summary>Align an axis of this frame with a target direction.</summary>
		/// <param name="axisIndex">The axis to align.</param>
		/// <param name="toDirection">The target direction.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly LocalFrame AlignAxis(int axisIndex, in float3 toDirection)
		{
			quaternion relativeRotation = quaternion.LookRotationSafe(toDirection, GetAxis(axisIndex));
			return Rotate(relativeRotation);
		}

		/// <summary>Compute rotation around vector that best-aligns axis of frame with target direction.</summary>
		/// <param name="axisIndex">Which axis to align.</param>
		/// <param name="toDirection">The target direction.</param>
		/// <param name="aroundVector">The rotation is constrained to be around this vector (ie this direction in frame stays constant)</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly LocalFrame ConstrainedAlignAxis(int axisIndex, in float3 toDirection, in float3 aroundVector)
		{
			float3 axis = GetAxis(axisIndex);
			float angleDegrees = VectorUtility.PlaneAngleSignedDegrees(axis, toDirection, aroundVector);
			quaternion relativeRotation = quaternion.AxisAngle(aroundVector, radians(angleDegrees));
			return Rotate(relativeRotation);
		}

		/// <summary>Compute intersection of ray with plane defined by frame origin and axis as normal.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly bool RayPlaneIntersection(in float3 rayOrigin, in float3 rayDirection, int planeNormalAxis, out float3 hitPoint)
		{
			float3 normal = GetAxis(planeNormalAxis);
			float planeD = -dot(Position, normal);
			float normalDot = dot(rayDirection, normal);
			if (MathUtility.NearlyEquals(0, MathConstants.ZeroTolerance))
			{
				hitPoint = float3(float.MaxValue, float.MaxValue,float.MaxValue);
				return false;
			}
			float t = -(dot(rayOrigin, normal) + planeD) / normalDot;
			if (t < 0)
			{
				hitPoint = float3(float.MaxValue, float.MaxValue, float.MaxValue);
				return false;
			}
			hitPoint = rayOrigin + t * rayDirection;
			return true;
		}
		
		/// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly bool Equals(LocalFrame other) => this == other;
		
		/// <inheritdoc cref="object.Equals(object)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly override bool Equals(object o) => o is LocalFrame converted && Equals(converted);

		/// <summary>Returns a hash code for the Transform.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = hash * 31 + Position.GetHashCode();
				hash = hash * 31 + Rotation.GetHashCode();
				return hash;
			}
		}

		/// <summary>Returns a string representation.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override string ToString() => $"Frame(Position={Position}, Rotation={Rotation})";

		/// <summary>Returns the result of a component-wise equality operation on two Transforms.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(in LocalFrame lhs, in LocalFrame rhs) => all(lhs.Position == rhs.Position) && all(lhs.Rotation.value == rhs.Rotation.value);

		/// <summary>Returns the result of a component-wise not equal operation on two Transforms.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(in LocalFrame lhs, in LocalFrame rhs) => any(lhs.Position != rhs.Position) || any(lhs.Rotation.value != rhs.Rotation.value);
    }
	
	public static class LocalFrameHelpers
	{
        /// <summary>Get a Frame from a Transform, in local space.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LocalFrame GetLocalFrame(this UnityEngine.Transform transform) => new LocalFrame(transform.localPosition, transform.localRotation);

        /// <summary>Get a Frame from a Transform, in world space.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LocalFrame GetWorldFrame(this UnityEngine.Transform transform) => new LocalFrame(transform.position, transform.rotation);
	}
}
