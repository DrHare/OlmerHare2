using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class TriggeredWaveSpecifics {
    public bool isExpanded = true;
    public bool enableWave = false;
    public KillerInt NumberToSpwn = new KillerInt(1, 0, 1024);
	public KillerInt MaxToSpawn = new KillerInt(1, 0, 1024);
	public KillerFloat WaveDelaySec = new KillerFloat(0f, 0f, float.MaxValue);
    public KillerFloat TimeToSpawnEntireWave = new KillerFloat(0f, 0f, float.MaxValue);
    public Transform prefabToSpawn;
    public WaveSpecifics.SpawnOrigin spawnSource = WaveSpecifics.SpawnOrigin.Specific;
    public int prefabPoolIndex = 0;
    public string prefabPoolName = null;

    public bool enableRepeatWave = false;
    public WaveSpecifics.RepeatWaveMode curWaveRepeatMode = WaveSpecifics.RepeatWaveMode.NumberOfRepetitions;
    public KillerFloat repeatWavePauseSec = new KillerFloat(-1f, .1f, float.MaxValue);
    public KillerInt maxRepeat = new KillerInt(2, 2, int.MaxValue);
    public KillerInt repeatItemInc = new KillerInt(0, -100, 100);
    public KillerInt repeatItemLmt = new KillerInt(100, 1, int.MaxValue);
    public KillerFloat repeatTimeInc = new KillerFloat(0f, float.MinValue, float.MaxValue);
    public KillerFloat repeatTimeLmt = new KillerFloat(100f, .1f, float.MaxValue);
    public bool useWaveSpawnBonusForRepeats = false;

    public WorldVariableCollection repeatPassCriteria = new WorldVariableCollection();
    public bool willDespawnOnEvent = false;

    public Vector3 waveOffset = Vector3.zero;

    public bool waveSpawnBonusesEnabled = false;
    public WorldVariableCollection waveSpawnVariableModifiers = new WorldVariableCollection();

    public bool useLayerFilter = false;
    public bool useTagFilter = false;
    public List<string> matchingTags = new List<string>() { "Untagged" };
    public List<int> matchingLayers = new List<int>() { 0 };
	
	public bool positionExpanded = true;
    public WaveSpecifics.PositionMode positionXmode = WaveSpecifics.PositionMode.UseSpawnerPosition;
    public WaveSpecifics.PositionMode positionYmode = WaveSpecifics.PositionMode.UseSpawnerPosition;
    public WaveSpecifics.PositionMode positionZmode = WaveSpecifics.PositionMode.UseSpawnerPosition;
	public KillerInt customPosX = new KillerInt(0, int.MinValue, int.MaxValue);
	public KillerInt customPosY = new KillerInt(0, int.MinValue, int.MaxValue);
	public KillerInt customPosZ = new KillerInt(0, int.MinValue, int.MaxValue);
	
	public WaveSpecifics.RotationMode curRotationMode = WaveSpecifics.RotationMode.UsePrefabRotation;
    public Vector3 customRotation = Vector3.zero;
    public Vector3 keepCenterRotation = Vector3.zero;
	
	public WaveSpecifics.SpawnerRotationMode curSpawnerRotMode = WaveSpecifics.SpawnerRotationMode.KeepRotation;
	
	public bool eventOriginIgnoreX = false; 
	public bool eventOriginIgnoreY = false; 
	public bool eventOriginIgnoreZ = false; 
	
	// for custom events only
	public bool customEventActive = false; 
	public bool isCustomEvent = false;
	public string customEventName = string.Empty;
	public Vector3 customEventLookRotation = Vector3.zero;
	
    public bool enableRandomizations;
    public bool randomXRotation;
    public bool randomYRotation;
    public bool randomZRotation;
    public KillerFloat randomDistX = new KillerFloat(0f, 0f, TriggeredSpawner.MAX_DISTANCE);
    public KillerFloat randomDistY = new KillerFloat(0f, 0f, TriggeredSpawner.MAX_DISTANCE);
    public KillerFloat randomDistZ = new KillerFloat(0f, 0f, TriggeredSpawner.MAX_DISTANCE);
    public KillerFloat randomXRotMin = new KillerFloat(0f, 0f, 360f);
    public KillerFloat randomXRotMax = new KillerFloat(360f, 0f, 360f);
    public KillerFloat randomYRotMin = new KillerFloat(0f, 0f, 360f);
    public KillerFloat randomYRotMax = new KillerFloat(360f, 0f, 360f);
    public KillerFloat randomZRotMin = new KillerFloat(0f, 0f, 360f);
    public KillerFloat randomZRotMax = new KillerFloat(360f, 0f, 360f);

    public bool enableIncrements;
    public bool enableKeepCenter;
    public KillerFloat incrementPositionX = new KillerFloat(0f, float.MinValue, float.MaxValue);
    public KillerFloat incrementPositionY = new KillerFloat(0f, float.MinValue, float.MaxValue);
    public KillerFloat incrementPositionZ = new KillerFloat(0f, float.MinValue, float.MaxValue);
    public KillerFloat incrementRotX = new KillerFloat(0f, -180f, 180f);
    public KillerFloat incrementRotY = new KillerFloat(0f, -180f, 180f);
    public KillerFloat incrementRotZ = new KillerFloat(0f, -180f, 180f);

    public bool enablePostSpawnNudge = false;
    public KillerFloat postSpawnNudgeFwd = new KillerFloat(0f, float.MinValue, float.MaxValue);
    public KillerFloat postSpawnNudgeRgt = new KillerFloat(0f, float.MinValue, float.MaxValue);
    public KillerFloat postSpawnNudgeDwn = new KillerFloat(0f, float.MinValue, float.MaxValue);

    // optional wave end triggers
    public bool stopWaveOnOppositeEvent = false;

    // retrigger limit settings
    public bool disableAfterFirstTrigger = false;
    public TriggeredSpawner.RetriggerLimitMode retriggerLimitMode = TriggeredSpawner.RetriggerLimitMode.None;
    public KillerInt limitPerXFrm = new KillerInt(1, 1, int.MaxValue);
    public KillerFloat limitPerXSec = new KillerFloat(0.1f, .1f, float.MaxValue);

    public int trigLastFrame = -10000;
    public float trigLastTime = -10000f;
	
    public enum SpawnSource {
        Specific,
        PrefabPool
    }

    public bool IsValid {
        get {
            if (!this.enableWave) {
                return false;
            }

            return true;
        }
    }
}
