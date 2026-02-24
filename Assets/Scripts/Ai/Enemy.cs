using UnityEngine;
using UnityEngine.AI;

public class GroundEnemy : EnemyBrain
{
    [Header("Points")]
    [SerializeField] private int pointsWorth = 1;
    internal int EnemyPoints => pointsWorth * Difficulty.DifficultyMultiplier;

    [Header("Behaviour")]
    [SerializeField] private bool speedUpWhenTargetingMain;
    [SerializeField] private float onCatchMainTargetSpeedBoost = 2.5f;
    private float initSpeed;

    private DayNightCycleManager dayNightCycle;

    public override void OnLogicalStart()
    {
        OnBehaviourStart();
        initSpeed = agent.speed;
        dayNightCycle = DayNightCycleManager.Instance;
    }

    public override void OnLogicalTick()
    {
        if (agent == null || !agent.isActiveAndEnabled) return;

        if (speedUpWhenTargetingMain)
            agent.speed = (target == mainTarget) ? initSpeed * onCatchMainTargetSpeedBoost : initSpeed;

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
            WaveManager.Instance.OnEnemyDefeated();
    }

    public virtual void OnBehaviourTick() { }
    public virtual void OnBehaviourStart() { }
}