using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingStation : MonoBehaviour
{
    private int currentRecipe = 0;
    [SerializeField] private List<CraftingRecipe> availableRecipes;
    private List<GameObject> tempUI = new();

    [SerializeField] private Transform uiHolder;  
    [SerializeField] private Image recipeIcon_Image;
    [SerializeField] private TMP_Text recipeDiscreption_Text;
    [SerializeField] private Image ingredientPrefab;

    private void Start()
    {
        InitializeUI();
    }

    #region Ui

    public void DisplayForwardRecipe()
    {
        ClearUi();

        currentRecipe++;
        if(currentRecipe >= availableRecipes.Count)
        {
            currentRecipe = 0;
        }

        InitializeUI();
    }

    public void DisplayBackwardRecipe()
    {
        ClearUi();

        currentRecipe--;
        if(currentRecipe < 0)
        {   
            currentRecipe = availableRecipes.Count - 1;
        }

        InitializeUI();
    }

    private void ClearUi()
    {
        if(tempUI.Count > 0)
            Array.ForEach(tempUI.ToArray(), uiElement => Destroy(uiElement.gameObject));
    }

    private void InitializeUI()
    {
        if(availableRecipes.Count == 0) 
            return;

        //Update recipe
        recipeIcon_Image.sprite = availableRecipes[currentRecipe].itemToGive.data.sprite;
        recipeDiscreption_Text.text = availableRecipes[currentRecipe].itemToGive.data.discription;

        //Add ingrediets
        foreach(Ingredient ingredient in availableRecipes[currentRecipe].ingredients)
        {
            
            IngredientUi ingredientUi = Instantiate(ingredientPrefab, uiHolder).GetComponent<IngredientUi>();
            ingredientUi.image.sprite = ingredient.item.data.sprite;
            ingredientUi.text.text = ingredient.item.data.discription;

            tempUI.Add(ingredientUi.gameObject);
        }
    }

    #endregion

    #region Crafting

    public void CraftRecipe(GameObject sender)
    {
        PlayerInventory playerInventory = sender.GetComponent<PlayerInventory>();

        foreach (Ingredient ingredient in availableRecipes[currentRecipe].ingredients)
        {
            if(!playerInventory.HasItem(ingredient.item, ingredient.quantity))
            {
                return;
            }
        }
        
        Item item = Instantiate(availableRecipes[currentRecipe].itemToGive);
        item.HeldQuantity = availableRecipes[currentRecipe].givenQuantity;
        
        playerInventory.GiveItem(item, out bool wasGiven);

        if(wasGiven)
        {
            foreach (Ingredient ingredient in availableRecipes[currentRecipe].ingredients)
            {
                playerInventory.TakeItem(ingredient.item, ingredient.quantity, out bool _);
            }   
        }

        else
        {
            Destroy(item);
        }
    }

    #endregion
}
