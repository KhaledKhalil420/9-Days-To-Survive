using UnityEngine;

public class CraftFlimtHammerQuest : Quest
{
    [SerializeField] internal Item item;

    public override void OnSpawned()
    {
        InvokeRepeating(nameof(CheckHasItem), 0.001f, 0.001f);
    }

    private void CheckHasItem()
    {   
        if(Player.inventory.HasItem(item, 1))
        {
            CompleteQuest();
        }
    }
}