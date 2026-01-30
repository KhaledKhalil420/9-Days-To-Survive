using System;
using DG.Tweening;
using UnityEngine;
using EZCameraShake;
using System.Collections.Generic;

[Serializable]
public class DamageMeshes
{
    [SerializeField, Range(0, 100)] internal int healthPercentage;
    [SerializeField] internal Mesh damageMesh;
}

[Serializable]
public class ItemLoot
{
    public Item item;
    public int minQuantity = 1, maxQauntity = 3;
}

public class Breakable_GiveOnDeath : Breakable
{
    [SerializeField] private int givenQuantityAverage;
    [SerializeField] private ParticleSystem destroyParticles;
    [SerializeField] private AudioClip destroySound;
    [SerializeField] private List<DamageMeshes> damageMeshes;
    [SerializeField] private List<ItemLoot> items;

    public override void OnDamage(int damage, GameObject sender)
    {
        if(damageMeshes.Count > 0)
            UpdateVisualMesh();

        CameraShaker.Instance?.ShakeOnce(3, 3, 0f, 1f);
        transform.DOShakeRotation(0.5f, 5, 2);
    }

    void UpdateVisualMesh()
    {
        float healthPercentage = (health / (float)fullHealth) * 100f;
    
        DamageMeshes selected = null;
        int smallestAbove = int.MaxValue;
    
        foreach (var d in damageMeshes)
        {
            if (d.healthPercentage >= healthPercentage &&
                d.healthPercentage < smallestAbove)
            {
                smallestAbove = d.healthPercentage;
                selected = d;
            }
        }
    
        if (selected != null)
            GetComponent<MeshFilter>().mesh = selected.damageMesh;
    }

    public override void OnDestroyed(GameObject sender)
    {
        //Give player material
        PlayerInventory playerInventory = sender.GetComponent<PlayerInventory>();

        Item givenItem = Instantiate(item.gameObject).GetComponent<Item>();
        givenItem.HeldQuantity = UnityEngine.Random.Range(givenQuantityAverage / 2, givenQuantityAverage);

        playerInventory.GiveItem(givenItem, out bool wasGiven);

        if(!wasGiven) 
            givenItem.transform.position = transform.position;

        GiveItems(playerInventory);

        //Playone shot, doesn't seem to be working for some reason btw
        source.audioSource.clip = destroySound;
        source.audioSource.Play();
        
        DisableBreakable();
        Instantiate(destroyParticles, transform.position, transform.rotation);

        DOVirtual.DelayedCall(destroySound.length, () => Destroy(gameObject));
    }

    private void GiveItems(PlayerInventory playerInventory)
    {
        foreach (var item in items)
        {
            Item givenItem = Instantiate(item.item.gameObject).GetComponent<Item>();
            givenItem.HeldQuantity = UnityEngine.Random.Range(item.minQuantity, item.maxQauntity);
    
            playerInventory.GiveItem(givenItem, out bool wasGiven);
    
            if(!wasGiven) 
                givenItem.transform.position = transform.position;
        }
    }
}
