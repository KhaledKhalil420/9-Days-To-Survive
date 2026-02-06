using UnityEngine;
using System.Collections.Generic;

//DUMBASS bug I've found, I have some pillars ok? when a floating piller is connected a ground pillar all is good, but when another piller grounded or not is connected to that floating pillar it automatically destroys for some reason. same goes for all types of buildings.. I hate this so much fix it please. write code the same way I do..  
public class Building : MonoBehaviour, IDamagable
{
    [Header("Attributes")]
    [SerializeField] internal int currentHealth = 5;
    [SerializeField] internal int extraDamage = 0;

    [Header("Building Data")]
    public Ingredient[] ingredients;
    public BuildingData data;
    
    [Header("Grid Settings")]
    public bool usesPivots = true;
    public bool affectedByGridSizePosition = true;
    public List<Transform> pivots;
    public List<Transform> pivotsOnBuild;
    
    [Header("Physics Settings")]
    [SerializeField] private float supportCheckScale = 1.15f;
    private LayerMask buildingLayers;

    private bool isPlaced = false;
    private Collider buildingCollider;
    private static HashSet<Building> checkingBuildings = new HashSet<Building>(); 

    private void Start()
    {
        buildingCollider = GetComponent<Collider>();
        buildingLayers = BuildingManager.Instance.PhysicsLayers;
    }

    #region Self destruction

    public void UpdateBuilding()
    {
        if (!isPlaced || buildingCollider == null) return;

        if (!HasGroundSupport())
        {
            Invoke(nameof(DestroyBuilding), 1);
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

    protected virtual void OnPlaced() { }
    protected virtual void OnDamage() { }
    protected virtual void OnDeath() { }

    #endregion

    private void DestroyBuilding()
    {
        OnDeath();
        Destroy(gameObject);
        if(TryGetComponent(out Renderer renderer)) ParticleSpawner.SpawnWithBounds(BuildingManager.Instance.smoke, transform.position, transform.rotation, renderer.bounds);
    }

    private void OnDestroy()
    {
        RebakeNav();

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