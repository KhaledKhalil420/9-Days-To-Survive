using UnityEngine;

public class EnemySpawner
{
    public static void SpawnWave(Wave wave, Vector3 playerPosition, float spawnRadius, float minDistance)
    {
        EnemyBrain enemy = wave.enemies[Random.Range(0, wave.enemies.Count)];
        Object.Instantiate(enemy, GetValidSpawnPosition(playerPosition, spawnRadius, minDistance, enemy.transform), Quaternion.identity);
    }

    static Vector3 GetValidSpawnPosition(Vector3 center, float radius, float minDistance, Transform enemy)
    {
        Vector3 pos = Vector3.zero;
        int attempts = 0;
        center.y = 0;

        do
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float dist = Random.Range(minDistance, radius);
            pos = center + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * dist;
            pos = pos + new Vector3(0, enemy.localScale.y, 0);
            attempts++;
            if (attempts > 20) 
                break; 
        } while (Vector3.Distance(pos, center) < minDistance);

        return pos;
    }
}
