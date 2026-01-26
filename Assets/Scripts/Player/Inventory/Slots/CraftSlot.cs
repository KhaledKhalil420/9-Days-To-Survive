using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CraftSlot : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private CraftingRecipe recipe;
    [SerializeField] private IngredientUi ingredientUi;
    [SerializeField] private Transform parent;

    private void Start()
    {
        GetComponentInChildren<Image>().sprite = recipe.itemToGive.data.sprite;
        foreach (Ingredient ingredient in recipe.ingredients)
        {
            ingredientUi.image.sprite = ingredient.item.data.sprite;
            ingredientUi.text.text = "x" + ingredient.quantity.ToString();

            Instantiate(ingredientUi, parent).gameObject.SetActive(true);
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
        
        AudioManager.Instance.PlaySound("Pickup", 0.95f, 1.1f);
        transform.parent.gameObject.SetActive(false);
    }
}
