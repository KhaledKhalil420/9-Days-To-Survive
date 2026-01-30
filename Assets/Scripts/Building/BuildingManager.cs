using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingManager : MonoBehaviour
{
    public static BuildingManager Instance;
    public event Action OnGridUpdated;

    [Header("Attributes")]
    public int buildLimit = 10;
    private int currentBuilds = 0;
    public float extraBuildingHealth = 0;
    public float extraBuildingDamage = 0;

    [Header("Build Settings")]
    public bool IsDay = true;
    public static bool CanBuild() { return Instance.IsDay && Instance.currentBuilds < Instance.buildLimit;}

    
    public int gridSize = 2;
    public float maxBuildDistance = 12f;
    public float snapDistance = 5f;
    public float sphereCastRadius = 1.5f;
    public float rotationAngle = 45f;

    [Header("Layers")]
    public LayerMask PhysicsLayers;
    public LayerMask buildableLayers;
    public LayerMask demolishLayers;

    [Header("UI")]
    [SerializeField] private TMP_Text buildsQuantityText;
    [SerializeField] private Transform canvasParent;
    [SerializeField] private Image buildIcon;
    [SerializeField] private TMP_Text buildTitle;
    [SerializeField] private Transform recipeParent;
    [SerializeField] private Image recipePrefab;

    List<GameObject> recipeInstances = new();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        DayNightCycleManager.OnDayChange += UpdateBuildingStatus;
    }

    private void UpdateBuildingStatus(bool value)
    {
        IsDay = value;

        if(canvasParent.gameObject.activeInHierarchy)
        ShowUI(!(value == false));
    }

    public void ShowUI(bool state)
    {
        canvasParent.gameObject.SetActive(state);
    }

    public void UpdateBuildUI(Building building, bool updateNumber)
    {
        buildIcon.sprite = building.data.sprite;
        buildTitle.text = building.data.buildingName;
        buildsQuantityText.text =  currentBuilds.ToString() + "/" + buildLimit.ToString();

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

    public void UpdateGrid()
    {
        OnGridUpdated?.Invoke();
        currentBuilds = GameObject.FindGameObjectsWithTag("Build").Length;
    }

    void ClearRecipe()
    {
        recipeInstances.ForEach(Destroy);
        recipeInstances.Clear();
    }
}
