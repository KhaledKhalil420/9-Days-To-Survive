using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine.AI;

public class WorldGenerator : MonoBehaviour
{
    public static WorldGenerator instance;
    
    [Header("Map")]
    public Vector3 mapMin, mapMax;
    public LayerMask groundMask;

    [Header("Day Spawnables")]
    public List<DaySpawnables> days = new();

    [Header("Grass")]
    [SerializeField] private GrassPainter grassPainter;
    [SerializeField] private int maxAttemptsGrass = 10;
    [SerializeField] private int grassAmount = 500;
    [SerializeField] private bool spawnGrassAtStart = true;

    [Header("Terrain")]
    [SerializeField] private NavMeshSurface surface;
    
    [Header("NavMesh Optimization")]
    [SerializeField] private float rebakeDelay = 0.5f;
    [SerializeField] private bool useLocalBaking = false;
    [SerializeField] private float localBakeRadius = 15f;
    private Coroutine pendingBake;
    
    int currentDay;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        DayNightCycleManager.OnDayChange += SpawnNextDay;
        
        if (spawnGrassAtStart)
            SpawnGrass();

        StartCoroutine(BakeSurfaceAsync());
    }

    #region Navmesh (I have no idea what happens here I stole it from github, sue me)

    /// <summary>
    /// Request a NavMesh rebake. This is debounced to prevent multiple rapid bakes.
    /// Call this when player builds or destroys objects.
    /// </summary>
    public static void RequestNavMeshRebake()
    {
        if (instance == null) return;
        
        if (instance.pendingBake != null)
        {
            instance.StopCoroutine(instance.pendingBake);
        }
        
        instance.pendingBake = instance.StartCoroutine(instance.DebouncedBake());
    }
    
    /// <summary>
    /// Request a local NavMesh rebake around a specific position.
    /// More efficient for localized changes like building a single structure.
    /// </summary>
    public static void RequestLocalNavMeshRebake(Vector3 position, float radius = 0f)
    {
        if (instance == null) return;
        
        if (!instance.useLocalBaking)
        {
            RequestNavMeshRebake();
            return;
        }
        
        if (instance.pendingBake != null)
        {
            instance.StopCoroutine(instance.pendingBake);
        }
        
        float bakeRadius = radius > 0 ? radius : instance.localBakeRadius;
        instance.pendingBake = instance.StartCoroutine(
            instance.DebouncedLocalBake(position, bakeRadius)
        );
    }

    /// <summary>
    /// Legacy method for backward compatibility. Use RequestNavMeshRebake() instead.
    /// </summary>
    public static void BakeSurface()
    {
        RequestNavMeshRebake();
    }

    private IEnumerator DebouncedBake()
    {
        // Wait for delay - if another request comes in, this coroutine is cancelled
        yield return new WaitForSeconds(rebakeDelay);
        
        // Now actually bake
        yield return BakeSurfaceAsync();
        
        pendingBake = null;
    }
    
    private IEnumerator DebouncedLocalBake(Vector3 center, float radius)
    {
        yield return new WaitForSeconds(rebakeDelay);
        
        Bounds bounds = new Bounds(center, Vector3.one * radius * 2);
        NavMeshBuildSettings buildSettings = surface.GetBuildSettings();
        List<NavMeshBuildSource> sources = new List<NavMeshBuildSource>();
        
        // Collect sources in the specified bounds
        List<NavMeshBuildMarkup> markups = new List<NavMeshBuildMarkup>();
        NavMeshBuilder.CollectSources(
            bounds, 
            surface.layerMask, 
            surface.useGeometry, 
            surface.defaultArea,
            markups,
            sources
        );
        
        var operation = NavMeshBuilder.UpdateNavMeshDataAsync(
            surface.navMeshData, 
            buildSettings, 
            sources, 
            bounds
        );
        
        while (!operation.isDone)
        {
            yield return null;
        }
        
        pendingBake = null;
    }

    private IEnumerator BakeSurfaceAsync()
    {
        AsyncOperation op = surface.UpdateNavMesh(surface.navMeshData);
        
        while (!op.isDone)
        {
            yield return null;
        }
        
        Debug.Log("NavMesh bake complete");
    }

    #endregion

    #region Grass

    private void SpawnGrass()
    {
        int successfulSpawns = 0;
        
        for (int i = 0; i < grassAmount; i++)
        {
            Vector3 pos = new Vector3(Random.Range(mapMin.x, mapMax.x), 100, Random.Range(mapMin.z, mapMax.z));
        
            if (Physics.Raycast(pos, Vector3.down, out RaycastHit hit, 200f))
            {
                if(hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                    grassPainter.PaintGrassAtPosition(pos);
                successfulSpawns++;
            }
        }
        
        grassPainter.FinalizeMesh();
        
        GrassComputeScript grassCompute = grassPainter.GetComponent<GrassComputeScript>();
        if (grassCompute != null)
        {
            grassCompute.ReLoadGrass(this, System.EventArgs.Empty);
        }
    }

    #endregion

    #region Spawnables

    public void SpawnNextDay(bool isDay)
    {
        if (currentDay >= days.Count || !isDay) return;
        
        var day = days[currentDay];
        foreach (var spawnable in day.spawnables)
        {
            int quantity = Random.Range(Mathf.Max(1, spawnable.minQuantity), spawnable.maxQuantity + 1);
            
            for (int i = 0; i < quantity; i++)
            {
                Vector3 spawnPos = Vector3.zero;
                Quaternion spawnRot = Quaternion.identity;
                bool spawned = false;
                
                for (int attempt = 0; attempt < maxAttemptsGrass; attempt++)
                {
                    Vector3 pos = new Vector3(
                        Random.Range(mapMin.x, mapMax.x), 
                        mapMax.y, 
                        Random.Range(mapMin.z, mapMax.z)
                    );
                    
                    if (Physics.Raycast(pos, Vector3.down, out RaycastHit hit, 200f, groundMask))
                    {
                        spawnPos = hit.point;
                        spawnRot = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                        spawned = true;

                        float randSize = Random.Range(0.9f, 1.25f);
                        Instantiate(spawnable.gameObject, spawnPos, spawnRot).transform.localScale *= randSize;
                        break;
                    }
                }
                
                if (!spawned)
                {
                    spawnPos = new Vector3(Random.Range(mapMin.x, mapMax.x), mapMin.y, Random.Range(mapMin.z, mapMax.z));
                    spawnRot = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                    float randSize = Random.Range(0.9f, 1.25f);
                    Instantiate(spawnable.gameObject, spawnPos, spawnRot).transform.localScale *= randSize;
                }
            }
        }
        currentDay++;
    }

    #endregion
}

[System.Serializable]
public class DaySpawnables
{
    public List<WorldSpawnable> spawnables = new();
}

[System.Serializable]
public class WorldSpawnable
{
    public float distanceBetweenSpawnables;
    public GameObject gameObject;
    public int minQuantity = 1;
    public int maxQuantity = 3;
}