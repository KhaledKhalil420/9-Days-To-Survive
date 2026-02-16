using UnityEngine;

public class WASDQuest : Quest
{
    private bool pressedW, pressedA, pressedS, pressedD;

    public override void OnSpawned()
    {
        pressedW = pressedA = pressedS = pressedD = false;
    }

    private void Update()
    {
        if (isCompleted) return;

        if (Input.GetKeyDown(KeyCode.W)) pressedW = true;
        if (Input.GetKeyDown(KeyCode.A)) pressedA = true;
        if (Input.GetKeyDown(KeyCode.S)) pressedS = true;
        if (Input.GetKeyDown(KeyCode.D)) pressedD = true;

        UpdateUi(BuildWASDString());

        if (pressedW && pressedA && pressedS && pressedD)
            CompleteQuest();
    }

    //Ui
    private string BuildWASDString()
    {
        return $"{Key("W", pressedW)}  {Key("A", pressedA)}  {Key("S", pressedS)}  {Key("D", pressedD)}";
    }

    private string Key(string letter, bool pressed)
    {
        string color = pressed ? "#00FF00" : "#555555";
        return $"<color={color}>{letter}</color>";
    }
}