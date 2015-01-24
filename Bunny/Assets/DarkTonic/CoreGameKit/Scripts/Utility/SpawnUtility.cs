using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This class is used as a wrapper for Pool Boss, but if you prefer to use Pool Manager, you can change that hookup here.
/// </summary>
public static class SpawnUtility {
    /// <summary>
    /// Call this method to despawn all instances of a prefab using Pool Boss. 
    /// </summary>
    /// <param name="transToDespawn">Transform to despawn</param>
    public static void DespawnAllOfPrefab(Transform transToDespawn) {
        PoolBoss.DespawnAllOfPrefab(transToDespawn);
    }

    /// <summary>
    /// Call this method to kill all instances of a prefab using Pool Boss. Only prefabs with a Killable component can be killed.  
    /// </summary>
    /// <param name="transToKill">Transform to kill</param>
    public static void KillAllOfPrefab(Transform transToKill) {
        PoolBoss.KillAllOfPrefab(transToKill);
    }

    /// <summary>
    /// Call this method to despawn all instances of all prefabs that use Pool Boss. 
    /// </summary>
    public static void DespawnAllPrefabs() {
        PoolBoss.DespawnAllPrefabs();
    }

    /// <summary>
    /// Call this method to kill all instances of all prefabs that use Pool Boss. Only prefabs with a Killable component can be killed.  
    /// </summary>
    public static void KillAllPrefabs() {
        PoolBoss.KillAllPrefabs();
    }

    public static bool SpawnedMembersAreAllBeyondDistance(Transform spawnerTrans, List<Transform> members, float minDist) {
        bool allMembersBeyondDistance = true;

        var spawnerPos = spawnerTrans.position;
        var sqrDist = minDist * minDist;

        foreach (var t in members) {
            if (t == null || !IsActive(t.gameObject)) { // .active will work with Pool Manager.
                continue;
            }

            if (Vector3.SqrMagnitude(spawnerPos - t.transform.position) < sqrDist) {
                allMembersBeyondDistance = false;
            }
        }

        return allMembersBeyondDistance;
    }

    public static void RecordSpawnerObjectIfKillable(Transform spawnedObject, GameObject spawnerObject) {
        var spawnedKill = spawnedObject.GetComponent<Killable>();
        if (spawnedKill != null) {
            spawnedKill.RecordSpawner(spawnerObject);
        }
    }

    /// <summary>
    /// This method will tell you if a GameObject is either despawned or destroyed.
    /// </summary>
    /// <param name="objectToCheck">The GameObject you're asking about.</param>
    /// <returns>True or false</returns>
    public static bool IsDespawnedOrDestroyed(GameObject objectToCheck) {
        return objectToCheck == null || !IsActive(objectToCheck);
    }

    /// <summary>
    /// This is a cross-Unity-version method to tell you if a GameObject is active in the Scene.
    /// </summary>
    /// <param name="go">The GameObject you're asking about.</param>
    /// <returns>True or false</returns>
    public static bool IsActive(GameObject go) {
#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
        return go.active;
#else
			return go.activeInHierarchy;
#endif
    }

    /// <summary>
    /// This is a cross-Unity-version method to set a GameObject to active in the Scene.
    /// </summary>
    /// <param name="go">The GameObject you're setting to active or inactive</param>
    /// <param name="isActive">True to set the object to active, false to set it to inactive.</param>
    public static void SetActive(GameObject go, bool isActive) {
#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
        go.SetActiveRecursively(isActive);
#else
			go.SetActive(isActive);
#endif
    }
}