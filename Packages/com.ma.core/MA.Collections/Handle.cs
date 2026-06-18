// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MA.Collections
{ 
    /// <summary>Represents a 64 byte handle.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Handle : IEquatable<Handle>, IComparable<Handle>
    {
        /// <summary>Represents an invalid <see cref="Handle"/>.</summary>
        public static readonly Handle Null = default;
        
        /// <summary>Constructs a new <see cref="Handle"/>.</summary>
        /// <param name="index">The ID of the <see cref="Handle"/>.</param>
        /// <param name="version">The generational version of the <see cref="Handle"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Handle(int index, int version)
        {
            Index = index;
            Version = version;
        }

        /// <summary>The index of a <see cref="Handle"/>.</summary>
        public int Index;

        /// <summary>The generational version of the <see cref="Handle"/>.</summary>
        /// <value>Used to determine whether this <see cref="Handle"/> still identifies an existing <see cref="Handle"/>.</value>
        public int Version;
        
        /// <summary>True if the <see cref="Handle"/> is has a valid version.</summary>
        /// <remarks>This does not check if the <see cref="Handle"/>'s <see cref="Index"/> is valid.</remarks>
        public bool IsCreated { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Version > 0; }

        /// <summary>A hash used for comparisons.</summary>
        /// <returns>A unique hash code.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => Index;
        
        /// <summary>Compare this <see cref="Handle"/> against a given one.</summary>
        /// <param name="other">The other <see cref="Handle"/> to compare to</param>
        /// <returns>Difference based on the <see cref="Handle"/> Index value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(Handle other) => Index - other.Index;
        
        /// <summary><see cref="Handle"/> instances are equal if they represent the same handle.</summary>
        /// <param name="handle">The other <see cref="Handle"/>.</param>
        /// <returns>True, if the <see cref="Handle"/> instances have the same <see cref="Index"/> and <see cref="Version"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Handle handle) => Index == handle.Index && Version == handle.Version;
        
        /// <summary><see cref="Handle"/> instances are equal if they refer to the same handle.</summary>
        /// <param name="compare">The <see cref="object"/> to compare to this <see cref="Handle"/>.</param>
        /// <returns>True, if the compare parameter contains an <see cref="Handle"/> object having the same <see cref="Index"/> and <see cref="Version"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object compare) => compare is Handle compareHandle && Equals(compareHandle);

        /// <summary><see cref="Handle"/> instances are equal if they refer to the same handle.</summary>
        /// <param name="lhs">An <see cref="Handle"/> instance.</param>
        /// <param name="rhs">Another <see cref="Handle"/> instance.</param>
        /// <returns>True, if both <see cref="Index"/> and <see cref="Version"/> are identical.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Handle lhs, Handle rhs) => lhs.Equals(rhs);
        
        /// <summary><see cref="Handle"/> instances are equal if they refer to the same handle.</summary>
        /// <param name="lhs">An <see cref="Handle"/> instance.</param>
        /// <param name="rhs">Another <see cref="Handle"/> instance.</param>
        /// <returns>True, if either <see cref="Index"/> and <see cref="Version"/> are different.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Handle lhs, Handle rhs) => !lhs.Equals(rhs);
        
        /// <summary>A <see cref="Handle"/> is considered true if it is not equal to <see cref="Null"/>.</summary>
        /// <param name="handle">The <see cref="Handle"/> to check.</param>
        /// <returns>True if the <see cref="Handle"/> is not equal to <see cref="Null"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator bool(Handle handle) => handle.IsCreated;
        
        /// <summary>Provides a debugging string.</summary>
        /// <returns>A string containing the handle index and generational version.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => Equals(Null) ? "Handle.Null" : $"Handle({Index}:{Version})";
    }
}