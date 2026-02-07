using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [Header("Player")]
    public static Player Player => Instance?.player;
    private Player player;
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
    public GameObject upgradesPopup;
    
    #region Unity

    private void Awake()
    {
        Instance = this;
        player = GameObject.FindWithTag("Player").GetComponent<Player>();

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
    private bool spawnPrize = false;
    public void TriggerWave(bool isDay)
    {
        if(isDay)
        {
            if(spawnPrize)
            {
                spawnPrize = false;
                Instantiate(upgradesPopup);
            }

            AIManager.KillAll();   
            waveTriggered = false;
            return;
        }

        else
        {
            waveTriggered = true;
            spawnPrize = true;
        }
        
        if(waves.Count > DayNightCycleManager.DayCount)
        {
            selectedWave = waves[DayNightCycleManager.DayCount];
        }
        else
        {
            selectedWave = waves[waves.Count - 1];
        }

        timer = selectedWave.spawningCooldown;
    }

    private float timer = 0;
    public void SpawnEnemies()
    {
        if(waveTriggered)
        {
            timer += Time.deltaTime;
            if(AIManager.Instance?.registeredEnemies.Count < selectedWave.maxEnemies && timer > selectedWave.spawningCooldown)
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
        #if UNITY_EDITOR
        foreach(Item item in starterItems)
            player.GetComponent<PlayerInventory>().GiveItem(item);

        #else 
        
        foreach(Item item in starterItems)
            Destroy(item.gameObject);

        #endif
    }

    public void PlayerLost()
    {
        //TEMP.. RESTART GAME
        SceneManager.LoadScene("MainMenu");
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
        foreach (GroundEnemy enemy in selectedWave.enemies)
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
    public List<GroundEnemy> enemies = new();

    [Header("Enemies")]
    public int requiredDefeats;
    public int maxEnemies = 100;

    [Header("Spawning")]
    public float spawningCooldown = 0;
    public float spawningRadius;
    public float minimumSpawningDistance;
}