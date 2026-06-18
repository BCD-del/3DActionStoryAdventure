// Copyright © Magnetic Arcade. All Rights Reserved.

using UnityEditor.SceneManagement;
using UnityEngine;

namespace MA.Core.Editor.Bridge
{
    enum StageContextRenderMode
    {
        Normal,
        GreyedOut,
        Hidden,
    }
    
    static class StageUtilityBridge
    {
        public const ulong DefaultSceneCullingMask = SceneCullingMasks.DefaultSceneCullingMask;
        public const ulong GameViewObjects = SceneCullingMasks.GameViewObjects;
        public const ulong MainStageSceneViewObjects = SceneCullingMasks.MainStageSceneViewObjects;
        public const ulong MainStageExcludingPrefabInstanceObjectsOpenInPrefabMode = SceneCullingMasks.MainStageExcludingPrefabInstanceObjectsOpenInPrefabMode;
        public const ulong MainStagePrefabInstanceObjectsOpenInPrefabMode = SceneCullingMasks.MainStagePrefabInstanceObjectsOpenInPrefabMode;
        public const ulong PrefabStagePrefabInstanceObjectsOpenInPrefabMode = SceneCullingMasks.PrefabStagePrefabInstanceObjectsOpenInPrefabMode;
        
        internal static bool IsPrefabInstanceHiddenForInContextEditing(GameObject obj) 
            => StageUtility.IsPrefabInstanceHiddenForInContextEditing(obj);

        internal static StageContextRenderMode GetContextRenderMode()
            => (StageContextRenderMode)StageNavigationManager.instance.contextRenderMode;
        
        internal static bool IsMainStage(this StageHandle stageHandle) 
            => stageHandle.isMainStage;

#if UNITY_2022_3_OR_NEWER
        internal static bool IsGizmoCulledBySceneCullingMasksOrFocusedScene(GameObject gameObject, Camera camera)
            => StageUtility.IsGizmoCulledBySceneCullingMasksOrFocusedScene(gameObject, camera);
#endif
    }
}