// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MA.Core.Editor.Bridge
{
    static class StageNavigationManagerBridge
    {
        internal static bool IsContextModeNormal(PrefabStage prefabStage) 
            => prefabStage.mode == PrefabStage.Mode.InContext &&
               StageNavigationManager.instance.contextRenderMode == StageUtility.ContextRenderMode.Normal;
        
        internal static event Action<Stage, Stage> stageChanging
        {
            add => StageNavigationManager.instance.stageChanging += value;
            remove => StageNavigationManager.instance.stageChanging -= value;
        }

        internal static event Action<Stage, Stage> stageChanged
        {
            add => StageNavigationManager.instance.stageChanged += value;
            remove => StageNavigationManager.instance.stageChanged -= value;
        }

        internal static event Action<Stage> beforeSwitchingAwayFromStage
{
            add => StageNavigationManager.instance.beforeSwitchingAwayFromStage += value;
            remove => StageNavigationManager.instance.beforeSwitchingAwayFromStage -= value;
        }

        internal static event Action<Stage> afterSuccessfullySwitchedToStage
        {
            add => StageNavigationManager.instance.afterSuccessfullySwitchedToStage += value;
            remove => StageNavigationManager.instance.afterSuccessfullySwitchedToStage -= value;
        }
    }
}