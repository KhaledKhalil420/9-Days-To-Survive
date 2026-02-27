using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyPool : MonoBehaviour
{
    public static EnemyPool Instance;

    private Dictionary<int, Queue<EnemyBrain>> pools = new();
    private Dictionary<EnemyBrain, int> instanceToPrefabId = new();
    private Dictionary<int, EnemyBrain> prefabRegistry = new();

    private void Awake() => Instance = this;
    private void OnDestroy() => Instance = null;

    public void Prewarm(List<EnemyBrain> prefabs, int countEach = 5)
    {
        foreach (EnemyBrain prefab in prefabs)
        {
            int id = prefab.GetInstanceID();

            if (!pools.ContainsKey(id))
            {
                pools[id] = new Queue<EnemyBrain>();
                prefabRegistry[id] = prefab;
            }

            for (int i = 0; i < countEach; i++)
            {
                EnemyBrain instance = CreateNew(prefab, id);
                instance.gameObject.SetActive(false);
                pools[id].Enqueue(instance);
            }
        }
    }

    public EnemyBrain Spawn(EnemyBrain prefab, Vector3 center, float radius, float minDistance)
    {
        // FIX: guard against invalid center before doing anything
        if (!IsFinite(center))
        {
            Debug.LogWarning("EnemyPool.Spawn: center position is not finite, aborting spawn.");
            return null;
        }

        int id = prefab.GetInstanceID();

        if (!pools.ContainsKey(id))
        {
            pools[id] = new Queue<EnemyBrain>();
            prefabRegistry[id] = prefab;
        }

        EnemyBrain enemy = pools[id].Count > 0 ? pools[id].Dequeue() : CreateNew(prefab, id);

        if (!TryGetNavMeshPosition(center, radius, minDistance, out Vector3 spawnPos))
        {
            Debug.LogWarning("EnemyPool.Spawn: could not find a valid NavMesh position, returning enemy to pool.");
            pools[id].Enqueue(enemy);
            return null;
        }

        enemy.transform.position = spawnPos;
        enemy.transform.rotation = Quaternion.identity;
        enemy.gameObject.SetActive(true);
        enemy.OnSpawn();

        return enemy;
    }

    public void Return(EnemyBrain enemy)
    {
        if (enemy == null) return;
        if (!instanceToPrefabId.TryGetValue(enemy, out int id)) return;

        enemy.OnDespawn();
        enemy.gameObject.SetActive(false);
        pools[id].Enqueue(enemy);
    }

    public void ReturnAll()
    {
        foreach (var (enemy, id) in instanceToPrefabId)
        {
            if (enemy == null || !enemy.gameObject.activeSelf) continue;
            enemy.OnDespawn();
            enemy.gameObject.SetActive(false);
            pools[id].Enqueue(enemy);
        }
    }

    private EnemyBrain CreateNew(EnemyBrain prefab, int id)
    {
        EnemyBrain instance = Instantiate(prefab, transform);
        instance.gameObject.SetActive(false);
        instanceToPrefabId[instance] = id;
        return instance;
    }
    
    private bool TryGetNavMeshPosition(Vector3 center, float radius, float minDistance, out Vector3 result)
    {
        center.y = 0;

        for (int i = 0; i < 30; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float dist  = Random.Range(minDistance, radius);
            Vector3 candidate = center + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * dist;

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }

        if (NavMesh.SamplePosition(center + Vector3.right * minDistance, out NavMeshHit fallback, 10f, NavMesh.AllAreas))
        {
            result = fallback.position;
            return true;
        }

        result = Vector3.zero;
        return false;
    }

    private static bool IsFinite(Vector3 v) =>
        !float.IsInfinity(v.x) && !float.IsInfinity(v.y) && !float.IsInfinity(v.z) &&
        !float.IsNaN(v.x)      && !float.IsNaN(v.y)      && !float.IsNaN(v.z);
}