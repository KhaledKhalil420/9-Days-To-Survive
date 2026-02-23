using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    public static AIManager Instance;

    [Header("Tick Rate")]
    [SerializeField] private float minTickRate = 0.1f;
    [SerializeField] private float maxTickRate = 0.4f;
    [SerializeField] private int enemyCountForMaxTickRate = 50;

    public List<EnemyBrain> registeredEnemies = new();
    public static LayerMask UnDetectableLayers => Instance.unDetectableLayers;
    [SerializeField] private LayerMask unDetectableLayers;

    private float timer;
    private float currentTickRate;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        //Smoothly scale tickrate depending on enemies.. (maybe it's better to set it instead of lerping but idk)
        currentTickRate = Mathf.Lerp(minTickRate, maxTickRate, (float)registeredEnemies.Count / enemyCountForMaxTickRate);

        timer += Time.deltaTime;
        if (timer < currentTickRate) return;

        float delta = timer;
        timer = 0f;

        for (int i = registeredEnemies.Count - 1; i >= 0; i--)
        {
            registeredEnemies[i].TickBrain(delta);
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