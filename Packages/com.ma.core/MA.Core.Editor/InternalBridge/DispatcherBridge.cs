// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using UnityEditor.Search;

namespace MA.Core.Editor.Bridge
{
    static class DispatcherBridge
    {
        internal static void Enqueue(Action action) => Dispatcher.Enqueue(action);
    }
}