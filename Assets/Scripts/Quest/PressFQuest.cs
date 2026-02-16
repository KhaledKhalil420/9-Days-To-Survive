using UnityEngine;

public class PressFQuest : Quest
{
    [SerializeField] private Item hammer;

    public override void OnSpawned()
    {
        
    }

    private void Update()
    {
        if(Input.GetKeyDown(Keybinds.Key("SelectBuild")) && Player.inventory.GetSelectedSlot().HeldItem.data == hammer.data)
        {
            CompleteQuest();
        }
    }
}