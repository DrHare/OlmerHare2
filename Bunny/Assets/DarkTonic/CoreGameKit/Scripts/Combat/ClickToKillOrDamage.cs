using UnityEngine;
using System.Collections;

[AddComponentMenu("Dark Tonic/Core GameKit/Combat/Click To Kill Or Damage")]
public class ClickToKillOrDamage : MonoBehaviour {
    public bool killObjects = true;
    public int damagePointsToInflict = 1;

    void Update() {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        var mouseDown = Input.GetMouseButtonDown(0);
        var fingerDown = Input.touches.Length == 1 && Input.touches[0].phase == TouchPhase.Began;

        if (mouseDown || fingerDown) {
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity)) {
                if (hit.collider == null) {
                    return;
                }

                KillOrDamage(hit.collider.gameObject);
            }
        }
    }

    private void KillOrDamage(GameObject go) {
        var kill = go.GetComponent<Killable>();
        if (kill == null) {
            return;
        }

        if (killObjects) {
            kill.DestroyKillable();
        } else {
            kill.TakeDamage(damagePointsToInflict, null);
        }
    }
}
