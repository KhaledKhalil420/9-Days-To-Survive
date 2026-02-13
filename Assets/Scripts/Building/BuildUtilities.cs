using UnityEngine;

public static class BuildUtilities
{
    private const float BUILDING_TOUCH_THRESHOLD = 0.1f;

    public static bool TryGetHit(Transform camera, float radius, float maxDistance, LayerMask layers, out RaycastHit hit)
    {
        return Physics.SphereCast(camera.position, radius, camera.forward, out hit, maxDistance, layers);
    }

    public static Vector3 CalculatePosition(RaycastHit hit, Building building, MeshFilter meshFilter, GameObject ghostObj, int gridSize, float rotation, float snapDistance)
    {
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
        return FindSnapPosition(hit, building, target, ghostObj, rotation, gridSize, snapDistance);
    }

    public static bool IsPositionValid(GameObject ghostObj, Building building, float gridSize)
    {
        // Get all colliders in the ghost building
        Collider[] ghostColliders = ghostObj.GetComponentsInChildren<Collider>();
        if (ghostColliders.Length == 0) return true;

        // Enable colliders temporarily for the check
        foreach (var col in ghostColliders)
            col.enabled = true;

        bool isValid = true;

        // Check each collider for overlaps
        foreach (var col in ghostColliders)
        {
            Collider[] overlaps = null;

            if (col is BoxCollider box)
            {
                Vector3 center = col.transform.TransformPoint(box.center);
                Vector3 halfExtents = Vector3.Scale(box.size / 2f, col.transform.lossyScale);
                // Shrink the box slightly for building checks
                overlaps = Physics.OverlapBox(center, halfExtents - Vector3.one * BUILDING_TOUCH_THRESHOLD, col.transform.rotation, ~0, QueryTriggerInteraction.Ignore);
            }
            else if (col is SphereCollider sphere)
            {
                Vector3 center = col.transform.TransformPoint(sphere.center);
                float radius = sphere.radius * Mathf.Max(col.transform.lossyScale.x, col.transform.lossyScale.y, col.transform.lossyScale.z);
                // Shrink the sphere slightly for building checks
                overlaps = Physics.OverlapSphere(center, radius - BUILDING_TOUCH_THRESHOLD, ~0, QueryTriggerInteraction.Ignore);
            }
            else if (col is CapsuleCollider capsule)
            {
                Vector3 center = col.transform.TransformPoint(capsule.center);
                float radius = capsule.radius * Mathf.Max(col.transform.lossyScale.x, col.transform.lossyScale.z);
                float height = capsule.height * col.transform.lossyScale.y;
                // Shrink the capsule slightly for building checks
                overlaps = Physics.OverlapCapsule(center + Vector3.up * (height / 2f - radius), center - Vector3.up * (height / 2f - radius), radius - BUILDING_TOUCH_THRESHOLD, ~0, QueryTriggerInteraction.Ignore);
            }

            if (overlaps != null)
            {
                foreach (var overlap in overlaps)
                {
                    // Ignore self colliders
                    if (overlap.transform.IsChildOf(ghostObj.transform)) continue;

                    // Allow touching GROUND layer
                    if (overlap.gameObject.layer == LayerMask.NameToLayer("Ground")) continue;

                    // Allow touching other buildings (they're checked with threshold already)
                    if (overlap.CompareTag("Build")) continue;

                    // Found a collision with something else - can't place
                    isValid = false;
                    break;
                }
            }

            if (!isValid) break;
        }

        // Disable colliders again
        foreach (var col in ghostColliders)
            col.enabled = false;

        return isValid;
    }

    private static Vector3 FindSnapPosition(RaycastHit hit, Building placing, Building target, GameObject ghostObj, float rotation, int gridSize, float snapDistance)
    {
        float closest = float.MaxValue;
        Vector3 best = hit.point;
        Vector3 bestOffset = Vector3.zero;
        float snapMultiplier = placing.affectedByGridSizePosition ? gridSize : 1f;

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