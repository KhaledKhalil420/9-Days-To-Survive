using UnityEngine;
using UnityEngine.AI;

public class GroundEnemy : EnemyBrain
{
    [Header("Points")]
    [SerializeField] private int pointsWorth = 1;
    internal int EnemyPoints => pointsWorth * Difficulty.DifficultyMultiplier;

    public override void OnLogicalTick()
    {
        if(distanation != Vector3.zero && (!agent.hasPath || agent.destination != distanation))
        {
            agent.SetDestination(distanation);
        }

        OnTick();
    }

    public virtual void OnTick()
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