// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;
using float3 = Unity.Mathematics.float3;
using float4x4 = Unity.Mathematics.float4x4;
using quaternion = Unity.Mathematics.quaternion;

namespace MA.Mathematics
{
    /// <summary>Position, rotation and scale of this entity, relative to the parent, or on world space, if no parent exists.</summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct LocalTransform : IEquatable<LocalTransform>
    {
        /// <summary>The identity transform.</summary>
        public static LocalTransform Identity = new LocalTransform(0, quaternion.identity, 1);

        /// <summary>The position of this transform.</summary>
        public float3 Position;
        /// <summary>Rotation of the transform.</summary>
        public quaternion Rotation;
        /// <summary>Scale of the transform.</summary>
        public float3 Scale;

        /// <summary>Returns the determinant of scale matrix.</summary>
        public readonly float Determinant
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Scale.x * Scale.y * Scale.z;
        }

        /// <summary>Constructs a translation, rotation and scaling transform.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LocalTransform(float3 position, in quaternion rotation, float3 scale)
        {
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }

        /// <summary>Constructs a transform from a 4x4 matrix.</summary>
        /// <remarks>Matrix must be a TRS matrix.</remarks>
        /// <param name="matrix">The matrix to construct the transform from.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LocalTransform FromMatrix(in float4x4 matrix)
        {
            quaternion rotation = matrix.Rotation();
            
            float determinant = math.determinant(matrix);
            if (determinant < 0) matrix[0].xyz = -matrix[0].xyz;

            float3 position = matrix.Translation();
            float3 scale = matrix.Scale();
            return FromPositionRotationScale(position, rotation, scale);
        }

        /// <summary>Returns a Transform initialized with the given position and rotation. Scale will be 1.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LocalTransform FromPositionRotation(float3 position, in quaternion rotation) 
            => new LocalTransform { Position = position, Scale = 1.0f, Rotation = rotation };

        /// <summary>Returns a Transform initialized with the given position and scale. Rotation will be identity.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LocalTransform FromPositionScale(float3 position, in float3 scale) 
            => new LocalTransform { Position = position, Scale = scale, Rotation = quaternion.identity };

        /// <summary>Returns a Transform initialized with the given position, rotation and scale.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LocalTransform FromPositionRotationScale(in float3 position, in quaternion rotation, in float3 scale) 
            => new LocalTransform { Position = position, Scale = scale, Rotation = rotation };

        /// <summary>Returns a Transform initialized with the given position. Rotation will be identity, and scale will be 1.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LocalTransform FromPosition(in float3 position) 
            => new LocalTransform { Position = position, Scale = 1.0f, Rotation = quaternion.identity };

        /// <summary>Returns a Transform initialized with the given position. Rotation will be identity, and scale will be 1.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LocalTransform FromPosition(float x, float y, float z) 
            => new LocalTransform { Position = new float3(x, y, z), Scale = 1.0f, Rotation = quaternion.identity };

        /// <summary>Returns a Transform initialized with the given rotation. Position will be 0,0,0, and scale will be 1.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LocalTransform FromRotation(in quaternion rotation)
            => new LocalTransform { Position = float3.zero, Scale = 1.0f, Rotation = rotation };

        /// <summary>Returns a Transform initialized with the given scale. Position will be 0,0,0, and rotation will be identity.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LocalTransform FromScale(in float3 scale) 
            => new LocalTransform { Position = float3.zero, Scale = scale, Rotation = quaternion.identity };

        /// <summary>Returns a transform constructed from a Unity Transform.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LocalTransform FromUnityTransform(UnityEngine.Transform unityTransform, UnityEngine.Space space)
        {
            return space == UnityEngine.Space.World
                ? FromPositionRotationScale(unityTransform.position, unityTransform.rotation, unityTransform.lossyScale)
                : FromPositionRotationScale(unityTransform.localPosition, unityTransform.localRotation, unityTransform.localScale);
        }

        /// <summary>Gets the right vector of unit length.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float3 Right() => TransformDirection(right());

        /// <summary>Gets the up vector of unit length.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float3 Up() => TransformDirection(up());

        /// <summary>Gets the forward vector of unit length.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float3 Forward() => TransformDirection(forward());

        /// <summary>Returns true if scale is nonuniform, within tolerance.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool HasNonUniformScale(float tolerance = MathConstants.ZeroTolerance)
        {
            return abs(Scale.x - Scale.y) > tolerance ||
                   abs(Scale.x - Scale.z) > tolerance ||
                   abs(Scale.y - Scale.z) > tolerance;
        }

