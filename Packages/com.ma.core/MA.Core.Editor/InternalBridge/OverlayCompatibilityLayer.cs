// Copyright © Magnetic Arcade. All Rights Reserved.

using System.Collections.Generic;
using System.Reflection;
using UnityEditor.Overlays;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;
using UnityObject = UnityEngine.Object;

#if UNITY_2022_1_OR_NEWER
    using OverlayToolbarElement = UnityEditor.Overlays.OverlayToolbar;
#else
    using OverlayToolbarElement = UnityEngine.UIElements.VisualElement;
#endif

namespace MA.Core.Editor
{
    public abstract class CreateToolbarEditor :
        UnityEditor.Editor,
        UnityEditor.Overlays.ICreateHorizontalToolbar,
        UnityEditor.Overlays.ICreateVerticalToolbar
    {
        protected virtual void AddToolbarElements(OverlayToolbar toolbar, Layout layout) { }

        OverlayToolbarElement ICreateHorizontalToolbar.CreateHorizontalToolbarContent() => CreateOverlayToolbar(Layout.HorizontalToolbar);

        OverlayToolbarElement ICreateVerticalToolbar.CreateVerticalToolbarContent() => CreateOverlayToolbar(Layout.VerticalToolbar);

        public sealed override VisualElement CreateInspectorGUI() => CreateOverlayToolbar(Layout.Panel);

        OverlayToolbar CreateOverlayToolbar(Layout layout)
        {
            var root = new OverlayToolbar();
            AddToolbarElements(root, layout);

            foreach (var child in root.Children())
                child.AddToClassList("unity-editor-toolbar-element");

            root.SetupChildrenAsButtonStrip();
            return root;
        }
    }
}

#if !UNITY_2022_1_OR_NEWER
namespace UnityEditor.Overlays
{
    public interface ICreateToolbar
    {
        public IEnumerable<string> toolbarElements { get; }
    }

    public class OverlayToolbar : VisualElement
    {
        public OverlayToolbar()
        {
            name = "toolbar-overlay";
            EditorToolbarUtility.LoadStyleSheets("EditorToolbar", this);
            AddToClassList("unity-toolbar-overlay");
        }

        public void SetupChildrenAsButtonStrip()
        {
            EditorToolbarUtility.SetupChildrenAsButtonStrip(this);
        }
    }
}

namespace UnityEditor.Toolbars
{
    static class LegacyEditorToolbarHelpers
    {
        public const string elementClassName = "unity-editor-toolbar-element";
        public const string elementIconClassName = elementClassName + "__icon";
        public const string elementLabelClassName = elementClassName + "__label";
        public const string elementTextIconClassName = elementClassName + "__text-icon";

        public static OverlayToolbar CreateOverlay(IEnumerable<string> toolbarElementIds, EditorWindow context = null)
        {
            var root = new OverlayToolbar();

            foreach (var id in toolbarElementIds)
            {
                if (TryCreateElement(id, context, out var ve))
                    root.Add(ve);
            }

            return root;
        }
        
        static bool TryCreateElement(string id, EditorWindow ctx, out VisualElement ve)
        {
            if (EditorToolbarManager.instance.TryCreateElementFromId(ctx, id, out ve))
            {
                if (ve is IAccessContainerWindow visualWithContext)
                    visualWithContext.containerWindow = ctx;
                ve.AddToClassList(elementClassName);
                return true;
            }

            return false;
        }
    }
}

namespace UnityEditor.EditorTools
{
    // Only 2022.1 and newer have overlays for tool contexts
    [Overlay(typeof(SceneView), "Tool Context Settings", true)]
    [Icon("Icons/Overlays/ToolSettings.png")]
    public sealed class EditorToolSettingsOverlay : Overlay, ITransientOverlay, ICreateToolbar, ICreateHorizontalToolbar, ICreateVerticalToolbar
    {
        Editor m_ContextEditor;

        protected internal override Layout supportedLayouts
        {
            get
            {
                var ret = Layout.Panel;

                if (m_ContextEditor == null)
                    return ret;

                if (m_ContextEditor is ICreateHorizontalToolbar or ICreateToolbar)
                    ret |= Layout.HorizontalToolbar;

                if (m_ContextEditor is ICreateVerticalToolbar or ICreateToolbar)
                    ret |= Layout.VerticalToolbar;

                return ret;
            }
        }

        public bool visible => EditorToolManager.activeToolContext is not GameObjectToolContext;

        public EditorToolSettingsOverlay()
        {
            ToolManager.activeToolChanged += OnToolChanged;
            ToolManager.activeContextChanged += OnToolChanged;
            CreateEditor();
        }

        public override void OnWillBeDestroyed()
        {
            UnityObject.DestroyImmediate(m_ContextEditor);
        }

        void CreateEditor()
        {
            UnityObject.DestroyImmediate(m_ContextEditor);
            m_ContextEditor = Editor.CreateEditor(EditorToolManager.activeToolContext);
        }

        void OnToolChanged()
        {
            CreateEditor();
            RebuildContent();
        }

        public OverlayToolbarElement CreateHorizontalToolbarContent()
        {
            var root = new OverlayToolbar();

            if (m_ContextEditor is ICreateHorizontalToolbar ctx)
                root.Add(ctx.CreateHorizontalToolbarContent());
            else if (m_ContextEditor is ICreateToolbar toolbar)
                root.Add(LegacyEditorToolbarHelpers.CreateOverlay(toolbar.toolbarElements, containerWindow));

            return root;
        }

        public OverlayToolbarElement CreateVerticalToolbarContent()
        {
            var root = new OverlayToolbar();

            if (m_ContextEditor is ICreateVerticalToolbar ctx)
                root.Add(ctx.CreateVerticalToolbarContent());
            else if (m_ContextEditor is ICreateToolbar toolbar)
                root.Add(LegacyEditorToolbarHelpers.CreateOverlay(toolbar.toolbarElements, containerWindow));

            return root;
        }

        public IEnumerable<string> toolbarElements
        {
            get
            {
                if (m_ContextEditor is ICreateToolbar ctx)
                {
                    foreach (var id in ctx.toolbarElements)
                        yield return id;
                }
            }
        }

        VisualElement GetPanelContent(Editor editor)
        {
            if (editor == null)
                return null;

            var root = editor.CreateInspectorGUI();

            if (root != null)
                return root;

            // If the Editor does not provide an OnInspectorGUI, try to fall back to a toolbar.
            var inspector = editor.GetType().GetMethod("OnInspectorGUI",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (inspector == null || inspector.DeclaringType != editor.GetType())
            {
                if (editor is ICreateToolbar toolbar)
                    return LegacyEditorToolbarHelpers.CreateOverlay(toolbar.toolbarElements, containerWindow);

                if (editor is ICreateHorizontalToolbar horizontal)
                    return horizontal.CreateHorizontalToolbarContent();

                if (editor is ICreateVerticalToolbar vertical)
                    return vertical.CreateVerticalToolbarContent();
            }

            return new IMGUIContainer(editor.OnInspectorGUI);
        }

        public override VisualElement CreatePanelContent()
        {
            var context = GetPanelContent(m_ContextEditor);
            var root = context is OverlayToolbar
                ? new OverlayToolbar()
                : new VisualElement();
            root.Add(context);
            return root;
        }
    }
}
#endif