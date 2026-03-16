using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine.AI;

public class WorldGenerator : MonoBehaviour
{
    public static WorldGenerator Instance;

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

    private Dictionary<WorldSpawnable, SpawnableRuntimeData> runtimeData = new();

    int currentDay;

    private void Awake()
    {
        Instance = this;
    }

    void OnDestroy()
    {
        Instance = null;
    }

    private void Start()
    {
        DayNightCycleManager.Instance.OnDayChange += OnDayChanged;

        if (spawnGrassAtStart)
            SpawnGrass();

        StartCoroutine(BakeSurfaceAsync());
    }

    #region Navmesh (I have no idea what happens here I stole it from github, sue me)

    public static void RequestNavMeshRebake()
    {
        if (Instance == null) return;

        if (Instance.pendingBake != null)
            Instance.StopCoroutine(Instance.pendingBake);

        Instance.pendingBake = Instance.StartCoroutine(Instance.DebouncedBake());
    }

    public static void RequestLocalNavMeshRebake(Vector3 position, float radius = 0f)
    {
        if (Instance == null) return;

        if (!Instance.useLocalBaking)
        {
            RequestNavMeshRebake();
            return;
        }

        if (Instance.pendingBake != null)
            Instance.StopCoroutine(Instance.pendingBake);

        float bakeRadius = radius > 0 ? radius : Instance.localBakeRadius;
        Instance.pendingBake = Instance.StartCoroutine(Instance.DebouncedLocalBake(position, bakeRadius));
    }

    public static void BakeSurface()
    {
        RequestNavMeshRebake();
    }

    private IEnumerator DebouncedBake()
    {
        yield return new WaitForSeconds(rebakeDelay);
        yield return BakeSurfaceAsync();
        pendingBake = null;
    }

    private IEnumerator DebouncedLocalBake(Vector3 center, float radius)
    {
        yield return new WaitForSeconds(rebakeDelay);

        Bounds bounds = new Bounds(center, Vector3.one * radius * 2);
        NavMeshBuildSettings buildSettings = surface.GetBuildSettings();
        List<NavMeshBuildSource> sources = new List<NavMeshBuildSource>();
        List<NavMeshBuildMarkup> markups = new List<NavMeshBuildMarkup>();

        NavMeshBuilder.CollectSources(bounds, surface.layerMask, surface.useGeometry, surface.defaultArea, markups, sources);

        var operation = NavMeshBuilder.UpdateNavMeshDataAsync(surface.navMeshData, buildSettings, sources, bounds);

        while (!operation.isDone)
            yield return null;

        pendingBake = null;
    }

    private IEnumerator BakeSurfaceAsync()
    {
        AsyncOperation op = surface.UpdateNavMesh(surface.navMeshData);

        while (!op.isDone)
            yield return null;

        Debug.Log("NavMesh bake complete");
    }

    #endregion

    #region Grass

    private void SpawnGrass()
    {
        for (int i = 0; i < grassAmount; i++)
        {
            Vector3 pos = new Vector3(Random.Range(mapMin.x, mapMax.x), 100, Random.Range(mapMin.z, mapMax.z));

            if (Physics.Raycast(pos, Vector3.down, out RaycastHit hit, 200f))
            {
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                    grassPainter.PaintGrassAtPosition(pos);
            }
        }

        grassPainter.FinalizeMesh();

        GrassComputeScript grassCompute = grassPainter.GetComponent<GrassComputeScript>();
        if (grassCompute != null)
            grassCompute.ReLoadGrass(this, System.EventArgs.Empty);
    }

    #endregion

    #region Spawnables

    private void OnDayChanged(bool isDay)
    {
        HandleExpiry(isDay);
        SpawnNextDay(isDay);
        HandleRespawns(isDay);
    }

