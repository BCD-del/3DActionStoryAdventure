using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal; 

namespace Assets._Project.Develop.Editor
{
    public class LayersAPIGenerator
    {
        private const string Namespace = "Assets._Project.Develop.Runtime.Utilites";
        private const string ClassName = "LayersAPI";

        private static string OutputPath
            => Path.Combine(Application.dataPath, "_Project/Develop/Runtime/Utilites/Layers/LayersAPI.cs");

        [InitializeOnLoadMethod]
        [MenuItem("Tools/GenerateLayersAPI")]
        private static void Generate()
        {
            StringBuilder sb = new StringBuilder();

            string[] layers = InternalEditorUtility.layers;

            sb.AppendLine("using UnityEngine;");
            sb.AppendLine();
            sb.AppendLine($"namespace {Namespace}");
            sb.AppendLine("{");
            sb.AppendLine($"\tpublic static class {ClassName}");
            sb.AppendLine("\t{");

            foreach (string layerName in layers)
            {
                if (string.IsNullOrEmpty(layerName)) 
                    continue;

                string constName = "Layer" + layerName.Replace(" ", "");

                sb.AppendLine($"\t\tpublic static readonly int {constName} = LayerMask.NameToLayer(\"{layerName}\");");
            }

            sb.AppendLine();

            foreach (string layerName in layers)
            {
                if (string.IsNullOrEmpty(layerName)) 
                    continue;

                string indexConstName = "Layer" + layerName.Replace(" ", "");
                string maskConstName = "LayerMask" + layerName.Replace(" ", "");

                sb.AppendLine($"\t\tpublic static readonly int {maskConstName} = 1 << {indexConstName};");
            }

            sb.AppendLine();
            sb.AppendLine("\t}");
            sb.AppendLine("}");

            Directory.CreateDirectory(Path.GetDirectoryName(OutputPath));
            File.WriteAllText(OutputPath, sb.ToString());

            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }
    }
}