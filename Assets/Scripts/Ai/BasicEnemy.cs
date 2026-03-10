using Sortify;
using UnityEngine;

public class BasicEnemy : GroundEnemy
{
    private Animator animator;

    [Header("Attacking")]
    private bool canAttack = true;
    [SerializeField, ReadOnly] private float attackCooldown = 1;
    [SerializeField] private float attackRange = 1;
    [SerializeField] private int attackDamage = 1;

    public override void OnBehaviourStart()
    {
        animator = GetComponentInChildren<Animator>();

        attackDamage *= Difficulty.DifficultyMultiplier;
    }

    public override void OnBehaviourTick()
    {
        HasReachedTarget();
        Animations();
    }

    public override void OnSpawn()
    {
        base.OnSpawn();
        canAttack = true;
        CancelInvoke(nameof(PrepareAttack));
    }

    public override void OnDespawn()
    {
        base.OnDespawn();
        CancelInvoke(nameof(PrepareAttack));
        canAttack = true;
    }

    private void Animations()
    {
        if (animator == null) return;
    
        float speedPercent = agent.velocity.magnitude / initSpeed;
        animator.SetBool("Moving", speedPercent > 0.15f);
        animator.speed = Mathf.Max(1f, speedPercent);
    }

    private void HasReachedTarget()
    {
        if (target == null) return;
        if (!agent.isOnNavMesh) return;
        if (agent.remainingDistance > agent.stoppingDistance) return;

        if (Vector3.Distance(transform.position, target.position) <= attackRange && canAttack)
            Attack();
    }

    private void Attack()
    {
        animator.SetTrigger("Attack");

        if (target.TryGetComponent(out IDamagable damagable))
            damagable.Damage(attackDamage);

        canAttack = false;
        Invoke(nameof(PrepareAttack), attackCooldown);
    }

    private void PrepareAttack() => canAttack = true;
}