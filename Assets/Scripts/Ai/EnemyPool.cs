using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyPool : MonoBehaviour
{
    public static EnemyPool Instance;

    //Pool per prefab (keyed by prefab instance ID)
    private Dictionary<int, Queue<GroundEnemy>> pools = new();

    //Reverse lookup: instance -> prefab ID so we know which queue to return to
    private Dictionary<GroundEnemy, int> instanceToPrefabId = new();

    //Prefab registry: ID -> prefab so we can Instantiate more if pool runs dry
    private Dictionary<int, GroundEnemy> prefabRegistry = new();

    #region Unity

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    #endregion

    #region Pool

    public void Prewarm(List<GroundEnemy> prefabs, int countEach = 5)
    {
        foreach (GroundEnemy prefab in prefabs)
        {
            int id = prefab.GetInstanceID();

            if (!pools.ContainsKey(id))
            {
                pools[id] = new Queue<GroundEnemy>();
                prefabRegistry[id] = prefab;
            }

            for (int i = 0; i < countEach; i++)
            {
                GroundEnemy instance = CreateNew(prefab, id);
                instance.gameObject.SetActive(false);
                pools[id].Enqueue(instance);
            }
        }
    }

    //Spawn an enemy from the pool at a random valid navmesh position around the player
    public GroundEnemy Spawn(GroundEnemy prefab, Vector3 center, float radius, float minDistance)
    {
        int id = prefab.GetInstanceID();

        if (!pools.ContainsKey(id))
        {
            pools[id] = new Queue<GroundEnemy>();
            prefabRegistry[id] = prefab;
        }

        GroundEnemy enemy = pools[id].Count > 0
            ? pools[id].Dequeue()
            : CreateNew(prefab, id);

        Vector3 spawnPos = GetNavMeshPosition(center, radius, minDistance);
        enemy.transform.position = spawnPos;
        enemy.transform.rotation = Quaternion.identity;
        enemy.gameObject.SetActive(true);
        enemy.OnSpawn();

        return enemy;
    }

    public void Return(GroundEnemy enemy)
    {
        if (enemy == null) return;
        if (!instanceToPrefabId.TryGetValue(enemy, out int id)) return;

        enemy.OnDespawn();
        enemy.gameObject.SetActive(false);
        pools[id].Enqueue(enemy);
    }

    #endregion

    #region Helpers

    private GroundEnemy CreateNew(GroundEnemy prefab, int id)
    {
        GroundEnemy instance = Instantiate(prefab, transform);
        instance.gameObject.SetActive(false);
        instanceToPrefabId[instance] = id;
        return instance;
    }

    //Find a valid navmesh point within radius but outside minDistance
    private Vector3 GetNavMeshPosition(Vector3 center, float radius, float minDistance)
    {
        for (int i = 0; i < 30; i++)
        {
            Vector2 rand2D = Random.insideUnitCircle.normalized * Random.Range(minDistance, radius);
            Vector3 candidate = center + new Vector3(rand2D.x, 0, rand2D.y);

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                return hit.position;
        }

        //Fallback: just return a point on the navmesh near center
        NavMesh.SamplePosition(center + Vector3.right * minDistance, out NavMeshHit fallback, 10f, NavMesh.AllAreas);
        return fallback.position;
    }

    #endregion
}