using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using EZCameraShake;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance;

    [Header("Feel")]
    [SerializeField] private Volume volume;

    [Header("Waves Rounds")]
    [SerializeField] private List<Wave> waves;
    [SerializeField] private Animator animator;
    [SerializeField] private Slider timerSlider;
    [SerializeField] private CanvasGroup groupStats;
    private Wave selectedWave;
    private bool waveTriggered;
    public int enemiesDefeated;

    private float elapsed;
    private float currentSpeedBoost;
    private int lastMilestoneTick;

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
        elapsed = 0f;
        currentSpeedBoost = 0f;
        lastMilestoneTick = 0;

        selectedWave = waves.Count > DayNightCycleManager.Instance.DayCount
            ? waves[DayNightCycleManager.Instance.DayCount]
            : waves[^1];

        foreach (WaveEnemy waveEnemy in selectedWave.enemies)
            waveEnemy.timer = waveEnemy.spawningCooldown;

        timerSlider.value = 0f;
    }

    public void OnEnemyDefeated(string enemyName = "")
    {
        AudioManager.Instance.PlaySound("EnemyKill", 0.9f, 1.1f);
        animator.SetTrigger("Trigger");
        enemiesDefeated++;

        WaveEnemy match = selectedWave.enemies.FirstOrDefault(e => enemyName.Contains(e.prefab.name));
        float threatRatio = match != null ? match.threatLevel / (float)selectedWave.waveThreatLevel : 1f;
        currentSpeedBoost = Mathf.Min(currentSpeedBoost + selectedWave.speedBoostPerKill * threatRatio, selectedWave.maxSpeedBoost);

        CheckSpeedMilestone();
    }

    private void CheckSpeedMilestone()
    {
        // Fires every 25% of the max speed boost reached
        int tick = Mathf.FloorToInt((currentSpeedBoost / selectedWave.maxSpeedBoost) / 0.25f);
        if (tick <= lastMilestoneTick) return;

        lastMilestoneTick = tick;
        AudioManager.Instance.PlaySound("Wave_SpeedUp");
        animator.SetTrigger("SpeedUp");
    }

    private void SpawnEnemies()
    {
        if (!waveTriggered)
        {
            enemiesDefeated = 0;
            return;
        }

        elapsed += Time.deltaTime * (1f + currentSpeedBoost);
        timerSlider.value = elapsed / selectedWave.survivalDuration;

        foreach (WaveEnemy waveEnemy in selectedWave.enemies)
        {
            waveEnemy.timer += Time.deltaTime;

            if (waveEnemy.timer < waveEnemy.spawningCooldown) continue;

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

        if (elapsed >= selectedWave.survivalDuration)
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
    public int threatLevel = 1;

    [HideInInspector] public float timer;
}

[System.Serializable]
public class Wave
{
    public List<WaveEnemy> enemies = new();

    [Header("Survival Goal")]
    public float survivalDuration = 120f;
    public int waveThreatLevel = 1;

    [Header("Speed Boost")]
    public float maxSpeedBoost = 0.1f;
    public float speedBoostPerKill = 0.005f;

    [Header("Shared Spawn Area")]
    public float spawningRadius;
    public float minimumSpawningDistance;
}