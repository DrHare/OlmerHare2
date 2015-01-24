using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// This class is used for Syncro Spawner setup.
/// </summary>
public class WaveSyncroPrefabSpawner : MonoBehaviour {
    public List<WaveSpecifics> waveSpecs = new List<WaveSpecifics>();
    public bool isExpanded = true;

    public LevelSettings.ActiveItemMode activeMode = LevelSettings.ActiveItemMode.Always;
    public WorldVariableRangeCollection activeItemCriteria = new WorldVariableRangeCollection();

    public TriggeredSpawner.GameOverBehavior gameOverBehavior = TriggeredSpawner.GameOverBehavior.Disable;
    public TriggeredSpawner.WavePauseBehavior wavePauseBehavior = TriggeredSpawner.WavePauseBehavior.Disable;
    public WaveSyncroSpawnerListener listener;

    public SpawnLayerTagMode spawnLayerMode = SpawnLayerTagMode.UseSpawnPrefabSettings;
    public SpawnLayerTagMode spawnTagMode = SpawnLayerTagMode.UseSpawnPrefabSettings;
    public int spawnCustomLayer = 0;
    public string spawnCustomTag = "Untagged";

    private int currentWaveSize;
    private int itemsToCompleteWave = 0;
    private float currentWaveLength;
    private bool waveFinishedSpawning;
    private bool levelSettingsNotifiedOfCompletion = false;
    private int countSpawned;
    private float singleSpawnTime;
    private float lastSpawnTime;
    private WaveSpecifics currentWave;
    private float waveStartTime;
    private Transform trans;
    private GameObject go;
    private List<Transform> spawnedWaveMembers = new List<Transform>();
    private float? repeatTimer;
    private float repeatWaitTime;
    private int waveRepetitionNumber;
    private bool spawnerValid;
    private WavePrefabPool wavePool = null;
    private int instanceId;
    private bool settingUpWave = false;

    private float currentRandomLimitDistance;

    public enum SpawnLayerTagMode {
        UseSpawnPrefabSettings,
        UseSpawnerSettings,
        Custom
    }

    // Use this for initialization
    void Awake() {
        this.go = this.gameObject;
        this.trans = this.transform;
        this.waveFinishedSpawning = true;
        this.levelSettingsNotifiedOfCompletion = false;
        this.repeatTimer = null;
        this.spawnerValid = true;
        this.waveRepetitionNumber = 0;
        this.instanceId = GetInstanceID();

        this.CheckForDuplicateWaveLevelSettings();
    }

    void Start() {
        for (var i = 0; i < waveSpecs.Count; i++) {
            var wave = waveSpecs[i];
            CheckForValidVariablesForWave(wave);
        }

        CheckForValidWorldVariables();
    }

    private bool SpawnerIsPaused {
        get {
            return LevelSettings.WavesArePaused && wavePauseBehavior == TriggeredSpawner.WavePauseBehavior.Disable;
        }
    }

    private bool GameIsOverForSpawner {
        get {
            return LevelSettings.IsGameOver && this.gameOverBehavior == TriggeredSpawner.GameOverBehavior.Disable;
        }
    }

    private void CheckForDuplicateWaveLevelSettings() {
        var waveLevelCombos = new List<string>();
        foreach (var wave in waveSpecs) {
            var combo = wave.SpawnLevelNumber + ":" + wave.SpawnWaveNumber;
            if (waveLevelCombos.Contains(combo)) {
                LevelSettings.LogIfNew(string.Format("Spawner '{0}' contains more than one wave setting for level: {1} and wave: {2}. Spawner aborting until this is fixed.",
                    this.name,
                    wave.SpawnLevelNumber + 1,
                    wave.SpawnWaveNumber + 1
                    ));
                this.spawnerValid = false;

                break;
            }

            waveLevelCombos.Add(combo);
        }
    }

    public bool WaveChange(bool isRestart) {
        if (!this.spawnerValid) {
            return false;
        }

        bool setupNew = SetupNextWave(true, isRestart);
        if (setupNew) {
            if (listener != null) {
                listener.WaveStart(this.currentWave);
            }
        }

        return setupNew;
    }

    public void WaveRepeat() {
        if (this.currentWave.waveRepeatBonusesEnabled && this.currentWave.waveRepeatVariableModifiers.statMods.Count > 0) {
            WorldVariableModifier mod = null;

            for (var i = 0; i < this.currentWave.waveRepeatVariableModifiers.statMods.Count; i++) {
                mod = this.currentWave.waveRepeatVariableModifiers.statMods[i];
                WorldVariableTracker.ModifyPlayerStat(mod, this.trans);
            }
        }

        if (SetupNextWave(false, false)) {
            if (listener != null) {
                listener.WaveRepeat(this.currentWave);
            }
        }
    }

    void Update() {
        if (GameIsOverForSpawner || SpawnerIsPaused || this.currentWave == null || !this.spawnerValid || settingUpWave) {
            return;
        }

        this.StartNextEliminationWave();

        if (this.waveFinishedSpawning
            || (Time.time - this.waveStartTime < this.currentWave.WaveDelaySec.Value)
            || (Time.time - this.lastSpawnTime <= this.singleSpawnTime && this.singleSpawnTime > Time.deltaTime)) {

            return;
        }

        int numberToSpawn = 1;
        if (this.singleSpawnTime < Time.deltaTime) {
            if (this.singleSpawnTime == 0) {
                numberToSpawn = currentWaveSize;
            } else {
                numberToSpawn = (int)Math.Ceiling(Time.deltaTime / this.singleSpawnTime);
            }
        }

        for (var i = 0; i < numberToSpawn; i++) {
            if (this.CanSpawnOne()) {
                SpawnOne();
            }

            if (this.countSpawned >= currentWaveSize) {
                if (LevelSettings.IsLoggingOn) {
                    Debug.Log(string.Format("Spawner '{0}' finished spawning wave# {1} on level# {2}.",
                        this.name,
                        this.currentWave.SpawnWaveNumber + 1,
                        this.currentWave.SpawnLevelNumber + 1));
                }
                this.waveFinishedSpawning = true;

                if (this.listener != null) {
                    this.listener.WaveFinishedSpawning(this.currentWave);
                }
            }

            this.lastSpawnTime = Time.time;
        }
    }

