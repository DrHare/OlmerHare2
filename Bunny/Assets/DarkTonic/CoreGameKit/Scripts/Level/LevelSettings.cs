using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This class is used to set up global settings and configure levels and waves for Syncro Spawners.
/// </summary>
public class LevelSettings : MonoBehaviour {
    #region Variables, constants and enums
    public const string DYNAMIC_EVENT_NAME = "[Type In]";
    public const string NO_EVENT_NAME = "[None]";
    public const string KILLER_POOLING_CONTAINER_TRANS_NAME = "PoolBoss";
    public const string PREFAB_POOLS_CONTAINER_TRANS_NAME = "PrefabPools";
    public const string SPAWNER_CONTAINER_TRANS_NAME = "Spawners";
    public const string WORLD_VARIABLES_CONTAINER_TRANS_NAME = "WorldVariables";
    public const string DROP_DOWN_NONE_OPTION = "-None-";
    public const string REVERT_LEVEL_SETTINGS_ALERT = "Please revert your LevelSettings prefab.";
    public const string NO_SPAWN_CONTAINER_ALERT = "You have no '" + SPAWNER_CONTAINER_TRANS_NAME + "' prefab under LevelSettings. " + REVERT_LEVEL_SETTINGS_ALERT;
    public const string NO_PREFAB_POOLS_CONTAINER_ALERT = "You have no '" + PREFAB_POOLS_CONTAINER_TRANS_NAME + "' prefab under LevelSettings. " + REVERT_LEVEL_SETTINGS_ALERT;
    public const string NO_WORLD_VARIABLES_CONTAINER_ALERT = "You have no '" + WORLD_VARIABLES_CONTAINER_TRANS_NAME + "' prefab under LevelSettings. " + REVERT_LEVEL_SETTINGS_ALERT;

    private const float WAVE_CHECK_INTERVAL = .1f; // reduce this to check for spawner activations more often. This is set to ~3x a second.

    public bool useMusicSettings = true;
    public bool showLevelSettings = true;
    public bool showCustomEvents = true;
    public bool gameStatsExpanded = false;
    public string newEventName = "my event";
    public LevelSettingsListener listener;
    public Transform RedSpawnerTrans;
    public Transform GreenSpawnerTrans;
    public Transform PrefabPoolTrans;
    public string newSpawnerName = "spawnerName";
    public string newPrefabPoolName = "EnemiesPool";
    public SpawnerType newSpawnerType = SpawnerType.Green;
    public LevelWaveMusicSettings gameOverMusicSettings = new LevelWaveMusicSettings();
    public bool levelsAreExpanded = true;
    public bool createSpawnersExpanded = true;
    public bool createPrefabPoolsExpanded = true;
    public bool killerPoolingExpanded = true;
    public bool disableSyncroSpawners = false;
    public bool startFirstWaveImmediately = true;
    public WaveRestartBehavior waveRestartMode = WaveRestartBehavior.LeaveSpawned;
    public bool enableWaveWarp = false;
    public KillerInt startLevelNumber = new KillerInt(1, 1, int.MaxValue);
    public KillerInt startWaveNumber = new KillerInt(1, 1, int.MaxValue);
    public bool persistBetweenScenes = false;
    public bool isLoggingOn = false;
    public List<LevelSpecifics> LevelTimes = new List<LevelSpecifics>();
    public bool useWaves = true;

    public static readonly List<string> illegalVariableNames = new List<string>() {
		LevelSettings.DROP_DOWN_NONE_OPTION,
		string.Empty
	};


    public static readonly YieldInstruction endOfFrameDelay = new WaitForEndOfFrame();

    private static LevelSettings _lsInstance;
    private static Dictionary<int, List<LevelWave>> waveSettingsByLevel = new Dictionary<int, List<LevelWave>>();
    private static int currentLevel;
    private static int currentLevelWave;
    private static bool gameIsOver;
    private static bool hasPlayerWon;
    private static bool wavesArePaused = false;
    private static LevelWave previousWave;
    private static Dictionary<int, WaveSyncroPrefabSpawner> eliminationSpawnersUnkilled = new Dictionary<int, WaveSyncroPrefabSpawner>();
    private static bool skipCurrentWave = false;
    private static List<Transform> spawnedItemsRemaining = new List<Transform>();
    private static int waveTimeRemaining;
    private static Dictionary<string, float> recentErrorsByTime = new Dictionary<string, float>();
    private static List<RespawnTimer> prefabsToRespawn = new List<RespawnTimer>();
    private static Dictionary<string, Dictionary<ICgkEventReceiver, Transform>> receiversByEventName = new Dictionary<string, Dictionary<ICgkEventReceiver, Transform>>();
    private static Transform trans;

    private List<WaveSyncroPrefabSpawner> syncroSpawners = new List<WaveSyncroPrefabSpawner>();
    private bool isValid;
    private float lastWaveChangeTime;
    private bool hasFirstWaveBeenStarted;
    private static bool appIsShuttingDown = false;
    public List<CgkCustomEvent> customEvents = new List<CgkCustomEvent>();

    private YieldInstruction loopDelay = new WaitForSeconds(WAVE_CHECK_INTERVAL);

    public enum EventReceiveMode {
        Always,
        WhenDistanceLessThan,
        WhenDistanceMoreThan,
        Never
    }

    public enum WaveOrder {
        SpecifiedOrder,
        RandomOrder
    }

    public enum WaveRestartBehavior {
        LeaveSpawned,
        DestroySpawned,
        DespawnSpawned
    }

    public enum VariableSource {
        Variable,
        Self
    }

    public enum WaveMusicMode {
        KeepPreviousMusic,
        PlayNew,
        Silence
    }

    public enum ActiveItemMode {
        Always,
        Never,
        IfWorldVariableInRange,
        IfWorldVariableOutsideRange
    }

