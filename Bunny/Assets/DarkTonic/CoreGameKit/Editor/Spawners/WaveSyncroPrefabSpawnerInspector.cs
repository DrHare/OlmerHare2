using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
	[CustomEditor(typeof(WaveSyncroPrefabSpawner))]
#else
	[CustomEditor(typeof(WaveSyncroPrefabSpawner), true)]
#endif
public class WaveSyncroPrefabSpawnerInspector : Editor {
	private LevelSettings levSettings;
	private WaveSyncroPrefabSpawner settings;
    private bool isDirty = false;


	public override void OnInspectorGUI() {
        EditorGUIUtility.LookLikeControls();
		
        settings = (WaveSyncroPrefabSpawner)target; 
		
		WorldVariableTracker.ClearInGamePlayerStats();

		isDirty = false;
		
        var myParent = settings.transform.parent;
        Transform levelSettingObj = null;
        LevelSettings levelSettings = null;
		
		LevelSettings.Instance = null; // clear cached version
		
        if (myParent != null)
        {
            levelSettingObj = myParent.parent;
            if (levelSettingObj != null)
            {
                levelSettings = levelSettingObj.GetComponent<LevelSettings>();
            }
        }

        if (myParent == null || levelSettingObj == null || levelSettings == null)
        {
            DrawDefaultInspector();
            return;
        }

		var allStats = KillerVariablesHelper.AllStatNames;

        DTInspectorUtility.DrawTexture(CoreGameKitInspectorResources.logoTexture);

        EditorGUILayout.Separator();
        EditorGUI.indentLevel = 0;

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
					AddActiveLimit(missingStatNames[newStat]);
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
                                    UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Limit Min");
									modifier._modValueIntMin = newMin;
								}
								GUILayout.Label("Max");
								
								var newMax = EditorGUILayout.IntField(modifier._modValueIntMax, GUILayout.MaxWidth(60));
								if (newMax != modifier._modValueIntMax) {
                                    UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Limit Max");
									modifier._modValueIntMax = newMax;
								}
								break;
							case WorldVariableTracker.VariableType._float:	
								var newMinFloat = EditorGUILayout.FloatField(modifier._modValueFloatMin, GUILayout.MaxWidth(60));
								if (newMinFloat != modifier._modValueFloatMin) {
                                    UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Limit Min");
									modifier._modValueFloatMin = newMinFloat;
								}
								GUILayout.Label("Max");
								
