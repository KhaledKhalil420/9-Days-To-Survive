using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CraftingRecipe : ScriptableObject
{
    public Item itemToGive;
    public int givenQuantity = 1;
    public List<Ingredient> ingredients;
}

[System.Serializable]
public class Ingredient
{
    public Item item;
    public int quantity;
}

