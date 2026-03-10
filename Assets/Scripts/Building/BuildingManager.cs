using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
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

    [Header("Building Effects")]
    public ParticleSystem smoke;

    [Header("Build Settings")]
    internal bool IsDay = true;
    public static bool CanBuild(float points) => Instance.IsDay && Instance.currentBuilds + points <= Instance.buildLimitPoints;
    public static bool CanBuild() => Instance.IsDay && Instance.currentBuilds <= Instance.buildLimitPoints;

    public int gridSize = 2;
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
    [SerializeField] private TMP_Text buildInspectTitle;
    [SerializeField] private Slider buildInspectHealthSlider;
    [SerializeField] private Image buildInspectIcon;
    [SerializeField] private TMP_Text buildInspectDamage;

    [Header("UI - Tabs")]
    [SerializeField] private CanvasGroup buildingTab;
    [SerializeField] private CanvasGroup propertiesTab;
    [SerializeField] private float tabFadeDuration = 0.2f;

    private Animator animator;
    private List<GameObject> recipeInstances = new();

    #region Unity

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        animator = canvasParent.GetComponent<Animator>();
        DayNightCycleManager.Instance.OnDayChange += UpdateBuildingStatus;
    }

    #endregion

    #region Status

    private void UpdateBuildingStatus(bool isDay)
    {
        IsDay = isDay;
        SetTab(isDay ? buildingTab : null);
    }

    public void ShowUI(bool state)
    {
        animator.SetBool("Using", state);
    }

    private void SetTab(CanvasGroup active)
    {
        FadeTab(buildingTab, active == buildingTab ? 1f : 0f);
        FadeTab(propertiesTab, active == propertiesTab ? 1f : 0f);
    }

    private void FadeTab(CanvasGroup tab, float target)
    {
        tab.DOFade(target, tabFadeDuration);
        tab.interactable = target > 0f;
        tab.blocksRaycasts = target > 0f;
    }

    #endregion

    #region Building Tab

    public void UpdateBuildUI(Building building, bool selectionMode)
    {
        SetTab(buildingTab);

        buildIcon.sprite = building.data.sprite;
        buildTitle.text = building.data.buildingName;
        buildPriceText.text = building.data.pointsWorth.ToString();

        string currentQuantity = currentBuilds > buildLimitPoints
            ? "<color=red>" + currentBuilds + "</color>"
            : currentBuilds.ToString();

        buildsQuantityText.text = currentQuantity + "/" + buildLimitPoints;

        selectionModeImage.gameObject.SetActive(selectionMode);
        UpdateRecipe(building.ingredients.ToList());
    }

    private void UpdateRecipe(List<Ingredient> ingredients)
    {
        while (recipeInstances.Count < ingredients.Count)
            recipeInstances.Add(Instantiate(recipePrefab, recipeParent).gameObject);

        while (recipeInstances.Count > ingredients.Count)
        {
            int last = recipeInstances.Count - 1;
            Destroy(recipeInstances[last]);
            recipeInstances.RemoveAt(last);
        }

        for (int i = 0; i < ingredients.Count; i++)
        {
            Image img = recipeInstances[i].GetComponent<Image>();
            img.sprite = ingredients[i].item.data.sprite;
            img.GetComponentInChildren<TMP_Text>().text = ingredients[i].quantity.ToString();
        }
    }

    private void ClearRecipe()
    {
        recipeInstances.ForEach(Destroy);
        recipeInstances.Clear();
    }

    #endregion

    #region Properties Tab

    public void InspectBuilding(Building building)
    {
        SetTab(propertiesTab);

        buildInspectTitle.text = building.data.buildingName;
        buildInspectIcon.sprite = building.data.sprite;
        buildInspectDamage.text = building.extraDamage.ToString();

        buildInspectHealthSlider.maxValue = building.initHealth;
        buildInspectHealthSlider.value = building.currentHealth;
    }

    public void ClearInspect()
    {
        SetTab(IsDay ? buildingTab : null);
    }

    #endregion

    #region Grid

    public void UpdateGrid()
    {
        OnGridUpdated?.Invoke();

        currentBuilds = 0;
        foreach (GameObject build in GameObject.FindGameObjectsWithTag("Build"))
            currentBuilds += build.GetComponent<Building>().data.pointsWorth;
    }

    #endregion

    private void OnValidate() => Instance = this;
}