using UnityEngine;
using UnityEngine.AI;

public class Enemy : EnemyBrain
{
    [Header("Points")]
    [SerializeField] private int pointsWorth = 1;
    internal int EnemyPoints => pointsWorth * Difficulty.DifficultyMultiplier;

    public override void OnTick()
    {
        if(distanation != Vector3.zero && (!agent.hasPath || agent.destination != distanation))
        {
            agent.SetDestination(distanation);
        }
    }
}