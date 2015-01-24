using UnityEngine;
using System.Collections;

[AddComponentMenu("Dark Tonic/Core GameKit/Spawners/Player Spawner")]
public class PlayerSpawner : MonoBehaviour {
    public Transform PlayerPrefab;
    public Transform RespawnParticlePrefab;
    public Vector3 RespawnParticleOffset = Vector3.zero;
    public float RespawnDelay = 1f;
    public Vector3 spawnPosition;
    public bool followPlayer = false;

    private Transform Player;
    private float? nextSpawnTime = null;
    private Vector3 playerPosition;
    private bool isDisabled = false;

    void Start() {
        if (PlayerPrefab == null) {
            LevelSettings.LogIfNew("No Player Prefab is assigned to PlayerSpawner. PlayerSpawn disabled.");
            this.isDisabled = true;
            return;
        }
        if (RespawnDelay < 0) {
            LevelSettings.LogIfNew("Respawn Delay must be a positive number. PlayerSpawn disabled.");
            this.isDisabled = true;
            return;
        }

        this.nextSpawnTime = null;
        this.playerPosition = this.spawnPosition;

        var pl = GameObject.Find(PlayerPrefab.name);
        Player = pl == null ? null : pl.transform;

        if (Player == null) {
            SpawnPlayer();
        }
    }

    void FixedUpdate() {
        if (isDisabled) {
            return;
        }

        if (Player == null || !SpawnUtility.IsActive(Player.gameObject)) {
            if (!this.nextSpawnTime.HasValue) {
                this.nextSpawnTime = Time.time + RespawnDelay;
            } else if (Time.time >= this.nextSpawnTime.Value && !LevelSettings.IsGameOver) {
                SpawnPlayer();
                nextSpawnTime = null;
            }
        } else if (followPlayer) {
            UpdateSpawnPosition(Player.position);
        }
    }

    private void SpawnPlayer() {
        Player = PoolBoss.SpawnOutsidePool(PlayerPrefab, playerPosition, PlayerPrefab.transform.rotation);

        var spawnPos = playerPosition + RespawnParticleOffset;
        if (RespawnParticlePrefab != null) {
            PoolBoss.SpawnInPool(RespawnParticlePrefab, spawnPos, RespawnParticlePrefab.transform.rotation);
        }
    }

    public void UpdateSpawnPosition(Vector3 newPosition) {
        playerPosition = newPosition;
    }
}
