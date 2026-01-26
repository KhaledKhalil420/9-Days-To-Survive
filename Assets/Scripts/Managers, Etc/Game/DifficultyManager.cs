using UnityEngine;

public class Difficulty : MonoBehaviour
{
    public static int DifficultyMultiplier = 1;

    public static void IncreaseDifficulty(int Difficulty)
    {
        DifficultyMultiplier += Difficulty;
    }
}
