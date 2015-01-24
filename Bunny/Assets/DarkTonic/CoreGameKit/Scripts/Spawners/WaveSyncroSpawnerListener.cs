using UnityEngine;
using System.Collections;

[AddComponentMenu("Dark Tonic/Core GameKit/Listeners/Syncro Spawner Listener")]
public class WaveSyncroSpawnerListener : MonoBehaviour {
    public string sourceSpawnerName = string.Empty;

    void Reset() {
        var src = this.GetComponent<WaveSyncroPrefabSpawner>();
        if (src != null) {
            src.listener = this;
            this.sourceSpawnerName = this.name;
        }
    }

    public virtual void ItemFailedToSpawn(Transform failedPrefabTrans) {
        // your code here. The transform is not spawned. This is just a reference
    }

    public virtual void ItemSpawned(Transform spawnedTrans) {
        // do something to the Transform.
    }

    public virtual void WaveFinishedSpawning(WaveSpecifics spec) {
        // Please do not manipulate values in the "spec". It is for your read-only information
    }

    public virtual void WaveStart(WaveSpecifics spec) {
        // Please do not manipulate values in the "spec". It is for your read-only information
    }

    public virtual void EliminationWaveCompleted(WaveSpecifics spec) { // called at the end of each wave, whether or not it is repeating. This is called before the Repeat delay
        // Please do not manipulate values in the "spec". It is for your read-only information
    }

    public virtual void WaveRepeat(WaveSpecifics spec) {
        // Please do not manipulate values in the "spec". It is for your read-only information
    }
}
