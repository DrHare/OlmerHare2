using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class WorldVariableRangeCollection {
    public List<WorldVariableRange> statMods = new List<WorldVariableRange>();

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
