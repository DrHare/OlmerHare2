using UnityEngine;
using System.Collections;

[AddComponentMenu("Dark Tonic/Core GameKit/Combat/Killable Child Visibility")]
public class KillableChildVisibility : MonoBehaviour {
    public Killable killableWithRenderer = null;

    private bool isValid = true;

    private Killable KillableToAlert {
        get {
            if (killableWithRenderer != null) {
                return killableWithRenderer;
            }

            if (this.transform.parent != null) {
                var parentKill = this.transform.parent.GetComponent<Killable>();

                if (parentKill != null) {
                    killableWithRenderer = parentKill;
                }
            }

            if (killableWithRenderer == null) {
                LevelSettings.LogIfNew("Could not locate Killable to alert from KillableChildVisibility script on GameObject '" + this.name + "'.");
                isValid = false;
                return null;
            }

            return killableWithRenderer;
        }
    }

    void OnBecameVisible() {
        if (!isValid) {
            return;
        }

        var killable = KillableToAlert;
        if (!isValid) {
            return;
        }

        killable.BecameVisible();
    }

    void OnBecameInvisible() {
        if (!isValid) {
            return;
        }

        var killable = KillableToAlert;
        if (!isValid) {
            return;
        }
        killable.BecameInvisible();
    }
}
