using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [Header("Pathfinding")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform target;
    [SerializeField] private float detectionSize;
    [SerializeField] private LayerMask unDetectableLayers;
    private int secondInterval = 0;
    private Collider[] results = new Collider[30];

    private void Start()
    {
        AIManager.Register(this);
    }

    public void UpdateBrain()
    {
        //Get path
        UpdatePath();

        //Optional virtual
        OnUpdate();
    }

    private void UpdatePath()
    {
        //Try finding a target
        if (target == null) 
        DetectTargets();

        else
        {
            //Update to find the best target on half intervals
            if(secondInterval == 0)
            {
                secondInterval = 1;
                DetectTargets();
            } 
            else
            {  
                secondInterval = 0;
            }

            //Set Detination
            agent.SetDestination(target.position);
        }
    }
    
    private void DetectTargets()
    {
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, detectionSize, results, ~unDetectableLayers);

        Target best = null;
        for (int i = 0; i < hitCount; i++)
        {
            var t = results[i].GetComponent<Target>();
            if (t == null) continue;

            if (best == null || t.priority > best.priority)
                best = t;
        }

        if (best != null)
            target = best.transform;
    }


    public virtual void OnUpdate()
    {
        
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, detectionSize);
    }
}