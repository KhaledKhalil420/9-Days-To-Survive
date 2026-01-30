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
            
            Collider[] colliders = new Collider[15];
            Physics.OverlapSphereNonAlloc(target.position, searchArea, colliders, targetLayers);
            
            foreach (Collider col in colliders)
            {
                if(col.TryGetComponent(out Target otherTarget))
                {
                    if(NavMesh.SamplePosition(otherTarget.transform.position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                    {
                        NavMesh.CalculatePath(seeker.position, hit.position, NavMesh.AllAreas, path2);

                        if(path2.status == NavMeshPathStatus.PathComplete)
                        {
                            return otherTarget.transform;
                        }
                    }
                }
            }
        }

        return null;
    }
}