using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class GraphicsSettingsData
{
    public int resolutionIndex;
    public int fullscreenMode;
    public bool vsync;
    public int fpsLimit;
    public int shadowQuality;
    public int shadowResolution;
    public int antiAliasing;
    public bool bloom;
    public bool motionBlur;
    public bool ambientOcclusion;
}

public class GraphicsSettings : MonoBehaviour
{
    [Header("Resolution")]
    public TMP_Dropdown resolutionDropdown;
    
    [Header("Display Mode")]
    public TMP_Dropdown fullscreenModeDropdown;
    
    [Header("VSync")]
    public Toggle vsyncToggle;
    public TMP_Text vsyncText;
    
    [Header("FPS Limit")]
    public Slider fpsLimitSlider;
    public TMP_Text fpsLimitText;
    
    [Header("Shadow Quality")]
    public TMP_Dropdown shadowQualityDropdown;
    
    [Header("Shadow Resolution")]
    public TMP_Dropdown shadowResolutionDropdown;
    
    [Header("Anti Aliasing")]
    public TMP_Dropdown antiAliasingDropdown;
    
    [Header("Bloom")]
    public Toggle bloomToggle;
    public TMP_Text bloomText;
    
    [Header("Motion Blur")]
    public Toggle motionBlurToggle;
    public TMP_Text motionBlurText;
    
    [Header("Ambient Occlusion")]
    public Toggle ambientOcclusionToggle;
    public TMP_Text ambientOcclusionText;

    [Header("URP Post Processing")]
    public UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset urpAsset;

    private GraphicsSettingsData settingsData = new GraphicsSettingsData();
    private string savePath;

    private List<Resolution> availableResolutions = new List<Resolution>();
    private Resolution maxResolution;

    void Awake()
    {
        //Save to game directory
        savePath = Path.Combine(Application.dataPath, "GraphicsSettings.json");
        
        //Get max screen resolution
        maxResolution = Screen.resolutions[Screen.resolutions.Length - 1];
        BuildAvailableResolutions();
    }

    void BuildAvailableResolutions()
    {
        // Define all possible 16:9 resolutions
        Resolution[] allResolutions = new Resolution[]
        {
            new Resolution { width = 256, height = 144 },   // 144p
            new Resolution { width = 426, height = 240 },   // 240p
            new Resolution { width = 640, height = 360 },   // 360p
            new Resolution { width = 854, height = 480 },   // 480p
            new Resolution { width = 1280, height = 720 },  // 720p
            new Resolution { width = 1920, height = 1080 }, // 1080p
            new Resolution { width = 2560, height = 1440 }, // 1440p
            new Resolution { width = 3840, height = 2160 }  // 2160p
        };

        //Only add resolutions that don't exceed screen max
        foreach (var res in allResolutions)
        {
            if (res.width <= maxResolution.width && res.height <= maxResolution.height)
            {
                availableResolutions.Add(res);
            }
        }

        Debug.Log($"Max screen resolution: {maxResolution.width}x{maxResolution.height}");
        Debug.Log($"Available resolutions: {availableResolutions.Count}");
    }

    void Start()
    {
        InitializeResolutions();
        InitializeFullscreenModes();
        InitializeShadowQuality();
        InitializeShadowResolution();
        InitializeAntiAliasing();
        InitializeFPSSlider();
        
        LoadSettings();
        
        //Add listeners
        resolutionDropdown.onValueChanged.AddListener(delegate { SetResolution(); });
        fullscreenModeDropdown.onValueChanged.AddListener(delegate { SetFullscreenMode(); });
        vsyncToggle.onValueChanged.AddListener(delegate { SetVSync(); });
        fpsLimitSlider.onValueChanged.AddListener(delegate { SetFPSLimit(); });
        shadowQualityDropdown.onValueChanged.AddListener(delegate { SetShadowQuality(); });
        shadowResolutionDropdown.onValueChanged.AddListener(delegate { SetShadowResolution(); });
        antiAliasingDropdown.onValueChanged.AddListener(delegate { SetAntiAliasing(); });
        bloomToggle.onValueChanged.AddListener(delegate { SetBloom(); });
        motionBlurToggle.onValueChanged.AddListener(delegate { SetMotionBlur(); });
        ambientOcclusionToggle.onValueChanged.AddListener(delegate { SetAmbientOcclusion(); });
    }

