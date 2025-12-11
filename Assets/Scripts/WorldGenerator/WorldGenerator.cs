using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public List<DaySpawnables> days = new();
    public Vector3 mapMin, mapMax;
    public LayerMask groundMask;
    public int maxAttempts = 10;

    int currentDay;

    private void Start()
    {
        SpawnNextDay();
    }

    public void SpawnNextDay()
    {
        if (currentDay >= days.Count) return;

        var day = days[currentDay];

        foreach (var spawnable in day.spawnables)
        {
            int quantity = Random.Range(Mathf.Max(1, spawnable.minQuantity), spawnable.maxQuantity + 1);

            for (int i = 0; i < quantity; i++)
            {
                Vector3 spawnPos = Vector3.zero;
                Quaternion spawnRot = Quaternion.identity;
                bool spawned = false;

                for (int attempt = 0; attempt < maxAttempts; attempt++)
                {
                    Vector3 pos = new(
                        Random.Range(mapMin.x, mapMax.x),
                        mapMax.y,
                        Random.Range(mapMin.z, mapMax.z)
                    );

                    if (Physics.Raycast(pos, Vector3.down, out RaycastHit hit, 200f, groundMask))
                    {
                        spawnPos = hit.point;
                        spawnRot = Quaternion.FromToRotation(Vector3.up, hit.normal) * Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                        spawned = true;
                        Instantiate(spawnable.gameObject, spawnPos, spawnRot);
                        break;
                    }
                }

                if (!spawned)
                {
                    spawnPos = new Vector3(
                        Random.Range(mapMin.x, mapMax.x),
                        mapMin.y,
                        Random.Range(mapMin.z, mapMax.z)
                    );
                    spawnRot = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                    Instantiate(spawnable.gameObject, spawnPos, spawnRot);
                }
            }
        }

        currentDay++;
    }
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
    public int minQuantity = 1; // minimum spawn per day
    public int maxQuantity = 3; // maximum spawn per day
}
