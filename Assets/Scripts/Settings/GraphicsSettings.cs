using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.IO;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable]
public class GraphicsSettingsData
{
    public int   resolutionIndex;
    public int   fullscreenMode;
    public bool  vsync;
    public int   fpsLimit;
    public int   shadowQuality;
    public int   shadowCascades;
    public int   shadowType;
    public float renderScale;
    public int   antiAliasing;
    public bool  bloom;
    public bool  motionBlur;
    public bool  ambientOcclusion;
    public bool grass;
}

public class GraphicsSettings : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private TMP_Dropdown dropdownPrefab;
    [SerializeField] private Slider       sliderPrefab;
    [SerializeField] private Toggle       togglePrefab;

    [Header("Category Parents")]
    [SerializeField] private Transform displayParent;
    [SerializeField] private Transform qualityParent;
    [SerializeField] private Transform shadowsParent;
    [SerializeField] private Transform postFXParent;

    [Header("Post Processing")]
    [SerializeField] private Volume postProcessVolume;

    // Spawned controls
    private TMP_Dropdown resolutionDropdown;
    private TMP_Dropdown fullscreenDropdown;
    private Toggle       vsyncToggle;
    private Slider       fpsSlider;

    private Slider       renderScaleSlider;
    private TMP_Dropdown antiAliasingDropdown;

    private TMP_Dropdown shadowQualityDropdown;
    private TMP_Dropdown shadowCascadesDropdown;
    private TMP_Dropdown shadowTypeDropdown;

    private Toggle bloomToggle;
    private Toggle motionBlurToggle;
    private Toggle ambientOcclusionToggle;
    private Toggle grassToggle;
    [SerializeField] private GrassComputeScript grassCompute;

    // URP / Post processing
    private UniversalRenderPipelineAsset urpAsset;
    private Bloom                        bloomOverride;
    private MotionBlur                   motionBlurOverride;
    private ScriptableRendererFeature    ssaoFeature;

    private GraphicsSettingsData settingsData         = new GraphicsSettingsData();
    private List<Resolution>     availableResolutions = new List<Resolution>();

    void Awake()
    {
        BuildAvailableResolutions();
        FindPostProcessing();
        SpawnControls();
        LoadSettings();      // always load — works in editor and in build
        HookListeners();
    }

    void SpawnControls()
    {
        // Display
        resolutionDropdown  = SpawnDropdown(displayParent, BuildResolutionOptions(), availableResolutions.Count - 1, "Resolution");
        fullscreenDropdown  = SpawnDropdown(displayParent, new List<string> { "Windowed", "Fullscreen", "Borderless" }, 1, "Fullscreen Mode");
        vsyncToggle         = SpawnToggle(displayParent, true,   "VSync");
        fpsSlider           = SpawnSlider(displayParent, 30, 600, 60, true, "FPS Limit");

        // Quality
        renderScaleSlider    = SpawnSlider(qualityParent, 0.1f, 1.15f, 1f, false, "Render Scale");
        antiAliasingDropdown = SpawnDropdown(qualityParent, new List<string> { "High (8x)", "Low (2x)", "Off" }, 0, "Anti-Aliasing");

        // Shadows
        shadowQualityDropdown  = SpawnDropdown(shadowsParent, new List<string> { "High (2048)", "Medium (1024)", "Low (512)" }, 0, "Shadow Quality");
        shadowCascadesDropdown = SpawnDropdown(shadowsParent, new List<string> { "1 Cascade", "2 Cascades", "3 Cascades", "4 Cascades" }, 3, "Shadow Cascades");
        shadowTypeDropdown     = SpawnDropdown(shadowsParent, new List<string> { "Soft", "Hard" }, 0, "Shadow Type");

        // Post FX
        bloomToggle            = SpawnToggle(postFXParent, true,  "Bloom");
        motionBlurToggle       = SpawnToggle(postFXParent, false, "Motion Blur");
        ambientOcclusionToggle = SpawnToggle(postFXParent, true,  "Ambient Occlusion");
        grassToggle = SpawnToggle(qualityParent, true,  "Ambient Occlusion");

        // Sliders save only when the user releases the handle
        AddSliderEndListener(fpsSlider);
        AddSliderEndListener(renderScaleSlider);
    }

    void HookListeners()
    {
        resolutionDropdown.onValueChanged.AddListener(delegate     { SetResolution(); });
        fullscreenDropdown.onValueChanged.AddListener(delegate     { SetFullscreen(); });
        vsyncToggle.onValueChanged.AddListener(delegate            { SetVSync(); });
        fpsSlider.onValueChanged.AddListener(delegate              { SetFPS(); });
        renderScaleSlider.onValueChanged.AddListener(delegate      { SetRenderScale(); });
        antiAliasingDropdown.onValueChanged.AddListener(delegate   { SetAntiAliasing(); });
        shadowQualityDropdown.onValueChanged.AddListener(delegate  { SetShadowQuality(); });
        shadowCascadesDropdown.onValueChanged.AddListener(delegate { SetShadowCascades(); });
        shadowTypeDropdown.onValueChanged.AddListener(delegate     { SetShadowType(); });
        bloomToggle.onValueChanged.AddListener(delegate            { SetBloom(); });
        grassToggle.onValueChanged.AddListener(delegate            { SetGrass(); });
        motionBlurToggle.onValueChanged.AddListener(delegate       { SetMotionBlur(); });
        ambientOcclusionToggle.onValueChanged.AddListener(delegate { SetAmbientOcclusion(); });
    }

    // ── Setters ───────────────────────────────────────────────────────────

    void SetResolution()
    {
        var res = availableResolutions[resolutionDropdown.value];
        Screen.SetResolution(res.width, res.height, GetFullscreenMode());
        settingsData.resolutionIndex = resolutionDropdown.value;
        SaveSettings();
    }

    void SetFullscreen()
    {
        Screen.fullScreenMode       = GetFullscreenMode();
        settingsData.fullscreenMode = fullscreenDropdown.value;
        SaveSettings();
    }

    void SetVSync()
    {
        QualitySettings.vSyncCount = vsyncToggle.isOn ? 1 : 0;
        settingsData.vsync         = vsyncToggle.isOn;
        SaveSettings();
    }

    void SetFPS()
    {
        Application.targetFrameRate = (int)fpsSlider.value;
        settingsData.fpsLimit       = (int)fpsSlider.value;
        // saved on pointer up via AddSliderEndListener
    }

    void SetRenderScale()
    {
        float scale = Mathf.Round(renderScaleSlider.value * 100f) / 100f;
        if (urpAsset != null) urpAsset.renderScale = scale;
        settingsData.renderScale = scale;
        // saved on pointer up via AddSliderEndListener
    }

    void SetAntiAliasing()
    {
        QualitySettings.antiAliasing = antiAliasingDropdown.value switch { 1 => 2, 2 => 0, _ => 8 };
        settingsData.antiAliasing    = antiAliasingDropdown.value;
        SaveSettings();
    }

    void SetShadowQuality()
    {
        int res = shadowQualityDropdown.value switch { 1 => 1024, 2 => 512, _ => 2048 };
        SetURPField("m_MainLightShadowmapResolution", res);
        settingsData.shadowQuality = shadowQualityDropdown.value;
        SaveSettings();
    }

    void SetShadowCascades()
    {
        int count = shadowCascadesDropdown.value + 1;
        SetURPField("m_ShadowCascadeCount", count);
        settingsData.shadowCascades = shadowCascadesDropdown.value;
        SaveSettings();
    }

    void SetShadowType()
    {
        SetURPField("m_SoftShadowsSupported", shadowTypeDropdown.value == 0);
        settingsData.shadowType = shadowTypeDropdown.value;
        SaveSettings();
    }

    void SetBloom()
    {
        if (bloomOverride != null) bloomOverride.active = bloomToggle.isOn;
        settingsData.bloom = bloomToggle.isOn;
        SaveSettings();
    }


    void SetGrass()
    {
        if (grassCompute != null) grassCompute.grassEnabled = grassToggle.isOn;
        settingsData.grass = grassToggle.isOn;
        SaveSettings();
    }

    void SetMotionBlur()
    {
        if (motionBlurOverride != null) motionBlurOverride.active = motionBlurToggle.isOn;
        settingsData.motionBlur = motionBlurToggle.isOn;
        SaveSettings();
    }

    void SetAmbientOcclusion()
    {
        if (ssaoFeature != null) ssaoFeature.SetActive(ambientOcclusionToggle.isOn);
        settingsData.ambientOcclusion = ambientOcclusionToggle.isOn;
        SaveSettings();
    }

    void ApplyAll()
    {
        SetResolution();    SetFullscreen();     SetVSync();       SetFPS();
        SetRenderScale();   SetAntiAliasing();
        SetShadowQuality(); SetShadowCascades(); SetShadowType();
        SetBloom();         SetMotionBlur();     SetAmbientOcclusion();
    }

    // ── Save / Load ──────────────────────────────────────────────────────

    public void SaveSettings() => SettingsFileManager.SaveGraphics(settingsData);

    void LoadSettings()
    {
        settingsData = SettingsFileManager.Data.graphics;
        settingsData.resolutionIndex = Mathf.Clamp(settingsData.resolutionIndex, 0, availableResolutions.Count - 1);
        settingsData.renderScale     = Mathf.Clamp(settingsData.renderScale, 0.1f, 1.15f);

        PushToUI();
        ApplyAll();
    }

    void PushToUI()
    {
        resolutionDropdown.SetValueWithoutNotify(settingsData.resolutionIndex);
        fullscreenDropdown.SetValueWithoutNotify(settingsData.fullscreenMode);
        vsyncToggle.SetIsOnWithoutNotify(settingsData.vsync);
        fpsSlider.SetValueWithoutNotify(settingsData.fpsLimit);
        shadowQualityDropdown.SetValueWithoutNotify(settingsData.shadowQuality);
        shadowCascadesDropdown.SetValueWithoutNotify(settingsData.shadowCascades);
        shadowTypeDropdown.SetValueWithoutNotify(settingsData.shadowType);
        renderScaleSlider.SetValueWithoutNotify(settingsData.renderScale);
        antiAliasingDropdown.SetValueWithoutNotify(settingsData.antiAliasing);
        bloomToggle.SetIsOnWithoutNotify(settingsData.bloom);
        grassToggle.SetIsOnWithoutNotify(settingsData.grass);
        motionBlurToggle.SetIsOnWithoutNotify(settingsData.motionBlur);
        ambientOcclusionToggle.SetIsOnWithoutNotify(settingsData.ambientOcclusion);
    }

    void OnApplicationQuit() => SaveSettings();

    // ── Spawn helpers ────────────────────────────────────────────────────

    TMP_Dropdown SpawnDropdown(Transform parent, List<string> options, int defaultVal, string label)
    {
        var dd = Instantiate(dropdownPrefab, parent);
        dd.gameObject.name = label;
        dd.ClearOptions();
        dd.AddOptions(options);
        dd.value = defaultVal;
        dd.RefreshShownValue();

        var labelText = dd.GetComponentInChildren<TMP_Text>();
        if (labelText != null) labelText.text = label;

        return dd;
    }

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

    // ── Post processing ──────────────────────────────────────────────────

    void FindPostProcessing()
    {
        var vol = postProcessVolume != null ? postProcessVolume : FindFirstObjectByType<Volume>();
        if (vol != null)
        {
            vol.profile.TryGet(out bloomOverride);
            vol.profile.TryGet(out motionBlurOverride);
        }

        urpAsset = UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset;

        if (urpAsset == null) return;

        var dataList = typeof(UniversalRenderPipelineAsset)
            .GetField("m_RendererDataList", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.GetValue(urpAsset) as ScriptableRendererData[];

        if (dataList == null) return;

        foreach (var data in dataList)
        {
            if (data == null) continue;
            foreach (var feature in data.rendererFeatures)
            {
                if (feature is not ScreenSpaceAmbientOcclusion) continue;
                ssaoFeature = feature;
                return;
            }
        }
    }

    // ── Resolution helpers ───────────────────────────────────────────────

    void BuildAvailableResolutions()
    {
        var nativeRes = Screen.resolutions[Screen.resolutions.Length - 1];

        Resolution[] all =
        {
            new Resolution { width = 640,  height = 360  },
            new Resolution { width = 854,  height = 480  },
            new Resolution { width = 1280, height = 720  },
            new Resolution { width = 1920, height = 1080 },
            new Resolution { width = 2560, height = 1440 },
            new Resolution { width = 3840, height = 2160 }
        };

        foreach (var r in all)
            if (r.width <= nativeRes.width && r.height <= nativeRes.height)
                availableResolutions.Add(r);
    }

    List<string> BuildResolutionOptions()
    {
        var opts = new List<string>();
        foreach (var res in availableResolutions)
            opts.Add(res.height switch
            {
                360  => "360p  (640×360)",
                480  => "480p  (854×480)",
                720  => "720p  (1280×720)",
                1080 => "1080p (1920×1080)",
                1440 => "1440p (2560×1440)",
                2160 => "4K    (3840×2160)",
                _    => $"{res.width}×{res.height}"
            });
        return opts;
    }

    FullScreenMode GetFullscreenMode() => fullscreenDropdown.value switch
    {
        0 => FullScreenMode.Windowed,
        2 => FullScreenMode.FullScreenWindow,
        _ => FullScreenMode.ExclusiveFullScreen
    };

    void SetURPField(string fieldName, object value)
    {
        if (urpAsset == null) return;
        typeof(UniversalRenderPipelineAsset)
            .GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(urpAsset, value);
    }
}