using UnityEngine;
using UnityEngine.AI;

public class TheCharger : GroundEnemy
{
    private enum ChargerState { Dashing, Backing }
    [SerializeField] private ChargerState state = ChargerState.Backing;

    [Header("Charge Settings")]
    [SerializeField] private float backOffSpeed = 3f;
    [SerializeField] private float backUpDistance = 4f;
    [SerializeField] private float chargeSpeed = 14f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private int attackDamage = 5;

    private Vector3 chargeDirection;
    private Animator animator;

    public override void OnBehaviourStart()
    {
        animator = GetComponentInChildren<Animator>();
        refreshPath = false;
    }

    public override void OnBehaviourTick()
    {
        switch (state)
        {
            case ChargerState.Dashing: TickDashing(); break;
            case ChargerState.Backing: TickBacking(); break;
        }
    }

    private void TickBacking()
    {
        initSpeed = backOffSpeed;

        // Rotate to face target while backing off
        if (target != null)
        {
            Vector3 dir = (target.position - transform.position);
            dir.y = 0f;
            if (dir != Vector3.zero)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * rotationSpeed);
        }

        if (!agent.pathPending && agent.remainingDistance < 1)
        {
            GetTarget();
            distanation = target != null ? target.position : distanation;
            chargeDirection = (distanation - transform.position).normalized;
            chargeDirection.y = 0f;
            state = ChargerState.Dashing;
            agent.updateRotation = true;
            initSpeed = chargeSpeed;
        }
    }

    private void TickDashing()
    {
        initSpeed = chargeSpeed;

        if (!agent.pathPending && agent.remainingDistance < 1)
        {
            initSpeed = backOffSpeed;
            agent.updateRotation = false;
            state = ChargerState.Backing;
            if(Vector3.Distance(transform.position, target.position) > 1) //it doesn't attack fix here thanks
            Attack();
            FindBackingPosition();
            GetTarget();
        }
    }

    private void Attack()
    {
        if (target == null) return;
        if (target.TryGetComponent(out IDamagable damagable))
            damagable.Damage(attackDamage);
    }

    private void FindBackingPosition()
    {
        distanation = transform.position + (-chargeDirection * backUpDistance);
    }

    public override void OnSpawn()
    {
        base.OnSpawn();
        refreshPath = false;
        state = ChargerState.Backing;
        initSpeed = backOffSpeed;
        distanation = transform.position;
    }

    public override void OnDespawn()
    {
        base.OnDespawn();
    }

    public void InterruptCharge()
    {
        if (state != ChargerState.Dashing) return;
        initSpeed = backOffSpeed;
        agent.updateRotation = false;
        state = ChargerState.Backing;
        FindBackingPosition();
    }
}