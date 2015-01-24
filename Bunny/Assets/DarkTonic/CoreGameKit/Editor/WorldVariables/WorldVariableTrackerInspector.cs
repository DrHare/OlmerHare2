using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(WorldVariableTracker))]
public class WorldVariableTrackerInspector : Editor {
	private WorldVariableTracker holder;
	private List<WorldVariable> stats;
    private bool isDirty = false;
	
	public override void OnInspectorGUI() {
		EditorGUIUtility.LookLikeControls();
		EditorGUI.indentLevel = 0;
		
		holder = (WorldVariableTracker) target;
		
		LevelSettings.Instance = null; // clear cached version
        DTInspectorUtility.DrawTexture(CoreGameKitInspectorResources.logoTexture);

        isDirty = false;

		var isInProjectView = DTInspectorUtility.IsPrefabInProjectView(holder);
		
		if (isInProjectView) {
			DTInspectorUtility.ShowLargeBarAlert("*You have selected the WorldVariableTracker prefab in Project View.");
			DTInspectorUtility.ShowLargeBarAlert("*Please select the one in your Scene to edit.");
			return;
		} 
		
		
		stats = GetPlayerStatsFromChildren(holder.transform);
		
		Transform statToRemove = null;
		
		stats.Sort(delegate(WorldVariable x, WorldVariable y) {
			return x.name.CompareTo(y.name);	
		});
		
		EditorGUILayout.BeginHorizontal(EditorStyles.objectFieldThumb);
		var newShowNewVar = EditorGUILayout.Toggle("Show Create New", holder.showNewVarSection);
		EditorGUILayout.EndHorizontal();
		
		if (newShowNewVar != holder.showNewVarSection) {
            UndoHelper.RecordObjectPropertyForUndo(ref isDirty, holder, "toggle Show Create New");
			holder.showNewVarSection = newShowNewVar;
		}
		
		if (holder.showNewVarSection) {
			var newVarName = EditorGUILayout.TextField("New Variable Name", holder.newVariableName);
			if (newVarName != holder.newVariableName) {
                UndoHelper.RecordObjectPropertyForUndo(ref isDirty, holder, "change New Variable Name");
				holder.newVariableName = newVarName;
			}
			
			var newVarType = (WorldVariableTracker.VariableType) EditorGUILayout.EnumPopup("New Variable Type", holder.newVarType);
			if (newVarType != holder.newVarType) {
                UndoHelper.RecordObjectPropertyForUndo(ref isDirty, holder, "change New Variable Type");
				holder.newVarType = newVarType;
			}
			
			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("Actions");
			GUI.contentColor = Color.green;
			GUILayout.Space(101);
			if (GUILayout.Button("Create Variable", EditorStyles.toolbarButton, GUILayout.MaxWidth(100))) {
				CreateNewVariable(holder.newVariableName, holder.newVarType);
				isDirty = true;
			}
			GUILayout.FlexibleSpace();
			GUI.contentColor = Color.white;
			
			EditorGUILayout.EndHorizontal();
		}
		
		EditorGUILayout.BeginHorizontal(EditorStyles.objectFieldThumb);
		EditorGUILayout.LabelField("All World Variables");
		EditorGUILayout.EndHorizontal();
		
		var totalInt = stats.FindAll(delegate(WorldVariable obj) {
			return obj.varType == WorldVariableTracker.VariableType._integer;	
		});
		var totalFloat = stats.FindAll(delegate(WorldVariable obj) {
			return obj.varType == WorldVariableTracker.VariableType._float;	
		});
		
		var showIntVariable = EditorGUILayout.Toggle("Show Integers (" + totalInt.Count + ")", holder.showIntVars);
		if (showIntVariable != holder.showIntVars) {
            UndoHelper.RecordObjectPropertyForUndo(ref isDirty, holder, "toggle Show Integers");
			holder.showIntVars = showIntVariable;
		}
		
		var showFloatVariable = EditorGUILayout.Toggle("Show Floats (" + totalFloat.Count + ")", holder.showFloatVars);
		if (showFloatVariable != holder.showFloatVars) {
            UndoHelper.RecordObjectPropertyForUndo(ref isDirty, holder, "toggle Show Floats");
			holder.showFloatVars = showFloatVariable;
		}
		
		var filteredStats = new List<WorldVariable>();
		filteredStats.AddRange(stats);
		if (!holder.showIntVars) {
			filteredStats.RemoveAll(delegate(WorldVariable obj) {
				return obj.varType == WorldVariableTracker.VariableType._integer;	
			});
		}
		if (!holder.showFloatVars) {
			filteredStats.RemoveAll(delegate(WorldVariable obj) {
				return obj.varType == WorldVariableTracker.VariableType._float;	
			});
		}
		
		if (filteredStats.Count == 0) {
			DTInspectorUtility.ShowColorWarning("*You have no World Variables of the selected type(s).");
		}
		
		EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
		var newExpanded = DTInspectorUtility.Foldout(holder.worldVariablesExpanded, string.Format("World Variables ({0})", filteredStats.Count));
		if (newExpanded != holder.worldVariablesExpanded) {
			holder.worldVariablesExpanded = newExpanded;
            UndoHelper.RecordObjectPropertyForUndo(ref isDirty, holder, "toggle World Variables");
		}
		
		GUILayout.FlexibleSpace();
        // Add expand/collapse buttons if there are items in the list
		if (stats.Count > 0) {
    	    EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(100));
	        GUIContent content;
			GUI.contentColor = Color.green;
			var collapseIcon = "Collapse";
            content = new GUIContent(collapseIcon, "Click to collapse all");
            var masterCollapse = GUILayout.Button(content, EditorStyles.toolbarButton);

			var expandIcon = "Expand";
            content = new GUIContent(expandIcon, "Click to expand all");
            var masterExpand = GUILayout.Button(content, EditorStyles.toolbarButton);
			if (masterExpand) {
				ExpandCollapseAll(true);
			} 
			if (masterCollapse) {
				ExpandCollapseAll(false);
			}
			EditorGUILayout.EndHorizontal();
			GUI.contentColor = Color.white;
		}
		
