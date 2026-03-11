using UnityEngine;
using UnityEngine.AI;

public class GroundEnemy : EnemyBrain
{
    private DayNightCycleManager dayNightCycle;

    [Header("Points")]
    [SerializeField] private int pointsWorth = 1;
    internal int EnemyPoints => pointsWorth * (int)Difficulty.DifficultyMultiplier;

    [Header("Behaviour")]
    [SerializeField] private bool speedUpWhenTargetingMain;
    [SerializeField] private float onCatchMainTargetSpeedBoost = 2.5f;
    internal float initSpeed, initAngularSpeed, initAcceleration, speedModifier = 1;

    public override void OnLogicalStart()
    {
        OnBehaviourStart();
        initSpeed = agent.speed - Random.Range(0, 0.25f);
        initAngularSpeed = agent.angularSpeed;
        initAcceleration = agent.acceleration;
        dayNightCycle = DayNightCycleManager.Instance;
    }

    public override void OnLogicalTick()
    {
        if (agent == null || !agent.isActiveAndEnabled) return;

        if (speedUpWhenTargetingMain)
            agent.speed = ((target == mainTarget) ? initSpeed * onCatchMainTargetSpeedBoost : initSpeed) * speedModifier;
        else
            agent.speed = initSpeed * speedModifier;

        agent.angularSpeed = initAngularSpeed * speedModifier;
        agent.acceleration = initAcceleration * speedModifier;

        if (distanation != Vector3.zero && (!agent.hasPath || agent.destination != distanation))
            agent.SetDestination(distanation);

        if (dayNightCycle != null && dayNightCycle.currentState == DayNightCycleManager.CycleState.Day)
        {
            AIManager.UnRegister(this);
            EnemyPool.Instance.Return(this);
            return;
        }

        OnBehaviourTick();
    }

    public override void OnSpawn()
    {
        speedModifier = 1;
        GetComponent<Damagable>()?.ResetHealth();
        AIManager.Register(this);
    }

    public override void OnDespawn()
    {
        AIManager.UnRegister(this);
        agent.ResetPath();
        agent.velocity = Vector3.zero;
    }

    public void TickDeath()
    {
        if (dayNightCycle != null && dayNightCycle.currentState == DayNightCycleManager.CycleState.Night)
        {
            PointsManager.Instance.GivePoints(pointsWorth);
            WaveManager.Instance.OnEnemyDefeated(gameObject.name);
        }
    }

    public virtual void OnBehaviourTick() { }
    public virtual void OnBehaviourStart() { }
}