// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityEditor;

namespace MA.Core.Editor.Bridge
{
    static class InspectorWindowBridge
    {
        internal static void FindInspectorMatching(UnityEditor.Editor editor, Action onFound)
        {
            List<InspectorWindow> inspectors = InspectorWindow.GetInspectors();
            foreach (InspectorWindow inspector in inspectors)
            {
                UnityEditor.Editor[] editors = inspector.tracker.activeEditors;
                foreach (UnityEditor.Editor otherEditor in editors)
                {
                    if (otherEditor == editor)
                    {
                        onFound?.Invoke();
                        return;
                    }
                }
            }
        }
    }
}