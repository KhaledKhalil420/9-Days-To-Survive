using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingManager : MonoBehaviour
{
    public static BuildingManager Instance;
    public event Action OnGridUpdated;

    [Header("Attributes")]
    public float buildLimitPoints = 100;
    private float currentBuilds = 0;
    public float extraBuildingHealth = 1;
    public float extraBuildingDamage = 1;

    [Header("Effects")]
    public ParticleSystem smoke;

    [Header("Build Settings")]
    internal bool IsDay = true;
    public static bool CanBuild(float points) => Instance.IsDay && Instance.currentBuilds + points <= Instance.buildLimitPoints;
    public static bool CanBuild() => Instance.IsDay && Instance.currentBuilds <= Instance.buildLimitPoints;

    public float maxBuildDistance = 12f;
    public float snapDistance = 5f;
    public float sphereCastRadius = 1.5f;
    public float rotationAngle = 45f;

    [Header("Layers")]
    public LayerMask PhysicsLayers;
    public LayerMask buildableLayers;
    public LayerMask demolishLayers;

    [Header("UI - Building")]
    [SerializeField] private TMP_Text buildsQuantityText;
    [SerializeField] private Transform canvasParent;
    [SerializeField] private Image buildIcon;
    [SerializeField] private Image selectionModeImage;
    [SerializeField] private TMP_Text buildPriceText;
    [SerializeField] private TMP_Text buildTitle;
    [SerializeField] private Transform recipeParent;
    [SerializeField] private Image recipePrefab;

    [Header("UI - Inspect")]
    [SerializeField] private Transform inspectCanvasParent;
    [SerializeField] private TMP_Text buildInspectTitle;
    [SerializeField] private Slider buildInspectHealthSlider;
    [SerializeField] private Image buildInspectbuildIcon;
    [SerializeField] private TMP_Text buildInspectDamage;
    [SerializeField] private TMP_Text buildInspectDamageText;

    private Animator animator;
    private Animator animatorInspect;
    private List<GameObject> recipeInstances = new();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        animator = canvasParent.GetComponent<Animator>();
        animatorInspect = inspectCanvasParent.GetComponent<Animator>();
        DayNightCycleManager.Instance.OnDayChange += UpdateBuildingStatus;
    }

    private void UpdateBuildingStatus(bool value) => IsDay = value;

    #region UI

    public void ShowUI(bool state) => animator.SetBool("Using", state);
    public void ShowInspectUI(bool state) => animatorInspect.SetBool("Using", state);

    public void UpdateBuildUI(Building building, bool selectionMode)
    {
        buildIcon.sprite = building.data.sprite;
        buildTitle.text = building.data.buildingName;
        buildPriceText.text = building.data.pointsWorth.ToString();

        string count = currentBuilds > buildLimitPoints
            ? "<color=red>" + currentBuilds + "</color>"
            : currentBuilds.ToString();
        buildsQuantityText.text = count + "/" + buildLimitPoints;

        selectionModeImage.gameObject.SetActive(selectionMode);
        UpdateRecipe(building.ingredients.ToList());
    }

    public void UpdateInspectUI(Building building)
    {
        buildInspectbuildIcon.sprite = building.data.sprite;
        buildInspectTitle.text = building.data.buildingName;

        if (building.currentDamage <= 0)
        {
            buildInspectDamageText.text = "";
            buildInspectDamage.text = "";
        }
        else
        {
            buildInspectDamageText.text = "Damage";
            buildInspectDamage.text = building.currentDamage.ToString();
        }

        buildInspectHealthSlider.maxValue = building.initHealth + extraBuildingHealth;
        buildInspectHealthSlider.value = building.currentHealth;
    }

    private void UpdateRecipe(List<Ingredient> ingredients)
    {
        //add slots if needed
        while (recipeInstances.Count < ingredients.Count)
            recipeInstances.Add(Instantiate(recipePrefab, recipeParent).gameObject);

        //remove excess slots
        while (recipeInstances.Count > ingredients.Count)
        {
            int last = recipeInstances.Count - 1;
            Destroy(recipeInstances[last]);
            recipeInstances.RemoveAt(last);
        }

        //fill slots
        for (int i = 0; i < ingredients.Count; i++)
        {
            Image img = recipeInstances[i].GetComponent<Image>();
            img.sprite = ingredients[i].item.data.sprite;
            img.GetComponentInChildren<TMP_Text>().text = ingredients[i].quantity.ToString();
        }
    }

    #endregion

    #region Grid

    public void UpdateGrid()
    {
        OnGridUpdated?.Invoke();
        currentBuilds = 0;

        foreach (var build in GameObject.FindGameObjectsWithTag("Build"))
            currentBuilds += build.GetComponent<Building>().data.pointsWorth;
    }

    #endregion

    void OnValidate() => Instance = this;
}