using UnityEngine;
using System.Collections;

[AddComponentMenu("Dark Tonic/Core GameKit/Despawners/Triggered Despawner")]
public class TriggeredDespawner : MonoBehaviour {
    private Transform trans;

    public EventDespawnSpecifics invisibleSpec = new EventDespawnSpecifics();
    public EventDespawnSpecifics mouseOverSpec = new EventDespawnSpecifics();
    public EventDespawnSpecifics mouseClickSpec = new EventDespawnSpecifics();
    public EventDespawnSpecifics collisionSpec = new EventDespawnSpecifics();
    public EventDespawnSpecifics triggerEnterSpec = new EventDespawnSpecifics();
    public EventDespawnSpecifics triggerExitSpec = new EventDespawnSpecifics();
    public EventDespawnSpecifics collision2dSpec = new EventDespawnSpecifics();
    public EventDespawnSpecifics triggerEnter2dSpec = new EventDespawnSpecifics();
    public EventDespawnSpecifics triggerExit2dSpec = new EventDespawnSpecifics();
    public EventDespawnSpecifics onClickSpec = new EventDespawnSpecifics();
    public TriggeredDespawnerListener listener;

    private bool isDespawning = false;

    void Awake() {
        this.trans = this.transform;
        SpawnedOrAwake();
    }

    void OnSpawned() {
        SpawnedOrAwake();
    }

    protected virtual void SpawnedOrAwake() {
        isDespawning = false;
    }

    void OnBecameInvisible() {
        if (invisibleSpec.eventEnabled) {
            Despawn(TriggeredSpawner.EventType.Invisible);
        }
    }

    void OnMouseEnter() {
        if (mouseOverSpec.eventEnabled) {
            Despawn(TriggeredSpawner.EventType.MouseOver_Legacy);
        }
    }

    void OnMouseDown() {
        if (mouseClickSpec.eventEnabled) {
            Despawn(TriggeredSpawner.EventType.MouseClick_Legacy);
        }
    }

    void OnClick() {
        if (onClickSpec.eventEnabled) {
            Despawn(TriggeredSpawner.EventType.OnClick_NGUI);
        }
    }

    void OnCollisionEnter(Collision collision) {
        // check filters for matches if turned on
        if (!collisionSpec.eventEnabled) {
            return;
        }

        if (collisionSpec.useLayerFilter && !collisionSpec.matchingLayers.Contains(collision.gameObject.layer)) {
            return;
        }

        if (collisionSpec.useTagFilter && !collisionSpec.matchingTags.Contains(collision.gameObject.tag)) {
            return;
        }

        Despawn(TriggeredSpawner.EventType.OnCollision);
    }

    void OnTriggerEnter(Collider other) {
        // check filters for matches if turned on
        if (!triggerEnterSpec.eventEnabled) {
            return;
        }

        if (triggerEnterSpec.useLayerFilter && !triggerEnterSpec.matchingLayers.Contains(other.gameObject.layer)) {
            return;
        }

        if (triggerEnterSpec.useTagFilter && !triggerEnterSpec.matchingTags.Contains(other.gameObject.tag)) {
            return;
        }

        Despawn(TriggeredSpawner.EventType.OnTriggerEnter);
    }

    void OnTriggerExit(Collider other) {
        // check filters for matches if turned on
        if (!triggerExitSpec.eventEnabled) {
            return;
        }

        if (triggerExitSpec.useLayerFilter && !triggerExitSpec.matchingLayers.Contains(other.gameObject.layer)) {
            return;
        }

        if (triggerExitSpec.useTagFilter && !triggerExitSpec.matchingTags.Contains(other.gameObject.tag)) {
            return;
        }

        Despawn(TriggeredSpawner.EventType.OnTriggerExit);
    }

#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2
    // not supported
#else
		void OnCollisionEnter2D(Collision2D collision) {
			// check filters for matches if turned on
			if (!collision2dSpec.eventEnabled) {
				return;
			}
			
			if (collision2dSpec.useLayerFilter && !collision2dSpec.matchingLayers.Contains(collision.gameObject.layer)) {
				return;
			}
			
			if (collision2dSpec.useTagFilter && !collision2dSpec.matchingTags.Contains(collision.gameObject.tag)) {
				return;
			}
			
			Despawn(TriggeredSpawner.EventType.OnCollision2D);
		}
	
		void OnTriggerEnter2D(Collider2D other) {
			// check filters for matches if turned on
			if (!triggerEnter2dSpec.eventEnabled) {
				return;
			}
			
			if (triggerEnter2dSpec.useLayerFilter && !triggerEnter2dSpec.matchingLayers.Contains(other.gameObject.layer)) {
				return;
			}
			
			if (triggerEnter2dSpec.useTagFilter && !triggerEnter2dSpec.matchingTags.Contains(other.gameObject.tag)) {
				return;
			}
			
			Despawn(TriggeredSpawner.EventType.OnTriggerEnter2D);
		}
		
		void OnTriggerExit2D(Collider2D other) {
			// check filters for matches if turned on
			if (!triggerExit2dSpec.eventEnabled) {
				return;
			}
			
			if (triggerExit2dSpec.useLayerFilter && !triggerExit2dSpec.matchingLayers.Contains(other.gameObject.layer)) {
				return;
			}
			
			if (triggerExit2dSpec.useTagFilter && !triggerExit2dSpec.matchingTags.Contains(other.gameObject.tag)) {
				return;
			}
			
			Despawn(TriggeredSpawner.EventType.OnTriggerExit2D);
		}
#endif

    private void Despawn(TriggeredSpawner.EventType eType) {
		if (LevelSettings.AppIsShuttingDown || isDespawning) {
            return;
        }

        isDespawning = true;

        if (listener != null) {
            listener.Despawning(eType, this.trans);
        }

        PoolBoss.Despawn(this.trans);
    }
}
