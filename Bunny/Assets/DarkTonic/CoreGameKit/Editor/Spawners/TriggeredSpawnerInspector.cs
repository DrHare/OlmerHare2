using UnityEditor; 
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

#if UNITY_4_6 || UNITY_5_0
    using UnityEngine.UI;
	using UnityEngine.EventSystems;
#endif

#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
	[CustomEditor(typeof(TriggeredSpawner))]
#else 
	[CustomEditor(typeof(TriggeredSpawner), true)]
#endif
public class TriggeredSpawnerInspector : Editor {
	private TriggeredSpawner settings;
	private List<string> allStats;
	private bool isDirty = false;
	private bool levelSettingsInScene = false;
	private List<string> customEventNames = null;
	private bool hasSlider = false;
	private bool hasButton = false;
	private bool hasRect = false;

	public override void OnInspectorGUI() {
        EditorGUIUtility.LookLikeControls();
		  
		settings = (TriggeredSpawner)target;

		WorldVariableTracker.ClearInGamePlayerStats();
		
		allStats = KillerVariablesHelper.AllStatNames;

		LevelSettings.Instance = null; // clear cached version

		var ls = LevelSettings.Instance;

		levelSettingsInScene = ls != null;

		if (levelSettingsInScene) {
			customEventNames = ls.CustomEventNames;
		}

        #if UNITY_4_6 || UNITY_5_0
            var showNewUIEvents = settings.unityUIMode == TriggeredSpawner.Unity_UIVersion.uGUI;
			hasSlider = settings.GetComponent<Slider>() != null;
			hasButton = settings.GetComponent<Button>() != null;
			hasRect = settings.GetComponent<RectTransform>() != null;
		#else
			var showNewUIEvents = false;
			hasSlider = false;	
			hasButton = false;
		#endif

		if (hasRect || hasButton || hasSlider || showNewUIEvents) { }

        DTInspectorUtility.DrawTexture(CoreGameKitInspectorResources.logoTexture);
		
		EditorGUI.indentLevel = 0;
		isDirty = false;

		var newLogMissing = EditorGUILayout.Toggle("Log Missing Events", settings.logMissingEvents);
		if (newLogMissing != settings.logMissingEvents) {
			UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle Log Missing Events");
			settings.logMissingEvents = newLogMissing;
		}

		var newUI = (TriggeredSpawner.Unity_UIVersion) EditorGUILayout.EnumPopup("Unity UI Version", settings.unityUIMode);
		if (newUI != settings.unityUIMode) {
			UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Unity UI Version");
			settings.unityUIMode = newUI;
		}

		var unusedEvents = getUnusedEventTypes();
		
		var newEventindex = EditorGUILayout.Popup("Event To Activate", 0, unusedEvents.ToArray());
		
		if (newEventindex > 0) {
			isDirty = true;
			ActivateEvent(newEventindex, unusedEvents);	
		}

		EditorGUILayout.Separator();

		var newActive = (LevelSettings.ActiveItemMode) EditorGUILayout.EnumPopup("Active Mode", settings.activeMode);
		if (newActive != settings.activeMode) {
			UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Active Mode");
			settings.activeMode = newActive;
		}
		
		switch (settings.activeMode) {
			case LevelSettings.ActiveItemMode.IfWorldVariableInRange:
			case LevelSettings.ActiveItemMode.IfWorldVariableOutsideRange:
				var missingStatNames = new List<string>();
				missingStatNames.AddRange(allStats);
				missingStatNames.RemoveAll(delegate(string obj) {
					return settings.activeItemCriteria.HasKey(obj);
				});
				
				var newStat = EditorGUILayout.Popup("Add Active Limit", 0, missingStatNames.ToArray());
				if (newStat != 0) {
					AddActiveLimit(missingStatNames[newStat], settings);
				}

				if (settings.activeItemCriteria.statMods.Count == 0) {
					DTInspectorUtility.ShowRedError("You have no Active Limits. Spawner will never be Active.");
				} else {
					EditorGUILayout.Separator();
					
					int? indexToDelete = null;
					
					for (var j = 0; j < settings.activeItemCriteria.statMods.Count; j++) {
						var modifier = settings.activeItemCriteria.statMods[j];
						EditorGUILayout.BeginHorizontal();
						GUILayout.Space(15);
						var statName = modifier._statName;	
						GUILayout.Label(statName);
					
						GUILayout.FlexibleSpace();
						GUILayout.Label("Min");
						
						switch (modifier._varTypeToUse) {
							case WorldVariableTracker.VariableType._integer:
								var newMin = EditorGUILayout.IntField(modifier._modValueIntMin, GUILayout.MaxWidth(60));
								if (newMin != modifier._modValueIntMin) {
									UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Modifier Min");
									modifier._modValueIntMin = newMin;
								}
		
								GUILayout.Label("Max");
								var newMax = EditorGUILayout.IntField(modifier._modValueIntMax, GUILayout.MaxWidth(60));
								if (newMax != modifier._modValueIntMax) {
									UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Modifier Max");
									modifier._modValueIntMax = newMax;
								}
								break;
							case WorldVariableTracker.VariableType._float:
								var newMinFloat = EditorGUILayout.FloatField(modifier._modValueFloatMin, GUILayout.MaxWidth(60));
								if (newMinFloat != modifier._modValueFloatMin) {
									UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Modifier Min");
									modifier._modValueFloatMin = newMinFloat;
								}
		
								GUILayout.Label("Max");
								var newMaxFloat = EditorGUILayout.FloatField(modifier._modValueFloatMax, GUILayout.MaxWidth(60));
								if (newMaxFloat != modifier._modValueFloatMax) {
									UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Modifier Max");
									modifier._modValueFloatMax = newMaxFloat;
								}
								break;
							default:
								Debug.LogError("Add code for varType: " + modifier._varTypeToUse.ToString());
								break;
						}
						GUI.backgroundColor = Color.green;
						if (GUILayout.Button(new GUIContent("Delete", "Remove this limit"), EditorStyles.miniButtonMid, GUILayout.MaxWidth(64))) {
							indexToDelete = j;
						}
						GUI.backgroundColor = Color.white;
						GUILayout.Space(5);
						EditorGUILayout.EndHorizontal();
					
						KillerVariablesHelper.ShowErrorIfMissingVariable(modifier._statName);
					
						var min = modifier._varTypeToUse == WorldVariableTracker.VariableType._integer ? modifier._modValueIntMin : modifier._modValueFloatMin;
						var max = modifier._varTypeToUse == WorldVariableTracker.VariableType._integer ? modifier._modValueIntMax : modifier._modValueFloatMax;
					
						if (min > max) {
							DTInspectorUtility.ShowRedError(modifier._statName + " Min cannot exceed Max, please fix!");
						}
					}
					
					DTInspectorUtility.ShowColorWarning("  *Limits are inclusive: i.e. 'Above' means >=");
					if (indexToDelete.HasValue) {
						UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "delete Modifier");
						settings.activeItemCriteria.DeleteByIndex(indexToDelete.Value);
					}
					
					EditorGUILayout.Separator();
				}
			
				break;
		}
		
		var childSpawnerCount = TriggeredSpawner.GetChildSpawners(settings.transform).Count;
		
		var newSource = (TriggeredSpawner.SpawnerEventSource) EditorGUILayout.EnumPopup("Trigger Source", settings.eventSourceType);
		if (newSource != settings.eventSourceType) {
			UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Trigger Source");
			settings.eventSourceType = newSource;
		}

        if (settings.eventSourceType == TriggeredSpawner.SpawnerEventSource.ReceiveFromParent && settings.transform.parent == null) {
            DTInspectorUtility.ShowRedError("Illegal Trigger Source - this prefab has no parent.");
        }

