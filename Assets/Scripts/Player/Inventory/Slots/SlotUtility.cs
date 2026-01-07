using UnityEngine;

public class SlotUtility : MonoBehaviour
{
    public static bool TryMove(BaseSlot from, BaseSlot to, int qty)
    {
        if (from.HeldItem == null || from == to) return false;
        
        //if to has an item, same type of item, not single quantity item, add to it
        if (to.HeldItem != null && !from.HeldItem.isSingleQuantityItem)
        {
            if(to.HeldItem.data == from.HeldItem.data)
            {
                //Do quantities
                to.HeldQuantity += qty;
                from.HeldQuantity -= qty;

                if(from.HeldQuantity == 0)
                {
                    to.CreateItem(from.HeldItem.data);
                }
                
                from.UpdateSlot();
                to.UpdateSlot();
                return true;   
            }

            else
            {
                TrySwap(from, to);
            }
        }
        
        //if to doesn't have an item.. leave it there
        if (to.HeldItem == null)
        {
            to.CreateItem(from.HeldItem.data);
            to.HeldQuantity = qty;

            from.HeldQuantity -= qty;

            from.UpdateSlot();
            to.UpdateSlot();
            return true;
        }

        return false;
    }

    public static bool TrySwap(BaseSlot a, BaseSlot b)
    {
        if (a == null || b == null) return false;
        if (a == b) return false;

        var item = a.HeldItem;
        var qty = a.HeldQuantity;

        a.HeldItem = b.HeldItem;
        a.HeldQuantity = b.HeldQuantity;

        b.HeldItem = item;
        b.HeldQuantity = qty;

        a.UpdateSlot();
        b.UpdateSlot();
        return true;
    }

}
