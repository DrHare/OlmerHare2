using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This class is used to spawn and despawn things using pooling (avoids Instantiate and Destroy calls).
/// </summary>
public class PoolBoss : MonoBehaviour {
    private const string SPAWNED_MESSAGE_NAME = "OnSpawned";
    private const string DESPAWNED_MESSAGE_NAME = "OnDespawned";
    private const string NOT_INIT_ERROR = "Pool Boss has not initialized (does so in Awake event) and is not ready to be used yet.";

    public List<PoolBossItem> poolItems = new List<PoolBossItem>();
    public bool logMessages = false;
    public bool poolItemsExpanded = true;
    public bool autoAddMissingPoolItems = false;

    private static Dictionary<string, PoolItemInstanceList> poolItemsByName = new Dictionary<string, PoolItemInstanceList>();
    private static Transform _trans;
    private static PoolBoss _instance;
    private static bool isReady = false;

    public class PoolItemInstanceList {
        public bool _logMessages = false;
        public bool _allowInstantiateMore = false;
        public int? _itemHardLimit = null;
        public Transform _sourceTrans = null;
        public List<Transform> _spawnedClones = new List<Transform>();
        public List<Transform> _despawnedClones = new List<Transform>();
        public bool _allowRecycle = false;

        public PoolItemInstanceList(List<Transform> clones) {
            _spawnedClones.Clear();
            _despawnedClones = clones;
        }
    }

    public static PoolBoss Instance {
        get {
            if (_instance == null) {
                _instance = (PoolBoss)GameObject.FindObjectOfType(typeof(PoolBoss));
            }

            return _instance;
        }
    }

    void Awake() {
        isReady = false;

        poolItemsByName.Clear();

        for (var p = 0; p < poolItems.Count; p++) {
            var item = poolItems[p];

            if (item.instancesToPreload <= 0) {
                continue;
            }

            if (item.prefabTransform == null) {
                LevelSettings.LogIfNew("You have an item in Pool Boss with no prefab assigned at position: " + (p + 1));
                continue;
            }

            var itemName = item.prefabTransform.name;
            if (poolItemsByName.ContainsKey(itemName)) {
                LevelSettings.LogIfNew("You have more than one instance of '" + itemName + "' in Pool Boss. Skipping the second instance.");
                continue;
            }

            var itemClones = new List<Transform>();

            for (var i = 0; i < item.instancesToPreload; i++) {
                var createdObjTransform = InstantiateForPool(item.prefabTransform, i + 1);
                itemClones.Add(createdObjTransform);
            }

            var instanceList = new PoolItemInstanceList(itemClones);
            instanceList._logMessages = item.logMessages;
            instanceList._allowInstantiateMore = item.allowInstantiateMore;
            instanceList._sourceTrans = item.prefabTransform;
            instanceList._itemHardLimit = item.itemHardLimit;
            instanceList._allowRecycle = item.allowRecycle;

            poolItemsByName.Add(itemName, instanceList);
        }

        isReady = true;
    }

    private static Transform InstantiateForPool(Transform prefabTrans, int cloneNumber) {
        var createdObjTransform = GameObject.Instantiate(prefabTrans, Trans.position, prefabTrans.rotation) as Transform;
        createdObjTransform.name = prefabTrans.name + " (Clone " + cloneNumber + ")"; // don't want the "(Clone)" suffix.
        createdObjTransform.parent = Trans;
        SpawnUtility.SetActive(createdObjTransform.gameObject, false);

        return createdObjTransform;
    }

    private static void CreateMissingPoolItem(Transform missingTrans, string itemName, bool isSpawn) {
        var instances = new List<Transform>();

        if (isSpawn) {
            var createdObjTransform = InstantiateForPool(missingTrans, instances.Count + 1);
            instances.Add(createdObjTransform);
        }
        var newItemSettings = new PoolItemInstanceList(instances);

        newItemSettings._logMessages = false;
        newItemSettings._allowInstantiateMore = true;
        newItemSettings._sourceTrans = missingTrans;

        poolItemsByName.Add(itemName, newItemSettings);

        // for the Inspector only
        PoolBoss.Instance.poolItems.Add(new PoolBossItem() {
            instancesToPreload = 1,
            isExpanded = true,
            allowInstantiateMore = true,
            logMessages = false,
            prefabTransform = missingTrans
        });

        if (PoolBoss.Instance.logMessages) {
            Debug.LogWarning("PoolBoss created Pool Item for missing item '" + itemName + "' at " + Time.time);
        }
    }

