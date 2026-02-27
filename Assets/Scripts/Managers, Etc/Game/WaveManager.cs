using System.Collections.Generic;
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
            EnemyPool.Instance.Prewarm(wave.enemies);
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

        if (waves.Count > DayNightCycleManager.Instance.DayCount)
            selectedWave = waves[DayNightCycleManager.Instance.DayCount];
        else
            selectedWave = waves[waves.Count - 1];

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
    private void SpawnEnemies()
    {
        if (waveTriggered)
        {
            timer += Time.deltaTime;
            if (timer > selectedWave.spawningCooldown && AIManager.Instance.registeredEnemies.Count < selectedWave.maxEnemies)
            {
                timer = 0;

                EnemyBrain prefab = selectedWave.enemies[Random.Range(0, selectedWave.enemies.Count)];
                EnemyPool.Instance.Spawn(prefab, GameManager.Player.transform.position, selectedWave.spawningRadius, selectedWave.minimumSpawningDistance);
            }

            if (enemiesDefeated >= selectedWave.requiredDefeats)
            {
                DayNightCycleManager.SetTime(DayNightCycleManager.CycleState.Day);
                waveTriggered = false;
                PointsManager.Instance.GivePoints(selectedWave);
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
}

[System.Serializable]
public class Wave
{
    public List<EnemyBrain> enemies = new();

    [Header("Enemies")]
    public int requiredDefeats;
    public int maxEnemies = 100;

    [Header("Spawning")]
    public float spawningCooldown = 0;
    public float spawningRadius;
    public float minimumSpawningDistance;
}