using UnityEngine;
using System.Collections;

/// <summary>
/// This class is used to configure a Timed Despawner
/// </summary>
[AddComponentMenu("Dark Tonic/Core GameKit/Despawners/Timed Despawner")]
public class TimedDespawner : MonoBehaviour {
    public float LifeSeconds = 5;
    public bool StartTimerOnSpawn = true;
    public TimedDespawnerListener listener;

    private Transform trans;
    YieldInstruction timerDelay = null;

    void Awake() {
        this.trans = this.transform;
        timerDelay = new WaitForSeconds(LifeSeconds);
        this.AwakeOrSpawn();
    }

    void OnSpawned() { // used by Core GameKit Pooling & also Pool Manager Pooling!
        this.AwakeOrSpawn();
    }

    void AwakeOrSpawn() {
        if (this.StartTimerOnSpawn) {
            this.StartTimer();
        }
    }

    /// <summary>
    /// Call this method to start the Timer if it's not set to start automatically.
    /// </summary>
    public void StartTimer() {
        StartCoroutine(WaitUntilTimeUp());
    }

    private IEnumerator WaitUntilTimeUp() {
        yield return timerDelay;

        if (listener != null) {
            listener.Despawning(this.trans);
        }
        PoolBoss.Despawn(trans);

    }
}