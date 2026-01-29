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
    [ReadOnly] internal Transform mainTarget, target;
    private LayerMask unDetectableLayers;
    internal Vector3 distanation;

    private void Start()
    {
        AIManager.Register(this);
        unDetectableLayers = AIManager.UnDetectableLayers;
        mainTarget = GameObject.FindWithTag(TargetTag).transform;

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
        }
    }
    
    private void GetTarget()
    {
        Debug.Log("Searching for a target...");
        target = NavMeshUtility.GetTarget(transform, mainTarget, ~unDetectableLayers, searchArea);
    }


    public virtual void OnLogicalTick()
    {
        
    }

    public virtual void OnLogicalStart()
    {
        
    }
}