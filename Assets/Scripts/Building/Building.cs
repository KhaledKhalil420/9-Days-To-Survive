using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public class Building : MonoBehaviour, IDamagable
{
    [Header("Attributes")]
    [SerializeField] internal int currentHealth = 5;
    [SerializeField] internal int extraDamage = 0;

    [Header("Building Data")]
    public Ingredient[] ingredients;
    public BuildingData data;
    public bool dropResourcesOnDestory = false;
    
    [Header("Grid Settings")]
    public bool usesPivots = true;
    public bool affectedByGridSizePosition = true;
    public List<Transform> pivots;
    public List<Transform> pivotsOnBuild;
    
    [Header("Physics Settings")]
    [SerializeField] private float supportCheckScale = 1.15f;
    private LayerMask buildingLayers;

    private bool isPlaced = false;
    private BoxCollider buildingCollider;
    private NavMeshObstacle obstacle;
    private static HashSet<Building> checkingBuildings = new HashSet<Building>(); 

    private void Start()
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
    }

    #region Self destruction

    public void UpdateBuilding()
    {
        if (!isPlaced || buildingCollider == null) return;

        if (!HasGroundSupport())
        {
            Invoke(nameof(DestroyBuilding), 0.5f);
        }
    }

    private bool HasGroundSupport()
    {
        if (checkingBuildings.Contains(this))
            return true;

        checkingBuildings.Add(this);
        Bounds bounds;

        if(buildingCollider == null)
        {
            buildingCollider = gameObject.GetComponent<BoxCollider>();

            if(buildingCollider == null)
            {
                buildingCollider = gameObject.AddComponent<BoxCollider>();
            }
        }

         bounds = buildingCollider.bounds;

        Debug.Log(gameObject);
        Collider[] hits = Physics.OverlapBox(bounds.center, bounds.extents * supportCheckScale, transform.rotation, buildingLayers, QueryTriggerInteraction.Ignore);

        bool hasSupport = false;

        foreach (var hit in hits)
        {
            if (hit == buildingCollider) continue;
            
            if (hit.CompareTag("Ground"))
            {
                hasSupport = true;
                break;
            }
            
            if (hit.CompareTag("Build"))
            {
                if(hit.TryGetComponent(out Building supportingBuilding))
                {
                    if (supportingBuilding.HasGroundSupport())
                    {
                        hasSupport = true;
                        break;
                    }
                }
            }
        }

        checkingBuildings.Remove(this);
        return hasSupport;
    }

    #endregion

    public void OnPlace(float extraHealth, float extraDamage)
    {
        if (isPlaced) 
            return;
        
        isPlaced = true;

        currentHealth += (int)extraHealth;
        extraDamage += extraDamage;
        
        pivots.AddRange(pivotsOnBuild);
        BuildingManager.Instance.OnGridUpdated += UpdateBuilding;
        BuildingManager.Instance.UpdateGrid();
        
        RebakeNav();
        OnPlaced();
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

    #endregion

    private void DestroyBuilding()
    {
        OnDeath();
        Destroy(gameObject);

        if(dropResourcesOnDestory)
        {
            foreach(Ingredient ingredient in ingredients)
            {
                Item item = Instantiate(ingredient.item, transform.position, transform.rotation).GetComponent<Item>();
                item.HeldQuantity = ingredient.quantity;
            }
        }
        
        if(TryGetComponent(out Renderer renderer)) ParticleSpawner.SpawnWithBounds(BuildingManager.Instance.smoke, transform.position, transform.rotation, renderer.bounds);
    }

    private void OnDestroy()
    {
        OnDeath();
        RebakeNav();
        
        if(obstacle != null)
            obstacle.enabled = false;

        BuildingManager.Instance.OnGridUpdated -= UpdateBuilding;
        BuildingManager.Instance.UpdateGrid();

    }

    public void Damage(float damage)
    {
        currentHealth -= (int)damage;
        OnDamage();
        
        if (currentHealth <= 0)
        {
            DestroyBuilding();
        }
    }
}