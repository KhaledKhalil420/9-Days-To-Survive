using UnityEngine;
using UnityEngine.AI;

public static class NavMeshUtility
{
    static readonly NavMeshPath path = new NavMeshPath();
    static readonly NavMeshPath path2 = new NavMeshPath();

    
    public static Transform GetTarget(Transform seeker, Transform target, LayerMask targetLayers, float searchArea)
    {
        NavMesh.CalculatePath(seeker.position, target.position, NavMesh.AllAreas, path);

        if(path.status == NavMeshPathStatus.PathComplete)
        {
            return target;
        }
        else  
        {
            Debug.Log("Path not found, getting closest target...");
            
            Collider[] colliders = Physics.OverlapSphere(target.position, searchArea, targetLayers);
            
            foreach (Collider col in colliders)
            {
                if(col.TryGetComponent(out Target otherTarget))
                {
                    // Try to find nearest valid NavMesh point near the target
                    if(NavMesh.SamplePosition(otherTarget.transform.position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                    {
                        NavMesh.CalculatePath(seeker.position, hit.position, NavMesh.AllAreas, path2);
                        Debug.Log("Checking target: " + otherTarget.name + ", Path status: " + path2.status);

                        if(path2.status == NavMeshPathStatus.PathComplete)
                        {
                            Debug.Log("Found valid target: " + otherTarget.name);
                            return otherTarget.transform;
                        }
                    }
                }
            }
            
            Debug.Log("No valid targets found in search area!");
        }

        return null;
    }
}