    public enum SkipWaveMode {
        None,
        Always,
        IfWorldVariableValueAbove,
        IfWorldVariableValueBelow,
    }

    public enum WaveType {
        Timed,
        Elimination
    }

    public enum SpawnerType {
        Green,
        Red
    }

    public enum RotationType {
        Identity,
        CustomEuler,
        SpawnerRotation
    }

    public enum SpawnPositionMode {
        UseVector3,
        UseThisObjectPosition,
        UseOtherObjectPosition
    }

    #endregion

    #region classes and structs
    public struct RespawnTimer {
        public float _timeToRespawn;
        public Transform _prefabToRespawn;
        public Vector3 _position;

        public RespawnTimer(float timeToWait, Transform prefab, Vector3 position) {
            _timeToRespawn = Time.realtimeSinceStartup + timeToWait;
            _prefabToRespawn = prefab;
            _position = position;
        }
    }
    #endregion

    #region MonoBehavior Events

    void Awake() {
        this.useGUILayout = false;
        trans = this.transform;

        hasFirstWaveBeenStarted = false;
        isValid = true;
        wavesArePaused = false;
        int iLevel = 0;
        currentLevel = 0;
        currentLevelWave = 0;
        previousWave = null;
        skipCurrentWave = false;

        if (persistBetweenScenes) {
            DontDestroyOnLoad(this.gameObject);
        }

        if (useWaves) {
            if (LevelTimes.Count == 0) {
                LogIfNew("NO LEVEL / WAVE TIMES DEFINED. ABORTING.");
                isValid = false;
                return;
            } else if (LevelTimes[0].WaveSettings.Count == 0) {
                LogIfNew("NO LEVEL 1 / WAVE 1 TIME DEFINED! ABORTING.");
                isValid = false;
                return;
            }
        }

        var levelSettingScripts = GameObject.FindObjectsOfType(typeof(LevelSettings));
        if (levelSettingScripts.Length > 1) {
            LogIfNew("You have more than one LevelWaveSettings prefab in your scene. Please delete all but one. Aborting.");
            isValid = false;
            return;
        }

        waveSettingsByLevel = new Dictionary<int, List<LevelWave>>();

        var waveLs = new List<LevelWave>();

        for (var i = 0; i < LevelTimes.Count; i++) {
            var level = LevelTimes[i];

            if (level.WaveSettings.Count == 0) {
                LogIfNew("NO WAVES DEFINED FOR LEVEL: " + (iLevel + 1));
                isValid = false;
                continue;
            }

            waveLs = new List<LevelWave>();
            LevelWave newLevelWave = null;

            var w = 0;

            foreach (var waveSetting in level.WaveSettings) {
                if (waveSetting.WaveDuration <= 0) {
                    LogIfNew("WAVE DURATION CANNOT BE ZERO OR LESS - OCCURRED IN LEVEL " + (i + 1) + ".");
                    isValid = false;
                    return;
                }

                newLevelWave = new LevelWave() {
                    waveType = waveSetting.waveType,
                    WaveDuration = waveSetting.WaveDuration,
                    musicSettings = new LevelWaveMusicSettings() {
                        WaveMusicMode = waveSetting.musicSettings.WaveMusicMode,
                        WaveMusicVolume = waveSetting.musicSettings.WaveMusicVolume,
                        WaveMusic = waveSetting.musicSettings.WaveMusic,
                        FadeTime = waveSetting.musicSettings.FadeTime
                    },
                    waveName = waveSetting.waveName,
                    waveDefeatVariableModifiers = waveSetting.waveDefeatVariableModifiers,
                    waveBeatBonusesEnabled = waveSetting.waveBeatBonusesEnabled,
                    skipWaveType = waveSetting.skipWaveType,
                    skipWavePassCriteria = waveSetting.skipWavePassCriteria,
                    sequencedWaveNumber = w,
                    endEarlyIfAllDestroyed = waveSetting.endEarlyIfAllDestroyed
                };

                if (waveSetting.waveType == WaveType.Elimination) {
                    newLevelWave.WaveDuration = 500; // super long to recognize this problem if it occurs.
                }

                waveLs.Add(newLevelWave);
                w++;
            }

            var sequencedWaves = new List<LevelWave>();

            switch (level.waveOrder) {
                case WaveOrder.SpecifiedOrder:
                    sequencedWaves.AddRange(waveLs);
                    break;
                case WaveOrder.RandomOrder:
                    while (waveLs.Count > 0) {
                        var randIndex = UnityEngine.Random.Range(0, waveLs.Count);
                        sequencedWaves.Add(waveLs[randIndex]);
                        waveLs.RemoveAt(randIndex);
                    }
                    break;
            }

            if (i == LevelTimes.Count - 1) { // extra bogus wave so that the real last wave will get run
                newLevelWave = new LevelWave() {
                    musicSettings = new LevelWaveMusicSettings() {
                        WaveMusicMode = WaveMusicMode.KeepPreviousMusic,
                        WaveMusic = null
                    },
                    WaveDuration = 1,
                    sequencedWaveNumber = w
                };

                sequencedWaves.Add(newLevelWave);
            }

            waveSettingsByLevel.Add(iLevel, sequencedWaves);

            iLevel++;
        }

        WaveSyncroPrefabSpawner spawner = null;

        foreach (var gObj in GetAllSpawners) {
            spawner = gObj.GetComponent<WaveSyncroPrefabSpawner>();

            syncroSpawners.Add(spawner);
        }

        waveTimeRemaining = 0;
        spawnedItemsRemaining.Clear();

        gameIsOver = false;
        hasPlayerWon = false;
    }

    void OnApplicationQuit() {
        AppIsShuttingDown = true; // very important!! Dont' take this out, false debug info will show up.
        WorldVariableTracker.FlushAll();
    }

    void OnLevelWasLoaded(int level) {
        WorldVariableTracker.FlushAll();
    }

