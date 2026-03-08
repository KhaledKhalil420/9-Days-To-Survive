using Sortify;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBrain : MonoBehaviour
{
    [Header("Pathfinding")]
    [SerializeField] internal NavMeshAgent agent;
    [SerializeField, Tooltip("Main target tag")] 
    private string TargetTag = "Player";

    [SerializeField] private float searchArea = 15f;

    [SerializeField, Tooltip("When attacking something, lose interest and attack something else after..")]
    private float loseInterestTime = 4f;

    [SerializeField, Tooltip("How often does the path get calculated")]
    private float pathRefreshInterval = 0.5f;

    private float loseInterestTimer;
    private float pathRefreshTimer;
    internal  bool refreshPath = true;

    [ReadOnly, SerializeField] internal Transform mainTarget, target;
    private LayerMask unDetectableLayers;
    internal Vector3 distanation;

    private void Start()
    {
        AIManager.Register(this);
        unDetectableLayers = AIManager.UnDetectableLayers;
        mainTarget = GameObject.FindWithTag(TargetTag).transform;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        pathRefreshTimer = Random.Range(0f, pathRefreshInterval);

        OnLogicalStart();
    }

    public void TickBrain(float delta)
    {
        UpdatePath(delta);
        OnLogicalTick();
    }

    private void UpdatePath(float delta)
    {
        if(!refreshPath)
            return;

        pathRefreshTimer += delta;

        if (pathRefreshTimer < pathRefreshInterval) return;
        pathRefreshTimer = 0f;

        if (target == null)
        {
            GetTarget();
            loseInterestTimer = 0f;
        }
        else
        {
            distanation = target.position;
            loseInterestTimer += pathRefreshInterval;

            if (loseInterestTimer >= loseInterestTime)
            {
                loseInterestTimer = 0f;
                GetTarget();
            }
        }
    }

    internal void GetTarget()
    {
        target = NavMeshUtility.GetTarget(transform, mainTarget, ~unDetectableLayers, searchArea);
    }

    private void OnDestroy()
    {
        AIManager.UnRegister(this);
    }

    public virtual void OnLogicalTick() { }
    public virtual void OnLogicalStart() { }
    public virtual void OnSpawn() { }
    public virtual void OnDespawn() { }
}