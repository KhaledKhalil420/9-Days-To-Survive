using System;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour, IDamagable
{
    [Header("Health")]
    public int currentHealth = 5;
    
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

    private void Awake()
    {
        buildingCollider = GetComponent<Collider>();
        buildingLayers = BuildManager.Instance.PhysicsLayers;
    }

    #region Self destruction

    public void UpdateBuilding()
    {
        if (!isPlaced || buildingCollider == null) return;

        if (!HasGroundSupport())
        {
            Invoke(nameof(DestroyBuilding), 0.15f);
        }
    }

    private bool HasGroundSupport()
    {
        if (checkingBuildings.Contains(this))
            return false;

        checkingBuildings.Add(this);

        Bounds bounds = buildingCollider.bounds;
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

    public void OnPlace()
    {
        if (isPlaced) 
            return;
        
        isPlaced = true;
        
        pivots.AddRange(pivotsOnBuild);
        BuildManager.Instance.OnGridUpdated += UpdateBuilding;
        BuildManager.Instance.UpdateGrid();
        
        RebakeNav();
        OnPlaced();
    }

    private void RebakeNav()
    {
        if (DayNightCycleManager.instance?.currentState == DayNightCycleManager.CycleState.Night)
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
    }

    private void OnDestroy()
    {
        RebakeNav();

        BuildManager.Instance.OnGridUpdated -= UpdateBuilding;
        BuildManager.Instance.UpdateGrid();

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