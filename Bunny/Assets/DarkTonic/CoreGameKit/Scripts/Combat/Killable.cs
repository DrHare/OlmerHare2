using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This class is used to set up Killable, used for combat objects with attack points and hit points. Also can be used for pickups such as coins and health packs.
/// </summary>
[AddComponentMenu("Dark Tonic/Core GameKit/Combat/Killable")]
public class Killable : MonoBehaviour {
    public const string DESTROYED_TEXT = "Destroyed";
    public const float COROUTINE_INTERVAL = .2f;
    public const int MAX_HIT_POINTS = 100000;
    public const int MAX_ATTACK_POINTS = 100000;
    public const int MIN_ATTACK_POINTS = -100000;

    #region Members
    public TriggeredSpawner.GameOverBehavior gameOverBehavior = TriggeredSpawner.GameOverBehavior.Disable;
    public bool syncHitPointWorldVariable = false;
    public KillerInt hitPoints = new KillerInt(1, 1, MAX_HIT_POINTS);
    public KillerInt maxHitPoints = new KillerInt(MAX_ATTACK_POINTS, MIN_ATTACK_POINTS, MAX_ATTACK_POINTS);
    public KillerInt atckPoints = new KillerInt(1, MIN_ATTACK_POINTS, MAX_ATTACK_POINTS);
    public Transform ExplosionPrefab;
    public KillableListener listener;

    public bool invincibilityExpanded = false;
    public bool isInvincible = false;
    public bool invincibleWhileChildrenKillablesExist = true;
    public bool disableCollidersWhileChildrenKillablesExist = false;

    public bool invincibleOnSpawn = false;
    public KillerFloat invincibleTimeSpawn = new KillerFloat(2f, 0f, float.MaxValue);

    public bool enableLogging = false;
    public bool filtersExpanded = true;

    public bool ignoreKillablesSpawnedByMe = true;
    public bool useLayerFilter = false;
    public bool useTagFilter = false;
    public bool showVisibilitySettings = true;
    public bool despawnWhenOffscreen = false;
    public bool despawnOnClick = false;
    public bool despawnOnMouseClick = false;
    public bool despawnIfNotVisible = false;
    public KillerFloat despawnIfNotVisibleForSec = new KillerFloat(5f, .1f, float.MaxValue);
    public bool ignoreOffscreenHits = true;
    public List<string> matchingTags = new List<string>() { "Untagged" };
    public List<int> matchingLayers = new List<int>() { 0 };
    public DespawnMode despawnMode = DespawnMode.ZeroHitPoints;

    // death player stat mods
    public bool despawnStatModifiersExpanded = false;
    public WorldVariableCollection playerStatDespawnModifiers = new WorldVariableCollection();
    public List<WorldVariableCollection> alternateModifiers = new List<WorldVariableCollection>();

    // damage prefab settings
    public bool damagePrefabExpanded = false;
    public SpawnSource damagePrefabSource = SpawnSource.None;
    public int damagePrefabPoolIndex = 0;
    public string damagePrefabPoolName = null;
    public Transform damagePrefabSpecific;
    public DamagePrefabSpawnMode damagePrefabSpawnMode = DamagePrefabSpawnMode.None;
    public KillerInt damagePrefabSpawnQuantity = new KillerInt(1, 1, 100);
    public KillerInt damageGroupsize = new KillerInt(1, 1, 500);
    public Vector3 damagePrefabOffset = Vector3.zero;
    public bool damagePrefabRandomizeXRotation = false;
    public bool damagePrefabRandomizeYRotation = false;
    public bool damagePrefabRandomizeZRotation = false;
    public bool despawnStatDamageModifiersExpanded = false;
    public WorldVariableCollection playerStatDamageModifiers = new WorldVariableCollection();

    // death prefab settings
    public WaveSpecifics.SpawnOrigin deathPrefabSource = WaveSpecifics.SpawnOrigin.Specific;
    public int deathPrefabPoolIndex = 0;
    public string deathPrefabPoolName = null;
    public bool deathPrefabSettingsExpanded = false;
    public Transform deathPrefabSpecific;
    public bool deathPrefabKeepSameParent = true;
    public KillerInt deathPrefabSpawnPercent = new KillerInt(100, 0, 100);
    public KillerInt deathPrefabQty = new KillerInt(1, 0, 100);
    public Vector3 deathPrefabOffset = Vector3.zero;
    public RotationMode rotationMode = RotationMode.UseDeathPrefabRotation;
    public bool deathPrefabKeepVelocity = true;
    public Vector3 deathPrefabCustomRotation = Vector3.zero;
    public KillerFloat deathDelay = new KillerFloat(0, 0, 100);

    // retrigger limit settings
    public TriggeredSpawner.RetriggerLimitMode retriggerLimitMode = TriggeredSpawner.RetriggerLimitMode.None;
    public KillerInt limitPerXFrame = new KillerInt(1, 1, int.MaxValue);
    public KillerFloat limitPerSeconds = new KillerFloat(0.1f, .1f, float.MaxValue);
    public SpawnerDestroyedBehavior spawnerDestroyedAction = SpawnerDestroyedBehavior.DoNothing;

    public DeathDespawnBehavior deathDespawnBehavior = DeathDespawnBehavior.ReturnToPool;
    public bool timerDeathEnabled = false;
    public KillerFloat timerDeathSeconds = new KillerFloat(1f, 0.1f, float.MaxValue);
    public SpawnerDestroyedBehavior timeUpAction = SpawnerDestroyedBehavior.Die;

    public int currentHitPoints;

    public bool isVisible;

