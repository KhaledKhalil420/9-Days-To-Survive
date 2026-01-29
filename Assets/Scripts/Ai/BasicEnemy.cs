using DG.Tweening;
using UnityEngine;

public class BasicEnemy : GroundEnemy
{
    [Header("Attacking")]
    private bool canAttack = true;
    [SerializeField] private float attackCooldown = 1;
    [SerializeField] private float attackRange = 1;
    [SerializeField] private int attackDamage = 1;

    public override void OnLogicalStart()
    {
        agent.stoppingDistance = attackRange * 0.8f;
    }

    public override void OnTick()
    {
        HasReachedTarget();
    }

    public void HasReachedTarget()
    {
        if(target == null) 
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
        if(target.TryGetComponent(out IDamagable damagable))
        {
            damagable.Damage(attackDamage);
        }

        canAttack = false;
        DOVirtual.DelayedCall(attackCooldown, () => {canAttack = true;});
    }
}