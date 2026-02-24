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
        int id = prefab.GetInstanceID();

        if (!pools.ContainsKey(id))
        {
            pools[id] = new Queue<EnemyBrain>();
            prefabRegistry[id] = prefab;
        }

        EnemyBrain enemy = pools[id].Count > 0 ? pools[id].Dequeue() : CreateNew(prefab, id);

        enemy.transform.position = GetNavMeshPosition(center, radius, minDistance);
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

    private Vector3 GetNavMeshPosition(Vector3 center, float radius, float minDistance)
    {
        for (int i = 0; i < 30; i++)
        {
            Vector2 rand2D = Random.insideUnitCircle.normalized * Random.Range(minDistance, radius);
            Vector3 candidate = center + new Vector3(rand2D.x, 0, rand2D.y);

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                return hit.position;
        }

        NavMesh.SamplePosition(center + Vector3.right * minDistance, out NavMeshHit fallback, 10f, NavMesh.AllAreas);
        return fallback.position;
    }
}