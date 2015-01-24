using UnityEngine;
using System.Collections;

[AddComponentMenu("Dark Tonic/Core GameKit/Listeners/Triggered Despawner Listener")]
public class TriggeredDespawnerListener : MonoBehaviour {
    public string sourceDespawnerName;

    void Reset() {
        var src = this.GetComponent<TriggeredDespawner>();
        if (src != null) {
            src.listener = this;
            this.sourceDespawnerName = this.name;
        }
    }

    public virtual void Despawning(TriggeredSpawner.EventType eType, Transform transDespawning) {
        // Your code here.
    }
}
