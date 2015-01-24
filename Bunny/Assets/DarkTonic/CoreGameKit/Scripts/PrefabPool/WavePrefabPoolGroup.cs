using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class WavePrefabPoolGroup : MonoBehaviour {
    void Awake() {
        this.useGUILayout = false;
    }

    void Start() {
        var poolNames = new List<string>();

        WavePrefabPool poolScript = null;

        for (var i = 0; i < this.transform.childCount; i++) {
            var pool = this.transform.GetChild(i);
            if (poolNames.Contains(pool.name)) {
                LevelSettings.LogIfNew("You have more than one Prefab Pool with the name '" + pool.name + "'. Please fix this before continuing.");
                LevelSettings.IsGameOver = true;
                return;
            }

            poolScript = pool.GetComponent<WavePrefabPool>();
            if (poolScript == null) {
                LevelSettings.LogIfNew("The Prefab Pool '" + pool.name + "' has no Prefab Pool script. Please delete it and fix this before continuing.");
                LevelSettings.IsGameOver = true;
                return;
            }

            poolNames.Add(pool.name);
        }
    }
}
