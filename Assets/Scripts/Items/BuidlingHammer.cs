using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BuildingHammer : Item
{
    private BuildingManager buildingManager;
    private DayNightCycleManager dayNightManager;
    private Transform cam;
    private Animator animator;

    [Header("Buildings")]
    [SerializeField] private List<Building> availableBuildings;
    [SerializeField] private ParticleSystem spawnParticles;
    private SnappingPoint snappedTo;

    private bool choosing, canPlace, reset = true;
    private int index;
    private float rotation;

    private GameObject ghost;
    private Building currentBuilding;
    private MeshFilter meshFilter;
    private Vector3 lastPos;
    private Tween rotTween;

    void Start()
    {
        //Cache Refs
        buildingManager = BuildingManager.Instance;
        dayNightManager = DayNightCycleManager.Instance;
        cam = PlayerLook.mainCamera.transform;
        animator = GetComponent<Animator>();
        
        //Spawn ghost and sub
        buildingManager.OnGridUpdated += UpdateUiGhost;
        SpawnGhost();
    }

    void LateUpdate()
    {
        if (!isItemPickedUp) return;
        HandleInput();
        UpdateGhost();
    }

    void FixedUpdate() => Inspect();

    void HandleInput()
    {
        if (Input.GetKeyDown(Keybinds.Key("SelectBuild")))
        {
            choosing = !choosing;
            PlayerInventory.Instance.CanScroll = !choosing;
            float p = choosing ? 1.25f : 1;
            AudioManager.Instance.PlaySound("Start_Selecting_Build", p - .1f, p + .1f);
        }

        if (choosing) Select();

        if (Input.GetKeyDown(Keybinds.Key("Rotate")))
        {
            rotation -= buildingManager.rotationAngle;
            rotTween?.Kill();
            if (ghost)
                rotTween = ghost.transform.DORotate(new Vector3(0, rotation, 0), .15f).SetEase(Ease.OutQuad);
            AudioManager.Instance.PlaySound("Rotating_Build", .9f, 1.15f);
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

    void Select()
    {
        if (!BuildingManager.CanBuild()) return;

        float s = Input.GetAxis("Mouse ScrollWheel");
        if (s == 0) return;

        index = (index + (s < 0 ? 1 : -1) + availableBuildings.Count) % availableBuildings.Count;
        SpawnGhost();
        AudioManager.Instance.PlaySound("Selecting_Build", .9f, 1.15f);
    }

    private void Inspect()
    {
        if (dayNightManager.currentState == DayNightCycleManager.CycleState.Night)
        {
            buildingManager?.ShowUI(false);

            if (Physics.Raycast(cam.position, cam.forward, out var hit, 3) && hit.transform.TryGetComponent(out Building b))
            {
                buildingManager.ShowInspectUI(true);
                buildingManager.UpdateInspectUI(b);
            }

            else 
            {
                buildingManager.ShowInspectUI(false);
            }

            reset = true;
        }
        else if (reset)
        {
            reset = false;
            buildingManager.ShowInspectUI(false);
            UpdateUiGhost();
        }
    }

    void SpawnGhost()
    {
        if (ghost) Destroy(ghost);

        ghost = Instantiate(availableBuildings[index].gameObject);
        ghost.tag = "Untagged";

        currentBuilding = ghost.GetComponent<Building>();
        meshFilter = ghost.GetComponent<MeshFilter>();

        foreach (var c in ghost.GetComponentsInChildren<Collider>())
            c.enabled = false;

        SetAlpha(.5f, ghost);

        if (currentBuilding && currentBuilding.data.usesPivots)
            ghost.transform.localScale = Vector3.one * buildingManager.gridSize;

        ghost.transform.rotation = Quaternion.Euler(0, rotation, 0);
    }

    void UpdateGhost()
    {
        if (!BuildingManager.CanBuild(currentBuilding.data.pointsWorth))
        {
            if (ghost) Destroy(ghost);
            return;
        }

        if (!ghost) return;

        if (!BuildUtilities.TryGetHit(cam, buildingManager.sphereCastRadius, buildingManager.maxBuildDistance, buildingManager.buildableLayers, out var hit))
        {
            ghost.SetActive(false);
            canPlace = false;
            return;
        }

        ghost.SetActive(true);

        Vector3 pos = BuildUtilities.CalculatePosition
        (
            hit, currentBuilding, meshFilter, ghost, 
            buildingManager.gridSize, rotation, buildingManager.snapDistance, 
            out bool snap, out SnappingPoint snapPoint
        );

        snappedTo = snapPoint;

        lastPos = pos;
        ghost.transform.position = pos;

        canPlace = currentBuilding.data.requireSnapping ? snap : BuildUtilities.IsPositionValid(ghost.transform, currentBuilding);
        SetColor(ghost, canPlace ? Color.green : Color.red);
    }

    void SetColor(GameObject o, Color c)
    {
        c.a = .5f;
        foreach (var r in o.GetComponentsInChildren<Renderer>())
        {
            var mats = r.materials;
            for (int i = 0; i < mats.Length; i++) mats[i].color = c;
            r.materials = mats;
        }
    }

    void SetAlpha(float a, GameObject o)
    {
        foreach (var r in o.GetComponentsInChildren<Renderer>())
        {
            var mats = r.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                var c = mats[i].color; c.a = a;
                mats[i].color = c;
            }
            r.materials = mats;
        }
    }

    void TryPlace()
    {
        if (!canPlace || !ghost || !TakeResources()) return;

        var placed = Instantiate(availableBuildings[index].gameObject, lastPos, Quaternion.Euler(0, Mathf.Round(rotation), 0));

        if (availableBuildings[index].data.usesPivots)
            placed.transform.localScale = Vector3.one * buildingManager.gridSize;

        placed.tag = "Build";
        SetAlpha(1, placed);
        
        if(snappedTo != null)
        snappedTo.snappedTo = placed.GetComponent<Building>();
        placed.GetComponent<Building>()?.OnPlace(buildingManager.extraBuildingHealth, buildingManager.extraBuildingDamage);

        if (placed.TryGetComponent(out Renderer r))
            ParticleSpawner.SpawnWithBounds(spawnParticles, placed.transform.position, placed.transform.rotation, r.bounds);

        AudioManager.Instance?.PlaySound("Build", .9f, 1.25f);
    }

    void TryDemolish()
    {
        if (!BuildUtilities.TryGetHit(cam, buildingManager.sphereCastRadius, buildingManager.maxBuildDistance, buildingManager.demolishLayers, out var hit) ||
            !hit.collider.CompareTag("Build")) return;

        var b = hit.collider.GetComponent<Building>() ?? hit.collider.GetComponentInParent<Building>();
        if (!b || !RefundResources(b)) return;

        b.OnDemolish();
        Destroy(b.gameObject);
        AudioManager.Instance?.PlaySound("Demolish", .9f, 1.25f);
    }

    bool TakeResources()
    {
        var inv = heldby.GetComponent<PlayerInventory>();

        foreach (var i in availableBuildings[index].data.ingredients)
            if (!inv.HasItem(i.item, i.quantity)) return false;

        foreach (var i in availableBuildings[index].data.ingredients)
            inv.TakeItem(i.item, i.quantity, out _);

        return true;
    }

    bool RefundResources(Building b)
    {
        var inv = heldby.GetComponent<PlayerInventory>();

        foreach (var i in b.data.ingredients)
        {
            var item = Instantiate(i.item).GetComponent<Item>();
            item.HeldQuantity = i.quantity;

            inv.GiveItem(item, out bool taken);

            if (!taken)
            {
                Destroy(item);
                return false;
            }
        }
        return true;
    }

    public override void OnSelectOnce() => UpdateUiGhost();

    public override void OnPick()
    {
        if (isSelected) UpdateUiGhost();
    }

    public void UpdateUiGhost()
    {
        if (!isSelected || !isItemPickedUp) return;
        SpawnGhost();
        if (dayNightManager.currentState == DayNightCycleManager.CycleState.Day)
            buildingManager.ShowUI(true);
    }

    void OnDestroy()
    {
        if (buildingManager) buildingManager.OnGridUpdated -= UpdateUiGhost;
    }

    public override void OnSelect() =>
        buildingManager?.UpdateBuildUI(availableBuildings[index], choosing);

    public override void OnChangingItems()
    {
        buildingManager?.ShowUI(false);
        buildingManager?.ShowInspectUI(false);
        rotTween?.Kill();
        Destroy(ghost);
        choosing = false;
        PlayerInventory.Instance.CanScroll = true;
    }
}