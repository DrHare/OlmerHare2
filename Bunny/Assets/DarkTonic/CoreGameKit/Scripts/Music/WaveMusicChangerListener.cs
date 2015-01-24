using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("Dark Tonic/Core GameKit/Listeners/Wave Music Changer Listener")]
public class WaveMusicChangerListener : MonoBehaviour {
	void Reset() {
		var src = this.GetComponent<WaveMusicChanger>();
		if (src != null) {
			src.listener = this;
		}
	}

	public virtual void MusicChanging(LevelWaveMusicSettings musicSettings) {
		// your code here.
	}
}
