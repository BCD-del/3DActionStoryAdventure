// Copyright © Magnetic Arcade. All Rights Reserved.

using System.Runtime.CompilerServices;
using UnityEngine.UIElements;

namespace MA.UIElements.Editor.Bridge
{
    public static class VisualElementEditorBridge
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetPseudoStates(VisualElement v) 
            => (int)v.pseudoStates;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetPseudoStates(VisualElement v, int s) 
            => v.pseudoStates = (UnityEngine.UIElements.PseudoStates)s;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddPseudoState(VisualElement v, int s) 
            => v.pseudoStates |= (UnityEngine.UIElements.PseudoStates)s;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemovePseudoState(VisualElement v, int s) 
            => v.pseudoStates &= ~(UnityEngine.UIElements.PseudoStates)s;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ContextualMenuManager GetContextualMenuManager(VisualElement element)
            => element.elementPanel.contextualMenuManager;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VisualElement GetVisualInput<TValueType>(BaseField<TValueType> element)
            => element.visualInput;
    }
}
