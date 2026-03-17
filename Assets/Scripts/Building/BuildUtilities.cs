using UnityEngine;

public static class BuildUtilities
{
    //spherecast from camera to find a surface
    public static bool TryGetHit(Transform camera, float radius, float maxDistance, LayerMask layers, out RaycastHit hit)
    {
        return Physics.SphereCast(camera.position, radius, camera.forward, out hit, maxDistance, layers);
    }

    //figure out where the ghost should go
    public static Vector3 CalculatePosition(RaycastHit hit, Building building, GameObject ghostObj, float rotation, float snapDistance, out bool isSnapped)
    {
        isSnapped = false;

        if (building == null || !building.usesPivots)
            return hit.point;

        //sit building on top of the ground using its collider height
        BoxCollider col = building.GetComponent<BoxCollider>();
        float halfHeight = col != null ? col.size.y * 0.5f : 0.5f;
        Vector3 groundPos = hit.point + Vector3.up * halfHeight;

        //not hitting a placed building? stay on ground
        Building target = hit.collider.GetComponent<Building>();
        if (target == null || target.pivots.Count == 0)
            return groundPos;

        //try snapping pivots together
        Vector3 snapped = FindSnapPosition(building, target, ghostObj, rotation, snapDistance, hit.point, out bool didSnap);
        isSnapped = didSnap;
        return didSnap ? snapped : groundPos;
    }

    //simple overlap check — all buildings are squares so one box is enough
    public static bool IsPositionValid(GameObject ghostObj, Building building)
    {
        BoxCollider col = building.GetComponent<BoxCollider>();
        if (col == null) return true;

        Vector3 center = ghostObj.transform.TransformPoint(col.center);

        //shrink slightly so perfectly touching buildings don't count as blocked
        Vector3 halfExtents = col.size * 0.5f - Vector3.one * 0.05f;

        Collider[] hits = Physics.OverlapBox(center, halfExtents, ghostObj.transform.rotation, ~0, QueryTriggerInteraction.Ignore);

        foreach (var hit in hits)
        {
            if (hit.transform.IsChildOf(ghostObj.transform)) continue;
            if (hit.gameObject.layer == LayerMask.NameToLayer("Ground")) continue;
            if (hit.CompareTag("Build")) continue; //touching other buildings is fine

            return false;
        }

        return true;
    }

    //move ghost so MY closest pivot lands on TARGET closest pivot
    private static Vector3 FindSnapPosition(Building placing, Building target, GameObject ghostObj, float rotation, float snapDistance, Vector3 hitPoint, out bool snapped)
    {
        snapped = false;
        Vector3 best = hitPoint;
        float closestDist = float.MaxValue;

        Quaternion ghostRot = Quaternion.Euler(0, rotation, 0);

        foreach (Transform targetPivot in target.pivots)
        {
            //too far from where we're aiming? skip
            if (Vector3.Distance(hitPoint, targetPivot.position) > snapDistance) continue;

            foreach (Transform myPivot in placing.pivots)
            {
                //where my pivot would land if ghost sits at current position
                Vector3 myPivotOffset = ghostRot * myPivot.localPosition;
                float dist = Vector3.Distance(ghostObj.transform.position + myPivotOffset, targetPivot.position);

                if (dist >= closestDist) continue;

                closestDist = dist;
                //offset ghost so my pivot sits exactly on the target pivot
                best = targetPivot.position - myPivotOffset;
                snapped = true;
            }
        }

        return best;
    }
}