        /// <summary>The maximum magnitude of all components of the 3D scale.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float GetMaximumAxisScale() => cmax(abs(Scale.xyz));

        /// <summary>Transforms a point by this transform.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float3 TransformPoint(in float3 point) => Position + rotate(Rotation, point) * Scale;

        /// <summary>Transforms a point by the inverse of this transform.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float3 InverseTransformPoint(in float3 point) => rotate(conjugate(Rotation), point - Position) * GetSafeScaleReciprocal(Scale);

        /// <summary>Transforms a direction by this transform.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float3 TransformDirection(in float3 direction) => rotate(Rotation, direction);

        /// <summary>Transforms a direction by the inverse of this transform.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float3 InverseTransformDirection(in float3 direction) => rotate(conjugate(Rotation), direction);

        /// <summary>Transforms a vector by this transform with scale applied.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float3 TransformVector(in float3 vector) => mul(Rotation, Scale * vector);

        /// <summary>Transforms a vector by the inverse of this transform with scale applied.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float3 InverseTransformVector(in float3 vector) => GetSafeScaleReciprocal(Scale) * mul(inverse(Rotation), vector);

        /// <summary>Surface Normals are special, their transform is Rotate( Normalize( (1/Scale) * Normal) ) ).
        /// However 1/Scale requires special handling in case any component is near-zero.</summary>
        /// <returns>input surface normal with transform applied.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float3 TransformNormal(in float3 normal)
        {
            // Transform normal by a safe inverse scale + normalize, and a standard rotation
            float3 s = Scale;
            float detSign = sign(s.x * s.y * s.z); // we only need to multiply by the sign of the determinant, rather than divide by it, since we normalize later anyway
            float3 safeInvS = float3(s.y * s.z * detSign, s.x * s.z * detSign, s.x * s.y * detSign);
            return TransformDirection(normalizesafe(safeInvS * normal));
        }

        /// <summary>Surface Normals are special, their inverse transform is InverseRotate( Normalize(Scale * Normal)) )</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float3 InverseTransformNormal(in float3 normal) => InverseTransformDirection(normalizesafe(Scale * normal));

        /// <summary>Transforms a rotation by this transform.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly quaternion TransformRotation(in quaternion rotation) => mul(Rotation, rotation);

        /// <summary>Transforms a rotation by the inverse of this transform.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly quaternion InverseTransformRotation(in quaternion rotation) => mul(conjugate(Rotation), rotation);

        /// <summary>Transforms a scale by this transform.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float3 TransformScale(in float3 scale) => scale * Scale;

        /// <summary>Transforms a scale by the inverse of this transform.</summary>
        /// <remarks>Throws if the <see cref="Scale"/> field is zero.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float3 InverseTransformScale(in float3 scale) => scale / Scale;

        /// <summary>Transforms a scale by the inverse of this transform.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float3 InverseTransformScaleSafe(in float3 scale) => scale * GetSafeScaleReciprocal(Scale);

        /// <summary>Returns an input ray with transformation applied.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly UnityEngine.Ray TransformRay(in UnityEngine.Ray ray)
        {
            float3 origin = TransformPoint(ray.origin);
            float3 direction = normalizesafe(TransformDirection(ray.direction));
            return new UnityEngine.Ray(origin, direction);
        }

        /// <summary>Returns an input ray with inverse transformation applied.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly UnityEngine.Ray InverseTransformRay(in UnityEngine.Ray ray)
        {
            float3 invOrigin = InverseTransformPoint(ray.origin);
            float3 invDirection = normalizesafe(InverseTransformDirection(ray.direction));
            return new UnityEngine.Ray(invOrigin, invDirection);
        }

        /// <summary>Transforms a Transform by this transform.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly LocalTransform Transform(in LocalTransform transform) => new LocalTransform
        {
            Position = TransformPoint(transform.Position),
            Scale = TransformScale(transform.Scale),
            Rotation = TransformRotation(transform.Rotation),
        };

        /// <summary>Transforms a <see cref="LocalTransform"/> by the inverse of this transform.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly LocalTransform InverseTransform(in LocalTransform transform) => new LocalTransform
        {
            Position = InverseTransformPoint(transform.Position),
            Scale = InverseTransformScale(transform.Scale),
            Rotation = InverseTransformRotation(transform.Rotation),
        };

