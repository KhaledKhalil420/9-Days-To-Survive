using UnityEngine;

public class OpenInventoryQuest : Quest
{
    private void Update()
    {
        if(Input.GetKeyDown(Keybinds.Key("InventoryOpen")) && !isCompleted)
        {
            CompleteQuest();
            AudioManager.Instance.PlaySound("Quest_Click");
        }
    }
}