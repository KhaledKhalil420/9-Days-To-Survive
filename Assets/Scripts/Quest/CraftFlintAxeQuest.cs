using UnityEngine;

public class CraftFlintAxeQuest : Quest
{
    [SerializeField] private Item item; //Expose this for an example
    private bool done = false;

    public override void OnSpawned()
    {
        InvokeRepeating(nameof(CheckHasItem), 0.001f, 0.001f);
    }

    private void CheckHasItem()
    {   
        if(Player.inventory.HasItem(item, 1) && !isCompleted)
        {
            done = true;
            CompleteQuest();
        }
    }
}