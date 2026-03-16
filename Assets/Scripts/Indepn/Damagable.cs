using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Reusable Damagable Component
/// </summary>
public class Damagable : MonoBehaviour, IDamagable
{
    public UnityEvent OnDamageEvent, OnDeathEvent;
    [SerializeField] internal bool destroyOnDeath = true, scaleWithDifficulty = true, isEnemy = true, doNumberEffect = true, poolObject = false, doKnockback = false;
    [SerializeField] private Vector3 numberEffectOffset = Vector3.zero;
    [SerializeField] private float MaxHealth = 5;
    [ReadOnly] private float currentHealth;
    private bool isDead = false;

    void Start()
    {
        if(scaleWithDifficulty)
            MaxHealth *= Difficulty.DifficultyMultiplier;
            
        currentHealth = MaxHealth;
    }

    //Called by EnemyPool when the enemy is pulled from the pool for reuse
    public void ResetHealth()
    {
        currentHealth = MaxHealth;
        isDead = false;
    }

    public void Damage(float damage)
    {
        currentHealth -= damage;
        OnDamageEvent.Invoke();
        if(doNumberEffect) DamageNumber.Spawn(damage, transform.position + numberEffectOffset);

        if(currentHealth <= 0 && !isDead)
        {
            isDead = true;
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