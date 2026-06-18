// Copyright © Magnetic Arcade. All Rights Reserved.

using System;
using UnityEditor;

namespace MA.Core.Editor.Bridge
{
    static class AnnotationUtilityBridge
    {
        internal static void GetAnnotationIdAndClass(Type type, out int id, out string klass)
        {
            var unityType = UnityType.FindTypeByName(type.Name);
            id = unityType?.persistentTypeID ?? 0;
            // In AnnotationManager, if script name is null or empty the persistent ID is used. If not, the type is
            // assumed to be a built-in type.
            klass = unityType == null ? type.Name : null;
        }

        internal static void SetGizmoEnabled(int classID, string scriptClass, int gizmoEnabled, bool addToMostRecentChanged) 
            => AnnotationUtility.SetGizmoEnabled(classID, scriptClass, gizmoEnabled, addToMostRecentChanged);

        internal static void EnableGizmosForType(Type type)
        {
            GetAnnotationIdAndClass(type, out var id, out var klass);

            var annotations = AnnotationUtility.GetAnnotations();
            foreach (var annotation in annotations)
            {
                if (annotation.scriptClass == klass || annotation.classID == id)
                {
                    SetGizmoEnabled(annotation.classID, annotation.scriptClass, 1, false);
                }
            }

            AnnotationUtility.SetGizmosDirty();
        }

        internal static void DisableGizmosForType(Type type)
        {
            GetAnnotationIdAndClass(type, out var id, out var klass);

            var annotations = AnnotationUtility.GetAnnotations();
            foreach (var annotation in annotations)
            {
                if (annotation.scriptClass == klass || annotation.classID == id)
                {
                    SetGizmoEnabled(annotation.classID, annotation.scriptClass, 0, false);
                }
            }

            AnnotationUtility.SetGizmosDirty();
        }

        internal static void SetIconEnabled(int classID, string scriptClass, int iconEnabled)
            => AnnotationUtility.SetIconEnabled(classID, scriptClass, iconEnabled);

        internal static void EnableIconsForType(Type type)
        {
            GetAnnotationIdAndClass(type, out var id, out var klass);

            var annotations = AnnotationUtility.GetAnnotations();
            foreach (var annotation in annotations)
            {
                if (annotation.scriptClass == klass || annotation.classID == id)
                {
                    SetIconEnabled(annotation.classID, annotation.scriptClass, 1);
                }
            }

            AnnotationUtility.SetGizmosDirty();
        }

        internal static void DisableIconsForType(Type type)
        {
            GetAnnotationIdAndClass(type, out var id, out var klass);

            var annotations = AnnotationUtility.GetAnnotations();
            foreach (var annotation in annotations)
            {
                if (annotation.scriptClass == klass || annotation.classID == id)
                {
                    SetIconEnabled(annotation.classID, annotation.scriptClass, 0);
                }
            }

            AnnotationUtility.SetGizmosDirty();
        }
    }
}
