using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public static class KillerVariablesHelper {
	private const int MOD_MODE_FIELD_WIDTH = 50;
	private const int SHORT_FIELD_WIDTH = 150;
	private const int FIELD_WIDTH = 200;
	
	public static List<string> AllStatNames {
		get {
			var allStatNames = new List<string>() {
				LevelSettings.DROP_DOWN_NONE_OPTION
			};
			
			var ls = LevelSettings.Instance;
			if (ls != null) {
				var statsHolder = ls.transform.Find(LevelSettings.WORLD_VARIABLES_CONTAINER_TRANS_NAME);
				for (var i = 0; i < statsHolder.childCount; i++) {
					var child = statsHolder.GetChild(i);
					allStatNames.Add(child.name);
				}
			}
			
			return allStatNames;
		}
	}
	
	public static List<string> AllStatNamesOfType(WorldVariableTracker.VariableType vType) {
		var allStatNames = new List<string>() {
			LevelSettings.DROP_DOWN_NONE_OPTION
		};
		
		var ls = LevelSettings.Instance;
		if (ls != null) {
			var statsHolder = ls.transform.Find(LevelSettings.WORLD_VARIABLES_CONTAINER_TRANS_NAME);
			for (var i = 0; i < statsHolder.childCount; i++) {
				var child = statsHolder.GetChild(i);
				var wv = child.GetComponent<WorldVariable>();
				if (wv.varType != vType) {
					continue;
				}
				allStatNames.Add(child.name);
			}
		}
		
		return allStatNames;
	}
	
	public static DTInspectorUtility.FunctionButtons DisplayKillerFloat(ref bool isDirty, KillerFloat killFloat, string fieldLabel, Object srcObject, bool showDeleteButton = false, bool indent = false) {
		var allStatNames = AllStatNamesOfType(WorldVariableTracker.VariableType._float);
		
		EditorGUILayout.BeginHorizontal();

		if (indent) {
			GUILayout.Space(12);
		}
		
		GUILayout.Label(fieldLabel);

		GUILayout.FlexibleSpace();
		
		var newVarIndex = -1;
		
		var unfoundVariableName = string.Empty;
		
		GUI.color = Color.green;
		GUILayout.Label(new GUIContent("F", "Float field"));
		GUI.color = Color.white;
		
		if (showDeleteButton) {
			var newModMode = (KillerVariable.ModMode) EditorGUILayout.EnumPopup("", killFloat.curModMode, GUILayout.Width(MOD_MODE_FIELD_WIDTH));	
			if (newModMode != killFloat.curModMode) {
				UndoHelper.RecordObjectPropertyForUndo(ref isDirty, srcObject, "change Mod Mode for " + fieldLabel);
				killFloat.curModMode = newModMode;
			}
		}
		
		var fieldWidth = showDeleteButton ? SHORT_FIELD_WIDTH : FIELD_WIDTH;
		
		switch (killFloat.variableSource) {
			case LevelSettings.VariableSource.Self:
				var newVal = EditorGUILayout.FloatField(killFloat.selfValue, GUILayout.Width(fieldWidth));
				if (newVal != killFloat.selfValue) {
					UndoHelper.RecordObjectPropertyForUndo(ref isDirty, srcObject, "change " + fieldLabel);
					killFloat.Value = newVal;
				}	
				break;
			case LevelSettings.VariableSource.Variable:
				var oldIndex = allStatNames.IndexOf(killFloat.worldVariableName);
				if (oldIndex < 0) {
					unfoundVariableName = killFloat.worldVariableName;	
					oldIndex = 0;
				}
			
				newVarIndex = EditorGUILayout.Popup("", oldIndex, allStatNames.ToArray(), GUILayout.Width(fieldWidth));
				if (oldIndex != newVarIndex) {
					UndoHelper.RecordObjectPropertyForUndo(ref isDirty, srcObject, "change Variable for " + fieldLabel);
					killFloat.worldVariableName = allStatNames[newVarIndex];
				}
				break;
		}
		
		GUI.backgroundColor = Color.cyan;
		var newSource = (LevelSettings.VariableSource) EditorGUILayout.EnumPopup(killFloat.variableSource, GUILayout.Width(70));
		if (newSource != killFloat.variableSource) {
			UndoHelper.RecordObjectPropertyForUndo(ref isDirty, srcObject, "change source of " + fieldLabel);
			killFloat.variableSource = newSource;
		}
		GUI.backgroundColor = Color.white;
		
		var deletePressed = false;
		
		if (showDeleteButton) {
			GUI.backgroundColor = Color.green;
			if (GUILayout.Button(new GUIContent("Delete", "Remove this mod"), EditorStyles.miniButtonMid, GUILayout.Width(64))) {
				deletePressed = true;
			}
			GUI.backgroundColor = Color.white;
			
			GUILayout.Space(10);
		}
		
		EditorGUILayout.EndHorizontal();

		if (killFloat.variableSource == LevelSettings.VariableSource.Variable && (killFloat.worldVariableName == LevelSettings.DROP_DOWN_NONE_OPTION || newVarIndex <= 0)) {
			if (string.IsNullOrEmpty(unfoundVariableName)) {
				DTInspectorUtility.ShowRedError("No variable has been selected. " + fieldLabel + " will get a value of " + KillerFloat.DefaultValue + ".");
			} else {
				DTInspectorUtility.ShowRedError("Could not find variable '" + unfoundVariableName + "' to assign to " + fieldLabel + ". Please select another.");
			}
		}
		
		return deletePressed ? DTInspectorUtility.FunctionButtons.Remove : DTInspectorUtility.FunctionButtons.None;
	}
	
	public static DTInspectorUtility.FunctionButtons DisplayKillerInt(ref bool isDirty, KillerInt killInt, string fieldLabel, Object srcObject, bool showDeleteButton = false, bool indent = false) {
		EditorGUILayout.BeginHorizontal();
		
		var allStatNames = AllStatNamesOfType(WorldVariableTracker.VariableType._integer);
		
		if (indent) {
			GUILayout.Space(12);
		}
		
		GUILayout.Label(fieldLabel);

		GUILayout.FlexibleSpace();
		
		var newVarIndex = -1;
		
		var unfoundVariableName = string.Empty;
		
		var fieldWidth = showDeleteButton ? SHORT_FIELD_WIDTH : FIELD_WIDTH;
		
		GUI.color = Color.green;
		GUILayout.Label(new GUIContent("I", "Integer field"));
		GUI.color = Color.white;

		if (showDeleteButton) {
			var newModMode = (KillerVariable.ModMode) EditorGUILayout.EnumPopup("", killInt.curModMode, GUILayout.Width(MOD_MODE_FIELD_WIDTH));	
			if (newModMode != killInt.curModMode) {
				UndoHelper.RecordObjectPropertyForUndo(ref isDirty, srcObject, "change Mod Mode for " + fieldLabel);
				killInt.curModMode = newModMode;
			}
		}
		
		switch (killInt.variableSource) {
			case LevelSettings.VariableSource.Self:
				var newVal = EditorGUILayout.IntField(killInt.selfValue, GUILayout.Width(fieldWidth));
				if (newVal != killInt.selfValue) {
					UndoHelper.RecordObjectPropertyForUndo(ref isDirty, srcObject, "change " + fieldLabel);
					killInt.Value = newVal;
				}	
				break;
			case LevelSettings.VariableSource.Variable:
				var oldIndex = allStatNames.IndexOf(killInt.worldVariableName);
				if (oldIndex < 0) {
					unfoundVariableName = killInt.worldVariableName;	
					oldIndex = 0;
				}
			
				newVarIndex = EditorGUILayout.Popup("", oldIndex, allStatNames.ToArray(), GUILayout.Width(fieldWidth));
				if (oldIndex != newVarIndex) {
					UndoHelper.RecordObjectPropertyForUndo(ref isDirty, srcObject, "change Variable for " + fieldLabel);
					killInt.worldVariableName = allStatNames[newVarIndex];
				}
				break;
		}
		
		GUI.backgroundColor = Color.cyan;
		var newSource = (LevelSettings.VariableSource) EditorGUILayout.EnumPopup(killInt.variableSource, GUILayout.Width(70));
		if (newSource != killInt.variableSource) {
			UndoHelper.RecordObjectPropertyForUndo(ref isDirty, srcObject, "change source of " + fieldLabel);
			killInt.variableSource = newSource;
		}
		GUI.backgroundColor = Color.white;
		
		var deletePressed = false;
		
		if (showDeleteButton) {
			GUI.backgroundColor = Color.green;
			if (GUILayout.Button(new GUIContent("Delete", "Remove this mod"), EditorStyles.miniButtonMid, GUILayout.Width(64))) {
				deletePressed = true;
			}
			GUI.backgroundColor = Color.white;
			
			GUILayout.Space(10);
		}
		
		EditorGUILayout.EndHorizontal();

		if (killInt.variableSource == LevelSettings.VariableSource.Variable && (killInt.worldVariableName == LevelSettings.DROP_DOWN_NONE_OPTION || newVarIndex <= 0)) {
			if (string.IsNullOrEmpty(unfoundVariableName)) {
				DTInspectorUtility.ShowRedError("No variable has been selected. " + fieldLabel + " will get a value of " + KillerInt.DefaultValue + ".");
			} else {
				DTInspectorUtility.ShowRedError("Could not find variable '" + unfoundVariableName + "' to assign to " + fieldLabel + ". Please select another.");
			}
		}
		
		return deletePressed ? DTInspectorUtility.FunctionButtons.Remove : DTInspectorUtility.FunctionButtons.None;
	}
	
	public static void ShowErrorIfMissingVariable(string varName) {
		if (!WorldVariableTracker.VariableExistsInScene(varName)) {
			DTInspectorUtility.ShowRedError(string.Format("Could not find World Variable '{0}'. Please fix.", varName));
		}
	}
}
