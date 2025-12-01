using System.Linq;  
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;

public class BasicEnemy : Enemy
{
    [Header("Attacking")]
    private bool canAttack = true;
    [SerializeField] private float attackCooldown = 1;
    [SerializeField] private LayerMask unAttackableLayers;
    [SerializeField] private float attackRange = 1;
    [SerializeField] private int attackDamage = 1;

    public override void OnUpdate()
    {
        HasReachedTarget();
    }

    public void HasReachedTarget()
    {
        if(target == null) 
            return;
        
        Vector3 position = transform.forward * attackRange;
        if(Physics.CheckSphere(position, attackRange, ~unAttackableLayers) && canAttack)
        {
            Attack();
        }
    }

    private void Attack()
    {
        if(target.TryGetComponent(out IBreakable building))
        {
            building.Damage(gameObject, attackDamage, BreakableType.Buildings, 1);
        }

        canAttack = false;
        DOVirtual.DelayedCall(attackCooldown, () => {canAttack = true;});
    }

    void OnDrawGizmosSelected()
    {
        Vector3 position = transform.forward * attackRange;
        Gizmos.DrawWireSphere(position, attackRange);
    }
}