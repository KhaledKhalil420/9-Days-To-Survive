using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using System;
using UnityEngine.Audio;
using NUnit.Framework;

public class Building : MonoBehaviour, IDamagable
{
    [Header("Attributes")]
    internal float initHealth;
    [SerializeField] internal float currentHealth = 5;
    [SerializeField] internal float extraDamage = 0;

    [Header("Building Data")]
    public Ingredient[] ingredients = Array.Empty<Ingredient>();
    public BuildingData data;
    public bool dropResourcesOnDestory = false;
    
    [Header("Grid Settings")]
    public bool usesPivots = true;
    public bool affectedByGridSizePosition = true;
    public List<Transform> pivots;
    public List<Transform> pivotsOnBuild;
    public List<Transform> pivotsOnBuildDisable;

    
    [Header("Physics Settings")]
    [SerializeField] private float supportCheckScale = 1.15f;
    private LayerMask buildingLayers;

    [Header("Sounds")]
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip onDestroySound;
    [SerializeField] private AudioClip onDamageSound;


    internal bool isPlaced = false;
    private BoxCollider buildingCollider;
    private NavMeshObstacle obstacle;
    private static HashSet<Building> checkingBuildings = new HashSet<Building>(); 

    private void Awake()
    {
        buildingCollider = GetComponent<BoxCollider>();
        buildingLayers = BuildingManager.Instance.PhysicsLayers;

        //Setup obstacle
        if(TryGetComponent(out obstacle))
        {
            obstacle.carving = true;
            obstacle.carveOnlyStationary = false;
            obstacle.shape = NavMeshObstacleShape.Box;
            obstacle.center = buildingCollider.center;
            obstacle.size = buildingCollider.size;
            obstacle.size += Vector3.one * 0.2f;
        }
        
        source = GetComponent<AudioSource>();
        if(source == null) source = gameObject.AddComponent<AudioSource>();

        initHealth = currentHealth;
    }

    private void Start()
    {
        Upgrades();
    }

    #region Self destruction

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
        if (checkingBuildings.Contains(this))
            return true;

        checkingBuildings.Add(this);

        try
        {
            if (buildingCollider == null)
            {
                buildingCollider = gameObject.GetComponent<BoxCollider>();
                if (buildingCollider == null)
                    buildingCollider = gameObject.AddComponent<BoxCollider>();
            }

            Bounds bounds = buildingCollider.bounds;
            Collider[] hits = Physics.OverlapBox(bounds.center, bounds.extents * supportCheckScale, transform.rotation, buildingLayers, QueryTriggerInteraction.Ignore);

            foreach (var hit in hits)
            {
                if (hit == buildingCollider) continue;

                if (hit.CompareTag("Ground"))
                    return true;

                if (hit.CompareTag("Build") && hit.TryGetComponent(out Building supportingBuilding))
                {
                    if (supportingBuilding.isPlaced && supportingBuilding.HasGroundSupport())
                        return true;
                }
            }

            return false;
        }
        finally
        {
            checkingBuildings.Remove(this);
        }
    }

    #endregion

    public void OnPlace(float extraHealth, float _extraDamage)
    {
        if (isPlaced) 
            return;
        
        isPlaced = true;

        currentHealth += (int)extraHealth;
        extraDamage += _extraDamage;
        
        pivots.AddRange(pivotsOnBuild);
        BuildingManager.Instance.OnGridUpdated += UpdateBuilding;
        BuildingManager.Instance.UpdateGrid();
        DayNightCycleManager.Instance.OnDayChange += ResetHealth;

        foreach(Transform pivot in pivotsOnBuildDisable) Destroy(pivot.gameObject);
        
        RebakeNav();
        OnPlaced();
    }

    private void ResetHealth(bool isDay)
    {
        if(isDay)
        {
            currentHealth = initHealth;
        }
    }

    private void RebakeNav()
    {
        if (DayNightCycleManager.Instance?.currentState == DayNightCycleManager.CycleState.Night)
        {
            WorldGenerator.RequestNavMeshRebake();
        }
    }

    #region Virtuals

    public virtual void OnPlaced() { }
    public virtual void OnDamage() { }
    public virtual void OnDeath() { }
    public virtual void OnDemolish(){ }

    #endregion

    private void DestroyBuilding()
    {
        OnDeath();

        if(dropResourcesOnDestory)
        {
            foreach(Ingredient ingredient in ingredients)
            {
                Item item = Instantiate(ingredient.item, transform.position, transform.rotation).GetComponent<Item>();
                item.HeldQuantity = ingredient.quantity;
            }
        }
        
        if(TryGetComponent(out Renderer renderer)) ParticleSpawner.SpawnWithBounds(BuildingManager.Instance.smoke, transform.position, transform.rotation, renderer.bounds);

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if(!isPlaced) 
            return; 
        RebakeNav();

        BuildingManager.Instance.OnGridUpdated -= UpdateBuilding;
        BuildingManager.Instance.UpdateGrid();
    }

    public void Damage(float damage)
    {
        currentHealth -= (int)damage;
        OnDamage();

        if(onDamageSound != null)
            source.PlayOneShot(onDamageSound, 0.8f);
        
        if (currentHealth <= 0)
        {
            DestroyBuilding();
        }
    }

    #region Upgrades
    
    public void CheckUpgrades()
    {
        Upgrades();
    }

    private void Upgrades()
    {
        //Do here
        if(!isHealthOnEnemyDeath && UpgradeManager.Instance.IncreaseHealthOnEnemyDeath)
        {
            UpgradeManager.Instance.OnEnemyDeath += IncreaseHealthOnEnemyDeath;
            isHealthOnEnemyDeath = true;
        }
    }

    private bool isHealthOnEnemyDeath = false;
    public void IncreaseHealthOnEnemyDeath()
    {
        if(UpgradeManager.Instance.IncreaseHealthOnEnemyDeath)
        {
            currentHealth += (initHealth * 0.15f) * UpgradeManager.Instance.IncreaseHealthOnEnemyDeathMultiplier;
        }
    }


    #endregion
}