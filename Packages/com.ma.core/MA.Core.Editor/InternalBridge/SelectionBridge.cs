// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace MA.Core.Editor.Bridge
{
    static class SelectionBridge
    {
        public static void Add(int instanceID)
            => Selection.Add(instanceID);
        
        public static void Remove(int instanceID)
            => Selection.Remove(instanceID);

#if UNITY_2022_3_OR_NEWER
        public static event Action<UnityObject, UnityObject, HashSet<DataMode>> DeclareDataModeSupport;
        
        public static event Action PostProcessSelectionMetaData;

        [InitializeOnLoadMethod]
        static void InitSelectionMetadata()
        {
            Selection.postProcessSelectionMetadata += () => PostProcessSelectionMetaData?.Invoke();
        }

        [DeclareDataModeSupport]
        static void AddDataModeSupport(
            UnityObject activeSelection,
            UnityObject activeContext,
            HashSet<DataMode> supportedModes)
            => DeclareDataModeSupport?.Invoke(activeSelection, activeContext, supportedModes);
        
        public static DataMode DataModeHint => Selection.dataModeHint;

        public static void SetSelection(
            UnityObject activeObject,
            UnityObject activeContext = null,
            DataMode dataModeHint = default)
            => Selection.SetSelection(activeObject, activeContext, dataModeHint);

        public static void SetSelection(
            UnityObject[] selection,
            UnityObject activeObject = null,
            UnityObject activeContext = null,
            DataMode dataModeHint = default)
            => Selection.SetSelection(selection, activeObject, activeContext, dataModeHint);

        public static void UpdateSelectionMetaData(UnityObject newContext, DataMode newDataModeHint)
            => Selection.UpdateSelectionMetaData(newContext, newDataModeHint);
#endif
    }
}