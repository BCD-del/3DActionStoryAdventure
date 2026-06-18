// Copyright © Magnetic Arcade. All Rights Reserved.

using UnityEditor;

namespace MA.Core.Editor.InternalBridge
{
    static class AssetDatabaseBridge
    {
        internal static System.Type GetTypeFromVisibleGUIDAndLocalFileIdentifier(GUID guid, long localId)
            => AssetDatabase.GetTypeFromVisibleGUIDAndLocalFileIdentifier(guid, localId);

        internal static UnityEngine.Object LoadMainAssetAtGUID(GUID assetGUID)
            => AssetDatabase.LoadMainAssetAtGUID(assetGUID);

        internal static int GetMainAssetInstanceID(string assetPath)
            => AssetDatabase.GetMainAssetInstanceID(assetPath);
    }
}