								var newMaxFloat = EditorGUILayout.FloatField(modifier._modValueFloatMax, GUILayout.MaxWidth(60));
								if (newMaxFloat != modifier._modValueFloatMax) {
                                    UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Limit Max");
									modifier._modValueFloatMax = newMaxFloat;
								}
								break;
							default:
								Debug.LogError("Add code for varType: " + modifier._varTypeToUse.ToString());
								break;
						}
						GUI.backgroundColor = Color.green;
						if (GUILayout.Button(new GUIContent("Delete", "Remove this Limit"), EditorStyles.miniButtonMid, GUILayout.MaxWidth(64))) {
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
                        UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "delete Limit");
						settings.activeItemCriteria.DeleteByIndex(indexToDelete.Value);
					}
					
					EditorGUILayout.Separator();
				}
			
				break;
		}

		var newGO = (TriggeredSpawner.GameOverBehavior)EditorGUILayout.EnumPopup("Game Over Behavior", settings.gameOverBehavior);
		if (newGO != settings.gameOverBehavior) {
            UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Game Over Behavior");
			settings.gameOverBehavior = newGO;
		}

		var newPause = (TriggeredSpawner.WavePauseBehavior)EditorGUILayout.EnumPopup("Wave Pause Behavior", settings.wavePauseBehavior);
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
		var newListener = (WaveSyncroSpawnerListener)EditorGUILayout.ObjectField("Listener Prefab", settings.listener, typeof(WaveSyncroSpawnerListener), true);
		if (newListener != settings.listener) {
			UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "assign Listener");
			settings.listener = newListener;
	        if (hadNoListener && settings.listener != null)
	        {
	            settings.listener.sourceSpawnerName = settings.transform.name;
	        }
		}

        EditorGUILayout.Separator();
        EditorGUI.indentLevel = 0;
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        var disabledText = "";
        if (settings.activeMode == LevelSettings.ActiveItemMode.Never)
        {
            disabledText = " --DISABLED--";
        }

		var newExpanded = DTInspectorUtility.Foldout(settings.isExpanded,
			string.Format("Wave Settings ({0}){1}", settings.waveSpecs.Count, disabledText));
		if (newExpanded != settings.isExpanded) {
            UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle expand Wave Settings");
			settings.isExpanded = newExpanded;
		}
        // BUTTONS...
        EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(100));

        DTInspectorUtility.FunctionButtons waveButtonPressed = DTInspectorUtility.FunctionButtons.None;

        if (settings.activeMode != LevelSettings.ActiveItemMode.Never) 
        {
			GUI.contentColor = Color.green;
			
            // Add expand/collapse buttons if there are items in the list
            if (settings.waveSpecs.Count > 0)
            {
                GUIContent content;
                content = new GUIContent("Collapse", "Click to collapse all");
                var masterCollapse = GUILayout.Button(content, EditorStyles.toolbarButton);

                content = new GUIContent("Expand", "Click to expand all");
                var masterExpand = GUILayout.Button(content, EditorStyles.toolbarButton);
                if (masterExpand)
                {
                    ExpandCollapseAll(true);
                }
                if (masterCollapse)
                {
                    ExpandCollapseAll(false);
                }
            }
            else
            {
                GUILayout.FlexibleSpace();
            }

            EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(50));
            // A little space between button groups

            var addText = string.Format("Click to add Wave{0}.", settings.waveSpecs.Count > 0 ? " before the first" : "");
			GUI.contentColor = Color.yellow;

            // Main Add button
            if (GUILayout.Button(new GUIContent("Add", addText), EditorStyles.toolbarButton))
            {
                if (levelSettings.LevelTimes.Count == 0)
                {
                    DTInspectorUtility.ShowAlert("You will not have any Level or Wave #'s to select in your Spawner Wave Settings until you add a Level in LevelSettings. Please do that first.");
                }
                else
                {
                    var newWave = new WaveSpecifics();
                    UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "add Wave");
					settings.waveSpecs.Add(newWave);
                }
            }

			GUI.contentColor = Color.white;
			
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndHorizontal();

            if (settings.isExpanded)
            {
                EditorGUI.indentLevel = 0;

				if (settings.waveSpecs.Count == 0) {
                	DTInspectorUtility.ShowLargeBarAlert("You have zero Wave Settings. Your spawner won't spawn anything.");
				}
				
                int waveToInsertAt = -1;
                WaveSpecifics waveToDelete = null;
                WaveSpecifics waveSetting = null;
                int? waveToMoveUp = null;
                int? waveToMoveDown = null;
                LevelWave levelWave = null;

                // get list of prefab pools.
                var poolNames = LevelSettings.GetSortedPrefabPoolNames();

                for (var w = 0; w < settings.waveSpecs.Count; w++)
                {
                    EditorGUI.indentLevel = 1;
                    waveSetting = settings.waveSpecs[w];
                    levelWave = GetLevelWaveFromWaveSpec(waveSetting);

                    EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

                    string sDisabled = "";
                    if (!waveSetting.isExpanded && !waveSetting.enableWave)
                    {
                        sDisabled = " DISABLED ";
                    }

					newExpanded = DTInspectorUtility.Foldout(waveSetting.isExpanded,
	                  string.Format("Wave Setting #{0} ({1}/{2}){3}", (w + 1),
				              waveSetting.SpawnLevelNumber + 1,
				              waveSetting.SpawnWaveNumber + 1,
				              sDisabled));
					if (newExpanded != waveSetting.isExpanded) {
                        UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle expand Wave Setting");
						waveSetting.isExpanded = newExpanded;
					}
					
					GUILayout.FlexibleSpace();
                    waveButtonPressed = DTInspectorUtility.AddFoldOutListItemButtons(w, settings.waveSpecs.Count, "Wave", false, true);
                    EditorGUILayout.EndHorizontal();

                    switch (waveButtonPressed)
                    {
                        case DTInspectorUtility.FunctionButtons.Remove:
                            waveToDelete = waveSetting;
							isDirty = true;
                            break;
                        case DTInspectorUtility.FunctionButtons.Add:
                            waveToInsertAt = w;
							isDirty = true;
                            break;
                        case DTInspectorUtility.FunctionButtons.ShiftDown:
                            waveToMoveDown = w;
							isDirty = true;
                            break;
                        case DTInspectorUtility.FunctionButtons.ShiftUp:
                            waveToMoveUp = w;
							isDirty = true;
                            break;
                    }

                    if (waveSetting.isExpanded)
                    {
                        EditorGUI.indentLevel = 0;
						var newEnabled = EditorGUILayout.BeginToggleGroup("Enable Wave", waveSetting.enableWave);
						if (newEnabled != waveSetting.enableWave) {
                            UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle Enable Wave");
							waveSetting.enableWave = newEnabled;
						}

                        var oldLevelNumber = waveSetting.SpawnLevelNumber;

						var newLevel = EditorGUILayout.IntPopup("Level#", waveSetting.SpawnLevelNumber + 1, LevelNames, LevelIndexes) - 1;
						if (newLevel != waveSetting.SpawnLevelNumber) {
                            UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Level#");
							waveSetting.SpawnLevelNumber = newLevel;

	                        if (oldLevelNumber != waveSetting.SpawnLevelNumber)
	                        {
	                            waveSetting.SpawnWaveNumber = 0;
	                        }
						}

						var newWave = EditorGUILayout.IntPopup("Wave#", waveSetting.SpawnWaveNumber + 1,
							WaveNamesForLevel(waveSetting.SpawnLevelNumber), WaveIndexesForLevel(waveSetting.SpawnLevelNumber)) - 1;
						if (newWave != waveSetting.SpawnWaveNumber) {
                            UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Wave#");
							waveSetting.SpawnWaveNumber = newWave;
						}

                        KillerVariablesHelper.DisplayKillerInt(ref isDirty, waveSetting.MinToSpwn, "Min To Spawn", settings, false, false);

                        KillerVariablesHelper.DisplayKillerInt(ref isDirty, waveSetting.MaxToSpwn, "Max To Spawn", settings, false, false);

                        KillerVariablesHelper.DisplayKillerFloat(ref isDirty, waveSetting.TimeToSpawnEntireWave, "Time To Spawn All", settings);

                        KillerVariablesHelper.DisplayKillerFloat(ref isDirty, waveSetting.WaveDelaySec, "Delay Wave (sec)", settings);
						
                        if (levelWave.waveType == LevelSettings.WaveType.Elimination) {
							var newComplete = EditorGUILayout.IntSlider("Wave Completion %", waveSetting.waveCompletePercentage, 1, 100);
							if (newComplete != waveSetting.waveCompletePercentage) {
                                UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "Wave Completion %");
								waveSetting.waveCompletePercentage = newComplete;
							}
						}
						
						var newSource = (WaveSpecifics.SpawnOrigin)EditorGUILayout.EnumPopup("Prefab Type", waveSetting.spawnSource);
						if (newSource != waveSetting.spawnSource) {
                            UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Prefab Type");
							waveSetting.spawnSource = newSource;
						}
                        switch (waveSetting.spawnSource)
                        {
                            case WaveSpecifics.SpawnOrigin.Specific:
								var newPrefab = (Transform)EditorGUILayout.ObjectField("Prefab To Spawn", waveSetting.prefabToSpawn, typeof(Transform), true);
								if (newPrefab != waveSetting.prefabToSpawn) {
                                    UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Prefab To Spawn");
									waveSetting.prefabToSpawn = newPrefab;
								}
								if (waveSetting.prefabToSpawn == null) {
									DTInspectorUtility.ShowRedError("Please specify a prefab to spawn.");
								}
                                break;
                            case WaveSpecifics.SpawnOrigin.PrefabPool:
                                if (poolNames != null)
                                {
                                    var pool = LevelSettings.GetFirstMatchingPrefabPool(waveSetting.prefabPoolName);
                                	var noPoolSelected = false;    
									var illegalPool = false;
									var noPools = false;
								
									if (pool == null)
                                    {
                                        if (string.IsNullOrEmpty(waveSetting.prefabPoolName))
                                        {
                                            noPoolSelected = true;
                                        }
                                        else
                                        {
											illegalPool = true;
                                        }
                                        waveSetting.prefabPoolIndex = 0;
                                    }
                                    else
                                    {
                                        waveSetting.prefabPoolIndex = poolNames.IndexOf(waveSetting.prefabPoolName);
                                    }

                                    if (poolNames.Count > 1)
                                    {
										var newPoolIndex = EditorGUILayout.Popup("Prefab Pool", waveSetting.prefabPoolIndex, poolNames.ToArray());
										if (newPoolIndex != waveSetting.prefabPoolIndex) {
                                            UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Prefab Pool");
											waveSetting.prefabPoolIndex = newPoolIndex;
										}

                                        if (waveSetting.prefabPoolIndex > 0)
                                        {
                                            var matchingPool = LevelSettings.GetFirstMatchingPrefabPool(poolNames[waveSetting.prefabPoolIndex]);
                                            if (matchingPool != null)
                                            {
                                                waveSetting.prefabPoolName = matchingPool.name;
                                            }
                                        } else {
											waveSetting.prefabPoolName = string.Empty;
										}
                                    }
                                    else
                                    {
                                        noPools = true;
                                    }
								
									if (noPools) {
										DTInspectorUtility.ShowRedError("You have no Prefab Pools. Create one first.");									
									} else if (noPoolSelected) {
										DTInspectorUtility.ShowRedError("No Prefab Pool selected.");
									} else if (illegalPool) {
										DTInspectorUtility.ShowRedError("Prefab Pool '" + waveSetting.prefabPoolName + "' not found. Select one.");									
									}
                                }
                                else
                                {
                                    DTInspectorUtility.ShowRedError(LevelSettings.NO_PREFAB_POOLS_CONTAINER_ALERT);
                                    DTInspectorUtility.ShowRedError(LevelSettings.REVERT_LEVEL_SETTINGS_ALERT);
                                }

                                break;
                        }

                        var newEx = EditorGUILayout.BeginToggleGroup("Position Settings", waveSetting.positionExpanded);
                        if (newEx != waveSetting.positionExpanded) {
                            UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle expand Position Settings");
                            waveSetting.positionExpanded = newEx;
                        }

                        if (waveSetting.positionExpanded) {
                            var newX = (WaveSpecifics.PositionMode)EditorGUILayout.EnumPopup("X Position Mode", waveSetting.positionXmode);
                            if (newX != waveSetting.positionXmode) {
                                UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change X Position Mode");
                                waveSetting.positionXmode = newX;
                            }

                            if (waveSetting.positionXmode == WaveSpecifics.PositionMode.CustomPosition) {
                                KillerVariablesHelper.DisplayKillerInt(ref isDirty, waveSetting.customPosX, "X Position", settings);
                            }

                            var newY = (WaveSpecifics.PositionMode)EditorGUILayout.EnumPopup("Y Position Mode", waveSetting.positionYmode);
                            if (newY != waveSetting.positionYmode) {
                                UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Y Position Mode");
                                waveSetting.positionYmode = newY;
                            }

                            if (waveSetting.positionYmode == WaveSpecifics.PositionMode.CustomPosition) {
                                KillerVariablesHelper.DisplayKillerInt(ref isDirty, waveSetting.customPosY, "Y Position", settings);
                            }

                            var newZ = (WaveSpecifics.PositionMode)EditorGUILayout.EnumPopup("Z Position Mode", waveSetting.positionZmode);
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

						var newRotation = (WaveSpecifics.RotationMode) EditorGUILayout.EnumPopup("Spawn Rotation Mode", waveSetting.curRotationMode);
						if (newRotation != waveSetting.curRotationMode) {
                            UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Rotation Mode");
							waveSetting.curRotationMode = newRotation;
						}
						
						if (waveSetting.curRotationMode == WaveSpecifics.RotationMode.CustomRotation) {
							var newCust = EditorGUILayout.Vector3Field("Custom Rotation Euler", waveSetting.customRotation);
							if (newCust != waveSetting.customRotation) {
                                UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Custom Rotation Euler");
								waveSetting.customRotation = newCust;
							}
						}
						
						newExpanded = EditorGUILayout.BeginToggleGroup("Spawn Limit Controls", waveSetting.enableLimits);
						if (newExpanded != waveSetting.enableLimits) {
                            UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle Spawn Limit Controls");
							waveSetting.enableLimits = newExpanded;
						}
                        if (waveSetting.enableLimits)
                        {
                            DTInspectorUtility.ShowColorWarning("Stop spawning until all spawns from wave satisfy:");

                            KillerVariablesHelper.DisplayKillerFloat(ref isDirty, waveSetting.doNotSpawnIfMbrCloserThan, "Min. Distance", settings);

                            KillerVariablesHelper.DisplayKillerFloat(ref isDirty, waveSetting.doNotSpawnRandomDist, "Random Distance", settings);
                        }
                        EditorGUILayout.EndToggleGroup();

						newExpanded = EditorGUILayout.BeginToggleGroup("Repeat Wave", waveSetting.repeatWaveUntilNew);
						if (newExpanded != waveSetting.repeatWaveUntilNew) {
                            UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle Repeat Wave");
							waveSetting.repeatWaveUntilNew = newExpanded;
						}
                        if (waveSetting.repeatWaveUntilNew)
                        {
                            if (levelWave.waveType == LevelSettings.WaveType.Elimination)
                            {
								var newRepeatMode = (WaveSpecifics.RepeatWaveMode)EditorGUILayout.EnumPopup("Repeat Mode", waveSetting.curWaveRepeatMode);
								if (newRepeatMode != waveSetting.curWaveRepeatMode) {
                                    UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Repeat Mode");
									waveSetting.curWaveRepeatMode = newRepeatMode;
								}
                            } else {
								// only one mode for non-elimination waves.
								var newRepeatMode = (WaveSpecifics.TimedRepeatWaveMode) EditorGUILayout.EnumPopup("Timed Repeat Mode", waveSetting.curTimedRepeatWaveMode);
								if (newRepeatMode != waveSetting.curTimedRepeatWaveMode) {
                                    UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Timed Repeat Mode");
									waveSetting.curTimedRepeatWaveMode = newRepeatMode;
								}
							}

                            switch (waveSetting.curWaveRepeatMode)
                            {
                                case WaveSpecifics.RepeatWaveMode.NumberOfRepetitions:
                                    if (levelWave.waveType == LevelSettings.WaveType.Elimination)
                                    {
                                        KillerVariablesHelper.DisplayKillerInt(ref isDirty, waveSetting.repetitionsToDo, "Repetitions", settings, false, false);
                                    }
                                    break;
                                case WaveSpecifics.RepeatWaveMode.UntilWorldVariableAbove:
                                case WaveSpecifics.RepeatWaveMode.UntilWorldVariableBelow:
                                    if (levelWave.waveType != LevelSettings.WaveType.Elimination)
                                    {
                                        break;
                                    }

                                    var missingStatNames = new List<string>();
                                    missingStatNames.AddRange(allStats);
                                    missingStatNames.RemoveAll(delegate(string obj)
                                    {
                                        return waveSetting.repeatPassCriteria.HasKey(obj);
                                    });

                                    var newStat = EditorGUILayout.Popup("Add Variable Limit", 0, missingStatNames.ToArray());
                                    if (newStat != 0)
                                    {
                                        AddStatModifier(missingStatNames[newStat], waveSetting);
                                    }

                                    if (waveSetting.repeatPassCriteria.statMods.Count == 0)
                                    {
                                        DTInspectorUtility.ShowRedError("You have no Variable Limits. Wave will not repeat.");
                                    }
                                    else
                                    {
                                        EditorGUILayout.Separator();

                                        int? indexToDelete = null;

                                        for (var i = 0; i < waveSetting.repeatPassCriteria.statMods.Count; i++)
                                        {
                                            var modifier = waveSetting.repeatPassCriteria.statMods[i];
                                            var buttonPressed = KillerVariablesHelper.DisplayKillerInt(ref isDirty, modifier._modValueIntAmt, modifier._statName, settings, true, true);
											if (buttonPressed == DTInspectorUtility.FunctionButtons.Remove) {
                                                indexToDelete = i;
                                            }
                                        }

                                        DTInspectorUtility.ShowColorWarning("  *Limits are inclusive: i.e. 'Above' means >=");
                                        if (indexToDelete.HasValue)
                                        {
                                            UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "delete Limit");
                                            waveSetting.repeatPassCriteria.DeleteByIndex(indexToDelete.Value);
                                        }

                                        EditorGUILayout.Separator();
                                    }
                                    break;
                            }

                            KillerVariablesHelper.DisplayKillerFloat(ref isDirty, waveSetting.repeatPauseMinimum, "Repeat Pause Min", settings);

                            KillerVariablesHelper.DisplayKillerFloat(ref isDirty, waveSetting.repeatPauseMaximum, "Repeat Pause Max", settings);

                            KillerVariablesHelper.DisplayKillerInt(ref isDirty, waveSetting.repeatItemInc, "Spawn Increase", settings, false, false);

                            KillerVariablesHelper.DisplayKillerInt(ref isDirty, waveSetting.repeatItemLmt, "Spawn Limit", settings, false, false);

                            KillerVariablesHelper.DisplayKillerFloat(ref isDirty, waveSetting.repeatTimeInc, "Time Increase", settings);

                            KillerVariablesHelper.DisplayKillerFloat(ref isDirty, waveSetting.repeatTimeLmt, "Time Limit", settings);
						
							
							// repeat wave variable modifiers
							var newBonusesEnabled = EditorGUILayout.Toggle("Wave Repeat Bonus", waveSetting.waveRepeatBonusesEnabled);
							if (newBonusesEnabled != waveSetting.waveRepeatBonusesEnabled) {
                                UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle Wave Repeat Bonus");
								waveSetting.waveRepeatBonusesEnabled = newBonusesEnabled;
							}
							
							if (waveSetting.waveRepeatBonusesEnabled) {
						        EditorGUI.indentLevel = 1;

								var missingBonusStatNames = new List<string>();
								missingBonusStatNames.AddRange(allStats);
								missingBonusStatNames.RemoveAll(delegate(string obj) {
									return waveSetting.waveRepeatVariableModifiers.HasKey(obj);
								});
								
								var newBonusStat = EditorGUILayout.Popup("Add Variable Modifer", 0, missingBonusStatNames.ToArray());
								if (newBonusStat != 0) {
									AddBonusStatModifier(missingBonusStatNames[newBonusStat], waveSetting);
								}
								
								if (waveSetting.waveRepeatVariableModifiers.statMods.Count == 0) {
									if (waveSetting.waveRepeatBonusesEnabled) {
										DTInspectorUtility.ShowColorWarning("*You currently are using no modifiers for this wave.");
									}
								} else {
									EditorGUILayout.Separator();
									
									int? indexToDelete = null;
									
									EditorGUI.indentLevel = 0;
									for (var i = 0; i < waveSetting.waveRepeatVariableModifiers.statMods.Count; i++) {
										var modifier = waveSetting.waveRepeatVariableModifiers.statMods[i];
										
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
										waveSetting.waveRepeatVariableModifiers.DeleteByIndex(indexToDelete.Value);
									}
									
									EditorGUILayout.Separator();
								}
							}
							
						}
                        EditorGUILayout.EndToggleGroup();

						EditorGUI.indentLevel = 0;
                        // show randomizations
                        var variantTag = "Randomization";

						newExpanded = EditorGUILayout.BeginToggleGroup(variantTag, waveSetting.enableRandomizations);
						if (newExpanded != waveSetting.enableRandomizations) {
                            UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle Randomization");
							waveSetting.enableRandomizations = newExpanded;
						}
                        if (waveSetting.enableRandomizations)
                        {
                            EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(100));
                            EditorGUILayout.LabelField("Random Rotation");

							var newRandX = GUILayout.Toggle(waveSetting.randomXRotation, "X");
							if (newRandX != waveSetting.randomXRotation) {
                                UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle Random Rotation X");
								waveSetting.randomXRotation = newRandX;
							}
                            GUILayout.Space(10);
                            
							var newRandY = GUILayout.Toggle(waveSetting.randomYRotation, "Y");
							if (newRandY != waveSetting.randomYRotation) {
                                UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle Random Rotation Y");
								waveSetting.randomYRotation = newRandY;
							}
                            GUILayout.Space(10);
                            
							var newRandZ = GUILayout.Toggle(waveSetting.randomZRotation, "Z");
							if (newRandZ != waveSetting.randomZRotation) {
                                UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle Random Rotation Z");
								waveSetting.randomZRotation = newRandZ;
							}
                            EditorGUILayout.EndHorizontal();

                            if (waveSetting.randomXRotation)
                            {
                                KillerVariablesHelper.DisplayKillerFloat(ref isDirty, waveSetting.randomXRotMin, "Rand. X Rot. Min", settings);

								KillerVariablesHelper.DisplayKillerFloat(ref isDirty, waveSetting.randomXRotMax, "Rand. X Rot. Max", settings);
                            }

                            if (waveSetting.randomYRotation)
                            {
								KillerVariablesHelper.DisplayKillerFloat(ref isDirty, waveSetting.randomYRotMin, "Rand. Y Rot. Min", settings);
								
								KillerVariablesHelper.DisplayKillerFloat(ref isDirty, waveSetting.randomYRotMax, "Rand. Y Rot. Max", settings);
                            }

                            if (waveSetting.randomZRotation)
                            {
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
						newExpanded = EditorGUILayout.BeginToggleGroup(incTag, waveSetting.enableIncrements);
						if (newExpanded != waveSetting.enableIncrements) {
                            UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle Incremental Settings");
							waveSetting.enableIncrements = newExpanded;
						}
                        if (waveSetting.enableIncrements)
                        {
                            KillerVariablesHelper.DisplayKillerFloat(ref isDirty, waveSetting.incrementPositionX, "Distance X", settings);

                            KillerVariablesHelper.DisplayKillerFloat(ref isDirty, waveSetting.incrementPositionY, "Distance Y", settings);

                            KillerVariablesHelper.DisplayKillerFloat(ref isDirty, waveSetting.incrementPositionZ, "Distance Z", settings);

                            EditorGUILayout.Separator();

                            if (waveSetting.enableRandomizations && waveSetting.randomXRotation)
                            {
                                DTInspectorUtility.ShowColorWarning("*Rotation X - cannot be used with Random Rotation X.");
                            }
                            else
                            {
                                KillerVariablesHelper.DisplayKillerFloat(ref isDirty, waveSetting.incrementRotX, "Rotation X", settings);
                            }

                            if (waveSetting.enableRandomizations && waveSetting.randomYRotation)
                            {
                                DTInspectorUtility.ShowColorWarning("*Rotation Y - cannot be used with Random Rotation Y.");
                            }
                            else
                            {
                                KillerVariablesHelper.DisplayKillerFloat(ref isDirty, waveSetting.incrementRotY, "Rotation Y", settings);
                            }

                            if (waveSetting.enableRandomizations && waveSetting.randomZRotation)
                            {
                                DTInspectorUtility.ShowColorWarning("*Rotation Z - cannot be used with Random Rotation Z.");
                            }
                            else
                            {
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
						newExpanded = EditorGUILayout.BeginToggleGroup(incTag, waveSetting.enablePostSpawnNudge);
						if (newExpanded != waveSetting.enablePostSpawnNudge) {
                            UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle Post-spawn Nudge Settings");
							waveSetting.enablePostSpawnNudge = newExpanded;
						}
                        if (waveSetting.enablePostSpawnNudge)
                        {
                            KillerVariablesHelper.DisplayKillerFloat(ref isDirty, waveSetting.postSpawnNudgeFwd, "Nudge Forward", settings);

                            KillerVariablesHelper.DisplayKillerFloat(ref isDirty, waveSetting.postSpawnNudgeRgt, "Nudge Right", settings);

                            KillerVariablesHelper.DisplayKillerFloat(ref isDirty, waveSetting.postSpawnNudgeDwn, "Nudge Down", settings);
                        }
                        EditorGUILayout.EndToggleGroup();

                        EditorGUILayout.EndToggleGroup();
                        EditorGUILayout.Separator();
                    }
                }

                if (waveToDelete != null)
                {
                    UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "delete Wave");
                    settings.waveSpecs.Remove(waveToDelete);
                }

                if (waveToInsertAt > -1)
                {
                    if (levelSettings.LevelTimes.Count == 0)
                    {
                        DTInspectorUtility.ShowAlert("You will not have any Level or Wave #'s to select in your Spawner Wave Settings until you add a Level in LevelSettings. Please do that first.");
                    }
                    else
                    {
                        var newWave = new WaveSpecifics();
                        UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "add Wave");
						settings.waveSpecs.Insert(waveToInsertAt + 1, newWave);
                    }
                }

                if (waveToMoveUp.HasValue)
                {
                    var item = settings.waveSpecs[waveToMoveUp.Value];
                    UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "shift up Wave");
					settings.waveSpecs.Insert(waveToMoveUp.Value - 1, item);
                    settings.waveSpecs.RemoveAt(waveToMoveUp.Value + 1);
                }

                if (waveToMoveDown.HasValue)
                {
                    var index = waveToMoveDown.Value + 1;

                    var item = settings.waveSpecs[index];
                    UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "shift down Wave");
					settings.waveSpecs.Insert(index - 1, item);
                    settings.waveSpecs.RemoveAt(index + 1);
                }
            }
        }
        else
        {
            EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndHorizontal();
        }

        if (GUI.changed || isDirty)
        {
            EditorUtility.SetDirty(target);	// or it won't save the data!!
        }

        //DrawDefaultInspector();
    }
	
	private void AddStatModifier(string modifierName, WaveSpecifics spec) {
		if (spec.repeatPassCriteria.HasKey(modifierName)) {
			DTInspectorUtility.ShowAlert("This wave already has a Variable Limit for World Variable: " + modifierName + ". Please modify the existing one instead.");
			return;
		}

        UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "add Variable Limit");
		
		var myVar = WorldVariableTracker.GetWorldVariableScript(modifierName);
		
		spec.repeatPassCriteria.statMods.Add(new WorldVariableModifier(modifierName, myVar.varType));
	}
	
	private void ExpandCollapseAll(bool isExpand) {
        UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle expand / collapse Wave Settings");

		foreach (var wave in settings.waveSpecs) {
			wave.isExpanded = isExpand;
		}
	}
	
	private string[] LevelNames {
		get {
			var names = new string[LevelSettings.Instance.LevelTimes.Count];
			for (var i = 0; i < LevelSettings.Instance.LevelTimes.Count; i++) {
				names[i] = (i + 1).ToString();
			}
			
			return names;
		}
	}

	private int[] LevelIndexes {
		get {
			var indexes = new int[LevelSettings.Instance.LevelTimes.Count];
			
			for (var i = 0; i < LevelSettings.Instance.LevelTimes.Count; i++) {
				indexes[i] = i + 1;
			}
			
			return indexes;
		}
	}
	
	private string[] WaveNamesForLevel(int levelNumber) {
		if (LevelSettings.Instance.LevelTimes.Count <= levelNumber) {
			return new string[0];
		}
		
		var level = LevelSettings.Instance.LevelTimes[levelNumber];
		var names = new string[level.WaveSettings.Count];
		
		for (var i = 0; i < level.WaveSettings.Count; i++) {
			names[i] = (i + 1).ToString();
		}
		
		return names;
	}

	private int[] WaveIndexesForLevel(int levelNumber) {
		if (LevelSettings.Instance.LevelTimes.Count <= levelNumber) {
			return new int[0];
		}

		var level = LevelSettings.Instance.LevelTimes[levelNumber];
		var indexes = new int[level.WaveSettings.Count];
		
		for (var i = 0; i < level.WaveSettings.Count; i++) {
			indexes[i] = i + 1;
		}
		
		return indexes;
	}
	
	private LevelWave GetLevelWaveFromWaveSpec(WaveSpecifics waveSpec) {
		var levelNumber = waveSpec.SpawnLevelNumber;
		var waveNumber = waveSpec.SpawnWaveNumber;
		
		if (LevelSettings.Instance.LevelTimes.Count <= levelNumber) {
			return null;
		}
		
		var wave = LevelSettings.Instance.LevelTimes[levelNumber].WaveSettings[waveNumber];
		return wave;
	}
	
	private float SecondsForWave(WaveSpecifics waveSpec) {
		var wave  = GetLevelWaveFromWaveSpec(waveSpec);
		
		return wave.waveType == LevelSettings.WaveType.Timed ? wave.WaveDuration : 99;
	}

	private void AddActiveLimit(string modifierName) {
		if (settings.activeItemCriteria.HasKey(modifierName)) {
			DTInspectorUtility.ShowAlert("This item already has a Active Limit for World Variable: " + modifierName + ". Please modify the existing one instead.");
			return;
		}
		
		UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "add Active Limit");
		
		var myVar = WorldVariableTracker.GetWorldVariableScript(modifierName);
		
		settings.activeItemCriteria.statMods.Add(new WorldVariableRange(modifierName, myVar.varType));
	}
	
	private void AddBonusStatModifier(string modifierName, WaveSpecifics waveSpec) {
		if (waveSpec.waveRepeatVariableModifiers.HasKey(modifierName)) {
			DTInspectorUtility.ShowAlert("This Wave already has a modifier for World Variable: " + modifierName + ". Please modify that instead.");
			return;
		}

        UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "add Wave Repeat Bonus modifier");
		
		WorldVariable vType = WorldVariableTracker.GetWorldVariableScript(modifierName);
		
		waveSpec.waveRepeatVariableModifiers.statMods.Add(new WorldVariableModifier(modifierName, vType.varType));
	}
}