using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BuildingHammer : Item
{
    //refs
    private BuildingManager buildManager;
    private Transform mainCamera;
    private Animator animator;
    private DayNightCycleManager dayNightCycle;

    [Header("Buildings")]
    [SerializeField, EditorChangeable] private List<Building> availableBuildings;
    [SerializeField] private ParticleSystem spawnParticles;
    private bool isChosingBuild = false;

    //selection
    private int selectedBuildingIndex;
    private float currentRotation;

    //ghost
    private GameObject ghostBuilding;
    private Building currentBuilding;
    private Renderer[] ghostRenderers;

    //placement
    private bool canPlace;
    private Vector3 lastValidPosition;
    private Tween rotationTween;

    #region Unity

    private void Start()
    {
        buildManager = BuildingManager.Instance;
        dayNightCycle = DayNightCycleManager.Instance;
        mainCamera = PlayerLook.mainCamera.transform;
        animator = GetComponent<Animator>();

        buildManager.OnGridUpdated += UpdateUiGhost;
        SpawnGhost();
    }

    private void LateUpdate()
    {
        if (!isItemPickedUp) return;

        HandleInput();
        UpdateGhost();
    }

    private void FixedUpdate()
    {
        BuildingInspectRaycast();
    }

    #endregion

    #region Input

    private void HandleInput()
    {
        //toggle build selection scroll
        if (Input.GetKeyDown(Keybinds.Key("SelectBuild")))
        {
            isChosingBuild = !isChosingBuild;
            PlayerInventory.Instance.CanScroll = !isChosingBuild;

            float pitch = isChosingBuild ? 1.25f : 1f;
            AudioManager.Instance.PlaySound("Start_Selecting_Build", pitch - 0.1f, pitch + 0.1f);
        }

        if (isChosingBuild) HandleSelection();

        //rotate ghost
        if (Input.GetKeyDown(Keybinds.Key("Rotate")))
        {
            currentRotation -= buildManager.rotationAngle;
            rotationTween?.Kill();

            if (ghostBuilding != null)
                rotationTween = ghostBuilding.transform.DORotate(new Vector3(0f, currentRotation, 0f), 0.15f).SetEase(Ease.OutQuad);

            AudioManager.Instance.PlaySound("Rotating_Build", 0.9f, 1.15f);
        }
    }

    public override void OnUse()
    {
        if (!BuildingManager.CanBuild(currentBuilding.data.pointsWorth)) return;

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
        if (!BuildingManager.CanBuild()) return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll == 0f) return;

        selectedBuildingIndex += scroll < 0 ? 1 : -1;

        //wrap around list
        if (selectedBuildingIndex >= availableBuildings.Count) selectedBuildingIndex = 0;
        else if (selectedBuildingIndex < 0) selectedBuildingIndex = availableBuildings.Count - 1;

        SpawnGhost();
        AudioManager.Instance.PlaySound("Selecting_Build", 0.9f, 1.15f);
    }

    #endregion

    #region Inspect

    private bool reset = true;

    private void BuildingInspectRaycast()
    {
        if (dayNightCycle.currentState == DayNightCycleManager.CycleState.Night)
        {
            buildManager?.ShowUI(false);

            if (Physics.Raycast(mainCamera.position, mainCamera.forward, out RaycastHit hit, 3))
            {
                if (hit.transform.TryGetComponent(out Building building))
                {
                    buildManager.ShowInspectUI(true);
                    buildManager.UpdateInspectUI(building);
                }
                else buildManager.ShowInspectUI(false);
            }

            reset = true;
        }
        else
        {
            if (!reset) return;

            reset = false;
            buildManager.ShowInspectUI(false);
            UpdateUiGhost();
        }
    }

    #endregion

    #region Ghost

    private void SpawnGhost()
    {
        if (ghostBuilding != null) Destroy(ghostBuilding);

        ghostBuilding = Instantiate(availableBuildings[selectedBuildingIndex].gameObject);
        ghostBuilding.tag = "Untagged";

        currentBuilding = ghostBuilding.GetComponent<Building>();
        ghostRenderers = ghostBuilding.GetComponentsInChildren<Renderer>();

        //disable so they don't mess with placement checks
        foreach (var col in ghostBuilding.GetComponentsInChildren<Collider>())
            col.enabled = false;

        SetGhostAlpha(0.5f);
        ghostBuilding.transform.rotation = Quaternion.Euler(0f, currentRotation, 0f);
    }

    private void UpdateGhost()
    {
        if (!BuildingManager.CanBuild(currentBuilding.data.pointsWorth))
        {
            if (ghostBuilding != null) Destroy(ghostBuilding);
            return;
        }

        if (ghostBuilding == null) return;

        //no surface? hide ghost
        if (!BuildUtilities.TryGetHit(mainCamera, buildManager.sphereCastRadius, buildManager.maxBuildDistance, buildManager.buildableLayers, out RaycastHit hit))
        {
            ghostBuilding.SetActive(false);
            canPlace = false;
            return;
        }

        ghostBuilding.SetActive(true);

        Vector3 position = BuildUtilities.CalculatePosition(hit, currentBuilding, ghostBuilding, currentRotation, buildManager.snapDistance, out bool isSnapped);

        lastValidPosition = position;
        ghostBuilding.transform.position = position;

        canPlace = currentBuilding.requireSnapping
            ? isSnapped
            : BuildUtilities.IsPositionValid(ghostBuilding, currentBuilding);

        UpdateGhostColor();
    }

    private void UpdateGhostColor()
    {
        Color color = canPlace ? Color.green : Color.red;
        color.a = 0.5f;

        foreach (var r in ghostRenderers)
        {
            Material[] mats = r.materials;
            for (int i = 0; i < mats.Length; i++) mats[i].color = color;
            r.materials = mats;
        }
    }

    private void SetGhostAlpha(float alpha)
    {
        foreach (var r in ghostRenderers)
        {
            Material[] mats = r.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                Material mat = new Material(mats[i]);
                Color c = mat.color;
                c.a = alpha;
                mat.color = c;
                mats[i] = mat;
            }
            r.materials = mats;
        }
    }

    #endregion

    #region Placement

    private void TryPlace()
    {
        if (!canPlace || ghostBuilding == null) return;
        if (!TakeResources()) return;

        GameObject placed = Instantiate(
            availableBuildings[selectedBuildingIndex].gameObject,
            lastValidPosition,
            Quaternion.Euler(0f, Mathf.Round(currentRotation), 0f));

        placed.tag = "Build";

        //restore full alpha
        foreach (var r in placed.GetComponentsInChildren<Renderer>())
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

        if (placed.TryGetComponent(out Renderer renderer))
            ParticleSpawner.SpawnWithBounds(spawnParticles, placed.transform.position, placed.transform.rotation, renderer.bounds);

        AudioManager.Instance?.PlaySound("Build", 0.9f, 1.25f);
    }

    private void TryDemolish()
    {
        if (!BuildUtilities.TryGetHit(mainCamera, buildManager.sphereCastRadius, buildManager.maxBuildDistance, buildManager.demolishLayers, out RaycastHit hit)) return;
        if (!hit.collider.CompareTag("Build")) return;

        Building building = hit.collider.GetComponent<Building>() ?? hit.collider.GetComponentInParent<Building>();
        if (building == null) return;
        if (!RefundResources(building)) return;

        building.OnDemolish();
        Destroy(building.gameObject);
        AudioManager.Instance?.PlaySound("Demolish", 0.9f, 1.25f);
    }

    #endregion

    #region Resources

    private bool TakeResources()
    {
        PlayerInventory inventory = heldby.GetComponent<PlayerInventory>();

        //check first, take after
        foreach (var ing in availableBuildings[selectedBuildingIndex].ingredients)
            if (!inventory.HasItem(ing.item, ing.quantity)) return false;

        foreach (var ing in availableBuildings[selectedBuildingIndex].ingredients)
            inventory.TakeItem(ing.item, ing.quantity, out _);

        return true;
    }

    private bool RefundResources(Building building)
    {
        PlayerInventory inventory = heldby.GetComponent<PlayerInventory>();

        foreach (var ing in building.ingredients)
        {
            Item item = Instantiate(ing.item).GetComponent<Item>();
            item.HeldQuantity = ing.quantity;

            inventory.GiveItem(item, out bool taken);
            if (!taken) { Destroy(item); return false; }
        }

        return true;
    }

    #endregion

    #region Item Overrides

    public override void OnSelectOnce() => UpdateUiGhost();

    public override void OnPick()
    {
        if (!isSelected) return;
        UpdateUiGhost();
    }

    public void UpdateUiGhost()
    {
        if (!isSelected || !isItemPickedUp) return;
        SpawnGhost();

        if (dayNightCycle.currentState == DayNightCycleManager.CycleState.Day)
            buildManager.ShowUI(true);
    }

    public override void OnSelect()
    {
        buildManager?.UpdateBuildUI(availableBuildings[selectedBuildingIndex], isChosingBuild);
    }

    public override void OnChangingItems()
    {
        buildManager?.ShowUI(false);
        buildManager?.ShowInspectUI(false);
        rotationTween?.Kill();
        Destroy(ghostBuilding);
        isChosingBuild = false;
        PlayerInventory.Instance.CanScroll = true;
    }

    private void OnDestroy()
    {
        if (buildManager) buildManager.OnGridUpdated -= UpdateUiGhost;
    }

    #endregion
}