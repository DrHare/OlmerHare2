using UnityEngine;
using System.Collections;

public class SpawnTracker : MonoBehaviour {
    private WaveSyncroPrefabSpawner sourceSpawner;
    private Transform trans;

    void Awake() {
    }

    void OnDisable() {
		if (SourceSpawner == null) {
			return;
		}

		SourceSpawner.RemoveSpawnedMember(Trans);
		SourceSpawner = null;
    }

    public WaveSyncroPrefabSpawner SourceSpawner {
        get {
            return this.sourceSpawner;
        }
        set {
            this.sourceSpawner = value;
        }
    }

	public Transform Trans {
		get {
			if (this.trans == null) {
				this.trans = this.transform;
			}

			return this.trans;
		}
	}
}
