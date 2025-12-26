using DG.Tweening;
using EZCameraShake;
using UnityEngine;

public class Tree : Breakable
{
    [SerializeField] private int givenQuantityOnHitMax;

    public override void OnDamage(int damage, GameObject sender)
    {
        //Give player material
        PlayerInventory playerInventory = sender.GetComponent<PlayerInventory>();

        Item givenItem = Instantiate(item.gameObject).GetComponent<Item>();
        givenItem.HeldQuantity = Random.Range(damage, damage * givenQuantityOnHitMax);

        playerInventory.GiveItem(givenItem, out bool wasGiven);

        if(!wasGiven) 
            Destroy(gameObject);

        CameraShaker.Instance?.ShakeOnce(3, 3, 0f, 1f);
        transform.DOShakeRotation(0.5f, 5, 2);
    }

    public override void OnDestroyed()
    {
        Destroy(gameObject);
    }
}
