using DG.Tweening;
using UnityEngine;

public class BasicEnemy : GroundEnemy
{
    private Animator animator;

    [Header("Attacking")]
    private bool canAttack = true;
    [SerializeField] private float attackCooldown = 1;
    [SerializeField] private float attackRange = 1;
    [SerializeField] private int attackDamage = 1;

    public override void OnBehaviourStart()
    {
        animator = GetComponentInChildren<Animator>();
    }

    public override void OnBehaviourTick()
    {
        HasReachedTarget();
        Animations();
    }

    //Reset attack state when pulled from pool
    public override void OnSpawn()
    {
        base.OnSpawn();
        canAttack = true;
        CancelInvoke(nameof(PrepareAttack));
    }

    //Clean up any pending invokes when returned to pool
    public override void OnDespawn()
    {
        base.OnDespawn();
        CancelInvoke(nameof(PrepareAttack));
        canAttack = true;
    }

    private void Animations()
    {
        float speedPercent = agent.velocity.magnitude / agent.speed;
        animator.SetBool("Moving", speedPercent > 0.15f);
        animator.speed = Mathf.Max(1f, speedPercent);
    }


    public void HasReachedTarget()
    {
        if(target == null) 
            return;
            
        if(!agent.isOnNavMesh) 
            return;

        if(agent.remainingDistance > agent.stoppingDistance) 
            return;
        
        //Check if target is within attack range
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        
        if(distanceToTarget <= attackRange && canAttack)
        {
            Attack();
        }
    }

    private void Attack()
    {
        animator.SetTrigger("Attack");

        if(target.TryGetComponent(out IDamagable damagable))
        {
            damagable.Damage(attackDamage);
        }

        canAttack = false;
        Invoke(nameof(PrepareAttack), attackCooldown);
    }

    private void PrepareAttack()
    {
        canAttack = true;
    }
}