		if (childSpawnerCount > 0) {
			var newTransmit = EditorGUILayout.Toggle("Propagate Triggers", settings.transmitEventsToChildren);
			if (newTransmit != settings.transmitEventsToChildren) {
				UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle Propagate Triggers");
				settings.transmitEventsToChildren = newTransmit;
			}
		} else {
			DTInspectorUtility.ShowLargeBarAlert("*Cannot propagate events with no child spawners");
		}

		var newGO = (TriggeredSpawner.GameOverBehavior) EditorGUILayout.EnumPopup("Game Over Behavior", settings.gameOverBehavior);
		if (newGO != settings.gameOverBehavior) {
			UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Game Over Behavior");
			settings.gameOverBehavior = newGO;
		}

		var newPause = (TriggeredSpawner.WavePauseBehavior) EditorGUILayout.EnumPopup("Wave Pause Behavior", settings.wavePauseBehavior);
		if (newPause != settings.wavePauseBehavior) {
			UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Wave Pause Behavior");
			settings.wavePauseBehavior = newPause;
		}
		
		var newUseLayer = (WaveSyncroPrefabSpawner.SpawnLayerTagMode) EditorGUILayout.EnumPopup("Spawn Layer Mode", settings.spawnLayerMode);
		if (newUseLayer != settings.spawnLayerMode) {
			UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle Spawn Layer Mode");
			settings.spawnLayerMode = newUseLayer;
		}
		
		if (settings.spawnLayerMode == WaveSyncroPrefabSpawner.SpawnLayerTagMode.Custom) {
	        EditorGUI.indentLevel = 1;
			
			var newCustomLayer = EditorGUILayout.LayerField("Custom Spawn Layer", settings.spawnCustomLayer);
			if (newCustomLayer != settings.spawnCustomLayer) {
				UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Custom Spawn Layer");
				settings.spawnCustomLayer = newCustomLayer;
			}
		}
		
        EditorGUI.indentLevel = 0;
		var newUseTag = (WaveSyncroPrefabSpawner.SpawnLayerTagMode) EditorGUILayout.EnumPopup("Spawn Tag Mode", settings.spawnTagMode);
		if (newUseTag != settings.spawnTagMode) {
			UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle Spawn Tag Mode");
			settings.spawnTagMode = newUseTag;
		}

		if (settings.spawnTagMode == WaveSyncroPrefabSpawner.SpawnLayerTagMode.Custom) {
	        EditorGUI.indentLevel = 1;
			var newCustomTag = EditorGUILayout.TagField("Custom Spawn Tag", settings.spawnCustomTag);
			if (newCustomTag != settings.spawnCustomTag) {
				UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Custom Spawn Tag");
				settings.spawnCustomTag = newCustomTag;
			}
		}
		
        EditorGUI.indentLevel = 0;
		var hadNoListener = settings.listener == null;
		var newListener = (TriggeredSpawnerListener) EditorGUILayout.ObjectField("Listener", settings.listener, typeof(TriggeredSpawnerListener), true);
		if (newListener != settings.listener) {
			UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "assign Listener");
			settings.listener = newListener;
			if (hadNoListener && settings.listener != null) {
				settings.listener.sourceSpawnerName = settings.transform.name;
			}
		}

		EditorGUILayout.Separator();
		
		EditorGUI.indentLevel = -1;
		
		if (settings.enableWave.enableWave) {
			RenderTriggeredWave(settings.enableWave, "Enabled Event", TriggeredSpawner.EventType.OnEnabled);
		}
		if (settings.disableWave.enableWave) {
			RenderTriggeredWave(settings.disableWave, "Disabled Event", TriggeredSpawner.EventType.OnDisabled);
		}
		if (settings.visibleWave.enableWave) {
			RenderTriggeredWave(settings.visibleWave, "Visible Event", TriggeredSpawner.EventType.Visible);
		}
		if (settings.invisibleWave.enableWave) {
			RenderTriggeredWave(settings.invisibleWave, "Invisible Event", TriggeredSpawner.EventType.Invisible);
		}
		if (settings.mouseOverWave.enableWave) {
			RenderTriggeredWave(settings.mouseOverWave, "Mouse Over (Legacy) Event", TriggeredSpawner.EventType.MouseOver_Legacy);
		}
		if (settings.mouseClickWave.enableWave) {
			RenderTriggeredWave(settings.mouseClickWave, "Mouse Click (Legacy) Event", TriggeredSpawner.EventType.MouseClick_Legacy);
		}

#if UNITY_4_6 || UNITY_5_0
        if (showNewUIEvents) {
			if (hasSlider && settings.unitySliderChangedWave.enableWave) {
				RenderTriggeredWave(settings.unitySliderChangedWave, "Slider Changed (uGUI) Event", TriggeredSpawner.EventType.SliderChanged_uGUI);
			}
			if (hasButton && settings.unityButtonClickedWave.enableWave) {
				RenderTriggeredWave(settings.unityButtonClickedWave, "Button Click (uGUI) Event", TriggeredSpawner.EventType.ButtonClicked_uGUI);
			}

			if (hasRect) {
				if (settings.unityPointerDownWave.enableWave) {
					RenderTriggeredWave(settings.unityPointerDownWave, "Pointer Down (uGUI) Event", TriggeredSpawner.EventType.PointerDown_uGUI);
				}
				if (settings.unityPointerUpWave.enableWave) {
					RenderTriggeredWave(settings.unityPointerUpWave, "Pointer Up (uGUI) Event", TriggeredSpawner.EventType.PointerUp_uGUI);
				}
				if (settings.unityPointerEnterWave.enableWave) {
					RenderTriggeredWave(settings.unityPointerEnterWave, "Pointer Enter (uGUI) Event", TriggeredSpawner.EventType.PointerEnter_uGUI);
				}
				if (settings.unityPointerExitWave.enableWave) {
					RenderTriggeredWave(settings.unityPointerExitWave, "Pointer Exit (uGUI) Event", TriggeredSpawner.EventType.PointerExit_uGUI);
				}
				if (settings.unityDragWave.enableWave) {
					RenderTriggeredWave(settings.unityDragWave, "Drag (uGUI) Event", TriggeredSpawner.EventType.Drag_uGUI);
				}
				if (settings.unityDropWave.enableWave) {
					RenderTriggeredWave(settings.unityDropWave, "Drop (uGUI) Event", TriggeredSpawner.EventType.Drop_uGUI);
				}
				if (settings.unityScrollWave.enableWave) {
					RenderTriggeredWave(settings.unityScrollWave, "Scroll (uGUI) Event", TriggeredSpawner.EventType.Scroll_uGUI);
				}
				if (settings.unityUpdateSelectedWave.enableWave) {
					RenderTriggeredWave(settings.unityUpdateSelectedWave, "Update Selected (uGUI) Event", TriggeredSpawner.EventType.UpdateSelected_uGUI);
				}
				if (settings.unitySelectWave.enableWave) {
					RenderTriggeredWave(settings.unitySelectWave, "Select (uGUI) Event", TriggeredSpawner.EventType.Select_uGUI);
				}
				if (settings.unityDeselectWave.enableWave) {
					RenderTriggeredWave(settings.unityDeselectWave, "Deselect (uGUI) Event", TriggeredSpawner.EventType.Deselect_uGUI);
				}
				if (settings.unityMoveWave.enableWave) {
					RenderTriggeredWave(settings.unityMoveWave, "Move (uGUI) Event", TriggeredSpawner.EventType.Move_uGUI);
				}
				if (settings.unityInitializePotentialDragWave.enableWave) {
					RenderTriggeredWave(settings.unityInitializePotentialDragWave, "Init. Potential Drag (uGUI) Event", TriggeredSpawner.EventType.InitializePotentialDrag_uGUI);
				}
				if (settings.unityBeginDragWave.enableWave) {
					RenderTriggeredWave(settings.unityBeginDragWave, "Begin Drag (uGUI) Event", TriggeredSpawner.EventType.BeginDrag_uGUI);
				}
				if (settings.unityEndDragWave.enableWave) {
					RenderTriggeredWave(settings.unityEndDragWave, "End Drag (uGUI) Event", TriggeredSpawner.EventType.EndDrag_uGUI);
				}
				if (settings.unitySubmitWave.enableWave) {
					RenderTriggeredWave(settings.unitySubmitWave, "Submit (uGUI) Event", TriggeredSpawner.EventType.Submit_uGUI);
				}
				if (settings.unityCancelWave.enableWave) {
					RenderTriggeredWave(settings.unityCancelWave, "Cancel (uGUI) Event", TriggeredSpawner.EventType.Cancel_uGUI);
				}
			}
		}
