using UnityEngine;

public class HoldGQuest : Quest
{
    private float Timer = 10;

    public override void OnSpawned()
    {
        
    }

    private void Update()
    {
        Timer -= Time.deltaTime;

        if(Timer <= 0) 
            CompleteQuest();

        UpdateUi("  " + (int)Timer);
    }
}