    void OnDisable() {
        WorldVariableTracker.FlushAll();
    }

    void Start() {
        if (!CheckForValidVariables()) {
            isValid = false;
        }

        if (!startFirstWaveImmediately) {
            wavesArePaused = true;
        }

        if (isValid) {
            StartCoroutine(this.CoUpdate());
        }
    }

    #endregion

    #region Helper Methods

    private bool CheckForValidVariables() {
        if (!useWaves) {
            return true; // don't bother checking
        }

        // check for valid custom start level
        if (enableWaveWarp) {
            var startLevelNum = startLevelNumber.Value;
            var startWaveNum = startWaveNumber.Value;

            if (startLevelNum > waveSettingsByLevel.Count) {
                LogIfNew(string.Format("Illegal Start Level# specified in Level Settings. There are only {0} level(s). Aborting.",
                    waveSettingsByLevel.Count));
                return false;
            }

            var waveCount = waveSettingsByLevel[startLevelNum - 1].Count - 1; // -1 for the fake final wave
            if (startWaveNum > waveCount) {
                LogIfNew(string.Format("Illegal Start Wave# specified in Level Settings. Level {0} only has {1} wave(s). Aborting.",
                    startLevelNum,
                    waveCount));
                return false;
            }
        }

        for (var i = 0; i < waveSettingsByLevel.Count; i++) {
            var wavesForLevel = waveSettingsByLevel[i];
            for (var w = 0; w < wavesForLevel.Count; w++) {
                // check "skip wave states".
                var wave = wavesForLevel[w];
                if (wave.skipWaveType == SkipWaveMode.IfWorldVariableValueAbove || wave.skipWaveType == SkipWaveMode.IfWorldVariableValueBelow) {
                    for (var skip = 0; skip < wave.skipWavePassCriteria.statMods.Count; skip++) {
                        var skipCrit = wave.skipWavePassCriteria.statMods[skip];

                        if (WorldVariableTracker.IsBlankVariableName(skipCrit._statName)) {
                            LogIfNew(string.Format("Level {0} Wave {1} specifies a Skip Wave criteria with no World Variable selected. Please select one.",
                                (i + 1),
                                (w + 1)));
                            isValid = false;
                        } else if (!WorldVariableTracker.VariableExistsInScene(skipCrit._statName)) {
                            LogIfNew(string.Format("Level {0} Wave {1} specifies a Skip Wave criteria of World Variable '{2}', which doesn't exist in the scene.",
                                (i + 1),
                                (w + 1),
                                skipCrit._statName));
                            isValid = false;
                        } else {
                            switch (skipCrit._varTypeToUse) {
                                case WorldVariableTracker.VariableType._integer:
                                    if (skipCrit._modValueIntAmt.variableSource == VariableSource.Variable) {
                                        if (!WorldVariableTracker.VariableExistsInScene(skipCrit._modValueIntAmt.worldVariableName)) {
                                            if (LevelSettings.illegalVariableNames.Contains(skipCrit._modValueIntAmt.worldVariableName)) {
                                                LevelSettings.LogIfNew(string.Format("Level {0} Wave {1} wants to skip wave if World Variable '{2}' is above the value of an unspecified World Variable. Please select one.",
                                                    (i + 1),
                                                    (w + 1),
                                                    skipCrit._statName));
                                            } else {
                                                LevelSettings.LogIfNew(string.Format("Level {0} Wave {1} wants to skip wave if World Variable '{2}' is above the value of World Variable '{3}', but the latter is not in the Scene.",
                                                    (i + 1),
                                                    (w + 1),
                                                    skipCrit._statName,
                                                    skipCrit._modValueIntAmt.worldVariableName));
                                            }
                                            isValid = false;
                                        }
                                    }

                                    break;
                                case WorldVariableTracker.VariableType._float:
                                    if (skipCrit._modValueFloatAmt.variableSource == VariableSource.Variable) {
                                        if (!WorldVariableTracker.VariableExistsInScene(skipCrit._modValueFloatAmt.worldVariableName)) {
                                            if (LevelSettings.illegalVariableNames.Contains(skipCrit._modValueFloatAmt.worldVariableName)) {
                                                LevelSettings.LogIfNew(string.Format("Level {0} Wave {1} wants to skip wave if World Variable '{2}' is above the value of an unspecified World Variable. Please select one.",
                                                    (i + 1),
                                                    (w + 1),
                                                    skipCrit._statName));
                                            } else {
                                                LevelSettings.LogIfNew(string.Format("Level {0} Wave {1} wants to skip wave if World Variable '{2}' is above the value of World Variable '{3}', but the latter is not in the Scene.",
                                                    (i + 1),
                                                    (w + 1),
                                                    skipCrit._statName,
                                                    skipCrit._modValueFloatAmt.worldVariableName));
                                            }
                                            isValid = false;
                                        }
                                    }

                                    break;
                                default:
                                    LogIfNew("Add code for varType: " + skipCrit._varTypeToUse.ToString());
                                    break;
                            }
                        }
                    }
                }

                // check "wave completion bonuses".
                if (!wave.waveBeatBonusesEnabled) {
                    continue;
                }

                for (var b = 0; b < wave.waveDefeatVariableModifiers.statMods.Count; b++) {
                    var beatMod = wave.waveDefeatVariableModifiers.statMods[b];

                    if (WorldVariableTracker.IsBlankVariableName(beatMod._statName)) {
                        LogIfNew(string.Format("Level {0} Wave {1} specifies a Wave Completion Bonus with no World Variable selected. Please select one.",
                            (i + 1),
                            (w + 1)));
                        isValid = false;
                    } else if (!WorldVariableTracker.VariableExistsInScene(beatMod._statName)) {
                        LogIfNew(string.Format("Level {0} Wave {1} specifies a Wave Completion Bonus of World Variable '{2}', which doesn't exist in the scene.",
                            (i + 1),
                            (w + 1),
                            beatMod._statName));
                        isValid = false;
                    } else {
                        switch (beatMod._varTypeToUse) {
                            case WorldVariableTracker.VariableType._integer:
                                if (beatMod._modValueIntAmt.variableSource == VariableSource.Variable) {
                                    if (!WorldVariableTracker.VariableExistsInScene(beatMod._modValueIntAmt.worldVariableName)) {
                                        if (LevelSettings.illegalVariableNames.Contains(beatMod._modValueIntAmt.worldVariableName)) {
                                            LevelSettings.LogIfNew(string.Format("Level {0} Wave {1} wants to award Wave Completion Bonus if World Variable '{2}' is above the value of an unspecified World Variable. Please select one.",
                                                (i + 1),
                                                (w + 1),
                                                beatMod._statName));
                                        } else {
                                            LevelSettings.LogIfNew(string.Format("Level {0} Wave {1} wants to award Wave Completion Bonus if World Variable '{2}' is above the value of World Variable '{3}', but the latter is not in the Scene.",
                                                (i + 1),
                                                (w + 1),
                                                beatMod._statName,
                                                beatMod._modValueIntAmt.worldVariableName));
                                        }
                                        isValid = false;
                                    }
                                }

                                break;
                            case WorldVariableTracker.VariableType._float:
                                if (beatMod._modValueFloatAmt.variableSource == VariableSource.Variable) {
                                    if (!WorldVariableTracker.VariableExistsInScene(beatMod._modValueFloatAmt.worldVariableName)) {
                                        if (LevelSettings.illegalVariableNames.Contains(beatMod._modValueFloatAmt.worldVariableName)) {
                                            LevelSettings.LogIfNew(string.Format("Level {0} Wave {1} wants to award Wave Completion Bonus if World Variable '{2}' is above the value of an unspecified World Variable. Please select one.",
                                                (i + 1),
                                                (w + 1),
                                                beatMod._statName));
                                        } else {
                                            LevelSettings.LogIfNew(string.Format("Level {0} Wave {1} wants to award Wave Completion Bonus if World Variable '{2}' is above the value of World Variable '{3}', but the latter is not in the Scene.",
                                                (i + 1),
                                                (w + 1),
                                                beatMod._statName,
                                                beatMod._modValueFloatAmt.worldVariableName));
                                        }
                                        isValid = false;
                                    }
                                }

                                break;
                            default:
                                LogIfNew("Add code for varType: " + beatMod._varTypeToUse.ToString());
                                break;
                        }
                    }
                }
            }
        }

        return true;
    }