    public bool showRespawnSettings = false;
    public RespawnType respawnType = RespawnType.None;
    public int timesToRespawn = 1;
    public int timesRespawned = 0;
    public KillerFloat respawnDelay = new KillerFloat(0, 0, 100);
    private Vector3 respawnLocation = Vector3.zero;

    private GameObject spawnedFromObject = null;
    private int? spawnedFromGOInstanceId = null;
    private WavePrefabPool deathPrefabWavePool = null;
    private Transform _trans;
    private GameObject go;
    private int? instanceId = null;
    private CharacterController charCtrl;
    private Rigidbody _body;
    private Killable parentKillable;
    private Collider _collider;

#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2
    // not supported
#else
	private Rigidbody2D _body2d;
	private Collider2D _collider2d;
#endif

    private int damageTaken = 0;
    private int damagePrefabsSpawned = 0;
    private WavePrefabPool damagePrefabWavePool = null;
    private int triggeredLastFrame = -200;
    private float triggeredLastTime = -100f;
    private bool becameVisible = false;
    private float spawnTime;
    public bool isDespawning = false;
    private bool isTemporarilyInvincible = false;
    private bool spawnerSet = false;
    private YieldInstruction loopDelay = new WaitForSeconds(COROUTINE_INTERVAL);
    private bool spawnLocationSet = false;
    private bool waitingToDestroy = false;
    public List<Killable> childKillables = new List<Killable>();
    #endregion

    #region enums
    public enum DeathDespawnBehavior {
        ReturnToPool,
        Disable
    }

    public enum RespawnType {
        None = 0,
        Infinite = 1,
        SetNumber = 2
    }

    public enum SpawnerDestroyedBehavior {
        DoNothing,
        Despawn,
        Die
    }

    public enum SpawnSource {
        None,
        Specific,
        PrefabPool
    }

    public enum DamagePrefabSpawnMode {
        None,
        PerHit,
        PerHitPointLost,
        PerGroupHitPointsLost
    }

    public enum RotationMode {
        CustomRotation,
        InheritExistingRotation,
        UseDeathPrefabRotation
    }

    public enum DespawnMode {
        None = -1,
        ZeroHitPoints = 0,
        LostAnyHitPoints = 1,
        CollisionOrTrigger = 2
    }
    #endregion

    #region MonoBehavior events and associated virtuals
    void Awake() {
        this.charCtrl = this.GetComponent<CharacterController>();

        ResetSpawnerInfo();
    }

    void Start() {
        SpawnedOrAwake(false);
    }

    void OnSpawned() { // used by Core GameKit Pooling & also Pool Manager Pooling!
        SpawnedOrAwake(true);
    }

    void OnDespawned() {
        this.spawnerSet = false;

        // reset velocity
        ResetVelocity();
        ResetSpawnerInfo();

        // add code here to fire when despawned
        Despawned();
    }

    /// <summary>
    /// This method is automatically called just before the Killable is Despawned
    /// </summary>
    protected virtual void Despawned() {
        // add code to subclass if needing functionality here		
    }

    void OnClick() {
        _OnClick();
    }

    protected virtual void _OnClick() {
        if (this.despawnOnClick) {
            this.DestroyKillable();
        }
    }

    void OnMouseDown() {
        _OnMouseDown();
    }

    protected virtual void _OnMouseDown() {
        if (this.despawnOnMouseClick) {
            this.DestroyKillable();
        }
    }

    void OnBecameVisible() {
        BecameVisible();
    }

    public virtual void BecameVisible() {
        if (this.isVisible) {
            return; // to fix Unity bug.
        }

        this.isVisible = true;
        this.becameVisible = true;
    }

    void OnBecameInvisible() {
        BecameInvisible();
    }

    public virtual void BecameInvisible() {
        this.isVisible = false;

        if (despawnWhenOffscreen) {
            this.Despawn(TriggeredSpawner.EventType.Invisible);
        }
    }

#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2
    // not supported
#else
	void OnCollisionEnter2D(Collision2D coll) {
		CollisionEnter2D(coll);
	}
	
	public virtual void CollisionEnter2D(Collision2D collision) {
		var othGo = collision.gameObject;
		
		if (!IsValidHit(othGo.layer, othGo.tag)) {
			return;
		}
		
		var enemy = GetOtherKillable(othGo);
		
		CheckForAttackPoints(enemy, othGo);
	}
	
	void OnTriggerEnter2D(Collider2D other) {
		TriggerEnter2D(other);
	}
	
	public virtual void TriggerEnter2D(Collider2D other) {
		var othGo = other.gameObject;
		
		if (!IsValidHit(othGo.layer, othGo.tag)) {
			return;
		}
		
		var enemy = GetOtherKillable(othGo);
		
		CheckForAttackPoints(enemy, othGo);
	}
#endif

    void OnCollisionEnter(Collision collision) {
        CollisionEnter(collision);
    }

    public virtual void CollisionEnter(Collision collision) {
        var othGo = collision.gameObject;

        if (!IsValidHit(othGo.layer, othGo.tag)) {
            return;
        }

        var enemy = GetOtherKillable(othGo);

        CheckForAttackPoints(enemy, othGo);
    }

    void OnTriggerEnter(Collider other) {
        TriggerEnter(other);
    }

    public virtual void TriggerEnter(Collider other) {
        if (!IsValidHit(other.gameObject.layer, other.gameObject.tag)) {
            return;
        }

        var enemy = GetOtherKillable(other.gameObject);

        CheckForAttackPoints(enemy, other.gameObject);
    }

    void OnControllerColliderHit(ControllerColliderHit hit) {
        ControllerColliderHit(hit.gameObject);
    }

