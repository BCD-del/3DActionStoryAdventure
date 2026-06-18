// Copyright © Magnetic Arcade. All Rights Reserved.

using UnityEditor;
using UnityEngine;

namespace MA.Core.Editor.Bridge
{
    static class AssetPreviewBridge
    {
        internal static Texture2D CreatePreviewForAsset(UnityEngine.Object obj, UnityEngine.Object[] subAssets, string assetPath)
            => AssetPreviewUpdater.CreatePreview(obj, subAssets, assetPath, 128, 128);

        internal static Texture2D CreatePreview(UnityEngine.Object obj, UnityEngine.Object[] subAssets, string assetPath, int width, int height)
            => AssetPreviewUpdater.CreatePreview(obj, subAssets, assetPath, width, height);

        internal static void ClearTemporaryAssetPreviews()
            => AssetPreview.ClearTemporaryAssetPreviews();

        internal static bool IsLoadingAssetPreview(int instanceID, int clientID)
            => AssetPreview.IsLoadingAssetPreview(instanceID, clientID);

        internal static Texture2D GetAssetPreview(int instanceID, int clientID)
            => AssetPreview.GetAssetPreview(instanceID, clientID);

        internal static void SetPreviewTextureCacheSize(int size, int clientID)
            => AssetPreview.SetPreviewTextureCacheSize(size, clientID);

        internal static void DeletePreviewTextureManagerByID(int clientID)
            => AssetPreview.DeletePreviewTextureManagerByID(clientID);
    }
}