		EditorGUILayout.EndHorizontal();

		if (holder.worldVariablesExpanded) {
			EditorGUILayout.BeginHorizontal();
			
			EditorGUILayout.EndHorizontal();
			
			for (var i = 0; i < filteredStats.Count; i++) {
				var aStat = filteredStats[i];

                var varDirty = false;

				EditorGUI.indentLevel = 1;
	            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
				var newExpand = DTInspectorUtility.Foldout(aStat.isExpanded, aStat.name);
				if (newExpand != aStat.isExpanded) {
                    UndoHelper.RecordObjectPropertyForUndo(ref varDirty, aStat, "toggle expand Variables");
					aStat.isExpanded = newExpand;
				}
	            
				GUILayout.FlexibleSpace();

				if (Application.isPlaying) {
					GUI.contentColor = Color.yellow;
					var sValue = "";
					
					switch (aStat.varType) {
						case WorldVariableTracker.VariableType._integer:
							var _int = WorldVariableTracker.GetExistingWorldVariableIntValue(aStat.name, aStat.startingValue);
							sValue = _int.HasValue ? _int.Value.ToString() : "";
							break;
						case WorldVariableTracker.VariableType._float:
							var _float = WorldVariableTracker.GetExistingWorldVariableFloatValue(aStat.name, aStat.startingValueFloat);
							sValue = _float.HasValue ? _float.Value.ToString() : "";
							break;
						default:	
							Debug.Log("add code for varType: " + aStat.varType);
							break;
					}
					
					EditorGUILayout.LabelField("Value: " + sValue, GUILayout.Width(120));
					GUI.contentColor = Color.white;
					GUILayout.Space(10);
				}
				
				GUI.contentColor = Color.yellow;
				GUILayout.Label(WorldVariableTracker.GetVariableTypeFriendlyString(aStat.varType));
				switch (aStat.varType) {
					case WorldVariableTracker.VariableType._float:
						GUILayout.Space(15);
						break;
				}
				GUI.contentColor = Color.white;
				var functionPressed = DTInspectorUtility.AddFoldOutListItemButtons(i, stats.Count, "variable", false, false);
	            EditorGUILayout.EndHorizontal();
	
				if (aStat.isExpanded) {
					EditorGUI.indentLevel = 0;
					
					var newName = EditorGUILayout.TextField("Name", aStat.transform.name);
					if (newName != aStat.transform.name) {
                        UndoHelper.RecordObjectPropertyForUndo(ref varDirty, aStat.gameObject, "change Name");
						aStat.transform.name = newName;
					}

					if (Application.isPlaying) {
						EditorGUILayout.BeginHorizontal();
						GUILayout.Label("Change Value", GUILayout.Width(100));
						GUILayout.Space(46);
						switch (aStat.varType) {
							case WorldVariableTracker.VariableType._integer:
								aStat.prospectiveValue = EditorGUILayout.IntField("", aStat.prospectiveValue, GUILayout.Width(200));
								break;
							case WorldVariableTracker.VariableType._float:
								aStat.prospectiveFloatValue = EditorGUILayout.FloatField("", aStat.prospectiveFloatValue, GUILayout.Width(200));
								break;
							default:
								Debug.LogError("Add code for varType: " + aStat.varType.ToString());
								break;
						}
						
						GUI.contentColor = Color.green;
						if (GUILayout.Button("Change Value", EditorStyles.toolbarButton, GUILayout.Width(120))) {
							var variable = WorldVariableTracker.GetWorldVariable(aStat.name);
							
							switch (aStat.varType) {
								case WorldVariableTracker.VariableType._integer:
									variable.CurrentIntValue = aStat.prospectiveValue;
									break;
								case WorldVariableTracker.VariableType._float:
									variable.CurrentFloatValue = aStat.prospectiveFloatValue;
									break;
								default:
									Debug.LogError("Add code for varType: " + aStat.varType.ToString());
									break;
							}
						}	
						GUI.contentColor = Color.white;
						
						GUILayout.Space(10);
					
						EditorGUILayout.EndHorizontal();
					}
					
					
					var newPersist = (WorldVariable.StatPersistanceMode) EditorGUILayout.EnumPopup("Persistence mode", aStat.persistanceMode);
					if (newPersist != aStat.persistanceMode) {
                        UndoHelper.RecordObjectPropertyForUndo(ref varDirty, aStat, "change Persistence mode");
						aStat.persistanceMode = newPersist;
					}

                    var newChange = (WorldVariable.VariableChangeMode)EditorGUILayout.EnumPopup("Modifications allowed", aStat.changeMode);
                    if (newChange != aStat.changeMode) {
                        UndoHelper.RecordObjectPropertyForUndo(ref varDirty, aStat, "change Modifications allowed");
                        aStat.changeMode = newChange;
                    }

					switch (aStat.varType) {
						case WorldVariableTracker.VariableType._integer:
							var newStart = EditorGUILayout.IntField("Starting value", aStat.startingValue);
							if (newStart != aStat.startingValue) {
                                UndoHelper.RecordObjectPropertyForUndo(ref varDirty, aStat, "change Starting value");
								aStat.startingValue = newStart;
							}
							break;
						case WorldVariableTracker.VariableType._float:
							var newStartFloat = EditorGUILayout.FloatField("Starting value", aStat.startingValueFloat);
							if (newStartFloat != aStat.startingValueFloat) {
                                UndoHelper.RecordObjectPropertyForUndo(ref varDirty, aStat, "change Starting value");
								aStat.startingValueFloat = newStartFloat;
							}
							break;
						default:	
							Debug.Log("add code for varType: " + aStat.varType);
							break;
					}
					
					var newNeg = EditorGUILayout.Toggle("Allow negative?", aStat.allowNegative);
					if (newNeg != aStat.allowNegative) {
                        UndoHelper.RecordObjectPropertyForUndo(ref varDirty, aStat, "toggle Allow negative");
						aStat.allowNegative = newNeg;
					}
	
					var newTopLimit = EditorGUILayout.Toggle("Has max value?", aStat.hasMaxValue);
					if (newTopLimit != aStat.hasMaxValue) {
                        UndoHelper.RecordObjectPropertyForUndo(ref varDirty, aStat, "toggle Has max value");
						aStat.hasMaxValue = newTopLimit;
					}
					
					if (aStat.hasMaxValue) {
						EditorGUI.indentLevel = 1;
						switch (aStat.varType) {
							case WorldVariableTracker.VariableType._integer:						
								var newMax = EditorGUILayout.IntField("Max Value", aStat.intMaxValue);
								if (newMax != aStat.intMaxValue) {
                                    UndoHelper.RecordObjectPropertyForUndo(ref varDirty, aStat, "change Max Value");
									aStat.intMaxValue = newMax;
								}
								break;
							case WorldVariableTracker.VariableType._float:						
								var newFloatMax = EditorGUILayout.FloatField("Max Value", aStat.floatMaxValue);
								if (newFloatMax != aStat.floatMaxValue) {
                                    UndoHelper.RecordObjectPropertyForUndo(ref varDirty, aStat, "change Max Value");
									aStat.floatMaxValue = newFloatMax;
								}
								break;
							default:	
								Debug.Log("add code for varType: " + aStat.varType);
								break;
						}
					}
					
					EditorGUI.indentLevel = 0;
					var newCanEnd = EditorGUILayout.Toggle("Triggers game over?", aStat.canEndGame);
					if (newCanEnd != aStat.canEndGame) {
                        UndoHelper.RecordObjectPropertyForUndo(ref varDirty, aStat, "toggle Triggers game over");
						aStat.canEndGame = newCanEnd;
					}
					if (aStat.canEndGame) {
						EditorGUI.indentLevel = 1;
						switch (aStat.varType) {
							case WorldVariableTracker.VariableType._integer:						
								var newMin = EditorGUILayout.IntField("G.O. min value", aStat.endGameMinValue);
								if (newMin != aStat.endGameMinValue) {
                                    UndoHelper.RecordObjectPropertyForUndo(ref varDirty, aStat, "change G.O. min value");
									aStat.endGameMinValue = newMin;
								} 
			
								var newMax = EditorGUILayout.IntField("G.O. max value", aStat.endGameMaxValue);
								if (newMax != aStat.endGameMaxValue) {
                                    UndoHelper.RecordObjectPropertyForUndo(ref varDirty, aStat, "change G.O. max value");
									aStat.endGameMaxValue = newMax;
								}
								break;
							case WorldVariableTracker.VariableType._float:						
								var newMinFloat = EditorGUILayout.FloatField("G.O. min value", aStat.endGameMinValueFloat);
								if (newMinFloat != aStat.endGameMinValueFloat) {
                                    UndoHelper.RecordObjectPropertyForUndo(ref varDirty, aStat, "change G.O. min value");
									aStat.endGameMinValueFloat = newMinFloat;
								} 
			
								var newMaxFloat = EditorGUILayout.FloatField("G.O. max value", aStat.endGameMaxValueFloat);
								if (newMaxFloat != aStat.endGameMaxValueFloat) {
                                    UndoHelper.RecordObjectPropertyForUndo(ref varDirty, aStat, "change G.O. max value");
									aStat.endGameMaxValueFloat = newMaxFloat;
								}
								break;
							default:	
								Debug.Log("add code for varType: " + aStat.varType);
								break;
						}
					}
	
					EditorGUI.indentLevel = 0;
					var listenerWasEmpty = aStat.listenerPrefab == null;
					var newListener = (WorldVariableListener) EditorGUILayout.ObjectField("Listener", aStat.listenerPrefab, typeof(WorldVariableListener), true); 
					if (newListener != aStat.listenerPrefab) {
                        UndoHelper.RecordObjectPropertyForUndo(ref varDirty, aStat, "assign Listener");
						aStat.listenerPrefab = newListener;
						if (listenerWasEmpty && aStat.listenerPrefab != null) {
							// just assigned.
							var listener = aStat.listenerPrefab.GetComponent<WorldVariableListener>();
							if (listener == null) {
								DTInspectorUtility.ShowAlert("You cannot assign a listener that doesn't have a WorldVariableListener script in it.");
								aStat.listenerPrefab = null;
							} else {
								listener.variableName = aStat.transform.name;
							}
						}
					}
				}
				
				switch (functionPressed) {
					case DTInspectorUtility.FunctionButtons.Remove:
						statToRemove = aStat.transform;
						break;
				}

                if (varDirty) {
                    EditorUtility.SetDirty(aStat);
                }
			}
		}
		
