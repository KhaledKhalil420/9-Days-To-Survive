using UnityEngine;

public class PointsManager : MonoBehaviour
{
    public static PointsManager Instance;

    [Header("Points")]
    public float StoredPoints = 0; //Display points in UI somehow ok?
    internal float MaxBuilds => BuildingManager.Instance.buildLimitPoints;
    internal int buildsBeforeNightStarted = 0;

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

    #endregion

    #region Points

    public void GivePoints(Wave selectedWave)
    {
        StoredPoints += CalculatePoints(selectedWave);
    }

    private float CalculatePoints(Wave selectedWave)
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