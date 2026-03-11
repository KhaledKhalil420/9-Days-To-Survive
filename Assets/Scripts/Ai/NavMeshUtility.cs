using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public static class NavMeshUtility
{
    private static readonly NavMeshPath path = new NavMeshPath();
    private static readonly NavMeshPath path2 = new NavMeshPath();
    private static readonly Collider[] colliderBuffer = new Collider[30];
    private static readonly List<Target> reachableTargets = new List<Target>(30);
    private static readonly List<Target> topPriorityTargets = new List<Target>(30);

    public static Transform GetTarget(Transform seeker, Transform target, LayerMask targetLayers, float searchArea)
    {
        NavMesh.CalculatePath(seeker.position, target.position, NavMesh.AllAreas, path);

        if (path.status == NavMeshPathStatus.PathComplete)
            return target;

        reachableTargets.Clear();

        int count = Physics.OverlapSphereNonAlloc(seeker.position, searchArea, colliderBuffer, targetLayers);

        for (int i = 0; i < count; i++)
        {
            Collider col = colliderBuffer[i];
            if (col == null) continue;

            if (!col.TryGetComponent(out Target otherTarget)) continue;

            if (!NavMesh.SamplePosition(otherTarget.transform.position, out NavMeshHit hit, 5f, NavMesh.AllAreas)) continue;

            NavMesh.CalculatePath(seeker.position, hit.position, NavMesh.AllAreas, path2);

            if (path2.status == NavMeshPathStatus.PathComplete)
                reachableTargets.Add(otherTarget);
        }

        if (reachableTargets.Count == 0)
            return null;

        float highestPriority = float.MinValue;

        for (int i = 0; i < reachableTargets.Count; i++)
        {
            if (reachableTargets[i].priority > highestPriority)
                highestPriority = reachableTargets[i].priority;
        }

        topPriorityTargets.Clear();

        for (int i = 0; i < reachableTargets.Count; i++)
        {
            if (reachableTargets[i].priority == highestPriority)
                topPriorityTargets.Add(reachableTargets[i]);
        }

        return topPriorityTargets[Random.Range(0, topPriorityTargets.Count)].transform;
    }
}