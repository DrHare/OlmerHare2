using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class TriggeredWaveMetaData {
    public WavePrefabPool wavePool = null;
    public List<Transform> spawnedWaveMembers = new List<Transform>();
    public int currentWaveSize;
    public float waveStartTime;
    public bool waveFinishedSpawning = false;
    public int countSpawned = 0;
    public float singleSpawnTime;
    public float lastSpawnTime;
    public TriggeredWaveSpecifics waveSpec = null;
    public int waveRepetitionNumber = 0;
    public float previousWaveEndTime = 0;
}
