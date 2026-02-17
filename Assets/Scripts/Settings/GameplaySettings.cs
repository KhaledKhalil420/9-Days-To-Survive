using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.IO;
using System.Collections.Generic;

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
    private string               savePath;

    void Awake()
    {
        savePath = Path.GetFullPath(Path.Combine(Application.dataPath, "../GameplaySettings.json"));

        SpawnControls();

#if UNITY_EDITOR
        ApplyDefaults();
#else
        LoadSettings();
#endif

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

    // ── Defaults ─────────────────────────────────────────────────────────

    void ApplyDefaults()
    {
        settingsData = new GameplaySettingsData
        {
            sensitivity    = 50f,
            questsEnabled  = true
        };

        PushToUI();
        ApplyAll();
    }

    // ── Save / Load ──────────────────────────────────────────────────────

    public void SaveSettings()
    {
        try
        {
            File.WriteAllText(savePath, JsonUtility.ToJson(settingsData, true));
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[GameplaySettings] Could not save settings: {e.Message}");
        }
    }

    void LoadSettings()
    {
        if (File.Exists(savePath))
        {
            try
            {
                settingsData = JsonUtility.FromJson<GameplaySettingsData>(File.ReadAllText(savePath));
                settingsData.sensitivity = Mathf.Clamp(settingsData.sensitivity, 1f, 100f);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[GameplaySettings] Corrupt save file, resetting defaults: {e.Message}");
                WriteDefaults();
            }
        }
        else
        {
            WriteDefaults();
        }

        PushToUI();
        ApplyAll();
    }

    void WriteDefaults()
    {
        settingsData = new GameplaySettingsData
        {
            sensitivity   = 50f,
            questsEnabled = true
        };
        SaveSettings();
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