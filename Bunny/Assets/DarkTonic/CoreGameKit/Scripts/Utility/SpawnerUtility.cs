using UnityEngine;
using System.Collections;

/// <summary>
/// This class is used for various Spawner methods, such as activate and deactivate wave.
/// </summary>
public static class SpawnerUtility {
    /// <summary>
    /// Use this method to activate a wave by Level and Wave # in a Syncro Spawner.
    /// </summary>
    /// <param name="transSpawner">The Transform of the Syncro Spawner.</param>
    /// <param name="levelNumber">The level number.</param>
    /// <param name="waveNumber">The wave number.</param>
    public static void ActivateWave(Transform transSpawner, int levelNumber, int waveNumber) {
        var spawner = transSpawner.GetComponent<WaveSyncroPrefabSpawner>();
        ActivateWave(spawner, levelNumber, waveNumber);
    }

    /// <summary>
    /// Use this method to activate a wave by Level and Wave # in a Syncro Spawner.
    /// </summary>
    /// <param name="transSpawner">The Spawner script of the Syncro Spawner.</param>
    /// <param name="levelNumber">The level number.</param>
    /// <param name="waveNumber">The wave number.</param>
    public static void ActivateWave(WaveSyncroPrefabSpawner spawner, int levelNumber, int waveNumber) {
        ChangeSpawnerWaveStatus(spawner, levelNumber, waveNumber, true);
    }

    /// <summary>
    /// Use this method to deactivate a wave by Level and Wave # in a Syncro Spawner.
    /// </summary>
    /// <param name="transSpawner">The Transform of the Syncro Spawner.</param>
    /// <param name="levelNumber">The level number.</param>
    /// <param name="waveNumber">The wave number.</param>
    public static void DeactivateWave(Transform transSpawner, int levelNumber, int waveNumber) {
        var spawner = transSpawner.GetComponent<WaveSyncroPrefabSpawner>();
        DeactivateWave(spawner, levelNumber, waveNumber);
    }

    /// <summary>
    /// Use this method to deactivate a wave by Level and Wave # in a Syncro Spawner.
    /// </summary>
    /// <param name="transSpawner">The Spawner script of the Syncro Spawner.</param>
    /// <param name="levelNumber">The level number.</param>
    /// <param name="waveNumber">The wave number.</param>
    public static void DeactivateWave(WaveSyncroPrefabSpawner spawner, int levelNumber, int waveNumber) {
        ChangeSpawnerWaveStatus(spawner, levelNumber, waveNumber, false);
    }

    private static void ChangeSpawnerWaveStatus(WaveSyncroPrefabSpawner spawner, int levelNumber, int waveNumber, bool isActivate) {
        var statusText = isActivate ? "activate" : "deactivate";

        if (spawner == null) {
            LevelSettings.LogIfNew(string.Format("Spawner was NULL. Cannot {0} wave# {1} in level# {2}",
                statusText,
                waveNumber,
                levelNumber));
            return;
        }

        foreach (var wave in spawner.waveSpecs) {
            if (wave.SpawnLevelNumber + 1 == levelNumber && wave.SpawnWaveNumber + 1 == waveNumber) {
                if (LevelSettings.IsLoggingOn) {
                    Debug.Log(string.Format("Logging '{0}' in spawner '{1}' for wave# {2}, level# {3}.",
                        statusText,
                        spawner.name,
                        waveNumber,
                        levelNumber));
                }
                wave.enableWave = isActivate;
                return;
            }
        }

        LevelSettings.LogIfNew(string.Format("Could not locate a wave matching wave# {0}, level# {1}, in spawner '{2}'.",
            waveNumber, levelNumber, spawner.name));
    }
}