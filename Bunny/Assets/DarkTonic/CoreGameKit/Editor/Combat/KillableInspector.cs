using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Killable inspector.
/// 3 Steps to make a subclass Inspector (if you're not on Unity 4).
/// 
/// 1) Duplicate the KillableInspector file (this one). Open it.
/// 2) Change "Killable" on line 16 and line 18 to the name of your Killable subclass. Also change the 2 instances of "Killable" on line 25 to the same.
/// 3) Change the "KillableInspector" on line 20 to your Killable subclass + "Inspector". Also change the filename to the same.
/// </summary>

#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
	[CustomEditor(typeof(Killable))]
#else
	[CustomEditor(typeof(Killable), true)]
#endif
public class KillableInspector : Editor {
	private Killable kill;
    private bool isDirty = false;

	public override void OnInspectorGUI() {
        EditorGUIUtility.LookLikeControls();
		EditorGUI.indentLevel = 0;
		
		kill = (Killable) target;

		WorldVariableTracker.ClearInGamePlayerStats();
		
		LevelSettings.Instance = null; // clear cached version
        DTInspectorUtility.DrawTexture(CoreGameKitInspectorResources.logoTexture);
		
		isDirty = false;

		var allStats = KillerVariablesHelper.AllStatNames;

		EditorGUILayout.BeginHorizontal(EditorStyles.objectFieldThumb);
		
		GUILayout.Label("Immediate Actions");
		
		GUILayout.FlexibleSpace();
		
		if (Application.isPlaying) {
			if (SpawnUtility.IsActive(kill.gameObject)) {
				GUI.backgroundColor = Color.green;
				if (GUILayout.Button("Kill", EditorStyles.toolbarButton, GUILayout.Width(82))) {
					kill.DestroyKillable();	
				}
				
				GUILayout.Space(10);
				
				if (GUILayout.Button("Despawn", EditorStyles.toolbarButton, GUILayout.Width(82))) {
					kill.Despawn(TriggeredSpawner.EventType.CodeTriggered1);
				}
				
				GUILayout.Space(10);
				
				if (GUILayout.Button("Take 1 Damage", EditorStyles.toolbarButton, GUILayout.Width(100))) {
					kill.TakeDamage(1);
				}
			} else {
				GUI.contentColor = Color.red;
				GUILayout.Label("Actions are unavailable when despawned.");
			}
		} else {
			GUI.contentColor = Color.green;
			GUILayout.Label("Actions are unavailable during edit mode.");
		}
		
		GUI.contentColor = Color.white;
		GUI.backgroundColor = Color.white;
		EditorGUILayout.EndHorizontal();

		KillerVariablesHelper.DisplayKillerInt(ref isDirty, kill.atckPoints, "Start Attack Points", kill);

		KillerVariablesHelper.DisplayKillerInt(ref isDirty, kill.hitPoints, "Start Hit Points", kill);

		KillerVariablesHelper.DisplayKillerInt(ref isDirty, kill.maxHitPoints, "Max Hit Points", kill);

		EditorGUI.indentLevel = 1;
		if (kill.hitPoints.variableSource == LevelSettings.VariableSource.Variable) {
			var newSync = EditorGUILayout.Toggle("Sync H.P. Variable", kill.syncHitPointWorldVariable);
			if (newSync != kill.syncHitPointWorldVariable) {
				UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "toggle Sync H.P. Variable");
				kill.syncHitPointWorldVariable = newSync;
			}
		}

		EditorGUI.indentLevel = 0;
		if (Application.isPlaying) {
			kill.currentHitPoints = EditorGUILayout.IntSlider("Remaining Hit Points", kill.currentHitPoints, 0, Killable.MAX_ATTACK_POINTS);
		}
		
