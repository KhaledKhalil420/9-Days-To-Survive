using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    public static AIManager instance;

    [SerializeField] private float intreval = 0.1f;
    private List<Enemy> registeredEnemies = new();
    private float timer;

    private void Awake()
    {
        instance = this;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer < 0.05f) return;
        timer = 0f;

        foreach (var e in registeredEnemies)
            e.OnUpdate(); 
    }

    public static void Register(Enemy e) => instance.registeredEnemies.Add(e);
    public static void Deregister(Enemy e) => instance.registeredEnemies.Remove(e);
}