    public virtual void ControllerColliderHit(GameObject hit, bool calledFromOtherKillable = false) {
        if (calledFromOtherKillable && charCtrl != null) {
            // we don't need to be called from a Char Controller if we are one. Abort to exit potential endless loop.
            return;
        }

        var enemy = GetOtherKillable(hit);

        if (enemy != null && !calledFromOtherKillable) {
            // for Character Controllers, the hit object will not register a hit, so we call it manually.
            enemy.ControllerColliderHit(GameObj, true);
        }

        if (!IsValidHit(hit.layer, hit.tag)) {
            return;
        }

        CheckForAttackPoints(enemy, hit);
    }

    #endregion

    #region Public Methods
    /// <summary>
    /// Call this method to make your Killable invincible for X seconds.
    /// </summary>
    /// <param name="seconds">Number of seconds to make your Killable invincible.</param>
    public void TemporaryInvincibility(float seconds) {
        if (isTemporarilyInvincible) {
            // already invincible.
            return;
        }
        StartCoroutine(SetSpawnInvincibleForSeconds(seconds));
    }

    /// <summary>
    /// Call this method to add attack points to the Killable.
    /// </summary>
    /// <param name="pointsToAdd">The number of attack points to add.</param>
    public void AddAttackPoints(int pointsToAdd) {
        atckPoints.Value += pointsToAdd;
        if (atckPoints.Value < 0) {
            atckPoints.Value = 0;
        }
    }

    /// <summary>
    /// Call this method to add hit points to the Killable.
    /// </summary>
    /// <param name="pointsToAdd">The number of hit points to add to "current hit points".</param>
    public void AddHitPoints(int pointsToAdd) {
        hitPoints.Value += pointsToAdd;
        if (hitPoints.Value < 0) {
            hitPoints.Value = 0;
        }

        currentHitPoints += pointsToAdd;
        if (currentHitPoints < 0) {
            currentHitPoints = 0;
        }
    }

    public bool IsUsingPrefabPool(Transform poolTrans) {
        var poolName = poolTrans.name;

        if (damagePrefabSource == SpawnSource.PrefabPool && damagePrefabPoolName == poolName) {
            return true;
        }

        return false;
    }

    public void RecordSpawner(GameObject spawnerObject) {
        spawnedFromObject = spawnerObject;
        spawnerSet = true;
    }

    #endregion

    #region Helper Methods
    private IEnumerator SetSpawnInvincibleForSeconds(float seconds) {
        isTemporarilyInvincible = true;
        isInvincible = true;

        yield return new WaitForSeconds(seconds);

        if (!isTemporarilyInvincible) {
            yield break;
        }

        isInvincible = false;
        isTemporarilyInvincible = false;
    }

    private void CheckForValidVariables() {
        // examine all KillerInts
        hitPoints.LogIfInvalid(this.Trans, "Killable Start Hit Points");
        maxHitPoints.LogIfInvalid(this.Trans, "Killable Max Hit Points");
        atckPoints.LogIfInvalid(this.Trans, "Killable Start Attack Points");
        damagePrefabSpawnQuantity.LogIfInvalid(this.Trans, "Killable Damage Prefab Spawn Quantity");
        damageGroupsize.LogIfInvalid(this.Trans, "Killable Group H.P. Amount");
        deathPrefabSpawnPercent.LogIfInvalid(this.Trans, "Killable Spawn % Chance");
        deathPrefabQty.LogIfInvalid(this.Trans, "Killable Death Prefab Spawn Quantity");
        limitPerXFrame.LogIfInvalid(this.Trans, "Killable Min Frames Between");
        deathDelay.LogIfInvalid(this.Trans, "Killable Death Delay");
        respawnDelay.LogIfInvalid(this.Trans, "Killable Respawn Delay");

        // examine all KillerFloats
        despawnIfNotVisibleForSec.LogIfInvalid(this.Trans, "Killable Not Visible Max Time");
        limitPerSeconds.LogIfInvalid(this.Trans, "Killable Min Seconds Between");
        if (timerDeathEnabled) {
            timerDeathSeconds.LogIfInvalid(this.Trans, "Killable Timer Death Seconds");
        }

        if (invincibleOnSpawn) {
            invincibleTimeSpawn.LogIfInvalid(this.Trans, "Killable Invincibility Time (sec)");
        }

        // check damage mod scenarios  
        for (var i = 0; i < playerStatDamageModifiers.statMods.Count; i++) {
            var mod = playerStatDamageModifiers.statMods[i];
            ValidateWorldVariableModifier(mod);
        }

        // check mod scenarios
        for (var i = 0; i < playerStatDespawnModifiers.statMods.Count; i++) {
            var mod = playerStatDespawnModifiers.statMods[i];
            ValidateWorldVariableModifier(mod);
        }

        for (var c = 0; c < alternateModifiers.Count; c++) {
            var alt = alternateModifiers[c];
            for (var i = 0; i < alt.statMods.Count; i++) {
                var mod = alt.statMods[i];
                ValidateWorldVariableModifier(mod);
            }
        }
    }

