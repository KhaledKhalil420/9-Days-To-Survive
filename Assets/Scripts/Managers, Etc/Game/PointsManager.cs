using System;
using UnityEngine;

public class PointsManager : MonoBehaviour
{
    public event Action onPointsChanged;
    public static PointsManager Instance;

    [Header("Points")]
    public float StoredPoints = 0; //Display points in UI somehow ok?
    [SerializeField] private float maxBuildBonus = 100f;
    internal float MaxBuilds => BuildingManager.Instance.buildLimitPoints;
    internal float buildsBeforeNightStarted = 0;

    #region Unity

    private void Awake()
    {
        Instance = this;
        StoredPoints = 0;
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    void OnValidate()
    {
        Instance = this;
    }

    #endregion

    #region Points

    public void TakePoints(float points)
    {
        StoredPoints -= points;
        StoredPoints = Mathf.Clamp(StoredPoints, 0, Mathf.Infinity);

        onPointsChanged?.Invoke();
    }

    public void GivePoints(Wave selectedWave)
    {
        StoredPoints += CalculatePoints(selectedWave);
        onPointsChanged?.Invoke();
    }

    public void GivePoints(int points)
    {
        StoredPoints += points;
        onPointsChanged?.Invoke();
    }

    private int CalculatePoints(Wave selectedWave)
    {
        //Unused build ratio (0 → 1)
        float unusedBuilds = Mathf.Max(0, MaxBuilds - buildsBeforeNightStarted);
        float buildRatio = MaxBuilds > 0 ? unusedBuilds / MaxBuilds : 0f;

        //Proportional capped bonus
        float buildBonus = buildRatio * maxBuildBonus;

        //Difficulty multiplier
        float difficultyMultiplier = 1f + (Difficulty.DifficultyMultiplier - 1f) * 0.2f;

        //Total reward for this night
        int totalPoints = Mathf.RoundToInt(buildBonus * difficultyMultiplier);

        return totalPoints;
    }

    #endregion
}