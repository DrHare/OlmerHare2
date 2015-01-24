using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Text;

[CustomEditor(typeof(PoolBoss))]
public class PoolBossInspector : Editor {
	private PoolBoss pool;
	private bool isDirty = false;

	public override void OnInspectorGUI() {
        EditorGUIUtility.LookLikeControls();
		EditorGUI.indentLevel = 0; 
		
		pool = (PoolBoss)target;
		
		isDirty = false;
		LevelSettings.Instance = null; // clear cached version

        DTInspectorUtility.DrawTexture(CoreGameKitInspectorResources.logoTexture);

		var isInProjectView = DTInspectorUtility.IsPrefabInProjectView(pool);
		
		if (isInProjectView) {
			DTInspectorUtility.ShowLargeBarAlert("*You have selected the PoolBoss prefab in Project View.");
			DTInspectorUtility.ShowLargeBarAlert("*Please select the one in your Scene to edit.");
			return;
		}
		
		var newAutoAdd = EditorGUILayout.Toggle("Auto-Add Missing Items", pool.autoAddMissingPoolItems);
		if (newAutoAdd != pool.autoAddMissingPoolItems) {
			UndoHelper.RecordObjectPropertyForUndo(ref isDirty, pool, "toggle Auto-Add Missing Items");
			pool.autoAddMissingPoolItems = newAutoAdd;
		}
		
		var newLog = EditorGUILayout.Toggle("Log Messages", pool.logMessages);
		if (newLog != pool.logMessages) {
			UndoHelper.RecordObjectPropertyForUndo(ref isDirty, pool, "toggle Log Messages");
			pool.logMessages = newLog;
		}

		EditorGUILayout.BeginHorizontal();
		GUILayout.Label("Actions", GUILayout.Width(100));
		GUI.contentColor = Color.green;
		GUILayout.Space(47);
		if (GUILayout.Button("Sort Items Alpha", EditorStyles.toolbarButton, GUILayout.Width(110))) {
			pool.poolItems.Sort(delegate(PoolBossItem x, PoolBossItem y) {
				if (x.prefabTransform == null || y.prefabTransform == null) {
					return 0;
				}
				
				return x.prefabTransform.name.CompareTo(y.prefabTransform.name);
			});
		}
		
		if (Application.isPlaying) {
			GUILayout.Space(10);
			if (GUILayout.Button(new GUIContent("Kill All", "Click to kill all prefab (Killables only can be killed)"), EditorStyles.toolbarButton, GUILayout.Width(90))) {
				SpawnUtility.KillAllPrefabs();
				//SpawnUtility.KillAllOfPrefab(poolItem.prefabTransform);
				isDirty = true;
			}
	
			GUILayout.Space(10);
			if (GUILayout.Button(new GUIContent("Despawn All", "Click to despawn prefabs"), EditorStyles.toolbarButton, GUILayout.Width(90))) {
				SpawnUtility.DespawnAllPrefabs();
				isDirty = true;
			}
		}
		GUI.contentColor = Color.white;
		
		GUI.contentColor = Color.white;
		EditorGUILayout.EndVertical();
		EditorGUILayout.Separator();
		
		EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
		var newExpanded = DTInspectorUtility.Foldout(pool.poolItemsExpanded, string.Format("Pool Item Settings ({0})", pool.poolItems.Count));
		if (newExpanded != pool.poolItemsExpanded) {
			UndoHelper.RecordObjectPropertyForUndo(ref isDirty, pool, "toggle expand Pool Settings");
			pool.poolItemsExpanded = newExpanded;
		}
		
		
        EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(100));

