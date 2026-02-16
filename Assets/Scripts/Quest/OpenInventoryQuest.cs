using UnityEngine;

public class OpenInventoryQuest : Quest
{
    public override void OnSpawned()
    {
        
    }

    private void Update()
    {
        if(Input.GetKeyDown(Keybinds.Key("InventoryOpen")) && !isCompleted)
        {
            CompleteQuest();
            AudioManager.Instance.PlaySound("Quest_Click");
        }
    }
}