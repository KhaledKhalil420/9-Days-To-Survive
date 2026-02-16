using System;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    public static AIManager Instance;

    [SerializeField] private float tickRate = 0.01f;
    public List<EnemyBrain> registeredEnemies = new();
    public static LayerMask UnDetectableLayers => Instance.unDetectableLayers;
    [SerializeField] private LayerMask unDetectableLayers;
    private float timer;

    private void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer < tickRate) return;
        timer = 0f;

        for (int i = registeredEnemies.Count - 1; i >= 0; i--)
        {
            registeredEnemies[i].TickBrain();
        }
    }

    public static void Register(EnemyBrain e) => Instance.registeredEnemies.Add(e);
    public static void UnRegister(EnemyBrain e) => Instance.registeredEnemies.Remove(e);

    public static void KillAll()
    {
        foreach (EnemyBrain enemy in Instance.registeredEnemies.ToArray())
        {
            UnRegister(enemy);
            Destroy(enemy.gameObject);
        }
    }
}