        /// <summary>Gets the inverse of this transform.</summary>
        /// <remarks>This method will throw if the <see cref="Scale"/> field is zero.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly LocalTransform Inverse()
        {
            quaternion inverseRotation = conjugate(Rotation);
            float3 inverseScale = GetSafeScaleReciprocal(Scale);
            return new LocalTransform
            {
                Position = -rotate(inverseRotation, Position) * inverseScale,
                Scale = inverseScale,
                Rotation = inverseRotation,
            };
        }

        /// <summary>Gets the matrix equivalent of this transform.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float4x4 ToMatrix() => float4x4.TRS(Position, Rotation, Scale);

        /// <summary>Gets the matrix equivalent of this transform, without scale.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float4x4 ToMatrixNoScale() => new float4x4(Rotation, Position);

        /// <summary>Gets the matrix inverse of this transform.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float4x4 ToInverseMatrix() => Inverse().ToMatrix();

        /// <summary>Gets an identical transform with a new position value.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly LocalTransform WithPosition(in float3 position) 
            => new LocalTransform { Position = position, Scale = Scale, Rotation = Rotation };

        /// <summary>Creates a transform that is identical but with a new position value.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly LocalTransform WithPosition(float x, float y, float z) 
            => new LocalTransform { Position = new float3(x, y, z), Scale = Scale, Rotation = Rotation };

        /// <summary>Gets an identical transform with a new rotation value.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly LocalTransform WithRotation(in quaternion rotation)
            => new LocalTransform { Position = Position, Scale = Scale, Rotation = rotation };

        /// <summary>Gets an identical transform with a new scale value.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly LocalTransform WithScale(in float3 scale) 
            => new LocalTransform { Position = Position, Scale = scale, Rotation = Rotation };

        /// <summary>Translates this transform by the specified vector.</summary>
        /// <remarks>Note that this doesn't modify the original transform. Rather it returns a new one.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly LocalTransform Translate(in float3 translation) 
            => new LocalTransform { Position = Position + translation, Scale = Scale, Rotation = Rotation };

        /// <summary>Scales this transform by the specified factor.</summary>
        /// <remarks>Note that this doesn't modify the original transform. Rather it returns a new one.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly LocalTransform ApplyScale(in float3 scale) 
            => new LocalTransform { Position = Position, Scale = Scale * scale, Rotation = Rotation };

        /// <summary>Rotates this Transform by the specified quaternion.</summary>
        /// <remarks>Note that this doesn't modify the original transform. Rather it returns a new one.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly LocalTransform Rotate(in quaternion rotation)
            => new LocalTransform { Position = Position, Scale = Scale, Rotation = mul(Rotation, rotation) };
        
        /// <summary>Rotates this Transform by the specified euler angles, in radians.</summary>
        /// <remarks>Note that this doesn't modify the original transform. Rather it returns a new one.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly LocalTransform Rotate(in float3 eulers)
            => new LocalTransform { Position = Position, Scale = Scale, Rotation = mul(Rotation, quaternion.EulerXYZ(eulers)) };
        
        /// <summary>Rotates this Transform by the specified axis and angle.</summary>
        /// <remarks>Note that this doesn't modify the original transform. Rather it returns a new one.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly LocalTransform Rotate(in float3 axis, float angle)
            => new LocalTransform { Position = Position, Scale = Scale, Rotation = mul(Rotation, quaternion.AxisAngle(axis, angle)) };
        
        /// <summary>Rotates the transform around a given point by the specified angle along the specified axis.</summary>
        /// <param name="point">The point around which to rotate.</param>
        /// <param name="axis">The axis to rotate around.</param>
        /// <param name="angle">The angle to rotate, in radians.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly LocalTransform RotateAround(float3 point, float3 axis, float angle)
        {
            quaternion q = quaternion.AxisAngle(normalizesafe(axis), angle);
            return new LocalTransform
            {
                Position = point + mul(q, Position - point),
                Rotation = mul(q, Rotation),
                Scale = Scale
            };
        }

        /// <summary>Rotates the transform around a given point by the specified angle along the specified axis.</summary>
        /// <param name="parent">An optional parent transform to use for the point and axis.</param>
        /// <param name="point">The point around which to rotate.</param>
        /// <param name="axis">The axis to rotate around.</param>
        /// <param name="angle">The angle to rotate, in radians.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly LocalTransform RotateAround(Transform parent, float3 point, float3 axis, float angle)
        {
            if (parent)
            {
                point = parent.InverseTransformPoint(point);
                axis = parent.InverseTransformDirection(axis);
            }
            
            return RotateAround(point, axis, angle);
        }

