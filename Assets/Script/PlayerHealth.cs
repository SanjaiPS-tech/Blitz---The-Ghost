// PlayerHealth.cs
// Simple health component for the player character. Attach this script to the player GameObject.
// It provides a TakeDamage(int amount) method that EnemyAI can call.

using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("Maximum health points for the player.")]
    public int maxHealth = 100;

    private int _currentHealth;

    void Awake()
    {
        _currentHealth = maxHealth;
    }

    // Called by enemies or other sources to apply damage.
    public void TakeDamage(int amount)
    {
        _currentHealth -= amount;
        _currentHealth = Mathf.Max(_currentHealth, 0);
        Debug.Log($"Player took {amount} damage, remaining health: {_currentHealth}");

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player died.");
        // Add death handling here (disable controls, show UI, respawn, etc.)
        // For now we just destroy the player GameObject.
        Destroy(gameObject);
    }
}
