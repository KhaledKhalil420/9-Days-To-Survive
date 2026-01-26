using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    public static AIManager Instance;

    [SerializeField] private float intreval = 0.1f;
    public List<Enemy> registeredEnemies = new();
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
        if (timer < intreval) return;
        timer = 0f;

        foreach (var e in registeredEnemies)
            e.UpdateBrain(); 
    }

    public static void Register(Enemy e) => Instance.registeredEnemies.Add(e);
    public static void Deregister(Enemy e) => Instance.registeredEnemies.Remove(e);
}