        // Add expand/collapse buttons if there are items in the list
		if (pool.poolItems.Count > 0) {
			GUI.contentColor = Color.green;
			GUIContent content;
			var collapseIcon = "Collapse";
            content = new GUIContent(collapseIcon, "Click to collapse all");
            var masterCollapse = GUILayout.Button(content, EditorStyles.toolbarButton, GUILayout.Width(60));

			var expandIcon = "Expand";
            content = new GUIContent(expandIcon, "Click to expand all");
			var masterExpand = GUILayout.Button(content, EditorStyles.toolbarButton, GUILayout.Width(60));
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
		
		var addText = string.Format("Click to add Pool Item{0}.", pool.poolItems.Count > 0 ? " at the end" : "");
		
        // Main Add button
		GUI.contentColor = Color.yellow;
		if (GUILayout.Button(new GUIContent("Add", addText), EditorStyles.toolbarButton)) {
			isDirty = true;
			CreateNewPoolItem();
		}
		GUI.contentColor = Color.white;

		EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndHorizontal();
		
		
		EditorGUILayout.EndHorizontal();
		
		if (pool.poolItemsExpanded) {
			int? indexToRemove = null;
			int? indexToInsertAt = null;
			int? indexToShiftUp = null;
			int? indexToShiftDown = null;
			
			for (var i = 0; i < pool.poolItems.Count; i++) {
				var poolItem = pool.poolItems[i];
				
				EditorGUI.indentLevel = 1;
				EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
				var itemName = poolItem.prefabTransform == null ? "[NO PREFAB]" : poolItem.prefabTransform.name;
				newExpanded = DTInspectorUtility.Foldout(poolItem.isExpanded, itemName);
				if (newExpanded != poolItem.isExpanded) {
					UndoHelper.RecordObjectPropertyForUndo(ref isDirty, pool, "toggle expand Pool Item");
					poolItem.isExpanded = newExpanded;
				}
				
				if (Application.isPlaying) {
					GUILayout.FlexibleSpace();
					
					GUI.contentColor = Color.green;
					if (GUILayout.Button(new GUIContent("Kill All", "Click to kill all of this prefab (Killables only can be killed)"), EditorStyles.toolbarButton, GUILayout.Width(80))) {
						SpawnUtility.KillAllOfPrefab(poolItem.prefabTransform);
						isDirty = true;
					}
					if (GUILayout.Button(new GUIContent("Despawn All", "Click to despawn all of this prefab"), EditorStyles.toolbarButton, GUILayout.Width(80))) {
						SpawnUtility.DespawnAllOfPrefab(poolItem.prefabTransform);
						isDirty = true;
					}
					GUI.contentColor = Color.white;
					
					GUI.contentColor = Color.yellow;
					if (poolItem.prefabTransform != null) {
						var itemInfo = PoolBoss.PoolItemInfoByName(itemName);
						if (itemInfo != null) {
							var spawnedCount = itemInfo._spawnedClones.Count;
							var despawnedCount = itemInfo._despawnedClones.Count;
							var content = new GUIContent(string.Format("{0} / {1} Spawned", spawnedCount, despawnedCount + spawnedCount), "Click here to select all spawned items.");
							if (GUILayout.Button(content, EditorStyles.toolbarButton, GUILayout.Width(110))) {
								var obj = new List<GameObject>();
								for (var o = 0; o < itemInfo._spawnedClones.Count; o++) {
									obj.Add(itemInfo._spawnedClones[o].gameObject);
								}

								Selection.objects = obj.ToArray();
							}
						}
					}
					GUI.contentColor = Color.white;
				}
				
				var buttonPressed = DTInspectorUtility.AddFoldOutListItemButtons(i, pool.poolItems.Count, "Pool Item", true, true);
				EditorGUILayout.EndHorizontal();

				if (poolItem.isExpanded) {
					EditorGUI.indentLevel = 0;
					
					var newPrefab = (Transform) EditorGUILayout.ObjectField("Prefab", poolItem.prefabTransform, typeof(Transform), false);
					if (newPrefab != poolItem.prefabTransform) {
						UndoHelper.RecordObjectPropertyForUndo(ref isDirty, pool, "change Pool Item Prefab");
						poolItem.prefabTransform = newPrefab;
					}
					
					var newPreloadQty = EditorGUILayout.IntSlider("Preload Qty", poolItem.instancesToPreload, 0, 10000);
					if (newPreloadQty != poolItem.instancesToPreload) {
						UndoHelper.RecordObjectPropertyForUndo(ref isDirty, pool, "change Pool Item Preload Qty");
						poolItem.instancesToPreload = newPreloadQty;
					}
					if (poolItem.instancesToPreload == 0) {
						DTInspectorUtility.ShowColorWarning("*You have set the Preload Qty to 0. This prefab will not be in the Pool.");
					}
					
					var newAllow = EditorGUILayout.Toggle("Allow Instantiate More", poolItem.allowInstantiateMore);
					if (newAllow != poolItem.allowInstantiateMore) {
						UndoHelper.RecordObjectPropertyForUndo(ref isDirty, pool, "toggle Allow Instantiate More");
						poolItem.allowInstantiateMore = newAllow;
					}
					
					if (poolItem.allowInstantiateMore) {
						var newLimit = EditorGUILayout.IntSlider("Item Limit", poolItem.itemHardLimit, poolItem.instancesToPreload, 1000);
						if (newLimit != poolItem.itemHardLimit) {
							UndoHelper.RecordObjectPropertyForUndo(ref isDirty, pool, "change Item Limit");
							poolItem.itemHardLimit = newLimit;
						}
					} else {
						var newRecycle = EditorGUILayout.Toggle("Recycle Oldest", poolItem.allowRecycle);
						if (newRecycle != poolItem.allowRecycle) {
							UndoHelper.RecordObjectPropertyForUndo(ref isDirty, pool, "toggle Recycle Oldest");
							poolItem.allowRecycle = newRecycle;
						}
					}
					
					newLog = EditorGUILayout.Toggle("Log Messages", poolItem.logMessages);
					if (newLog != poolItem.logMessages) {
						UndoHelper.RecordObjectPropertyForUndo(ref isDirty, pool, "toggle Log Messages");
						poolItem.logMessages = newLog;
					}
				}
				
				switch (buttonPressed) {
					case DTInspectorUtility.FunctionButtons.Remove:
						indexToRemove = i;
						break;
					case DTInspectorUtility.FunctionButtons.Add:
						indexToInsertAt = i;
						break;
					case DTInspectorUtility.FunctionButtons.ShiftUp:
						indexToShiftUp = i;
						break;
					case DTInspectorUtility.FunctionButtons.ShiftDown:
						indexToShiftDown = i;
						break;
					case DTInspectorUtility.FunctionButtons.DespawnAll:
						PoolBoss.DespawnAllOfPrefab(poolItem.prefabTransform);
						break;
				}
			}
			
			if (indexToRemove.HasValue) {
				UndoHelper.RecordObjectPropertyForUndo(ref isDirty, pool, "remove Pool Item");
				pool.poolItems.RemoveAt(indexToRemove.Value);
			}
			if (indexToInsertAt.HasValue) {
				UndoHelper.RecordObjectPropertyForUndo(ref isDirty, pool, "insert Pool Item");
				pool.poolItems.Insert(indexToInsertAt.Value, new PoolBossItem());
			}
			if (indexToShiftUp.HasValue) {
				UndoHelper.RecordObjectPropertyForUndo(ref isDirty, pool, "shift up Pool Item");
				var item = pool.poolItems[indexToShiftUp.Value];
				pool.poolItems.Insert(indexToShiftUp.Value - 1, item);
				pool.poolItems.RemoveAt(indexToShiftUp.Value + 1);
			}
			
			if (indexToShiftDown.HasValue) {
				UndoHelper.RecordObjectPropertyForUndo(ref isDirty, pool, "shift down Pool Item");
				var index = indexToShiftDown.Value + 1;
				var item = pool.poolItems[index];
				pool.poolItems.Insert(index - 1, item);
				pool.poolItems.RemoveAt(index + 1);
			}
		}
			
		if (GUI.changed || isDirty) {
			EditorUtility.SetDirty(target);	// or it won't save the data!!
		}

        Repaint();
		//DrawDefaultInspector();
    }

	private void ExpandCollapseAll(bool isExpand) {
		UndoHelper.RecordObjectPropertyForUndo(ref isDirty, pool, "toggle expand / collapse all Pool Boss Items");

		if (isExpand) {
			pool.poolItemsExpanded = true;
		}

		foreach (var item in pool.poolItems) {
			item.isExpanded = isExpand;
		}
	}
	
	private void CreateNewPoolItem() {
		pool.poolItems.Add(new PoolBossItem());
	}
}