#endif

		if (settings.collisionWave.enableWave) {
			RenderTriggeredWave(settings.collisionWave, "Collision Enter Event", TriggeredSpawner.EventType.OnCollision);
		}
		if (settings.triggerEnterWave.enableWave) {
			RenderTriggeredWave(settings.triggerEnterWave, "Trigger Enter Event", TriggeredSpawner.EventType.OnTriggerEnter);
		}
		if (settings.triggerExitWave.enableWave) {
			RenderTriggeredWave(settings.triggerExitWave, "Trigger Exit Event", TriggeredSpawner.EventType.OnTriggerExit);
		}

		#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
			// not supported
		#else
			// Unity 4.3 Events
			if (settings.collision2dWave.enableWave) {
				RenderTriggeredWave(settings.collision2dWave, "2D Collision Enter Event", TriggeredSpawner.EventType.OnCollision2D);
			}

			if (settings.triggerEnter2dWave.enableWave) {
				RenderTriggeredWave(settings.triggerEnter2dWave, "2D Trigger Enter Event", TriggeredSpawner.EventType.OnTriggerEnter2D);
			}

			if (settings.triggerExit2dWave.enableWave) {
				RenderTriggeredWave(settings.triggerExit2dWave, "2D Trigger Exit Event", TriggeredSpawner.EventType.OnTriggerExit2D);
			}

		#endif

		// code triggered event
		if (settings.codeTriggeredWave1.enableWave) {
			RenderTriggeredWave(settings.codeTriggeredWave1, "Code-Triggered Event 1", TriggeredSpawner.EventType.CodeTriggered1);
		}
		if (settings.codeTriggeredWave2.enableWave) {
			RenderTriggeredWave(settings.codeTriggeredWave2, "Code-Triggered Event 2", TriggeredSpawner.EventType.CodeTriggered2);
		}

        // Pool Boss & Pool Manager events (same for both).
		if (settings.spawnedWave.enableWave) {
			RenderTriggeredWave(settings.spawnedWave, "Spawned Event", TriggeredSpawner.EventType.OnSpawned);
		}
		if (settings.despawnedWave.enableWave) {
			RenderTriggeredWave(settings.despawnedWave, "Despawned Event", TriggeredSpawner.EventType.OnDespawned);
		}
		
		// NGUI events
		if (settings.clickWave.enableWave) {
			RenderTriggeredWave(settings.clickWave, "NGUI OnClick Event", TriggeredSpawner.EventType.OnClick_NGUI);
		}

		for (var i = 0; i < settings.userDefinedEventWaves.Count; i++) {
			var aWave = settings.userDefinedEventWaves[i];
			RenderTriggeredWave(aWave, "Custom Event", TriggeredSpawner.EventType.CustomEvent, i);
		}

		if (GUI.changed || isDirty) {
  			EditorUtility.SetDirty(target);	// or it won't save the data!!
		}

		//DrawDefaultInspector();
    }
	
	private void RenderTriggeredWave(TriggeredWaveSpecifics waveSetting, string toggleText, TriggeredSpawner.EventType eventType, int? itemIndex = null) {
		var disabledText = string.Empty;

		if (settings.activeMode == LevelSettings.ActiveItemMode.Never) {
			disabledText = " - DISABLED";
		}

		toggleText += disabledText;

		EditorGUI.indentLevel = 0;
        EditorGUILayout.BeginHorizontal(EditorStyles.objectFieldThumb);

		if (settings.activeMode == LevelSettings.ActiveItemMode.Never) {
			EditorGUILayout.LabelField(toggleText);
			EditorGUILayout.EndHorizontal();
			return;
		} else {
			if (eventType == TriggeredSpawner.EventType.CustomEvent) {
				var newUse = EditorGUILayout.Toggle(toggleText, waveSetting.customEventActive);
				if (newUse != waveSetting.customEventActive) {
					UndoHelper.RecordObjectPropertyForUndo (ref isDirty, settings, "toggle Custom Event active");
					waveSetting.customEventActive = newUse;
				}

				var buttonPressed = DTInspectorUtility.AddCustomEventDeleteIcon(false);
		
				switch (buttonPressed) {
					case DTInspectorUtility.FunctionButtons.Remove:
						UndoHelper.RecordObjectPropertyForUndo (ref isDirty, settings, "delete Custom Event Sound");
						settings.userDefinedEventWaves.RemoveAt(itemIndex.Value);
						waveSetting.customEventActive = false;
						break;
				}
			} else {
				var newEnable = EditorGUILayout.Toggle(toggleText, waveSetting.enableWave);
				if (newEnable != waveSetting.enableWave) {
					UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "disable " + eventType.ToString() + " event");
					waveSetting.enableWave = newEnable;
				}
			}
		}

        EditorGUILayout.EndHorizontal();
	
		if (eventType == TriggeredSpawner.EventType.CustomEvent) {
			if (levelSettingsInScene) {
				var existingIndex = customEventNames.IndexOf(waveSetting.customEventName);

				int? customEventIndex = null;
				
				EditorGUI.indentLevel = 0;
				
				var noEvent = false;
				var noMatch = false;
				
				if (existingIndex >= 1) {
					customEventIndex = EditorGUILayout.Popup("Custom Event Name", existingIndex, customEventNames.ToArray());
					if (existingIndex == 1) {
						noEvent = true;
					}
				} else if (existingIndex == -1 && waveSetting.customEventName == LevelSettings.NO_EVENT_NAME) {
					customEventIndex = EditorGUILayout.Popup("Custom Event Name", existingIndex, customEventNames.ToArray());
				} else { // non-match
					noMatch = true;
					var newEventName = EditorGUILayout.TextField("Custom Event Name", waveSetting.customEventName);
					if (newEventName != waveSetting.customEventName) {
						UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Custom Event Name");
						waveSetting.customEventName = newEventName;
					}
					
					var newIndex = EditorGUILayout.Popup("All Custom Events", -1, customEventNames.ToArray());
					if (newIndex >= 0) {
						customEventIndex = newIndex;
					}
				}
				
				if (noEvent) {
					DTInspectorUtility.ShowRedError("No Custom Event specified. This section will do nothing.");
				} else if (noMatch) {
					DTInspectorUtility.ShowRedError("Custom Event found no match. Type in or choose one.");
				}
				
				if (customEventIndex.HasValue) {
					if (existingIndex != customEventIndex.Value) {
						UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Custom Event");
					}
					if (customEventIndex.Value == -1) {
						waveSetting.customEventName = LevelSettings.NO_EVENT_NAME;
					} else {
						waveSetting.customEventName = customEventNames[customEventIndex.Value];
					}
				}
			} else {
				var newCustomEvent = EditorGUILayout.TextField("Custom Event Name", waveSetting.customEventName);
				if (newCustomEvent != waveSetting.customEventName) {
					UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "Custom Event Name");
					waveSetting.customEventName = newCustomEvent;
				}
			}
		}

		var poolNames = PoolNames;

		if (waveSetting.enableWave) {
			var newSource = (WaveSpecifics.SpawnOrigin) EditorGUILayout.EnumPopup("Prefab Type", waveSetting.spawnSource);
			if (newSource != waveSetting.spawnSource) {
				UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Prefab Type");
				waveSetting.spawnSource = newSource;
			}
			switch (waveSetting.spawnSource) {
				case WaveSpecifics.SpawnOrigin.Specific:
					var newSpecific = (Transform) EditorGUILayout.ObjectField("Prefab To Spawn", waveSetting.prefabToSpawn, typeof(Transform), true);
					if (newSpecific != waveSetting.prefabToSpawn) {
						UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Prefab To Spawn");
						waveSetting.prefabToSpawn = newSpecific;
					}
				
					if (waveSetting.prefabToSpawn == null) {
						DTInspectorUtility.ShowRedError("Please specify a prefab to spawn.");
					}
					break;
				case WaveSpecifics.SpawnOrigin.PrefabPool:
					if (poolNames != null) {
						var pool = LevelSettings.GetFirstMatchingPrefabPool(waveSetting.prefabPoolName);
						var noPoolSelected = false;
						var illegalPool = false;
						var noPools = false;
					
						if (pool == null) {
							if (string.IsNullOrEmpty(waveSetting.prefabPoolName)) {
								noPoolSelected = true;
							} else {
								illegalPool = true;
							}
							waveSetting.prefabPoolIndex = 0;
						} else {
							waveSetting.prefabPoolIndex = poolNames.IndexOf(waveSetting.prefabPoolName);
						}

						if (poolNames.Count > 1) {
							var newPool = EditorGUILayout.Popup("Prefab Pool", waveSetting.prefabPoolIndex, poolNames.ToArray());
							if (newPool != waveSetting.prefabPoolIndex) {
								UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Prefab Pool");
								waveSetting.prefabPoolIndex = newPool;
							}
						
							if (waveSetting.prefabPoolIndex > 0) {						
								var matchingPool = 	LevelSettings.GetFirstMatchingPrefabPool(poolNames[waveSetting.prefabPoolIndex]);
								if (matchingPool != null) {	
									waveSetting.prefabPoolName = matchingPool.name;
								}
							} else {
								waveSetting.prefabPoolName = string.Empty;
							}
						} else {
							noPools = true;
						}
					
						if (noPools) {
							DTInspectorUtility.ShowRedError("You have no Prefab Pools. Create one first.");
						} else if (noPoolSelected) {
							DTInspectorUtility.ShowRedError("No Prefab Pool selected.");
						} else if (illegalPool) {
							DTInspectorUtility.ShowRedError("Prefab Pool '" + waveSetting.prefabPoolName + "' not found. Select one.");						
						}
					} else {
						DTInspectorUtility.ShowRedError(LevelSettings.NO_PREFAB_POOLS_CONTAINER_ALERT);
						DTInspectorUtility.ShowRedError(LevelSettings.REVERT_LEVEL_SETTINGS_ALERT);
					}
				
					break;
			}
			
			KillerVariablesHelper.DisplayKillerInt(ref isDirty, waveSetting.NumberToSpwn, "Min To Spawn", settings, false, false);
			KillerVariablesHelper.DisplayKillerInt(ref isDirty, waveSetting.MaxToSpawn, "Max To Spawn", settings, false, false);

			if (!TriggeredSpawner.eventsWithInflexibleWaveLength.Contains(eventType)) {
				KillerVariablesHelper.DisplayKillerFloat(ref isDirty, waveSetting.TimeToSpawnEntireWave, "Time To Spawn All", settings);
			} 
			
			if (!TriggeredSpawner.eventsWithInflexibleWaveLength.Contains(eventType)) {
				KillerVariablesHelper.DisplayKillerFloat(ref isDirty, waveSetting.WaveDelaySec, "Delay Wave (sec)", settings);
			} 
			

			var newEx = EditorGUILayout.BeginToggleGroup("Position Settings", waveSetting.positionExpanded);
            if (newEx != waveSetting.positionExpanded) {
                UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle expand Position Settings");
                waveSetting.positionExpanded = newEx;
            }
			
			if (waveSetting.positionExpanded) {
				var newX = (WaveSpecifics.PositionMode) EditorGUILayout.EnumPopup("X Position Mode", waveSetting.positionXmode);
				if (newX != waveSetting.positionXmode) {
					UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change X Position Mode");
					waveSetting.positionXmode = newX;
				}
	
				if (waveSetting.positionXmode == WaveSpecifics.PositionMode.CustomPosition) {
					KillerVariablesHelper.DisplayKillerInt(ref isDirty, waveSetting.customPosX, "X Position", settings);
				}
	
				var newY = (WaveSpecifics.PositionMode) EditorGUILayout.EnumPopup("Y Position Mode", waveSetting.positionYmode);
				if (newY != waveSetting.positionYmode) {
					UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Y Position Mode");
					waveSetting.positionYmode = newY;
				}
	
				if (waveSetting.positionYmode == WaveSpecifics.PositionMode.CustomPosition) {
					KillerVariablesHelper.DisplayKillerInt(ref isDirty, waveSetting.customPosY, "Y Position", settings);
				}
	
				var newZ = (WaveSpecifics.PositionMode) EditorGUILayout.EnumPopup("Z Position Mode", waveSetting.positionZmode);
				if (newZ != waveSetting.positionZmode) {
					UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Z Position Mode");
					waveSetting.positionZmode = newZ;
				}
	
				if (waveSetting.positionZmode == WaveSpecifics.PositionMode.CustomPosition) {
					KillerVariablesHelper.DisplayKillerInt(ref isDirty, waveSetting.customPosZ, "Z Position", settings);
				}
				
				var newOffset = EditorGUILayout.Vector3Field("Wave Offset", waveSetting.waveOffset);
				if (newOffset != waveSetting.waveOffset) {
					UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Wave Offset");
					waveSetting.waveOffset = newOffset;
				}

				EditorGUILayout.Separator();
			}			
			
			EditorGUILayout.EndToggleGroup();
			
			if (waveSetting.isCustomEvent) {
				var newLookAt = (WaveSpecifics.SpawnerRotationMode) EditorGUILayout.EnumPopup("Spawner Rotation Mode", waveSetting.curSpawnerRotMode);
				if (newLookAt != waveSetting.curSpawnerRotMode) {
					UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Spawner Rotation Mode");
					waveSetting.curSpawnerRotMode = newLookAt;
				}
			}
	
			var newRotation = (WaveSpecifics.RotationMode) EditorGUILayout.EnumPopup("Spawn Rotation Mode", waveSetting.curRotationMode);
			if (newRotation != waveSetting.curRotationMode) {
				UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Rotation Mode");
				waveSetting.curRotationMode = newRotation;
			}
			
			if (waveSetting.curRotationMode == WaveSpecifics.RotationMode.LookAtCustomEventOrigin) {
				if (!waveSetting.isCustomEvent) {
					DTInspectorUtility.ShowRedError("Look At Custom Event Origin rotation mode is only valid for Custom Events.");
				} else {
			        EditorGUI.indentLevel = 1;
					
					var ignoreX = EditorGUILayout.Toggle("Ignore Origin X", waveSetting.eventOriginIgnoreX);
					if (ignoreX != waveSetting.eventOriginIgnoreX) {
						UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle Ignore Origin X");
						waveSetting.eventOriginIgnoreX = ignoreX;
					}
					
					var ignoreY = EditorGUILayout.Toggle("Ignore Origin Y", waveSetting.eventOriginIgnoreY);
					if (ignoreY != waveSetting.eventOriginIgnoreY) {
						UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle Ignore Origin Y");
						waveSetting.eventOriginIgnoreY = ignoreY;
					}

					var ignoreZ = EditorGUILayout.Toggle("Ignore Origin Z", waveSetting.eventOriginIgnoreZ);
					if (ignoreZ != waveSetting.eventOriginIgnoreZ) {
						UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle Ignore Origin Z");
						waveSetting.eventOriginIgnoreZ = ignoreZ;
					}
				}
			}
			
	        EditorGUI.indentLevel = 0;
			if (waveSetting.curRotationMode == WaveSpecifics.RotationMode.CustomRotation) {
				var newCust = EditorGUILayout.Vector3Field("Custom Rotation Euler", waveSetting.customRotation);
				if (newCust != waveSetting.customRotation) {
					UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Custom Rotation Euler");
					waveSetting.customRotation = newCust;
				}
			}
			
			var newDisable = false;
			
			switch (eventType) {
				case TriggeredSpawner.EventType.Visible:
					newDisable = EditorGUILayout.Toggle("Stop On Invisible", waveSetting.stopWaveOnOppositeEvent);
					if (newDisable != waveSetting.stopWaveOnOppositeEvent) {
						UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle Stop On Invisible");
						waveSetting.stopWaveOnOppositeEvent = newDisable;	
					}
					break;
				case TriggeredSpawner.EventType.OnTriggerEnter:
					newDisable = EditorGUILayout.Toggle("Stop When Trigger Exit", waveSetting.stopWaveOnOppositeEvent);
					if (newDisable != waveSetting.stopWaveOnOppositeEvent) {
						UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle Stop On Trigger Exit");
						waveSetting.stopWaveOnOppositeEvent = newDisable;	
					}
					break;
				case TriggeredSpawner.EventType.OnTriggerExit:
					newDisable = EditorGUILayout.Toggle("Stop When Trigger Enter", waveSetting.stopWaveOnOppositeEvent);
					if (newDisable != waveSetting.stopWaveOnOppositeEvent) {
						UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle Stop On Trigger Enter");
						waveSetting.stopWaveOnOppositeEvent = newDisable;	
					}
					break;
				case TriggeredSpawner.EventType.OnTriggerEnter2D:
					newDisable = EditorGUILayout.Toggle("Stop When Trigger Exit 2D", waveSetting.stopWaveOnOppositeEvent);
					if (newDisable != waveSetting.stopWaveOnOppositeEvent) {
						UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle Stop On Trigger Exit 2D");
						waveSetting.stopWaveOnOppositeEvent = newDisable;	
					}
					break;
				case TriggeredSpawner.EventType.OnTriggerExit2D:
					newDisable = EditorGUILayout.Toggle("Stop When Trigger Enter 2D", waveSetting.stopWaveOnOppositeEvent);
					if (newDisable != waveSetting.stopWaveOnOppositeEvent) {
						UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle Stop On Trigger Enter 2D");
						waveSetting.stopWaveOnOppositeEvent = newDisable;	
					}
					break;
			}
			
			if (!waveSetting.disableAfterFirstTrigger) {
				if (!TriggeredSpawner.eventsWithInflexibleWaveLength.Contains(eventType)) {
					var newRetrigger = (TriggeredSpawner.RetriggerLimitMode) EditorGUILayout.EnumPopup("Retrigger Limit Mode", waveSetting.retriggerLimitMode);
					if (newRetrigger != waveSetting.retriggerLimitMode) {
						UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Retrigger Limit Mode");
						waveSetting.retriggerLimitMode = newRetrigger;
					}
					switch (waveSetting.retriggerLimitMode) {
						case TriggeredSpawner.RetriggerLimitMode.FrameBased:
							KillerVariablesHelper.DisplayKillerInt(ref isDirty, waveSetting.limitPerXFrm, "Min Frames Between", settings, false, false);
							break;
						case TriggeredSpawner.RetriggerLimitMode.TimeBased:
							KillerVariablesHelper.DisplayKillerFloat(ref isDirty, waveSetting.limitPerXSec, "Min Seconds Between", settings);
							break;
					}
				}
			}
			
			newDisable = EditorGUILayout.Toggle("Disable Event After", waveSetting.disableAfterFirstTrigger);
			if (newDisable != waveSetting.disableAfterFirstTrigger) {
				UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle Disable Event After");
				waveSetting.disableAfterFirstTrigger = newDisable;
			}
	
			if (TriggeredSpawner.eventsThatCanTriggerDespawn.Contains(eventType)) {
				var newWillDespawn = EditorGUILayout.Toggle("Despawn This", waveSetting.willDespawnOnEvent);
				if (newWillDespawn != waveSetting.willDespawnOnEvent) {
					UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle Despawn This");
					waveSetting.willDespawnOnEvent = newWillDespawn;
				}
			}			
			
			if (TriggeredSpawner.eventsWithTagLayerFilters.Contains(eventType)) {
				var newLayer = EditorGUILayout.BeginToggleGroup("Layer Filter", waveSetting.useLayerFilter);
				if (newLayer != waveSetting.useLayerFilter) {
					UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle Layer Filter");
					waveSetting.useLayerFilter = newLayer;
				}
				if (waveSetting.useLayerFilter) {
					for (var i = 0; i < waveSetting.matchingLayers.Count; i++) {
						var newMatch = EditorGUILayout.LayerField("Layer Match " + (i + 1), waveSetting.matchingLayers[i]);
						if (newMatch != waveSetting.matchingLayers[i]) {
							UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Layer Match");
							waveSetting.matchingLayers[i] = newMatch;
						}
					}
					EditorGUILayout.BeginHorizontal();
					GUILayout.Space(12);
					GUI.contentColor = Color.green;
					if (GUILayout.Button(new GUIContent("Add", "Click to add a Layer Match at the end"), EditorStyles.toolbarButton, GUILayout.Width(60))) {
						UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "add Layer Match");
						waveSetting.matchingLayers.Add(0);
					}
					GUILayout.Space(10);
					if (waveSetting.matchingLayers.Count > 1) {
						if (GUILayout.Button(new GUIContent("Remove", "Click to remove the last Layer Match"), EditorStyles.toolbarButton, GUILayout.Width(60))) {
							UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "remove Layer Match");
							waveSetting.matchingLayers.RemoveAt(waveSetting.matchingLayers.Count - 1);
						}
					}
					GUI.contentColor = Color.white;
					EditorGUILayout.EndHorizontal();
				}
				EditorGUILayout.EndToggleGroup();
				
				var newTag = EditorGUILayout.BeginToggleGroup("Tag Filter", waveSetting.useTagFilter);
				if (newTag != waveSetting.useTagFilter) {
					UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle Tag Filter");
					waveSetting.useTagFilter = newTag;
				}
				if (waveSetting.useTagFilter) {
					for (var i = 0; i < waveSetting.matchingTags.Count; i++) {
						var newMatch = EditorGUILayout.TagField("Tag Match " + (i + 1), waveSetting.matchingTags[i]);
						if (newMatch != waveSetting.matchingTags[i]) {
							UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Tag Match");
							waveSetting.matchingTags[i] = newMatch;
						}
					}
					EditorGUILayout.BeginHorizontal();
					GUILayout.Space(12);
					GUI.contentColor = Color.green;
					if (GUILayout.Button(new GUIContent("Add", "Click to add a Tag Match at the end"), EditorStyles.toolbarButton, GUILayout.Width(60))) {
						UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "add Tag Match");
						waveSetting.matchingTags.Add("Untagged");
					}
					GUILayout.Space(10);
					if (waveSetting.matchingTags.Count > 1) {
						if (GUILayout.Button(new GUIContent("Remove", "Click to remove the last Tag Match"), EditorStyles.toolbarButton, GUILayout.Width(60))) {
							UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "remove Tag Match");
							waveSetting.matchingTags.RemoveAt(waveSetting.matchingLayers.Count - 1);
						}
					}
					GUI.contentColor = Color.white;
					EditorGUILayout.EndHorizontal();
				}
				EditorGUILayout.EndToggleGroup();
			}
			
			// repeat wave spawn variable modifiers
			var newBonusesEnabled = EditorGUILayout.BeginToggleGroup("Wave Spawn Bonus", waveSetting.waveSpawnBonusesEnabled);
			if (newBonusesEnabled != waveSetting.waveSpawnBonusesEnabled) {
				UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle Wave Spawn Bonus");
				waveSetting.waveSpawnBonusesEnabled = newBonusesEnabled;
			}
			
			if (waveSetting.waveSpawnBonusesEnabled) {
		        EditorGUI.indentLevel = 1;

				var missingBonusStatNames = new List<string>();
				missingBonusStatNames.AddRange(allStats);
				missingBonusStatNames.RemoveAll(delegate(string obj) {
					return waveSetting.waveSpawnVariableModifiers.HasKey(obj);
				});
				
				var newBonusStat = EditorGUILayout.Popup("Add Variable Modifer", 0, missingBonusStatNames.ToArray());
				if (newBonusStat != 0) {
					AddBonusStatModifier(missingBonusStatNames[newBonusStat], waveSetting);
				}
				
				if (waveSetting.waveSpawnVariableModifiers.statMods.Count == 0) {
					if (waveSetting.waveSpawnBonusesEnabled) {
						DTInspectorUtility.ShowRedError("You currently are using no modifiers for this wave.");
					}
				} else {
					EditorGUILayout.Separator();
					
					int? indexToDelete = null;
					
					for (var i = 0; i < waveSetting.waveSpawnVariableModifiers.statMods.Count; i++) {
						var modifier = waveSetting.waveSpawnVariableModifiers.statMods[i];
						
						var buttonPressed = DTInspectorUtility.FunctionButtons.None;
						switch (modifier._varTypeToUse) {
							case WorldVariableTracker.VariableType._integer:	
								buttonPressed = KillerVariablesHelper.DisplayKillerInt(ref isDirty, modifier._modValueIntAmt, modifier._statName, settings, true, true);	
								break;
							case WorldVariableTracker.VariableType._float:	
								buttonPressed = KillerVariablesHelper.DisplayKillerFloat(ref isDirty, modifier._modValueFloatAmt, modifier._statName, settings, true, true);	
								break;
							default:
								Debug.LogError("Add code for varType: " + modifier._varTypeToUse.ToString());
								break;
						}
						
						KillerVariablesHelper.ShowErrorIfMissingVariable(modifier._statName);
						
						if (buttonPressed == DTInspectorUtility.FunctionButtons.Remove) {
							indexToDelete = i;
						}
					}
					
					if (indexToDelete.HasValue) {
						UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "delete Variable Modifier");
						waveSetting.waveSpawnVariableModifiers.DeleteByIndex(indexToDelete.Value);
					}
					
					EditorGUILayout.Separator();
				}
			}
			EditorGUILayout.EndToggleGroup();			
			
			
			if (TriggeredSpawner.eventsThatCanRepeatWave.Contains(eventType)) {
				var newRepeat = EditorGUILayout.BeginToggleGroup("Repeat Wave", waveSetting.enableRepeatWave);
				if (newRepeat != waveSetting.enableRepeatWave) {
					UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle Repeat Wave");
					waveSetting.enableRepeatWave = newRepeat;
				}
				if (waveSetting.enableRepeatWave) {
					var newRepeatMode = (WaveSpecifics.RepeatWaveMode) EditorGUILayout.EnumPopup("Repeat Mode", waveSetting.curWaveRepeatMode);
					if (newRepeatMode != waveSetting.curWaveRepeatMode) { 
						UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Repeat Mode");
						waveSetting.curWaveRepeatMode = newRepeatMode;
					}

					switch (waveSetting.curWaveRepeatMode) {
						case WaveSpecifics.RepeatWaveMode.NumberOfRepetitions:
							KillerVariablesHelper.DisplayKillerInt(ref isDirty, waveSetting.maxRepeat, "Wave Repetitions", settings, false, false);
							break;
						case WaveSpecifics.RepeatWaveMode.UntilWorldVariableAbove:
						case WaveSpecifics.RepeatWaveMode.UntilWorldVariableBelow:
							var missingStatNames = new List<string>();
							missingStatNames.AddRange(allStats);
							missingStatNames.RemoveAll(delegate(string obj) {
								return waveSetting.repeatPassCriteria.HasKey(obj);
							});
							
							var newStat = EditorGUILayout.Popup("Add Variable Limit", 0, missingStatNames.ToArray());
							if (newStat != 0) {
								AddStatModifier(missingStatNames[newStat], waveSetting);
							}

							if (waveSetting.repeatPassCriteria.statMods.Count == 0) {
								DTInspectorUtility.ShowRedError("You have no Variable Limits. Wave will not repeat.");
							} else {
								EditorGUILayout.Separator();
								
								int? indexToDelete = null;
								
								for (var i = 0; i < waveSetting.repeatPassCriteria.statMods.Count; i++) {
									var modifier = waveSetting.repeatPassCriteria.statMods[i];
									var buttonPressed = KillerVariablesHelper.DisplayKillerInt(ref isDirty, modifier._modValueIntAmt, modifier._statName, settings, true, true);
									if (buttonPressed == DTInspectorUtility.FunctionButtons.Remove) {
										indexToDelete = i;
									}
								
									KillerVariablesHelper.ShowErrorIfMissingVariable(modifier._statName);
								}
								
								DTInspectorUtility.ShowColorWarning("  *Limits are inclusive: i.e. 'Above' means >=");
								if (indexToDelete.HasValue) {
									UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "delete Modifier");
									waveSetting.repeatPassCriteria.DeleteByIndex(indexToDelete.Value);
								}
								
								EditorGUILayout.Separator();
							}
							
							break;
					}
					
					KillerVariablesHelper.DisplayKillerFloat(ref isDirty, waveSetting.repeatWavePauseSec, "Pause Before Repeat", settings);

					KillerVariablesHelper.DisplayKillerInt(ref isDirty, waveSetting.repeatItemInc, "Spawn Increase", settings, false, false);
					
					KillerVariablesHelper.DisplayKillerInt(ref isDirty, waveSetting.repeatItemLmt, "Spawn Limit", settings, false, false);
					
					KillerVariablesHelper.DisplayKillerFloat(ref isDirty, waveSetting.repeatTimeInc, "Time Increase", settings);
                    
					KillerVariablesHelper.DisplayKillerFloat(ref isDirty, waveSetting.repeatTimeLmt, "Time Limit", settings);
					
					if (waveSetting.waveSpawnBonusesEnabled) {
						EditorGUI.indentLevel = 0;
						var newUseRepeatBonus = EditorGUILayout.Toggle("Use Wave Spawn Bonus", waveSetting.useWaveSpawnBonusForRepeats);
						if (newUseRepeatBonus != waveSetting.useWaveSpawnBonusForRepeats) {
							UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle Use Spawn Bonus");
							waveSetting.useWaveSpawnBonusForRepeats = newUseRepeatBonus;
						}
					}
					
				}
				EditorGUILayout.EndToggleGroup();
			}
			
			// show randomizations
			var variantTag = "Randomization";

			var newRand = EditorGUILayout.BeginToggleGroup(variantTag, waveSetting.enableRandomizations);
			if (newRand != waveSetting.enableRandomizations) {
				UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle Randomization");
				waveSetting.enableRandomizations = newRand;
			}
			if (waveSetting.enableRandomizations) {
				EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(100));
	            EditorGUILayout.LabelField("Random Rotation");

				var newRandX = GUILayout.Toggle(waveSetting.randomXRotation, "X");
				if (newRandX != waveSetting.randomXRotation) {
					UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle Random X Rotation");
					waveSetting.randomXRotation = newRandX;
				}
				GUILayout.Space(10);
				var newRandY = GUILayout.Toggle(waveSetting.randomYRotation, "Y");
				if (newRandY != waveSetting.randomYRotation) {
					UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle Random Y Rotation");
					waveSetting.randomYRotation = newRandY;
				}
				GUILayout.Space(10);
				var newRandZ = GUILayout.Toggle(waveSetting.randomZRotation, "Z");
				if (newRandZ != waveSetting.randomZRotation) {
					UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle Random Z Rotation");
					waveSetting.randomZRotation = newRandZ;
				}
	            EditorGUILayout.EndHorizontal();
				
				if (waveSetting.randomXRotation) {
					KillerVariablesHelper.DisplayKillerFloat(ref isDirty, waveSetting.randomXRotMin, "Rand. X Rot. Min", settings);
					
					KillerVariablesHelper.DisplayKillerFloat(ref isDirty, waveSetting.randomXRotMax, "Rand. X Rot. Max", settings);
				}
				if (waveSetting.randomYRotation) {
					KillerVariablesHelper.DisplayKillerFloat(ref isDirty, waveSetting.randomYRotMin, "Rand. Y Rot. Min", settings);
					
					KillerVariablesHelper.DisplayKillerFloat(ref isDirty, waveSetting.randomYRotMax, "Rand. Y Rot. Max", settings);
				}
				if (waveSetting.randomZRotation) {
					KillerVariablesHelper.DisplayKillerFloat(ref isDirty, waveSetting.randomZRotMin, "Rand. Z Rot. Min", settings);
					
					KillerVariablesHelper.DisplayKillerFloat(ref isDirty, waveSetting.randomZRotMax, "Rand. Z Rot. Max", settings);
				}
					
				EditorGUILayout.Separator();

				KillerVariablesHelper.DisplayKillerFloat(ref isDirty, waveSetting.randomDistX, "Rand. Distance X", settings);
				
				KillerVariablesHelper.DisplayKillerFloat(ref isDirty, waveSetting.randomDistY, "Rand. Distance Y", settings);

				KillerVariablesHelper.DisplayKillerFloat(ref isDirty, waveSetting.randomDistZ, "Rand. Distance Z", settings);
			}
			EditorGUILayout.EndToggleGroup();
		
			
			// show increments
			var incTag = "Incremental Settings";
			var newIncrements = EditorGUILayout.BeginToggleGroup(incTag, waveSetting.enableIncrements);
			if (newIncrements != waveSetting.enableIncrements) {
				UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle Incremental Settings");
				waveSetting.enableIncrements = newIncrements;
			}
			if (waveSetting.enableIncrements) {
				KillerVariablesHelper.DisplayKillerFloat(ref isDirty, waveSetting.incrementPositionX, "Distance X", settings);

				KillerVariablesHelper.DisplayKillerFloat(ref isDirty, waveSetting.incrementPositionY, "Distance Y", settings);
				
				KillerVariablesHelper.DisplayKillerFloat(ref isDirty, waveSetting.incrementPositionZ, "Distance Z", settings);

				EditorGUILayout.Separator();
				
				if (waveSetting.enableRandomizations && waveSetting.randomXRotation) {
					DTInspectorUtility.ShowColorWarning("*Rotation X - cannot be used with Random Rotation X.");
				} else {
					KillerVariablesHelper.DisplayKillerFloat(ref isDirty, waveSetting.incrementRotX, "Rotation X", settings);
				}

				if (waveSetting.enableRandomizations && waveSetting.randomYRotation) {
					DTInspectorUtility.ShowColorWarning("*Rotation Y - cannot be used with Random Rotation Y.");
				} else {
					KillerVariablesHelper.DisplayKillerFloat(ref isDirty, waveSetting.incrementRotY, "Rotation Y", settings);
				}

				if (waveSetting.enableRandomizations && waveSetting.randomZRotation) {
					DTInspectorUtility.ShowColorWarning("*Rotation Z - cannot be used with Random Rotation Z.");
				} else {
					KillerVariablesHelper.DisplayKillerFloat(ref isDirty, waveSetting.incrementRotZ, "Rotation Z", settings);
				}
				
                var newIncKC = EditorGUILayout.Toggle("Keep Center", waveSetting.enableKeepCenter);
                if (newIncKC != waveSetting.enableKeepCenter)
                {
					UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle Keep Center");
                    waveSetting.enableKeepCenter = newIncKC;
                }
			}
			EditorGUILayout.EndToggleGroup();

			
			// show increments
			incTag = "Post-spawn Nudge Settings";
			var newPostEnabled = EditorGUILayout.BeginToggleGroup(incTag, waveSetting.enablePostSpawnNudge);
			if (newPostEnabled != waveSetting.enablePostSpawnNudge) {
				UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle Post-spawn Nudge Settings");
				waveSetting.enablePostSpawnNudge = newPostEnabled;
			}
			if (waveSetting.enablePostSpawnNudge) {
				KillerVariablesHelper.DisplayKillerFloat(ref isDirty, waveSetting.postSpawnNudgeFwd, "Nudge Forward", settings);

				KillerVariablesHelper.DisplayKillerFloat(ref isDirty, waveSetting.postSpawnNudgeRgt, "Nudge Right", settings);
				
				KillerVariablesHelper.DisplayKillerFloat(ref isDirty, waveSetting.postSpawnNudgeDwn, "Nudge Down", settings);
			}
			EditorGUILayout.EndToggleGroup();
		}
	}

	private List<string> getUnusedEventTypes() {
		var unusedEvents = new List<string>();
		unusedEvents.Add("-None-");
		if (!settings.enableWave.enableWave) {
			unusedEvents.Add("Enabled");
		}
		if (!settings.disableWave.enableWave) {
			unusedEvents.Add("Disabled");
		}
		if (!settings.visibleWave.enableWave) {
			unusedEvents.Add("Visible");
		}
		if (!settings.invisibleWave.enableWave) {
			unusedEvents.Add("Invisible");
		}

		if (settings.unityUIMode == TriggeredSpawner.Unity_UIVersion.Legacy) {
			if (!settings.mouseOverWave.enableWave) {
				unusedEvents.Add("Mouse Over (Legacy)");
			}
			if (!settings.mouseClickWave.enableWave) {
				unusedEvents.Add("Mouse Click (Legacy)");
			}
		}

#if UNITY_4_6 || UNITY_5_0
        if (settings.unityUIMode == TriggeredSpawner.Unity_UIVersion.uGUI) {
			if (hasSlider && !settings.unitySliderChangedWave.enableWave) {
				unusedEvents.Add("Slider Changed (uGUI)");
			}
			if (hasButton && !settings.unityButtonClickedWave.enableWave) {
				unusedEvents.Add("Button Click (uGUI)");
			}
			if (hasRect) {
				if (!settings.unityPointerDownWave.enableWave) {
					unusedEvents.Add("Pointer Down (uGUI)");
				}
				if (!settings.unityPointerUpWave.enableWave) {
					unusedEvents.Add("Pointer Up (uGUI)");
				}
				if (!settings.unityPointerEnterWave.enableWave) {
					unusedEvents.Add("Pointer Enter (uGUI)");
				}
				if (!settings.unityPointerExitWave.enableWave) {
					unusedEvents.Add("Pointer Exit (uGUI)");
				}
				if (!settings.unityDragWave.enableWave) {
					unusedEvents.Add("Drag (uGUI)");
				}
				if (!settings.unityDropWave.enableWave) {
					unusedEvents.Add("Drop (uGUI)");
				}
				if (!settings.unityScrollWave.enableWave) {
					unusedEvents.Add("Scroll (uGUI)");
				}
				if (!settings.unityUpdateSelectedWave.enableWave) {
					unusedEvents.Add("Update Selected (uGUI)");
				}
				if (!settings.unitySelectWave.enableWave) {
					unusedEvents.Add("Select (uGUI)");
				}
				if (!settings.unityDeselectWave.enableWave) {
					unusedEvents.Add("Deselect (uGUI)");
				}
				if (!settings.unityMoveWave.enableWave) {
					unusedEvents.Add("Move (uGUI)");
				}
				if (!settings.unityInitializePotentialDragWave.enableWave) {
					unusedEvents.Add("Init. Potential Drag (uGUI)");
				}
				if (!settings.unityBeginDragWave.enableWave) {
					unusedEvents.Add("Begin Drag (uGUI)");
				}
				if (!settings.unityEndDragWave.enableWave) {
					unusedEvents.Add("End Drag (uGUI)");
				}
				if (!settings.unitySubmitWave.enableWave) {
					unusedEvents.Add("Submit (uGUI)");
				}
				if (!settings.unityCancelWave.enableWave) {
					unusedEvents.Add("Cancel (uGUI)");
				}
			}
		}
#endif

		if (!settings.collisionWave.enableWave) {
			unusedEvents.Add("Collision Enter");
		}
		if (!settings.triggerEnterWave.enableWave) {
			unusedEvents.Add("Trigger Enter");
		}
		if (!settings.triggerExitWave.enableWave) {
			unusedEvents.Add("Trigger Exit");
		}
		#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
			// not supported
		#else
			if (!settings.collision2dWave.enableWave) {
				unusedEvents.Add("2D Collision Enter");
			}
			if (!settings.triggerEnter2dWave.enableWave) {
				unusedEvents.Add("2D Trigger Enter");
			}
			if (!settings.triggerExit2dWave.enableWave) {
				unusedEvents.Add("2D Trigger Exit");
			}
		#endif
		if (!settings.codeTriggeredWave1.enableWave) {
			unusedEvents.Add("Code-Triggered 1");
		}
		if (!settings.codeTriggeredWave2.enableWave) {
			unusedEvents.Add("Code-Triggered 2");
		}
		if (!settings.spawnedWave.enableWave) {
			unusedEvents.Add("Spawned");
		}
		if (!settings.despawnedWave.enableWave) {
			unusedEvents.Add("Despawned");
		}
		if (!settings.clickWave.enableWave) {
			unusedEvents.Add("NGUI OnClick");
		}

		unusedEvents.Add ("Custom Event");

		return unusedEvents;
	}

	private void ActivateEvent(int index, List<string> unusedEvents) {
		var item = unusedEvents[index];
	
		UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "activate Event");

		switch (item) {
			case "Code-Triggered 1":
				settings.codeTriggeredWave1.enableWave = true;
				break;
			case "Code-Triggered 2":
				settings.codeTriggeredWave2.enableWave = true;
				break;
			case "Invisible":
				settings.invisibleWave.enableWave = true;
				break;
			case "Mouse Click (Legacy)":
				settings.mouseClickWave.enableWave = true;
				break;
			case "Mouse Over (Legacy)":
				settings.mouseOverWave.enableWave = true;
				break;
			case "NGUI OnClick":
				settings.clickWave.enableWave = true;
				break;
			case "Collision Enter":
				settings.collisionWave.enableWave = true;
				break;
			case "Despawned":
				settings.despawnedWave.enableWave = true;
				break;
			case "Disabled":
				settings.disableWave.enableWave = true;
				break;
			case "Enabled":
				settings.enableWave.enableWave = true;
				break;
			case "Spawned":
				settings.spawnedWave.enableWave = true;
				break;
			case "Trigger Enter":
				settings.triggerEnterWave.enableWave = true;
				break;
			case "Trigger Exit":
				settings.triggerExitWave.enableWave = true;
				break;
			case "Visible":
				settings.visibleWave.enableWave = true;
				break;
			case "2D Collision Enter":
				settings.collision2dWave.enableWave = true;
				break;
			case "2D Trigger Enter":
				settings.triggerEnter2dWave.enableWave = true;
				break;
			case "2D Trigger Exit":
				settings.triggerExit2dWave.enableWave = true;
				break;
			case "Slider Changed (uGUI)":
				settings.unitySliderChangedWave.enableWave = true;
				break;
			case "Button Click (uGUI)":
				settings.unityButtonClickedWave.enableWave = true;
				break;
			case "Pointer Down (uGUI)":
				settings.unityPointerDownWave.enableWave = true;
				break;
			case "Pointer Up (uGUI)":
				settings.unityPointerUpWave.enableWave = true;
				break;
			case "Pointer Enter (uGUI)":
				settings.unityPointerEnterWave.enableWave = true;
				break;
			case "Pointer Exit (uGUI)":
				settings.unityPointerExitWave.enableWave = true;
				break;
			case "Drag (uGUI)":
				settings.unityDragWave.enableWave = true;
				break;
			case "Drop (uGUI)":
				settings.unityDropWave.enableWave = true;
				break;
			case "Scroll (uGUI)":
				settings.unityScrollWave.enableWave = true;
				break;
			case "Update Selected (uGUI)":
				settings.unityUpdateSelectedWave.enableWave = true;
				break;
			case "Select (uGUI)":
				settings.unitySelectWave.enableWave = true;
				break;
			case "Deselect (uGUI)":
				settings.unityDeselectWave.enableWave = true;
				break;
			case "Move (uGUI)":
				settings.unityMoveWave.enableWave = true;
				break;
			case "Init. Potential Drag (uGUI)":
				settings.unityInitializePotentialDragWave.enableWave = true;
				break;
			case "Begin Drag (uGUI)":
				settings.unityBeginDragWave.enableWave = true;
				break;
			case "End Drag (uGUI)":
				settings.unityEndDragWave.enableWave = true;
				break;
			case "Submit (uGUI)":
				settings.unitySubmitWave.enableWave = true;
				break;
			case "Cancel (uGUI)":
				settings.unityCancelWave.enableWave = true;
				break;
			case "Custom Event":
				CreateCustomEvent(false);
				break;
		}
	}

	private void AddStatModifier(string modifierName, TriggeredWaveSpecifics spec) {
		if (spec.repeatPassCriteria.HasKey(modifierName)) {
			DTInspectorUtility.ShowAlert("This wave already has a Variable Limit for World Variable: " + modifierName + ". Please modify the existing one instead.");
			return;
		}
	
		UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "add Variable Limit");
		
		var myVar = WorldVariableTracker.GetWorldVariableScript(modifierName);
		
		spec.repeatPassCriteria.statMods.Add(new WorldVariableModifier(modifierName, myVar.varType));
	}

	private void AddActiveLimit(string modifierName, TriggeredSpawner spec) {
		if (spec.activeItemCriteria.HasKey(modifierName)) {
			DTInspectorUtility.ShowAlert("This item already has a Active Limit for World Variable: " + modifierName + ". Please modify the existing one instead.");
			return;
		}

		UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "add Active Limit");
		
		var myVar = WorldVariableTracker.GetWorldVariableScript(modifierName);
		
		spec.activeItemCriteria.statMods.Add(new WorldVariableRange(modifierName, myVar.varType));
	}

	private void AddBonusStatModifier(string modifierName, TriggeredWaveSpecifics waveSpec) {
		if (waveSpec.waveSpawnVariableModifiers.HasKey(modifierName)) {
			DTInspectorUtility.ShowAlert("This Wave already has a modifier for World Variable: " + modifierName + ". Please modify that instead.");
			return;
		}

		UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "add Wave Repeat Bonus modifier");
		
		WorldVariable vType = WorldVariableTracker.GetWorldVariableScript(modifierName);
		
		waveSpec.waveSpawnVariableModifiers.statMods.Add(new WorldVariableModifier(modifierName, vType.varType));
	}
	
	private static List<string> PoolNames {
		get {
			return LevelSettings.GetSortedPrefabPoolNames();
		}
	}

	private void CreateCustomEvent(bool recordUndo) {
		var newWave = new TriggeredWaveSpecifics();
		newWave.customEventActive = true;
		newWave.isCustomEvent = true;
		newWave.enableWave = true;

		if (recordUndo) {
			UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "add Custom Event");
		}
		
		settings.userDefinedEventWaves.Add(newWave);
	}
}