    /// <summary>
    /// Call this method to spawn a prefab using Pool Boss, which will be spawned with no parent Transform (outside the pool)
    /// </summary>
    /// <param name="spawn">Transform to spawn</param>
    /// <param name="spawnPos">The position to spawn it at</param>
    /// <param name="spawnRotation">The rotation to use</param>
    /// <returns>The Transform of the spawned object. It can be null if spawning failed from limits you have set.</returns>
    public static Transform SpawnOutsidePool(Transform transToSpawn, Vector3 position, Quaternion rotation) {
        return Spawn(transToSpawn, position, rotation, null);
    }

    /// <summary>
    /// Call this method to spawn a prefab using Pool Boss, which will be a child of the Pool Boss prefab.
    /// </summary>
    /// <param name="spawn">Transform to spawn</param>
    /// <param name="spawnPos">The position to spawn it at</param>
    /// <param name="spawnRotation">The rotation to use</param>
    /// <returns>The Transform of the spawned object. It can be null if spawning failed from limits you have set.</returns>
    public static Transform SpawnInPool(Transform transToSpawn, Vector3 position, Quaternion rotation) {
        return Spawn(transToSpawn, position, rotation, Trans);
    }

    /// <summary>
    /// Call this method to spawn a prefab using Pool Boss. All the Spawners and Killable use this method.
    /// </summary>
    /// <param name="spawn">Transform to spawn</param>
    /// <param name="spawnPos">The position to spawn it at</param>
    /// <param name="spawnRotation">The rotation to use</param>
    /// <param name="parentTransform">The parent Transform to use, if any (optional)</param>
    /// <returns>The Transform of the spawned object. It can be null if spawning failed from limits you have set.</returns>
    public static Transform Spawn(Transform transToSpawn, Vector3 position, Quaternion rotation, Transform parentTransform) {
        if (!isReady) {
            LevelSettings.LogIfNew(NOT_INIT_ERROR);
            return null;
        }

        if (transToSpawn == null) {
            LevelSettings.LogIfNew("No Transform passed to Spawn method.");
            return null;
        }

        if (Instance == null) {
            return null;
        }

        var itemName = transToSpawn.name;
        if (!poolItemsByName.ContainsKey(itemName)) {
            if (PoolBoss.Instance.autoAddMissingPoolItems) {
                CreateMissingPoolItem(transToSpawn, itemName, true);
            } else {
                LevelSettings.LogIfNew("The Transform '" + itemName + "' passed to Spawn method is not configured in Pool Boss.");
                return null;
            }
        }

        var itemSettings = poolItemsByName[itemName];

        Transform cloneToSpawn = null;

        if (itemSettings._despawnedClones.Count == 0) {
            if (!itemSettings._allowInstantiateMore) {
                if (itemSettings._allowRecycle) {
                    cloneToSpawn = itemSettings._spawnedClones[0];
                } else {
                    LevelSettings.LogIfNew("The Transform '" + transToSpawn.name + "' has no available clones left to Spawn in Pool Boss. Please increase your Preload Qty, turn on Allow Instantiate More or turn on Recycle Oldest (Recycle is only for non-essential things like decals).", true);
                    return null;
                }
            } else {
                // Instantiate a new one
                var curCount = NumberOfClones(itemSettings);
                if (curCount >= itemSettings._itemHardLimit) {
                    LevelSettings.LogIfNew("The Transform '" + transToSpawn.name + "' has reached its item limit in Pool Boss. Please increase your Preload Qty or Item Limit.", true);
                    return null;
                }

                var createdObjTransform = InstantiateForPool(itemSettings._sourceTrans, curCount + 1);
                itemSettings._despawnedClones.Add(createdObjTransform);

                if (PoolBoss.Instance.logMessages || itemSettings._logMessages) {
                    Debug.LogWarning("Pool Boss Instantiated an extra '" + itemName + "' at " + Time.time + " because there were none left in the Pool.");
                }
            }
        }

        if (cloneToSpawn == null) {
            var randomIndex = UnityEngine.Random.Range(0, itemSettings._despawnedClones.Count);
            cloneToSpawn = itemSettings._despawnedClones[randomIndex];
        } else { // recycling
            cloneToSpawn.BroadcastMessage(DESPAWNED_MESSAGE_NAME, SendMessageOptions.DontRequireReceiver);
        }

        if (cloneToSpawn == null) {
            LevelSettings.LogIfNew("One or more of the prefab '" + itemName + "' in Pool Boss has been destroyed. You should never destroy objects in the Pool. Despawn instead. Not spawning anything for this call.");
            return null;
        }

        cloneToSpawn.position = position;
        cloneToSpawn.rotation = rotation;
        SpawnUtility.SetActive(cloneToSpawn.gameObject, true);

        if (PoolBoss.Instance.logMessages || itemSettings._logMessages) {
            Debug.Log("Pool Boss spawned '" + itemName + "' at " + Time.time);
        }

        if (parentTransform != null) {
            cloneToSpawn.parent = parentTransform;
        }

        cloneToSpawn.BroadcastMessage(SPAWNED_MESSAGE_NAME, SendMessageOptions.DontRequireReceiver);

        itemSettings._despawnedClones.Remove(cloneToSpawn);
        itemSettings._spawnedClones.Add(cloneToSpawn);

        return cloneToSpawn;
    }

