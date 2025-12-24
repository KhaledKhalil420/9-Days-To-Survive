using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance;

    [Header("Build Settings")]
    public int gridSize = 2;
    public float maxBuildDistance = 12f;
    public float snapDistance = 5f;
    public float sphereCastRadius = 1.5f;
    public float rotationAngle = 45f;

    [Header("Layers")]
    public LayerMask buildableLayers;
    public LayerMask demolishLayers;

    [Header("UI")]
    [SerializeField] Transform canvasParent;
    [SerializeField] Image buildIcon;
    [SerializeField] TMP_Text buildTitle;
    [SerializeField] Transform recipeParent;
    [SerializeField] Image recipePrefab;

    List<GameObject> recipeInstances = new();

    private void Update()
    {
        Instance = this;
    }

    public void ShowUI(bool state)
    {
        canvasParent.gameObject.SetActive(state);
    }

    public void UpdateBuildUI(Building building)
    {
        buildIcon.sprite = building.data.sprite;
        buildTitle.text = building.data.buildingName;

        ClearRecipe();

        foreach (var ingredient in building.ingredients)
        {
            Image img = Instantiate(recipePrefab, recipeParent);
            img.sprite = ingredient.item.data.sprite;
            img.GetComponentInChildren<TMP_Text>().text =
                ingredient.quantity.ToString();

            recipeInstances.Add(img.gameObject);
        }
    }

    void ClearRecipe()
    {
        recipeInstances.ForEach(Destroy);
        recipeInstances.Clear();
    }
}
