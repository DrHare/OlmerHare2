using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(WorldVariable))]
public class WorldVariableInspector : Editor {
	public override void OnInspectorGUI() {
        EditorGUIUtility.LookLikeControls();
		EditorGUI.indentLevel = 0;
		
		WorldVariable stat = (WorldVariable) target;
		
		LevelSettings.Instance = null; // clear cached version
		DTInspectorUtility.DrawTexture(CoreGameKitInspectorResources.logoTexture);
		
		bool isDirty = false;
		
		var newName = EditorGUILayout.TextField("Name", stat.transform.name);
		if (newName != stat.transform.name) {
			UndoHelper.RecordObjectPropertyForUndo(ref isDirty, stat.gameObject, "change Name");
			stat.transform.name = newName;
		}

		EditorGUILayout.BeginHorizontal();
		GUILayout.Label("Variable Type");
		GUILayout.Space(61);
		GUI.contentColor = Color.yellow;
		GUILayout.Label(WorldVariableTracker.GetVariableTypeFriendlyString(stat.varType));
		
		if (Application.isPlaying) {
			var sValue = "";
			
			switch (stat.varType) {
				case WorldVariableTracker.VariableType._integer:
					var _int = WorldVariableTracker.GetExistingWorldVariableIntValue(stat.name, stat.startingValue);
					sValue = _int.HasValue ? _int.Value.ToString() : "";
					break;
				case WorldVariableTracker.VariableType._float:
					var _float = WorldVariableTracker.GetExistingWorldVariableFloatValue(stat.name, stat.startingValue);
					sValue = _float.HasValue ? _float.Value.ToString() : "";
					break;
				default:	
					Debug.Log("add code for varType: " + stat.varType);
					break;
			}
			
			GUILayout.Label("Value:");
			
			GUILayout.Label(sValue);
		}
		
		GUILayout.FlexibleSpace();
		GUI.contentColor = Color.white;
		EditorGUILayout.EndHorizontal();
		
		if (Application.isPlaying) {
			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("Change Value", GUILayout.Width(100));
			GUILayout.Space(46);
			switch (stat.varType) {
				case WorldVariableTracker.VariableType._integer:
					stat.prospectiveValue = EditorGUILayout.IntField("", stat.prospectiveValue, GUILayout.Width(200));
					break;
				case WorldVariableTracker.VariableType._float:
					stat.prospectiveFloatValue = EditorGUILayout.FloatField("", stat.prospectiveFloatValue, GUILayout.Width(200));
					break;
				default:
					Debug.LogError("Add code for varType: " + stat.varType.ToString());
					break;
			}
			
			GUI.contentColor = Color.green;
			if (GUILayout.Button("Change Value", EditorStyles.toolbarButton, GUILayout.Width(120))) {
				var variable = WorldVariableTracker.GetWorldVariable(stat.name);
				
				switch (stat.varType) {
					case WorldVariableTracker.VariableType._integer:
						variable.CurrentIntValue = stat.prospectiveValue;
						break;
					case WorldVariableTracker.VariableType._float:
						variable.CurrentFloatValue = stat.prospectiveFloatValue;
						break;
					default:
						Debug.LogError("Add code for varType: " + stat.varType.ToString());
						break;
				}
			}	
			GUI.contentColor = Color.white;
			
			GUILayout.Space(10);
		
			EditorGUILayout.EndHorizontal();
		}
		
		var newPersist = (WorldVariable.StatPersistanceMode) EditorGUILayout.EnumPopup("Persistence mode", stat.persistanceMode);
		if (newPersist != stat.persistanceMode) {
            UndoHelper.RecordObjectPropertyForUndo(ref isDirty, stat, "change Persistence Mode");
			stat.persistanceMode = newPersist;
		}

        var newChange = (WorldVariable.VariableChangeMode)EditorGUILayout.EnumPopup("Modifications allowed", stat.changeMode);
        if (newChange != stat.changeMode) {
            UndoHelper.RecordObjectPropertyForUndo(ref isDirty, stat, "change Modifications allowed");
            stat.changeMode = newChange;
        }

		var newStart = EditorGUILayout.IntField("Starting value", stat.startingValue);
		if (newStart != stat.startingValue) {
            UndoHelper.RecordObjectPropertyForUndo(ref isDirty, stat, "change Starting value");
			stat.startingValue = newStart;
		}
		
		var newNeg = EditorGUILayout.Toggle("Allow negative?", stat.allowNegative);
		if (newNeg != stat.allowNegative) {
            UndoHelper.RecordObjectPropertyForUndo(ref isDirty, stat, "toggle Allow negative");
			stat.allowNegative = newNeg;
		}
		
		var newTopLimit = EditorGUILayout.Toggle("Has max value?", stat.hasMaxValue);
		if (newTopLimit != stat.hasMaxValue) {
            UndoHelper.RecordObjectPropertyForUndo(ref isDirty, stat, "toggle Has max value");
			stat.hasMaxValue = newTopLimit;
		}
		
		if (stat.hasMaxValue) {
			EditorGUI.indentLevel = 1;
			switch (stat.varType) {
				case WorldVariableTracker.VariableType._integer:						
					var newMax = EditorGUILayout.IntField("Max Value", stat.intMaxValue);
					if (newMax != stat.intMaxValue) {
                        UndoHelper.RecordObjectPropertyForUndo(ref isDirty, stat, "change Max Value");
						stat.intMaxValue = newMax;
					}
					break;
				case WorldVariableTracker.VariableType._float:						
					var newFloatMax = EditorGUILayout.FloatField("Max Value", stat.floatMaxValue);
					if (newFloatMax != stat.floatMaxValue) {
                        UndoHelper.RecordObjectPropertyForUndo(ref isDirty, stat, "change Max Value");
						stat.floatMaxValue = newFloatMax;
					}
					break;
				default:	
					Debug.Log("add code for varType: " + stat.varType);
					break;
			}
		}
		
		EditorGUI.indentLevel = 0;
		
		var newCanEnd = EditorGUILayout.Toggle("Triggers game over?", stat.canEndGame);
		if (newCanEnd != stat.canEndGame) {
            UndoHelper.RecordObjectPropertyForUndo(ref isDirty, stat, "toggle Triggers game over");
			stat.canEndGame = newCanEnd;
		}
		if (stat.canEndGame) {
			EditorGUI.indentLevel = 1;
			var newMin = EditorGUILayout.IntField("G.O. min value", stat.endGameMinValue);
			if (newMin != stat.endGameMinValue) {
                UndoHelper.RecordObjectPropertyForUndo(ref isDirty, stat, "change G.O. min value");
				stat.endGameMinValue = newMin;
			}

			var newMax = EditorGUILayout.IntField("G.O. max value", stat.endGameMaxValue);
			if (newMax != stat.endGameMaxValue) {
                UndoHelper.RecordObjectPropertyForUndo(ref isDirty, stat, "change G.O. max value");
				stat.endGameMaxValue = newMax;
			}
		}

		EditorGUI.indentLevel = 0;
		var listenerWasEmpty = stat.listenerPrefab == null;
		var newListener = (WorldVariableListener) EditorGUILayout.ObjectField("Listener", stat.listenerPrefab, typeof(WorldVariableListener), true); 
		if (newListener != stat.listenerPrefab) {
            UndoHelper.RecordObjectPropertyForUndo(ref isDirty, stat, "assign Listener");
			stat.listenerPrefab = newListener;
			if (listenerWasEmpty && stat.listenerPrefab != null) {
				// just assigned.
				var listener = stat.listenerPrefab.GetComponent<WorldVariableListener>();
				if (listener == null) {
					DTInspectorUtility.ShowAlert("You cannot assign a listener that doesn't have a WorldVariableListener script in it.");
					stat.listenerPrefab = null;
				} else {
					listener.variableName = stat.transform.name;
					listener.vType = stat.varType;
				}
			}
		}
			
		if (GUI.changed || isDirty) {
  			EditorUtility.SetDirty(target);	// or it won't save the data!!
		}

		//DrawDefaultInspector();
    }
}
