using System.Collections.Generic;
using UnityEngine;

//if this code was a woman I would've married it
public class BuildingHammer : Item
{    
    //References
    private Transform mainCamera;
    private GameObject ghostBuilding;
    private Building currentBuildingComponent;
    private MeshFilter ghostMeshFilter;
    private Renderer ghostRenderer;
    private Animator animator;
    
    //Building Selection
    private int selectedBuildingIndex;
    public List<Building> availableBuildings;
    
    [Header("Building Settings")]
    public int gridSize = 2;
    public float maxBuildDistance = 12f;
    public LayerMask buildableLayers;
    public int rotationAngle = 45;
    
    [Header("Snapping Settings")]
    public float sphereCastRadius = 1.5f; 
    public float snapDistance = 5f;
    
    [Header("Demolish Settings")]
    public LayerMask demolishLayers;
    public KeyCode demolishKey = KeyCode.X;
    
    //Internal State
    private int currentRotation = 0;
    private bool canPlaceBuilding = false;
    private Vector3 lastValidPosition;
        
    private void Start()
    {
        mainCamera = PlayerLook.mainCamera.transform;
        animator = GetComponent<Animator>();
    }
    
    private void LateUpdate()
    {
        if(!isItemPickedUp) 
        return;
        HandleInputs();
        UpdateGhostBuildingPosition();
    }
    
    #region Input Handling
    
    private void HandleInputs()
    {
        HandleBuildingSelection();
        
        //Rotate building
        if (Input.GetKeyDown(Keybinds.Key("Rotate")))
        {
            RotateBuilding();
        }
        
        //Place building
        if (Input.GetMouseButtonDown(0))
        {
            PlaceBuilding();

            //Animation
            animator.SetTrigger("Place");
        }
        
        //Demolish building
        if (Input.GetKeyDown(Keybinds.Key("Demolish")))
        {
            DemolishBuilding();
            animator.SetTrigger("Demolish");
        }
    }
    
    private void HandleBuildingSelection()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        
        if (scrollInput == 0f)
            return;
        
        if (scrollInput < 0f)
        {
            selectedBuildingIndex++;
        }
        else
        {
            selectedBuildingIndex--;
        }
        
        if (selectedBuildingIndex >= availableBuildings.Count)
        {
            selectedBuildingIndex = 0;
        }
        else if (selectedBuildingIndex < 0)
        {
            selectedBuildingIndex = availableBuildings.Count - 1;
        }
        