		var newIgnore = EditorGUILayout.Toggle("Ignore Offscreen Hits", kill.ignoreOffscreenHits);
		if (newIgnore != kill.ignoreOffscreenHits) {
            UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "toggle Ignore Offscreen Hits");
			kill.ignoreOffscreenHits = newIgnore;
		}

		var newExplosion = (Transform) EditorGUILayout.ObjectField("Explosion Prefab", kill.ExplosionPrefab, typeof(Transform), true);
		if (newExplosion != kill.ExplosionPrefab) {
            UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "change Explosion Prefab");
			kill.ExplosionPrefab = newExplosion;
		}

		// retrigger limit section
		var newLimitMode = (TriggeredSpawner.RetriggerLimitMode) EditorGUILayout.EnumPopup("Retrigger Limit Mode", kill.retriggerLimitMode);
		if (newLimitMode != kill.retriggerLimitMode) {
            UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "change Retrigger Limit Mode");
			kill.retriggerLimitMode = newLimitMode;
		}
		switch (kill.retriggerLimitMode) {
			case TriggeredSpawner.RetriggerLimitMode.FrameBased:
                KillerVariablesHelper.DisplayKillerInt(ref isDirty, kill.limitPerXFrame, "Min Frames Between", kill);	
				break;
			case TriggeredSpawner.RetriggerLimitMode.TimeBased:
                KillerVariablesHelper.DisplayKillerFloat(ref isDirty, kill.limitPerSeconds, "Min Seconds Between", kill);
				break;
		}

		var newGO = (TriggeredSpawner.GameOverBehavior) EditorGUILayout.EnumPopup("Game Over Behavior", kill.gameOverBehavior);
		if (newGO != kill.gameOverBehavior) {
            UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "change Game Over Behavior");
			kill.gameOverBehavior = newGO;
		}

		var newLog = EditorGUILayout.Toggle("Log Events", kill.enableLogging);
		if (newLog != kill.enableLogging) {
            UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "toggle Log Events");
			kill.enableLogging = newLog;
		}
		
		var hadNoListener = kill.listener == null;
		var newListener = (KillableListener) EditorGUILayout.ObjectField("Listener", kill.listener, typeof(KillableListener), true);
		if (newListener != kill.listener) {
            UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "assign Listener");
			kill.listener = newListener;
			if (hadNoListener && kill.listener != null) {
				kill.listener.sourceKillableName = kill.transform.name;
			}
		}

		EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
		var newExp = DTInspectorUtility.Foldout(kill.invincibilityExpanded, "Invincibility Settings");
		if (newExp != kill.invincibilityExpanded) {
			UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "toggle expand Invincibility Settings");
			kill.invincibilityExpanded = newExp;
		}
		EditorGUILayout.EndHorizontal();

		if (kill.invincibilityExpanded) {
			var newInvince = EditorGUILayout.Toggle ("Invincible?", kill.isInvincible);
			if (newInvince != kill.isInvincible) {
					UndoHelper.RecordObjectPropertyForUndo (ref isDirty, kill, "toggle Invincible");
					kill.isInvincible = newInvince;
			}

			newInvince = EditorGUILayout.Toggle ("Inv. While Children Alive", kill.invincibleWhileChildrenKillablesExist);
			if (newInvince != kill.invincibleWhileChildrenKillablesExist) {
				UndoHelper.RecordObjectPropertyForUndo (ref isDirty, kill, "toggle Inv. While Children Alive");
				kill.invincibleWhileChildrenKillablesExist = newInvince;
			}

			if (kill.invincibleWhileChildrenKillablesExist) {
				EditorGUI.indentLevel = 1;

				var newDisable = EditorGUILayout.Toggle("Disable Colliders Also", kill.disableCollidersWhileChildrenKillablesExist);
				if (newDisable != kill.disableCollidersWhileChildrenKillablesExist) {
					UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "toggle Disable Colliders Also");
					kill.disableCollidersWhileChildrenKillablesExist = newDisable;
				}
			}

			EditorGUI.indentLevel = 0;
			newInvince = EditorGUILayout.Toggle ("Invincible On Spawn", kill.invincibleOnSpawn);
			if (newInvince != kill.invincibleOnSpawn) {
				UndoHelper.RecordObjectPropertyForUndo (ref isDirty, kill, "toggle Invincible On Spawn");
				kill.invincibleOnSpawn = newInvince;
			}

			if (kill.invincibleOnSpawn) {
				EditorGUI.indentLevel = 1;
				KillerVariablesHelper.DisplayKillerFloat (ref isDirty, kill.invincibleTimeSpawn, "Invincibility Time (sec)", kill, false, true);
			}
		}

		// layer / tag / limit filters
		EditorGUI.indentLevel = 0;
		EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

		var newExpanded = DTInspectorUtility.Foldout(kill.filtersExpanded, "Layer and Tag filters");
		if (newExpanded != kill.filtersExpanded) {
            UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "toggle expand Layer and Tag filters");
			kill.filtersExpanded = newExpanded;
		}
		EditorGUILayout.EndHorizontal();
		
		if (kill.filtersExpanded) {
			EditorGUI.indentLevel = 0;
			DTInspectorUtility.ShowColorWarning("*This section controls which other Killables can damage this one.");

			var newIgnoreSpawned = EditorGUILayout.Toggle("Ignore Killables I Spawn", kill.ignoreKillablesSpawnedByMe);
			if (kill.ignoreKillablesSpawnedByMe != newIgnoreSpawned) {
				UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "toggle Ignore Killables I Spawn");
				kill.ignoreKillablesSpawnedByMe = newIgnoreSpawned;
			}

			EditorGUILayout.Separator();

			var newUseLayer = EditorGUILayout.BeginToggleGroup("Layer Filter", kill.useLayerFilter);
			if (newUseLayer != kill.useLayerFilter) {
                UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "toggle Layer Filter");
				kill.useLayerFilter = newUseLayer;
			}
			if (kill.useLayerFilter) {
				for (var i = 0; i < kill.matchingLayers.Count; i++) {
					var newLayer = EditorGUILayout.LayerField("Layer Match " + (i + 1), kill.matchingLayers[i]);
					if (newLayer != kill.matchingLayers[i]) {
                        UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "change Layer Match");
						kill.matchingLayers[i] = newLayer;
					}
				}
				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(12);
				GUI.contentColor = Color.green;
				if (GUILayout.Button(new GUIContent("Add", "Click to add a Layer Match at the end"), EditorStyles.toolbarButton, GUILayout.Width(60))) {
                    UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "add Layer Match");
					kill.matchingLayers.Add(0);
				}
				GUILayout.Space(10);
				if (kill.matchingLayers.Count > 1) {
					if (GUILayout.Button(new GUIContent("Remove", "Click to remove the last Layer Match"), EditorStyles.toolbarButton, GUILayout.Width(60))) {
                        UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "remove Layer Match");
						kill.matchingLayers.RemoveAt(kill.matchingLayers.Count - 1);
					}
				}
				GUI.contentColor = Color.white;
				EditorGUILayout.EndHorizontal();
			} 
			EditorGUILayout.EndToggleGroup();

			newExpanded = EditorGUILayout.BeginToggleGroup("Tag Filter", kill.useTagFilter);
			if (newExpanded != kill.useTagFilter) {
                UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "toggle Tag Filter");
				kill.useTagFilter = newExpanded;
			}
			if (kill.useTagFilter) {
				for (var i = 0; i < kill.matchingTags.Count; i++) {
					var newTag = EditorGUILayout.TagField("Tag Match " + (i + 1), kill.matchingTags[i]);
					if (newTag != kill.matchingTags[i]) {
                        UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "change Tag Match");
						kill.matchingTags[i] = newTag;
					}
				}
				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(12);
				GUI.contentColor = Color.green;
				if (GUILayout.Button(new GUIContent("Add", "Click to add a Tag Match at the end"), EditorStyles.toolbarButton, GUILayout.Width(60))) {
					UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "add Tag Match");
					kill.matchingTags.Add("Untagged");
				}
				GUILayout.Space(10);
				if (kill.matchingTags.Count > 1) {
					if (GUILayout.Button(new GUIContent("Remove", "Click to remove the last Tag Match"), EditorStyles.toolbarButton, GUILayout.Width(60))) {
						UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "remove Tag Match");
						kill.matchingTags.RemoveAt(kill.matchingLayers.Count - 1);
					}
				}
				GUI.contentColor = Color.white;
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndToggleGroup();

			EditorGUILayout.Separator();
		}

		var poolNames = LevelSettings.GetSortedPrefabPoolNames();
		
		// damage prefab section
		EditorGUI.indentLevel = 0;
		EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
		newExpanded = DTInspectorUtility.Foldout(kill.damagePrefabExpanded, "Damage Prefab Settings");
		if (newExpanded != kill.damagePrefabExpanded) {
            UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "toggle expand Damage Prefab Settings");
			kill.damagePrefabExpanded = newExpanded;
		}
		EditorGUILayout.EndHorizontal();
		EditorGUI.indentLevel = 0;
		if (kill.damagePrefabExpanded) {
			var newSpawnMode = (Killable.DamagePrefabSpawnMode) EditorGUILayout.EnumPopup("Spawn Frequency", kill.damagePrefabSpawnMode);
			if (newSpawnMode != kill.damagePrefabSpawnMode) {
                UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "change Spawn Frequency");
				kill.damagePrefabSpawnMode = newSpawnMode;
			}

			if (kill.damagePrefabSpawnMode != Killable.DamagePrefabSpawnMode.None) {
				if (kill.damagePrefabSpawnMode == Killable.DamagePrefabSpawnMode.PerGroupHitPointsLost) {
                    KillerVariablesHelper.DisplayKillerInt(ref isDirty, kill.damageGroupsize, "Group H.P. Amount", kill);
				}

                KillerVariablesHelper.DisplayKillerInt(ref isDirty, kill.damagePrefabSpawnQuantity, "Spawn Quantity", kill);
				
				var newDmgSource = (Killable.SpawnSource) EditorGUILayout.EnumPopup("Damage Prefab Type", kill.damagePrefabSource);
				if (newDmgSource != kill.damagePrefabSource) {
                    UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "change Damage Prefab Type");
					kill.damagePrefabSource = newDmgSource;
				}
				switch (kill.damagePrefabSource) {
					case Killable.SpawnSource.PrefabPool:
						if (poolNames != null) {
							var pool = LevelSettings.GetFirstMatchingPrefabPool(kill.damagePrefabPoolName);
							var noDmgPool = false;
							var invalidDmgPool = false;
							var noPrefabPools = false;	
						
							if (pool == null) {
								if (string.IsNullOrEmpty(kill.damagePrefabPoolName)) {
									noDmgPool = true;
								} else {
									invalidDmgPool = true;
								}
								kill.damagePrefabPoolIndex = 0;
							} else {
								kill.damagePrefabPoolIndex = poolNames.IndexOf(kill.damagePrefabPoolName);
							}
	
							if (poolNames.Count > 1) {
								var newPoolIndex = EditorGUILayout.Popup("Damage Prefab Pool", kill.damagePrefabPoolIndex, poolNames.ToArray());
								if (newPoolIndex != kill.damagePrefabPoolIndex) {
                                    UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "change Damage Prefab Pool");
									kill.damagePrefabPoolIndex = newPoolIndex;
								}
							
								if (kill.damagePrefabPoolIndex > 0) {						
									var matchingPool = 	LevelSettings.GetFirstMatchingPrefabPool(poolNames[kill.damagePrefabPoolIndex]);
									if (matchingPool != null) {	
										kill.damagePrefabPoolName = matchingPool.name;
									} 
								} else {
									kill.damagePrefabPoolName = string.Empty;
								}
							} else {
								noPrefabPools = true;	
							}
					
							if (noPrefabPools) {
								DTInspectorUtility.ShowRedError("You have no Prefab Pools. Create one first.");
							} else if (noDmgPool) {
								DTInspectorUtility.ShowRedError("No Damage Prefab Pool selected.");
							} else if (invalidDmgPool) {
								DTInspectorUtility.ShowRedError("Damage Prefab Pool '" + kill.damagePrefabPoolName + "' not found. Select one.");	
							}
						} else {
							DTInspectorUtility.ShowRedError(LevelSettings.NO_PREFAB_POOLS_CONTAINER_ALERT);
							DTInspectorUtility.ShowRedError(LevelSettings.REVERT_LEVEL_SETTINGS_ALERT);
						}

						break;
					case Killable.SpawnSource.Specific:
						var newSpecific = (Transform) EditorGUILayout.ObjectField("Damage Prefab", kill.damagePrefabSpecific, typeof(Transform), true);
						if (newSpecific != kill.damagePrefabSpecific) {
                            UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "change Damage Prefab");
							kill.damagePrefabSpecific = newSpecific;
						}
						if (kill.damagePrefabSpecific == null) {
							DTInspectorUtility.ShowRedError("Please assign a Damage prefab.");
						}
						break;
				}
				
				if (kill.damagePrefabSource != Killable.SpawnSource.None) {
					var newOffset = EditorGUILayout.Vector3Field("Spawn Offset", kill.damagePrefabOffset);
					if (newOffset != kill.damagePrefabOffset) {
						UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "change Damage Prefab Spawn Offset");
						kill.damagePrefabOffset = newOffset;
					}
					
					EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(100));
		            EditorGUILayout.LabelField("Random Rotation");
	
					var newRandomX = GUILayout.Toggle(kill.damagePrefabRandomizeXRotation, "X");
					if (newRandomX != kill.damagePrefabRandomizeXRotation) {
	                    UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "toggle Random X Rotation");
						kill.damagePrefabRandomizeXRotation = newRandomX;
					}
					GUILayout.Space(10);
					var newRandomY = GUILayout.Toggle(kill.damagePrefabRandomizeYRotation, "Y");
					if (newRandomY != kill.damagePrefabRandomizeYRotation) {
	                    UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "toggle Random Y Rotation");
						kill.damagePrefabRandomizeYRotation = newRandomY;
					}
					GUILayout.Space(10);
					var newRandomZ = GUILayout.Toggle(kill.damagePrefabRandomizeZRotation, "Z");
					if (newRandomZ != kill.damagePrefabRandomizeZRotation) {
	                    UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "toggle Random Z Rotation");
						kill.damagePrefabRandomizeZRotation = newRandomZ;
					}
		            EditorGUILayout.EndHorizontal();
				}
				
			} else {
				DTInspectorUtility.ShowColorWarning("Change Spawn Frequency to show more settings.");
			}
		}		
		
		// player stat damage modifiers
		EditorGUI.indentLevel = 0;
		EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
		newExpanded = DTInspectorUtility.Foldout(kill.despawnStatDamageModifiersExpanded, "Damage World Variable Modifiers");
		if (newExpanded != kill.despawnStatDamageModifiersExpanded) {
            UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "toggle expand Damage World Variable Modifiers");
			kill.despawnStatDamageModifiersExpanded = newExpanded;
		}
		EditorGUILayout.EndHorizontal();
		EditorGUI.indentLevel = 0;
		if (kill.despawnStatDamageModifiersExpanded) {
			var missingStatNames = new List<string>();
			missingStatNames.AddRange(allStats);
			missingStatNames.RemoveAll(delegate(string obj) {
				return kill.playerStatDamageModifiers.HasKey(obj);	
			});
			
			var newStat = EditorGUILayout.Popup("Add Variable Modifer", 0, missingStatNames.ToArray());
			if (newStat != 0) {
				AddStatModifier(missingStatNames[newStat], kill.playerStatDamageModifiers);
			}
			
			if (kill.playerStatDamageModifiers.statMods.Count == 0) {
				DTInspectorUtility.ShowColorWarning("*You currently have no damage modifiers for this prefab.");
			} else {
				EditorGUILayout.Separator();
				
				int? indexToDelete = null;
				
				for (var i = 0; i < kill.playerStatDamageModifiers.statMods.Count; i++) {
					var modifier = kill.playerStatDamageModifiers.statMods[i];
					
					var buttonPressed = DTInspectorUtility.FunctionButtons.None;
					switch (modifier._varTypeToUse) {
						case WorldVariableTracker.VariableType._integer:
                            buttonPressed = KillerVariablesHelper.DisplayKillerInt(ref isDirty, modifier._modValueIntAmt, modifier._statName, kill, true, true);	
							break;
						case WorldVariableTracker.VariableType._float:
                            buttonPressed = KillerVariablesHelper.DisplayKillerFloat(ref isDirty, modifier._modValueFloatAmt, modifier._statName, kill, true, true);	
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
                    UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "delete Modifier");
					kill.playerStatDamageModifiers.DeleteByIndex(indexToDelete.Value);
				}
				
				EditorGUILayout.Separator();
			}
		} 
		
		// despawn trigger section
		EditorGUI.indentLevel = 0;
		EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
		newExpanded = DTInspectorUtility.Foldout(kill.showVisibilitySettings, "Despawn & Death Triggers");
		if (newExpanded != kill.showVisibilitySettings) {
            UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "toggle expand Despawn Triggers");
			kill.showVisibilitySettings = newExpanded;
		}
		EditorGUILayout.EndHorizontal();
		EditorGUI.indentLevel = 0;
		if (kill.showVisibilitySettings) {
			var newSpawnerDest = (Killable.SpawnerDestroyedBehavior) EditorGUILayout.EnumPopup("If Spawner Destroyed? ", kill.spawnerDestroyedAction);
			if (newSpawnerDest != kill.spawnerDestroyedAction) {
                UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "change If Spawner Destroyed");
				kill.spawnerDestroyedAction = newSpawnerDest;
			}

			var newTimer = EditorGUILayout.Toggle("Use Death Timer", kill.timerDeathEnabled);
			if (newTimer != kill.timerDeathEnabled) {
				UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "toggle Use Death Timer");
				kill.timerDeathEnabled = newTimer;
			}

			if (kill.timerDeathEnabled) {
				KillerVariablesHelper.DisplayKillerFloat(ref isDirty, kill.timerDeathSeconds, "Death Timer (sec)", kill, false, true);
				EditorGUI.indentLevel = 1;
				var newTimerAction = (Killable.SpawnerDestroyedBehavior) EditorGUILayout.EnumPopup("Time Up Action", kill.timeUpAction);
				if (newTimerAction != kill.timeUpAction) {
					UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "change Time Up Action");
					kill.timeUpAction = newTimerAction;
				}
			}

			EditorGUILayout.Separator();
			
			EditorGUI.indentLevel = 0;
			EditorGUILayout.LabelField("Despawn Triggers", EditorStyles.boldLabel);
				
			EditorGUI.indentLevel = 1;
			var newOffscreen = EditorGUILayout.Toggle("Invisible Event", kill.despawnWhenOffscreen);
			if (newOffscreen != kill.despawnWhenOffscreen) {
                UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "toggle Invisible Event");
				kill.despawnWhenOffscreen = newOffscreen;
			}

			var newNotVisible = EditorGUILayout.Toggle("Not Visible Too Long", kill.despawnIfNotVisible);
			if (newNotVisible != kill.despawnIfNotVisible) {
                UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "toggle Not Visible Too Long");
				kill.despawnIfNotVisible = newNotVisible;
			}

			if (kill.despawnIfNotVisible) {
                KillerVariablesHelper.DisplayKillerFloat(ref isDirty, kill.despawnIfNotVisibleForSec, "Not Visible Max Time", kill, false, false);
			}
			
			EditorGUILayout.Separator();
			EditorGUI.indentLevel = 0;
			EditorGUILayout.LabelField("Death Triggers", EditorStyles.boldLabel);

			EditorGUI.indentLevel = 1;
			var newClick = EditorGUILayout.Toggle("MouseDown Event", kill.despawnOnMouseClick);
			if (newClick != kill.despawnOnMouseClick) {
                UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "toggle MouseDown Event");
				kill.despawnOnMouseClick = newClick;
			}
			
			newClick = EditorGUILayout.Toggle("OnClick Event (NGUI)", kill.despawnOnClick);
			if (newClick != kill.despawnOnClick) {
                UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "toggle OnClick Event (NGUI)");
				kill.despawnOnClick = newClick;
			}

			var newDespawn = (Killable.DespawnMode) EditorGUILayout.EnumPopup("HP Death Mode", kill.despawnMode);
			if (newDespawn != kill.despawnMode) {
                UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "change HP Death Mode");
				kill.despawnMode = newDespawn;
			}
		}
		
		// death prefab section
		EditorGUI.indentLevel = 0;
		EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
		newExpanded = DTInspectorUtility.Foldout(kill.deathPrefabSettingsExpanded, "Death Prefab Settings");
		if (newExpanded != kill.deathPrefabSettingsExpanded) {
            UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "toggle expand Death Prefab Settings");
			kill.deathPrefabSettingsExpanded = newExpanded;
		}
		EditorGUILayout.EndHorizontal();
		EditorGUI.indentLevel = 0;
		if (kill.deathPrefabSettingsExpanded) {
			KillerVariablesHelper.DisplayKillerFloat(ref isDirty, kill.deathDelay, "Death Delay (sec)", kill, false);
			
			var newDeathSource = (WaveSpecifics.SpawnOrigin) EditorGUILayout.EnumPopup("Death Prefab Type", kill.deathPrefabSource);
			if (newDeathSource != kill.deathPrefabSource) {
                UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "change Death Prefab Type");
				kill.deathPrefabSource = newDeathSource;
			}
			switch (kill.deathPrefabSource) {
				case WaveSpecifics.SpawnOrigin.PrefabPool:
					if (poolNames != null) {
						var pool = LevelSettings.GetFirstMatchingPrefabPool(kill.deathPrefabPoolName);
						var noDeathPool = false;
						var illegalDeathPref = false;
						var noPrefabPools = false;
					
						if (pool == null) {
							if (string.IsNullOrEmpty(kill.deathPrefabPoolName)) {
								noDeathPool = true;
							} else {
								illegalDeathPref = true;
							}
							kill.deathPrefabPoolIndex = 0;
						} else {
							kill.deathPrefabPoolIndex = poolNames.IndexOf(kill.deathPrefabPoolName);
						}

						if (poolNames.Count > 1) {
							var newDeathPool = EditorGUILayout.Popup("Death Prefab Pool", kill.deathPrefabPoolIndex, poolNames.ToArray());
							if (newDeathPool != kill.deathPrefabPoolIndex) {
                                UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "change Death Prefab Pool");
								kill.deathPrefabPoolIndex = newDeathPool;
							}
						
							if (kill.deathPrefabPoolIndex > 0) {						
								var matchingPool = 	LevelSettings.GetFirstMatchingPrefabPool(poolNames[kill.deathPrefabPoolIndex]);
								if (matchingPool != null) {	
									kill.deathPrefabPoolName = matchingPool.name;
								}
							} else {
								kill.deathPrefabPoolName = string.Empty;
							}
						} else {
							noPrefabPools = true;
						}
						
						if (noPrefabPools) {
							DTInspectorUtility.ShowRedError("You have no Prefab Pools. Create one first.");
						} else if (noDeathPool) {
							DTInspectorUtility.ShowRedError("No Death Prefab Pool selected.");
						} else if (illegalDeathPref) {
							DTInspectorUtility.ShowRedError("Death Prefab Pool '" + kill.deathPrefabPoolName + "' not found. Select one.");
						}
					} else {
						DTInspectorUtility.ShowRedError(LevelSettings.NO_PREFAB_POOLS_CONTAINER_ALERT);
						DTInspectorUtility.ShowRedError(LevelSettings.REVERT_LEVEL_SETTINGS_ALERT);
					}
					break;
				case WaveSpecifics.SpawnOrigin.Specific:
					var newDeathSpecific = (Transform) EditorGUILayout.ObjectField("Death Prefab", kill.deathPrefabSpecific, typeof(Transform), true);
					if (newDeathSpecific != kill.deathPrefabSpecific) {
                        UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "change Death Prefab");
						kill.deathPrefabSpecific = newDeathSpecific;
					}
				
					if (kill.deathPrefabSpecific == null) {
						DTInspectorUtility.ShowRedError("Please assign a Death prefab.");
					}
				
					break;
			}

			var newKeepParent = EditorGUILayout.Toggle("Keep Same Parent", kill.deathPrefabKeepSameParent);
			if (newKeepParent != kill.deathPrefabKeepSameParent) {
				UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "toggle Keep Same Parent");
				kill.deathPrefabKeepSameParent = newKeepParent;
			}

            KillerVariablesHelper.DisplayKillerInt(ref isDirty, kill.deathPrefabSpawnPercent, "Spawn % Chance", kill);

            KillerVariablesHelper.DisplayKillerInt(ref isDirty, kill.deathPrefabQty, "Spawn Quantity", kill);
			 
			var newDeathOffset = EditorGUILayout.Vector3Field("Spawn Offset", kill.deathPrefabOffset);
			if (newDeathOffset != kill.deathPrefabOffset) {
                UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "change Spawn Offset");
				kill.deathPrefabOffset = newDeathOffset;
			}
			
			if (kill.Body == null || kill.Body.isKinematic) {
				DTInspectorUtility.ShowColorWarning("*Inherit Velocity can only be used on gravity rigidbodies");
			} else {
				var newKeep = EditorGUILayout.Toggle("Inherit Velocity", kill.deathPrefabKeepVelocity);
				if (newKeep != kill.deathPrefabKeepVelocity) {
                    UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "toggle Inherit Velocity");
					kill.deathPrefabKeepVelocity = newKeep;
				}
			}

			var newMode = (Killable.RotationMode) EditorGUILayout.EnumPopup("Rotation Mode", kill.rotationMode);
			if (newMode != kill.rotationMode) {
                UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "change Rotation Mode");
				kill.rotationMode = newMode;
			}
			if (kill.rotationMode == Killable.RotationMode.CustomRotation) {
				var newCustomRot = EditorGUILayout.Vector3Field("Custom Rotation Euler", kill.deathPrefabCustomRotation);
				if (newCustomRot != kill.deathPrefabCustomRotation) {
                    UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "change Custom Rotation Euler");
					kill.deathPrefabCustomRotation = newCustomRot;
				}
			}
		}
		
		// player stat modifiers
		EditorGUI.indentLevel = 0;
		EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
		newExpanded = DTInspectorUtility.Foldout(kill.despawnStatModifiersExpanded, "Death World Variable Modifier Scenarios");
		if (newExpanded != kill.despawnStatModifiersExpanded) {
            UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "toggle expand Death World Variable Modifier Scenarios");
			kill.despawnStatModifiersExpanded = newExpanded;
		}
		EditorGUILayout.EndHorizontal();
		EditorGUI.indentLevel = 0;
		if (kill.despawnStatModifiersExpanded) {
			EditorGUILayout.BeginHorizontal(EditorStyles.objectFieldThumb);
			EditorGUILayout.LabelField("If \"" + Killable.DESTROYED_TEXT + "\"");
			GUI.backgroundColor = Color.green;
			if (GUILayout.Button(new GUIContent("Add Else"), EditorStyles.miniButtonMid, GUILayout.MaxWidth(80))) {
				AddModifierElse(kill);
			}
			GUI.backgroundColor = Color.white;
			EditorGUILayout.EndHorizontal();

			EditorGUI.indentLevel = 0;
			
			var missingStatNames = new List<string>();
			missingStatNames.AddRange(allStats);
			missingStatNames.RemoveAll(delegate(string obj) {
				return kill.playerStatDespawnModifiers.HasKey(obj);	
			});
			
			var newStat = EditorGUILayout.Popup("Add Variable Modifer", 0, missingStatNames.ToArray());
			if (newStat != 0) {
				AddStatModifier(missingStatNames[newStat], kill.playerStatDespawnModifiers);
			}
			
			if (kill.playerStatDespawnModifiers.statMods.Count == 0) {
				DTInspectorUtility.ShowColorWarning("*You currently have no death modifiers for this prefab.");
			} else {
				EditorGUILayout.Separator();
				
				int? indexToDelete = null;
				
				for (var i = 0; i < kill.playerStatDespawnModifiers.statMods.Count; i++) {
					var modifier = kill.playerStatDespawnModifiers.statMods[i];
					
					var buttonPressed = DTInspectorUtility.FunctionButtons.None;
					switch (modifier._varTypeToUse) {
						case WorldVariableTracker.VariableType._integer:
                            buttonPressed = KillerVariablesHelper.DisplayKillerInt(ref isDirty, modifier._modValueIntAmt, modifier._statName, kill, true, true);	
							break;
						case WorldVariableTracker.VariableType._float:
                            buttonPressed = KillerVariablesHelper.DisplayKillerFloat(ref isDirty, modifier._modValueFloatAmt, modifier._statName, kill, true, true);	
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
                    UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "delete Modifier");
					kill.playerStatDespawnModifiers.DeleteByIndex(indexToDelete.Value);
				}
				
				EditorGUILayout.Separator();
			}

			// alternate cases
			WorldVariableCollection alternate = null;
			int? iElseToDelete = null;
			for (var i = 0; i < kill.alternateModifiers.Count; i++) {
				alternate = kill.alternateModifiers[i];
				
				EditorGUI.indentLevel = 0;
				EditorGUILayout.BeginHorizontal(EditorStyles.objectFieldThumb);
				GUILayout.Label("Else If", GUILayout.Width(40));
				var newScen = EditorGUILayout.TextField(alternate.scenarioName, GUILayout.MaxWidth(150));
				if (newScen != alternate.scenarioName) {
                    UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "change Scenario name");
					alternate.scenarioName = newScen;
				}
				GUILayout.FlexibleSpace();
				GUI.backgroundColor = Color.green;
				if (GUILayout.Button(new GUIContent("Delete Else"), EditorStyles.miniButtonMid, GUILayout.MaxWidth(80))) {
					iElseToDelete = i;
				}
				GUI.backgroundColor = Color.white;
				EditorGUILayout.EndHorizontal();

				EditorGUI.indentLevel = 0;
				// display modifers
				
				missingStatNames = new List<string>();
				missingStatNames.AddRange(allStats);
				missingStatNames.RemoveAll(delegate(string obj) {
					return alternate.HasKey(obj);	
				});				
				
				var newMod = EditorGUILayout.Popup("Add Variable Modifer", 0, missingStatNames.ToArray());
				if (newMod != 0) {
					AddStatModifier(missingStatNames[newMod], alternate);
				}
				
				if (alternate.statMods.Count == 0) {
					DTInspectorUtility.ShowColorWarning("*You currently are using no Modifiers for this prefab.");
				} else {
					EditorGUILayout.Separator();
					
					int? indexToDelete = null;
					
					for (var j = 0; j < alternate.statMods.Count; j++) {
						var modifier = alternate.statMods[j];
						
						var buttonPressed = DTInspectorUtility.FunctionButtons.None;
						switch (modifier._varTypeToUse) {
							case WorldVariableTracker.VariableType._integer:
                                buttonPressed = KillerVariablesHelper.DisplayKillerInt(ref isDirty, modifier._modValueIntAmt, modifier._statName, kill, true, true);	
								break;
							case WorldVariableTracker.VariableType._float:
                                buttonPressed = KillerVariablesHelper.DisplayKillerFloat(ref isDirty, modifier._modValueFloatAmt, modifier._statName, kill, true, true);	
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
                        UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "delete Modifier");
						alternate.DeleteByIndex(indexToDelete.Value);
					}
					
					EditorGUILayout.Separator();
				}
			}
			
			if (iElseToDelete.HasValue) {
                UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "delete Scenario");
				kill.alternateModifiers.RemoveAt(iElseToDelete.Value);
			}
		} 
			
		
		// respawn settings section
		EditorGUI.indentLevel = 0;
		EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
		newExpanded = DTInspectorUtility.Foldout(kill.showRespawnSettings, "Respawn Settings");
		if (newExpanded != kill.showRespawnSettings) {
            UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "toggle expand Respawn Settings");
			kill.showRespawnSettings = newExpanded;
		}
		EditorGUILayout.EndHorizontal();
		EditorGUI.indentLevel = 0;
		if (kill.showRespawnSettings) {
			var newRespawn = (Killable.RespawnType) EditorGUILayout.EnumPopup("Death Respawn Type", kill.respawnType);
			if (newRespawn != kill.respawnType) {
                UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "toggle Death Respawn Type");
				kill.respawnType = newRespawn;
			}
			
			if (kill.respawnType == Killable.RespawnType.SetNumber) {
				var newTimes = EditorGUILayout.IntSlider("Times to Respawn", kill.timesToRespawn, 1, int.MaxValue);
				if (newTimes != kill.timesToRespawn) {
                    UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "change Times to Respawn");
					kill.timesToRespawn = newTimes;
				}
				
				if (Application.isPlaying) {
					GUI.contentColor = Color.yellow;
					GUILayout.Label("Times Respawned: " + kill.timesRespawned);
					GUI.contentColor = Color.white;
				}
			}
			
			if (kill.respawnType != Killable.RespawnType.None) {
				KillerVariablesHelper.DisplayKillerFloat(ref isDirty, kill.respawnDelay, "Respawn Delay (sec)", kill, false);
			}
		}
		
		if (GUI.changed || isDirty) {
  			EditorUtility.SetDirty(target);	// or it won't save the data!!
		}
		
		//DrawDefaultInspector();
    }
	
	private void AddModifierElse(Killable kil) {
        UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kil, "add Else");

		kil.alternateModifiers.Add(new WorldVariableCollection());
	}
	
	private void AddStatModifier(string modifierName, WorldVariableCollection modifiers) {
		if (modifiers.HasKey(modifierName)) {
			DTInspectorUtility.ShowAlert("This Killable already has a modifier for World Variable: " + modifierName + ". Please modify that instead.");
			return;
		}

        UndoHelper.RecordObjectPropertyForUndo(ref isDirty, kill, "add Modifier");
		
		var myVar = WorldVariableTracker.GetWorldVariableScript(modifierName);
		
		modifiers.statMods.Add(new WorldVariableModifier(modifierName, myVar.varType));
	}

}