    /// <summary>
    /// Call this method to despawn a prefab using Pool Boss. All the Spawners and Killable use this method.
    /// </summary>
    /// <param name="spawned">Transform to despawn</param>
    public static void Despawn(Transform transToDespawn) {
        if (!isReady) {
            LevelSettings.LogIfNew(NOT_INIT_ERROR);
            return;
        }

        if (!SpawnUtility.IsActive(transToDespawn.gameObject)) {
            return; // already sent to despawn
        }

        if (PoolBoss.Instance == null) {
            // Scene changing, do nothing.
            return;
        }

        if (transToDespawn == null) {
            LevelSettings.LogIfNew("No Transform passed to Despawn method.");
            return;
        }

        var itemName = GetPrefabName(transToDespawn);

        if (!poolItemsByName.ContainsKey(itemName)) {
            if (PoolBoss.Instance.autoAddMissingPoolItems) {
                CreateMissingPoolItem(transToDespawn, itemName, false);
            } else {
                LevelSettings.LogIfNew("The Transform '" + itemName + "' passed to Despawn is not in Pool Boss. Not despawning.");
                return;
            }
        }

        transToDespawn.BroadcastMessage(DESPAWNED_MESSAGE_NAME, SendMessageOptions.DontRequireReceiver);

        var cloneList = poolItemsByName[itemName];

        transToDespawn.parent = Trans;
        SpawnUtility.SetActive(transToDespawn.gameObject, false);

        if (PoolBoss.Instance.logMessages || cloneList._logMessages) {
            Debug.Log("Pool Boss despawned '" + itemName + "' at " + Time.time);
        }

        cloneList._spawnedClones.Remove(transToDespawn);
        cloneList._despawnedClones.Add(transToDespawn);
    }

    /// <summary>
    /// This method will despawn all spawned instances of prefabs.
    /// </summary>
    public static void DespawnAllPrefabs() {
        if (PoolBoss.Instance == null) {
            // Scene changing, do nothing.
            return;
        }

        var items = poolItemsByName.Values.GetEnumerator();
        while (items.MoveNext()) {
            DespawnAllOfPrefab(items.Current._sourceTrans);
        }
    }

    /// <summary>
    /// This method will "Kill" all spawned instances of all prefab.
    /// </summary>
    public static void KillAllPrefabs() {
        if (PoolBoss.Instance == null) {
            // Scene changing, do nothing.
            return;
        }

        var items = poolItemsByName.Values.GetEnumerator();
        while (items.MoveNext()) {
            KillAllOfPrefab(items.Current._sourceTrans);
        }
    }

    /// <summary>
    /// This method will despawn all spawned instances of the prefab you pass in.
    /// </summary>
    /// <param name="transToDespawn">Transform component of a prefab</param>
    public static void DespawnAllOfPrefab(Transform transToDespawn) {
        if (PoolBoss.Instance == null) {
            // Scene changing, do nothing.
            return;
        }

        if (transToDespawn == null) {
            LevelSettings.LogIfNew("No Transform passed to DespawnAllOfPrefab method.");
            return;
        }

        var itemName = GetPrefabName(transToDespawn);

        if (!poolItemsByName.ContainsKey(itemName)) {
            LevelSettings.LogIfNew("The Transform '" + itemName + "' passed to DespawnAllOfPrefab is not in Pool Boss. Not despawning.");
            return;
        }

        var spawned = poolItemsByName[itemName]._spawnedClones;

        var max = spawned.Count;
        while (spawned.Count > 0 && max > 0) {
            Despawn(spawned[0]);
            max--;
        }
    }

    /// <summary>
    /// This method will "Kill" all spawned instances of the prefab you pass in.
    /// </summary>
    /// <param name="transToDespawn">Transform component of a prefab</param>
    public static void KillAllOfPrefab(Transform transToKill) {
        if (PoolBoss.Instance == null) {
            // Scene changing, do nothing.
            return;
        }

        if (transToKill == null) {
            LevelSettings.LogIfNew("No Transform passed to KillAllOfPrefab method.");
            return;
        }

        var itemName = GetPrefabName(transToKill);

        if (!poolItemsByName.ContainsKey(itemName)) {
            LevelSettings.LogIfNew("The Transform '" + itemName + "' passed to KillAllOfPrefab is not in Pool Boss. Not killing.");
            return;
        }

        var spawned = poolItemsByName[itemName]._spawnedClones;

        var count = spawned.Count;
        for (var i = 0; i < spawned.Count && count > 0; i++) {
            var kill = spawned[i].GetComponent<Killable>();
            if (kill != null) {
                kill.DestroyKillable();
            }

            count--;
        }
    }

