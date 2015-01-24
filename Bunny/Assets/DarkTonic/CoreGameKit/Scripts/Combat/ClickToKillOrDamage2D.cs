using UnityEngine;
using System.Collections;

#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2
// not supported
#else
[AddComponentMenu("Dark Tonic/Core GameKit/Combat/Click To Kill Or Damage 2D")]
public class ClickToKillOrDamage2D : MonoBehaviour {
    public bool killObjects = true;
    public int damagePointsToInflict = 1;

    void Update() {
        var mouseDown = Input.GetMouseButtonDown(0);
        var fingerDown = Input.touches.Length == 1 && Input.touches[0].phase == TouchPhase.Began;

        if (mouseDown || fingerDown) {
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			RaycastHit2D hit2D = Physics2D.Raycast(ray.origin, ray.direction);
			if (hit2D.collider != null) {
                KillOrDamage(hit2D.collider.gameObject);
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
#endif