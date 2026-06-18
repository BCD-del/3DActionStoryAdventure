// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using UnityEditor;

namespace MA.Core.Editor.Bridge
{
    static class EditorApplicationBridge
    {
        internal static event EditorApplication.CallbackFunction tick
        {
            add => EditorApplication.tick += value;
            remove => EditorApplication.tick -= value;
        }

        internal static void SignalTick() 
            => EditorApplication.SignalTick();
        
        internal static Action CallDelayed(EditorApplication.CallbackFunction action, double delaySeconds = 0.0)
            => EditorApplication.CallDelayed(action, delaySeconds);
    }
}