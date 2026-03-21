using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngredientUi : MonoBehaviour
{
    public Image image;
    public TMP_Text text;
    public Ingredient ingredient;
    private PlayerInventory inventory;

    private void Start()
    {
        inventory = Player.inventory;
        InvokeRepeating(nameof(UpdateRecipe), 0.01f, 0.01f);
    }

    private void UpdateRecipe()
    {
        image.sprite = ingredient.item.data.sprite;
        
        if(inventory.HasItem(ingredient.item, ingredient.quantity))
        {
            text.color = Color.white;
        }
        else
        {
            inventory.FindSlotWithItem(ingredient.item, out SlotHolder slotHolder);
            text.color = Color.red;

            if(slotHolder != null)
            {
                text.text = slotHolder.HeldQuantity + " / " + ingredient.quantity;
            }

            else
            {
                text.text = ingredient.quantity.ToString();
            }
        }
    }

    void OnDestroy()
    {
        CancelInvoke();
    }
}