    private IEnumerator CoUpdate() {
        while (true) {
            yield return loopDelay;

            // respawn timers
            if (prefabsToRespawn.Count > 0) {
                var respawnedIndexes = new List<int>();

                for (var i = 0; i < prefabsToRespawn.Count; i++) {
                    var p = prefabsToRespawn[i];
                    if (Time.realtimeSinceStartup < p._timeToRespawn) {
                        continue;
                    }

                    var spawned = PoolBoss.SpawnInPool(p._prefabToRespawn, p._position, p._prefabToRespawn.rotation);
                    if (spawned == null) {
                        continue;
                    }

                    respawnedIndexes.Add(i);
                }

                for (var i = 0; i < respawnedIndexes.Count; i++) {
                    prefabsToRespawn.RemoveAt(respawnedIndexes[i]);
                }
            }

            if (gameIsOver || wavesArePaused || !useWaves) {
                continue;
            }

            WaveType waveType;

            //check if level or wave is done.
            if (hasFirstWaveBeenStarted && !skipCurrentWave) {
                int timeToCompare = 0;

                timeToCompare = ActiveWaveInfo.WaveDuration;
                waveType = ActiveWaveInfo.waveType;

                switch (waveType) {
                    case WaveType.Timed:
                        var tempTime = (int)(timeToCompare - (Time.time - this.lastWaveChangeTime));
                        if (tempTime != TimeRemainingInCurrentWave) {
                            TimeRemainingInCurrentWave = tempTime;
                        }

                        var allDead = ActiveWaveInfo.endEarlyIfAllDestroyed && eliminationSpawnersUnkilled.Count == 0;

                        if (!allDead && Time.time - this.lastWaveChangeTime < timeToCompare) {
                            continue;
                        }

                        EndCurrentWaveNormally();

                        break;
                    case WaveType.Elimination:
                        if (eliminationSpawnersUnkilled.Count > 0) {
                            continue;
                        }

                        EndCurrentWaveNormally();
                        break;
                }
            }

            if (skipCurrentWave && listener != null) {
                listener.WaveEndedEarly(CurrentWaveInfo);
            }

            bool waveSkipped = false;

            do {
                var waveInfo = CurrentWaveInfo;

                if (!disableSyncroSpawners) {
                    // notify all synchro spawners
                    waveSkipped = SpawnOrSkipNewWave(waveInfo);
                    if (waveSkipped) {
                        if (isLoggingOn) {
                            Debug.Log("Wave skipped - wave# is: " + (currentLevelWave + 1) + " on Level: " + (currentLevel + 1));
                        }
                    } else {
                        waveSkipped = false;
                    }
                } else {
                    waveSkipped = false;
                }

                LevelWaveMusicSettings musicSpec = null;

                // change music maybe
                if (currentLevel > 0 && currentLevelWave == 0) {
                    if (isLoggingOn) {
                        Debug.Log("Level up - new level# is: " + (currentLevel + 1) + " . Wave 1 starting, occurred at time: " + Time.time);
                    }

                    musicSpec = waveInfo.musicSettings;
                } else if (currentLevel > 0 || currentLevelWave > 0) {
                    if (isLoggingOn) {
                        Debug.Log("Wave up - new wave# is: " + (currentLevelWave + 1) + " on Level: " + (currentLevel + 1) + ". Occured at time: " + Time.time);
                    }

                    musicSpec = waveInfo.musicSettings;
                } else if (currentLevel == 0 && currentLevelWave == 0) {
                    musicSpec = waveInfo.musicSettings;
                }

                previousWave = CurrentWaveInfo;
                currentLevelWave++;

                if (currentLevelWave >= WaveLengths.Count) {
                    currentLevelWave = 0;
                    currentLevel++;

                    if (!gameIsOver && currentLevel >= waveSettingsByLevel.Count) {
                        musicSpec = gameOverMusicSettings;
                        Win();
                        IsGameOver = true;
                    }
                }

                PlayMusicIfSet(musicSpec);
            }
            while (waveSkipped);

            lastWaveChangeTime = Time.time;
            hasFirstWaveBeenStarted = true;
            skipCurrentWave = false;
        }
    }

