using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [Header("Player")]
    public static Player Player => Instance?.player;
    private Player player;
    public List<Item> starterItems;

    [Header("Death")]
    [SerializeField] private Volume volume;

    [Header("Waves Rounds")]
    [SerializeField] private List<Wave> waves;
    private Wave selectedWave;
    private bool waveTriggered;
    public int enemiesDefeated;

    [Header("Points")]
    public static float StoredPoints = 0;
    internal float MaxBuilds => BuildingManager.Instance.buildLimitPoints;
    internal int buildsBeforeNightStarted = 0;
    public GameObject upgradesPopup;
    
    #region Unity

    private void Awake()
    {
        Instance = this;
        player = GameObject.FindWithTag("Player").GetComponent<Player>();

    }

    private void OnDestroy()
    {
        Instance = null;
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
        
        if(waves.Count > DayNightCycleManager.Instance.DayCount)
        {
            selectedWave = waves[DayNightCycleManager.Instance.DayCount];
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
        DOTween.KillAll(false);
        
        DOVirtual.Float(volume.weight, 1, 1f,value => volume.weight = value);
        Sequence eseq = DOTween.Sequence();
        Transform cam = GameObject.FindWithTag("MainCamera").transform;
        AudioManager.Instance.PlaySound("PlayerDeath");
        eseq.Append(cam.transform.DOLocalMove(cam.localPosition + Vector3.back * 1.5f - new Vector3(0, 1, 0), 0.5f))
            .Join(cam.transform.DOLocalRotate(new Vector3(-65, 0, 0), 2f));
            
        DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 0, 2f)
            .SetUpdate(true);

        DOTween.To(() => Time.fixedDeltaTime, x => Time.fixedDeltaTime = x, 0, 2f)
            .SetUpdate(true);

        player.Disable();
        UiManager.ToggleUi(true);

        StartCoroutine(AudioManager.Instance.FadeOutLowpass());
    }

    #endregion

    #region Points
    
    private void GivePoints()
    {
        StoredPoints += CalculatePoints();
    }

    private float CalculatePoints()
    {
        //Enemy points calculation
        int enemyPoints = 0;
        foreach (GroundEnemy enemy in selectedWave.enemies)
            enemyPoints += enemy.EnemyPoints;

        //Get unsed buildings bonus (the less builds you use, the more points you get)
        float unusedBuilds = MaxBuilds - buildsBeforeNightStarted;
        float buildBonus = unusedBuilds * 5;
        
        //More points for difficulty
        float difficultyMultiplier = 1 + (Difficulty.DifficultyMultiplier - 1) * 0.2f; 
        
        //Total Points calculation
        float totalPoints = Mathf.RoundToInt((enemyPoints + buildBonus) * difficultyMultiplier);

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