    private void ValidateWorldVariableModifier(WorldVariableModifier mod) {
        if (WorldVariableTracker.IsBlankVariableName(mod._statName)) {
            LevelSettings.LogIfNew(string.Format("Killable '{0}' specifies a World Variable Modifier with no World Variable name. Please delete and re-add.",
                                                 this.Trans.name));
        } else if (!WorldVariableTracker.VariableExistsInScene(mod._statName)) {
            LevelSettings.LogIfNew(string.Format("Killable '{0}' specifies a World Variable Modifier with World Variable '{1}', which doesn't exist in the scene.",
                                                 this.Trans.name,
                                                 mod._statName));
        } else {
            switch (mod._varTypeToUse) {
                case WorldVariableTracker.VariableType._integer:
                    if (mod._modValueIntAmt.variableSource == LevelSettings.VariableSource.Variable) {
                        if (!WorldVariableTracker.VariableExistsInScene(mod._modValueIntAmt.worldVariableName)) {
                            if (LevelSettings.illegalVariableNames.Contains(mod._modValueIntAmt.worldVariableName)) {
                                LevelSettings.LogIfNew(string.Format("Killable '{0}' wants to modify World Variable '{1}' using the value of an unspecified World Variable. Please specify one.",
                                                                     this.Trans.name,
                                                                     mod._statName));
                            } else {
                                LevelSettings.LogIfNew(string.Format("Killable '{0}' wants to modify World Variable '{1}' using the value of World Variable '{2}', but the latter is not in the Scene.",
                                                                     this.Trans.name,
                                                                     mod._statName,
                                                                     mod._modValueIntAmt.worldVariableName));
                            }
                        }
                    }

                    break;
                case WorldVariableTracker.VariableType._float:

                    break;
                default:
                    LevelSettings.LogIfNew("Add code for varType: " + mod._varTypeToUse.ToString());
                    break;
            }
        }
    }

    private Killable GetOtherKillable(GameObject other) {
        var enemy = other.GetComponent<Killable>();
        if (enemy == null) {
            var childKill = other.GetComponent<KillableChildCollision>();
            if (childKill != null) {
                enemy = childKill.killable;
            }
        }

        return enemy;
    }

    private void CheckForAttackPoints(Killable enemy, GameObject goHit) {
        if (enemy == null) {
            LogIfEnabled("Not taking any damage because you've collided with non-Killable object '" + goHit.name + "'.");
            return;
        }

        if (this.ignoreKillablesSpawnedByMe) {
            if (enemy.SpawnedFromObjectId == this.KillableId) {
                LogIfEnabled("Not taking any damage because you've collided with a Killable named '" + goHit.name + "' spawned by this Killable.");
                return;
            }
        }

        this.TakeDamage(enemy.atckPoints.Value, enemy);
    }

    private bool GameIsOverForKillable {
        get {
            return LevelSettings.IsGameOver && this.gameOverBehavior == TriggeredSpawner.GameOverBehavior.Disable;
        }
    }

    private bool IsValidHit(int layer, string tag) {
        if (GameIsOverForKillable) {
            LogIfEnabled("Invalid hit because game is over for Killable. Modify Game Over Behavior to get around this.");
            return false;
        }

        // check filters for matches if turned on
        if (useLayerFilter && !matchingLayers.Contains(layer)) {
            LogIfEnabled("Invalid hit because layer of other object is not in the Layer Filter.");
            return false;
        }

        if (useTagFilter && !matchingTags.Contains(tag)) {
            LogIfEnabled("Invalid hit because tag of other object is not in the Tag Filter.");
            return false;
        }

        if (!this.isVisible && this.ignoreOffscreenHits) {
            LogIfEnabled("Invalid hit because Killable is set to ignore offscreen hits and is invisible or offscreen right now. Consider using the KillableChildVisibility script if the Renderer is in a child object.");
            return false;
        }

        switch (this.retriggerLimitMode) {
            case TriggeredSpawner.RetriggerLimitMode.FrameBased:
                if (Time.frameCount - this.triggeredLastFrame < this.limitPerXFrame.Value) {
                    LogIfEnabled("Invalid hit - has been limited by frame count. Not taking damage from current hit.");
                    return false;
                }
                break;
            case TriggeredSpawner.RetriggerLimitMode.TimeBased:
                if (Time.time - this.triggeredLastTime < this.limitPerSeconds.Value) {
                    LogIfEnabled("Invalid hit - has been limited by time since last hit. Not taking damage from current hit.");
                    return false;
                }
                break;
        }


        return true;
    }

    private bool SpawnDamagePrefabsIfPerHit(int damagePoints) {
        if (this.damagePrefabSpawnMode == DamagePrefabSpawnMode.PerHit) {
            SpawnDamagePrefabs(damagePoints);
            return true;
        }

        return false;
    }

    private void SpawnDamagePrefabs(int damagePoints) {
        var numberToSpawn = 0;

        switch (this.damagePrefabSpawnMode) {
            case DamagePrefabSpawnMode.None:
                return;
            case DamagePrefabSpawnMode.PerHit:
                numberToSpawn = 1;
                break;
            case DamagePrefabSpawnMode.PerHitPointLost:
                numberToSpawn = Math.Min(this.hitPoints.Value, damagePoints);
                break;
            case DamagePrefabSpawnMode.PerGroupHitPointsLost:
                damageTaken += damagePoints;
                var numberOfGroups = (int)Math.Floor(damageTaken / (float)damageGroupsize.Value);
                numberToSpawn = numberOfGroups - damagePrefabsSpawned;
                break;
        }

        if (numberToSpawn == 0) {
            return;
        }

        numberToSpawn *= damagePrefabSpawnQuantity.Value;

        var spawnPos = this.Trans.position + damagePrefabOffset;

        for (var i = 0; i < numberToSpawn; i++) {
            var prefabToSpawn = CurrentDamagePrefab;
            if (damagePrefabSource != SpawnSource.None && prefabToSpawn == null) {
                // empty element in Prefab Pool
                continue;
            }

            var spawnedDamagePrefab = PoolBoss.SpawnInPool(prefabToSpawn, spawnPos, Quaternion.identity);
            if (spawnedDamagePrefab == null) {
                if (this.listener != null) {
                    this.listener.DamagePrefabFailedToSpawn(prefabToSpawn);
                }
            } else {
                SpawnUtility.RecordSpawnerObjectIfKillable(spawnedDamagePrefab, GameObj);

                // affect the spawned object.
                Vector3 euler = prefabToSpawn.rotation.eulerAngles;

                if (this.damagePrefabRandomizeXRotation) {
                    euler.x = UnityEngine.Random.Range(0f, 360f);
                }
                if (this.damagePrefabRandomizeYRotation) {
                    euler.y = UnityEngine.Random.Range(0f, 360f);
                }
                if (this.damagePrefabRandomizeZRotation) {
                    euler.z = UnityEngine.Random.Range(0f, 360f);
                }

                spawnedDamagePrefab.rotation = Quaternion.Euler(euler);

                if (this.listener != null) {
                    this.listener.DamagePrefabSpawned(spawnedDamagePrefab);
                }
            }
        }

        // clean up
        damagePrefabsSpawned += numberToSpawn;
    }

