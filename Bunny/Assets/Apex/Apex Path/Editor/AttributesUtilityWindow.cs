﻿/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Editor
{
    using System;
    using System.IO;
    using System.Text;
    using Apex.Common;
    using UnityEditor;
    using UnityEngine;

    public class AttributesUtilityWindow : EditorWindow
    {
        private string[] _entries = new string[32];
        private string _fileLocation;
        private int _lastEntry;
        private bool _refreshing;
        private bool _isValid;
        private Vector2 _scrollPos;

        private void OnInspectorUpdate()
        {
            if (_refreshing)
            {
                Repaint();
            }
        }

        private void OnEnable()
        {
            _lastEntry = -1;

            if (AttributesMaster.attributesEnabled)
            {
                var vals = Enum.GetValues(AttributesMaster.attributesEnumType);
                var names = Enum.GetNames(AttributesMaster.attributesEnumType);
                int idx = 0;
                foreach (int v in vals)
                {
                    var val = Math.Abs((long)v);
                    if (val > 0)
                    {
                        _lastEntry = (int)Math.Log(val, 2);
                        _entries[_lastEntry] = names[idx];
                    }

                    idx++;
                }

                _fileLocation = LocateAttributeFile();
            }
        }

        private void OnGUI()
        {
            if (_refreshing)
            {
                if (!AttributesMaster.attributesEnabled)
                {
                    EditorGUILayout.HelpBox("Loading new Attributes file, please wait....", MessageType.Info);
                    return;
                }

                _refreshing = false;
            }

            var rect = EditorGUILayout.BeginVertical(GUILayout.Width(400), GUILayout.Height(400));
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            if (AttributesMaster.attributesEnabled)
            {
                if (string.IsNullOrEmpty(_fileLocation))
                {
                    EditorGUILayout.HelpBox("An attribute set has been defined, but it was not possible to find its file. Be sure to name the file the same as the enum.", MessageType.Info);
                    return;
                }
                else
                {
                    EditorGUILayout.HelpBox("An attribute set has been defined, and you can edit it below.\n\nPlease be aware that editing existing values will only change their name, not their value.\nTo delete a value simply remove its name.", MessageType.Info);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Welcome to the Attributes Utility.\nHere you can define an Attribute set to use with Apex products.", MessageType.Info);
            }

            DrawEntries();
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Remaining Slots:", (31 - _lastEntry).ToString());

            GUI.enabled = _isValid;
            if (AttributesMaster.attributesEnabled)
            {
                if (GUILayout.Button("Update Attribute Set"))
                {
                    WriteAttributeFile(_fileLocation, AttributesMaster.attributesEnumType.Name);
                    _refreshing = true;
                    AssetDatabase.Refresh();
                }
            }
            else
            {
                if (GUILayout.Button("Create Attribute Set"))
                {
                    _fileLocation = Path.Combine(Application.dataPath, "ApexEntityAttributes.cs");
                    WriteAttributeFile(_fileLocation, "ApexEntityAttributes");
                    _refreshing = true;
                    AssetDatabase.Refresh();
                }
            }

            GUI.enabled = true;

            EditorGUILayout.Separator();
            EditorGUILayout.EndVertical();
            this.minSize = new Vector2(rect.width, rect.height);
        }

        private void DrawEntries()
        {
            _isValid = true;
            EditorGUIUtility.labelWidth = 25f;

            EditorGUILayout.LabelField("0.", "None");

            for (int i = 0; i <= _lastEntry; i++)
            {
                _entries[i] = GetEntry(i, (i + 1) + ".", _entries[i]);
            }

            if (_lastEntry < 31 && (_lastEntry == -1 || !string.IsNullOrEmpty(_entries[_lastEntry])))
            {
                var newEntry = EditorGUILayout.TextField((_lastEntry + 2) + ".", string.Empty);
                if (!string.IsNullOrEmpty(newEntry))
                {
                    _lastEntry++;
                }
            }

            EditorGUIUtility.labelWidth = 0f;
        }

        private string GetEntry(int idx, string label, string currentValue)
        {
            var val = EditorGUILayout.TextField(label, currentValue);
            if (EditorUtilities.IsReservedWord(val))
            {
                var style = new GUIStyle(GUI.skin.label);
                style.normal.textColor = Color.red;
                EditorGUILayout.LabelField("The above word is a reserved word, please choose another.", style);
                _isValid = false;
            }
            else if (!string.IsNullOrEmpty(val) && Array.IndexOf(_entries, val) != idx)
            {
                var style = new GUIStyle(GUI.skin.label);
                style.normal.textColor = Color.red;
                EditorGUILayout.LabelField("The above word already exists in this set, please choose another.", style);
                _isValid = false;
            }

            return val;
        }

        private string LocateAttributeFile()
        {
            string name = AttributesMaster.attributesEnumType.Name + ".cs";
            var matches = Directory.GetFiles(Application.dataPath, name, SearchOption.AllDirectories);
            if (matches.Length > 0)
            {
                return matches[0];
            }

            return null;
        }

        private void WriteAttributeFile(string file, string enumName)
        {
            var b = new StringBuilder();

            for (int i = 0; i <= _lastEntry; i++)
            {
                if (!string.IsNullOrEmpty(_entries[i]))
                {
                    b.AppendFormat(Template.ItemTemplate, _entries[i], 1 << i);
                    b.AppendLine();
                }
            }

            using (var writer = new StreamWriter(file))
            {
                writer.WriteLine(Template.FileTemplate, enumName, b);
            }
        }

        private class Template
        {
            public const string ItemTemplate = "\t\t{0} = {1},";
            public const string FileTemplate = @"//------------------------------------------------------------------------------
// <auto-generated>
// This enum was auto generated by the Attributes Utility
// </auto-generated>
//------------------------------------------------------------------------------
namespace Apex.Generated
{{
    using System;
    using Apex.Common;

    [Flags, EntityAttributesEnum]
    public enum {0}
    {{
        None = 0,
{1}
    }}
}}";
        }
    }
}