    private void EndCurrentWaveNormally() {
        // check for wave end bonuses
        if (ActiveWaveInfo.waveBeatBonusesEnabled && ActiveWaveInfo.waveDefeatVariableModifiers.statMods.Count > 0) {
            if (this.listener != null) {
                listener.WaveCompleteBonusesStart(ActiveWaveInfo.waveDefeatVariableModifiers.statMods);
            }

            WorldVariableModifier mod = null;

            for (var i = 0; i < ActiveWaveInfo.waveDefeatVariableModifiers.statMods.Count; i++) {
                mod = ActiveWaveInfo.waveDefeatVariableModifiers.statMods[i];
                WorldVariableTracker.ModifyPlayerStat(mod, trans);
            }
        }

        if (listener != null) {
            listener.WaveEnded(CurrentWaveInfo);
        }
    }

    private bool SkipWaveOrNot(LevelWave waveInfo, bool valueAbove) {
        var skipThisWave = true;

        for (var i = 0; i < waveInfo.skipWavePassCriteria.statMods.Count; i++) {
            var stat = waveInfo.skipWavePassCriteria.statMods[i];

            var variable = WorldVariableTracker.GetWorldVariable(stat._statName);
            if (variable == null) {
                skipThisWave = false;
                break;
            }
            var varVal = stat._varTypeToUse == WorldVariableTracker.VariableType._integer ? variable.CurrentIntValue : variable.CurrentFloatValue;
            var compareVal = stat._varTypeToUse == WorldVariableTracker.VariableType._integer ? stat._modValueIntAmt.Value : stat._modValueFloatAmt.Value;

            if (valueAbove) {
                if (varVal < compareVal) {
                    skipThisWave = false;
                    break;
                }
            } else {
                if (varVal > compareVal) {
                    skipThisWave = false;
                    break;
                }
            }
        }

        return skipThisWave;
    }

    private void SpawnNewWave(LevelWave waveInfo, bool isRestartWave) {
        eliminationSpawnersUnkilled.Clear();
        spawnedItemsRemaining.Clear();
        WaveRemainingItemsChanged();

        foreach (var syncro in syncroSpawners) {
            if (!syncro.WaveChange(isRestartWave)) { // returns true if wave found.
                continue;
            }

            switch (waveInfo.waveType) {
                case WaveType.Elimination:
                    eliminationSpawnersUnkilled.Add(syncro.GetInstanceID(), syncro);
                    break;
                case WaveType.Timed:
                    eliminationSpawnersUnkilled.Add(syncro.GetInstanceID(), syncro);
                    TimeRemainingInCurrentWave = CurrentWaveInfo.WaveDuration;
                    break;
            }
        }

        if (this.listener != null) {
            this.listener.WaveStarted(CurrentWaveInfo);
        }
    }

    // Return true to skip wave, false means we started spawning the wave.
    private bool SpawnOrSkipNewWave(LevelWave waveInfo) {
        var skipThisWave = true;

        if (enableWaveWarp) {
            // check for Custom Start Wave and skip all before it
            if (CurrentLevel < startLevelNumber.Value - 1) {
                return true; // skip
            } else if (CurrentLevel == startLevelNumber.Value - 1 && CurrentLevelWave < startWaveNumber.Value - 1) {
                return true; // skip
            } else {
                enableWaveWarp = false; // should only happen once after you pass the warped wave.
            }
        }

        if (waveInfo.skipWavePassCriteria.statMods.Count == 0 || waveInfo.skipWaveType == SkipWaveMode.None) {
            skipThisWave = false;
        }

        if (skipThisWave) {
            switch (waveInfo.skipWaveType) {
                case SkipWaveMode.Always:
                    break;
                case SkipWaveMode.IfWorldVariableValueAbove:
                    if (!SkipWaveOrNot(waveInfo, true)) {
                        skipThisWave = false;
                    }
                    break;
                case SkipWaveMode.IfWorldVariableValueBelow:
                    if (!SkipWaveOrNot(waveInfo, false)) {
                        skipThisWave = false;
                    }
                    break;
            }
        }

        if (skipThisWave) {
            if (listener != null) {
                listener.WaveSkipped(waveInfo);
            }
            return true;
        }

        SpawnNewWave(waveInfo, false);
        return false;
    }

    private void Win() {
        HasPlayerWon = true;
    }

    #endregion

    #region Public Static Methods
    public static void AddWaveSpawnedItem(Transform _trans) {
        if (spawnedItemsRemaining.Contains(_trans)) {
            return;
        }

        spawnedItemsRemaining.Add(_trans);
        WaveRemainingItemsChanged();
    }

    public static LevelSettings Instance {
        get {
            if (_lsInstance == null) {
                _lsInstance = (LevelSettings)GameObject.FindObjectOfType(typeof(LevelSettings));
            }

            return _lsInstance;
        }
        set {
            _lsInstance = null;
        }
    }

    public static void EliminationSpawnerCompleted(int instanceId) {
        eliminationSpawnersUnkilled.Remove(instanceId);
    }

