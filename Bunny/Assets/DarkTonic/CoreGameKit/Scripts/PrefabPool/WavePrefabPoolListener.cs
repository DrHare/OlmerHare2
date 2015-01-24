using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("Dark Tonic/Core GameKit/Listeners/Prefab Pool Listener")]
public class WavePrefabPoolListener : MonoBehaviour {
    public string sourcePrefabPoolName;

    void Reset() {
        var src = this.GetComponent<WavePrefabPool>();
        if (src != null) {
            src.listener = this;
            this.sourcePrefabPoolName = this.name;
        }
    }

    public virtual void PrefabGrabbedFromPool(Transform transGrabbed) {
        // your code here
    }

    public virtual void PoolRefilling() {
        // your code here
    }
}
