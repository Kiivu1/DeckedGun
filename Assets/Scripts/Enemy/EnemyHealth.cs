using System;
using UnityEngine;

// Vida del enemigo. EnemyHitbox detecta el impacto y llama a TakeDamage.
public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 30f;

    private float currentHealth;
    private bool isDead; // Evita morir dos veces si llegan varias balas en el mismo frame

    // Se lanza al morir. Lo usará el gestor de oleadas para contar las bajas
    public event Action<EnemyHealth> OnDeath;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        Debug.Log($"{name} took {amount} damage ({currentHealth}/{maxHealth})");

        if (currentHealth <= 0f) Die();
    }

    void Die()
    {
        isDead = true;
        OnDeath?.Invoke(this);
        Destroy(gameObject);
    }
}