    public void SpawnNextDay(bool isDay)
    {
        if (currentDay >= days.Count || !isDay) return;

        var day = days[currentDay];

        foreach (var spawnable in day.spawnables)
        {
            var data = GetOrCreateData(spawnable);
            int quantity = Random.Range(Mathf.Max(1, spawnable.minQuantity), spawnable.maxQuantity + 1);
            data.targetQuantity = quantity;
            SpawnBatch(spawnable, quantity);
        }

        currentDay++;
    }

    private void HandleExpiry(bool isDay)
    {
        if (!isDay) return;

        foreach (var kvp in runtimeData)
        {
            var spawnable = kvp.Key;
            var data = kvp.Value;

            if (!spawnable.enableExpiry || spawnable.expireAfterDays <= 0) continue;

            var expired = data.spawnDayByInstance
                .Where(pair => pair.Key != null && currentDay - pair.Value >= spawnable.expireAfterDays)
                .Select(pair => pair.Key)
                .ToList();

            foreach (var go in expired)
            {
                Debug.Log($"[WorldGenerator] Expiring {spawnable.prefab.name} after {spawnable.expireAfterDays} days.");
                data.spawnDayByInstance.Remove(go);
                data.alive.Remove(go);
                Destroy(go);
            }
        }
    }

    private void HandleRespawns(bool isDay)
    {
        if (!isDay) return;

        foreach (var kvp in runtimeData)
        {
            var spawnable = kvp.Key;
            var data = kvp.Value;

            if (spawnable.respawnAfterDays <= 0) continue;
            if (data.lastSpawnDay < 0) continue;
            if (currentDay - data.lastSpawnDay < spawnable.respawnAfterDays) continue;

            int aliveCount = data.alive.Count(go => go != null);
            int deficit = data.targetQuantity - aliveCount;

            if (deficit > 0)
            {
                Debug.Log($"[WorldGenerator] Respawning {deficit}x {spawnable.prefab.name} (day {currentDay})");
                SpawnBatch(spawnable, deficit);
            }
        }
    }

    private void SpawnBatch(WorldSpawnable spawnable, int quantity)
    {
        var data = GetOrCreateData(spawnable);

        List<Vector3> occupiedPositions = data.alive
            .Where(go => go != null)
            .Select(go => go.transform.position)
            .ToList();

        for (int i = 0; i < quantity; i++)
        {
            if (spawnable.maxAlive > 0 && data.alive.Count(go => go != null) >= spawnable.maxAlive)
            {
                break;
            }

            float rolledDist = spawnable.RandomSpacingDistance();

            Vector3? anchor = TryGetGroundPosition(mapMin, mapMax, occupiedPositions, rolledDist, spawnable.overlapCheckRadius);
            if (anchor == null)
            {
                continue;
            }

            GameObject anchorGo = SpawnInstance(spawnable, anchor.Value);
            data.alive.Add(anchorGo);
            data.spawnDayByInstance[anchorGo] = currentDay;
            occupiedPositions.Add(anchor.Value);

            if (spawnable.useCluster)
            {
                int clusterCount = Random.Range(spawnable.minClusterSize, spawnable.maxClusterSize + 1);

                for (int c = 0; c < clusterCount; c++)
                {
                    if (spawnable.maxAlive > 0 && data.alive.Count(go => go != null) >= spawnable.maxAlive)
                        break;

                    float clusterDist = spawnable.RandomSpacingDistance();

                    Vector3? clusterPos = TryGetClusterPosition(anchor.Value, spawnable.clusterRadius, occupiedPositions, clusterDist, spawnable.overlapCheckRadius);
                    if (clusterPos == null) continue;

                    GameObject clusterGo = SpawnInstance(spawnable, clusterPos.Value);
                    data.alive.Add(clusterGo);
                    data.spawnDayByInstance[clusterGo] = currentDay;
                    occupiedPositions.Add(clusterPos.Value);
                }
            }
        }

        data.lastSpawnDay = currentDay;
    }