    private void ModifyWorldVariables(WorldVariableCollection modCollection, bool isDamage) {
        if (modCollection.statMods.Count > 0 && this.listener != null) {
            if (isDamage) {
                this.listener.ModifyingDamageWorldVariables(modCollection.statMods);
            } else {
                this.listener.ModifyingDeathWorldVariables(modCollection.statMods);
            }
        }
        foreach (var modifier in modCollection.statMods) {
            WorldVariableTracker.ModifyPlayerStat(modifier, this.Trans);
        }
    }

    private void SpawnDeathPrefabs() {
        if (UnityEngine.Random.Range(0, 100) < this.deathPrefabSpawnPercent.Value) {
            for (var i = 0; i < this.deathPrefabQty.Value; i++) {
                Transform deathPre = CurrentDeathPrefab;

                if (deathPrefabSource == WaveSpecifics.SpawnOrigin.PrefabPool && deathPre == null) {
                    continue; // nothing to spawn
                }

                var spawnRotation = deathPre.transform.rotation;
                switch (this.rotationMode) {
                    case RotationMode.InheritExistingRotation:
                        spawnRotation = this.Trans.rotation;
                        break;
                    case RotationMode.CustomRotation:
                        spawnRotation = Quaternion.Euler(deathPrefabCustomRotation);
                        break;
                }

                var spawnPos = this.Trans.position;
                spawnPos += this.deathPrefabOffset;

                Transform theParent = deathPrefabKeepSameParent ? this.Trans.parent : null;
                var spawnedDeathPrefab = PoolBoss.Spawn(deathPre, spawnPos, spawnRotation, theParent);

                if (spawnedDeathPrefab != null) {
                    if (this.listener != null) {
                        this.listener.DeathPrefabSpawned(spawnedDeathPrefab);
                    }

                    SpawnUtility.RecordSpawnerObjectIfKillable(spawnedDeathPrefab, GameObj);

                    if (deathPrefabKeepVelocity) {
                        var spawnedBody = spawnedDeathPrefab.GetComponent<Rigidbody>();
                        if (spawnedBody != null && !spawnedBody.isKinematic && Body != null && !Body.isKinematic) {
                            spawnedBody.velocity = Body.velocity;
                        } else {
#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2
                            // not supported
#else
							var spawnedBody2D = spawnedDeathPrefab.GetComponent<Rigidbody2D>();
							if (spawnedBody2D != null && !spawnedBody2D.isKinematic && Body2D != null && !Body2D.isKinematic) {
								spawnedBody2D.velocity = Body2D.velocity;
                            }
#endif
                        }
                    }
                } else {
                    if (this.listener != null) {
                        this.listener.DeathPrefabFailedToSpawn(deathPre);
                    }
                }
            }
        }
    }

    private Transform CurrentDeathPrefab {
        get {
            switch (deathPrefabSource) {
                case WaveSpecifics.SpawnOrigin.Specific:
                    return deathPrefabSpecific;
                case WaveSpecifics.SpawnOrigin.PrefabPool:
                    return deathPrefabWavePool.GetRandomWeightedTransform();
            }

            return null;
        }
    }

    private Transform CurrentDamagePrefab {
        get {
            switch (damagePrefabSource) {
                case SpawnSource.Specific:
                    return damagePrefabSpecific;
                case SpawnSource.PrefabPool:
                    if (damagePrefabWavePool == null) {
                        return null;
                    }

                    return damagePrefabWavePool.GetRandomWeightedTransform();
            }

            return null;
        }
    }

    private void LogIfEnabled(string msg) {
        if (!enableLogging) {
            return;
        }

        Debug.Log("Killable '" + Trans.name + "' log: " + msg);
    }

    private void ResetVelocity() {
        if (this.Body != null && this.Body.useGravity && !this.Body.isKinematic) {
            this.Body.velocity = Vector3.zero;
            this.Body.angularVelocity = Vector3.zero;
        }

#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2
        // not supported
#else
        if (this.Body2D != null && !this.Body2D.isKinematic) {
            this.Body2D.velocity = Vector3.zero;
            this.Body2D.angularVelocity = 0f;
        }
#endif
    }

    #endregion

