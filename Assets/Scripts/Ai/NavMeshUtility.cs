using UnityEngine;
using UnityEngine.AI;

public static class NavMeshUtility
{
    // reused to avoid GC
    static readonly NavMeshPath s_path = new NavMeshPath();

    /// <summary>
    /// Returns true if there's a blocker between from->to.
    /// If false, path is complete/reachable.
    /// If true, hitPosition contains the NavMeshHit position where the NavMesh blocks (use that to search for Build).
    /// </summary>
    public static bool TryGetBlocker(Vector3 from, Vector3 to, out Vector3 hitPosition)
    {
        if (NavMesh.CalculatePath(from, to, NavMesh.AllAreas, s_path) &&
            s_path.status == NavMeshPathStatus.PathComplete)
        {
            hitPosition = Vector3.zero;
            return false;
        }

        if (NavMesh.Raycast(from, to, out var hit, NavMesh.AllAreas))
        {
            hitPosition = hit.position;
            return true;
        }

        // fallback: no ray hit but CalculatePath failed (rare). Return approximate point along direction
        hitPosition = from + (to - from).normalized * Mathf.Min(1f, Vector3.Distance(from, to));
        return true;
    }
}
