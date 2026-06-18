// Copyright © Magnetic Arcade. All Rights Reserved.

using UnityEditor;

namespace MA.Core.Editor
{
    class AssetEvents : AssetPostprocessor
    {
        public static event AssetsChangedDelegate AssetsChangedOnHDD;

        public delegate void AssetsChangedDelegate(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths);
        
        static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            AssetsChangedOnHDD?.Invoke(importedAssets, deletedAssets, movedAssets, movedFromAssetPaths);
        }
    }
}