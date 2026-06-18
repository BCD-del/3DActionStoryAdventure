// Copyright © Magnetic Arcade. All Rights Reserved.

using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace MA.Core
{
    /// <summary>Provides utility functions for working with Unity.</summary>
    public static class UnityUtility
    {
        static int s_MainThreadId = -1;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#endif
        static void InitializeMainThreadId()
        {
            // Cache the main thread ID on load.
            s_MainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
        }

        /// <summary>Returns true if the current thread is the main thread.</summary>
        public static bool IsMainThread
            => System.Threading.Thread.CurrentThread.ManagedThreadId == s_MainThreadId;

        /// <summary>Destroys a UnityObject safely.</summary>
        /// <param name="obj">Object to be destroyed.</param>
        public static void Destroy(UnityObject obj)
        {
            if (obj != null)
            {
#if UNITY_EDITOR
                if (Application.isPlaying && !UnityEditor.EditorApplication.isPaused)
                    UnityObject.Destroy(obj);
                else
                    UnityObject.DestroyImmediate(obj);
#else
                UnityObject.Destroy(obj);
#endif
            }
        }

        /// <summary>Compatibility wrapper for <see cref="UnityEngine.Object.FindFirstObjectByType{T}(UnityEngine.FindObjectsInactive)"/></summary>
        public static T FindFirstObjectByType<T>(bool includeInactive = false) where T : UnityEngine.Object
        {
#if HAS_FIND_OBJECTS_BY_TYPE
            return UnityEngine.Object.FindFirstObjectByType<T>(includeInactive ? UnityEngine.FindObjectsInactive.Include : UnityEngine.FindObjectsInactive.Exclude);
#else
            return UnityEngine.Object.FindObjectOfType<T>(includeInactive);
#endif
        }

        /// <summary>Compatibility wrapper for <see cref="UnityEngine.Object.FindObjectsByType{T}(UnityEngine.FindObjectsSortMode)"/></summary>
        public static T[] FindObjectsByType<T>(bool includeInactive = false) where T : UnityEngine.Object
        {
#if HAS_FIND_OBJECTS_BY_TYPE
            return UnityEngine.Object.FindObjectsByType<T>(
                includeInactive ? UnityEngine.FindObjectsInactive.Include : UnityEngine.FindObjectsInactive.Exclude,
                UnityEngine.FindObjectsSortMode.InstanceID);
#else
            return UnityEngine.Object.FindObjectsOfType<T>(includeInactive);
#endif
        }

        /// <summary>Returns the first <see cref="Camera"/> that is relevant for the current Gizmo drawing.</summary>
        public static Camera GetGizmosCamera()
        {
            // Try to get the relevant camera for the Gizmo
            Camera camera;
#if UNITY_EDITOR
            if (UnityEditor.SceneView.currentDrawingSceneView &&
                UnityEditor.SceneView.currentDrawingSceneView.camera)
                camera = UnityEditor.SceneView.currentDrawingSceneView.camera;
            else
#endif
            if (Camera.current)
                camera = Camera.current;
            else
                camera = Camera.main;

            return camera;
        }
    }
}
