using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class BuildingHammer : Item
{
    //References
    private BuildingManager buildManager;
    private Transform mainCamera;
    private Animator animator;

    [Header("Buildings")]
    [SerializeField, EditorChangeable] private List<Building> availableBuildings;
    [SerializeField] private ParticleSystem spawnParticles;
    private bool isChosingBuild = false;

    //Selection state
    private int selectedBuildingIndex;
    private float currentRotation;

    //Ghost data
    private GameObject ghostBuilding;
    private Building currentBuilding;
    private MeshFilter ghostMeshFilter;
    private Renderer[] ghostRenderers;

    //Placement state
    private bool canPlace;
    private Vector3 lastValidPosition;
    private Tween rotationTween;

    #region Unity

    private void Start()
    {
        //Cache refs
        buildManager = BuildingManager.Instance;
        mainCamera = PlayerLook.mainCamera.transform;
        animator = GetComponent<Animator>();
        SpawnGhost();
    }

    private void LateUpdate()
    {
        if (!isItemPickedUp) return;

        HandleInput();
        UpdateGhost();
    }

    #endregion

    #region Input

    private void HandleInput()
    {
        //Scroll selection
        if(Input.GetKeyDown(Keybinds.Key("SelectBuild"))) 
        {
            isChosingBuild = !isChosingBuild;
            PlayerInventory.CanScroll = !isChosingBuild;
        }

        if(isChosingBuild)
            HandleSelection();

        //Rotate
        if (Input.GetKeyDown(Keybinds.Key("Rotate")))
        {
            currentRotation -= buildManager.rotationAngle;
            rotationTween?.Kill();
            rotationTween = ghostBuilding?.transform.DORotate(new Vector3(0f, currentRotation, 0f), 0.15f).SetEase(Ease.OutQuad);
        }
    }

    public override void OnUse()
    {
        if(!BuildingManager.CanBuild(currentBuilding.data.pointsWorth))
            return;

        TryPlace();
        animator.SetTrigger("Place");

    }

    public override void OnUseAlt()
    {
        TryDemolish();
        animator.SetTrigger("Demolish");       
    }

    private void HandleSelection()
    {
        if(!BuildingManager.CanBuild())
            return;

        //Read scroll
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll == 0f) return;

        //Update index
        selectedBuildingIndex += scroll < 0 ? 1 : -1;

        //Wrap index
        if (selectedBuildingIndex >= availableBuildings.Count)
            selectedBuildingIndex = 0;
        else if (selectedBuildingIndex < 0)
            selectedBuildingIndex = availableBuildings.Count - 1;

        //Refresh ghost + ui
        SpawnGhost();
    }

    #endregion

    #region Ghost

    private void SpawnGhost()
    {
        //Destroy old ghost
        if (ghostBuilding != null) Destroy(ghostBuilding);

        //Spawn new ghost
        ghostBuilding = Instantiate(availableBuildings[selectedBuildingIndex].gameObject);
        ghostBuilding.tag = "Untagged";

        //Cache components
        currentBuilding = ghostBuilding.GetComponent<Building>();
        ghostMeshFilter = ghostBuilding.GetComponent<MeshFilter>();
        ghostRenderers = ghostBuilding.GetComponentsInChildren<Renderer>();

        //Disable colliders
        foreach (var col in ghostBuilding.GetComponentsInChildren<Collider>())
            col.enabled = false;

        //Set transparency
        SetGhostAlpha(0.5f);

        //Scale if using pivots
        if (currentBuilding != null && currentBuilding.usesPivots)
            ghostBuilding.transform.localScale = Vector3.one * BuildingManager.Instance.gridSize;

        //Set initial rotation
        ghostBuilding.transform.rotation = Quaternion.Euler(0f, currentRotation, 0f);
    }

    private void UpdateGhost()
    {
        if(!BuildingManager.CanBuild(currentBuilding.data.pointsWorth))
        {
            if(ghostBuilding != null)
                Destroy(ghostBuilding);
            return;
        }

        //Skip if missing
        if (ghostBuilding == null) return;

        //Spherecast
        if (!BuildUtilities.TryGetHit(mainCamera, buildManager.sphereCastRadius, buildManager.maxBuildDistance, buildManager.buildableLayers, out RaycastHit hit))
        {
            ghostBuilding.SetActive(false);
            canPlace = false;
            return;
        }

        //Show ghost
        ghostBuilding.SetActive(true);

        //Calculate position
        Vector3 position = BuildUtilities.CalculatePosition(hit, currentBuilding, ghostMeshFilter, ghostBuilding, buildManager.gridSize, currentRotation, buildManager.snapDistance);

        //Apply transform
        lastValidPosition = position;
        ghostBuilding.transform.position = position;

        //Check if position is valid (not inside anything)
        canPlace = BuildUtilities.IsPositionValid(ghostBuilding, currentBuilding, buildManager.gridSize);
        
        //Update visuals
        UpdateGhostColor();
    }

    private void UpdateGhostColor()
    {
        //Apply color
        Color color = canPlace ? Color.green : Color.red;
        color.a = 0.5f;

        foreach (var r in ghostRenderers)
        {
            Material[] mats = r.materials;
            for (int i = 0; i < mats.Length; i++)
                mats[i].color = color;
            r.materials = mats;
        }
    }

    private void SetGhostAlpha(float alpha)
    {
        //Set ghost alpha
        foreach (var r in ghostRenderers)
        {
            Material[] mats = r.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                Material newMat = new Material(mats[i]);
                Color c = newMat.color;
                c.a = alpha;
                newMat.color = c;
                mats[i] = newMat;
            }
            r.materials = mats;
        }
    }

    #endregion

    #region Placement

    private void TryPlace()
    {
        //Verify
        if (!canPlace || ghostBuilding == null) return;
        if (!TakeResources()) return;

        //Spawn building
        GameObject placed = Instantiate(availableBuildings[selectedBuildingIndex].gameObject, lastValidPosition, Quaternion.Euler(0f, Mathf.Round(currentRotation), 0f));

        //Scale if needed
        if (availableBuildings[selectedBuildingIndex].usesPivots)
            placed.transform.localScale = Vector3.one * buildManager.gridSize;

        //Finalize build
        placed.tag = "Build";

        //Restore material alpha
        Renderer[] renderers = placed.GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
        {
            Material[] mats = r.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                Color c = mats[i].color;
                c.a = 1f;
                mats[i].color = c;
            }
            r.materials = mats;
        }

        placed.GetComponent<Building>()?.OnPlace(buildManager.extraBuildingHealth, buildManager.extraBuildingDamage);

        //Particles
        if(placed.TryGetComponent(out Renderer renderer))
        {
            ParticleSpawner.SpawnWithBounds(spawnParticles, placed.transform.position, placed.transform.rotation, renderer.bounds);
        }

        //Sound
        AudioManager.Instance?.PlaySound("Build", 0.9f, 1.25f);
    }

    private void TryDemolish()
    {
        //Check if there's a hit
        if (!BuildUtilities.TryGetHit(mainCamera, buildManager.sphereCastRadius, buildManager.maxBuildDistance, buildManager.demolishLayers, out RaycastHit hit)) return;

        //Double check if it's a build
        if (!hit.collider.CompareTag("Build")) return;

        //Check for building component (or parent)
        Building building = hit.collider.GetComponent<Building>();
        if (building == null)
        {
            building = hit.collider.GetComponentInParent<Building>();
            if (building == null) return;
        }

        //Refund and destroy
        if (!RefundResources(hit.transform.GetComponent<Building>())) return;

        Destroy(building.gameObject);
        AudioManager.Instance?.PlaySound("Demolish", 0.9f, 1.25f);
    }

    #endregion

    #region Resources

    bool TakeResources()
    {
        //Inventory
        PlayerInventory inventory = heldby.GetComponent<PlayerInventory>();

        //Check cost
        foreach (var ing in availableBuildings[selectedBuildingIndex].ingredients)
            if (!inventory.HasItem(ing.item, ing.quantity))
                return false;

        //Take
        foreach (var ing in availableBuildings[selectedBuildingIndex].ingredients)
            inventory.TakeItem(ing.item, ing.quantity, out _);

        return true;
    }

    bool RefundResources(Building building)
    {
        //Inventory
        PlayerInventory inventory = heldby.GetComponent<PlayerInventory>();

        //Refund items
        foreach (var ing in building.ingredients)
        {
            Item item = Instantiate(ing.item).GetComponent<Item>();
            item.HeldQuantity = ing.quantity;

            inventory.GiveItem(item, out bool taken);
            if (!taken)
            {
                Destroy(item);
                return false;
            }
        }

        return true;
    }

    #endregion

    #region Item Overrides

    public override void OnSelectOnce()
    {
        SpawnGhost();
        Debug.Log("T");
        buildManager?.ShowUI(true);
    }

    public override void OnSelect()
    {
        buildManager?.UpdateBuildUI(availableBuildings[selectedBuildingIndex], true);
    }

    public override void OnChangingItems()
    {
        //Cleanup
        buildManager?.ShowUI(false);
        rotationTween?.Kill();
        Destroy(ghostBuilding);
        isChosingBuild = false;
        PlayerInventory.CanScroll = true;
    }

    #endregion
}