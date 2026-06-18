// Copyright © Magnetic Arcade. All Rights Reserved.
// ReSharper disable InconsistentNaming

using System;
using UnityEditor;

namespace MA.Core.Editor.Bridge
{
    static class SceneViewMotionBridge
    {
        internal static event Action viewToolActiveChanged
        {
            add => SceneViewMotion.viewToolActiveChanged += value;
            remove => SceneViewMotion.viewToolActiveChanged -= value;
        }
    }
}