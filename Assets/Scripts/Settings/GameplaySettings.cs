using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

[System.Serializable]
public class GameplaySettingsData
{
    public float sensitivity;
    public bool  questsEnabled;
}

public class GameplaySettings : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private Slider  sliderPrefab;
    [SerializeField] private Toggle  togglePrefab;

    [Header("Category Parents")]
    [SerializeField] private Transform controlsParent;
    [SerializeField] private Transform gameplayParent;

    // Spawned controls
    private Slider sensitivitySlider;
    private Toggle questsToggle;

    private GameplaySettingsData settingsData = new GameplaySettingsData();

    void Awake()
    {
        SpawnControls();
        LoadSettings();      // always load — works in editor and in build
        HookListeners();
    }

    void SpawnControls()
    {
        // Controls
        sensitivitySlider = SpawnSlider(controlsParent, 1f, 100f, 50f, false, "Sensitivity");

        // Gameplay
        questsToggle = SpawnToggle(gameplayParent, true, "Quests");

        // Slider saves on pointer up only
        AddSliderEndListener(sensitivitySlider);
    }

    void HookListeners()
    {
        sensitivitySlider.onValueChanged.AddListener(delegate { SetSensitivity(); });
        questsToggle.onValueChanged.AddListener(delegate      { SetQuests(); });
    }

    // ── Setters ───────────────────────────────────────────────────────────

    void SetSensitivity()
    {
        float val = Mathf.Round(sensitivitySlider.value * 10f) / 10f;
        settingsData.sensitivity = val;

        if (PlayerLook.instance != null)
            PlayerLook.instance.Sensitivity = val;
        // saved on pointer up via AddSliderEndListener
    }

    void SetQuests()
    {
        settingsData.questsEnabled = questsToggle.isOn;

        if (QuestManager.Instance != null)
            QuestManager.Instance.turnOn = questsToggle.isOn;

        SaveSettings();
    }

    void ApplyAll()
    {
        SetSensitivity();
        SetQuests();
    }

    // ── Save / Load ──────────────────────────────────────────────────────

    public void SaveSettings() => SettingsFileManager.SaveGameplay(settingsData);

    void LoadSettings()
    {
        settingsData = SettingsFileManager.Data.gameplay;
        settingsData.sensitivity = Mathf.Clamp(settingsData.sensitivity, 1f, 100f);

        PushToUI();
        ApplyAll();
    }

    void PushToUI()
    {
        sensitivitySlider.SetValueWithoutNotify(settingsData.sensitivity);
        questsToggle.SetIsOnWithoutNotify(settingsData.questsEnabled);
    }

    void OnApplicationQuit() => SaveSettings();

    // ── Spawn helpers ────────────────────────────────────────────────────

    Slider SpawnSlider(Transform parent, float min, float max, float defaultVal, bool wholeNumbers, string label)
    {
        var s = Instantiate(sliderPrefab, parent);
        s.gameObject.name = label;
        s.minValue        = min;
        s.maxValue        = max;
        s.wholeNumbers    = wholeNumbers;
        s.value           = defaultVal;

        var labelText = s.GetComponentInChildren<TMP_Text>();
        if (labelText != null) labelText.text = label;

        return s;
    }

    Toggle SpawnToggle(Transform parent, bool defaultVal, string label)
    {
        var t = Instantiate(togglePrefab, parent);
        t.gameObject.name = label;
        t.isOn            = defaultVal;

        var labelText = t.GetComponentInChildren<TMP_Text>();
        if (labelText != null) labelText.text = label;

        return t;
    }

    void AddSliderEndListener(Slider slider)
    {
        var trigger = slider.gameObject.GetComponent<EventTrigger>()
                      ?? slider.gameObject.AddComponent<EventTrigger>();

        var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        entry.callback.AddListener((_) => SaveSettings());
        trigger.triggers.Add(entry);
    }
}