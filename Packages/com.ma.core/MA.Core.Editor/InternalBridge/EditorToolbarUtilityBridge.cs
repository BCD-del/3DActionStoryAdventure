// Copyright © Magnetic Arcade. All Rights Reserved.

using UnityEditor.Toolbars;
using UnityEngine.UIElements;

namespace MA.Core.Editor.Bridge
{
    static class EditorToolbarUtilityBridge
    {
        internal static void LoadStyleSheets(string name, VisualElement target)
            => EditorToolbarUtility.LoadStyleSheets(name, target);
    }
}