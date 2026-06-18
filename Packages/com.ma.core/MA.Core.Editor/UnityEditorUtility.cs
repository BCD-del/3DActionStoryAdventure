// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using MA.Core.Editor.Bridge;
using UnityEditor;

namespace MA.Core.Editor
{
    static class UnityEditorUtility
    {
        internal static Action CallDelayed(EditorApplication.CallbackFunction action, double delaySeconds = 0.0)
            => EditorApplicationBridge.CallDelayed(action, delaySeconds);
    }
}