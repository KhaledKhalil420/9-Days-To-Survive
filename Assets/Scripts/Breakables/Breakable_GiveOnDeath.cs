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
    public int minQuantity = 1, maxQuantity = 3;
}

public class Breakable_GiveOnDeath : Breakable
{
    [SerializeField] private int givenQuantityAverage;
    [SerializeField] private ParticleSystem destroyParticles;
    [SerializeField] private AudioClip destroySound;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private List<DamageMeshes> damageMeshes;
    [SerializeField] private List<ItemLoot> items;
    [SerializeField] private bool updateColliderOnMesh = false;

    public override void OnDamage(float damage, GameObject sender)
    {
        if (damageMeshes.Count > 0)
            UpdateVisualMesh();

        CameraShaker.Instance?.ShakeOnce(3, 3, 0f, 1f);
        transform.DOShakeRotation(0.5f, 5, 2);
    }

    private void UpdateVisualMesh()
    {
        float healthPercentage = (health / (float)fullHealth) * 100f;

        DamageMeshes selected = null;
        int smallestAbove = int.MaxValue;

        foreach (var d in damageMeshes)
        {
            if (d.healthPercentage >= healthPercentage && d.healthPercentage < smallestAbove)
            {
                smallestAbove = d.healthPercentage;
                selected = d;
            }
        }

        if (selected == null) return;

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = selected.damageMesh;

        if (updateColliderOnMesh && TryGetComponent(out MeshCollider meshCollider))
        {
            meshCollider.sharedMesh = null;
            meshCollider.convex = true;
            meshCollider.sharedMesh = selected.damageMesh;
        }
    }

    public override void OnDestroyed(GameObject sender, int toughness)
    {
        PlayerInventory playerInventory = sender.GetComponent<PlayerInventory>();

        if (item != null)
        {
            Item givenItem = Instantiate(item.gameObject).GetComponent<Item>();
            givenItem.HeldQuantity = UnityEngine.Random.Range(givenQuantityAverage / 2, givenQuantityAverage);

            playerInventory.GiveItem(givenItem, out bool wasGiven);
            if (!wasGiven) givenItem.transform.position = transform.position;
        }

        GiveItems(playerInventory, toughness);

        audioSource.PlayOneShot(destroySound);

        DisableBreakable();

        Bounds bounds = GetComponent<Renderer>().bounds;
        ParticleSpawner.SpawnWithBounds(destroyParticles, bounds.center, transform.rotation, bounds);

        DOVirtual.DelayedCall(destroySound.length, () => Destroy(gameObject));
    }

    private void GiveItems(PlayerInventory playerInventory, int _toughness)
    {
        float bonus = 0;
        int quantity = 0;

        foreach (var item in items)
        {
            quantity = UnityEngine.Random.Range(item.minQuantity, item.maxQuantity);
            bonus = (_toughness - toughness) * quantity / 2;
            Item givenItem = Instantiate(item.item.gameObject).GetComponent<Item>();
            givenItem.HeldQuantity = quantity + Mathf.RoundToInt(bonus);

            playerInventory.GiveItem(givenItem, out bool wasGiven);
            if (!wasGiven) givenItem.transform.position = transform.position;
        }
    }
}