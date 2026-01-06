using DG.Tweening;
using EZCameraShake;
using UnityEngine;

public class Tree : Breakable
{
    [SerializeField] private int givenQuantityOnHitMax;
    [SerializeField] private ParticleSystem destroyParticles;
    [SerializeField] private AudioClip destroySound;

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

    public override void OnDestroyed(GameObject sender)
    {
        //Playone shot, doesn't seem to be working for some reason btw
        source.audioSource.clip = destroySound;
        source.audioSource.Play();
        
        DisableBreakable();
        Instantiate(destroyParticles, GetComponent<Renderer>().bounds.center / 1.25f, transform.rotation);

        DOVirtual.DelayedCall(destroySound.length, () => Destroy(gameObject));
    }
}
