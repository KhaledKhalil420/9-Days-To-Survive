using Sortify;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.AI;


/// <summary>
/// Pathfinding logic for enemies. 
/// </summary>
public class EnemyBrain : MonoBehaviour
{
    [Header("Pathfinding")]
    [SerializeField] internal NavMeshAgent agent;
    [SerializeField] private string TargetTag = "Player";
    [SerializeField] private float searchArea = 15;
    private float loseIntrestTimer = 0;
    [ReadOnly, SerializeField] internal Transform mainTarget, target;
    private LayerMask unDetectableLayers;
    internal Vector3 distanation;

    private void Start()
    {
        AIManager.Register(this);
        unDetectableLayers = AIManager.UnDetectableLayers;
        mainTarget = GameObject.FindWithTag(TargetTag).transform;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;

        OnLogicalStart();
    }

    public void TickBrain()
    {
        //Get path
        UpdatePath();

        //Optional virtual
        OnLogicalTick();
    }

    private void UpdatePath()
    {
        //Try finding a target
        if (target == null)
        {
            GetTarget();
        }
        
        //Set Detination
        else
        {
            distanation = target.position;
            loseIntrestTimer += Time.deltaTime;
        }

        if(loseIntrestTimer > 4)
        {
            loseIntrestTimer = 0;
            GetTarget();
        }
    }
    
    private void GetTarget()
    {
        target = NavMeshUtility.GetTarget(transform, mainTarget, ~unDetectableLayers, searchArea);
    }

    void OnDestroy()
    {
        AIManager.UnRegister(this);
    }

    public virtual void OnLogicalTick()
    {
        
    }

    public virtual void OnLogicalStart()
    {
        
    }
}