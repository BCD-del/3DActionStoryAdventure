// Copyright © Magnetic Arcade. All Rights Reserved.

using System.Runtime.CompilerServices;

namespace MA.UIElements
{
    static class StringExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string WithUssElement(this string blockName, string elementName) 
            => blockName + "__" + elementName;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string WithUssModifier(this string blockName, string modifier) 
            => blockName + "--" + modifier;
    }
}