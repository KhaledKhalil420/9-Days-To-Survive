using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public List<DaySpawnables> days = new();
    public Vector3 mapMin, mapMax;
    public LayerMask groundMask;
    public int maxAttempts = 10;
    public GrassPainter grassPainter;
    public bool spawnGrassAtStart = true;
    public int grassAmount = 500;
    
    int currentDay;

    private void Start()
    {
        SpawnNextDay();
        
        if (spawnGrassAtStart)
            SpawnGrass();
    }

    void SpawnGrass()
    {
        int successfulSpawns = 0;
        
        // Paint all grass positions WITHOUT building mesh each time
        for (int i = 0; i < grassAmount; i++)
        {
            Vector3 pos = new Vector3(
                Random.Range(mapMin.x, mapMax.x), 
                100, // High Y position to raycast down from
                Random.Range(mapMin.z, mapMax.z)
            );
            
            if (Physics.Raycast(pos, Vector3.down, out RaycastHit hit, 200f))
            {
                // Use PaintGrassAtPosition to add grass without building mesh

                if(hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                grassPainter.PaintGrassAtPosition(pos);
                successfulSpawns++;
            }
        }
        
        // Build the mesh ONCE after all grass is placed
        grassPainter.FinalizeMesh();
        
        // Force the GrassComputeScript to reload
        GrassComputeScript grassCompute = grassPainter.GetComponent<GrassComputeScript>();
        if (grassCompute != null)
        {
            grassCompute.ReLoadGrass(this, System.EventArgs.Empty);
        }
        
        Debug.Log($"Spawned grass at {successfulSpawns} locations with {grassPainter.i} total grass blades");
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
    public int minQuantity = 1;
    public int maxQuantity = 3;
}