    void InitializeResolutions()
    {
        resolutionDropdown.ClearOptions();
        
        var options = new List<string>();
        foreach (var res in availableResolutions)
        {
            string label = "";
            if (res.height == 144) label = "144p (256x144)";
            else if (res.height == 240) label = "240p (426x240)";
            else if (res.height == 360) label = "360p (640x360)";
            else if (res.height == 480) label = "480p (854x480)";
            else if (res.height == 720) label = "720p (1280x720)";
            else if (res.height == 1080) label = "1080p (1920x1080)";
            else if (res.height == 1440) label = "1440p (2560x1440)";
            else if (res.height == 2160) label = "4K (3840x2160)";
            else label = $"{res.width}x{res.height}";
            
            options.Add(label);
        }
        
        resolutionDropdown.AddOptions(options);
        
        // Set default to highest available resolution
        resolutionDropdown.value = availableResolutions.Count - 1;
        resolutionDropdown.RefreshShownValue();
    }

    void InitializeFullscreenModes()
    {
        fullscreenModeDropdown.ClearOptions();
        fullscreenModeDropdown.AddOptions(new List<string>
        {
            "Windowed",
            "Fullscreen",
            "Windowed Borderless"
        });
        fullscreenModeDropdown.value = 1;
        fullscreenModeDropdown.RefreshShownValue();
    }

    void InitializeShadowQuality()
    {
        shadowQualityDropdown.ClearOptions();
        shadowQualityDropdown.AddOptions(new List<string>
        {
            "High",
            "Medium",
            "Low"
        });
        shadowQualityDropdown.value = 0;
        shadowQualityDropdown.RefreshShownValue();
    }

    void InitializeShadowResolution()
    {
        shadowResolutionDropdown.ClearOptions();
        shadowResolutionDropdown.AddOptions(new List<string>
        {
            "High",
            "Medium",
            "Low"
        });
        shadowResolutionDropdown.value = 0;
        shadowResolutionDropdown.RefreshShownValue();
    }

    void InitializeAntiAliasing()
    {
        antiAliasingDropdown.ClearOptions();
        antiAliasingDropdown.AddOptions(new List<string>
        {
            "High",
            "Low",
            "Off"
        });
        antiAliasingDropdown.value = 0;
        antiAliasingDropdown.RefreshShownValue();
    }

    void InitializeFPSSlider()
    {
        fpsLimitSlider.minValue = 30;
        fpsLimitSlider.maxValue = 240;
        fpsLimitSlider.wholeNumbers = true;
        fpsLimitSlider.value = 60;
        UpdateFPSText();
    }

    public void SetResolution()
    {
        int index = resolutionDropdown.value;
        Resolution res = availableResolutions[index];
        
        FullScreenMode mode = GetCurrentFullscreenMode();
        Screen.SetResolution(res.width, res.height, mode);
        
        settingsData.resolutionIndex = index;
        
        Debug.Log($"Resolution set to: {res.width}x{res.height}");
    }

    public void SetFullscreenMode()
    {
        FullScreenMode mode = GetCurrentFullscreenMode();
        Screen.fullScreenMode = mode;
        
        settingsData.fullscreenMode = fullscreenModeDropdown.value;
    }

    FullScreenMode GetCurrentFullscreenMode()
    {
        switch (fullscreenModeDropdown.value)
        {
            case 0: return FullScreenMode.Windowed;
            case 1: return FullScreenMode.ExclusiveFullScreen;
            case 2: return FullScreenMode.FullScreenWindow;
            default: return FullScreenMode.ExclusiveFullScreen;
        }
    }

    public void SetVSync()
    {
        QualitySettings.vSyncCount = vsyncToggle.isOn ? 1 : 0;
        vsyncText.text = vsyncToggle.isOn ? "ON" : "OFF";
        
        settingsData.vsync = vsyncToggle.isOn;
    }

    public void SetFPSLimit()
    {
        int fps = (int)fpsLimitSlider.value;
        Application.targetFrameRate = fps;
        UpdateFPSText();
        
        settingsData.fpsLimit = fps;
    }

    void UpdateFPSText()
    {
        fpsLimitText.text = ((int)fpsLimitSlider.value).ToString();
    }

