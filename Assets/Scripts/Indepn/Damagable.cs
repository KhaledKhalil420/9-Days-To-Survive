using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Reusable Damagable Component
/// </summary>
public class Damagable : MonoBehaviour, IDamagable
{
    public UnityEvent OnDamageEvent, OnDeathEvent;
    [SerializeField] private bool destroyOnDeath = true, scaleWithDifficulty = true, isEnemy = true, doNumberEffect = true, poolObject = false, doKnockback = false;
    [SerializeField] private float MaxHealth = 5;
    [ReadOnly] private float currentHealth;

    void Start()
    {
        if(scaleWithDifficulty)
        MaxHealth *= 1 + (Difficulty.DifficultyMultiplier - 1) * 0.2f;
        currentHealth = MaxHealth;
    }

    //Called by EnemyPool when the enemy is pulled from the pool for reuse
    public void ResetHealth()
    {
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
                UpgradeManager.Instance.OnEnemyDeath?.Invoke();
            }

            if(destroyOnDeath)
            {
                if(poolObject)
                {
                    //Return to pool instead of destroying
                    EnemyPool.Instance.Return(GetComponent<GroundEnemy>());
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}