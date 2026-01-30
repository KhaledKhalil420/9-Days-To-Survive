using TMPro;
using UnityEngine;

public class DayCounterText : MonoBehaviour
{
    [SerializeField] private TMP_Text text;

    private void Start()
    {
        DayNightCycleManager.OnDayChange += UpdateText;
    }

    public void UpdateText(bool isDay)
    {
        text.text = "Day: " + (DayNightCycleManager.DayCount + 1).ToString();
    }
}
