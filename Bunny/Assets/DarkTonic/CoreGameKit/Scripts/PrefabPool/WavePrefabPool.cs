using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This class is used for Prefab Pool setup, to give randomness and weight to the groups of prefabs in a single spawner wave (or Killable spawn).
/// </summary>
public class WavePrefabPool : MonoBehaviour {
    public bool isExpanded = true;
    public bool exhaustiveList = true;
    public PoolDispersalMode dispersalMode = PoolDispersalMode.Randomized;
    public WavePrefabPoolListener listener;

    private bool isValid = false;
    public List<WavePrefabPoolItem> poolItems;
    private List<int> poolItemIndexes = new List<int>();

    public enum PoolDispersalMode {
        Randomized,
        OriginalPoolOrder
    }

    void Awake() {
        this.useGUILayout = false;
        FillPool();
    }

    void FillPool() {
        // fill weighted pool
        for (var item = 0; item < poolItems.Count; item++) {
            var poolItem = poolItems[item];

            bool includeItem = true;

            switch (poolItem.activeMode) {
                case LevelSettings.ActiveItemMode.Always:
                    break;
                case LevelSettings.ActiveItemMode.Never:
                    continue;
                case LevelSettings.ActiveItemMode.IfWorldVariableInRange:
                    if (poolItem.activeItemCriteria.statMods.Count == 0) {
                        includeItem = false;
                    }

                    for (var i = 0; i < poolItem.activeItemCriteria.statMods.Count; i++) {
                        var stat = poolItem.activeItemCriteria.statMods[i];
                        if (!WorldVariableTracker.VariableExistsInScene(stat._statName)) {
                            Debug.LogError(string.Format("Prefab Pool '{0}' could not find World Variable '{1}', which is used in its Active Item Criteria.",
                                this.transform.name,
                                stat._statName));
                            includeItem = false;
                            break;
                        }

                        var variable = WorldVariableTracker.GetWorldVariable(stat._statName);
                        if (variable == null) {
                            includeItem = false;
                            break;
                        }
                        var varVal = stat._varTypeToUse == WorldVariableTracker.VariableType._integer ? variable.CurrentIntValue : variable.CurrentFloatValue;

                        var min = stat._varTypeToUse == WorldVariableTracker.VariableType._integer ? stat._modValueIntMin : stat._modValueFloatMin;
                        var max = stat._varTypeToUse == WorldVariableTracker.VariableType._integer ? stat._modValueIntMax : stat._modValueFloatMax;

                        if (min > max) {
                            LevelSettings.LogIfNew("The Min cannot be greater than the Max for Active Item Limit in Prefab Pool '" + this.transform.name + "'. Skipping item '" + poolItem.prefabToSpawn.name + "'.");
                            includeItem = false;
                            break;
                        }

                        if (varVal < min || varVal > max) {
                            includeItem = false;
                            break;
                        }
                    }

                    break;
                case LevelSettings.ActiveItemMode.IfWorldVariableOutsideRange:
                    if (poolItem.activeItemCriteria.statMods.Count == 0) {
                        includeItem = false;
                    }

                    for (var i = 0; i < poolItem.activeItemCriteria.statMods.Count; i++) {
                        var stat = poolItem.activeItemCriteria.statMods[i];
                        var variable = WorldVariableTracker.GetWorldVariable(stat._statName);
                        if (variable == null) {
                            includeItem = false;
                            break;
                        }
                        var varVal = stat._varTypeToUse == WorldVariableTracker.VariableType._integer ? variable.CurrentIntValue : variable.CurrentFloatValue;

                        var min = stat._varTypeToUse == WorldVariableTracker.VariableType._integer ? stat._modValueIntMin : stat._modValueFloatMin;
                        var max = stat._varTypeToUse == WorldVariableTracker.VariableType._integer ? stat._modValueIntMax : stat._modValueFloatMax;

                        if (min > max) {
                            LevelSettings.LogIfNew("The Min cannot be greater than the Max for Active Item Limit in Prefab Pool '" + this.transform.name + "'. Skipping item '" + poolItem.prefabToSpawn.name + "'.");
                            includeItem = false;
                            break;
                        }

                        if (varVal >= min && varVal <= max) {
                            includeItem = false;
                            break;
                        }
                    }

                    break;
            }

            if (!includeItem) {
                continue;
            }

            for (int i = 0; i < poolItem.thisWeight.Value; i++) {
                poolItemIndexes.Add(item);
            }
        }

        if (poolItemIndexes.Count == 0) {
            LevelSettings.LogIfNew("The Prefab Pool '" + this.name + "' has no active Prefab Pool items. Please add some or delete the Prefab pool before continuing. Disabling Core GameKit.");
            LevelSettings.IsGameOver = true;
            return;
        }

        isValid = true;
    }

    /// <summary>
    /// This returns a random (unless you've set the Pool to non-random) Transform to you for your own use to spawn. All spawners and Killable use this.
    /// </summary>
    /// <returns></returns>
    public Transform GetRandomWeightedTransform() {
        if (!isValid) {
            return null;
        }

        var index = 0; // for non-random
        if (dispersalMode == PoolDispersalMode.Randomized) {
            index = UnityEngine.Random.Range(0, poolItemIndexes.Count);
        }

        var prefabIndex = poolItemIndexes[index];

        if (exhaustiveList || dispersalMode == PoolDispersalMode.OriginalPoolOrder) {
            poolItemIndexes.RemoveAt(index);

            if (poolItemIndexes.Count == 0) {
                // refill
                if (LevelSettings.IsLoggingOn) {
                    Debug.Log(string.Format("Prefab Pool '{0}' refilling exhaustion list.",
                        this.name));
                }

                if (this.listener != null) {
                    this.listener.PoolRefilling();
                }

                FillPool();
            }
        }

        var spawnable = poolItems[prefabIndex].prefabToSpawn;

        if (LevelSettings.IsLoggingOn) {
            Debug.Log(string.Format("Prefab Pool '{0}' spawning random item '{1}'.",
                this.name,
                spawnable.name));
        }

        if (this.listener != null) {
            this.listener.PrefabGrabbedFromPool(spawnable);
        }

        return spawnable;
    }

    public int PoolInstancesOfIndex(int index) {
        return poolItemIndexes.FindAll(delegate(int obj) {
            return obj.Equals(index);
        }).Count;
    }
}