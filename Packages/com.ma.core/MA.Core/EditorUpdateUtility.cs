// Copyright © Magnetic Arcade. All Rights Reserved.

using UnityEngine;

namespace MA.Core
{
    /// <summary>Utility for forcing an update in the editor.</summary>
    /// <remarks>Useful for forcing an update in the editor when a change is made to a script.</remarks>
    public static class EditorUpdateUtility
    {
#if UNITY_EDITOR
        public static bool DidRequest = false;
        public static void EditModeQueuePlayerLoopUpdate()
        {
            if (!Application.isPlaying && !DidRequest)
            {
                DidRequest = true;
                UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
                UnityEditor.EditorApplication.update += EditorUpdate;
            }
        }

        static void EditorUpdate()
        {
            DidRequest = false;
            UnityEditor.EditorApplication.update -= EditorUpdate;
            UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
        }

#else
        public static void EditModeQueuePlayerLoopUpdate() {}
#endif
    }
}