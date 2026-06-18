// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using System.Runtime.CompilerServices;
using MA.UIElements.Editor.Bridge;
using UnityEngine.UIElements;

namespace MA.UIElements.Editor
{
    /// <summary>Mirrors the internal PseudoStates enum from <see cref="UnityEngine.UIElements"/>.</summary>
    [Flags]
    public enum PseudoStates
    {
        Active   = 1 << 0,
        Hover    = 1 << 1,
        Checked  = 1 << 3,
        Disabled = 1 << 5,
        Focus    = 1 << 6,
        Root     = 1 << 7
    }
    
    public static class VisualElementEditorExtensions
    {
        /// <summary>Retrieves the PseudoStates flags currently set on the element.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PseudoStates GetPseudoStates(this VisualElement v) 
            => (PseudoStates)VisualElementEditorBridge.GetPseudoStates(v);

        /// <summary>Sets the active PseudoStates flags for the element.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetPseudoStates(this VisualElement v, PseudoStates s) 
            => VisualElementEditorBridge.SetPseudoStates(v, (int)s);

        /// <summary>Adds a PseudoStates flag to the element.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddPseudoState(this VisualElement v, PseudoStates s) 
            => VisualElementEditorBridge.AddPseudoState(v, (int)s);

        /// <summary>Removes a PseudoStates flag from the element.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemovePseudoState(this VisualElement v, PseudoStates s)
            => VisualElementEditorBridge.RemovePseudoState(v, (int)s);
        
        /// <summary>Returns the contextual menu manager for this element.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ContextualMenuManager GetContextualMenuManager(this VisualElement element)
            => VisualElementEditorBridge.GetContextualMenuManager(element);
    }
}