        /// <summary>Rotates this Transform around the X axis.</summary>
        /// <remarks>Note that this doesn't modify the original transform. Rather it returns a new one.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly LocalTransform RotateX(float angleRadians) => Rotate(quaternion.RotateX(angleRadians));

        /// <summary>Rotates this Transform around the Y axis.</summary>
        /// <remarks>Note that this doesn't modify the original transform. Rather it returns a new one.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly LocalTransform RotateY(float angleRadians) => Rotate(quaternion.RotateY(angleRadians));

        /// <summary>Rotates this Transform around the Z axis.</summary>
        /// <remarks>Note that this doesn't modify the original transform. Rather it returns a new one.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly LocalTransform RotateZ(float angleRadians) => Rotate(quaternion.RotateZ(angleRadians));

        /// <summary>Returns a transform with added translation.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LocalTransform operator +(in LocalTransform lhs, in float3 rhs) => lhs.Translate(rhs);

        /// <summary>Returns a transform with subtracted translation.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LocalTransform operator -(in LocalTransform lhs, in float3 rhs) => lhs.Translate(-rhs);

        /// <summary>Clamp all scale components to a minimum value. Sign of scale components is preserved.</summary>
        /// <remarks>This is used to remove uninvertible zero/near-zero scaling.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClampMinimumScale(float minimumScale = MathConstants.ZeroTolerance)
        {
            for (int j = 0; j < 3; ++j)
            {
                float value = Scale[j];
                if (abs(value) < minimumScale)
                {
                    value = minimumScale * sign(value);
                    Scale[j] = value;
                }
            }
        }

        /// <summary>Returns the safe reciprocal of scale.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 GetSafeScaleReciprocal(in float3 scale, float tolerance = MathConstants.ZeroTolerance) 
            => select(1.0f / scale, 0.0f, abs(scale) <= float3(tolerance));

        /// <summary>Returns a string representation.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override string ToString() => $"Position={Position.ToString()} Rotation={Rotation.ToString()} Scale={Scale.ToString()}";

        /// <summary>Checks if a transform has equal position, rotation, and scale to another.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(LocalTransform other) => Position.Equals(other.Position) && Rotation.Equals(other.Rotation) && Scale.Equals(other.Scale);

        /// <summary>Checks if a transform has equal position, rotation, and scale to another.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override bool Equals(object o) => o is LocalTransform converted && Equals(converted);

        /// <summary>Returns a hash code for the transform.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override int GetHashCode() => unchecked((int)(hash(Position) + hash(Rotation) + hash(Scale) + 0x4FC93C25u));
        
        /// <summary>Returns true if the two transforms are equal.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in LocalTransform lhs, in LocalTransform rhs) => lhs.Equals(rhs);
        
        /// <summary>Returns true if the two transforms are not equal.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in LocalTransform lhs, in LocalTransform rhs) => !lhs.Equals(rhs);
    }

    public static class LocalTransformHelpers
    {
        /// <summary>Get a <see cref="LocalTransform"/> from a <see cref="UnityEngine.Transform"/>, in local space.</summary>
        /// <remarks>Use <see cref="UnityEngine.Transform.localPosition"/>, <see cref="UnityEngine.Transform.localRotation"/>, and <see cref="UnityEngine.Transform.localScale"/>.</remarks>
        /// <param name="t">The Unity Transform to get the local transform from.</param>
        /// <param name="space">The space to get the transform in.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LocalTransform GetTransform(this UnityEngine.Transform t, Space space)
        {
            return space == Space.World
                ? LocalTransform.FromPositionRotationScale(t.position, t.rotation, t.lossyScale) 
                : LocalTransform.FromPositionRotationScale(t.localPosition, t.localRotation, t.localScale);
        }
        
        /// <summary>Sets a <see cref="UnityEngine.Transform"/> from a <see cref="LocalTransform"/>, in local or world space.</summary>
        /// <param name="t">The Unity Transform to set the transform to.</param>
        /// <param name="transform">The transform to set.</param>
        /// <param name="space">The space to set the transform in.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetTransform(this UnityEngine.Transform t, in LocalTransform transform, Space space)
        {
            if (space == Space.World)
            {
                Transform parent = t.parent;
                t.parent = null; // Required to set world scale correctly
                t.position = transform.Position;
                t.rotation = transform.Rotation;
                t.localScale = transform.Scale;
                t.SetParent(parent, true);
            }
            else
            {
                t.localPosition = transform.Position;
                t.localRotation = transform.Rotation;
                t.localScale = transform.Scale;
            }
        }
    }
}