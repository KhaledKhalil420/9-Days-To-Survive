using System;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour, IBreakable
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
    
    private bool isPlaced = false;
    private Collider buildingCollider;
    private static HashSet<Building> checkingBuildings = new HashSet<Building>(); 

    private void Awake()
    {
        buildingCollider = GetComponent<Collider>();
    }

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
        Collider[] hits = Physics.OverlapBox(bounds.center, bounds.extents * supportCheckScale, transform.rotation, BuildManager.Instance.PhysicsLayers, QueryTriggerInteraction.Ignore);

        bool hasSupport = false;

        foreach (var hit in hits)
        {
            if (hit == buildingCollider) continue;
            
            // Direct ground contact = supported
            if (hit.CompareTag("Ground"))
            {
                hasSupport = true;
                break;
            }
            
            if (hit.CompareTag("Build"))
            {
                Building supportingBuilding = hit.GetComponent<Building>();
                if (supportingBuilding != null && supportingBuilding.HasGroundSupport())
                {
                    hasSupport = true;
                    break;
                }
            }
        }

        checkingBuildings.Remove(this);
        return hasSupport;
    }

    public void OnPlace()
    {
        if (isPlaced) return;
        
        isPlaced = true;
        
        pivots.AddRange(pivotsOnBuild);
        BuildManager.Instance.OnGridUpdated += UpdateBuilding;
        BuildManager.Instance.UpdateGrid();
        
        RebakeNav();
        OnPlaced();
    }

    public void Damage(GameObject sender, int damage, BreakableType type, int toughness)
    {
        if (type != BreakableType.Buildings) return;
        
        currentHealth -= damage;
        OnDamage();
        
        if (currentHealth <= 0)
        {
            DestroyBuilding();
        }
    }

    private void DestroyBuilding()
    {
        OnDeath();
        Destroy(gameObject);
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

    private void OnDestroy()
    {
        RebakeNav();
        if (BuildManager.Instance != null)
        {
            BuildManager.Instance.OnGridUpdated -= UpdateBuilding;
            BuildManager.Instance.UpdateGrid();
        }
    }

    #region Debug

    private void OnDrawGizmosSelected()
    {
        if (buildingCollider == null) buildingCollider = GetComponent<Collider>();
        if (buildingCollider == null) return;
        
        Gizmos.color = Color.yellow;
        Bounds bounds = buildingCollider.bounds;
        Gizmos.matrix = Matrix4x4.TRS(bounds.center, transform.rotation, bounds.size * supportCheckScale);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }

    #endregion
}