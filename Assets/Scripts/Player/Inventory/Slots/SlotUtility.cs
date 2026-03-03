using UnityEngine;

public class SlotUtility
{
    public static bool TryMove(BaseSlot from, BaseSlot to, int qty)
    {
        if (from == null || to == null || from.HeldItem == null || from == to) return false;

        qty = Mathf.Clamp(qty, 1, from.HeldQuantity);

        //If target has an item
        if (to.HeldItem != null)
        {
            //Same type -> merge quantities
            if (to.HeldItem.data == from.HeldItem.data && !from.HeldItem.isSingleQuantityItem)
            {
                to.HeldQuantity += qty;
                from.HeldQuantity -= qty;

                //If source emptied, destroy its GameObject and clear reference
                if (from.HeldQuantity <= 0)
                {
                    from.HeldItem.OnChangingItems();
                    Object.Destroy(from.HeldItem.gameObject);
                    from.ResetSlot();
                }

                from.UpdateSlot();
                to.UpdateSlot();
                return true;
            }
            else
            {
                //Different items, swap
                return TrySwap(from, to);
            }
        }

        // If target is empty -> either move the whole stack (move GameObject) or split (create new)
        if (to.HeldItem == null)
        {
            // Move full stack: transfer the existing Item GameObject (no instantiate)
            if (qty >= from.HeldQuantity)
            {
                to.HeldItem = from.HeldItem;
                to.HeldQuantity = from.HeldQuantity;

                // Reparent the item GameObject to the target holder's hand
                if (to.heldBy != null && to.heldBy.hand != null && to.HeldItem != null)
                {
                    to.HeldItem.heldby = to.heldBy.parent.gameObject;
                    to.HeldItem.SetItemParent(to.heldBy.hand);
                }

                from.HeldItem = null;
                from.HeldQuantity = 0;

                from.UpdateSlot();
                to.UpdateSlot();
                return true;
            }
            else
            {
                // Split stack: create a new item instance for the target
                to.HeldQuantity = qty;
                to.CreateItem(from.HeldItem.data);

                from.HeldQuantity -= qty;

                from.UpdateSlot();
                to.UpdateSlot();
                return true;
            }
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