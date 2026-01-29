using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Reusable Damagable Component
/// </summary>
public class Damagable : MonoBehaviour, IDamagable
{
    public UnityEvent OnDamageEvent, OnDeathEvent;
    [SerializeField] private bool destroyOnDeath = true, scaleWithDifficulty = true;
    [SerializeField] private float MaxHeatlh = 5;
    [ReadOnly] private float currentHealth;

    void Start()
    {
        MaxHeatlh *= 1 + (Difficulty.DifficultyMultiplier - 1) * 0.2f;
        currentHealth = MaxHeatlh;
    }

    public void Damage(float damage)
    {
        currentHealth--;
        OnDamageEvent.Invoke();

        if(currentHealth <= 0)
        {
            OnDeathEvent.Invoke();   

            if(destroyOnDeath)
            {
                Destroy(gameObject);
            }
        }
    }
}