    /// <summary> 
    /// Calling this method will spawn one of the current wave for this Level and Wave, if this Spawner is configured to use that Level and Wave.
    /// </summary>
    /// <returns>The Transform of the spawned item</returns>
    public Transform SpawnOneItem() {
        return SpawnOne(true);
    }

    private Transform SpawnOne(bool fromExternalScript = false) {
        if (fromExternalScript && this.currentWave == null) {
            return null; // no active spawner for this wave.
        }

        Transform prefabToSpawn = this.GetSpawnable(this.currentWave);

        if (this.currentWave.spawnSource == WaveSpecifics.SpawnOrigin.PrefabPool && prefabToSpawn == null) {
            return null;
        }

        if (prefabToSpawn == null) {
            if (fromExternalScript) {
                LevelSettings.LogIfNew("Cannot 'spawn one' from spawner: " + this.trans.name + " because it has either no settings or selected prefab to spawn for the current wave.");
                return null;
            }

            LevelSettings.LogIfNew(string.Format("Spawner '{0}' has no prefab to spawn for wave# {1} on level# {2}.",
                this.name,
                this.currentWave.SpawnWaveNumber + 1,
                this.currentWave.SpawnLevelNumber + 1));

            this.spawnerValid = false;
            return null;
        }

        var spawnPosition = this.GetSpawnPosition(this.trans.position, this.countSpawned);

        var spawnedPrefab = PoolBoss.SpawnInPool(prefabToSpawn,
            spawnPosition, this.GetSpawnRotation(prefabToSpawn, this.countSpawned));

        if (spawnedPrefab == null) {
            LevelSettings.LogIfNew("Could not spawn: " + prefabToSpawn); // in case you might want to increase your pool size so this doesn't happen. If not, comment out this line.
            if (listener != null) {
                listener.ItemFailedToSpawn(prefabToSpawn);
            }
            return null;
        } else {
            SpawnUtility.RecordSpawnerObjectIfKillable(spawnedPrefab, this.go);
        }

        this.AddSpawnTracker(spawnedPrefab);
        this.AfterSpawn(spawnedPrefab);
		
		this.spawnedWaveMembers.Add(spawnedPrefab);
        LevelSettings.AddWaveSpawnedItem(spawnedPrefab);
        this.countSpawned++;

        if (this.currentWave.enableLimits) {
            currentRandomLimitDistance = UnityEngine.Random.Range(-this.currentWave.doNotSpawnRandomDist.Value, this.currentWave.doNotSpawnRandomDist.Value);
        }
		
		return spawnedPrefab;
    }

    protected virtual Vector3 GetSpawnPosition(Vector3 pos, int itemSpawnedIndex) {
        if (this.currentWave.positionXmode == WaveSpecifics.PositionMode.CustomPosition) {
            pos.x = this.currentWave.customPosX.Value;
        }

        if (this.currentWave.positionYmode == WaveSpecifics.PositionMode.CustomPosition) {
            pos.y = this.currentWave.customPosY.Value;
        }

        if (this.currentWave.positionZmode == WaveSpecifics.PositionMode.CustomPosition) {
            pos.z = this.currentWave.customPosZ.Value;
        }

        var addVector = Vector3.zero;

        addVector += this.currentWave.waveOffset;

        if (this.currentWave.enableRandomizations) {
            addVector.x = UnityEngine.Random.Range(-currentWave.randomDistX.Value, currentWave.randomDistX.Value);
            addVector.y = UnityEngine.Random.Range(-currentWave.randomDistY.Value, currentWave.randomDistY.Value);
            addVector.z = UnityEngine.Random.Range(-currentWave.randomDistZ.Value, currentWave.randomDistZ.Value);
        }

        if (this.currentWave.enableIncrements && itemSpawnedIndex > 0) {
            addVector.x += (currentWave.incrementPositionX.Value * itemSpawnedIndex);
            addVector.y += (currentWave.incrementPositionY.Value * itemSpawnedIndex);
            addVector.z += (currentWave.incrementPositionZ.Value * itemSpawnedIndex);
        }

        return pos + addVector;
    }

    protected virtual Quaternion GetSpawnRotation(Transform prefabToSpawn, int itemSpawnedIndex) {
        Vector3 euler = Vector3.zero;

        switch (currentWave.curRotationMode) {
            case WaveSpecifics.RotationMode.UsePrefabRotation:
                euler = prefabToSpawn.rotation.eulerAngles;
                break;
            case WaveSpecifics.RotationMode.UseSpawnerRotation:
                euler = this.trans.rotation.eulerAngles;
                break;
            case WaveSpecifics.RotationMode.CustomRotation:
                euler = currentWave.customRotation;
                break;
        }

        if (this.currentWave.enableRandomizations && this.currentWave.randomXRotation) {
            euler.x = UnityEngine.Random.Range(this.currentWave.randomXRotMin.Value, this.currentWave.randomXRotMax.Value);
        } else if (this.currentWave.enableIncrements && itemSpawnedIndex > 0) {
            if (currentWave.enableKeepCenter) {
                euler.x += (itemSpawnedIndex * currentWave.incrementRotX.Value - (currentWaveSize * currentWave.incrementRotX.Value * .5f));
            } else {
                euler.x += (itemSpawnedIndex * currentWave.incrementRotX.Value);
            }
        }

        if (this.currentWave.enableRandomizations && this.currentWave.randomYRotation) {
            euler.y = UnityEngine.Random.Range(this.currentWave.randomYRotMin.Value, this.currentWave.randomYRotMax.Value);
        } else if (this.currentWave.enableIncrements && itemSpawnedIndex > 0) {
            if (currentWave.enableKeepCenter) {
                euler.y += (itemSpawnedIndex * currentWave.incrementRotY.Value - (currentWaveSize * currentWave.incrementRotY.Value * .5f));
            } else {
                euler.y += (itemSpawnedIndex * currentWave.incrementRotY.Value);
            }
        }

        if (this.currentWave.enableRandomizations && this.currentWave.randomZRotation) {
            euler.z = UnityEngine.Random.Range(this.currentWave.randomZRotMin.Value, this.currentWave.randomZRotMax.Value);
        } else if (this.currentWave.enableIncrements && itemSpawnedIndex > 0) {
            if (currentWave.enableKeepCenter) {
                euler.z += (itemSpawnedIndex * currentWave.incrementRotZ.Value - (currentWaveSize * currentWave.incrementRotZ.Value * .5f));
            } else {
                euler.z += (itemSpawnedIndex * currentWave.incrementRotZ.Value);
            }
        }

        return Quaternion.Euler(euler);
    }

