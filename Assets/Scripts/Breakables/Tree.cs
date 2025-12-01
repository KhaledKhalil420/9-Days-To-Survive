using EZCameraShake;
using UnityEngine;

public class Tree : Breakable
{
    [SerializeField] private int givenQuantityOnHitMin, givenQuantityOnHitMax;

    public override void OnDamage(GameObject sender)
    {
        //Give player material
        PlayerInventory playerInventory = sender.GetComponent<PlayerInventory>();

        Item givenItem = Instantiate(item.gameObject).GetComponent<Item>();
        givenItem.HeldQuantity = Random.Range(givenQuantityOnHitMin, givenQuantityOnHitMax);

        playerInventory.GiveItem(givenItem, out bool wasGiven);

        if(!wasGiven) 
            Destroy(gameObject);

        CameraShaker.Instance?.ShakeOnce(6, 3, 0f, 1f);
    }

    public override void OnDestroyed()
    {
        Destroy(gameObject);
    }
}
