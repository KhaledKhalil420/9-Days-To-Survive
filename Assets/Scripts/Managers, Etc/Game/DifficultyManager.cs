using UnityEngine;

public class Difficulty : MonoBehaviour
{
    public static int DifficultyMultiplier = 1;

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
            IncreaseDifficulty(1);
        }
    }

    public static void IncreaseDifficulty(int Difficulty)
    {
        DifficultyMultiplier += Difficulty;
    }
}
    