    private Vector3? TryGetGroundPosition(Vector3 min, Vector3 max, List<Vector3> occupied, float minDist, float overlapRadius)
    {
        for (int attempt = 0; attempt < maxAttemptsGrass; attempt++)
        {
            Vector3 candidate = new Vector3(Random.Range(min.x, max.x), max.y, Random.Range(min.z, max.z));

            if (!Physics.Raycast(candidate, Vector3.down, out RaycastHit hit, 200f, groundMask))
                continue;

            if (!IsFarEnough(hit.point, occupied, minDist))
                continue;

            if (IsOverlapping(hit.point, overlapRadius))
                continue;

            return hit.point;
        }

        return null;
    }

    private Vector3? TryGetClusterPosition(Vector3 anchor, float radius, List<Vector3> occupied, float minDist, float overlapRadius)
    {
        for (int attempt = 0; attempt < maxAttemptsGrass; attempt++)
        {
            Vector2 offset = Random.insideUnitCircle * radius;
            Vector3 candidate = new Vector3(anchor.x + offset.x, mapMax.y, anchor.z + offset.y);

            if (!Physics.Raycast(candidate, Vector3.down, out RaycastHit hit, 200f, groundMask))
                continue;

            if (!IsFarEnough(hit.point, occupied, minDist))
                continue;

            if (IsOverlapping(hit.point, overlapRadius))
                continue;

            return hit.point;
        }

        return null;
    }

    // Returns true if anything (except ground) is already occupying this spot
    private bool IsOverlapping(Vector3 point, float radius)
    {
        if (radius <= 0f) return false;

        Collider[] hits = Physics.OverlapSphere(point, radius);

        foreach (var col in hits)
        {
            // Ignore ground layer — it's expected to be there
            if (col.gameObject.layer == LayerMask.NameToLayer("Ground")) continue;

            return true;
        }

        return false;
    }

    private GameObject SpawnInstance(WorldSpawnable spawnable, Vector3 position)
    {
        Quaternion rot = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
        float scale = Random.Range(0.9f, 1.25f);
        GameObject go = Instantiate(spawnable.prefab, position, rot);
        go.transform.localScale *= scale;
        return go;
    }

    private bool IsFarEnough(Vector3 candidate, List<Vector3> existing, float minDistance)
    {
        if (minDistance <= 0f) return true;

        foreach (var pos in existing)
        {
            if (Vector3.Distance(candidate, pos) < minDistance)
                return false;
        }

        return true;
    }

    private SpawnableRuntimeData GetOrCreateData(WorldSpawnable spawnable)
    {
        if (!runtimeData.ContainsKey(spawnable))
            runtimeData[spawnable] = new SpawnableRuntimeData();
        return runtimeData[spawnable];
    }

    #endregion
}

public class SpawnableRuntimeData
{
    public List<GameObject> alive = new();
    public Dictionary<GameObject, int> spawnDayByInstance = new();
    public int targetQuantity;
    public int lastSpawnDay = -1;
}

[System.Serializable]
public class DaySpawnables
{
    public List<WorldSpawnable> spawnables = new();
}

[System.Serializable]
public class WorldSpawnable
{
    public GameObject prefab;
    public int minQuantity = 1;
    public int maxQuantity = 3;

    [Header("Spacing")]
    public float minSpacingDistance = 2f;
    public float maxSpacingDistance = 6f;

    [Header("Overlap Check")]
    [Tooltip("Radius to check for existing colliders before spawning. Set to 0 to skip.")]
    public float overlapCheckRadius = 1f;

    [Header("Cluster")]
    public bool useCluster = true;
    public int minClusterSize = 1;
    public int maxClusterSize = 3;
    public float clusterRadius = 5f;

    [Header("Cap")]
    public int maxAlive = 20;

    [Header("Respawn")]
    public int respawnAfterDays = 3;

    [Header("Expiry")]
    [Tooltip("Enable automatic removal of this resource after a set number of days.")]
    public bool enableExpiry = false;
    [Tooltip("Destroy this resource after this many days alive.")]
    public int expireAfterDays = 5;

    public float RandomSpacingDistance() => Random.Range(minSpacingDistance, maxSpacingDistance);
}