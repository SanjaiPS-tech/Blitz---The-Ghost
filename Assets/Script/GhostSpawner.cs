// GhostSpawner.cs
// This script spawns enemy ghosts and respawns them when they die.
// Attach this script to an empty GameObject in the scene (e.g., "GhostSpawner").
// Assign the Enemy prefab in the inspector. The spawned enemies will have a reference back to this spawner.

using UnityEngine;
using System.Collections;

public class GhostSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("Prefab of the enemy ghost to spawn. Must have EnemyAI component attached.")]
    public GameObject enemyPrefab;

    [Tooltip("Initial number of ghosts to spawn in the scene.")]
    public int initialSpawnCount = 1;
public float spawnDelay = 2f; // seconds delay before respawn

    [Tooltip("Optional spawn points. If empty, enemies will spawn at the spawner's position.")]
    public Transform[] spawnPoints;

    private void Awake()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("GhostSpawner: enemyPrefab is not assigned.");
            return;
        }
    }

    private void Start()
    {
        // Spawn the initial set of ghosts.
        for (int i = 0; i < initialSpawnCount; i++)
        {
            SpawnGhost();
        }
    }

    // Called by EnemyAI when an enemy dies.
    public void OnEnemyDeath(EnemyAI deadEnemy)
    {
        // You could add death effects here before respawning.
        StartCoroutine(RespawnAfterDelay());
    }

    // Spawns a new ghost at a random spawn point (or at the spawner's position if none defined).
    public void SpawnGhost()
    {
        Transform spawnTransform = this.transform;
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            spawnTransform = spawnPoints[Random.Range(0, spawnPoints.Length)];
        }

        GameObject newGhost = Instantiate(enemyPrefab, spawnTransform.position, spawnTransform.rotation);

        // Assign the spawner reference so the new ghost can notify us on death.
        EnemyAI enemyAI = newGhost.GetComponent<EnemyAI>();
        if (enemyAI != null)
        {
            enemyAI.spawner = this;
        }
        else
        {
            Debug.LogWarning("Spawned prefab does not contain EnemyAI component.");
        }
    }
    
    private IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(spawnDelay);
        SpawnGhost();
    }
}