    #region Virtual methods
    /// <summary>
    /// Call this method whenever the object is spawned or starts in a Scene (from Awake event)
    /// </summary>
    /// <param name="spawned">True if spawned, false if in the Scene at beginning.</param>
    protected virtual void SpawnedOrAwake(bool spawned = true) {
        waitingToDestroy = false;

        // anything you want to do each time this is spawned.
        if (this.timesRespawned == 0) {
            this.isVisible = false;
            this.becameVisible = false;
        }

        this.isDespawning = false;
        this.spawnTime = Time.time;
        this.isTemporarilyInvincible = false;

        if (this.respawnType != RespawnType.None && !this.spawnLocationSet) {
            this.respawnLocation = this.Trans.position;
            this.spawnLocationSet = true;
        }

        // respawning from "respawn" setting.
        if (this.timesRespawned > 0) {
            this.Trans.position = this.respawnLocation;
        } else {
            // register child Killables with parent, if any
            var _parent = this.Trans.parent;
            while (_parent != null) {
                parentKillable = _parent.GetComponent<Killable>();
                if (parentKillable == null) {
                    _parent = _parent.parent;
                    continue;
                }

                parentKillable.RegisterChildKillable(this);
                break;
            }
        }

        this.currentHitPoints = hitPoints.Value;

        damageTaken = 0;
        damagePrefabsSpawned = 0;

        if (deathPrefabPoolName != null && deathPrefabSource == WaveSpecifics.SpawnOrigin.PrefabPool) {
            deathPrefabWavePool = LevelSettings.GetFirstMatchingPrefabPool(deathPrefabPoolName);
            if (deathPrefabWavePool == null) {
                LevelSettings.LogIfNew("Death Prefab Pool '" + deathPrefabPoolName + "' not found for Killable '" + this.name + "'.");
            }
        }
        if (damagePrefabSpawnMode != DamagePrefabSpawnMode.None && damagePrefabPoolName != null && damagePrefabSource == SpawnSource.PrefabPool) {
            damagePrefabWavePool = LevelSettings.GetFirstMatchingPrefabPool(damagePrefabPoolName);
            if (damagePrefabWavePool == null) {
                LevelSettings.LogIfNew("Damage Prefab Pool '" + damagePrefabWavePool + "' not found for Killable '" + this.name + "'.");
            }
        }

        if (damagePrefabSpawnMode != DamagePrefabSpawnMode.None && damagePrefabSource == SpawnSource.Specific && damagePrefabSpecific == null) {
            LevelSettings.LogIfNew(string.Format("Damage Prefab for '{0}' is not assigned.", this.Trans.name));
        }

        CheckForValidVariables();

        StopAllCoroutines(); // for respawn purposes.
        StartCoroutine(this.CoUpdate());

        deathDespawnBehavior = DeathDespawnBehavior.ReturnToPool;

        if (invincibleOnSpawn) {
            TemporaryInvincibility(invincibleTimeSpawn.Value);
        }
    }

    /// <summary>
    /// Call this method to inflict X points of damage to a Killable. 
    /// </summary>
    /// <param name="damagePoints">The number of points of damage to inflict.</param>
    public virtual void TakeDamage(int damagePoints) {
        TakeDamage(damagePoints, null);
    }

    /// <summary>
    /// Call this method to inflict X points of damage to a Killable. 
    /// </summary>
    /// <param name="damagePoints">The number of points of damage to inflict.</param>
    /// <param name="enemy">The other Killable that collided with this one.</param>
    public virtual void TakeDamage(int damagePoints, Killable enemy) {
        var dmgPrefabsSpawned = false;
        var varsModded = false;

        if (IsInvincible()) {
            if (damagePoints >= 0) {
                LogIfEnabled("Taking no damage because Invincible is checked!");
            }

            if (this.listener != null) {
                this.listener.DamagePrevented(damagePoints, enemy);
            }

            if (this.despawnMode == DespawnMode.CollisionOrTrigger) {
                this.DestroyKillable();
            }

            // mod variables and spawn dmg prefabs
            if (!varsModded) {
                ModifyWorldVariables(playerStatDamageModifiers, true);
                varsModded = true;
            }

            dmgPrefabsSpawned = SpawnDamagePrefabsIfPerHit(damagePoints);
            // end mod variables and spawn dmg prefabs

            if (damagePoints >= 0) { // allow negative damage to continue
                return;
            }
        }

        if (this.listener != null) {
            this.listener.TakingDamage(damagePoints, enemy);
        }

        // mod variables and spawn dmg prefabs
        if (!varsModded) {
            ModifyWorldVariables(playerStatDamageModifiers, true);
            varsModded = true;
        }

        if (!dmgPrefabsSpawned) {
            dmgPrefabsSpawned = SpawnDamagePrefabsIfPerHit(damagePoints);
        }
        // end mod variables and spawn dmg prefabs

        if (damagePoints == 0) {
            return;
        }

        if (enableLogging) {
            LogIfEnabled("Taking " + damagePoints + " points damage!");
        }

        this.currentHitPoints -= damagePoints;

        if (this.currentHitPoints < 0) {
            this.currentHitPoints = 0;
        } else if (this.currentHitPoints > maxHitPoints.Value) {
            this.currentHitPoints = maxHitPoints.Value;
        }

        if (hitPoints.variableSource == LevelSettings.VariableSource.Variable && syncHitPointWorldVariable) {
            var _var = WorldVariableTracker.GetWorldVariable(hitPoints.worldVariableName);
            if (_var != null) {
                _var.CurrentIntValue = this.currentHitPoints;
            }
        }

        switch (this.retriggerLimitMode) {
            case TriggeredSpawner.RetriggerLimitMode.FrameBased:
                this.triggeredLastFrame = Time.frameCount;
                break;
            case TriggeredSpawner.RetriggerLimitMode.TimeBased:
                this.triggeredLastTime = Time.time;
                break;
        }

        // mod variables and spawn dmg prefabs
        if (!varsModded) {
            ModifyWorldVariables(playerStatDamageModifiers, true);
            varsModded = true;
        }

        if (!dmgPrefabsSpawned) {
            SpawnDamagePrefabs(damagePoints);
        }
        // end mod variables and spawn dmg prefabs

        switch (this.despawnMode) {
            case DespawnMode.ZeroHitPoints:
                if (this.currentHitPoints > 0) {
                    return;
                }
                break;
            case DespawnMode.None:
                return;
        }

        this.DestroyKillable();
    }

