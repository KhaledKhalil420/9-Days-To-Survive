using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CraftSlot : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private CraftingRecipe recipe;
    [SerializeField] private IngredientUi ingredientUi;
    [SerializeField] private Transform parent;
    [SerializeField] private Image image;

    private void Start()
    {
        image.sprite = recipe.itemToGive.data.sprite;
        foreach (Ingredient ingredient in recipe.ingredients)
        {
            IngredientUi ingredientUiSpawned = Instantiate(ingredientUi, parent);
            ingredientUiSpawned.ingredient = ingredient;
            ingredientUiSpawned.gameObject.SetActive(true);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            CraftRecipe();
        }
    }

    public void CraftRecipe()
    {
        PlayerInventory playerInventory = PlayerInventory.Instance;

        foreach (Ingredient ingredient in recipe.ingredients)
        {
            if(!playerInventory.HasItem(ingredient.item, ingredient.quantity))
            {
                return;
            }
        }
        
        Item item = Instantiate(recipe.itemToGive, Vector3.zero, Quaternion.identity);
        item.HeldQuantity = recipe.givenQuantity;
        
        playerInventory.GiveItem(item, out bool wasGiven);
        
        if(!wasGiven)
        {
            Destroy(item);
            return;
        }

        foreach (Ingredient ingredient in recipe.ingredients)
        {
            playerInventory.TakeItem(ingredient.item, ingredient.quantity, out bool _);
        }   
        
        AudioManager.Instance.PlaySound("Craft", 0.95f, 1.1f);
        transform.parent.gameObject.SetActive(false);
    }
}
