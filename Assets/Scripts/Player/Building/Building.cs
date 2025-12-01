using System;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour, IBreakable
{
    public int currentHealth = 5;
    public Ingredient[] ingredient;
    public BuildingData data;
    public List<Transform> pivots;

    public void Damage(GameObject sender, int damage, BreakableType type, int toughness)
    {
        if(type != BreakableType.Buildings) 
        return;

        currentHealth -= damage;

        if(currentHealth <= 0) Destroy(gameObject);
    }
}
