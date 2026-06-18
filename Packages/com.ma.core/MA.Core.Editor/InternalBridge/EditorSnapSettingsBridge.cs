// Copyright © Magnetic Arcade. All Rights Reserved.

namespace MA.Core.Editor.Bridge
{
    static class EditorSnapSettingsBridge
    {
        internal static bool incrementalSnapActive => UnityEditor.EditorSnapSettings.incrementalSnapActive;
        internal static bool gridSnapActive => UnityEditor.EditorSnapSettings.gridSnapActive;
        internal static bool vertexSnapActive => UnityEditor.EditorSnapSettings.vertexSnapActive;
    }
}