    private void AddSpawnTracker(Transform spawnedTrans) {
        var tracker = spawnedTrans.GetComponent<SpawnTracker>();
        if (tracker == null) {
            spawnedTrans.gameObject.AddComponent(typeof(SpawnTracker));
            tracker = spawnedTrans.GetComponent<SpawnTracker>();
        }

        tracker.SourceSpawner = this;
    }

    protected virtual void AfterSpawn(Transform spawnedTrans) {
        if (this.currentWave.enablePostSpawnNudge) {
            spawnedTrans.Translate(Vector3.forward * this.currentWave.postSpawnNudgeFwd.Value);
            spawnedTrans.Translate(Vector3.right * this.currentWave.postSpawnNudgeRgt.Value);
            spawnedTrans.Translate(Vector3.down * this.currentWave.postSpawnNudgeDwn.Value);
        }

        switch (spawnLayerMode) {
            case SpawnLayerTagMode.UseSpawnerSettings:
                spawnedTrans.gameObject.layer = go.layer;
                break;
            case SpawnLayerTagMode.Custom:
                spawnedTrans.gameObject.layer = spawnCustomLayer;
                break;
        }

        switch (spawnTagMode) {
            case SpawnLayerTagMode.UseSpawnerSettings:
                spawnedTrans.gameObject.tag = go.tag;
                break;
            case SpawnLayerTagMode.Custom:
                spawnedTrans.gameObject.tag = spawnCustomTag;
                break;
        }

        if (this.listener != null) {
            listener.ItemSpawned(spawnedTrans);
        }
    }

    protected virtual bool CanSpawnOne() {
        if (!this.currentWave.enableLimits) {
			return true;
		}

		var allSpawnedAreFarEnoughAway = SpawnUtility.SpawnedMembersAreAllBeyondDistance(this.trans, this.spawnedWaveMembers,
            this.currentWave.doNotSpawnIfMbrCloserThan.Value + currentRandomLimitDistance);
        
		return allSpawnedAreFarEnoughAway;
    }

    public bool IsUsingPrefabPool(Transform poolTrans) {
        foreach (var _wave in this.waveSpecs) {
            if (_wave.spawnSource == WaveSpecifics.SpawnOrigin.PrefabPool && _wave.prefabPoolName == poolTrans.name) {
                return true;
            }
        }

        return false;
    }

    public WaveSpecifics FindWave(int levelToMatch, int waveToMatch) {
        foreach (var _wave in this.waveSpecs) {
            if (_wave.SpawnLevelNumber != levelToMatch || _wave.SpawnWaveNumber != waveToMatch) {
                continue;
            }

            // found the match, get outa here!!
            return _wave;
        }

        return null;
    }

    private void LogAdjustments(int adjustments) {
        if (adjustments > 0) {
            Debug.Log(string.Format("Adjusted {0} wave(s) in spawner '{1}' to match new Level/Wave numbers", adjustments, this.name));
        }
    }

    public void DeleteLevel(int level) {
        var deadWaves = new List<WaveSpecifics>();

        foreach (var wrongWave in this.waveSpecs) {
            if (wrongWave.SpawnLevelNumber == level) {
                deadWaves.Add(wrongWave);
            }
        }

        foreach (var dead in deadWaves) {
            this.waveSpecs.Remove(dead);
        }

        if (deadWaves.Count > 0) {
            Debug.Log(string.Format("Deleted {0} matching wave(s) in spawner '{1}'", deadWaves.Count, this.name));
        }

        int adjusted = 0;
        foreach (var wrongWave in this.waveSpecs) {
            if (wrongWave.SpawnLevelNumber > level) {
                wrongWave.SpawnLevelNumber--;
                adjusted++;
            }
        }

        LogAdjustments(adjusted);
    }

    public void InsertLevel(int level) {
        int adjustments = 0;

        foreach (var wrongWave in this.waveSpecs) {
            if (wrongWave.SpawnLevelNumber >= level) {
                wrongWave.SpawnLevelNumber++;
                adjustments++;
            }
        }

        LogAdjustments(adjustments);
    }

    public void InsertWave(int newWaveNumber, int level) {
        int adjustments = 0;

        foreach (var wrongWave in this.waveSpecs) {
            if (wrongWave.SpawnLevelNumber == level && wrongWave.SpawnWaveNumber >= newWaveNumber) {
                wrongWave.SpawnWaveNumber++;
                adjustments++;
            }
        }

        LogAdjustments(adjustments);
    }

    public void DeleteWave(int level, int wav) {
        var matchingWave = FindWave(level, wav);
        if (matchingWave != null) {
            this.waveSpecs.Remove(matchingWave);
            Debug.Log(string.Format("Deleted matching wave in spawner '{0}'", this.name));
        }

        int adjustments = 0;

        // move same level, higher waves back one.
        foreach (var wrongWave in this.waveSpecs) {
            if (wrongWave.SpawnLevelNumber == level && wrongWave.SpawnWaveNumber > wav) {
                wrongWave.SpawnWaveNumber--;
                adjustments++;
            }
        }

        LogAdjustments(adjustments);
    }