		if (statToRemove != null) {
			isDirty = true;
			RemoveStat(statToRemove);
		}
		
		if (GUI.changed || isDirty) {
  			EditorUtility.SetDirty(target);	// or it won't save the data!!
		}

		//DrawDefaultInspector();
    }
	
	private List<WorldVariable> GetPlayerStatsFromChildren(Transform holder) {
		var stats = new List<WorldVariable>();
		
		for (var i = 0; i < holder.childCount; i++) {
			var aTrans = holder.GetChild(i);
			
			var aStat = aTrans.GetComponent<WorldVariable>();
			if (aStat == null) {
				DTInspectorUtility.ShowColorWarning("A prefab under 'PlayerStats' named '" + aTrans.name + "' does");
				DTInspectorUtility.ShowColorWarning("not have a WorldVariable script. Please delete it.");
				continue;
			}
			
			stats.Add(aStat);
		}			
		
		return stats;
	}
	
	private void RemoveStat(Transform stat) {
		UndoHelper.DestroyForUndo(stat.gameObject);
	}
	
	private void CreateNewVariable(string varName, WorldVariableTracker.VariableType varType) {
		varName = varName.Trim();
		
		var match = holder.transform.FindChild(varName);
		if (match != null) {
			DTInspectorUtility.ShowAlert("You already have a World Variable named '" + varName + "'. Please choose a unique name.");
			return;
		}

		var newStat = (GameObject) GameObject.Instantiate(holder.statPrefab.gameObject, holder.transform.position, Quaternion.identity);
		
		UndoHelper.CreateObjectForUndo(newStat, "create World Variable");
		
		newStat.name = varName;

		var variable = newStat.GetComponent<WorldVariable>();
		variable.varType = varType;
		
		newStat.transform.parent = holder.transform;
	}
	
	private void ExpandCollapseAll(bool isExpand) {
        UndoHelper.RecordObjectPropertyForUndo(ref isDirty, holder, "toggle expand / collapse all World Variables");

		foreach (var variable in stats) {
			variable.isExpanded = isExpand;
		}
	}
}
