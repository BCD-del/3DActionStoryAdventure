// Copyright © Magnetic Arcade. All Rights Reserved.

using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace MA.Core.Editor.Bridge
{
    class EditorGUIUtilityBridge
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Texture2D LoadIcon(string path) => EditorGUIUtility.LoadIcon(path);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Texture2D LoadIconRequired(string path) => EditorGUIUtility.LoadIconRequired(path);
    }
}