        SpawnNewGhostBuilding();
    }
    
    private void RotateBuilding()
    {
        currentRotation -= rotationAngle;
    }
    
    #endregion

    #region Ghost Building Setup
    
    private void SpawnNewGhostBuilding()
    {
        //Destroy old ghost if it exists
        if (ghostBuilding != null)
        {
            Destroy(ghostBuilding);
        }
        
        if (availableBuildings[selectedBuildingIndex] == null)
            return;
        
        // Spawn new ghost
        ghostBuilding = Instantiate(availableBuildings[selectedBuildingIndex].gameObject);
        currentBuildingComponent = ghostBuilding.GetComponent<Building>();
        
        MakeGhostTransparent();
    }
    
    private void MakeGhostTransparent()
    {
        //Get components
        ghostMeshFilter = ghostBuilding.GetComponent<MeshFilter>();
        ghostRenderer = ghostBuilding.GetComponent<Renderer>();
        
        //Make material semi-transparent
        Material ghostMaterial = ghostRenderer.material;
        Color ghostColor = ghostMaterial.color;
        ghostColor.a = 0.5f; // 50% transparent
        ghostMaterial.color = ghostColor;
        ghostRenderer.material = ghostMaterial;
        
        //Scale to grid size
        ghostBuilding.transform.localScale = Vector3.one * gridSize;
        
        //Turn off colliders
        Collider[] allColliders = ghostBuilding.GetComponentsInChildren<Collider>();
        foreach (Collider collider in allColliders)
        {
            collider.enabled = false;
        }
    }
    
    #endregion

    #region Ghost Building Position Update
    
    private void UpdateGhostBuildingPosition()
    {
        //Don't update if no ghost exists
        if (ghostBuilding == null)
            return;
        
        //Apply current rotation
        
        ghostBuilding.transform.rotation = Quaternion.Lerp(ghostBuilding.transform.rotation, Quaternion.Euler(0f, currentRotation, 0f), Time.deltaTime * 30f);        
        //Use SphereCast (I fucking used it to make it easier to snap to pivots)
        RaycastHit hitInfo;
        bool hitSomething = Physics.SphereCast(
            mainCamera.position,
            sphereCastRadius,
            mainCamera.forward,
            out hitInfo,
            maxBuildDistance,
            buildableLayers
        );
        
        //If spherecast missed, hide ghost and return
        if (!hitSomething)
        {
            canPlaceBuilding = false;
            ghostBuilding.SetActive(false);
            return;
        }
        
        //Show ghost if it was hidden
        if (!ghostBuilding.activeSelf)
        {
            ghostBuilding.SetActive(true);
        }
        
        //Calculate position based on what we hit
        Vector3 targetPosition = CalculateBuildingPosition(hitInfo);
        
        //Move ghost to target position
        lastValidPosition = targetPosition;
        ghostBuilding.transform.position = targetPosition;
        
        //Update visuals
        canPlaceBuilding = true;
        UpdateGhostColor();
    }
    
    #endregion

    #region Pivot Snapping System //Mostly done by AI.. like 50% if the work was done by AI

    private Vector3 CalculateBuildingPosition(RaycastHit hitInfo)
    {
        // Get the size of the building mesh
        Vector3 meshSize = ghostMeshFilter.mesh.bounds.extents * gridSize;
        
        // Default: place on surface with correct height offset
        Vector3 position = hitInfo.point + Vector3.up * (meshSize.y - ghostMeshFilter.mesh.bounds.center.y);
        
        // Check if we hit an existing building
        Building hitBuilding = hitInfo.collider.GetComponent<Building>();
        bool hitExistingBuilding = hitInfo.collider.CompareTag("Build");
        bool hitBuildingHasPivots = hitBuilding != null && hitBuilding.pivots.Count > 0;
        
        // If we hit a building with pivot points, snap to it
        if (hitExistingBuilding && hitBuildingHasPivots)
        {
            position = FindBestSnapPosition(hitInfo, hitBuilding);
        }
        
        return position;
    }

    private Vector3 FindBestSnapPosition(RaycastHit hitInfo, Building targetBuilding)
    {
        float closestDistance = float.PositiveInfinity;
        Vector3 finalPosition = hitInfo.point;
        Vector3 bestOffset = Vector3.zero;
        
        // Loop through all pivot points on the building we hit
        foreach (Transform targetPivot in targetBuilding.pivots)
        {
            // Get pivot position in world space with rotation
            Vector3 worldPivotPosition = GetRotatedPivotPosition(
                targetPivot.position,
                hitInfo.collider.transform.position,
                hitInfo.collider.transform.eulerAngles.y
            );
            
            // Calculate direction for snapping
            Vector3 direction = (worldPivotPosition - hitInfo.collider.transform.position).normalized;
            direction = (direction + hitInfo.normal).normalized * gridSize / 2f;
            
            // Check if we're close enough to this pivot (using adjustable snapDistance)
            float distanceToPivot = Vector3.Distance(hitInfo.point, worldPivotPosition);
            if (distanceToPivot < snapDistance)
            {
                // Temporarily move ghost to this position
                ghostBuilding.transform.position = worldPivotPosition;
                
                // Check all of OUR pivot points to find best match
                foreach (Transform myPivot in currentBuildingComponent.pivots)
                {
                    Vector3 myWorldPivotPosition = GetRotatedPivotPosition(
                        myPivot.position,
                        ghostBuilding.transform.position,
                        currentRotation
                    );
                    
                    // Calculate distance between the two pivots
                    float distance = Vector3.Distance(myWorldPivotPosition - direction, worldPivotPosition);
                    
                    // Keep track of closest match
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        bestOffset = myWorldPivotPosition - ghostBuilding.transform.position;
                        finalPosition = worldPivotPosition;
                    }
                }
            }
        }
        
        // Return final position with offset applied
        return finalPosition + bestOffset;
    }
    
    private Vector3 GetRotatedPivotPosition(Vector3 pivotPosition, Vector3 centerPosition, float yRotation)
    {
        // Get direction from center to pivot
        Vector3 direction = pivotPosition - centerPosition;
        
        // Rotate the direction
        direction = Quaternion.Euler(0f, yRotation, 0f) * direction;
        
        // Return rotated position
        return direction + centerPosition;
    }
    
    #endregion

    #region Visual Feedback
    
    private void UpdateGhostColor()
    {
        Material ghostMaterial = ghostRenderer.material;
        Color newColor = canPlaceBuilding ? Color.green : Color.red;
        newColor.a = 0.5f;
        ghostMaterial.color = newColor;
        ghostRenderer.material = ghostMaterial;
    }
    
    #endregion

    #region Building Placement
    
    private void PlaceBuilding()
    {
        if (!canPlaceBuilding || ghostBuilding == null)
            return;
        
        // TODO: Check if player has required materials
        // if (!PlayerHasRequiredMaterials()) return;
        
        //Spawn building
        GameObject newBuilding = Instantiate(
            availableBuildings[selectedBuildingIndex].gameObject,
            lastValidPosition,
            Quaternion.Euler(0f, currentRotation, 0f)
        );
        
        //Set scale and tag
        newBuilding.transform.localScale = Vector3.one * gridSize;
        newBuilding.tag = "Build";
        
        //Enable colliders on the placed building
        Collider[] allColliders = newBuilding.GetComponentsInChildren<Collider>();
        foreach (Collider collider in allColliders)
        {
            collider.enabled = true;
        }
        
        //Materials
        Renderer buildingRenderer = newBuilding.GetComponent<Renderer>();
        if (buildingRenderer != null)
        {
            Material buildingMaterial = buildingRenderer.material;
            Color solidColor = buildingMaterial.color;
            solidColor.a = 1f; // Fully opaque
            buildingMaterial.color = solidColor;
            buildingRenderer.material = buildingMaterial;
        }
    }

    private void DemolishBuilding()
    {
        //Use SphereCast for demolishing too (I love it goddamn it)
        RaycastHit hitInfo;
        bool hitSomething = Physics.SphereCast(
            mainCamera.position,
            sphereCastRadius,
            mainCamera.forward,
            out hitInfo,
            maxBuildDistance,
            demolishLayers
        );
        
        if (!hitSomething)
            return;
        
        if (!hitInfo.collider.CompareTag("Build"))
            return;
        
        //Get the building component
        Building buildingToDemolish = hitInfo.collider.GetComponent<Building>();
        if (buildingToDemolish == null)
            return;
        
        // TODO: Refund materials to player
        // PlayerInventory.GiveItem(buildingRefund);
                
        // Destroy the building
        Destroy(hitInfo.collider.gameObject);
    }

    #endregion

    public override void OnChangingItems()
    {
        ghostRenderer = null;
        ghostMeshFilter = null;
        currentBuildingComponent = null;
        Destroy(ghostBuilding);
    }

    public override void OnSelectOnce()
    {
        SpawnNewGhostBuilding();
    }
}