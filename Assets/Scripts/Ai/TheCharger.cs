using UnityEngine;
using UnityEngine.AI;

public class TheCharger : GroundEnemy, IInteruptable
{
    private enum ChargerState { Dashing, Backing, Idle }
    [SerializeField] private ChargerState state = ChargerState.Backing;

    [Header("Charge Settings")]
    [SerializeField] private float backOffSpeed = 3f;
    [SerializeField] private float backUpDistance = 4f;
    [SerializeField] private float chargeSpeed = 14f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private int attackDamage = 5;
    [SerializeField] private float cooldown = 1;
    private float coolDownTimer = 0;

    [Header("Audio")]
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip bullCharge;
    [SerializeField] private AudioClip bullAttack;


    private Vector3 chargeDirection;
    private Animator animator;

    public override void OnBehaviourStart()
    {
        animator = GetComponentInChildren<Animator>();
        refreshPath = false;
        
        //Set off backing start
        initSpeed = backOffSpeed;
        agent.updateRotation = false;
        state = ChargerState.Backing;
        FindBackingPosition();
        animator.SetBool("Moving", true);
        animator.SetBool("Charging", false);
        
        Vector3 dir = new Vector3();
        if(mainTarget != null)
        {    
            dir = mainTarget.position - transform.position;
        }

        else
        {
            dir = GameObject.FindWithTag("Player").transform.position - transform.position;

        }
        
        dir.y = 0f;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);
        attackDamage *= (int)Difficulty.DifficultyMultiplier;

        coolDownTimer = cooldown / 2;
    }

    public override void OnBehaviourTick()
    {
        switch (state)
        {
            case ChargerState.Dashing: TickDashing(); break;
            case ChargerState.Backing: TickBacking(); break;
            case ChargerState.Idle: TickIdle(); break;
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
            //Transition to idle
            state = ChargerState.Idle;
            animator.SetBool("Moving", false);
            animator.SetBool("Charging", false);
        }
    }

    private void TickIdle()
    {
        coolDownTimer += Time.deltaTime;
        if(coolDownTimer >= cooldown)
        {
            GetTarget();
            coolDownTimer = 0;

            if(target == null) 
                return;

            //Transition to dashing
            distanation = target != null ? target.position : distanation;
            chargeDirection = (distanation - transform.position).normalized;
            chargeDirection.y = 0f;
            state = ChargerState.Dashing;
            agent.updateRotation = true;
            initSpeed = chargeSpeed;
            animator.SetBool("Moving", true);
            animator.SetBool("Charging", true);
            source.PlayOneShot(bullCharge, 0.25f);
        }
    }

    private void TickDashing()
    {
        initSpeed = chargeSpeed;

        if (!agent.pathPending && agent.remainingDistance < 2.75f)
        {
            initSpeed = backOffSpeed;
            agent.updateRotation = false;
            state = ChargerState.Backing;
            if(Vector3.Distance(transform.position, target.position) < 3) 
            Attack();
            animator.SetTrigger("Attack");
            FindBackingPosition();
            GetTarget();
            animator.SetBool("Moving", true);
            animator.SetBool("Charging", false);
        }
    }

    private void Attack()
    {
        if (target == null) return;
        if (target.TryGetComponent(out IDamagable damagable))
        {
            damagable.Damage(attackDamage);
            source.PlayOneShot(bullAttack, 0.45f);

            if(target.TryGetComponent(out Rigidbody rigidbody))
            {
                Vector3 knockback = chargeDirection * 80f + Vector3.up * 20f;
                rigidbody.AddForce(knockback, ForceMode.Impulse);
            }
        }
    }

    private void FindBackingPosition()
    {
        distanation = transform.position + (-chargeDirection * backUpDistance);
    }

    public override void OnSpawn()
    {
        base.OnSpawn();
        OnBehaviourStart();
    }

    public override void OnDespawn()
    {
        base.OnDespawn();
    }

    public void InterruptCharge()
    {
        int roll = Random.Range(0, 2);

        if(roll != 0) return;
        if (state != ChargerState.Dashing) return;
        initSpeed = backOffSpeed;
        agent.updateRotation = false;
        state = ChargerState.Backing;
        animator.SetTrigger("Attack");
        FindBackingPosition();
        GetTarget();
        animator.SetBool("Moving", true);
        animator.SetBool("Charging", false);
    }

    public void Interupt()
    {
        InterruptCharge();
    }
}