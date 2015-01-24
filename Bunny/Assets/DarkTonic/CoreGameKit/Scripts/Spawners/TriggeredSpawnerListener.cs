using UnityEngine;
using System.Collections;


[AddComponentMenu("Dark Tonic/Core GameKit/Listeners/Triggered Spawner Listener")]
public class TriggeredSpawnerListener : MonoBehaviour {
    public string sourceSpawnerName = string.Empty;

    void Reset() {
        var src = this.GetComponent<TriggeredSpawner>();
        if (src != null) {
            src.listener = this;
            this.sourceSpawnerName = this.name;
        }
    }

    public virtual void EventPropagating(TriggeredSpawner.EventType eType, Transform transmitterTrans, int receiverSpawnerCount) {
        // your code here.
    }

    public virtual void PropagatedEventReceived(TriggeredSpawner.EventType eType, Transform transmitterTrans) {
        // your code here. 
    }

    public virtual void WaveEndedEarly(TriggeredSpawner.EventType eType) {
        // your code here. 
    }

    public virtual void PropagatedWaveEndedEarly(TriggeredSpawner.EventType eType, string customEventName, Transform transmitterTrans, int receiverSpawnerCount) {
        // your code here. 
    }

    public virtual void ItemFailedToSpawn(TriggeredSpawner.EventType eType, Transform failedPrefabTrans) {
        // your code here. The transform is not spawned. This is just a reference
    }

    public virtual void ItemSpawned(TriggeredSpawner.EventType eType, Transform spawnedTrans) {
        // do something to the Transform.
    }

    public virtual void WaveFinishedSpawning(TriggeredSpawner.EventType eType, TriggeredWaveSpecifics spec) {
        // please do not manipulate values in the "spec". It is for your read-only information
    }

    public virtual void WaveStart(TriggeredSpawner.EventType eType, TriggeredWaveSpecifics spec) {
        // please do not manipulate values in the "spec". It is for your read-only information
    }

    public virtual void WaveRepeat(TriggeredSpawner.EventType eType, TriggeredWaveSpecifics spec) {
        // please do not manipulate values in the "spec". It is for your read-only information
    }

    public virtual void SpawnerDespawning(Transform transDespawning) {
        // your code here.
    }

	public virtual void CustomEventReceived(string customEventName, Vector3 eventOrigin) {
		// your code here.
	}
}