    /// <summary>
    /// Call this method when you want the Killable to die. The death prefab (if any) will be spawned and World Variable Modifiers will be executed.
    /// </summary>
    /// <param name="scenarioName">(optional) pass the name of an alternate scenario if you wish to use a different set of World Variable Modifiers from that scenario.</param>
    public virtual void DestroyKillable(string scenarioName = DESTROYED_TEXT) {
        if (waitingToDestroy) {
            return; // already on it's way out! Don't destroy twice.
        }

        waitingToDestroy = true;

        if (deathDelay.Value > 0f) {
            StartCoroutine(WaitThenDestroy(scenarioName));
        } else {
            PerformDeath(scenarioName);
        }
    }

    public virtual string DetermineScenario(string scenarioName) {
        return scenarioName;
    }

    /// <summary>
    /// Call this method to despawn the Killable. This is not the same as DestroyKillable. This will not spawn a death prefab and will not modify World Variables.
    /// </summary>
    /// <param name="eType"></param>
    public virtual void Despawn(TriggeredSpawner.EventType eType) {
        if (LevelSettings.AppIsShuttingDown || this.isDespawning) {
            return;
        }

        this.isDespawning = true;

        if (this.listener != null) {
            this.listener.Despawning(eType);
        }

        DespawnOrRespawn();
    }

    /// <summary>
    /// This handles just despawning the item, when it's decided that you don't want to just immediately respawn it.
    /// </summary>
    public virtual void DespawnThis() {
        if (parentKillable != null) {
            parentKillable.UnregisterChildKillable(this);
        }

        PoolBoss.Despawn(this.Trans);
    }

    /// <summary>
    /// Despawns or respawns depending on the setup option chosen. Except when Despawn Behavior is set to "Disable", in which case this game object is disabled instead.
    /// </summary>
    public virtual void DespawnOrRespawn() {
        EnableColliders();

        // possibly move this into OnDespawned if it causes problems
        this.childKillables.Clear();

        ResetSpawnerInfo();

        if (deathDespawnBehavior == DeathDespawnBehavior.Disable) {
            if (parentKillable != null) {
                parentKillable.UnregisterChildKillable(this);
            }

            SpawnUtility.SetActive(GameObj, false);
            return;
        }

        if (respawnType == RespawnType.None) {
            DespawnThis();
        } else if (timesRespawned >= timesToRespawn && respawnType != RespawnType.Infinite) {
            timesRespawned = 0;
            spawnLocationSet = false;
            DespawnThis();
        } else {
            timesRespawned++;

            // reset velocity
            ResetVelocity();

            if (this.respawnDelay.Value <= 0f) {
                SpawnedOrAwake(false);
            } else {
                LevelSettings.TrackTimedRespawn(respawnDelay.Value, this.Trans, this.Trans.position);
                DespawnThis();
            }
        }
    }

    /// <summary>
    /// Determines whether this instance is temporarily invincible.
    /// </summary>
    /// <returns><c>true</c> if this instance is temporarily invincible; otherwise, <c>false</c>.</returns>
    public virtual bool IsTemporarilyInvincible() {
        return isTemporarilyInvincible;
    }

    /// <summary>
    /// Determines whether this instance is invincible.
    /// </summary>
    /// <returns><c>true</c> if this instance is invincible; otherwise, <c>false</c>.</returns>
    public virtual bool IsInvincible() {
        return isInvincible || (invincibleWhileChildrenKillablesExist && childKillables.Count > 0);
    }

    #endregion

    #region CoRoutines
    IEnumerator CoUpdate() {
        while (true) {
            yield return loopDelay;

            switch (spawnerDestroyedAction) {
                case SpawnerDestroyedBehavior.DoNothing:
                    break;
                case SpawnerDestroyedBehavior.Despawn:
                    if (spawnerSet && SpawnUtility.IsDespawnedOrDestroyed(spawnedFromObject)) {
                        if (listener != null) {
                            listener.SpawnerDestroyed();
                        }
                        Despawn(TriggeredSpawner.EventType.SpawnerDestroyed);
                    }
                    break;
                case SpawnerDestroyedBehavior.Die:
                    if (spawnerSet && SpawnUtility.IsDespawnedOrDestroyed(spawnedFromObject)) {
                        if (listener != null) {
                            listener.SpawnerDestroyed();
                        }
                        DestroyKillable();
                    }
                    break;
            }

            // check for death timer.
            if (timerDeathEnabled && Time.time - this.spawnTime > this.timerDeathSeconds.Value) {
                switch (this.timeUpAction) {
                    case SpawnerDestroyedBehavior.DoNothing:
                        break;
                    case SpawnerDestroyedBehavior.Despawn:
                        Despawn(TriggeredSpawner.EventType.DeathTimer);
                        break;
                    case SpawnerDestroyedBehavior.Die:
                        DestroyKillable();
                        break;
                }
                continue;
            }

            // check for "not visible too long"
            if (!this.despawnIfNotVisible || this.becameVisible) {
                continue;
            }

            if (Time.time - this.spawnTime > this.despawnIfNotVisibleForSec.Value) {
                this.Despawn(TriggeredSpawner.EventType.Invisible);
            }
        }
    }

