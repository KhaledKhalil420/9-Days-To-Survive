using UnityEngine;

public class Tree : Breakable
{
    [SerializeField] private int givenQuantityOnHit;

    public override void OnDamage(GameObject sender)
    {
        //Give player material
        PlayerInventory playerInventory = sender.GetComponent<PlayerInventory>();

        Item givenItem = Instantiate(item.gameObject).GetComponent<Item>();
        givenItem.HeldQuantity = givenQuantityOnHit;

        playerInventory.GiveItem(givenItem, out bool wasGiven);

        if(!wasGiven) 
            Destroy(gameObject);
    }

    public override void OnDestroyed()
    {
        Destroy(gameObject);
    }
}
