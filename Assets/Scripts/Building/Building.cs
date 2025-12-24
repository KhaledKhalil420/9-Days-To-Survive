using System;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour, IBreakable, IHoldInteractable
{
    public int currentHealth = 5;
    public Ingredient[] ingredients;
    public BuildingData data;
    public bool usesPivots = true;
    public bool affectedByGridSizePosition = true;
    public List<Transform> pivots;
    public List<Transform> pivotsOnBuild;

    public float HoldDuration => interactFor;

    float IHoldInteractable.holdProgress { get; set;}

    [SerializeField] private float interactFor = 1;

    public void OnPlace()
    {
        Array.ForEach(pivotsOnBuild.ToArray(), pivot => pivots.Add(pivot));
    }

    public void Damage(GameObject sender, int damage, BreakableType type, int toughness)
    {
        if(type != BreakableType.Buildings) 
        return;

        currentHealth -= damage;

        if(currentHealth <= 0) Destroy(gameObject);
    }

    public void OnHoldProgress(float progress)
    {
        Debug.Log(progress);
    }

    public void OnHoldComplete()
    {
        Debug.Log("complete");
    }
}