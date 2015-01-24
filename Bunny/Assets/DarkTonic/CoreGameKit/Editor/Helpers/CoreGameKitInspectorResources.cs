using UnityEngine;
using System.Collections;
using UnityEditor;

public static class CoreGameKitInspectorResources {
	public const string CoreGameKitFolderPath = "Core GameKit";

	public static Texture logoTexture = EditorGUIUtility.LoadRequired(string.Format("{0}/inspector_header_killer_waves.png", CoreGameKitFolderPath)) as Texture;
    public static Texture settingsTexture = EditorGUIUtility.LoadRequired(string.Format("{0}/gearIcon.png", CoreGameKitFolderPath)) as Texture;
    public static Texture deleteTexture = EditorGUIUtility.LoadRequired(string.Format("{0}/deleteIcon.png", CoreGameKitFolderPath)) as Texture;
	public static Texture leftArrowTexture = EditorGUIUtility.LoadRequired(string.Format("{0}/arrow_left.png", CoreGameKitFolderPath)) as Texture;
	public static Texture rightArrowTexture = EditorGUIUtility.LoadRequired(string.Format("{0}/arrow_right.png", CoreGameKitFolderPath)) as Texture;
	public static Texture upArrowTexture = EditorGUIUtility.LoadRequired(string.Format("{0}/arrow_up.png", CoreGameKitFolderPath)) as Texture;
	public static Texture downArrowTexture = EditorGUIUtility.LoadRequired(string.Format("{0}/arrow_down.png", CoreGameKitFolderPath)) as Texture;
}