    private bool SetupNextWave(bool scanForWave, bool isRestart) {
        this.repeatTimer = null;

        if (this.activeMode == LevelSettings.ActiveItemMode.Never) { // even in repeating waves.
            return false;
        }

        if (isRestart && this.currentWave == null) {
            return false; // can't restart because the current wave isn't configured in this Spawner.
        }

        var shouldInit = scanForWave || isRestart;

        if (scanForWave && !isRestart) {
            // find wave
            settingUpWave = true;
            this.currentWave = FindWave(LevelSettings.CurrentLevel, LevelSettings.CurrentWaveInfo.sequencedWaveNumber);

            // validate for all things that could go wrong!
            if (this.currentWave == null || !this.currentWave.enableWave) {
                return false;
            }

            // check "active mode" for conditions
            switch (activeMode) {
                case LevelSettings.ActiveItemMode.Never:
                    return false;
                case LevelSettings.ActiveItemMode.IfWorldVariableInRange:
                    if (activeItemCriteria.statMods.Count == 0) {
                        return false;
                    }
                    for (var i = 0; i < activeItemCriteria.statMods.Count; i++) {
                        var stat = activeItemCriteria.statMods[i];
                        var variable = WorldVariableTracker.GetWorldVariable(stat._statName);
                        if (variable == null) {
                            return false;
                        }
                        var varVal = stat._varTypeToUse == WorldVariableTracker.VariableType._integer ? variable.CurrentIntValue : variable.CurrentFloatValue;

                        var min = stat._varTypeToUse == WorldVariableTracker.VariableType._integer ? stat._modValueIntMin : stat._modValueFloatMin;
                        var max = stat._varTypeToUse == WorldVariableTracker.VariableType._integer ? stat._modValueIntMax : stat._modValueFloatMax;

                        if (min > max) {
                            LevelSettings.LogIfNew("The Min cannot be greater than the Max for Active Item Limit in Syncro Spawner '" + this.trans.name + "'.");
                            return false;
                        }

                        if (varVal < min || varVal > max) {
                            return false;
                        }
                    }

                    break;
                case LevelSettings.ActiveItemMode.IfWorldVariableOutsideRange:
                    if (activeItemCriteria.statMods.Count == 0) {
                        return false;
                    }
                    for (var i = 0; i < activeItemCriteria.statMods.Count; i++) {
                        var stat = activeItemCriteria.statMods[i];
                        var variable = WorldVariableTracker.GetWorldVariable(stat._statName);
                        if (variable == null) {
                            return false;
                        }
                        var varVal = stat._varTypeToUse == WorldVariableTracker.VariableType._integer ? variable.CurrentIntValue : variable.CurrentFloatValue;

                        var min = stat._varTypeToUse == WorldVariableTracker.VariableType._integer ? stat._modValueIntMin : stat._modValueFloatMin;
                        var max = stat._varTypeToUse == WorldVariableTracker.VariableType._integer ? stat._modValueIntMax : stat._modValueFloatMax;

                        if (min > max) {
                            LevelSettings.LogIfNew("The Min cannot be greater than the Max for Active Item Limit in Syncro Spawner '" + this.trans.name + "'.");
                            return false;
                        }

                        if (varVal >= min && varVal <= max) {
                            return false;
                        }
                    }

                    break;
            }

            if (this.currentWave.MinToSpwn.Value == 0 || this.currentWave.MaxToSpwn.Value == 0) {
                return false;
            }

            if (scanForWave && this.currentWave.WaveDelaySec.Value + this.currentWave.TimeToSpawnEntireWave.Value >= LevelSettings.CurrentWaveInfo.WaveDuration && LevelSettings.CurrentWaveInfo.waveType == LevelSettings.WaveType.Timed) {
                LevelSettings.LogIfNew(string.Format("Wave TimeToSpawnWholeWave plus Wave DelaySeconds must be less than the current LevelSettings wave duration, occured in spawner: {0}, wave# {1}, level {2}.",
                    this.name,
                    this.currentWave.SpawnWaveNumber + 1,
                    this.currentWave.SpawnLevelNumber + 1));
                return false;
            }

            if (this.currentWave.MinToSpwn.Value > this.currentWave.MaxToSpwn.Value) {
                LevelSettings.LogIfNew(string.Format("Wave MinToSpawn cannot be greater than Wave MaxToSpawn, occured in spawner: {0}, wave# {1}, level {2}.",
                    this.name,
                    this.currentWave.SpawnWaveNumber + 1,
                    this.currentWave.SpawnLevelNumber + 1));
                return false;
            }

            if (this.currentWave.repeatWaveUntilNew && this.currentWave.repeatPauseMinimum.Value > this.currentWave.repeatPauseMaximum.Value) {
                LevelSettings.LogIfNew(string.Format("Wave Repeat Pause Min cannot be greater than Wave Repeat Pause Max, occurred in spawner: {0}, wave# {1}, level {2}.",
                    this.name,
                    this.currentWave.SpawnWaveNumber + 1,
                    this.currentWave.SpawnLevelNumber + 1));
                return false;
            }
        }

        if (LevelSettings.IsLoggingOn) {
            var waveStatus = isRestart ? "Restarting" : string.Empty;
            if (string.IsNullOrEmpty(waveStatus)) {
                waveStatus = scanForWave ? "Starting" : "Repeating";
            }

            Debug.Log(string.Format("{0} matching wave from spawner: {1}, wave# {2}, level {3}.",
                waveStatus,
                this.name,
                this.currentWave.SpawnWaveNumber + 1,
                this.currentWave.SpawnLevelNumber + 1));
        }

        if (this.currentWave.spawnSource == WaveSpecifics.SpawnOrigin.PrefabPool) {
            var poolTrans = LevelSettings.GetFirstMatchingPrefabPool(this.currentWave.prefabPoolName);
            if (poolTrans == null) {
                LevelSettings.LogIfNew(string.Format("Spawner '{0}' wave# {1}, level {2} is trying to use a Prefab Pool that can't be found.",
                    this.name,
                    this.currentWave.SpawnWaveNumber + 1,
                    this.currentWave.SpawnLevelNumber + 1));
                spawnerValid = false;
                this.currentWave = null;
                return false;
            }

            wavePool = poolTrans;
        } else {
            wavePool = null;
        }

        settingUpWave = false;

        CheckForValidVariablesForWave(this.currentWave);

        this.spawnedWaveMembers.Clear();

        this.currentWaveSize = UnityEngine.Random.Range(currentWave.MinToSpwn.Value, currentWave.MaxToSpwn.Value);
        this.currentWaveLength = currentWave.TimeToSpawnEntireWave.Value;

        itemsToCompleteWave = (int)(this.currentWaveSize * currentWave.waveCompletePercentage * .01f);

        if (this.currentWave.repeatWaveUntilNew) {
            if (shouldInit && (LevelSettings.CurrentWaveInfo.waveType != LevelSettings.WaveType.Elimination || this.currentWave.curWaveRepeatMode == WaveSpecifics.RepeatWaveMode.Endless)) { // only the first time!
                this.currentWave.repetitionsToDo.Value = int.MaxValue;
            }

            this.currentWaveSize += (this.waveRepetitionNumber * this.currentWave.repeatItemInc.Value);
            this.currentWaveSize = Math.Min(this.currentWaveSize, this.currentWave.repeatItemLmt.Value); // cannot exceed limits

            currentWaveLength += (this.waveRepetitionNumber * this.currentWave.repeatTimeInc.Value);
            this.currentWaveLength = Math.Min(this.currentWaveLength, this.currentWave.repeatTimeLmt.Value); // cannot exceed limits
        }

        currentWaveLength = Math.Max(0f, currentWaveLength);

        if (shouldInit) { // not on wave repeat!
            this.waveRepetitionNumber = 0;
        }

        this.waveStartTime = Time.time;
        this.waveFinishedSpawning = false;
        this.levelSettingsNotifiedOfCompletion = false;
        this.countSpawned = 0;
        this.singleSpawnTime = currentWaveLength / (float)this.currentWaveSize;

        if (this.currentWave.enableLimits) {
            currentRandomLimitDistance = UnityEngine.Random.Range(-this.currentWave.doNotSpawnRandomDist.Value, this.currentWave.doNotSpawnRandomDist.Value);
        }

        return true;
    }