    private IEnumerator WaitThenDestroy(string scenarioName) {
        if (listener != null) {
            listener.DeathDelayStarted(deathDelay.Value);
        }

        if (deathDelay.Value > 0f) {
            if (listener != null) {
                listener.WaitingToDestroyKillable(this);
            }

            yield return new WaitForSeconds(deathDelay.Value);
        }

        PerformDeath(scenarioName);
    }

    private void PerformDeath(string scenarioName) {
        scenarioName = DetermineScenario(scenarioName);

        if (listener != null) {
            listener.DestroyingKillable(this);
            scenarioName = listener.DeterminingScenario(this, scenarioName);
        }

        if (ExplosionPrefab != null) {
            PoolBoss.SpawnInPool(ExplosionPrefab, this.Trans.position, Quaternion.identity);
        }

        if (deathPrefabSource == WaveSpecifics.SpawnOrigin.Specific && deathPrefabSpecific == null) {
            // no death prefab.
        } else {
            SpawnDeathPrefabs();
        }

        // modify world variables
        if (scenarioName == DESTROYED_TEXT) {
            ModifyWorldVariables(this.playerStatDespawnModifiers, false);
        } else {
            WorldVariableCollection scenario = alternateModifiers.Find(delegate(WorldVariableCollection obj) {
                return obj.scenarioName == scenarioName;
            });

            if (scenario == null) {
                LevelSettings.LogIfNew("Scenario: '" + scenarioName + "' not found in Killable '" + this.Trans.name + "'. No World Variables modified by destruction.");
            } else {
                ModifyWorldVariables(scenario, false);
            }
        }

        this.Despawn(TriggeredSpawner.EventType.LostHitPoints);
    }

    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the respawn position. Defaults to the location last spawned.
    /// </summary>
    /// <value>The respawn position.</value>
    public Vector3 RespawnPosition {
        get {
            return respawnLocation;
        }
        set {
            respawnLocation = value;
        }
    }

    /// <summary>
    /// This property returns a cached lazy-lookup of the Transform component.
    /// </summary>
    public Transform Trans {
        get {
            if (this._trans == null) {
                this._trans = this.transform;
            }

            return this._trans;
        }
    }

    /// <summary>
    /// The current hit points.
    /// </summary>
    public int CurrentHitPoints {
        get {
            return currentHitPoints;
        }
        set {
            currentHitPoints = value;
        }
    }

    public Collider Colidr {
        get {
            if (this._collider == null) {
                this._collider = this.GetComponent<Collider>();
            }

            return this._collider;
        }
    }

    public Rigidbody Body {
        get {
            if (this._body == null) {
                this._body = this.GetComponent<Rigidbody>();
            }

            return this._body;
        }
    }

#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2
    // not supported
#else
	private Rigidbody2D Body2D {
		get {
			if (this._body2d == null) {
				this._body2d = this.GetComponent<Rigidbody2D>();
			}
			
			return this._body2d;
		}
	}

	private Collider2D Colidr2D {
		get {
			if (this._collider2d == null) {
				this._collider2d = this.GetComponent<Collider2D>();
			}

			return this._collider2d;
		}
	}
#endif

    /// <summary>
    /// The game object this Killable was spawned from, if any.
    /// </summary>
    public GameObject SpawnedFromObject {
        get {
            return spawnedFromObject;
        }
    }

    public int? SpawnedFromObjectId {
        get {
            if (SpawnedFromObject != null && !this.spawnedFromGOInstanceId.HasValue) {
                this.spawnedFromGOInstanceId = SpawnedFromObject.GetInstanceID();
            }

            return this.spawnedFromGOInstanceId;
        }
    }

    private GameObject GameObj {
        get {
            if (this.go == null) {
                this.go = this.gameObject;
            }

            return this.go;
        }
    }

    private int? KillableId {
        get {
            if (!this.instanceId.HasValue) {
                this.instanceId = this.GameObj.GetInstanceID();
            }

            return this.instanceId;
        }
    }

    #endregion

    public void RegisterChildKillable(Killable kill) {
        if (this.childKillables.Contains(kill)) {
            return;
        }

        this.childKillables.Add(kill);

        if (this.invincibleWhileChildrenKillablesExist && this.disableCollidersWhileChildrenKillablesExist) {
            DisableColliders();
        }

        // Diagnostic code to uncomment if things are going wrong.
        //Debug.Log("ADD - children of '" + this.name + "': " + this.childKillables.Count);
    }

    private void DisableColliders() {
        if (Colidr != null) {
            Colidr.enabled = false;
        }

#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2
        // unsupported
#else
			if (Colidr2D != null) {
				Colidr2D.enabled = false;
			}
#endif
    }

    private void EnableColliders() {
        if (Colidr != null) {
            Colidr.enabled = true;
        }

#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2
        // unsupported
#else
		if (Colidr2D != null) {
			Colidr2D.enabled = true;
		}
#endif
    }

    public void UnregisterChildKillable(Killable kill) {
        this.childKillables.Remove(kill);

        deathDespawnBehavior = DeathDespawnBehavior.Disable;

        if (this.childKillables.Count == 0 && this.invincibleWhileChildrenKillablesExist && this.disableCollidersWhileChildrenKillablesExist) {
            EnableColliders();
        }

        // Diagnostic code to uncomment if things are going wrong.
        //Debug.Log("REMOVE - children of '" + this.name + "': " + this.childKillables.Count);
    }

    private void ResetSpawnerInfo() {
        this.spawnedFromObject = null;
        this.spawnerSet = false;
        this.spawnedFromGOInstanceId = null;
    }
}