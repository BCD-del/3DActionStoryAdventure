// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MA.Core
{
    /// <summary>A value that can be overridden.</summary>
    /// <typeparam name="T">The value type.</typeparam>
    [Serializable]
    public struct OverridableValue<T>
    {
        /// <summary>Whether this overridable value is overridden.</summary>
        public bool OverrideState;
        
        /// <summary>The value of this overridable value.</summary>
        public T Value;

        /// <summary>Creates a new overridable value.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public OverridableValue(T value, bool overrideState = false)
        {
            OverrideState = overrideState;
            Value = value;
        }

        /// <summary>Returns the value of this overridable value if it is overridden, otherwise returns a default value.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetValueOrDefault(T defaultValue) => OverrideState ? Value : defaultValue;

        /// <summary>Returns a hash code for the current object.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + OverrideState.GetHashCode();

                if (!EqualityComparer<T>.Default.Equals(Value, default)) // Catches null for references with boxing of value types
                    hash = hash * 23 + Value.GetHashCode();

                return hash;
            }
        }

        /// <summary>Compares the value in a overridable value with another value of the same type.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(OverridableValue<T> lhs, T rhs) => lhs.Value.Equals(rhs);

        /// <summary>Compares the value store in a overridable value with another value of the same type.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(OverridableValue<T> lhs, T rhs) => !(lhs == rhs);

        /// <summary>Checks if this overridable value is equal to another.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(OverridableValue<T> other) => EqualityComparer<T>.Default.Equals(Value, other.Value);

        /// <summary>Determines whether two object instances are equal.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj) => obj is OverridableValue<T> other && Equals(other);

        /// <summary>Explicitly downcast a <see cref="OverridableValue{T}"/> to a value of type
        /// <typeparamref name="T"/>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator T(OverridableValue<T> prop) => prop.Value;
    }
}
