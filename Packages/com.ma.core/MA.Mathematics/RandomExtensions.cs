// Copyright © Magnetic Arcade. All Rights Reserved.

using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace MA.Mathematics
{
    public static class RandomExtensions
    {
        /// <summary>Generates a new random generator with a random seed based on the current random seed.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Random NextRandom(ref this Random random) => new Random(random.NextUInt(1, uint.MaxValue));
    }
}
