// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using UnityEditor.Search;
using UnityEngine.Search;

namespace MA.Core.Editor.Bridge
{
    static class SearchViewStateBridge
    {
        internal static void SetSelectHandler(SearchViewState state, Action<SearchItem, bool> handler)
            => state.selectHandler = handler;
        
        internal static void SetExcludeClearItem(SearchViewState state, bool exclude)
            => state.excludeClearItem = exclude;
        
        internal static void SetHideAllGroup(SearchViewState state, bool hide)
            => state.hideAllGroup = hide;
        
        internal static void SetHideTabs(SearchViewState state, bool hide)
            => state.hideTabs = hide;
    }
}