// Copyright © Magnetic Arcade. All Rights Reserved.

namespace MA.Core.Editor.Bridge
{
    static class EditorUtilityBridge
    {
        internal static object GetPreview(this UnityEditor.Editor editor)
            => editor.preview;

        internal static UnityEngine.GameObject InstantiateForAnimatorPreview(UnityEngine.Object original)
            => UnityEditor.EditorUtility.InstantiateForAnimatorPreview(original);
    }
}