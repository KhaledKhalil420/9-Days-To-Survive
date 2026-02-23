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
        if (speedUpWhenTargetingMain)
        {
            agent.speed = (target == mainTarget) ? initSpeed * onCatchMainTargetSpeedBoost : initSpeed;
        }

        if (distanation != Vector3.zero && (!agent.hasPath || agent.destination != distanation))
        {
            agent.SetDestination(distanation);
        }

        //Destroy during daytime
        if (dayNightCycle != null && dayNightCycle.currentState == DayNightCycleManager.CycleState.Day)
        {
            AIManager.UnRegister(this);
            Destroy(gameObject);
            return;
        }

        OnBehaviourTick();
    }

    public virtual void OnBehaviourTick() { }
    public virtual void OnBehaviourStart() { }

    public void TickDeath()
    {
        if (dayNightCycle != null && dayNightCycle.currentState == DayNightCycleManager.CycleState.Night)
        {
            GameManager.Instance.OnEnemyDefeated();
        }
    }
}