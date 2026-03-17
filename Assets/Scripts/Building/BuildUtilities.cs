using UnityEngine;

public static class BuildUtilities
{

    public static bool TryGetHit(Transform camera, float radius, float maxDistance, LayerMask layers, out RaycastHit hit)
    {
        return Physics.SphereCast(camera.position, radius, camera.forward, out hit, maxDistance, layers);
    }

    public static Vector3 CalculatePosition(RaycastHit hit, Building building, MeshFilter meshFilter, GameObject ghostObj, int gridSize, float rotation, float snapDistance, out bool isSnap)
    {
        isSnap = false;
        
        if (building == null || !building.usesPivots)
            return hit.point;

        //Get scale and extents
        float scale = building.affectedByGridSizePosition ? gridSize : 1f;
        Vector3 extents = meshFilter.mesh.bounds.extents * scale;

        //Base position
        Vector3 basePosition = hit.point + Vector3.up * (extents.y - meshFilter.mesh.bounds.center.y);
    
        //Check target building
        Building target = hit.collider.GetComponent<Building>();
        if (target == null || target.pivots.Count == 0)
            return basePosition;
        
        //Find snap position
        Vector3 pos = FindSnapPosition(hit, building, target, ghostObj, rotation, gridSize, snapDistance, out bool snapped);
        isSnap = snapped;
        return pos;
    }

    public static bool IsPositionValid(GameObject ghostObj, Building building)
    {
        MeshRenderer renderer = building.GetComponent<MeshRenderer>();
        if (renderer == null) return true;
    
        //use renderer bounds for the box shape
        Vector3 center = ghostObj.transform.position + renderer.localBounds.center;
        Vector3 halfExtents = renderer.localBounds.extents - Vector3.one * 0.05f;
    
        Collider[] overlaps = Physics.OverlapBox(center, halfExtents, ghostObj.transform.rotation, ~0, QueryTriggerInteraction.Ignore);
    
        foreach (var overlap in overlaps)
        {
            if (overlap.transform.IsChildOf(ghostObj.transform)) continue;
            if (overlap.gameObject.layer == LayerMask.NameToLayer("Ground")) continue;
    
            return false;
        }
    
        return true;
    }

    private static Vector3 FindSnapPosition(RaycastHit hit, Building placing, Building target, GameObject ghostObj, float rotation, int gridSize, float snapDistance, out bool snapped)
    {
        float closest = float.MaxValue;
        Vector3 best = hit.point;
        Vector3 bestOffset = Vector3.zero;
        float snapMultiplier = placing.affectedByGridSizePosition ? gridSize : 1f;
        snapped = false;

        foreach (var targetPivot in target.pivots)
        {
            Vector3 worldPivotPos = GetRotatedPivotPosition(targetPivot.position, hit.collider.transform.position, hit.collider.transform.eulerAngles.y);
            Vector3 direction = (worldPivotPos - hit.collider.transform.position).normalized;
            direction = (direction + hit.normal).normalized * snapMultiplier / 2f;

            float distToPivot = Vector3.Distance(hit.point, worldPivotPos);
            if (distToPivot > snapDistance) continue;

            ghostObj.transform.position = worldPivotPos;

            foreach (var myPivot in placing.pivots)
            {
                Vector3 myWorldPivotPos = GetRotatedPivotPosition(myPivot.position, ghostObj.transform.position, rotation);
                float d = Vector3.Distance(myWorldPivotPos - direction, worldPivotPos);

                if (d < closest)
                {
                    closest = d;
                    bestOffset = myWorldPivotPos - ghostObj.transform.position;
                    best = worldPivotPos;
                    snapped = true;
                }
            }
        }
        
        return best + bestOffset;
    }

    private static Vector3 GetRotatedPivotPosition(Vector3 pivotPos, Vector3 centerPos, float yRotation)
    {
        Vector3 direction = pivotPos - centerPos;
        direction = Quaternion.Euler(0f, yRotation, 0f) * direction;
        return direction + centerPos;
    }
}