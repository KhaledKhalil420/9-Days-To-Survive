using System.Collections.Generic;
using DG.Tweening;
using TMPro;
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
    [SerializeField] private CanvasGroup group;

    [Header("Waves Rounds")]
    [SerializeField] private List<Wave> waves;
    [SerializeField] private Animator animator;
    [SerializeField] private TMP_Text text;
    [SerializeField] private CanvasGroup groupStats;
    private Wave selectedWave;
    private bool waveTriggered;
    public int enemiesDefeated;

    [Header("Points")]
    public static float StoredPoints = 0; //Display points in UI somehow ok?
    internal float MaxBuilds => BuildingManager.Instance.buildLimitPoints;
    internal int buildsBeforeNightStarted = 0;
    public GameObject upgradesPopup;
    
    #region Unity

    private void Awake()
    {
        Instance = this;
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        StoredPoints = 0;

    }

    private void OnDestroy()
    {
        Instance = null;
    }

    private void Start()
    {
        DayNightCycleManager.Instance.OnDayChange += TriggerWave;
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
            waveTriggered = false;
            AIManager.KillAll();   

            if(spawnPrize)
            {
                spawnPrize = false;
                groupStats.DOFade(0, 0.5f);
                Instantiate(upgradesPopup);
            }

            return;
        }

        else
        {
            animator.Play("WaveStats_OnStart");
            groupStats.DOFade(1, 0.5f);
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
        text.text = enemiesDefeated.ToString() + "/" + selectedWave.requiredDefeats;
    }

    public void OnEnemyDefeated()
    {
        AudioManager.Instance.PlaySound("EnemyKill", 0.9f, 1.1f);
        animator.SetTrigger("Trigger");
        enemiesDefeated++;
        text.text = enemiesDefeated.ToString() + "/" + selectedWave.requiredDefeats;
    }

    private float timer = 0;
    private void SpawnEnemies() //Failed to create agent because it's not close enough to the navemsh
    {
        if(waveTriggered)
        {
            timer += Time.deltaTime;
            if(timer > selectedWave.spawningCooldown && AIManager.Instance.registeredEnemies.Count < selectedWave.maxEnemies)
            {
                timer = 0;
                EnemySpawner.SpawnWave(selectedWave, player.transform.position, selectedWave.spawningRadius, selectedWave.minimumSpawningDistance);
            }

            if(enemiesDefeated >= selectedWave.requiredDefeats)
            {
                DayNightCycleManager.SetTime(DayNightCycleManager.CycleState.Day);
                waveTriggered = false;
                GivePoints();
                AIManager.KillAll();   

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
        // #if UNITY_EDITOR
        foreach(Item item in starterItems)
            player.GetComponent<PlayerInventory>().GiveItem(item);

        // #else 
        
        // foreach(Item item in starterItems)
        //     Destroy(item.gameObject);

        // #endif
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

        AudioManager.Instance.SlowDown();
        StartCoroutine(AudioManager.Instance.FadeOutLowpass());

        UiManager.ToggleUi(true);
        UiManager.CloseAll();
        
        DOVirtual.DelayedCall(5f, () => {AudioManager.Instance.FadeOut(false); group.DOFade(1f, 2.5f).SetUpdate(true).OnComplete(() => { SceneManager.LoadScene(0); Time.timeScale = 1f; Time.fixedDeltaTime = 0.01f; }); }).SetUpdate(true);
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