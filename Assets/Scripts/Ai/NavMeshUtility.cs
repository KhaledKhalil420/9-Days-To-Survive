using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public static class NavMeshUtility
{
    static readonly NavMeshPath path = new NavMeshPath();
    static readonly NavMeshPath path2 = new NavMeshPath();
    static readonly List<Transform> reachableTargets = new List<Transform>(15);

    public static Transform GetTarget(Transform seeker, Transform target, LayerMask targetLayers, float searchArea)
    {
        NavMesh.CalculatePath(seeker.position, target.position, NavMesh.AllAreas, path);

        if(path.status == NavMeshPathStatus.PathComplete)
        {
            return target;
        }
        else  
        {            
            reachableTargets.Clear();
            
            Collider[] colliders = new Collider[15];
            int count = Physics.OverlapSphereNonAlloc(target.position, searchArea, colliders, targetLayers);
            
            for (int i = 0; i < count; i++)
            {
                Collider col = colliders[i];
                if(col == null) continue;
                
                if(col.TryGetComponent(out Target otherTarget))
                {
                    if(NavMesh.SamplePosition(otherTarget.transform.position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                    {
                        NavMesh.CalculatePath(seeker.position, hit.position, NavMesh.AllAreas, path2);

                        if(path2.status == NavMeshPathStatus.PathComplete)
                        {
                            reachableTargets.Add(otherTarget.transform);
                        }
                    }
                }
            }
            
            if(reachableTargets.Count > 0)
            {
                return reachableTargets[Random.Range(0, reachableTargets.Count)];
            }
        }

        return null;
    }
}