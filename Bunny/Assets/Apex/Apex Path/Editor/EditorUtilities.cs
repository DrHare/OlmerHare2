namespace Apex.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using UnityEditor;
    using UnityEngine;

    public static class EditorUtilities
    {
        private static readonly HashSet<string> _keywords = new HashSet<string>()
        {
            "abstract", "as",  "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const", "continue", 
            "decimal", "default", "delegate", "do", "double", "else", "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit", "in", "int", "interface", 
            "internal", "is", "lock", "long", "namespace", "new", "null", "object", "operator", "out", "override", "params", "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "short", "sizeof", 
            "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual", "void", "volatile", "while"
        };

        public static bool IsReservedWord(string s)
        {
            return _keywords.Contains(s);
        }

        public static string SplitToWords(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return s;
            }

            var transformer = new StringBuilder();

            transformer.Append(char.ToUpper(s[0]));
            for (int i = 1; i < s.Length; i++)
            {
                if (char.IsUpper(s, i))
                {
                    transformer.Append(" ");
                }

                transformer.Append(s[i]);
            }

            return transformer.ToString();
        }

        public static void Section(string label)
        {
            if (EditorGUI.indentLevel > 0)
            {
                EditorGUI.indentLevel -= 1;
            }

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField(label);
            EditorGUI.indentLevel += 1;
        }

        public static void EnumToIntField(Rect position, SerializedProperty property, Type enumType, GUIContent label)
        {
            var currentValue = property.intValue;
            var curEnumVal = (Enum)Enum.ToObject(enumType, property.intValue);

            var newValRaw = EditorGUI.EnumMaskField(position, label, curEnumVal) as IConvertible;

            var newVal = newValRaw.ToInt32(null);
            if (newVal != currentValue)
            {
                property.intValue = newVal;
            }
        }

        public static void CreateOrUpdateAsset<T>(T obj, string assetName = "", string defaultAssetSubPath = "") where T : UnityEngine.Object
        {
            string path = AssetDatabase.GetAssetPath(obj);
            if (AssetDatabase.Contains(obj))
            {
                EditorUtility.SetDirty(obj);
            }
            else
            {
                //Have to do this rather cumbersome path construction due to the workings of the AssetDatabase methods
                path = "Assets";

                var subPath = GetSubPath(defaultAssetSubPath);
                if (!string.IsNullOrEmpty(subPath))
                {
                    path = string.Concat(path, "/", subPath);
                }

                var folderId = AssetDatabase.AssetPathToGUID(path);
                if (string.IsNullOrEmpty(folderId))
                {
                    folderId = AssetDatabase.CreateFolder("Assets", subPath);
                    path = AssetDatabase.GUIDToAssetPath(folderId);
                }

                if (string.IsNullOrEmpty(assetName))
                {
                    assetName = typeof(T).Name;
                }

                path = string.Concat(path, "/", assetName, ".asset");
                path = AssetDatabase.GenerateUniqueAssetPath(path);

                AssetDatabase.CreateAsset(obj, path);

                EditorGUIUtility.PingObject(obj);
            }

            AssetDatabase.SaveAssets();
        }

        public static void RemoveAsset<T>(T obj) where T : ScriptableObject
        {
            if (AssetDatabase.Contains(obj))
            {
                string path = AssetDatabase.GetAssetPath(obj);
                AssetDatabase.DeleteAsset(path);
            }
        }

        private static string GetSubPath(string path)
        {
            if (string.IsNullOrEmpty(path) || path.Equals("Assets", StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            return path.Trim('/', '\\');
        }
    }
}