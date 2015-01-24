using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Text;

[CustomEditor(typeof(LevelSettings))]
public class LevelSettingsInspector : Editor {
	public static readonly Color inactiveClr = new Color(.00f, .77f, .33f);
	public static readonly Color activeClr = new Color(.33f, .99f, .66f);

	private LevelSettings settings;
	private bool isDirty = false; 

	public override void OnInspectorGUI() {
        EditorGUIUtility.LookLikeControls();
		EditorGUI.indentLevel = 0;
		
		settings = (LevelSettings)target;

		var isInProjectView = DTInspectorUtility.IsPrefabInProjectView(settings);
		
		WorldVariableTracker.ClearInGamePlayerStats();

        DTInspectorUtility.DrawTexture(CoreGameKitInspectorResources.logoTexture);

		isDirty = false;

		if (isInProjectView) {
			DTInspectorUtility.ShowLargeBarAlert("*You have selected the LevelWaveSettings prefab in Project View.");
			DTInspectorUtility.ShowRedError("Do not drag this prefab into the Scene. It will be linked to this prefab if you do.");
			DTInspectorUtility.ShowLargeBarAlert("*Click the button below to create a LevelWaveSettings prefab in the Scene.");
			
			EditorGUILayout.Separator();
			
			GUI.contentColor = Color.green;
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(10);
			if (GUILayout.Button("Create LevelWaveSettings Prefab", EditorStyles.toolbarButton, GUILayout.Width(180))) {
				CreateLevelSettingsPrefab();
			}
			EditorGUILayout.EndHorizontal();
			GUI.contentColor = Color.white;
			return;
		}
		
		var allStats = KillerVariablesHelper.AllStatNames;
		
		var playerStatsHolder = settings.transform.FindChild(LevelSettings.WORLD_VARIABLES_CONTAINER_TRANS_NAME);
		if (playerStatsHolder == null) {
			Debug.LogError("You have no child prefab of LevelSettings called '" + LevelSettings.WORLD_VARIABLES_CONTAINER_TRANS_NAME + "'. " + LevelSettings.REVERT_LEVEL_SETTINGS_ALERT);
			DTInspectorUtility.ShowRedError("Please check the console. You have a breaking error.");
			return;
		}

		EditorGUI.indentLevel = 0;

		var newUseWaves = EditorGUILayout.BeginToggleGroup("Use Global Waves", settings.useWaves);
		if (newUseWaves != settings.useWaves) {
			UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle Use Global Waves");
			settings.useWaves = newUseWaves;
		}

		if (settings.useWaves) {
			EditorGUI.indentLevel = 1;

			var newUseMusic = EditorGUILayout.Toggle("Use Music Settings", settings.useMusicSettings);
			if (newUseMusic != settings.useMusicSettings) {
				UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle Use Music Settings");
				settings.useMusicSettings = newUseMusic;
			}
			
			if (settings.useMusicSettings) {
				EditorGUI.indentLevel = 2;
				
				var newGoMusic = (LevelSettings.WaveMusicMode) EditorGUILayout.EnumPopup("G.O. Music Mode", settings.gameOverMusicSettings.WaveMusicMode);
				if (newGoMusic != settings.gameOverMusicSettings.WaveMusicMode) {
					UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change G.O. Music Mode");
					settings.gameOverMusicSettings.WaveMusicMode = newGoMusic;
				}
				if (settings.gameOverMusicSettings.WaveMusicMode == LevelSettings.WaveMusicMode.PlayNew) {
					var newWaveMusic = (AudioClip) EditorGUILayout.ObjectField("G.O. Music", settings.gameOverMusicSettings.WaveMusic, typeof(AudioClip), true);
					if (newWaveMusic != settings.gameOverMusicSettings.WaveMusic) {
						UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "assign G.O. Music");
						settings.gameOverMusicSettings.WaveMusic = newWaveMusic;
					}
				}
				if (settings.gameOverMusicSettings.WaveMusicMode != LevelSettings.WaveMusicMode.Silence) {
					var newMusicVol = EditorGUILayout.Slider("G.O. Music Volume", settings.gameOverMusicSettings.WaveMusicVolume, 0f, 1f);
					if (newMusicVol != settings.gameOverMusicSettings.WaveMusicVolume) {
						UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change G.O. Music Volume");
						settings.gameOverMusicSettings.WaveMusicVolume = newMusicVol;
					}
				} else {
					var newFadeTime = EditorGUILayout.Slider("Silence Fade Time", settings.gameOverMusicSettings.FadeTime, 0f, 15f);
					if (newFadeTime != settings.gameOverMusicSettings.FadeTime) {
						UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Silence Fade Time");
						settings.gameOverMusicSettings.FadeTime = newFadeTime;
					}
				}
			}
	
			EditorGUI.indentLevel = 1;
			var newDisableSyncro = EditorGUILayout.Toggle ("Syncro Spawners Off", settings.disableSyncroSpawners);
			if (newDisableSyncro != settings.disableSyncroSpawners) {
					UndoHelper.RecordObjectPropertyForUndo (ref isDirty, settings, "toggle Syncro Spawners Off");
					settings.disableSyncroSpawners = newDisableSyncro;
			}

			var newStart = EditorGUILayout.Toggle ("Auto Start Waves", settings.startFirstWaveImmediately);
			if (newStart != settings.startFirstWaveImmediately) {		
					UndoHelper.RecordObjectPropertyForUndo (ref isDirty, settings, "toggle Auto Start Waves");
					settings.startFirstWaveImmediately = newStart;
			}

			var newDestroy = (LevelSettings.WaveRestartBehavior)EditorGUILayout.EnumPopup ("Wave Restart Mode", settings.waveRestartMode);
			if (newDestroy != settings.waveRestartMode) {
					UndoHelper.RecordObjectPropertyForUndo (ref isDirty, settings, "toggle Wave Restart Mode");
					settings.waveRestartMode = newDestroy;
			}

			var newEnableWarp = EditorGUILayout.Toggle ("Custom Start Wave?", settings.enableWaveWarp);
			if (newEnableWarp != settings.enableWaveWarp) {
					UndoHelper.RecordObjectPropertyForUndo (ref isDirty, settings, "toggle Custom Start Wave?");
					settings.enableWaveWarp = newEnableWarp;
			}

			if (settings.enableWaveWarp) {
					EditorGUI.indentLevel = 0;

					KillerVariablesHelper.DisplayKillerInt (ref isDirty, settings.startLevelNumber, "Custom Start Level#", settings, false, true);
					KillerVariablesHelper.DisplayKillerInt (ref isDirty, settings.startWaveNumber, "Custom Start Wave#", settings, false, true);
			}
		}