    private void CheckForValidWorldVariables() {
        if (activeMode == LevelSettings.ActiveItemMode.IfWorldVariableInRange || activeMode == LevelSettings.ActiveItemMode.IfWorldVariableOutsideRange) {
            for (var i = 0; i < activeItemCriteria.statMods.Count; i++) {
                var crit = activeItemCriteria.statMods[i];

                if (WorldVariableTracker.IsBlankVariableName(crit._statName)) {
                    LevelSettings.LogIfNew(string.Format("Spawner '{0}' has an Active Item Limit criteria with no World Variable selected. Please select one.",
                        this.trans.name));
                    spawnerValid = false;
                } else if (!WorldVariableTracker.VariableExistsInScene(crit._statName)) {
                    LevelSettings.LogIfNew(string.Format("Spawner '{0}' has an Active Item Limit criteria criteria of World Variable '{1}', which doesn't exist in the scene.",
                        this.trans.name,
                        crit._statName));
                    spawnerValid = false;
                }
            }
        }

        for (var i = 0; i < waveSpecs.Count; i++) {
            var wave = waveSpecs[i];

            if (!wave.waveRepeatBonusesEnabled) {
                continue;
            }

            for (var b = 0; b < wave.waveRepeatVariableModifiers.statMods.Count; b++) {
                var beatMod = wave.waveRepeatVariableModifiers.statMods[b];

                if (WorldVariableTracker.IsBlankVariableName(beatMod._statName)) {
                    LevelSettings.LogIfNew(string.Format("Spawner '{0}', Level {1} Wave {2} specifies a Wave Repeat Bonus with no World Variable selected. Please select one.",
                        this.trans.name,
                        (wave.SpawnLevelNumber + 1),
                        (wave.SpawnWaveNumber + 1)));
                    spawnerValid = false;
                } else if (!WorldVariableTracker.VariableExistsInScene(beatMod._statName)) {
                    LevelSettings.LogIfNew(string.Format("Spawner '{0}', Level {1} Wave {2} specifies a Wave Repeat Bonus of World Variable '{3}', which doesn't exist in the scene.",
                        this.trans.name,
                        (wave.SpawnLevelNumber + 1),
                        (wave.SpawnWaveNumber + 1),
                        beatMod._statName));
                    spawnerValid = false;
                } else {
                    switch (beatMod._varTypeToUse) {
                        case WorldVariableTracker.VariableType._integer:
                            if (beatMod._modValueIntAmt.variableSource == LevelSettings.VariableSource.Variable) {
                                if (!WorldVariableTracker.VariableExistsInScene(beatMod._modValueIntAmt.worldVariableName)) {
                                    if (LevelSettings.illegalVariableNames.Contains(beatMod._modValueIntAmt.worldVariableName)) {
                                        LevelSettings.LogIfNew(string.Format("Spawner '{0}', Level {1} Wave {2} wants to award Wave Repeat Bonus if World Variable '{3}' is above the value of an unspecified World Variable. Please select one.",
                                            this.trans.name,
                                            (wave.SpawnLevelNumber + 1),
                                            (wave.SpawnWaveNumber + 1),
                                            beatMod._statName));
                                    } else {
                                        LevelSettings.LogIfNew(string.Format("Spawner '{0}', Level {1} Wave {2} wants to award Wave Repeat Bonus if World Variable '{3}' is above the value of World Variable '{4}', but the latter is not in the Scene.",
                                            this.trans.name,
                                            (wave.SpawnLevelNumber + 1),
                                            (wave.SpawnWaveNumber + 1),
                                            beatMod._statName,
                                            beatMod._modValueIntAmt.worldVariableName));
                                    }
                                    spawnerValid = false;
                                }
                            }

                            break;
                        case WorldVariableTracker.VariableType._float:
                            if (beatMod._modValueFloatAmt.variableSource == LevelSettings.VariableSource.Variable) {
                                if (!WorldVariableTracker.VariableExistsInScene(beatMod._modValueFloatAmt.worldVariableName)) {
                                    if (LevelSettings.illegalVariableNames.Contains(beatMod._modValueFloatAmt.worldVariableName)) {
                                        LevelSettings.LogIfNew(string.Format("Spawner '{0}', Level {1} Wave {2} wants to award Wave Repeat Bonus if World Variable '{3}' is above the value of an unspecified World Variable. Please select one.",
                                            this.trans.name,
                                            (wave.SpawnLevelNumber + 1),
                                            (wave.SpawnWaveNumber + 1),
                                            beatMod._statName));
                                    } else {
                                        LevelSettings.LogIfNew(string.Format("Spawner '{0}', Level {1} Wave {2} wants to award Wave Repeat Bonus if World Variable '{3}' is above the value of World Variable '{4}', but the latter is not in the Scene.",
                                            this.trans.name,
                                            (wave.SpawnLevelNumber + 1),
                                            (wave.SpawnWaveNumber + 1),
                                            beatMod._statName,
                                            beatMod._modValueFloatAmt.worldVariableName));
                                    }
                                    spawnerValid = false;
                                }
                            }

                            break;
                        default:
                            LevelSettings.LogIfNew("Add code for varType: " + beatMod._varTypeToUse.ToString());
                            break;
                    }
                }
            }

        }
    }

