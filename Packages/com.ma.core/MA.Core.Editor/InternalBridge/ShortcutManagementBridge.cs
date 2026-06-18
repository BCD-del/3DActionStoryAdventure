// Copyright © Magnetic Arcade. All Rights Reserved.

using UnityEditor.ShortcutManagement;

namespace MA.Core.Editor.Bridge
{
    abstract class ShortcutToolContextBridge :
    #if UNITY_6000_0_OR_NEWER
        IShortcutContext
    #else
        IShortcutToolContext
    #endif
    {
        public virtual bool active => false;
    }
    
    static class ShortcutIntegrationBridge
    {
        internal static void RegisterToolContext<T>(T context) where T : ShortcutToolContextBridge 
            => ShortcutIntegration.instance.contextManager.RegisterToolContext(context);

        internal static void DeregisterToolContext<T>(T context) where T : ShortcutToolContextBridge 
            => ShortcutIntegration.instance.contextManager.DeregisterToolContext(context);
    }
}
