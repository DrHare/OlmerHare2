using UnityEngine;
using System.Collections;

[AddComponentMenu("Dark Tonic/Core GameKit/Combat/Killable Child Collision")]
public class KillableChildCollision : MonoBehaviour {
    public Killable killable = null;

    private bool isValid = true;

    private Killable KillableToAlert {
        get {
            if (killable != null) {
                return killable;
            }

            if (this.transform.parent != null) {
                var parentKill = this.transform.parent.GetComponent<Killable>();

                if (parentKill != null) {
                    killable = parentKill;
                }
            }

            if (killable == null) {
                LevelSettings.LogIfNew("Could not locate Killable to alert from KillableChildCollision script on GameObject '" + this.name + "'.");
                isValid = false;
                return null;
            }

            return killable;
        }
    }

    void OnCollisionEnter(Collision collision) {
        if (!isValid) {
            return;
        }

        var kill = KillableToAlert;
        if (!isValid) {
            return;
        }

        kill.CollisionEnter(collision);
    }

    void OnTriggerEnter(Collider other) {
        if (!isValid) {
            return;
        }

        var kill = KillableToAlert;
        if (!isValid) {
            return;
        }

        kill.TriggerEnter(other);
    }

#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2
    // not supported
#else
		void OnCollisionEnter2D(Collision2D coll) {
			if (!isValid) {
				return;
			}
			
			KillableToAlert.CollisionEnter2D(coll);
		}
	
		void OnTriggerEnter2D(Collider2D other) {
			if (!isValid) {
				return;
			}
			
			KillableToAlert.TriggerEnter2D(other);
		}
#endif
}