    private void CheckForValidVariablesForWave(WaveSpecifics wave) {
        // examine all KillerInts
        wave.MinToSpwn.LogIfInvalid(this.trans, "Min To Spawn", wave.SpawnLevelNumber, wave.SpawnWaveNumber);
        wave.MaxToSpwn.LogIfInvalid(this.trans, "Max To Spawn", wave.SpawnLevelNumber, wave.SpawnWaveNumber);
        wave.repeatItemInc.LogIfInvalid(this.trans, "Spawn Increase", wave.SpawnLevelNumber, wave.SpawnWaveNumber);
        wave.repeatItemLmt.LogIfInvalid(this.trans, "Spawn Limit", wave.SpawnLevelNumber, wave.SpawnWaveNumber);
        wave.repetitionsToDo.LogIfInvalid(this.trans, "Repetitions", wave.SpawnLevelNumber, wave.SpawnWaveNumber);

        if (wave.positionXmode == WaveSpecifics.PositionMode.CustomPosition) {
            wave.customPosX.LogIfInvalid(this.trans, "Custom X Position", wave.SpawnLevelNumber, wave.SpawnWaveNumber);
        }
        if (wave.positionYmode == WaveSpecifics.PositionMode.CustomPosition) {
            wave.customPosY.LogIfInvalid(this.trans, "Custom Y Position", wave.SpawnLevelNumber, wave.SpawnWaveNumber);
        }
        if (wave.positionZmode == WaveSpecifics.PositionMode.CustomPosition) {
            wave.customPosZ.LogIfInvalid(this.trans, "Custom Z Position", wave.SpawnLevelNumber, wave.SpawnWaveNumber);
        }

        // examine all KillerFloats
        wave.WaveDelaySec.LogIfInvalid(this.trans, "Delay Wave (sec)", wave.SpawnLevelNumber, wave.SpawnWaveNumber);
        wave.TimeToSpawnEntireWave.LogIfInvalid(this.trans, "Time To Spawn All", wave.SpawnLevelNumber, wave.SpawnWaveNumber);
        wave.repeatPauseMinimum.LogIfInvalid(this.trans, "Repeat Pause Min", wave.SpawnLevelNumber, wave.SpawnWaveNumber);
        wave.repeatPauseMaximum.LogIfInvalid(this.trans, "Repeat Pause Max", wave.SpawnLevelNumber, wave.SpawnWaveNumber);
        wave.repeatTimeInc.LogIfInvalid(this.trans, "Repeat Time Increase", wave.SpawnLevelNumber, wave.SpawnWaveNumber);
        wave.repeatItemLmt.LogIfInvalid(this.trans, "Repeat Time Limit", wave.SpawnLevelNumber, wave.SpawnWaveNumber);
        wave.doNotSpawnIfMbrCloserThan.LogIfInvalid(this.trans, "Spawn Limit Min. Distance", wave.SpawnLevelNumber, wave.SpawnWaveNumber);
        wave.doNotSpawnRandomDist.LogIfInvalid(this.trans, "Spawn Limit Random Distance", wave.SpawnLevelNumber, wave.SpawnWaveNumber);
        wave.randomDistX.LogIfInvalid(this.trans, "Rand. Distance X", wave.SpawnLevelNumber, wave.SpawnWaveNumber);
        wave.randomDistY.LogIfInvalid(this.trans, "Rand. Distance Y", wave.SpawnLevelNumber, wave.SpawnWaveNumber);
        wave.randomDistZ.LogIfInvalid(this.trans, "Rand. Distance Z", wave.SpawnLevelNumber, wave.SpawnWaveNumber);
        wave.randomXRotMin.LogIfInvalid(this.trans, "Rand. X Rot. Min", wave.SpawnLevelNumber, wave.SpawnWaveNumber);
        wave.randomXRotMax.LogIfInvalid(this.trans, "Rand. X Rot. Max", wave.SpawnLevelNumber, wave.SpawnWaveNumber);
        wave.randomYRotMin.LogIfInvalid(this.trans, "Rand. Y Rot. Min", wave.SpawnLevelNumber, wave.SpawnWaveNumber);
        wave.randomYRotMax.LogIfInvalid(this.trans, "Rand. Y Rot. Max", wave.SpawnLevelNumber, wave.SpawnWaveNumber);
        wave.randomYRotMin.LogIfInvalid(this.trans, "Rand. Z Rot. Min", wave.SpawnLevelNumber, wave.SpawnWaveNumber);
        wave.randomYRotMax.LogIfInvalid(this.trans, "Rand. Z Rot. Max", wave.SpawnLevelNumber, wave.SpawnWaveNumber);
        wave.incrementPositionX.LogIfInvalid(this.trans, "Incremental Distance X", wave.SpawnLevelNumber, wave.SpawnWaveNumber);
        wave.incrementPositionY.LogIfInvalid(this.trans, "Incremental Distance Y", wave.SpawnLevelNumber, wave.SpawnWaveNumber);
        wave.incrementPositionZ.LogIfInvalid(this.trans, "Incremental Distance Z", wave.SpawnLevelNumber, wave.SpawnWaveNumber);
        wave.incrementRotX.LogIfInvalid(this.trans, "Incremental Rotation X", wave.SpawnLevelNumber, wave.SpawnWaveNumber);
        wave.incrementRotY.LogIfInvalid(this.trans, "Incremental Rotation Y", wave.SpawnLevelNumber, wave.SpawnWaveNumber);
        wave.incrementRotZ.LogIfInvalid(this.trans, "Incremental Rotation Z", wave.SpawnLevelNumber, wave.SpawnWaveNumber);
        wave.postSpawnNudgeFwd.LogIfInvalid(this.trans, "Nudge Forward", wave.SpawnLevelNumber, wave.SpawnWaveNumber);
        wave.postSpawnNudgeDwn.LogIfInvalid(this.trans, "Nudge Down", wave.SpawnLevelNumber, wave.SpawnWaveNumber);
        wave.postSpawnNudgeRgt.LogIfInvalid(this.trans, "Nudge Right", wave.SpawnLevelNumber, wave.SpawnWaveNumber);

        if (wave.curWaveRepeatMode == WaveSpecifics.RepeatWaveMode.UntilWorldVariableAbove || wave.curWaveRepeatMode == WaveSpecifics.RepeatWaveMode.UntilWorldVariableBelow) {
            for (var i = 0; i < wave.repeatPassCriteria.statMods.Count; i++) {
                var crit = wave.repeatPassCriteria.statMods[i];

                if (WorldVariableTracker.IsBlankVariableName(crit._statName)) {
                    LevelSettings.LogIfNew(string.Format("Spawner '{0}', Level {1} Wave {2} has a Repeat Item Limit with no World Variable selected. Please select one.",
                        this.trans.name,
                        wave.SpawnLevelNumber + 1,
                        wave.SpawnWaveNumber + 1));
                    spawnerValid = false;
                } else if (!WorldVariableTracker.VariableExistsInScene(crit._statName)) {
                    LevelSettings.LogIfNew(string.Format("Spawner '{0}', Level {1} Wave {2} has a Repeat Item Limit using World Variable '{3}', which doesn't exist in the scene.",
                        this.trans.name,
                        wave.SpawnLevelNumber + 1,
                        wave.SpawnWaveNumber + 1,
                        crit._statName));
                    spawnerValid = false;
                } else {
                    switch (crit._varTypeToUse) {
                        case WorldVariableTracker.VariableType._integer:
                            if (crit._modValueIntAmt.variableSource == LevelSettings.VariableSource.Variable) {
                                if (!WorldVariableTracker.VariableExistsInScene(crit._modValueIntAmt.worldVariableName)) {
                                    if (LevelSettings.illegalVariableNames.Contains(crit._modValueIntAmt.worldVariableName)) {
                                        LevelSettings.LogIfNew(string.Format("Spawner '{0}', Level {1} Wave {2} has a Repeat Item Limit criteria with no World Variable selected. Please select one.",
                                            this.trans.name,
                                            wave.SpawnLevelNumber + 1,
                                            wave.SpawnWaveNumber + 1));
                                    } else {
                                        LevelSettings.LogIfNew(string.Format("Spawner '{0}', Level {1} Wave {2} has a Repeat Item Limit using the value of World Variable '{3}', which doesn't exist in the Scene.",
                                            this.trans.name,
                                            wave.SpawnLevelNumber + 1,
                                            wave.SpawnWaveNumber + 1,
                                            crit._modValueIntAmt.worldVariableName));
                                    }
                                    spawnerValid = false;
                                }
                            }

                            break;
                        case WorldVariableTracker.VariableType._float:
                            if (crit._modValueIntAmt.variableSource == LevelSettings.VariableSource.Variable) {
                                if (!WorldVariableTracker.VariableExistsInScene(crit._modValueFloatAmt.worldVariableName)) {
                                    if (LevelSettings.illegalVariableNames.Contains(crit._modValueFloatAmt.worldVariableName)) {
                                        LevelSettings.LogIfNew(string.Format("Spawner '{0}', Level {1} Wave {2} has a Repeat Item Limit criteria with no World Variable selected. Please select one.",
                                            this.trans.name,
                                            wave.SpawnLevelNumber + 1,
                                            wave.SpawnWaveNumber + 1));
                                    } else {
                                        LevelSettings.LogIfNew(string.Format("Spawner '{0}', Level {1} Wave {2} has a Repeat Item Limit using the value of World Variable '{3}', which doesn't exist in the Scene.",
                                            this.trans.name,
                                            wave.SpawnLevelNumber + 1,
                                            wave.SpawnWaveNumber + 1,
                                            crit._modValueFloatAmt.worldVariableName));
                                    }
                                    spawnerValid = false;
                                }
                            }

                            break;
                        default:
                            LevelSettings.LogIfNew("Add code for varType: " + crit._varTypeToUse.ToString());
                            break;
                    }
                }
            }
        }
    }

