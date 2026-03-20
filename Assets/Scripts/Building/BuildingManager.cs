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

    [Header("Building Effects")]
    public ParticleSystem smoke;

    [Header("Build Settings")]
    internal bool IsDay = true;
    public static bool CanBuild(float points) { return Instance.IsDay && Instance.currentBuilds + points <= Instance.buildLimitPoints; }
    public static bool CanBuild() { return Instance.IsDay && Instance.currentBuilds <= Instance.buildLimitPoints; }

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
    [Header("Building")]
    [SerializeField] private TMP_Text buildsQuantityText;
    [SerializeField] private Transform canvasParent;
    [SerializeField] private Image buildIcon;
    [SerializeField] private Image selectionModeImage;
    [SerializeField] private TMP_Text buildPriceText;
    [SerializeField] private TMP_Text buildTitle;
    [SerializeField] private Transform recipeParent;
    [SerializeField] private Image recipePrefab;

    [Header("Building Property Inspecting")]
    [SerializeField] private Transform inspectCanvasParent;
    [SerializeField] private TMP_Text buildInspectTitle;
    [SerializeField] private Slider buildInspectHealthSlider;
    [SerializeField] private Image buildInspectbuildIcon;
    [SerializeField] private TMP_Text buildInspectDamage;
    [SerializeField] private TMP_Text buildInspectDamageText;

    private Animator animator;
    private Animator animatorInspect;

    private List<GameObject> recipeInstances = new();
    private List<Building> nightDisabledBuildings = new();

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

    private void UpdateBuildingStatus(bool isDay)
    {
        IsDay = isDay;
        if (isDay) RespawnNightDisabledBuildings();
    }

    #region Night Disabled Buildings

    public void RegisterDisabled(Building building)
    {
        nightDisabledBuildings.Add(building);
    }

    private void RespawnNightDisabledBuildings()
    {
        foreach (Building building in nightDisabledBuildings)
        {
            if (building == null) continue;
            building.currentHealth = building.initHealth;
            building.isPendingDestroy = false;
            building.gameObject.SetActive(true);
        }

        nightDisabledBuildings.Clear();
    }

    #endregion

    public void ShowUI(bool state)
    {
        animator.SetBool("Using", state);
    }

    public void ShowInspectUI(bool state)
    {
        animatorInspect.SetBool("Using", state);
    }

    public void UpdateBuildUI(Building building, bool selectionMode)
    {
        buildIcon.sprite = building.data.sprite;
        buildTitle.text = building.data.buildingName;
        buildPriceText.text = building.data.pointsWorth.ToString();
        string currentQuantity = currentBuilds > buildLimitPoints ? "<color=red>" + currentBuilds.ToString() + "</color>" : currentBuilds.ToString();
        buildsQuantityText.text = currentQuantity + "/" + buildLimitPoints.ToString();

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

    void UpdateRecipe(List<Ingredient> ingredients)
    {
        while (recipeInstances.Count < ingredients.Count)
        {
            Image img = Instantiate(recipePrefab, recipeParent);
            recipeInstances.Add(img.gameObject);
        }

        while (recipeInstances.Count > ingredients.Count)
        {
            int lastIndex = recipeInstances.Count - 1;
            Destroy(recipeInstances[lastIndex]);
            recipeInstances.RemoveAt(lastIndex);
        }

        for (int i = 0; i < ingredients.Count; i++)
        {
            Image img = recipeInstances[i].GetComponent<Image>();
            img.sprite = ingredients[i].item.data.sprite;
            img.GetComponentInChildren<TMP_Text>().text = ingredients[i].quantity.ToString();
        }
    }

    public void UpdateGrid()
    {
        OnGridUpdated?.Invoke();

        currentBuilds = 0;

        GameObject[] builds = GameObject.FindGameObjectsWithTag("Build");
        for (int i = 0; i < builds.Length; i++)
            currentBuilds += builds[i].GetComponent<Building>().data.pointsWorth;
    }

    void ClearRecipe()
    {
        recipeInstances.ForEach(Destroy);
        recipeInstances.Clear();
    }

    void OnValidate()
    {
        Instance = this;
    }
}