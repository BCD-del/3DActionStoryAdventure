using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityObject = UnityEngine.Object;

namespace MA.UIElements.Editor
{
    /// <summary>A <see cref="VisualElement"/> that displays a <see cref="UnityEditor.Editor"/> inspector.</summary>
    /// <remarks><see cref="InspectorElement"/> does not support <see cref="UnityEditor.Editor"/>s that use <see cref="Editor.OnInspectorGUI"/> with multiple targets.</remarks>
    public class EditorElement<T> : VisualElement 
        where T : UnityEditor.Editor
    {
        /// <summary>The USS class name for Foldout elements.</summary>
        public const string ClassName = "ma-editor-element";
        
        /// <summary>The USS class name for Foldout elements.</summary>
        public const string InspectorClassName = ClassName + "__inspector";
        
        /// <summary>The editor instance.</summary>
        public T Editor { get; private set; }
        
        /// <summary>The indent level of the inspector.</summary>
        public int IndentLevel { get; set; }
        
        /// <summary>Set the target for the inspector.</summary>
        public UnityObject Target
        {
            set
            {
                if (value)
                {
                    UnityEditor.Editor editor = Editor;
                    UnityEditor.Editor.CreateCachedEditor(value, typeof(T), ref editor);
                    Editor = (T)editor;
                }
                else
                {
                    Editor = null;
                }
            }
        }
        
        /// <summary>Set the targets for the inspector.</summary>
        public UnityObject[] Targets
        {
            set
            {
                if (value.Length > 0)
                {
                    UnityEditor.Editor editor = Editor;
                    UnityEditor.Editor.CreateCachedEditor(value, typeof(T), ref editor);
                    Editor = (T)editor;
                }
                else
                {
                    Editor = null;
                }
            }
        }
        
        /// <summary>Create a new <see cref="EditorElement{T}"/>.</summary>
        public EditorElement()
        {
            AddToClassList(ClassName);
            
            IMGUIContainer imguiContainer = new IMGUIContainer(OnGUI);
            imguiContainer.AddToClassList(InspectorClassName);
            imguiContainer.name = "ma-inspector";
            imguiContainer.style.overflow = Overflow.Hidden;
            Add(imguiContainer);
        }
        
        void OnGUI()
        {
            if (Editor)
            {
                if (Editor.target == null)
                    return;
                
                int originalIndentLevel = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;
                
                bool originalHierarchyMode = EditorGUIUtility.hierarchyMode;
                EditorGUIUtility.hierarchyMode = true;
                
                bool originalWideMode = EditorGUIUtility.wideMode;
                EditorGUIUtility.wideMode = true;

                try
                {
                    using (new EditorGUILayout.VerticalScope(EditorStyles.inspectorDefaultMargins))
                    {
                        Editor.OnInspectorGUI();
                    }
                }
                finally
                {
                    EditorGUI.indentLevel = originalIndentLevel;
                    EditorGUIUtility.hierarchyMode = originalHierarchyMode;
                    EditorGUIUtility.wideMode = originalWideMode;
                }
            }
        }
    }
}