using UnityEngine;

public class OpenInventoryQuest : Quest
{
    public override void OnSpawned()
    {
        
    }

    private void Update()
    {
        if(Input.GetKeyDown(Keybinds.Key("InventoryOpen")))
        {
            CompleteQuest();
        }
    }
}