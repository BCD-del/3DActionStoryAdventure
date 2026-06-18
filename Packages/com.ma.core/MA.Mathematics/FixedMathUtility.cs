// Copyright © Magnetic Arcade. All Rights Reserved.

using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace MA.Mathematics
{
    /// <summary>Utility methods for fixed-point math.</summary>
    public static class FixedMathUtility
    {
        /// <summary>The default resolution for converting a float to a fixed-point value.</summary>
        public const float DefaultFixedDistanceResolution = 100.0f;

        /// <summary>Create a fixed-point value from a float using the specified resolution with ceiling rounding.</summary>
        /// <param name="f">The float value to convert.</param>
        /// <param name="resolution">The resolution to use for the conversion.</param>
        /// <returns>A ushort fixed-point value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort FixedFromFloatCeilU16(float f, float resolution = DefaultFixedDistanceResolution)
            => (ushort)math.clamp((int)math.ceil(f * resolution), 0, 0xffff);

        /// <summary>Create a fixed-point value from a float using the specified resolution with floor rounding.</summary>
        /// <param name="f">The float value to convert.</param>
        /// <param name="resolution">The resolution to use for the conversion.</param>
        /// <returns>A ushort fixed-point value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort FixedFromFloatFloorU16(float f, float resolution = DefaultFixedDistanceResolution)
            => (ushort)math.clamp((int)math.floor(f * resolution), 0, 0xffff);
    }
}