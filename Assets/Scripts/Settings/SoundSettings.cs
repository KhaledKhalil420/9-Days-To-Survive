using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.IO;

[System.Serializable]
public class SoundSettingsData
{
    public float masterVolume;
    public float musicVolume;
    public float sfxVolume;
}

public class SoundSettings : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private Slider sliderPrefab;

    [Header("Parent")]
    [SerializeField] private Transform soundParent;

    // Spawned controls
    private Slider masterSlider;
    private Slider musicSlider;
    private Slider sfxSlider;

    private SoundSettingsData settingsData = new SoundSettingsData();
    private string            savePath;

    void Awake()
    {
        savePath = Path.GetFullPath(Path.Combine(Application.dataPath, "../SoundSettings.json"));

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
        masterSlider = SpawnSlider(0f, 1f, 1f,    "Master Volume");
        musicSlider  = SpawnSlider(0f, 1f, 0.75f, "Music Volume");
        sfxSlider    = SpawnSlider(0f, 1f, 1f,    "SFX Volume");

        AddSliderEndListener(masterSlider);
        AddSliderEndListener(musicSlider);
        AddSliderEndListener(sfxSlider);
    }

    void HookListeners()
    {
        masterSlider.onValueChanged.AddListener(delegate { SetMaster(); });
        musicSlider.onValueChanged.AddListener(delegate  { SetMusic(); });
        sfxSlider.onValueChanged.AddListener(delegate    { SetSFX(); });
    }

    // ── Setters ───────────────────────────────────────────────────────────

    void SetMaster()
    {
        settingsData.masterVolume = masterSlider.value;
        SetMixerVolume(AudioManager.Instance.masterMixerGroup, masterSlider.value);
    }

    void SetMusic()
    {
        settingsData.musicVolume = musicSlider.value;
        SetMixerVolume(AudioManager.Instance.musicMixerGroup, musicSlider.value);
    }

    void SetSFX()
    {
        settingsData.sfxVolume = sfxSlider.value;
        SetMixerVolume(AudioManager.Instance.soundEffectMixerGroup, sfxSlider.value);
    }

    void SetMixerVolume(UnityEngine.Audio.AudioMixerGroup group, float linearValue)
    {
        if (AudioManager.Instance == null || group == null) return;
        float db = linearValue > 0.0001f ? Mathf.Log10(linearValue) * 20f : -80f;
        group.audioMixer.SetFloat("Volume" + group.name, db);
    }

    void ApplyAll()
    {
        SetMaster();
        SetMusic();
        SetSFX();
    }

    // ── Defaults ─────────────────────────────────────────────────────────

    void ApplyDefaults()
    {
        settingsData = new SoundSettingsData
        {
            masterVolume = 1f,
            musicVolume  = 0.75f,
            sfxVolume    = 1f
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
            Debug.LogWarning($"[SoundSettings] Could not save settings: {e.Message}");
        }
    }

    void LoadSettings()
    {
        if (File.Exists(savePath))
        {
            try
            {
                settingsData = JsonUtility.FromJson<SoundSettingsData>(File.ReadAllText(savePath));
                settingsData.masterVolume = Mathf.Clamp01(settingsData.masterVolume);
                settingsData.musicVolume  = Mathf.Clamp01(settingsData.musicVolume);
                settingsData.sfxVolume    = Mathf.Clamp01(settingsData.sfxVolume);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[SoundSettings] Corrupt save file, resetting defaults: {e.Message}");
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
        settingsData = new SoundSettingsData
        {
            masterVolume = 1f,
            musicVolume  = 0.75f,
            sfxVolume    = 1f
        };
        SaveSettings();
    }

    void PushToUI()
    {
        masterSlider.SetValueWithoutNotify(settingsData.masterVolume);
        musicSlider.SetValueWithoutNotify(settingsData.musicVolume);
        sfxSlider.SetValueWithoutNotify(settingsData.sfxVolume);
    }

    void OnApplicationQuit() => SaveSettings();

    // ── Spawn helpers ────────────────────────────────────────────────────

    Slider SpawnSlider(float min, float max, float defaultVal, string label)
    {
        var s = Instantiate(sliderPrefab, soundParent);
        s.gameObject.name = label;
        s.minValue        = min;
        s.maxValue        = max;
        s.wholeNumbers    = false;
        s.value           = defaultVal;

        var labelText = s.GetComponentInChildren<TMP_Text>();
        if (labelText != null) labelText.text = label;

        return s;
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