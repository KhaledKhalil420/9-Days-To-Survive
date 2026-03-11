using UnityEngine;

public class Difficulty : MonoBehaviour
{
    public static float DifficultyMultiplier = 1;

    private void Awake()
    {
        DifficultyMultiplier = 1;
    }

    private void Start()
    {
        DayNightCycleManager.Instance.OnDayChange += IncreaseDif;
    }

    public void IncreaseDif(bool isDay)
    {
        if(!isDay)
        {
            IncreaseDifficulty(0.25f);
        }
    }

    public static void IncreaseDifficulty(float Difficulty)
    {
        DifficultyMultiplier += Difficulty;
    }
}
    