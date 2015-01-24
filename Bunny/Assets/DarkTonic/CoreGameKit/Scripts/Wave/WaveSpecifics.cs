using UnityEngine;
using System;
using System.Collections;

[Serializable]
public class WaveSpecifics {
    public bool isExpanded = true;
    public bool enableWave = true;
    public int SpawnLevelNumber = 0;
    public int SpawnWaveNumber = 0;
    public KillerInt MinToSpwn = new KillerInt(1, 0, int.MaxValue);
    public KillerInt MaxToSpwn = new KillerInt(2, 0, int.MaxValue);
    public KillerFloat WaveDelaySec = new KillerFloat(0f, 0f, float.MaxValue);
    public KillerFloat TimeToSpawnEntireWave = new KillerFloat(3f, 0f, float.MaxValue);
    public Transform prefabToSpawn;
    public SpawnOrigin spawnSource = SpawnOrigin.Specific;
    public int prefabPoolIndex = 0;
    public string prefabPoolName = "";
    public bool repeatWaveUntilNew = false;
    public int waveCompletePercentage = 100;

    public RepeatWaveMode curWaveRepeatMode = RepeatWaveMode.NumberOfRepetitions;
    public TimedRepeatWaveMode curTimedRepeatWaveMode = TimedRepeatWaveMode.EliminationStyle;
    public KillerFloat repeatPauseMinimum = new KillerFloat(0f, 0f, float.MaxValue);
    public KillerFloat repeatPauseMaximum = new KillerFloat(0f, 0f, float.MaxValue);
    public KillerInt repeatItemInc = new KillerInt(0, -100, 100);
    public KillerFloat repeatTimeInc = new KillerFloat(0f, float.MinValue, float.MaxValue);
    public KillerInt repeatItemLmt = new KillerInt(100, 0, 1000);
    public KillerFloat repeatTimeLmt = new KillerFloat(100f, .1f, float.MaxValue);

    public KillerInt repetitionsToDo = new KillerInt(2, 2, int.MaxValue);
    public WorldVariableCollection repeatPassCriteria = new WorldVariableCollection();

    public bool waveRepeatBonusesEnabled = false;
    public WorldVariableCollection waveRepeatVariableModifiers = new WorldVariableCollection();

    public bool positionExpanded = true;
    public WaveSpecifics.PositionMode positionXmode = WaveSpecifics.PositionMode.UseSpawnerPosition;
    public WaveSpecifics.PositionMode positionYmode = WaveSpecifics.PositionMode.UseSpawnerPosition;
    public WaveSpecifics.PositionMode positionZmode = WaveSpecifics.PositionMode.UseSpawnerPosition;
    public KillerInt customPosX = new KillerInt(0, int.MinValue, int.MaxValue);
    public KillerInt customPosY = new KillerInt(0, int.MinValue, int.MaxValue);
    public KillerInt customPosZ = new KillerInt(0, int.MinValue, int.MaxValue);

    public RotationMode curRotationMode = RotationMode.UsePrefabRotation;
    public Vector3 customRotation = Vector3.zero;
	
    public bool enableLimits = false;
    public KillerFloat doNotSpawnIfMbrCloserThan = new KillerFloat(5f, 0.1f, float.MaxValue);
    public KillerFloat doNotSpawnRandomDist = new KillerFloat(0f, .1f, float.MaxValue);

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

    public Vector3 waveOffset = Vector3.zero;

    public bool enablePostSpawnNudge = false;
    public KillerFloat postSpawnNudgeFwd = new KillerFloat(0f, float.MinValue, float.MaxValue);
    public KillerFloat postSpawnNudgeRgt = new KillerFloat(0f, float.MinValue, float.MaxValue);
    public KillerFloat postSpawnNudgeDwn = new KillerFloat(0f, float.MinValue, float.MaxValue);

    public enum RepeatWaveMode {
        Endless,
        NumberOfRepetitions,
        UntilWorldVariableAbove,
        UntilWorldVariableBelow
    }

    public enum TimedRepeatWaveMode {
        EliminationStyle,
        StrictTimeStyle
    }

    public enum SpawnOrigin {
        Specific,
        PrefabPool
    }
	
	public enum PositionMode {
		UseSpawnerPosition,
		CustomPosition
	}
	
	public enum SpawnerRotationMode {
		KeepRotation,
		LookAtCustomEventOrigin
	}
	
    public enum RotationMode {
        UsePrefabRotation,
        UseSpawnerRotation,
        CustomRotation,
		LookAtCustomEventOrigin
    }

    public bool IsValid {
        get {
            if (!this.enableWave) {
                return false;
            }

            if (this.repeatPauseMinimum.Value > this.repeatPauseMaximum.Value) {
                return false;
            }

            if (this.MinToSpwn.Value > this.MaxToSpwn.Value) {
                return false;
            }

            return true;
        }
    }
}
