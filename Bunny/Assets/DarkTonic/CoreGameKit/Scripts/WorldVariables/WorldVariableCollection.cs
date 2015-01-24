using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class WorldVariableCollection {
    public string scenarioName = "CONDITION_NAME";
    public List<WorldVariableModifier> statMods = new List<WorldVariableModifier>();

    public void DeleteByIndex(int index) {
        statMods.RemoveAt(index);
    }

    public bool HasKey(string key) {
        for (var i = 0; i < statMods.Count; i++) {
            if (statMods[i]._statName == key) {
                return true;
            }
        }

        return false;
    }
}