    public void SetShadowQuality()
    {
        switch (shadowQualityDropdown.value)
        {
            case 0: QualitySettings.shadows = ShadowQuality.All; break;
            case 1: QualitySettings.shadows = ShadowQuality.HardOnly; break;
            case 2: QualitySettings.shadows = ShadowQuality.Disable; break;
        }
        
        settingsData.shadowQuality = shadowQualityDropdown.value;
    }

    public void SetShadowResolution()
    {
        switch (shadowResolutionDropdown.value)
        {
            case 0: QualitySettings.shadowResolution = ShadowResolution.VeryHigh; break;
            case 1: QualitySettings.shadowResolution = ShadowResolution.Medium; break;
            case 2: QualitySettings.shadowResolution = ShadowResolution.Low; break;
        }
        
        settingsData.shadowResolution = shadowResolutionDropdown.value;
    }

    public void SetAntiAliasing()
    {
        switch (antiAliasingDropdown.value)
        {
            case 0: QualitySettings.antiAliasing = 8; break;
            case 1: QualitySettings.antiAliasing = 2; break;
            case 2: QualitySettings.antiAliasing = 0; break;
        }
        
        settingsData.antiAliasing = antiAliasingDropdown.value;
    }

    public void SetBloom()
    {
        bloomText.text = bloomToggle.isOn ? "ON" : "OFF";
        settingsData.bloom = bloomToggle.isOn;
    }

    public void SetMotionBlur()
    {
        motionBlurText.text = motionBlurToggle.isOn ? "ON" : "OFF";
        settingsData.motionBlur = motionBlurToggle.isOn;
    }

    public void SetAmbientOcclusion()
    {
        ambientOcclusionText.text = ambientOcclusionToggle.isOn ? "ON" : "OFF";
        settingsData.ambientOcclusion = ambientOcclusionToggle.isOn;
    }

    // SAVE TO JSON
    public void SaveSettings()
    {
        string json = JsonUtility.ToJson(settingsData, true);
        File.WriteAllText(savePath, json);
        Debug.Log($"Settings saved to: {savePath}");
    }

    // LOAD FROM JSON
    void LoadSettings()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            settingsData = JsonUtility.FromJson<GraphicsSettingsData>(json);
            
            //Clamp resolution index to available resolutions
            settingsData.resolutionIndex = Mathf.Clamp(settingsData.resolutionIndex, 0, availableResolutions.Count - 1);
            
            //Apply loaded settings to UI
            resolutionDropdown.value = settingsData.resolutionIndex;
            fullscreenModeDropdown.value = settingsData.fullscreenMode;
            vsyncToggle.isOn = settingsData.vsync;
            vsyncText.text = settingsData.vsync ? "ON" : "OFF";
            fpsLimitSlider.value = settingsData.fpsLimit;
            shadowQualityDropdown.value = settingsData.shadowQuality;
            shadowResolutionDropdown.value = settingsData.shadowResolution;
            antiAliasingDropdown.value = settingsData.antiAliasing;
            bloomToggle.isOn = settingsData.bloom;
            bloomText.text = settingsData.bloom ? "ON" : "OFF";
            motionBlurToggle.isOn = settingsData.motionBlur;
            motionBlurText.text = settingsData.motionBlur ? "ON" : "OFF";
            ambientOcclusionToggle.isOn = settingsData.ambientOcclusion;
            ambientOcclusionText.text = settingsData.ambientOcclusion ? "ON" : "OFF";
            
            //Apply settings to Unity
            SetResolution();
            SetFullscreenMode();
            SetVSync();
            SetFPSLimit();
            SetShadowQuality();
            SetShadowResolution();
            SetAntiAliasing();
            
            Debug.Log($"Settings loaded from: {savePath}");
        }
        else
        {
            //Set default values
            settingsData.resolutionIndex = availableResolutions.Count - 1; // Highest available
            settingsData.fullscreenMode = 1;
            settingsData.vsync = true;
            settingsData.fpsLimit = 60;
            settingsData.shadowQuality = 0;
            settingsData.shadowResolution = 0;
            settingsData.antiAliasing = 0;
            settingsData.bloom = true;
            settingsData.motionBlur = false;
            settingsData.ambientOcclusion = true;
            
            SaveSettings();
            Debug.Log("No settings file found. Created default settings.");
        }
    }

    public void ApplySettings()
    {
        SaveSettings();
        Debug.Log("Graphics settings applied and saved!");
    }

    void OnApplicationQuit()
    {
        SaveSettings();
    }
}