using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using EZCameraShake;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance;

    [Header("Feel")]
    [SerializeField] private Volume volume;

    [Header("Waves Rounds")]
    [SerializeField] private List<Wave> waves;
    [SerializeField] private Animator animator;
    [SerializeField] private TMP_Text text;
    [SerializeField] private CanvasGroup groupStats;
    private Wave selectedWave;
    private bool waveTriggered;
    public int enemiesDefeated;

    [Header("Points")]
    public GameObject upgradesPopup;

    #region Unity

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    private void Start()
    {
        DayNightCycleManager.Instance.OnDayChange += TriggerWave;

        foreach (Wave wave in waves)
            EnemyPool.Instance.Prewarm(wave.enemies.Select(e => e.prefab).ToList());
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
        if (isDay)
        {
            waveTriggered = false;
            AIManager.KillAll();

            if (spawnPrize)
            {
                spawnPrize = false;
                groupStats.DOFade(0, 0.5f);

                CameraShaker.Instance.ShakeOnce(2, 2, 0.25f, 1f);
                AudioManager.Instance.PlaySound("Wave_Survived");

                DOTween.Sequence()
                    .Append(DOTween.To(() => volume.weight, x => volume.weight = x, 1f, 0.25f).SetEase(Ease.OutCubic))
                    .AppendInterval(0.1f)
                    .Append(DOTween.To(() => volume.weight, x => volume.weight = x, 0f, 3.75f).SetEase(Ease.InOutSine))
                    .OnComplete(() => Instantiate(upgradesPopup));

                animator.SetTrigger("Survive");

                BuildingManager.Instance.buildLimitPoints *= 1.5f;
            }

            return;
        }

        animator.Play("WaveStats_OnStart");
        groupStats.DOFade(1, 0.5f);
        waveTriggered = true;
        spawnPrize = true;

        selectedWave = waves.Count > DayNightCycleManager.Instance.DayCount
            ? waves[DayNightCycleManager.Instance.DayCount]
            : waves[^1];

        // Reset every enemy's spawn timer when the wave begins
        foreach (WaveEnemy waveEnemy in selectedWave.enemies)
            waveEnemy.timer = waveEnemy.spawningCooldown; // Fire first spawn immediately

        text.text = $"0/{selectedWave.requiredDefeats}";
    }

    public void OnEnemyDefeated()
    {
        AudioManager.Instance.PlaySound("EnemyKill", 0.9f, 1.1f);
        animator.SetTrigger("Trigger");
        enemiesDefeated++;
        text.text = $"{enemiesDefeated}/{selectedWave.requiredDefeats}";
    }

    private void SpawnEnemies()
    {
        if (!waveTriggered)
        {
            enemiesDefeated = 0;
            return;
        }

        foreach (WaveEnemy waveEnemy in selectedWave.enemies)
        {
            waveEnemy.timer += Time.deltaTime;

            if (waveEnemy.timer < waveEnemy.spawningCooldown) continue;

            // Count how many of this specific enemy type are currently alive
            int activeCount = AIManager.Instance.registeredEnemies
                .Count(e => e != null && e.gameObject.name == waveEnemy.prefab.name + "(Clone)");

            if (activeCount >= waveEnemy.maxEnemies) continue;

            waveEnemy.timer = 0;
            EnemyPool.Instance.Spawn(
                waveEnemy.prefab,
                GameManager.Player.transform.position,
                selectedWave.spawningRadius,
                selectedWave.minimumSpawningDistance
            );
        }

        if (enemiesDefeated >= selectedWave.requiredDefeats)
        {
            DayNightCycleManager.SetTime(DayNightCycleManager.CycleState.Day);
            waveTriggered = false;
            PointsManager.Instance.GivePoints(selectedWave);
            AIManager.KillAll();
        }
    }

    #endregion
}

[System.Serializable]
public class WaveEnemy
{
    public EnemyBrain prefab;

    [Header("Per-Enemy Settings")]
    public int maxEnemies = 10;
    public float spawningCooldown = 2f;

    [HideInInspector] public float timer;
}

[System.Serializable]
public class Wave
{
    public List<WaveEnemy> enemies = new();

    [Header("Defeat Goal")]
    public int requiredDefeats;

    [Header("Shared Spawn Area")]
    public float spawningRadius;
    public float minimumSpawningDistance;
}