    protected virtual Transform GetSpawnable(WaveSpecifics wave) {
        if (wave == null) {
            return null;
        }

        switch (wave.spawnSource) {
            case WaveSpecifics.SpawnOrigin.Specific:
                return wave.prefabToSpawn;
            case WaveSpecifics.SpawnOrigin.PrefabPool:
                return wavePool.GetRandomWeightedTransform();
        }

        return null;
    }

    public void RemoveSpawnedMember(Transform transMember) {
        if (this.spawnedWaveMembers.Count == 0) {
            return;
        }
		
        if (this.spawnedWaveMembers.Contains(transMember)) {
            this.spawnedWaveMembers.Remove(transMember);
            LevelSettings.RemoveWaveSpawnedItem(transMember);
            itemsToCompleteWave--;
        }

        this.StartNextEliminationWave();
    }

    private void StartNextEliminationWave() {
        if (GameIsOverForSpawner || this.currentWave == null || !this.spawnerValid || !this.waveFinishedSpawning || !currentWave.IsValid) {
            return;
        }

        var hasNoneLeft = this.spawnedWaveMembers.Count == 0;
        if (LevelSettings.PreviousWaveInfo.waveType == LevelSettings.WaveType.Elimination && currentWave.waveCompletePercentage < 100) {
            if (this.itemsToCompleteWave <= 0) { // end wave early
                hasNoneLeft = true;
            }
        }

        if (!hasNoneLeft) {
            return;
        }

        if (LevelSettings.PreviousWaveInfo.waveType == LevelSettings.WaveType.Elimination &&
            ((currentWave.curWaveRepeatMode == WaveSpecifics.RepeatWaveMode.NumberOfRepetitions && waveRepetitionNumber + 1 >= currentWave.repetitionsToDo.Value) || !currentWave.repeatWaveUntilNew)) {

            if (!levelSettingsNotifiedOfCompletion) {
                levelSettingsNotifiedOfCompletion = true;

                if (listener != null) {
                    listener.EliminationWaveCompleted(this.currentWave);
                }

                LevelSettings.EliminationSpawnerCompleted(instanceId);
            }
        } else if (currentWave.repeatWaveUntilNew) {
            if (!this.repeatTimer.HasValue) {
                if (listener != null) {
                    listener.EliminationWaveCompleted(this.currentWave);
                }

                this.repeatTimer = Time.time;
                this.repeatWaitTime = UnityEngine.Random.Range(this.currentWave.repeatPauseMinimum.Value, this.currentWave.repeatPauseMaximum.Value);
            } else if (Time.time - this.repeatTimer.Value > this.repeatWaitTime) {
                waveRepetitionNumber++;

                MaybeRepeatWave();
            }
        } else if (!currentWave.repeatWaveUntilNew) {
            LevelSettings.EliminationSpawnerCompleted(instanceId);
		}
    }

