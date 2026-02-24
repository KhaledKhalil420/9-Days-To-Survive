using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;
using DG.Tweening;
using Sortify;

public class Damagable : MonoBehaviour, IDamagable
{
    public UnityEvent OnDamageEvent, OnDeathEvent;

    [SerializeField] private bool destroyOnDeath = true, scaleWithDifficulty = true, isEnemy = true, doNumberEffect = true, poolObject = false, doKnockback = true;

    [Header("Health")]
    [SerializeField] private float MaxHealth = 5;
    [ReadOnly] private float currentHealth;

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 6f;
    [SerializeField] private float knockbackDuration = 0.15f;

    private NavMeshAgent agent;
    private Tween knockbackTween;

    void Awake() => agent = GetComponent<NavMeshAgent>();

    void Start()
    {
        if (scaleWithDifficulty)
            MaxHealth *= 1 + (Difficulty.DifficultyMultiplier - 1) * 0.2f;
        currentHealth = MaxHealth;
    }

    public void ResetHealth() => currentHealth = MaxHealth;

    public void Damage(float damage)
    {
        currentHealth -= damage;
        OnDamageEvent.Invoke();

        if (doNumberEffect)
            DamageNumber.Spawn(damage, transform.position);

        if (doKnockback && agent != null)
            ApplyKnockback((transform.position - Player.inventory.transform.position).normalized);

        if (currentHealth > 0) return;

        // Kill tween BEFORE death so OnComplete never fires on a dead agent
        knockbackTween?.Kill();
        ResetAgent();

        OnDeathEvent.Invoke();
        if (isEnemy) UpgradeManager.Instance.OnEnemyDeath?.Invoke();

        if (destroyOnDeath)
        {
            if (poolObject) EnemyPool.Instance.Return(GetComponent<GroundEnemy>());
            else Destroy(gameObject);
        }
    }

    void ApplyKnockback(Vector3 dir)
    {
        knockbackTween?.Kill();

        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.updatePosition = false;

        knockbackTween = DOVirtual.Float(knockbackForce, 0f, knockbackDuration, force =>
            transform.position += dir * force * Time.deltaTime)
            .SetEase(Ease.OutQuad)
            .OnComplete(ResetAgent);
    }

    void ResetAgent()
    {
        if (agent == null || !agent.isOnNavMesh) return;

        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2f, agent.areaMask))
            agent.Warp(hit.position);

        agent.updatePosition = true;
        agent.isStopped = false;
    }
}