    /// <summary>
    /// Call this method to immediately finish the current wave for Syncro Spawners.
    /// </summary>
    public static void EndWave() {
        skipCurrentWave = true;
    }

    /// <summary>
    /// Call this method to immediately finish the current wave for Syncro Spawners and go to a different level / wave you specify.
    /// </summary>
    /// <param name="levelNum">The level number to skip to.</param>
    /// <param name="waveNum">The wave number to skip to.</param>
    public static void GotoWave(int levelNum, int waveNum) {
        skipCurrentWave = true;
        currentLevel = levelNum - 1;
        currentLevelWave = waveNum - 1;
    }

    public static WavePrefabPool GetFirstMatchingPrefabPool(string poolName) {
        var poolsHolder = GetPoolsHolder;

        if (poolsHolder == null) {
            return null;
        }

        var oChild = poolsHolder.FindChild(poolName);

        if (oChild == null) {
            return null;
        }

        return oChild.GetComponent<WavePrefabPool>();
    }

    public static List<string> GetSortedPrefabPoolNames() {
        var poolsHolder = GetPoolsHolder;

        if (poolsHolder == null) {
            return null;
        }

        var pools = new List<string>();

        for (var i = 0; i < poolsHolder.childCount; i++) {
            var oChild = poolsHolder.GetChild(i);
            pools.Add(oChild.name);
        }

        pools.Sort();

        pools.Insert(0, "-None-");

        return pools;
    }

    public static void LogIfNew(string message, bool logAsWarning = false) {
        if (recentErrorsByTime.ContainsKey(message)) {
            var item = recentErrorsByTime[message];
            if (Time.time - 1f > item) {
                // it's been over 1 second. Log again
                recentErrorsByTime.Remove(message);
            } else {
                return;
            }
        }

        recentErrorsByTime.Add(message, Time.time);

        if (logAsWarning) {
            Debug.LogWarning(message);
        } else {
            Debug.LogError(message);
        }
    }

    /// <summary>
    /// Use this method to pause the current wave for Syncro Spawners.
    /// </summary>
    public static void PauseWave() {
        wavesArePaused = true;
    }

    /// <summary>
    /// Use this method to restart the current wave for Syncro Spawners. This puts the repeat wave counter back to zero as well. Also, this unpauses the wave if paused.
    /// </summary>
    public static void RestartCurrentWave() {
        if (IsGameOver) {
            LogIfNew("Cannot restart current wave because game is over for Core GameKit.");
            return; // no wave
        }

        // destroy spawns from current wave if any
        var restartMode = LevelSettings.Instance.waveRestartMode;
        if (restartMode != WaveRestartBehavior.LeaveSpawned) {
            var i = spawnedItemsRemaining.Count + 1;

            while (spawnedItemsRemaining.Count > 0) {
                var item = spawnedItemsRemaining[0];
                Killable maybeKillable = null;
                if (LevelSettings.Instance.waveRestartMode == WaveRestartBehavior.DestroySpawned) {
                    maybeKillable = item.GetComponent<Killable>();
                }

                if (maybeKillable != null) {
                    maybeKillable.DestroyKillable();
                } else {
                    PoolBoss.Despawn(spawnedItemsRemaining[0]);
                }

                i--;
                if (i < 0) {
                    break; // just in case. Don't want endless loops.
                }
            }
        }

        UnpauseWave();
        LevelSettings.Instance.SpawnNewWave(ActiveWaveInfo, true);

        if (LevelSettings.Instance.listener != null) {
            LevelSettings.Instance.listener.WaveRestarted(ActiveWaveInfo);
        }
    }

    public static void RemoveWaveSpawnedItem(Transform _trans) {
        if (!spawnedItemsRemaining.Contains(_trans)) {
            return;
        }

        spawnedItemsRemaining.Remove(_trans);
        WaveRemainingItemsChanged();
    }

    public static void TrackTimedRespawn(float delay, Transform prefabTrans, Vector3 pos) {
        prefabsToRespawn.Add(new RespawnTimer(delay, prefabTrans, pos));
    }

    /// <summary>
    /// Use this method to unpause the current wave for Syncro Spawners.
    /// </summary>
    public static void UnpauseWave() {
        wavesArePaused = false;
    }

    /// <summary>
    /// This method lets you start at a custom level and wave number. You must call this no later than Start for it to work properly.
    /// </summary>
    /// <param name="levelNumber">The level number to start on.</param>
    /// <param name="waveNumber">The wave number to start on.</param>
    public static void WarpToLevel(int levelNumber, int waveNumber) {
        LevelSettings.Instance.enableWaveWarp = true;
        LevelSettings.Instance.startLevelNumber.Value = (levelNumber - 1);
        LevelSettings.Instance.startWaveNumber.Value = waveNumber - 1;
    }

    #endregion

    #region Private Static Methods
    private static void PlayMusicIfSet(LevelWaveMusicSettings musicSpec) {
        if (LevelSettings.Instance.useMusicSettings && Instance.useWaves && musicSpec != null) {
            WaveMusicChanger.WaveUp(musicSpec);
        }
    }

    private static void WaveRemainingItemsChanged() {
        if (Listener != null) {
            Listener.WaveItemsRemainingChanged(WaveRemainingItemCount);
        }
    }
    #endregion

    #region Public Properties
    public static bool AppIsShuttingDown {
        get {
            return appIsShuttingDown;
        }
        set {
            appIsShuttingDown = value;
        }
    }

    /// <summary>
    /// This property returns the current wave info for Syncro Spawners
    /// </summary>
    public static LevelWave ActiveWaveInfo { // This is the only one you would read from code. "CurrentWaveInfo" is to be used by spawners only.
        get {
            LevelWave wave;
            if (previousWave != null) {
                wave = previousWave;
            } else {
                wave = CurrentWaveInfo;
            }

            return wave;
        }
    }

