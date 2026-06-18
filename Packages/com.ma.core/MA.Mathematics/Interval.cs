// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

namespace MA.Mathematics
{
    /// <summary>Clamps an interval to a minimum and maximum value.</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class IntervalClampAttribute : PropertyAttribute
    {
        public readonly float Min;
        public readonly float Max;

        public IntervalClampAttribute(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }
    
    /// <summary>Specifies the interval should provide a min max slider for the editor.</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class IntervalRangeAttribute : PropertyAttribute
    {
        public readonly float Min;
        public readonly float Max;
        
        public IntervalRangeAttribute(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }

    /// <summary>Clamps an interval to a minimum value.</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class IntervalMinAttribute : PropertyAttribute
    {
        public readonly float Min;

        public IntervalMinAttribute(float min)
        {
            Min = min;
        }
    }

    /// <summary>Represents a float interval between Min and Max.</summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Interval : IEquatable<Interval>
    {
        /// <summary>Represents and empty interval.</summary>
        public static Interval Empty = new Interval(float.MaxValue, -float.MaxValue);

        /// <summary>The minimum value of the interval.</summary>
        public float Min;
        /// <summary>The maximum value of the interval.</summary>
        public float Max;

        /// <summary>The center of the interval.</summary>
        public readonly float Center
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => lerp(Min, Max, 0.5f);
        }
        /// <summary>The half length of the interval.</summary>
        public readonly float Extent
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (Max - Min) * 0.5f;
        }
        /// <summary>The length of the interval</summary>
        public readonly float Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Max - Min;
        }
        /// <summary>Returns true if the interval is empty.</summary>
        public readonly bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Max < Min;
        }
        /// <summary>Returns the absolute maximum value held by the interval.</summary>
        public readonly float MaxAbsExtrema
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => max(abs(Min), abs(Max));
        }

        /// <summary>Creates an interval between min and max.</summary>
        public Interval(float min, float max)
        {
            Min = min;
            Max = max;
        }

        /// <summary>Expands the interval to contain value.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Contain(float value)
        {
            if (value < Min) { Min = value; }
            if (value > Max) { Max = value; }
        }

        /// <summary>Expands the interval to contain value.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Contain(Interval i)
        {
            if (i.Min < Min) { Min = i.Min; }
            if (i.Max > Max) { Max = i.Max; }
        }

        /// <summary>Returns true if the interval contains value.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(float value) => value >= Min && value <= Max;

        /// <summary>Returns true if this interval contains interval other.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(Interval i) => Contains(i.Min) && Contains(i.Max);

        /// <summary>Returns true if this interval overlaps with interval other.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Overlaps(Interval i) => !(i.Min > Max || i.Max < Min);

        /// <summary>Returns the squared distance between this and interval other.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float SquaredDistance(Interval i)
        {
            if      (Max < i.Min) { return lengthsq(i.Min - Max); }
            else if (Min > i.Max) { return lengthsq(Min - i.Max); }
            else                      { return 0; }
        }

        /// <summary>Returns the distance between this and interval other.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float Distance(Interval i)
        {
            if      (Max < i.Min) { return i.Min - Max; }
            else if (Min > i.Max) { return Min - i.Max; }
            else                  { return 0; }
        }

        /// <summary>Returns an interval that is an intersection of this interval and other.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Interval IntersectionWith(Interval i)
        {
            if (i.Min > Max || i.Max < Min) { return Empty; }
            return new Interval(max(Min, i.Min), min(Max, i.Max));
        }

        /// <summary>Clamps value to interval [Min,Max]</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Interval Clamped(float min, float max)
        {
            return new Interval(
                (Min < min) ? min : (Min > max) ? max : Min,
                (Max < min) ? min : (Max > max) ? max : Max);
        }

        /// <summary>Clamps value to interval [Min,Max]</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float Clamp(float value)
        {
            return (value < Min) ? Min :
                   (value > Max) ? Max : value;
        }

        /// <summary>Interpolate between Min and Max using value t in range [0,1]</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float Interpolate(float t) 
            => (1f - t) * Min + (t) * Max;

        /// <summary>Convert value into (clamped) t value in range [0,1]</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float GetT(float value)
        {
            if      (value <= Min) { return 0.0f; }
            else if (value >= Max) { return 1.0f; }
            else if (Min == Max)   { return 0.5f; }
            else                   { return (value - Min) / (Max - Min); }
        }

        /// <summary>Expands the interval to contain radius.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Expand(float radius)
        {
            Max += radius;
            Min -= radius;
        }
        
        /// <summary>Sorts the min and max values of the interval.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Interval Sorted() => select(this, ((float2)this).yx, Min > Max);
        
        /// <summary>Returns a string representation.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => $"Interval(Min={Min}, Max={Max})";

        /// <summary>Returns the hash code for this instance.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => unchecked((int)hash(new float2(Min, Max)));

        /// <summary>Returns true if the specified interval is equal to this instance.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Interval other) => this == other;

        /// <summary>Returns true if the specified object is equal to this instance.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object o) => o is Interval converted && Equals(converted);
        
        /// <summary>Returns the result of a component-wise equality operation on two segments.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Interval lhs, Interval rhs) => lhs.Min == rhs.Min && lhs.Max == rhs.Max;
        
        /// <summary>Returns the result of a component-wise not equal operation on two segments.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Interval lhs, Interval rhs) => lhs.Min != rhs.Min || lhs.Max != rhs.Max;

        /// <summary>Converts a float to an interval.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Interval(float f) => new Interval(f, f);

        /// <summary>Converts an interval to a float2.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator float2(Interval range) => new float2(range.Min, range.Max);

        /// <summary>Converts a float2 to an interval.</summary>
        public static implicit operator Interval(float2 f) => new Interval { Min = f.x, Max = f.y };

        /// <summary>Converts an interval to a float2.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Vector2(Interval range) => new Vector2(range.Min, range.Max);

        /// <summary>Converts a float2 to an interval.</summary>
        public static implicit operator Interval(Vector2 f) => new Interval { Min = f.x, Max = f.y };

        /// <summary>Returns the result of a component-wise unary minus operation on a Interval.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Interval operator -(Interval i) => new Interval(-i.Min, -i.Max);
        
        /// <summary>Returns the result of a component-wise addition operation on a Interval and a float value.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Interval operator +(Interval lhs, float f) => new Interval(lhs.Min + f, lhs.Max + f);
        
        /// <summary>Returns the result of a component-wise subtraction operation on a Interval and a float value.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Interval operator -(Interval lhs, float f) => new Interval(lhs.Min - f, lhs.Max - f);
        
        /// <summary>Returns the result of a component-wise multiplication operation on a Interval and a float value.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Interval operator *(Interval lhs, float f) => new Interval(lhs.Min * f, lhs.Max * f);
    }
}
