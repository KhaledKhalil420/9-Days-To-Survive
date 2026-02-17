using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Reusable Damagable Component
/// </summary>
public class Damagable : MonoBehaviour, IDamagable
{
    public UnityEvent OnDamageEvent, OnDeathEvent;
    [SerializeField] private bool destroyOnDeath = true, scaleWithDifficulty = true, isEnemy = true, doNumberEffect = true;
    [SerializeField] private float MaxHealth = 5;
    [ReadOnly] private float currentHealth;

    void Start()
    {
        if(scaleWithDifficulty)
        MaxHealth *= 1 + (Difficulty.DifficultyMultiplier - 1) * 0.2f;
        currentHealth = MaxHealth;
    }

    public void Damage(float damage)
    {
        currentHealth--;
        OnDamageEvent.Invoke();
        if(doNumberEffect) DamageNumber.Spawn(damage, transform.position);

        if(currentHealth <= 0)
        {
            OnDeathEvent.Invoke();   
            if(isEnemy)
            {
                UpgradeManager.OnEnemyDeath?.Invoke();
            }

            if(destroyOnDeath)
            {
                Destroy(gameObject);
            }
        }
    }
}
