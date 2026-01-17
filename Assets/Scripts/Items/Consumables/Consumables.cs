using UnityEngine;

public class Consumables : Item
{
    public override void OnUse()
    {
        Consume();
        OnConsume();
    }

    private void Consume()
    {
        parentSlot.HeldQuantity -= 1; 
    }

    public virtual void OnConsume()
    {
        
    }
}
