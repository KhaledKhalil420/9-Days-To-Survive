using System.Collections.Generic;
using EZCameraShake;
using UnityEngine;

public class Stone : Breakable
{
    [SerializeField] private int givenQuantityOnHitMax;

    public override void OnDamage(float damage, GameObject sender)
    {
        //Give player material
        PlayerInventory playerInventory = sender.GetComponent<PlayerInventory>();

        Item givenItem = Instantiate(item.gameObject).GetComponent<Item>();
        givenItem.HeldQuantity = (int)Random.Range(damage, damage * givenQuantityOnHitMax);

        playerInventory.GiveItem(givenItem, out bool wasGiven);

        if(!wasGiven) 
            Destroy(gameObject);

        CameraShaker.Instance?.ShakeOnce(6, 3, 0f, 1f);
    }

    public override void OnDestroyed(GameObject sender, int toughness)
    {
        Destroy(gameObject);
    }
}