    private void MaybeRepeatWave() {
        var allPassed = true;

        if (LevelSettings.PreviousWaveInfo.waveType == LevelSettings.WaveType.Elimination) {
            switch (this.currentWave.curWaveRepeatMode) {
                case WaveSpecifics.RepeatWaveMode.NumberOfRepetitions:
                case WaveSpecifics.RepeatWaveMode.Endless:
                    WaveRepeat();
                    break;
                case WaveSpecifics.RepeatWaveMode.UntilWorldVariableAbove:
                    for (var i = 0; i < this.currentWave.repeatPassCriteria.statMods.Count; i++) {
                        var stat = this.currentWave.repeatPassCriteria.statMods[i];

                        if (!WorldVariableTracker.VariableExistsInScene(stat._statName)) {
                            LevelSettings.LogIfNew(string.Format("Spawner '{0}' wants to repeat until World Variable '{1}' is a certain value, but that Variable is not in the Scene.",
                                this.trans.name,
                                stat._statName));
                            continue;
                        }

                        var variable = WorldVariableTracker.GetWorldVariable(stat._statName);
                        if (variable == null) {
                            continue;
                        }
                        var varVal = stat._varTypeToUse == WorldVariableTracker.VariableType._integer ? variable.CurrentIntValue : variable.CurrentFloatValue;
                        var compareVal = stat._varTypeToUse == WorldVariableTracker.VariableType._integer ? stat._modValueIntAmt.Value : stat._modValueFloatAmt.Value;

                        if (varVal >= compareVal) {
                            continue;
                        }

                        allPassed = false;
                        break;
                    }

                    if (!allPassed) {
                        WaveRepeat();
                    } else {
                        LevelSettings.EliminationSpawnerCompleted(instanceId); // since this never happens above due to infinite repetitions
                    }
                    break;
                case WaveSpecifics.RepeatWaveMode.UntilWorldVariableBelow:
                    for (var i = 0; i < this.currentWave.repeatPassCriteria.statMods.Count; i++) {
                        var stat = this.currentWave.repeatPassCriteria.statMods[i];

                        if (!WorldVariableTracker.VariableExistsInScene(stat._statName)) {
                            LevelSettings.LogIfNew(string.Format("Spawner '{0}' wants to repeat until World Variable '{1}' is a certain value, but that Variable is not in the Scene.",
                                this.trans.name,
                                stat._statName));
                            continue;
                        }

                        var variable = WorldVariableTracker.GetWorldVariable(stat._statName);
                        if (variable == null) {
                            continue;
                        }
                        var varVal = stat._varTypeToUse == WorldVariableTracker.VariableType._integer ? variable.CurrentIntValue : variable.CurrentFloatValue;
                        var compareVal = stat._varTypeToUse == WorldVariableTracker.VariableType._integer ? stat._modValueIntAmt.Value : stat._modValueFloatAmt.Value;

                        if (varVal <= compareVal) {
                            continue;
                        }

                        allPassed = false;
                        break;
                    }

                    if (!allPassed) {
                        WaveRepeat();
                    } else {
                        LevelSettings.EliminationSpawnerCompleted(instanceId); // since this never happens above due to infinite repetitions
                    }
                    break;
            }
        } else if (LevelSettings.PreviousWaveInfo.waveType == LevelSettings.WaveType.Timed) {
            switch (this.currentWave.curTimedRepeatWaveMode) {
                case WaveSpecifics.TimedRepeatWaveMode.EliminationStyle:
                    WaveRepeat();
                    break;
                case WaveSpecifics.TimedRepeatWaveMode.StrictTimeStyle:
                    WaveRepeat();
                    break;
            }
        }
    }
}