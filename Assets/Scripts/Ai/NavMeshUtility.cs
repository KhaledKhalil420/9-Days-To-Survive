using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public static class NavMeshUtility
{
    // Cached per-call — safe since everything runs on the main thread sequentially
    private static readonly NavMeshPath path = new NavMeshPath();
    private static readonly NavMeshPath path2 = new NavMeshPath();
    private static readonly Collider[] colliderBuffer = new Collider[15]; // No per-call allocation
    private static readonly List<Transform> reachableTargets = new List<Transform>(15);

    public static Transform GetTarget(Transform seeker, Transform target, LayerMask targetLayers, float searchArea)
    {
        //Check if we have a direct path to the main target first
        NavMesh.CalculatePath(seeker.position, target.position, NavMesh.AllAreas, path);

        if (path.status == NavMeshPathStatus.PathComplete)
            return target;

        //Main target unreachable — find nearest reachable alternative
        reachableTargets.Clear();

        int count = Physics.OverlapSphereNonAlloc(seeker.position, searchArea, colliderBuffer, targetLayers);

        for (int i = 0; i < count; i++)
        {
            Collider col = colliderBuffer[i];
            if (col == null) continue;

            if (!col.TryGetComponent(out Target otherTarget)) continue;

            //Sample nav mesh near the target to get a valid nav point
            if (!NavMesh.SamplePosition(otherTarget.transform.position, out NavMeshHit hit, 5f, NavMesh.AllAreas)) continue;

            NavMesh.CalculatePath(seeker.position, hit.position, NavMesh.AllAreas, path2);

            if (path2.status == NavMeshPathStatus.PathComplete)
                reachableTargets.Add(otherTarget.transform);
        }

        if (reachableTargets.Count > 0)
            return reachableTargets[Random.Range(0, reachableTargets.Count)];

        return null;
    }
}