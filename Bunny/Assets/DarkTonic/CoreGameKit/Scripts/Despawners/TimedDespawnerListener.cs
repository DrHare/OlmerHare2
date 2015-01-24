using UnityEngine;
using System.Collections;

[AddComponentMenu("Dark Tonic/Core GameKit/Listeners/Timed Despawner Listener")]
public class TimedDespawnerListener : MonoBehaviour {
    public string sourceDespawnerName;

    void Reset() {
        var src = this.GetComponent<TimedDespawner>();
        if (src != null) {
            src.listener = this;
            this.sourceDespawnerName = this.name;
        }
    }

    public virtual void Despawning(Transform transDespawning) {
        // Your code here.
    }
}