		EditorGUILayout.EndToggleGroup();
		
		EditorGUI.indentLevel = 0;
		
		var newPersist = EditorGUILayout.Toggle("Persist Between Scenes", settings.persistBetweenScenes);
		if (newPersist != settings.persistBetweenScenes) {
			UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle Persist Between Scenes");
			settings.persistBetweenScenes = newPersist;
		}
		
		var newLogging = EditorGUILayout.Toggle("Log Messages", settings.isLoggingOn);
		if (newLogging != settings.isLoggingOn) {
			UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle Log Messages");
			settings.isLoggingOn = newLogging;
		}
		
		var hadNoListener = settings.listener == null;
		var newListener = (LevelSettingsListener) EditorGUILayout.ObjectField("Listener", settings.listener, typeof(LevelSettingsListener), true);
		if (newListener != settings.listener) {
			UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "assign Listener");
			settings.listener = newListener;
			if (hadNoListener && settings.listener != null) {
				settings.listener.sourceTransName = settings.transform.name;
			}
		}

		EditorGUILayout.Separator();
		
        // show Pool Boss section
		GUI.color = settings.killerPoolingExpanded ? activeClr : inactiveClr;

		EditorGUILayout.BeginHorizontal(EditorStyles.objectFieldThumb);
		EditorGUI.indentLevel = 0;
        var newExpanded = EditorGUILayout.Toggle("Show Pool Boss", settings.killerPoolingExpanded);
		if (newExpanded != settings.killerPoolingExpanded) {
			UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle Show Pool Boss");
			settings.killerPoolingExpanded = newExpanded;
		}
		EditorGUILayout.EndHorizontal();
		GUI.color = Color.white;

		var poolingHolder = settings.transform.FindChild(LevelSettings.KILLER_POOLING_CONTAINER_TRANS_NAME);
		if (poolingHolder == null) {
			Debug.LogError("You have no child prefab of LevelSettings called '" + LevelSettings.KILLER_POOLING_CONTAINER_TRANS_NAME + "'. " + LevelSettings.REVERT_LEVEL_SETTINGS_ALERT);
			return;
		}
		if (settings.killerPoolingExpanded) {
			PoolBoss kp = poolingHolder.GetComponent<PoolBoss>();
			if (kp == null) {
				Debug.LogError("You have no PoolBoss script on your " +  LevelSettings.KILLER_POOLING_CONTAINER_TRANS_NAME + " subprefab. " + LevelSettings.REVERT_LEVEL_SETTINGS_ALERT);
				return;
			}
			
			DTInspectorUtility.ShowColorWarning(string.Format("*You have {0} Pool Items set up. Click the button to configure Pooling.", kp.poolItems.Count));
			
			EditorGUILayout.BeginHorizontal();
			GUI.contentColor = Color.green;
			GUILayout.Label("", GUILayout.Width(147));
			if (GUILayout.Button("Configure Pooling", EditorStyles.toolbarButton, GUILayout.Width(120))) {
				Selection.activeGameObject = poolingHolder.gameObject;
			}
			GUI.contentColor = Color.white;
			
			EditorGUILayout.EndHorizontal();
		}
        // end Pool Boss section
		
		// create Prefab Pools section
		GUI.color = settings.createPrefabPoolsExpanded ? activeClr : inactiveClr;
		EditorGUILayout.BeginHorizontal(EditorStyles.objectFieldThumb);
		EditorGUI.indentLevel = 0;
		newExpanded = EditorGUILayout.Toggle("Show Prefab Pools", settings.createPrefabPoolsExpanded);
		if (newExpanded != settings.createPrefabPoolsExpanded) {
			UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle Show Prefab Pools");
			settings.createPrefabPoolsExpanded = newExpanded;
		}
		
		if (settings.createPrefabPoolsExpanded) {
	        // BUTTONS...
	        EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(100));
			EditorGUI.indentLevel = 0;
	
	        // Add expand/collapse buttons if there are items in the list
	
	        EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(100));
	        // A little space between button groups
	        GUILayout.Space(6);
			
			EditorGUILayout.EndHorizontal();
	        EditorGUILayout.EndHorizontal();
	        EditorGUILayout.EndHorizontal();
			GUI.color = Color.white;

			EditorGUILayout.LabelField("Prefab Pools", EditorStyles.miniBoldLabel);
			
			var pools = LevelSettings.GetAllPrefabPools;
			if (pools.Count == 0) {
				DTInspectorUtility.ShowColorWarning("*You currently have no Prefab Pools.");
			}

			GUI.backgroundColor = Color.cyan;
			for (var i = 0; i < pools.Count; i++) {
				var pool = pools[i];
				EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
				GUILayout.Label(pool.name);
				GUILayout.FlexibleSpace();
				
				var buttonPressed = DTInspectorUtility.AddControlButtons(settings, "Prefab Pool");
				if (buttonPressed == DTInspectorUtility.FunctionButtons.Edit) {
					Selection.activeGameObject = pool.gameObject;
				}
				
				EditorGUILayout.EndHorizontal();
			}
			GUI.backgroundColor = Color.white;
			
			EditorGUILayout.Separator();
			EditorGUILayout.LabelField("Create New", EditorStyles.miniBoldLabel);
			var newPoolName = EditorGUILayout.TextField("New Pool Name", settings.newPrefabPoolName);
			if (newPoolName != settings.newPrefabPoolName) {
				UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change New Pool Name");
				settings.newPrefabPoolName = newPoolName;
			}
	        
			EditorGUILayout.BeginHorizontal(EditorStyles.boldLabel);
			EditorGUILayout.LabelField("", EditorStyles.miniLabel);
			GUILayout.Space(7);
			GUI.contentColor = Color.green;
			if (GUILayout.Button("Create Prefab Pool", EditorStyles.toolbarButton, GUILayout.MaxWidth(110))) {
				CreatePrefabPool();		
			}
			GUI.contentColor = Color.white;
			GUILayout.FlexibleSpace();
		}
		EditorGUILayout.EndHorizontal();
		GUI.color = Color.white;
		// end create prefab pools section
		
		// create spawners section
		GUI.color = settings.createSpawnersExpanded ? activeClr : inactiveClr;
		EditorGUILayout.BeginHorizontal(EditorStyles.objectFieldThumb);
		EditorGUI.indentLevel = 0;
		
		newExpanded = EditorGUILayout.Toggle("Show Syncro Spawners", settings.createSpawnersExpanded);
		if (newExpanded != settings.createSpawnersExpanded) {
			UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle Show Syncro Spawners");
			settings.createSpawnersExpanded = newExpanded;
		}
		
		if (settings.createSpawnersExpanded) {
	        // BUTTONS...
	        EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(100));
			EditorGUI.indentLevel = 0;
	
	        // Add expand/collapse buttons if there are items in the list
	
	        EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(100));
	        // A little space between button groups
	        GUILayout.Space(6);
			
			EditorGUILayout.EndHorizontal();
	        EditorGUILayout.EndHorizontal();
	        EditorGUILayout.EndHorizontal();
			// end create spawners section
			GUI.color = Color.white;

			EditorGUILayout.LabelField("Spawners", EditorStyles.miniBoldLabel);
			var spawners = LevelSettings.GetAllSpawners;
			if (spawners.Count == 0) {
				DTInspectorUtility.ShowColorWarning("*You currently have no Syncro Spawners.");
			}
			
			GUI.backgroundColor = Color.cyan;
			for (var i = 0; i < spawners.Count; i++) {
				var spawner = spawners[i];
				EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
				GUILayout.Label(spawner.name);
				GUILayout.FlexibleSpace();
				var buttonPressed = DTInspectorUtility.AddControlButtons(settings, "Spawner");
				if (buttonPressed == DTInspectorUtility.FunctionButtons.Edit) {
					Selection.activeGameObject = spawner.gameObject;
				}
				EditorGUILayout.EndHorizontal();
			}
			GUI.backgroundColor = Color.white;
			
			EditorGUILayout.Separator();
			EditorGUILayout.LabelField("Create New", EditorStyles.miniBoldLabel);
			
			var newName = EditorGUILayout.TextField("New Spawner Name", settings.newSpawnerName);
			if (newName != settings.newSpawnerName) {
				UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change New Spawner Name");
				settings.newSpawnerName = newName;
			}

			var newType = (LevelSettings.SpawnerType) EditorGUILayout.EnumPopup("New Spawner Color", settings.newSpawnerType);
			if (newType != settings.newSpawnerType) {
				UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change New Spawner Color");
				settings.newSpawnerType = newType;
			}
	        
			EditorGUILayout.BeginHorizontal(EditorStyles.boldLabel);
			EditorGUILayout.LabelField("", EditorStyles.miniLabel);
			GUILayout.Space(7);
			GUI.contentColor = Color.green;
			if (GUILayout.Button("Create Spawner", EditorStyles.toolbarButton, GUILayout.MaxWidth(110))) {
				CreateSpawner();		
			}
			GUI.contentColor = Color.white;
			GUILayout.FlexibleSpace();
		}

		EditorGUILayout.EndHorizontal();
		GUI.color = Color.white;

		
		// Player stats
		GUI.color = settings.gameStatsExpanded ? activeClr : inactiveClr;
		EditorGUILayout.BeginHorizontal(EditorStyles.objectFieldThumb);
		EditorGUI.indentLevel = 0;
		newExpanded = EditorGUILayout.Toggle("Show World Variables", settings.gameStatsExpanded);
		if (newExpanded != settings.gameStatsExpanded) {
			UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle Show World Variables");
			settings.gameStatsExpanded = newExpanded;
		}
		
		if (settings.gameStatsExpanded) {
	        // BUTTONS...
	        EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(100));
			EditorGUI.indentLevel = 0;
	
	        // Add expand/collapse buttons if there are items in the list
	
	        EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(100));
	        // A little space between button groups
	        GUILayout.Space(6);
			
			EditorGUILayout.EndHorizontal();
	        EditorGUILayout.EndHorizontal();
	        EditorGUILayout.EndHorizontal();
			GUI.color = Color.white;

			var variables = LevelSettings.GetAllWorldVariables;
			if (variables.Count == 0) {
				DTInspectorUtility.ShowColorWarning("*You currently have no World Variables.");
			}
			
			GUI.backgroundColor = Color.cyan;
			for (var i = 0; i < variables.Count; i++) {
				var worldVar = variables[i];
				
				EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
				GUILayout.Label(worldVar.name);
				
				GUILayout.FlexibleSpace();
				
				var variable = worldVar.GetComponent<WorldVariable>();
				GUI.contentColor = Color.yellow;
				GUILayout.Label(WorldVariableTracker.GetVariableTypeFriendlyString(variable.varType));
				GUI.contentColor = Color.white;
				
				var buttonPressed = DTInspectorUtility.AddControlButtons(settings, "World Variable");
				if (buttonPressed == DTInspectorUtility.FunctionButtons.Edit) {
					Selection.activeGameObject = worldVar.gameObject;
				}
				
				EditorGUILayout.EndHorizontal();
			}
			GUI.backgroundColor = Color.white;
			
			EditorGUILayout.BeginHorizontal(EditorStyles.boldLabel);
			EditorGUILayout.LabelField("", EditorStyles.miniLabel);
			GUILayout.Space(7);
			GUI.contentColor = Color.green;
			if (GUILayout.Button("World Variable Panel", EditorStyles.toolbarButton, GUILayout.MaxWidth(130))) {
				Selection.objects = new Object[] {
					playerStatsHolder.gameObject
				};
				return;
			}
			GUI.contentColor = Color.white;
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();
		} else {
	        EditorGUILayout.EndHorizontal();
		}
		// end Player  stats
		GUI.color = Color.white;

		// level waves
		GUI.color = settings.showLevelSettings ? activeClr : inactiveClr;
		EditorGUILayout.BeginHorizontal(EditorStyles.objectFieldThumb);
		var newShowLevels = EditorGUILayout.Toggle("Show Level Waves", settings.showLevelSettings);
		if (newShowLevels != settings.showLevelSettings) {
			UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle Show Level Waves");
			settings.showLevelSettings = newShowLevels;
		}
		EditorGUILayout.EndHorizontal();
		GUI.color = Color.white;

		if (settings.showLevelSettings) {
			if (settings.useWaves) {
				EditorGUILayout.BeginHorizontal();
				EditorGUI.indentLevel = 0;  // Space will handle this for the header

				EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
				newExpanded = DTInspectorUtility.Foldout(settings.levelsAreExpanded, "Level Wave Settings");
				if (newExpanded != settings.levelsAreExpanded) {
					UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle expand Level Wave Settings");
					settings.levelsAreExpanded = newExpanded;
				}
				
		        // BUTTONS...
		        EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(100));
		
		        // Add expand/collapse buttons if there are items in the list
				if (settings.LevelTimes.Count > 0) {
					GUI.contentColor = Color.green;
					GUIContent content;
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
					GUI.contentColor = Color.white;
				} else {
		         	GUILayout.FlexibleSpace();
		        }
		
		        EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(50));
				
				var addText = string.Format("Click to add level{0}.", settings.LevelTimes.Count > 0 ? " at the end" : "");
				
		        // Main Add button
				GUI.contentColor = Color.yellow;
		        if (GUILayout.Button(new GUIContent("Add", addText), EditorStyles.toolbarButton)) {
					isDirty = true;
					CreateNewLevelAfter();
				}
				GUI.contentColor = Color.white;

				EditorGUILayout.EndHorizontal();
		
		        EditorGUILayout.EndHorizontal();
		        EditorGUILayout.EndHorizontal();
		        EditorGUILayout.EndHorizontal();

				
				DTInspectorUtility.FunctionButtons levelButtonPressed = DTInspectorUtility.FunctionButtons.None;
				DTInspectorUtility.FunctionButtons waveButtonPressed = DTInspectorUtility.FunctionButtons.None;
				
				if (settings.levelsAreExpanded) { 
					EditorGUI.indentLevel = 0;

					if (settings.LevelTimes.Count == 0) {
						DTInspectorUtility.ShowLargeBarAlert("You have no Levels set up.");
					}
					
					int levelToDelete = -1;
					int levelToInsertAt = -1;
					int waveToInsertAt = -1;
					int waveToDelete = -1;
					
					LevelSpecifics levelSetting = null;
					for (var l = 0; l < settings.LevelTimes.Count; l++) {
						EditorGUI.indentLevel = 1;
						levelSetting = settings.LevelTimes[l];

						var newOrder = (LevelSettings.WaveOrder) EditorGUILayout.EnumPopup("Wave Sequence", levelSetting.waveOrder);
						if (newOrder != levelSetting.waveOrder) {
							UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Wave Sequence");
							levelSetting.waveOrder = newOrder;
						}
						
			            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
			            // Display foldout with current state
						newExpanded = DTInspectorUtility.Foldout(levelSetting.isExpanded, 
						  string.Format("Level {0} waves", (l + 1)));
						if (newExpanded != levelSetting.isExpanded) {
							UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle expand Level Waves");
							levelSetting.isExpanded = newExpanded;
						}
			            levelButtonPressed = DTInspectorUtility.AddFoldOutListItemButtons(l, settings.LevelTimes.Count, "level", false, false);
			            EditorGUILayout.EndHorizontal();
						
						if (levelSetting.isExpanded) {
							for (var w = 0; w < levelSetting.WaveSettings.Count; w++) {
								var waveSetting = levelSetting.WaveSettings[w];
		
								EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
								EditorGUI.indentLevel = 2;
					            // Display foldout with current state
								var innerExpanded = DTInspectorUtility.Foldout(waveSetting.isExpanded, "Wave " + (w + 1));
								if (innerExpanded != waveSetting.isExpanded) {
									UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle expand Wave");
									waveSetting.isExpanded = innerExpanded;
								}
					            waveButtonPressed = DTInspectorUtility.AddFoldOutListItemButtons(w, levelSetting.WaveSettings.Count, "wave", true, false);
					            EditorGUILayout.EndHorizontal();
								
								if (waveSetting.isExpanded) {
									EditorGUI.indentLevel = 0;
									if (waveSetting.skipWaveType == LevelSettings.SkipWaveMode.Always) {
										DTInspectorUtility.ShowColorWarning("*This wave is set to be skipped.");
									}
									
									if (string.IsNullOrEmpty(waveSetting.waveName)) {
										waveSetting.waveName = "UNNAMED";
									}
		
									var newWaveName = EditorGUILayout.TextField("Wave Name", waveSetting.waveName);
									if (newWaveName != waveSetting.waveName) {
										UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Wave Name");
										waveSetting.waveName = newWaveName;
									}
		
									var newWaveType = (LevelSettings.WaveType) EditorGUILayout.EnumPopup("Wave Type", waveSetting.waveType);
									if (newWaveType != waveSetting.waveType) {
										UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Wave Type");
										waveSetting.waveType = newWaveType;
									}
									
									if (waveSetting.waveType == LevelSettings.WaveType.Timed) {
										var newEnd = EditorGUILayout.Toggle("End When All Destroyed", waveSetting.endEarlyIfAllDestroyed);
										if (newEnd != waveSetting.endEarlyIfAllDestroyed) {
											UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle End Early When All Destroyed");
											waveSetting.endEarlyIfAllDestroyed = newEnd;
										}
										
										var newDuration = EditorGUILayout.IntSlider("Duration (sec)", waveSetting.WaveDuration, 1, 2000);
										if (newDuration != waveSetting.WaveDuration) {
											UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Duration");
											waveSetting.WaveDuration = newDuration;
										}
									}
									
									switch (waveSetting.skipWaveType) {
										case LevelSettings.SkipWaveMode.IfWorldVariableValueAbove:
										case LevelSettings.SkipWaveMode.IfWorldVariableValueBelow:
											EditorGUILayout.Separator();
											break;
									}
		
									var newSkipType = (LevelSettings.SkipWaveMode) EditorGUILayout.EnumPopup("Skip Wave Type", waveSetting.skipWaveType);
									if (newSkipType != waveSetting.skipWaveType) {
										UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Skip Wave Type");
										waveSetting.skipWaveType = newSkipType;
									}
									
									switch (waveSetting.skipWaveType) {
										case LevelSettings.SkipWaveMode.IfWorldVariableValueAbove:
										case LevelSettings.SkipWaveMode.IfWorldVariableValueBelow:
											var missingStatNames = new List<string>();
											missingStatNames.AddRange(allStats);
											missingStatNames.RemoveAll(delegate(string obj) {
												return waveSetting.skipWavePassCriteria.HasKey(obj);
											});
											
											var newStat = EditorGUILayout.Popup("Add Skip Wave Limit", 0, missingStatNames.ToArray());
											if (newStat != 0) {
												AddWaveSkipLimit(missingStatNames[newStat], waveSetting);
											}
			
											if (waveSetting.skipWavePassCriteria.statMods.Count == 0) {
												DTInspectorUtility.ShowRedError("You have no Skip Wave Limits. Wave will never be skipped.");
											} else {
												EditorGUILayout.Separator();
												
												int? indexToDelete = null;
												
												for (var i = 0; i < waveSetting.skipWavePassCriteria.statMods.Count; i++) {
													var modifier = waveSetting.skipWavePassCriteria.statMods[i];
													
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
												
												DTInspectorUtility.ShowColorWarning("  *Limits are inclusive: i.e. 'Above' means >=");
												if (indexToDelete.HasValue) {
													UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "remove Skip Wave Limit");
													waveSetting.skipWavePassCriteria.DeleteByIndex(indexToDelete.Value);
												}
												
												EditorGUILayout.Separator();
											}
										
											break;
									}
									
									if (settings.useMusicSettings) {
										if (l > 0 || w > 0) {
											var newMusicMode = (LevelSettings.WaveMusicMode) EditorGUILayout.EnumPopup("Music Mode", waveSetting.musicSettings.WaveMusicMode);
											if (newMusicMode != waveSetting.musicSettings.WaveMusicMode) {
												UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Music Mode");
												waveSetting.musicSettings.WaveMusicMode = newMusicMode;
											}
										}
										
										if (waveSetting.musicSettings.WaveMusicMode == LevelSettings.WaveMusicMode.PlayNew) {
											var newWavMusic = (AudioClip) EditorGUILayout.ObjectField( "Music", waveSetting.musicSettings.WaveMusic, typeof(AudioClip), true);
											if (newWavMusic != waveSetting.musicSettings.WaveMusic) {
												UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Wave Music");
												waveSetting.musicSettings.WaveMusic = newWavMusic;
											}
										}
										if (waveSetting.musicSettings.WaveMusicMode != LevelSettings.WaveMusicMode.Silence) {
											var newVol = EditorGUILayout.Slider("Music Volume", waveSetting.musicSettings.WaveMusicVolume, 0f, 1f);
											if (newVol != waveSetting.musicSettings.WaveMusicVolume) {
												UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Music Volume");
												waveSetting.musicSettings.WaveMusicVolume = newVol;
											}
										} else {
											var newFadeTime = EditorGUILayout.Slider("Silence Fade Time", waveSetting.musicSettings.FadeTime, 0f, 15f);
											if (newFadeTime != waveSetting.musicSettings.FadeTime) {
												UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Silence Fade Time");
												waveSetting.musicSettings.FadeTime = newFadeTime;
											}
										}
									}
									
									// beat level variable modifiers
									var newBonusesEnabled = EditorGUILayout.BeginToggleGroup("Wave Completion Bonus", waveSetting.waveBeatBonusesEnabled);
									if (newBonusesEnabled != waveSetting.waveBeatBonusesEnabled) {
										UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle Wave Completion Bonus");
										waveSetting.waveBeatBonusesEnabled = newBonusesEnabled;
									}
									
									var missingBonusStatNames = new List<string>();
									missingBonusStatNames.AddRange(allStats);
									missingBonusStatNames.RemoveAll(delegate(string obj) {
										return waveSetting.waveDefeatVariableModifiers.HasKey(obj);
									});
									
									var newBonusStat = EditorGUILayout.Popup("Add Variable Modifer", 0, missingBonusStatNames.ToArray());
									if (newBonusStat != 0) {
										AddBonusStatModifier(missingBonusStatNames[newBonusStat], waveSetting);
									}
									
									if (waveSetting.waveDefeatVariableModifiers.statMods.Count == 0) {
										if (waveSetting.waveBeatBonusesEnabled) {
											DTInspectorUtility.ShowColorWarning("*You currently are using no modifiers for this wave.");
										}
									} else {
										EditorGUILayout.Separator();
										
										int? indexToDelete = null;
										
										for (var i = 0; i < waveSetting.waveDefeatVariableModifiers.statMods.Count; i++) {
											var modifier = waveSetting.waveDefeatVariableModifiers.statMods[i];
											
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
											waveSetting.waveDefeatVariableModifiers.DeleteByIndex(indexToDelete.Value);
										}
										
										EditorGUILayout.Separator();
									}
									EditorGUILayout.EndToggleGroup();
									
									if (!Application.isPlaying) {
										var spawnersUsed = FindMatchingSpawners(settings, l, w);
										
										if (spawnersUsed.Count == 0) {
											DTInspectorUtility.ShowRedError("You have no Spawners using this Wave.");																						
										} else {
											GUI.contentColor = Color.yellow;
											GUILayout.Label("Spawners using this wave: " + spawnersUsed.Count);
											GUI.contentColor = Color.white;
										}
										
										GUI.backgroundColor = Color.cyan;
										for (var s = 0; s < spawnersUsed.Count; s++) {
											var spawner = spawnersUsed[s];
											EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
											GUILayout.Label(spawner.name);
											GUILayout.FlexibleSpace();
											
											var buttonPressed = DTInspectorUtility.AddControlButtons(settings, "World Variable");
											if (buttonPressed == DTInspectorUtility.FunctionButtons.Edit) {
												Selection.activeGameObject = spawner.gameObject;
											}
											
											EditorGUILayout.EndHorizontal();
										}
										GUI.backgroundColor = Color.white;

										EditorGUILayout.Separator();
									}
								}
								
								switch (waveButtonPressed) {
									case DTInspectorUtility.FunctionButtons.Remove:
										if (levelSetting.WaveSettings.Count <= 1) {
											DTInspectorUtility.ShowAlert("You cannot delete the only Wave in a Level. Delete the Level if you like.");
										} else {
											waveToDelete = w;
										}	
		
										isDirty = true;
										break;
									case DTInspectorUtility.FunctionButtons.Add:
										waveToInsertAt = w;
										isDirty = true;
										break;
								}
							}
							
							if (waveToDelete >= 0) {
								if (DTInspectorUtility.ConfirmDialog("Delete wave? This cannot be undone.")) {
									DeleteWave(levelSetting, waveToDelete, l);
									isDirty = true;
								}
							}
							if (waveToInsertAt > -1) {
								InsertWaveAfter(levelSetting, waveToInsertAt, l);
								isDirty = true;
							}
						} 
						
						switch (levelButtonPressed) {
							case DTInspectorUtility.FunctionButtons.Remove:
								if (DTInspectorUtility.ConfirmDialog("Delete level? This cannot be undone.")) {
									levelToDelete = l;
									isDirty = true;
								}
								break;
							case DTInspectorUtility.FunctionButtons.Add:
								isDirty = true;
								levelToInsertAt = l;
								break;
						}
					}
					
					if (levelToDelete > -1) {
						DeleteLevel(levelToDelete);
					}
					
					if (levelToInsertAt > -1) {
						CreateNewLevelAfter(levelToInsertAt); 
					}
				}
			} else {
				EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
				EditorGUILayout.LabelField(" Level Wave Settings (DISABLED)");
				EditorGUILayout.EndHorizontal();			
			}
		}

		// level waves
		GUI.color = settings.showCustomEvents ? activeClr : inactiveClr;
		EditorGUILayout.BeginHorizontal(EditorStyles.objectFieldThumb);
		var newShowEvents = EditorGUILayout.Toggle("Show Custom Events", settings.showCustomEvents);
		if (newShowEvents != settings.showCustomEvents) {
			UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle Show Custom Events");
			settings.showCustomEvents = newShowEvents;
		}
		EditorGUILayout.EndHorizontal();
		GUI.color = Color.white;
		
		if (settings.showCustomEvents) {
			var newEvent = EditorGUILayout.TextField("New Event Name", settings.newEventName);
			if (newEvent != settings.newEventName) {
				UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change New Event Name");
				settings.newEventName = newEvent;
			}
			
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(10);
			GUI.contentColor = Color.green;
			if (GUILayout.Button("Create New Event", EditorStyles.toolbarButton, GUILayout.Width(100))) {
				CreateCustomEvent(settings.newEventName);
			}
			GUI.contentColor = Color.white;
			EditorGUILayout.EndHorizontal();
			
			if (settings.customEvents.Count == 0) {
				DTInspectorUtility.ShowLargeBarAlert("You currently have no custom events.");
			}
			
			EditorGUILayout.Separator();
			
			int? customEventToDelete = null;
			int? eventToRename = null;
			
			for (var i = 0; i < settings.customEvents.Count; i++) {
				EditorGUI.indentLevel = 0;
				var anEvent = settings.customEvents[i];

				EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
				var exp = DTInspectorUtility.Foldout(anEvent.eventExpanded, anEvent.EventName);
				if (exp != anEvent.eventExpanded) {
					UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle expand Custom Event");
					anEvent.eventExpanded = exp;
				}

				GUILayout.FlexibleSpace();
				if (Application.isPlaying) {
					var receivers = LevelSettings.ReceiversForEvent(anEvent.EventName);
					
					GUI.contentColor = Color.green;
					if (receivers.Count > 0) {
						if (GUILayout.Button("Select", EditorStyles.toolbarButton, GUILayout.Width(50))) {
							var matches = new List<GameObject>(receivers.Count);

							for (var s = 0; s < receivers.Count; s++) {
								matches.Add(receivers[s].gameObject);
							}
							Selection.objects = matches.ToArray();
						}
					}

					if (GUILayout.Button("Fire!", EditorStyles.toolbarButton, GUILayout.Width(50))) {
						LevelSettings.FireCustomEvent(anEvent.EventName, settings.transform.position); 
					}

					GUI.contentColor = Color.yellow;
					GUILayout.Label(string.Format("Receivers: {0}", receivers.Count));
					GUI.contentColor = Color.white;
				} else {
					var newName = GUILayout.TextField(anEvent.ProspectiveName, GUILayout.Width(170));
					if (newName != anEvent.ProspectiveName) {
						UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Proposed Event Name");
						anEvent.ProspectiveName = newName;
					}
					
					var buttonPressed = DTInspectorUtility.AddCustomEventDeleteIcon(true);
					
					switch (buttonPressed) {
						case DTInspectorUtility.FunctionButtons.Remove:
							customEventToDelete = i;
							break;
						case DTInspectorUtility.FunctionButtons.Rename:
							eventToRename = i;
							break;
					}
				}
				
				EditorGUILayout.EndHorizontal();


				if (anEvent.eventExpanded) {
					EditorGUI.indentLevel = 1;
					var rcvMode = (LevelSettings.EventReceiveMode) EditorGUILayout.EnumPopup("Send To Receivers", anEvent.eventRcvMode);	
					if (rcvMode != anEvent.eventRcvMode) {
						UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Send To Receivers");
						anEvent.eventRcvMode = rcvMode;
					}

					if (rcvMode == LevelSettings.EventReceiveMode.WhenDistanceLessThan || rcvMode == LevelSettings.EventReceiveMode.WhenDistanceMoreThan) {
						KillerVariablesHelper.DisplayKillerFloat(ref isDirty, anEvent.distanceThreshold, "Distance Threshold", settings, false, true);
					}

					EditorGUILayout.Separator();
				}
			}

			if (customEventToDelete.HasValue) {
				settings.customEvents.RemoveAt(customEventToDelete.Value);
			}
			if (eventToRename.HasValue) {
				RenameEvent(settings.customEvents[eventToRename.Value]);
			}
		}

		if (GUI.changed || isDirty) {
			EditorUtility.SetDirty(target);	// or it won't save the data!!
		}

		//DrawDefaultInspector();
    }
	
	private void ExpandCollapseAll(bool isExpand) {
		UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle expand / collapse all Level Wave Settings");

		foreach (var level in settings.LevelTimes) {
			level.isExpanded = isExpand;
			foreach (var wave in level.WaveSettings) {
				wave.isExpanded = isExpand;
			}
		}
	}
	
	private void CreateSpawner() {
		string name = settings.newSpawnerName;
		
		if (string.IsNullOrEmpty(name)) {
			DTInspectorUtility.ShowAlert("You must enter a name for your new Spawner.");
			return;
		}
		
		Transform spawnerTrans = null;
		
		switch (settings.newSpawnerType) {
			case LevelSettings.SpawnerType.Green:
				spawnerTrans = settings.GreenSpawnerTrans;
				break;
			case LevelSettings.SpawnerType.Red:
				spawnerTrans = settings.RedSpawnerTrans;
				break;
		}
		
		var spawnPos = settings.transform.position;
		spawnPos.x += Random.Range(-10, 10);
		spawnPos.z += Random.Range(-10, 10);

		var newSpawner = GameObject.Instantiate(spawnerTrans.gameObject, spawnPos, Quaternion.identity) as GameObject;
		UndoHelper.CreateObjectForUndo(newSpawner.gameObject, "create Spawner");
		newSpawner.name = name;
		
		var spawnersHolder = settings.transform.FindChild(LevelSettings.SPAWNER_CONTAINER_TRANS_NAME);
		if (spawnersHolder == null) {
			DTInspectorUtility.ShowAlert(LevelSettings.NO_SPAWN_CONTAINER_ALERT);
			
			GameObject.DestroyImmediate(newSpawner);
			
			return;
		}
		
		newSpawner.transform.parent = spawnersHolder.transform;
	}
	
	private void CreatePrefabPool() {
		string name = settings.newPrefabPoolName;
		
		if (string.IsNullOrEmpty(name)) {
			DTInspectorUtility.ShowAlert("You must enter a name for your new Prefab Pool.");
			return;
		}
		
		var spawnPos = settings.transform.position;
		
		var newPool = GameObject.Instantiate(settings.PrefabPoolTrans.gameObject, spawnPos, Quaternion.identity) as GameObject;
		newPool.name = name;
		
		var poolsHolder = settings.transform.FindChild(LevelSettings.PREFAB_POOLS_CONTAINER_TRANS_NAME);
		if (poolsHolder == null) {
			DTInspectorUtility.ShowAlert(LevelSettings.NO_PREFAB_POOLS_CONTAINER_ALERT);
			
			GameObject.DestroyImmediate(newPool);
			return;
		}
		
		var dupe = poolsHolder.FindChild(name);
		if (dupe != null) {
			DTInspectorUtility.ShowAlert("You already have a Prefab Pool named '" + name + "', please choose another name.");
			
			GameObject.DestroyImmediate(newPool);
			return;
		}
		
		UndoHelper.CreateObjectForUndo(newPool.gameObject, "create Prefab Pool");
		newPool.transform.parent = poolsHolder.transform;
	}
	
	private void InsertWaveAfter(LevelSpecifics spec, int waveToInsertAt, int level) {
		var spawners = LevelSettings.GetAllSpawners;
			
		var newWave = new LevelWave();

		waveToInsertAt++;
		spec.WaveSettings.Insert(waveToInsertAt, newWave);

		WaveSyncroPrefabSpawner spawnerScript = null;
	
		foreach (var spawner in spawners) {
			spawnerScript = spawner.GetComponent<WaveSyncroPrefabSpawner>();
			spawnerScript.InsertWave(waveToInsertAt, level);
		}		
	}
	
	private void DeleteLevel(int levelToDelete) {
		List<Transform> spawners = LevelSettings.GetAllSpawners;

		settings.LevelTimes.RemoveAt(levelToDelete);
		
		WaveSyncroPrefabSpawner spawnerScript = null;
	
		foreach (var spawner in spawners) {
			spawnerScript = spawner.GetComponent<WaveSyncroPrefabSpawner>();
			spawnerScript.DeleteLevel(levelToDelete);
		}		
	}
	
	private void CreateNewLevelAfter(int? index = null) {
		List<Transform> spawners = LevelSettings.GetAllSpawners;
		
		var newLevel = new LevelSpecifics();
		var newWave = new LevelWave();
		newLevel.WaveSettings.Add(newWave);
		
		int newLevelIndex = 0;
		
		if (index == null) {
			newLevelIndex = settings.LevelTimes.Count;
		} else {
			newLevelIndex = index.Value + 1;
		}

		UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "Add Level");

		settings.LevelTimes.Insert(newLevelIndex, newLevel);
		
		WaveSyncroPrefabSpawner spawnerScript = null;
	
		foreach (var spawner in spawners) {
			spawnerScript = spawner.GetComponent<WaveSyncroPrefabSpawner>();
			spawnerScript.InsertLevel(newLevelIndex);
		}		
	}

	private void DeleteWave(LevelSpecifics spec, int waveToDelete, int levelNumber) {
		var spawners = LevelSettings.GetAllSpawners;
		var affectedObjects = new List<Object>();
		affectedObjects.Add(settings);

		var spawnerScripts = new List<WaveSyncroPrefabSpawner>();
		foreach (var s in spawners) {
			spawnerScripts.Add(s.GetComponent<WaveSyncroPrefabSpawner>());
			affectedObjects.Add(s);
		}

		spec.WaveSettings.RemoveAt(waveToDelete);

		foreach (var script in spawnerScripts) {
			script.DeleteWave(levelNumber, waveToDelete);
		}
	}

	private void AddWaveSkipLimit(string modifierName, LevelWave spec) {
		if (spec.skipWavePassCriteria.HasKey(modifierName)) {
			DTInspectorUtility.ShowAlert("This wave already has a Skip Wave Limit for World Variable: " + modifierName + ". Please modify the existing one instead.");
			return;
		}
	
		var myVar = WorldVariableTracker.GetWorldVariableScript(modifierName);
		
		UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "add Skip Wave Limit");
		
		spec.skipWavePassCriteria.statMods.Add(new WorldVariableModifier(modifierName, myVar.varType));
	}
	
	private List<WaveSyncroPrefabSpawner> FindMatchingSpawners(LevelSettings levSettings, int level, int wave) {
		var spawners = LevelSettings.GetAllSpawners;
		WaveSyncroPrefabSpawner spawnerScript = null;
		
		var matchingSpawners = new List<WaveSyncroPrefabSpawner>();
		
		foreach (var spawner in spawners) {
			spawnerScript = spawner.GetComponent<WaveSyncroPrefabSpawner>();
			var matchingWave = spawnerScript.FindWave(level, wave);
			if (matchingWave == null) {
				continue;
			}
			
			matchingSpawners.Add(spawnerScript);
		}
		
		return matchingSpawners;
	}

	private string[] LevelNames {
		get {
			var names = new string[settings.LevelTimes.Count];
			for (var i = 0; i < settings.LevelTimes.Count; i++) {
				names[i] = (i + 1).ToString();
			}
			
			return names;
		}
	}

	private int[] LevelIndexes {
		get {
			var indexes = new int[settings.LevelTimes.Count];
			
			for (var i = 0; i < settings.LevelTimes.Count; i++) {
				indexes[i] = i + 1;
			}
			
			return indexes;
		}
	}
	
	private string[] WaveNamesForLevel(int levelNumber) {
		if (settings.LevelTimes.Count <= levelNumber || settings.LevelTimes.Count < 1 || levelNumber < 0) {
			return new string[0];
		}
		
		var level = settings.LevelTimes[levelNumber];
		var names = new string[level.WaveSettings.Count];
		
		for (var i = 0; i < level.WaveSettings.Count; i++) {
			names[i] = (i + 1).ToString();
		}
		
		return names;
	}

	private int[] WaveIndexesForLevel(int levelNumber) {
		if (settings.LevelTimes.Count <= levelNumber || settings.LevelTimes.Count < 1 || levelNumber < 0) {
			return new int[0];
		}

		var level = settings.LevelTimes[levelNumber];
		var indexes = new int[level.WaveSettings.Count];
		
		for (var i = 0; i < level.WaveSettings.Count; i++) {
			indexes[i] = i + 1;
		}
		
		return indexes;
	}
	
	private void AddBonusStatModifier(string modifierName, LevelWave waveSpec) {
		if (waveSpec.waveDefeatVariableModifiers.HasKey(modifierName)) {
			DTInspectorUtility.ShowAlert("This Wave already has a modifier for World Variable: " + modifierName + ". Please modify that instead.");
			return;
		}

		UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "add Wave Completion Bonus modifier");
		
		WorldVariable vType = WorldVariableTracker.GetWorldVariableScript(modifierName);
		
		waveSpec.waveDefeatVariableModifiers.statMods.Add(new WorldVariableModifier(modifierName, vType.varType));
	}
	
	private void CreateLevelSettingsPrefab() {
        var go = GameObject.Instantiate(settings.gameObject) as GameObject;
		go.name = "LevelWaveSettings";
		go.transform.position = Vector3.zero;
	}

	private void CreateCustomEvent(string newEventName) {
		if (settings.customEvents.FindAll(delegate(CgkCustomEvent obj) {
			return obj.EventName == newEventName;
		}).Count > 0) {
			DTInspectorUtility.ShowAlert("You already have a custom event named '" + newEventName + "'. Please choose a different name.");
			return;
		}
		
		settings.customEvents.Add(new CgkCustomEvent(newEventName));
	}

	private void RenameEvent(CgkCustomEvent cEvent) {
		var match = settings.customEvents.FindAll(delegate(CgkCustomEvent obj) {
			return obj.EventName == cEvent.ProspectiveName;
		});
		
		if (match.Count > 0) {
			DTInspectorUtility.ShowAlert("You already have a custom event named '" + cEvent.ProspectiveName + "'. Please choose a different name.");
			return;
		}

		UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "rename Custom Event");
		cEvent.EventName = cEvent.ProspectiveName;
	}
}
