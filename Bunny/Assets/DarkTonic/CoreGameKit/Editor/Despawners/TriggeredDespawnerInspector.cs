using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
	[CustomEditor(typeof(TriggeredDespawner))]
#else
	[CustomEditor(typeof(TriggeredDespawner), true)]
#endif
public class TriggeredDespawnerInspector : Editor {
	private TriggeredDespawner settings;
	private bool isDirty = false;

	public override void OnInspectorGUI() {
        EditorGUIUtility.LookLikeControls();
		
		settings = (TriggeredDespawner)target;
		LevelSettings.Instance = null; // clear cached version

        DTInspectorUtility.DrawTexture(CoreGameKitInspectorResources.logoTexture);

		isDirty = false;

		EditorGUI.indentLevel = 0;
		
		List<bool> changedList = new List<bool>();
		changedList.Add(RenderEventTypeControls(settings.invisibleSpec, "Invisible Event", TriggeredSpawner.EventType.Invisible));
		changedList.Add(RenderEventTypeControls(settings.mouseOverSpec, "Mouse Over (Legacy) Event", TriggeredSpawner.EventType.MouseOver_Legacy));
		changedList.Add(RenderEventTypeControls(settings.mouseClickSpec, "Mouse Click (Legacy) Event", TriggeredSpawner.EventType.MouseClick_Legacy));
		changedList.Add(RenderEventTypeControls(settings.collisionSpec, "Collision Enter Event", TriggeredSpawner.EventType.OnCollision));
		changedList.Add(RenderEventTypeControls(settings.triggerEnterSpec, "Trigger Enter Event", TriggeredSpawner.EventType.OnTriggerEnter));
		changedList.Add(RenderEventTypeControls(settings.triggerExitSpec, "Trigger Exit Event", TriggeredSpawner.EventType.OnTriggerExit));

		#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2
		// not supported
		#else
			changedList.Add(RenderEventTypeControls(settings.collision2dSpec, "2D Collision Enter Event", TriggeredSpawner.EventType.OnCollision2D));
			changedList.Add(RenderEventTypeControls(settings.triggerEnter2dSpec, "2D Trigger Enter Event", TriggeredSpawner.EventType.OnTriggerEnter2D));
			changedList.Add(RenderEventTypeControls(settings.triggerExit2dSpec, "2D Trigger Exit Event", TriggeredSpawner.EventType.OnTriggerExit2D));
		#endif

		changedList.Add(RenderEventTypeControls(settings.onClickSpec, "NGUI OnClick Event", TriggeredSpawner.EventType.OnClick_NGUI));
		
		var hadNoListener = settings.listener == null;
		var newListener = (TriggeredDespawnerListener) EditorGUILayout.ObjectField("Listener", settings.listener, typeof(TriggeredDespawnerListener), true);
		if (newListener != settings.listener) {
			UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "assign Listener");
			settings.listener = newListener;

			if (hadNoListener && settings.listener != null) {
				settings.listener.sourceDespawnerName = settings.transform.name;
			}
		}
		
		if (GUI.changed || isDirty || changedList.Contains(true)) {
  			EditorUtility.SetDirty(target);	// or it won't save the data!!
		}

		//DrawDefaultInspector();
    }
	
	private bool RenderEventTypeControls(EventDespawnSpecifics despawnSettings, string toggleText, TriggeredSpawner.EventType eventType) {
		EditorGUI.indentLevel = 0;
        EditorGUILayout.BeginHorizontal(EditorStyles.objectFieldThumb);
		
		var newEnabled = EditorGUILayout.Toggle(toggleText, despawnSettings.eventEnabled);
		if (newEnabled != despawnSettings.eventEnabled) {
			UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle " + toggleText + " enabled");
			despawnSettings.eventEnabled = newEnabled;
		}
        EditorGUILayout.EndHorizontal();
		 
		if (despawnSettings.eventEnabled) {
			if (TriggeredSpawner.eventsWithTagLayerFilters.Contains(eventType)) {
				var newUseLayerFilter = EditorGUILayout.BeginToggleGroup("Layer filters", despawnSettings.useLayerFilter);
				if (newUseLayerFilter != despawnSettings.useLayerFilter) {
					UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "toggle Layer filters");
					despawnSettings.useLayerFilter = newUseLayerFilter;
				}
				if (despawnSettings.useLayerFilter) {
					for (var i = 0; i < despawnSettings.matchingLayers.Count; i++) {
						var newMatch = EditorGUILayout.LayerField("Layer Match " + (i + 1), despawnSettings.matchingLayers[i]);
						if (newMatch != despawnSettings.matchingLayers[i]) {
							UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Layer Match");
							despawnSettings.matchingLayers[i] = newMatch;
						}
					}
					EditorGUILayout.BeginHorizontal();
					GUILayout.Space(12);
					GUI.contentColor = Color.green;
					if (GUILayout.Button(new GUIContent("Add", "Click to add a Layer Match at the end"), EditorStyles.toolbarButton, GUILayout.Width(60))) {
						UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "add Layer Match");
						
						despawnSettings.matchingLayers.Add(0);
					}
					GUILayout.Space(10);
					if (despawnSettings.matchingLayers.Count > 1) {
						if (GUILayout.Button(new GUIContent("Remove", "Click to remove the last Layer Match"), EditorStyles.toolbarButton, GUILayout.Width(60))) {
							UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "remove Layer Match");

							despawnSettings.matchingLayers.RemoveAt(despawnSettings.matchingLayers.Count - 1);
						}
					}
					GUI.contentColor = Color.white;
					EditorGUILayout.EndHorizontal();
				}
				EditorGUILayout.EndToggleGroup();
				
				despawnSettings.useTagFilter = EditorGUILayout.BeginToggleGroup("Tag filter", despawnSettings.useTagFilter);
				if (despawnSettings.useTagFilter) {
					for (var i = 0; i < despawnSettings.matchingTags.Count; i++) {
						var newMatch = EditorGUILayout.TagField("Tag Match " + (i + 1), despawnSettings.matchingTags[i]);
						if (newMatch != despawnSettings.matchingTags[i]) {
							UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "change Tag Match");
							despawnSettings.matchingTags[i] = newMatch;
						}
					}
					EditorGUILayout.BeginHorizontal();
					GUILayout.Space(12);
					GUI.contentColor = Color.green;
					if (GUILayout.Button(new GUIContent("Add", "Click to add a Tag Match at the end"), EditorStyles.toolbarButton, GUILayout.Width(60))) {
						UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "add Tag Match");
						despawnSettings.matchingTags.Add("Untagged");
					}
					GUILayout.Space(10);
					if (despawnSettings.matchingTags.Count > 1) {
						if (GUILayout.Button(new GUIContent("Remove", "Click to remove the last Tag Match"), EditorStyles.toolbarButton, GUILayout.Width(60))) {
							UndoHelper.RecordObjectPropertyForUndo(ref isDirty, settings, "remove Tag Match");
							despawnSettings.matchingTags.RemoveAt(despawnSettings.matchingLayers.Count - 1);
						}
					}
					GUI.contentColor = Color.white;
					EditorGUILayout.EndHorizontal();
				}
				EditorGUILayout.EndToggleGroup();
			} else {
				EditorGUI.indentLevel = 0;
				DTInspectorUtility.ShowColorWarning("No additional properties for this event type.");
			}
		}
		
		return isDirty;
	}

}
