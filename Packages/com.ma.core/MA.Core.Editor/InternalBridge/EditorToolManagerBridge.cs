// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using UnityEditor.EditorTools;
using UnityEngine;

namespace MA.Core.Editor.Bridge
{
    static class EditorToolManagerBridge
    {
        internal static EditorTool NoneTool => (EditorTool)GetSingleton(typeof(UnityEditor.NoneTool));

        internal static EditorToolContext ActiveToolContext
        {
            get => EditorToolManager.activeToolContext;
            set => EditorToolManager.activeToolContext = value;
        }

        internal static EditorTool ActiveTool
        {
            get => EditorToolManager.activeTool;
            set => EditorToolManager.activeTool = value;
        }

        internal static T GetSingleton<T>() where T : ScriptableObject
            => (T)EditorToolManager.GetSingleton(typeof(T));

        internal static ScriptableObject GetSingleton(Type type) 
            => EditorToolManager.GetSingleton(type);

        internal static EditorTool GetActiveTool()
            => EditorToolManager.GetActiveTool();
    }
}