    /// <summary>
    /// This property returns the current level number (zero-based) for Syncro Spawners.
    /// </summary>
    public static int CurrentLevel {
        get {
            return currentLevel;
        }
    }

    /// <summary>
    /// This property returns the current wave number (zero-based) in the current level for Syncro Spawners.
    /// </summary>
    public static int CurrentLevelWave {
        get {
            return currentLevelWave;
        }
    }

    /// <summary>
    /// This property returns the current level number (zero-based) for Syncro Spawners.
    /// </summary>
    public int LevelNumber {
        get {
            return CurrentLevel;
        }
    }

    /// <summary>
    /// This property returns the current wave number (zero-based) in the current level for Syncro Spawners.
    /// </summary>
    public int WaveNumber {
        get {
            return CurrentLevelWave;
        }
    }

    public static LevelWave CurrentWaveInfo {
        get {
            if (WaveLengths.Count == 0) {
                LogIfNew("Not possible to restart wave. There are no waves set up in LevelSettings.");
                return null;
            }

            var waveInfo = WaveLengths[currentLevelWave];
            return waveInfo;
        }
    }

    public static List<Transform> GetAllPrefabPools {
        get {
            var holder = GetPoolsHolder;

            if (holder == null) {
                LogIfNew(NO_PREFAB_POOLS_CONTAINER_ALERT);
                return null;
            }

            var pools = new List<Transform>();
            for (var i = 0; i < holder.childCount; i++) {
                pools.Add(holder.GetChild(i));
            }

            return pools;
        }
    }

    public static List<Transform> GetAllSpawners {
        get {
            var spawnContainer = LevelSettings.Instance.transform.FindChild(SPAWNER_CONTAINER_TRANS_NAME);

            if (spawnContainer == null) {
                LogIfNew(NO_SPAWN_CONTAINER_ALERT);
                return null;
            }

            var spawners = new List<Transform>();
            for (var i = 0; i < spawnContainer.childCount; i++) {
                spawners.Add(spawnContainer.GetChild(i));
            }

            return spawners;
        }
    }

    public static List<Transform> GetAllWorldVariables {
        get {
            var holder = GetWorldVariablesHolder;

            if (holder == null) {
                LogIfNew(NO_WORLD_VARIABLES_CONTAINER_ALERT);
                return null;
            }

            var vars = new List<Transform>();
            for (var i = 0; i < holder.childCount; i++) {
                vars.Add(holder.GetChild(i));
            }

            return vars;
        }
    }

    /// <summary>
    /// Use this property to read or set "IsGameOver". If game is over, game over behavior will come into play, Syncro Spawners will stop spawning and waves will not advance.
    /// </summary>
    public static bool IsGameOver {
        get {
            return gameIsOver;
        }
        set {
            bool wasGameOver = gameIsOver;
            gameIsOver = value;

            if (gameIsOver) {
                if (!wasGameOver) {
                    if (Listener != null) {
                        Listener.GameOver(HasPlayerWon);

                        if (!HasPlayerWon) {
                            Listener.Lose();
                        }
                    }
                }

                var musicSpec = LevelSettings.Instance.gameOverMusicSettings;

                PlayMusicIfSet(musicSpec);
            }
        }
    }

    public static bool HasPlayerWon {
        get {
            return hasPlayerWon;
        }
        set {
            hasPlayerWon = value;

            if (value && Listener != null) {
                Listener.Win();
            }
        }
    }

    /// <summary>
    /// This property returns whether or not logging is turned on in Level Settings.
    /// </summary>
    public static bool IsLoggingOn {
        get {
            return LevelSettings.Instance != null && LevelSettings.Instance.isLoggingOn;
        }
    }

    /// <summary>
    /// This property returns the number of the last level you have set up (zero-based).
    /// </summary>
    public static int LastLevel {
        get {
            return waveSettingsByLevel.Count;
        }
    }

    public static LevelSettingsListener Listener {
        get {
            if (AppIsShuttingDown) {
                return null;
            }

            if (LevelSettings.Instance != null) {
                return LevelSettings.Instance.listener;
            } else {
                return null;
            }
        }
    }

    public static LevelWave PreviousWaveInfo {
        get {
            return previousWave;
        }
    }

    /// <summary>
    /// This property returns the number of seconds remaining in the current wave for Syncro Spawners. -1 is returned for elimination waves.
    /// </summary>
    public static int TimeRemainingInCurrentWave {
        get {
            LevelWave wave = ActiveWaveInfo;

            switch (wave.waveType) {
                case WaveType.Elimination:
                    return -1;
                case WaveType.Timed:
                    return waveTimeRemaining;
            }

            return -1;
        }
        set {
            waveTimeRemaining = value;

            if (ActiveWaveInfo.waveType == WaveType.Timed && Listener != null) {
                Listener.WaveTimeRemainingChanged(waveTimeRemaining);
            }
        }
    }

    /// <summary>
    /// This property returns a list of all wave settings in the current Level.
    /// </summary>
    public static List<LevelWave> WaveLengths {
        get {
            if (!waveSettingsByLevel.ContainsKey(currentLevel)) {
                return new List<LevelWave>();
            }
            return waveSettingsByLevel[currentLevel];
        }
    }

    /// <summary>
    /// This property will return whether the current wave is paused for Syncro Spawners.
    /// </summary>
    public static bool WavesArePaused {
        get {
            return wavesArePaused;
        }
    }

    #endregion

    #region Private properties
    private static Transform GetPoolsHolder {
        get {
            var lev = LevelSettings.Instance;
            if (lev == null) {
                return null;
            }

            return lev.transform.FindChild(PREFAB_POOLS_CONTAINER_TRANS_NAME);
        }
    }

    private static Transform GetWorldVariablesHolder {
        get {
            var lev = LevelSettings.Instance;
            if (lev == null) {
                return null;
            }

            return lev.transform.FindChild(WORLD_VARIABLES_CONTAINER_TRANS_NAME);
        }
    }

