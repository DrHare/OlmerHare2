using UnityEngine;
using System;
using System.Collections.Generic;

[AddComponentMenu("Dark Tonic/Core GameKit/Listeners/Level Settings Listener")]
public class LevelSettingsListener : MonoBehaviour {
    public string sourceTransName;

    void Reset() {
        var src = this.GetComponent<LevelSettings>();
        if (src != null) {
            src.listener = this;
            this.sourceTransName = this.name;
        }
    }

    public virtual void WaveItemsRemainingChanged(int waveItemsRemaining) {
        // your code here.
    }

    public virtual void WaveTimeRemainingChanged(int secondsRemaining) {
        // your code here.
    }

    public virtual void Win() {
        // your code here.
    }
	
	public virtual void Lose() {
		// your code here.
	}
	
    public virtual void GameOver(bool hasWon) {
        // your code here.
    }

    public virtual void WaveStarted(LevelWave levelWaveInfo) {
        // your code here.
    }

    public virtual void WaveEnded(LevelWave levelWaveInfo) {
        // your code here.
    }

    public virtual void WaveRestarted(LevelWave levelWaveInf) {
        // your code here.
    }

    public virtual void WaveCompleteBonusesStart(List<WorldVariableModifier> bonusModifiers) {
        // your code here.
    }

    public virtual void WaveEndedEarly(LevelWave levelWaveInfo) {
        // your code here.
    }

    public virtual void WaveSkipped(LevelWave levelWaveInfo) {
        // your code here.
    }
}
