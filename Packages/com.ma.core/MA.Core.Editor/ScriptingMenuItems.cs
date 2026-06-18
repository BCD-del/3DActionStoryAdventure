// Copyright © Magnetic Arcade. All Rights Reserved.

using UnityEditor;
using UnityEditor.Compilation;

namespace MA.Core.Editor
{
    static class ScriptingUtilityMenuItems
    {
        // [MenuItem("Tools/Request Script Reload")]
        public static void TriggerDomainReload()
        {
            EditorUtility.RequestScriptReload();
        }

        // [MenuItem("Tools/Request Script Compilation")]
        public static void RequestScriptCompilation()
        {
            CompilationPipeline.RequestScriptCompilation();
        }

        // [MenuItem("Tools/Request Script Compilation Clean Build Cache")]
        public static void RequestScriptCompilationFullRebuild()
        {
            CompilationPipeline.RequestScriptCompilation(RequestScriptCompilationOptions.CleanBuildCache);
        }
    }
}
