using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class DTInspectorUtility {
	private const string ALERT_TITLE = "Core GameKit Alert";
	private const string ALERT_OK_TEXT = "Ok";
	
	private const string FOLD_OUT_TOOLTIP = "Click to expand or collapse";
    private const int CONTROLS_DEFAULT_LABEL_WIDTH = 140;
	
	public enum FunctionButtons { None, Add, Remove, ShiftUp, ShiftDown, Edit, DespawnAll, Rename }
	
	public static FunctionButtons AddControlButtons(LevelSettings settings, string itemName) {
		GUIContent settingsIcon = null;
		if (CoreGameKitInspectorResources.settingsTexture!= null) {
            settingsIcon = new GUIContent(CoreGameKitInspectorResources.settingsTexture, "Click to edit " + itemName);
		} else {
			settingsIcon = new GUIContent("Edit", "Click to edit " + itemName);
		}
		
		if (GUILayout.Button(settingsIcon, EditorStyles.toolbarButton)) {
			return FunctionButtons.Edit;
		}
		
		return FunctionButtons.None;
	}
	
    public static FunctionButtons AddFoldOutListItemButtons(int position, int totalPositions, string itemName, bool showAddButton, bool showMoveButtons = false)
    {
		if (Application.isPlaying) {
			return FunctionButtons.None;
		}

		if (showMoveButtons) {
			if (position > 0) {
				// the up arrow.
				var upArrow = CoreGameKitInspectorResources.upArrowTexture;
        		if (GUILayout.Button(new GUIContent(upArrow, "Click to shift " + itemName + " up"),
                                          EditorStyles.toolbarButton, GUILayout.Width(24))) {
					return FunctionButtons.ShiftUp;
				}
			} else {
				GUILayout.Space(24);
			}

			if (position < totalPositions - 1) {
	        	// The down arrow will move things towards the end of the List
				var dnArrow = CoreGameKitInspectorResources.downArrowTexture;
	        	if (GUILayout.Button(new GUIContent(dnArrow, "Click to shift " + itemName + " down"), 
					EditorStyles.toolbarButton, GUILayout.Width(24))) {
					
					return FunctionButtons.ShiftDown;
				}
			} else {
				GUILayout.Space(24);
			}
		}
		
		if (showAddButton) {
			GUI.contentColor = Color.yellow;

			var addPress = false;
			if (GUILayout.Button(new GUIContent("Add", "Click to insert " + itemName),
	                                              EditorStyles.toolbarButton, GUILayout.Width(32))) {
				addPress = true;
			}

			GUI.contentColor = Color.white;

			if (addPress) {
				return FunctionButtons.Add;
			}
		}
		
		// Remove Button - Process presses later
        var removeContent = CoreGameKitInspectorResources.deleteTexture == null ? new GUIContent("-", "Click to remove " + itemName) : new GUIContent(CoreGameKitInspectorResources.deleteTexture, "Click to remove " + itemName);
		
		if (GUILayout.Button(removeContent, EditorStyles.toolbarButton, GUILayout.Width(32))) {
			return FunctionButtons.Remove;
		}

        return FunctionButtons.None;
    }

	public static FunctionButtons AddCustomEventDeleteIcon(bool showRenameButton) {
		GUI.contentColor = Color.green;
		
		var shouldRename = false;
		if (showRenameButton) {
			shouldRename = GUILayout.Button(new GUIContent("Rename", "Click to rename Custom Event"), EditorStyles.toolbarButton, GUILayout.MaxWidth(50));
		}
		
		GUI.contentColor = Color.red;
		var shouldDelete = GUILayout.Button(new GUIContent(CoreGameKitInspectorResources.deleteTexture, "Click to delete Custom Event"), EditorStyles.toolbarButton, GUILayout.MaxWidth(50));
		GUI.contentColor = Color.white;
		
		if (shouldDelete) {
			return FunctionButtons.Remove;
		}
		if (shouldRename) {
			return FunctionButtons.Rename;
		}
		
		return FunctionButtons.None;
	}

	public static bool Foldout(bool expanded, string label)
    {
        var content = new GUIContent(label, FOLD_OUT_TOOLTIP);
        expanded = EditorGUILayout.Foldout(expanded, content);

        return expanded;
    }
	
	public static void DrawTexture(Texture tex) {
		if (tex == null) {
			Debug.Log("Logo texture missing");
			return;
		}
		
		Rect rect = GUILayoutUtility.GetRect(0f, 0f);
		rect.width = tex.width;
		rect.height = tex.height;
		GUILayout.Space(rect.height);
		GUI.DrawTexture(rect, tex);
		
		var e = Event.current;
		if (e.type == EventType.MouseUp) {
			if (rect.Contains(e.mousePosition)) {
				var ls = LevelSettings.Instance;
				if (ls != null) {
					Selection.activeObject = ls.gameObject;
				}
			}
		}
		
	}
	
	public static void ShowColorWarning(string warningText) {
		GUI.color = Color.green;
		EditorGUILayout.LabelField(warningText, EditorStyles.miniLabel);
		GUI.color = Color.white;
	}

	public static void ShowRedError(string errorText) {
		GUI.color = Color.red;
		EditorGUILayout.LabelField(errorText, EditorStyles.toolbarButton);
		GUI.color = Color.white;
	}
	
	public static bool ConfirmDialog(string text) {
		if (Application.isPlaying) {
			return true;
		}

		return EditorUtility.DisplayDialog(ALERT_TITLE, text, ALERT_OK_TEXT);
	}

	public static void ShowAlert(string text) {
		if (Application.isPlaying) {
			Debug.LogWarning(text);
		} else {
			EditorUtility.DisplayDialog(ALERT_TITLE, text, ALERT_OK_TEXT);
		}
	}

	public static void ShowLargeBarAlert(string errorText) {
		GUI.color = Color.yellow;
		EditorGUILayout.LabelField(errorText, EditorStyles.toolbarButton);
		GUI.color = Color.white;
	}
	
	private static PrefabType GetPrefabType(Object gObject) {
		return PrefabUtility.GetPrefabType(gObject);
	}
	
	public static bool IsPrefabInProjectView(Object gObject) {
		return GetPrefabType(gObject) == PrefabType.Prefab;
	}
}
