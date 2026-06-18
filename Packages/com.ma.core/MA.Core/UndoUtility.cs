// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityObject = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MA.Core
{
    /// <summary>A struct for handling undo scopes within <see cref="UndoUtility"/>.</summary>
    public readonly struct UndoScope : IDisposable
    {
#if UNITY_EDITOR
        readonly bool m_PrevUndoEnabled;
#endif

        /// <summary>Creates a new undo scope.</summary>
        /// <param name="undoEnabled">Whether undo is enabled for this scope.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UndoScope(bool undoEnabled)
        {
#if UNITY_EDITOR
            m_PrevUndoEnabled = UndoUtility.UndoEnabled;
            UndoUtility.UndoEnabled = undoEnabled;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
#if UNITY_EDITOR
            UndoUtility.UndoEnabled = m_PrevUndoEnabled;
#endif
        }
    }
    
    /// <summary>A class for handling undo operations in Flora.</summary>
    public static class UndoUtility
    {
        /// <summary>Whether or not undo is enabled.</summary>
        public static bool UndoEnabled { get; set; } = true;

        /// <summary>Destroys an object.</summary>
        /// <param name="objToDestroy">The object to destroy.</param>
        /// <param name="objToDirty">The object to mark as dirty.</param>
        [Conditional("UNITY_EDITOR")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DestroyObject(UnityObject objToDestroy, UnityObject objToDirty = null)
        {
#if UNITY_EDITOR
            if (objToDestroy == null || !UndoEnabled)
                return;

            if (objToDirty != null)
                EditorUtility.SetDirty(objToDirty);

            if (!Application.isPlaying)
                Undo.DestroyObjectImmediate(objToDestroy);
            else
                UnityObject.Destroy(objToDestroy);
#else
            if (objToDestroy != null)
                UnityObject.Destroy(objToDestroy);
#endif
        }

        /// <summary>Records an array of objects.</summary>
        /// <param name="objsToDirty">The objects to mark as dirty.</param>
        /// <param name="operation">The name of the operation.</param>
        [Conditional("UNITY_EDITOR")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RecordObject(UnityObject[] objsToDirty, string operation)
        {
#if UNITY_EDITOR
            if (objsToDirty == null || !UndoEnabled)
                return;

            for (int i = 0; i < objsToDirty.Length; i++)
                EditorUtility.SetDirty(objsToDirty[i]);

            Undo.RegisterCompleteObjectUndo(objsToDirty, UndoName(operation));
#endif
        }

        /// <summary>Records an object.</summary>
        /// <param name="objToDirty">The object to mark as dirty.</param>
        /// <param name="operation">The name of the operation.</param>
        [Conditional("UNITY_EDITOR")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RecordObject(UnityObject objToDirty, string operation)
        {
#if UNITY_EDITOR
            if (objToDirty != null && UndoEnabled)
            {
                EditorUtility.SetDirty(objToDirty);
                Undo.RegisterCompleteObjectUndo(objToDirty, UndoName(operation));
            }
#endif
        }

        /// <summary>Records an object that has just been created.</summary>
        /// <param name="objCreated">The object that was created.</param>
        /// <param name="operation">The name of the operation.</param>
        [Conditional("UNITY_EDITOR")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RegisterCreatedObjectUndo(UnityObject objCreated, string operation)
        {
#if UNITY_EDITOR
            if (objCreated != null && UndoEnabled)
            {
                Undo.RegisterCreatedObjectUndo(objCreated, UndoName(operation));
            }
#endif
        }

        /// <summary>Sets an object's parent transform.</summary>
        /// <param name="transform">The object's transform.</param>
        /// <param name="newParent">The new parent transform.</param>
        /// <param name="worldPositionStays">Whether to maintain world position.</param>
        /// <param name="operation">The name of the operation.</param>
        [Conditional("UNITY_EDITOR")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetTransformParent(Transform transform, Transform newParent, bool worldPositionStays, string operation)
        {
#if UNITY_EDITOR
            if (transform != null && newParent != null && UndoEnabled)
            {
                Undo.SetTransformParent(transform, newParent, worldPositionStays, UndoName(operation));
            }
#endif
        }

        /// <summary>Registers a full object hierarchy undo operation.</summary>
        /// <param name="objToUndo">The object to undo.</param>
        /// <param name="operation">The name of the operation.</param>
        [Conditional("UNITY_EDITOR")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RegisterFullObjectHierarchyUndo(UnityObject objToUndo, string operation)
        {
#if UNITY_EDITOR
            if (objToUndo != null && UndoEnabled)
            {
                Undo.RegisterFullObjectHierarchyUndo(objToUndo, UndoName(operation));
            }
#endif
        }

        /// <summary>Flushes undo record objects.</summary>
        [Conditional("UNITY_EDITOR")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FlushUndoRecordObjects()
        {
#if UNITY_EDITOR
            if (UndoEnabled)
            {
                Undo.FlushUndoRecordObjects();
            }
#endif
        }

        /// <summary>Sets an object as dirty.</summary>
        /// <param name="objToDirty">The object to mark as dirty.</param>
        [Conditional("UNITY_EDITOR")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetDirty(UnityObject objToDirty)
        {
#if UNITY_EDITOR
            if (objToDirty != null)
            {
                if (UnityUtility.IsMainThread)
                    EditorUtility.SetDirty(objToDirty);
            }
#endif
        }

        /// <summary>Adds a component to a game object.</summary>
        /// <param name="gameObject">The game object to add the component to.</param>
        /// <typeparam name="T">The type of component to add.</typeparam>
        /// <returns>The added component.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T AddComponent<T>(GameObject gameObject) where T : Component
        {
            if (gameObject != null)
            {
#if UNITY_EDITOR
                return UndoEnabled ? (T)Undo.AddComponent(gameObject, typeof(T)) : gameObject.AddComponent<T>();
#else
                return gameObject.AddComponent<T>();
#endif
            }

            return null;
        }

        /// <summary>Returns the name of an undo operation.</summary>
        /// <param name="name">The base name of the operation.</param>
        /// <returns>The formatted undo operation name.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static string UndoName(string name) => name + " (Flora)";
        
        /// <summary>Checks if undo is processing.</summary>
        public static bool IsProcessing
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if UNITY_EDITOR
#if UNITY_2022_3_OR_NEWER
                return Undo.isProcessing;
#else
                return s_IsUndoRedoing;
#endif
#else
                return false;
#endif
            }
        }

#if UNITY_EDITOR && !UNITY_2022_3_OR_NEWER
        [InitializeOnLoadMethod]
        static void InitializeUndoRedoHandler()
        {
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
            Undo.willFlushUndoRecord += OnWillFlushUndoRecord;
        }
        
        static bool s_IsUndoRedoing;

        static void OnUndoRedoPerformed()
        {
            s_IsUndoRedoing = true;
        }
        
        static void OnWillFlushUndoRecord()
        {
            s_IsUndoRedoing = false;
        }
#endif
    }
}