    private static int WaveRemainingItemCount {
        get {
            return spawnedItemsRemaining.Count;
        }
    }

    #endregion

    #region Custom Events
    /// <summary>
    /// This method is used by MasterAudio to keep track of enabled CustomEventReceivers automatically. This is called when then CustomEventReceiver prefab is enabled.
    /// </summary>
    public static void AddCustomEventReceiver(ICgkEventReceiver receiver, Transform receiverTrans) {
        if (AppIsShuttingDown) {
            return;
        }

        for (var i = 0; i < LevelSettings.Instance.customEvents.Count; i++) {
            var anEvent = LevelSettings.Instance.customEvents[i];
            if (!receiver.SubscribesToEvent(anEvent.EventName)) {
                continue;
            }

            if (!receiversByEventName.ContainsKey(anEvent.EventName)) {
                receiversByEventName.Add(anEvent.EventName, new Dictionary<ICgkEventReceiver, Transform> {
                    { receiver, receiverTrans }
                });
            } else {
                var dict = receiversByEventName[anEvent.EventName];
                if (dict.ContainsKey(receiver)) {
                    continue;
                }

                dict.Add(receiver, receiverTrans);
            }
        }
    }

    /// <summary>
    /// This method is used by MasterAudio to keep track of enabled CustomEventReceivers automatically. This is called when then CustomEventReceiver prefab is disabled.
    /// </summary>
    public static void RemoveCustomEventReceiver(ICgkEventReceiver receiver) {
        if (AppIsShuttingDown) {
            return;
        }

        for (var i = 0; i < LevelSettings.Instance.customEvents.Count; i++) {
            var anEvent = LevelSettings.Instance.customEvents[i];
            if (!receiver.SubscribesToEvent(anEvent.EventName)) {
                continue;
            }

            var dict = receiversByEventName[anEvent.EventName];
            dict.Remove(receiver);
        }
    }

    public static List<Transform> ReceiversForEvent(string customEventName) {
        var receivers = new List<Transform>();

        if (!receiversByEventName.ContainsKey(customEventName)) {
            return receivers;
        }

        var dict = receiversByEventName[customEventName];

        foreach (var receiver in dict.Keys) {
            if (receiver.SubscribesToEvent(customEventName)) {
                receivers.Add(dict[receiver]);
            }
        }

        return receivers;
    }

    /// <summary>
    /// Calling this method will fire a Custom Event at the originPoint position. All CustomEventReceivers with the named event specified will do whatever action is assigned to them. If there is a distance criteria applied to receivers, it will be applied.
    /// </summary>
    /// <param name="customEventName">The name of the custom event.</param>
    /// <param name="originPoint">The position of the event.</param> 
    public static void FireCustomEvent(string customEventName, Vector3 originPoint) {
        if (AppIsShuttingDown) {
            return;
        }

        if (!CustomEventExists(customEventName)) {
            Debug.LogError("Custom Event '" + customEventName + "' was not found in Core GameKit.");
            return;
        }

        var customEvent = GetCustomEventByName(customEventName);

        if (customEvent.frameLastFired >= Time.frameCount) {
            Debug.LogWarning("Already fired Custom Event '" + customEventName + "' this frame. Cannot be fired twice in the same frame.");
            return;
        }

        customEvent.frameLastFired = Time.frameCount;

        float? sqrDist = null;
        switch (customEvent.eventRcvMode) {
            case EventReceiveMode.Never:
                if (IsLoggingOn) {
                    LogIfNew("Custom Event '" + customEventName + "' not being transmitted because it is set to 'Never transmit'.", true);
                }
                return; // no transmission.
            case EventReceiveMode.WhenDistanceLessThan:
            case EventReceiveMode.WhenDistanceMoreThan:
                sqrDist = customEvent.distanceThreshold.Value * customEvent.distanceThreshold.Value;
                break;
        }

        if (!receiversByEventName.ContainsKey(customEventName)) {
            //Debug.LogWarning("There are no Receivers for Custom Event '" + customEventName + "'.");
            return;
        }

        var dict = receiversByEventName[customEventName];
        foreach (var receiver in dict.Keys) {
            switch (customEvent.eventRcvMode) {
                case EventReceiveMode.WhenDistanceLessThan:
                    var dist = (dict[receiver].position - originPoint).sqrMagnitude;
                    if (dist > sqrDist) {
                        continue;
                    }
                    break;
                case EventReceiveMode.WhenDistanceMoreThan:
                    var dist2 = (dict[receiver].position - originPoint).sqrMagnitude;
                    if (dist2 < sqrDist) {
                        continue;
                    }
                    break;
            }

            receiver.ReceiveEvent(customEventName, originPoint);
        }
    }

    private static CgkCustomEvent GetCustomEventByName(string customEventName) {
        var matches = LevelSettings.Instance.customEvents.FindAll(delegate(CgkCustomEvent obj) {
            return obj.EventName == customEventName;
        });

        return matches.Count > 0 ? matches[0] : null;
    }

    /// <summary>
    /// Calling this method will return whether or not the specified Custom Event exists.
    /// </summary>
    public static bool CustomEventExists(string customEventName) {
        if (AppIsShuttingDown) {
            return true;
        }

        return GetCustomEventByName(customEventName) != null;
    }

    /// <summary>
    /// This will return a list of all the Custom Events you have defined, including the selectors for "type in" and "none".
    /// </summary>
    public List<string> CustomEventNames {
        get {
            var customEventNames = new List<string>();

            customEventNames.Add(DYNAMIC_EVENT_NAME);
            customEventNames.Add(NO_EVENT_NAME);

            var custEvents = LevelSettings.Instance.customEvents;

            for (var i = 0; i < custEvents.Count; i++) {
                customEventNames.Add(custEvents[i].EventName);
            }

            return customEventNames;
        }
    }

    #endregion
}