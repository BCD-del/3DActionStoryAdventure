// Copyright © Magnetic Arcade. All Rights Reserved.

using System.Runtime.CompilerServices;

namespace MA.Collections
{
    /// <summary>Provides extension methods for <see cref="System.Array"/>.</summary>
    public static class ArrayUtility
    {
        /// <summary>Determines if a given index is valid for the array.</summary>
        /// <param name="array">The array to check the index against.</param>
        /// <param name="index">The index to validate.</param>
        /// <typeparam name="T">The type of the elements in the array.</typeparam>
        /// <returns><c>true</c> if the index is valid; otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidIndex<T>(this T[] array, int index) => index >= 0 && index < array.Length;
    }
}
