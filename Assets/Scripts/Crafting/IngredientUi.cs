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
    public string additionalText, beforeText;

    private void Start()
    {
        inventory = Player.inventory;
        InvokeRepeating(nameof(UpdateRecipe), 0.01f, 0.01f);
    }

    private void UpdateRecipe()
    {
        image.sprite = ingredient.item.data.sprite;
        inventory.FindSlotWithItem(ingredient.item, out SlotHolder slotHolder);
        bool hasEnough = inventory.HasItem(ingredient.item, ingredient.quantity);
        int held = slotHolder != null ? slotHolder.HeldQuantity : 0;

        text.color = hasEnough ? Color.white : Color.red;
        text.text = beforeText + (hasEnough ? ingredient.quantity : held + "/" + ingredient.quantity) + " " + additionalText;
    }

    void OnDestroy()
    {
        CancelInvoke();
    }
}