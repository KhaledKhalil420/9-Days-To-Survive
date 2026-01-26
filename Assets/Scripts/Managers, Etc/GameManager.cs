using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    
    [Header("Player")]
    private GameObject player;
    public List<Item> starterItems;

    [Header("Waves Rounds")]
    [SerializeField] private List<Wave> waves;
    private Wave selectedWave;
    private bool waveTriggered;
    public int enemiesDefeated;

    [Header("Points")]
    public static int StoredPoints = 0;
    internal int MaxBuilds => BuildingManager.Instance.buildLimit;
    internal int buildsBeforeNightStarted = 0;
    
    #region Unity

    private void Awake()
    {
        instance = this;
        player = GameObject.FindWithTag("Player");
    }

    private void Start()
    {
        DayNightCycleManager.OnDayChange += TriggerWave;
        GivePlayerStarterItems();
    }

    private void Update()
    {
        SpawnEnemies();
    }

    #endregion

    #region Waves

    public void TriggerWave(bool isDay)
    {
        if(isDay) 
            return;
        
        selectedWave = waves[DayNightCycleManager.DayCount];
        timer = selectedWave.spawningCooldown;
        waveTriggered = true;
    }

    private float timer = 0;
    public void SpawnEnemies()
    {
        if(waveTriggered)
        {
            timer += Time.deltaTime;
            if(AIManager.Instance.registeredEnemies.Count < selectedWave.maxEnemies && timer > selectedWave.spawningCooldown)
            {
                EnemySpawner.SpawnWave(selectedWave, player.transform.position, selectedWave.spawningRadius, selectedWave.minimumSpawningDistance);
                timer = 0;
            }

            if(enemiesDefeated >= selectedWave.requiredDefeats)
            {
                DayNightCycleManager.SetTime(DayNightCycleManager.CycleState.Day);
                waveTriggered = false;
                GivePoints();

            }
        }

        else
        {
            timer = 0;
            enemiesDefeated = 0;
        }
    }

    #endregion

    #region Player
    
    private void GivePlayerStarterItems()
    {
        foreach(Item item in starterItems)
            player.GetComponent<PlayerInventory>().GiveItem(item);
    }

    #endregion

    #region Points
    
    private void GivePoints()
    {
        StoredPoints += CalculatePoints();
    }

    private int CalculatePoints()
    {
        //Enemy points calculation
        int enemyPoints = 0;
        foreach (Enemy enemy in selectedWave.enemies)
            enemyPoints += enemy.EnemyPoints;

        //Get unsed buildings bonus (the less builds you use, the more points you get)
        int unusedBuilds = MaxBuilds - buildsBeforeNightStarted;
        int buildBonus = unusedBuilds * 5;
        
        //More points for difficulty
        float difficultyMultiplier = 1 + (Difficulty.DifficultyMultiplier - 1) * 0.2f; 
        
        //Total Points calculation
        int totalPoints = Mathf.RoundToInt((enemyPoints + buildBonus) * difficultyMultiplier);

        return totalPoints;
    }


    #endregion
}

[System.Serializable]
public class Wave
{
    public List<Enemy> enemies = new();

    [Header("Enemies")]
    public int requiredDefeats;
    public int maxEnemies = 100;

    [Header("Spawning")]
    public float spawningCooldown = 0;
    public float spawningRadius;
    public float minimumSpawningDistance;
}