    /// <summary>
    /// Call this method get info on a Pool Boss item (number of spawned and despawned copies, allow instantiate more, log etc).
    /// </summary>
    /// <param name="poolItemName">The name of the prefab you're asking about.</param>
    public static PoolItemInstanceList PoolItemInfoByName(string poolItemName) {
        if (string.IsNullOrEmpty(poolItemName)) {
            return null;
        }

        if (!poolItemsByName.ContainsKey(poolItemName)) {
            return null;
        }

        return poolItemsByName[poolItemName];
    }

    /// <summary>
    /// Call this method determine if the item (Transform) you pass in is set up in Pool Boss.
    /// </summary>
    /// <param name="trans">Transform you want to know is in the Pool or not.</param>
    public static bool PrefabIsInPool(Transform trans) {
        if (!isReady) {
            LevelSettings.LogIfNew(NOT_INIT_ERROR);
            return false;
        }

        return PrefabIsInPool(trans.name);
    }

    /// <summary>
    /// Call this method determine if the item name you pass in is set up in Pool Boss.
    /// </summary>
    /// <param name="string">Item name you want to know is in the Pool or not.</param>
    public static bool PrefabIsInPool(string transName) {
        if (!isReady) {
            LevelSettings.LogIfNew(NOT_INIT_ERROR);
            return false;
        }

        return poolItemsByName.ContainsKey(transName);
    }

    /// <summary>
    /// This will tell you how many available clones of a prefab are despawned and ready to spawn. A value of -1 indicates an error
    /// </summary>
    /// <param name="transToDespawn">The transform component of the prefab you want the despawned count of.</param>
    public static int PrefabDespawnedCount(Transform transPrefab) {
        if (PoolBoss.Instance == null) {
            // Scene changing, do nothing.
            return -1;
        }

        if (transPrefab == null) {
            LevelSettings.LogIfNew("No Transform passed to DespawnedCountOfPrefab method.");
            return -1;
        }

        var itemName = GetPrefabName(transPrefab);

        if (!poolItemsByName.ContainsKey(itemName)) {
            LevelSettings.LogIfNew("The Transform '" + itemName + "' passed to DespawnedCountOfPrefab is not in Pool Boss. Not despawning.");
            return -1;
        }

        var despawned = poolItemsByName[itemName]._despawnedClones.Count;
        return despawned;
    }

    /// <summary>
    /// This will tell you how many clones of a prefab are already spawned out of Pool Boss. A value of -1 indicates an error
    /// </summary>
    /// <param name="transToDespawn">The transform component of the prefab you want the spawned count of.</param>
    public static int PrefabSpawnedCount(Transform transPrefab) {
        if (PoolBoss.Instance == null) {
            // Scene changing, do nothing.
            return -1;
        }

        if (transPrefab == null) {
            LevelSettings.LogIfNew("No Transform passed to SpawnedCountOfPrefab method.");
            return -1;
        }

        var itemName = GetPrefabName(transPrefab);

        if (!poolItemsByName.ContainsKey(itemName)) {
            LevelSettings.LogIfNew("The Transform '" + itemName + "' passed to SpawnedCountOfPrefab is not in Pool Boss. Not despawning.");
            return -1;
        }

        var spawned = poolItemsByName[itemName]._spawnedClones.Count;
        return spawned;
    }

    /// <summary>
    /// Call this method to find out if all are despawned
    /// </summary>
    /// <param name="transPrefab">The transform of the prefab you are asking about.</param>
    /// <returns>True or False</returns>
    public static bool AllOfPrefabAreDespawned(Transform transPrefab) {
        return PrefabDespawnedCount(transPrefab) == 0;
    }

    /// <summary>
    /// This method will tell you how many different items are set up in Pool Boss.
    /// </summary>
    public static int PrefabCount {
        get {
            if (!isReady) {
                LevelSettings.LogIfNew(NOT_INIT_ERROR);
                return -1;
            }

            return poolItemsByName.Count;
        }
    }

    private static int NumberOfClones(PoolItemInstanceList instList) {
        if (!isReady) {
            LevelSettings.LogIfNew(NOT_INIT_ERROR);
            return -1;
        }

        return instList._despawnedClones.Count + instList._spawnedClones.Count;
    }

    private static string GetPrefabName(Transform trans) {
        if (trans == null) {
            return null;
        }

        var itemName = trans.name;
        var iParen = itemName.IndexOf(" (");
        if (iParen > -1) {
            itemName = itemName.Substring(0, iParen);
        }

        return itemName;
    }

    public static Transform Trans {
        get {
            if (_trans == null) {
                _trans = Instance.GetComponent<Transform>();
            }

            return _trans;
        }
    }
}
