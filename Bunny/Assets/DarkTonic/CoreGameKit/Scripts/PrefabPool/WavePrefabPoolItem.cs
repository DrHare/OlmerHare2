using UnityEngine;
using System;
using System.Collections;

[Serializable]
public class WavePrefabPoolItem {
    public Transform prefabToSpawn;
    public LevelSettings.ActiveItemMode activeMode = LevelSettings.ActiveItemMode.Always;
    public WorldVariableRangeCollection activeItemCriteria = new WorldVariableRangeCollection();
    public KillerInt thisWeight = new KillerInt(1, 0, 256);
    public bool isExpanded = true;
}
