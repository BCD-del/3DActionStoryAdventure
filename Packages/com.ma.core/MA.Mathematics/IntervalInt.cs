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
    public class IntervalIntClampAttribute : PropertyAttribute
    {
        public readonly int Min;
        public readonly int Max;

        public IntervalIntClampAttribute(int min, int max)
        {
            Min = min;
            Max = max;
        }
    }

    /// <summary>Clamps an interval to a minimum value.</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class IntervalIntMinAttribute : PropertyAttribute
    {
        public readonly int Min;

        public IntervalIntMinAttribute(int min)
        {
            Min = min;
        }
    }

    /// <summary>Represents a float interval between Min and Max.</summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct IntervalInt : IEquatable<IntervalInt>
    {
        /// <summary>Represents and empty interval.</summary>
        public static IntervalInt Empty = new IntervalInt(int.MaxValue, -int.MaxValue);

        /// <summary>The minimum value of the interval.</summary>
        public int Min;
        /// <summary>The maximum value of the interval.</summary>
        public int Max;

        /// <summary>The length of the interval</summary>
        public readonly int Length
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
        public readonly int MaxAbsExtrema
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => max(abs(Min), abs(Max));
        }

        /// <summary>Creates an interval between min and max.</summary>
        public IntervalInt(int min, int max)
        {
            Min = min;
            Max = max;
        }

        /// <summary>Expands the interval to contain value.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Contain(int value)
        {
            if (value < Min) { Min = value; }
            if (value > Max) { Max = value; }
        }

        /// <summary>Expands the interval to contain value.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Contain(IntervalInt value)
        {
            if (!value.IsEmpty)
            {
                if (value.Min < Min) { Min = value.Min; }
                if (value.Max > Max) { Max = value.Max; }
            }
        }

        /// <summary>Expands the interval to contain value.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IntervalInt Contained(int value)
        {
            IntervalInt i = this;
            if (value < i.Min) { i.Min = value; }
            if (value > i.Max) { i.Max = value; }
            return i;
        }

        /// <summary>Returns true if the interval contains value.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(int value) => value >= Min && value <= Max;

        /// <summary>Returns true if this interval contains interval other.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(IntervalInt i) => Contains(i.Min) && Contains(i.Max);

        /// <summary>Returns true if this interval overlaps with interval other.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Overlaps(IntervalInt i) => !(i.Min > Max || i.Max < Min);

        /// <summary>Returns the squared distance between this and interval other.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float SquaredDistance(IntervalInt i)
        {
            if      (Max < i.Min) { return lengthsq(i.Min - Max); }
            else if (Min > i.Max) { return lengthsq(Min - i.Max); }
            else                      { return 0; }
        }

        /// <summary>Returns the distance between this and interval other.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float Distance(IntervalInt i)
        {
            if      (Max < i.Min) { return i.Min - Max; }
            else if (Min > i.Max) { return Min - i.Max; }
            else                  { return 0; }
        }

        /// <summary>Returns an interval that is an intersection of this interval and other.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly IntervalInt IntersectionWith(IntervalInt i)
        {
            if (i.Min > Max || i.Max < Min) { return Empty; }
            return new IntervalInt(max(Min, i.Min), min(Max, i.Max));
        }

        /// <summary>Clamps value to interval [Min,Max]</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float Clamp(int value)
        {
            return (value < Min) ? Min :
                   (value > Max) ? Max : value;
        }

        /// <summary>Clamps value to interval [Min,Max]</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly IntervalInt Clamped(int min, int max)
        {
            return new IntervalInt(
                (Min < min) ? min : (Min > max) ? max : Min,
                (Max < min) ? min : (Max > max) ? max : Max);
        }
        
        /// <summary>Expands the interval to contain radius.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Expand(int radius)
        {
            Max += radius;
            Min -= radius;
        }
        
        /// <summary>Sorts the minimum and maximum values of the interval.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly IntervalInt Sorted() => select(this, ((int2)this).yx, Min > Max);

        /// <summary>Returns a string representation.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => $"Interval(Min={Min}, Max={Max})";

        /// <summary>Generates a hash code for the interval.</summary>
        /// <returns>A hash code.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => unchecked((int)hash(new int2(Min, Max)));
        
        /// <summary>Returns true if the interval is equal to another interval.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(IntervalInt other) => this == other;

        /// <summary>Returns true if the interval is equal to an object.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object o) => o is IntervalInt converted && Equals(converted);
        
        /// <summary>Returns the result of a component-wise equality operation on two segments.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(IntervalInt lhs, IntervalInt rhs) => lhs.Min == rhs.Min && lhs.Max == rhs.Max;
        
        /// <summary>Returns the result of a component-wise not equal operation on two segments.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(IntervalInt lhs, IntervalInt rhs) => lhs.Min != rhs.Min || lhs.Max != rhs.Max;

        /// <summary>Converts a float to an interval.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator IntervalInt(int i) => new IntervalInt(i, i);

        /// <summary>Converts an interval to a float2.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator int2(IntervalInt i) => new int2(i.Min, i.Max);

        /// <summary>Converts a float2 to an interval.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator IntervalInt(int2 i2) => new IntervalInt { Min = i2.x, Max = i2.y };

        /// <summary>Returns the result of a component-wise unary minus operation on a IntervalInt.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntervalInt operator -(IntervalInt i) => new IntervalInt(-i.Min, -i.Max);
        
        /// <summary>Returns the result of a component-wise addition operation on a IntervalInt and a float value.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntervalInt operator +(IntervalInt lhs, int i) => new IntervalInt(lhs.Min + i, lhs.Max + i);
        
        /// <summary>Returns the result of a component-wise subtraction operation on a IntervalInt and a float value.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntervalInt operator -(IntervalInt lhs, int i) => new IntervalInt(lhs.Min - i, lhs.Max - i);
        
        /// <summary>Returns the result of a component-wise multiplication operation on a IntervalInt and a float value.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntervalInt operator *(IntervalInt lhs, int i) => new IntervalInt(lhs.Min * i, lhs.Max * i);
    }
}
