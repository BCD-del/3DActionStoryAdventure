// Copyright © Magnetic Arcade. All Rights Reserved.

using System.IO;
using UnityEditor;
using UnityEditor.Search;
using UnityEditorInternal;
using UnityEngine;
using SearchUtils = UnityEditor.Search.SearchUtils;

namespace MA.Core.Editor.Bridge
{
    static class SearchUtilsBridge
    {
        internal static void StartDrag(UnityEngine.Object[] objects, string[] paths, string label = null)
            => Utils.StartDrag(objects, paths, label);

        internal static void SelectObject(UnityEngine.Object obj, bool ping = false)
            => Utils.SelectObject(obj, ping);

        internal static void FrameAssetFromPath(string path)
            => Utils.FrameAssetFromPath(path);

        internal static int GetMainAssetInstanceID(string assetPath)
            => Utils.GetMainAssetInstanceID(assetPath);

        // --- 2021.3 Internal API ---

        internal static SearchProvider CreateGroupProvider(SearchProvider templateProvider, string groupId, int groupPriority, bool cacheProvider = false)
            => SearchUtils.CreateGroupProvider(templateProvider, groupId, groupPriority, cacheProvider);

        internal static Texture2D GetTypeIcon(in System.Type type)
            => SearchUtils.GetTypeIcon(type);

        // --- Search Context Helpers, part of the Unity 6.0 API ---

        internal static int GetClientId(SearchContext ctx)
        {
#if UNITY_2023_1_OR_NEWER
            return ctx is { searchView: not null } ? ctx.searchView.GetViewId() : 0;
#else
            return ctx is { searchView: QuickSearch qs } ? qs.GetInstanceID() : 0;
#endif
        }

        internal static Texture2D GetSceneObjectPreview(SearchContext ctx, GameObject obj, Vector2 previewSize, FetchPreviewOptions options, Texture2D defaultThumbnail)
        {
#if UNITY_2023_1_OR_NEWER
            return Utils.GetSceneObjectPreview(ctx, obj, previewSize, options, defaultThumbnail);
#else
            var sr = obj.GetComponent<SpriteRenderer>();
            if (sr && sr.sprite && sr.sprite.texture)
                return sr.sprite.texture;

            var clientId = GetClientId(ctx);
            if (!options.HasAny(FetchPreviewOptions.Large))
            {
                var preview = AssetPreview.GetAssetPreview(obj.GetInstanceID(), clientId);
                if (preview)
                    return preview;

                if (AssetPreview.IsLoadingAssetPreview(obj.GetInstanceID(), clientId))
                    return null;
            }

            var assetPath = SearchUtils.GetHierarchyAssetPath(obj, true);
            if (string.IsNullOrEmpty(assetPath))
                return AssetPreview.GetAssetPreview(obj.GetInstanceID(), clientId) ?? defaultThumbnail;

            return GetAssetPreviewFromPath(ctx, assetPath, previewSize, options);
#endif
        }

        static Texture2D GetAssetPreviewFromGUID(SearchContext ctx, string guid)
            => AssetPreview.GetAssetPreviewFromGUID(guid, GetClientId(ctx));

        internal static Texture2D GetAssetPreviewFromPath(SearchContext ctx, string path, FetchPreviewOptions previewOptions)
            => GetAssetPreviewFromPath(ctx, path, new Vector2(128, 128), previewOptions);

        internal static Texture2D GetAssetPreviewFromPath(SearchContext ctx, string path, Vector2 previewSize, FetchPreviewOptions previewOptions)
        {
#if UNITY_2023_1_OR_NEWER
            return Utils.GetAssetPreviewFromPath(ctx, path, previewSize, previewOptions);
#else
            var assetType = AssetDatabase.GetMainAssetTypeAtPath(path);
            if (assetType == typeof(SceneAsset))
                return AssetDatabase.GetCachedIcon(path) as Texture2D;

            if (previewOptions.HasAny(FetchPreviewOptions.Normal))
            {
                if (assetType == typeof(AudioClip))
                    return GetAssetThumbnailFromPath(ctx, path);

                try
                {
                    var fi = new FileInfo(path);
                    if (!fi.Exists)
                        return null;
                    if (fi.Length > 16 * 1024 * 1024)
                        return GetAssetThumbnailFromPath(ctx, path);
                }
                catch
                {
                    return null;
                }
            }

            if (typeof(Texture).IsAssignableFrom(assetType))
            {
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (tex)
                    return tex;
            }

            var obj = AssetDatabase.LoadMainAssetAtPath(path);
            if (obj == null)
                return null;

            if (previewOptions.HasAny(FetchPreviewOptions.Large))
            {
                var tex = AssetPreviewUpdater.CreatePreview(obj, null, path, (int)previewSize.x, (int)previewSize.y);
                if (tex)
                    return tex;
            }

            return GetAssetPreview(ctx, obj, previewOptions) ?? AssetDatabase.GetCachedIcon(path) as Texture2D;
#endif
        }

        internal static Texture2D GetAssetPreview(SearchContext ctx, UnityEngine.Object obj, FetchPreviewOptions previewOptions)
        {
#if UNITY_2023_1_OR_NEWER
            return Utils.GetAssetPreview(ctx, obj, previewOptions);
#else
            var preview = AssetPreview.GetAssetPreview(obj.GetInstanceID(), GetClientId(ctx));
            if (preview == null || previewOptions.HasAny(FetchPreviewOptions.Large))
            {
                var largePreview = AssetPreview.GetMiniThumbnail(obj);
                if (preview == null || (largePreview != null && largePreview.width > preview.width))
                    preview = largePreview;
            }
            return preview;
#endif
        }

        internal static Texture2D GetAssetThumbnailFromPath(SearchContext ctx, string path)
        {
            var thumbnail = GetAssetPreviewFromGUID(ctx, AssetDatabase.AssetPathToGUID(path));
            if (thumbnail)
                return thumbnail;
            thumbnail = AssetDatabase.GetCachedIcon(path) as Texture2D;
            return thumbnail ?? InternalEditorUtility.FindIconForFile(path);
        }
    }
}
