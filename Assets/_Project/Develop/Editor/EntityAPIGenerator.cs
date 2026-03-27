using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Collections;

namespace Assets._Project.Develop.Editor
{
    public class EntityAPIGenerator
    {
        private const string AssemblyName = "Assembly-CSharp";
        private const string ComponentSuffix = "C";

        private const string Component = nameof(Component);
        private const string Value = nameof(Value);

        private static string OutputPath
            => Path.Combine(Application.dataPath, "_Project/Develop/Runtime/Gameplay/EntitiesCore/Generated/EntityAPI.cs");

        [InitializeOnLoadMethod] // может багать
        [MenuItem("Tools/GenerateEntityAPI")]
        private static void Generate()
        { 
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"namespace {typeof(Entity).Namespace}");
            sb.AppendLine("{");
            sb.AppendLine($"\tpublic partial class {typeof(Entity).Name}");
            sb.AppendLine("\t{");

            Assembly assembly = Assembly.Load(AssemblyName);
            IEnumerable<Type> componentTypes = GetComponentTypesFrom(assembly);

            foreach (Type componentType in componentTypes)
            {
                string typeName = componentType.Name;
                string fullTypeName = componentType.FullName;

                string componentName = RemoveSuffixIfExists(typeName, Component);
                string modifiedComponentName = componentName + ComponentSuffix;

                // свойство на получение компонента
                sb.AppendLine($"\t\tpublic {fullTypeName} {modifiedComponentName} => GetComponent<{fullTypeName}>();");
                sb.AppendLine();

                if (HasSingleField(componentType, out FieldInfo field) && field.Name == Value)
                {
                    // свойство для получения поля из компонента
                    sb.AppendLine($"\t\tpublic {GetValidTypeName(field.FieldType)} {componentName} => {modifiedComponentName}.{field.Name};");
                    sb.AppendLine();

                    // метод TryGet
                    sb.AppendLine($"\t\tpublic bool TryGet{componentName}(out {GetValidTypeName(field.FieldType)} {GetVariableNameFrom(field.Name)})");
                    sb.AppendLine("\t\t{");
                    sb.AppendLine($"\t\t\tbool result = TryGetComponent(out {fullTypeName} component);");
                    sb.AppendLine($"\t\t\tif (result)");
                    sb.AppendLine($"\t\t\t\t{GetVariableNameFrom(field.Name)} = component.{field.Name};");
                    sb.AppendLine("\t\t\telse");
                    sb.AppendLine($"\t\t\t\t{GetVariableNameFrom(field.Name)} = default({GetValidTypeName(field.FieldType)});");
                    sb.AppendLine("\t\t\treturn result;");
                    sb.AppendLine("\t\t}");
                    sb.AppendLine();

                    // метод Add() если есть одно поле с пустым конструктором
                    if (HasEmptyConstructor(field.FieldType))
                    {
                        string initializer = "{ " + field.Name + " = new " + GetValidTypeName(field.FieldType) + "() }";

                        sb.AppendLine($"\t\tpublic {typeof(Entity).FullName} Add{componentName}()");
                        sb.AppendLine("\t\t{");
                        sb.AppendLine($"\t\t\treturn AddComponent(new {fullTypeName}() {initializer}); ");
                        sb.AppendLine("\t\t}");
                        sb.AppendLine();
                    }
                }

                // метод Add() с указанием параметров
                string componentParametrs = GetParametrs(componentType);

                sb.AppendLine($"\t\tpublic {typeof(Entity).FullName} Add{componentName}({componentParametrs})");
                sb.AppendLine("\t\t{");
                sb.AppendLine($"\t\t\treturn AddComponent(new {fullTypeName}() {GetInitializer(componentType)}); ");
                sb.AppendLine("\t\t}");
                sb.AppendLine();
            }

            sb.AppendLine("\t}");
            sb.AppendLine("}");

            File.WriteAllText(OutputPath, sb.ToString());

            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }

        private static bool HasEmptyConstructor(Type type)
        {
            return
                type.GetConstructor(Type.EmptyTypes) != null
                && type.IsSubclassOf(typeof(UnityEngine.Object)) == false;
        }

        private static string GetParametrs(Type type)
        {
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

            if (fields.Any() == false)
                return "";

            IEnumerable<string> parametrs = fields
                .Select(field => $"{GetValidTypeName(field.FieldType)} {GetVariableNameFrom(field.Name)}");

            return string.Join(",", parametrs);
        }

        private static string GetInitializer(Type type)
        {
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

            if (fields.Any() == false)
                return "";

            IEnumerable<string> initializers = fields
                .Select(field => $"{field.Name} = {GetVariableNameFrom(field.Name)}");

            return "{" + string.Join(", ", initializers) + "}";
        }

        private static string GetVariableNameFrom(string name) => char.ToLowerInvariant(name[0]) + name.Substring(1);

        private static bool HasSingleField(Type type, out FieldInfo field)
        {
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

            if (fields.Length != 1)
            {
                field = null;

                return false;
            }

            field = fields[0];
            return true;
        }

        private static string RemoveSuffixIfExists(string str, string suffix)
        {
            if (str.EndsWith(suffix))
            {
                return str.Substring(0, str.Length - suffix.Length);
            }

            return str;
        }

        private static IEnumerable<Type> GetComponentTypesFrom(Assembly assembly)
        {
            return assembly
                .GetTypes()
                .Where(type => type.IsInterface == false
                    && type.IsAbstract == false
                    && typeof(IEntityComponent).IsAssignableFrom(type));
        }

        public static string GetValidTypeName(Type type)
        {
            if (type.IsGenericType)
            {
                StringBuilder sb = new StringBuilder();

                string fullTypeName = type.FullName;
                var backtickIndex = fullTypeName.IndexOf('`');

                if (backtickIndex >= 0)
                    fullTypeName = fullTypeName.Substring(0, backtickIndex);

                sb.Append(fullTypeName);
                sb.Append("<");

                Type[] genericArgs = type.GetGenericArguments();

                for (int i = 0; i < genericArgs.Length; i++)
                {
                    if (i > 0)
                        sb.Append(", ");

                    sb.Append(GetValidTypeName(genericArgs[i]));
                }

                sb.Append(">");

                return sb.ToString();
            }
            else
            {
                return type.FullName;
            }
        }
    }
}
