// EnemyAI.cs
// This script controls an enemy that uses NavMeshAgent to track the player, attack, and report its death to a spawner.
// Attach this script to your enemy prefab. Ensure the prefab has a NavMeshAgent component and a Collider.

using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    // Reference to the NavMeshAgent component
    private NavMeshAgent _agent;

    // The player transform to chase. Assigned at runtime by finding the object with the "Player" tag.
    private Transform _player;

    // Attack settings
    [Header("Attack Settings")]
    [Tooltip("Distance at which the enemy can attack the player.")]
    public float attackRange = 2f;

    [Tooltip("Damage dealt per attack.")]
    public int damage = 10;

    [Tooltip("Time between attacks.")]
    public float attackCooldown = 1.5f;
    private float _nextAttackTime;

    // Health settings
    [Header("Health Settings")]
    public int maxHealth = 100;
    private int _currentHealth;

    // Reference to the spawner that should be notified when this enemy dies
    [Header("Spawner Reference")]
    [Tooltip("Assign the GhostSpawner component that will respawn this enemy.")]
    public GhostSpawner spawner;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        if (_agent == null)
        {
            Debug.LogError("EnemyAI requires a NavMeshAgent component.");
        }
        _currentHealth = maxHealth;
    }

    void Start()
    {
        // Find the player by tag. Ensure your player GameObject is tagged "Player".
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _player = playerObj.transform;
        }
        else
        {
            Debug.LogError("Player with tag 'Player' not found in the scene.");
        }
    }

    void Update()
    {
        if (_player == null) return;

        // Move towards the player using NavMeshAgent
        _agent.SetDestination(_player.position);

        // Check attack range
        float distanceToPlayer = Vector3.Distance(transform.position, _player.position);
        if (distanceToPlayer <= attackRange && Time.time >= _nextAttackTime)
        {
            Attack();
            _nextAttackTime = Time.time + attackCooldown;
        }
    }

    private void Attack()
    {
        // Simple damage logic - you can replace this with your own combat system.
        var playerHealth = _player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }
        else
        {
            Debug.LogWarning("PlayerHealth component not found on player. Attack has no effect.");
        }
    }

    // Public method that other scripts (e.g., projectile or trap) can call to apply damage.
    public void TakeDamage(int amount)
    {
        _currentHealth -= amount;
        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Notify the spawner that this enemy died so it can spawn a new one.
        if (spawner != null)
        {
            spawner.OnEnemyDeath(this);
        }
        else
        {
            Debug.LogWarning("Spawner reference missing on EnemyAI. No respawn will occur.");
        }

        // Optional: play death animation / particles before destroying.
        // For now we just destroy the GameObject.
        Destroy(gameObject);
    }
}
