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
    
    public override void OnLogicalStart()
    {
        OnBehaviourStart();

        initSpeed = agent.speed;
    }

    public override void OnLogicalTick()
    {
        if(speedUpWhenTargetingMain)
        {
            if(target == mainTarget) agent.speed = initSpeed * onCatchMainTargetSpeedBoost;
            else agent.speed = initSpeed;
        }

        if(distanation != Vector3.zero && (!agent.hasPath || agent.destination != distanation))
        {
            agent.SetDestination(distanation);
        }

        if(DayNightCycleManager.Instance?.currentState == DayNightCycleManager.CycleState.Day)
        {
            AIManager.UnRegister(this);
            Destroy(gameObject);
        }

        OnBehaviourTick();
    }

    public virtual void OnBehaviourTick()
    {
        
    }

    public virtual void OnBehaviourStart()
    {
        
    }

    public void TickDeath()
    {
        if(DayNightCycleManager.Instance.currentState == DayNightCycleManager.CycleState.Night)
        {
            GameManager.Instance.enemiesDefeated++;
        }
    }
}