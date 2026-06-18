// Copyright © Magnetic Arcade. All Rights Reserved.

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace MA.Core.Editor.Bridge
{
    static class SceneHierarchyBridge
    {
        internal static EditorWindow GetLastHierarchyWindow() 
            => SceneHierarchyWindow.lastInteractedHierarchyWindow;

        internal static TreeViewItem GetItem(EditorWindow window, int instanceId)
        {
            if (window is SceneHierarchyWindow sceneHierarchyWindow)
            {
                TreeViewDataSource data = (TreeViewDataSource)sceneHierarchyWindow.sceneHierarchy.treeView.data;
                if (data == null)
                    return null;

                IList<TreeViewItem> rows = data.GetRows();
                int itemRow = data.GetRow(instanceId);
                if (itemRow < 0 || itemRow >= rows.Count)
                    return null;

                return rows[itemRow];
            }

            return null;
        }
    }
}
