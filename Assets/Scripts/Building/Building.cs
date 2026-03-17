using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.AI;

public class Building : MonoBehaviour, IDamagable
{
    [Header("Attributes")]
    internal float initHealth;
    [SerializeField] internal float currentHealth = 5;
    [SerializeField] internal float extraDamage = 0;
    [SerializeField] internal float currentDamage;

    [Header("Building Data")]
    public Ingredient[] ingredients = Array.Empty<Ingredient>();
    public BuildingData data;
    public bool dropResourcesOnDestory = false;

    [Header("Snapping")]
    public bool requireSnapping = false;
    public bool usesPivots = true;
    public List<Transform> pivots;
    public List<Transform> pivotsOnBuild;
    public List<Transform> pivotsOnBuildDisable;

    [Header("Physics")]
    [SerializeField] private float supportCheckScale = 1.15f;
    private LayerMask buildingLayers;

    [Header("Sounds")]
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip onDestroySound;
    [SerializeField] private AudioClip onDamageSound;

    internal bool isPlaced = false;
    private BoxCollider buildingCollider;
    private NavMeshObstacle obstacle;
    private static HashSet<Building> checkingBuildings = new();

    private void Awake()
    {
        buildingCollider = GetComponent<BoxCollider>();
        buildingLayers = BuildingManager.Instance.PhysicsLayers;

        if (TryGetComponent(out obstacle))
        {
            obstacle.carving = true;
            obstacle.carveOnlyStationary = false;
            obstacle.shape = NavMeshObstacleShape.Box;
            obstacle.center = buildingCollider.center;
            obstacle.size = buildingCollider.size + Vector3.one * 0.2f;
        }

        source = GetComponent<AudioSource>();
        if (source == null) source = gameObject.AddComponent<AudioSource>();

        initHealth = currentHealth;
    }

    private void Start()
    {
        Upgrades();
    }

    #region Support

    private bool isPendingDestroy = false;

    public void UpdateBuilding()
    {
        if (!isPlaced || buildingCollider == null || isPendingDestroy) return;

        if (!HasGroundSupport())
        {
            isPendingDestroy = true;
            Invoke(nameof(DestroyBuilding), 0.25f);
            return;
        }

        CheckUpgrades();
    }

    private bool HasGroundSupport()
    {
        //avoid infinite loops from circular support chains
        if (checkingBuildings.Contains(this)) return false;
        checkingBuildings.Add(this);

        try
        {
            if (buildingCollider == null)
                buildingCollider = GetComponent<BoxCollider>() ?? gameObject.AddComponent<BoxCollider>();

            Collider[] hits = Physics.OverlapBox(
                buildingCollider.bounds.center,
                buildingCollider.bounds.extents * supportCheckScale,
                transform.rotation, buildingLayers, QueryTriggerInteraction.Ignore);

            foreach (var hit in hits)
            {
                if (hit == buildingCollider) continue;
                if (hit.CompareTag("Ground")) return true;
                if (hit.CompareTag("Build") && hit.TryGetComponent(out Building b))
                    if (b.isPlaced && b.HasGroundSupport()) return true;
            }

            return false;
        }
        finally
        {
            checkingBuildings.Remove(this);
        }
    }

    #endregion

    #region Placement

    public void OnPlace(float extraHealth, float _extraDamage)
    {
        if (isPlaced) return;

        isPlaced = true;
        currentHealth += (int)extraHealth;
        extraDamage += _extraDamage;

        pivots.AddRange(pivotsOnBuild);
        BuildingManager.Instance.OnGridUpdated += UpdateBuilding;
        BuildingManager.Instance.UpdateGrid();
        DayNightCycleManager.Instance.OnDayChange += ResetHealth;

        foreach (Transform p in pivotsOnBuildDisable) Destroy(p.gameObject);

        RebakeNav();
        OnPlaced();
    }

    private void ResetHealth(bool isDay)
    {
        if (isDay) return;

        initHealth += BuildingManager.Instance.extraBuildingHealth;
        currentHealth = initHealth;
        extraDamage = BuildingManager.Instance.extraBuildingDamage;
        CheckUpgrades();
    }

    private void RebakeNav()
    {
        if (DayNightCycleManager.Instance?.currentState == DayNightCycleManager.CycleState.Night)
            WorldGenerator.RequestNavMeshRebake();
    }

    #endregion

    #region Virtuals

    public virtual void OnPlaced() { }
    public virtual void OnDamage() { }
    public virtual void OnDeath() { }
    public virtual void OnDemolish() { }

    #endregion

    #region Damage & Death

    public void Damage(float damage)
    {
        currentHealth -= (int)damage;
        OnDamage();

        if (onDamageSound != null) source?.PlayOneShot(onDamageSound, 0.8f);
        if (currentHealth <= 0) DestroyBuilding();
    }

    private void DestroyBuilding()
    {
        OnDeath();

        if (dropResourcesOnDestory || DayNightCycleManager.Instance.currentState == DayNightCycleManager.CycleState.Day)
        {
            foreach (Ingredient ing in ingredients)
            {
                Item item = Instantiate(ing.item, transform.position, transform.rotation).GetComponent<Item>();
                item.HeldQuantity = ing.quantity;
            }
        }

        if (TryGetComponent(out Renderer renderer))
            ParticleSpawner.SpawnWithBounds(BuildingManager.Instance.smoke, transform.position, transform.rotation, renderer.bounds);

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (!isPlaced) return;

        RebakeNav();

        if (DayNightCycleManager.Instance == null || BuildingManager.Instance == null) return;

        DayNightCycleManager.Instance.OnDayChange -= ResetHealth;
        BuildingManager.Instance.OnGridUpdated -= UpdateBuilding;
        BuildingManager.Instance.UpdateGrid();
    }

    #endregion

    #region Upgrades

    public void CheckUpgrades() => Upgrades();

    private bool isHealthOnEnemyDeath = false;

    private void Upgrades()
    {
        if (!isHealthOnEnemyDeath && UpgradeManager.Instance.IncreaseHealthOnEnemyDeath)
        {
            UpgradeManager.Instance.OnEnemyDeath += IncreaseHealthOnEnemyDeath;
            isHealthOnEnemyDeath = true;
        }
    }

    public void IncreaseHealthOnEnemyDeath()
    {
        if (!UpgradeManager.Instance.IncreaseHealthOnEnemyDeath) return;

        //50% chance to heal
        if (UnityEngine.Random.Range(0, 2) == 0) return;

        float percent = UnityEngine.Random.Range(0.025f, 0.05f);
        float heal = initHealth * percent * UpgradeManager.Instance.IncreaseHealthOnEnemyDeathMultiplier;
        currentHealth = Mathf.Min(currentHealth + heal, initHealth);
    }

    #endregion

    #region Editor

    private void OnDrawGizmosSelected()
    {
        BoxCollider col = buildingCollider != null ? buildingCollider : GetComponent<BoxCollider>();
        if (col == null) return;

        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.matrix = Matrix4x4.TRS(col.bounds.center, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, col.bounds.extents * supportCheckScale * 2);
    }

    #endregion
}