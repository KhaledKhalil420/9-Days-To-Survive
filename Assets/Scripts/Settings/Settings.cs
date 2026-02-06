// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Reflection;
// using UnityEngine;
// using UnityEngine.UI;
// using UnityEngine.Audio;
// using TMPro;
// using DG.Tweening;
// using UnityEngine.EventSystems;

// [Serializable]
// public class SaveSettingsData
// {
//     public float Sensitivity = 1f;
//     public float RenderDistance = 100f;
//     public int QualityLevel = 2;
//     public int ResolutionIndex = 0;
//     public float MusicVolume = 0f;
//     public float SfxVolume = 0f;
//     public float MasterVolume = 0f;
//     public float AmbienceVolume = 0f;
//     public bool VSync = true;
//     public List<KeyEntry> KeyBindings = new();
// }

// public class Settings : MonoBehaviour
// {
//     private const string SaveFileName = "settings.json";

//     [Header("Player")]
//     [SerializeField] private Camera mainCamera;
//     private PlayerLook playerLook;

//     [Header("UI Elements")]
//     [SerializeField] private Slider sensitivitySlider;
//     [SerializeField] private Slider renderDistanceSlider;
//     [SerializeField] private Slider musicSlider;
//     [SerializeField] private Slider sfxSlider;
//     [SerializeField] private Slider masterSlider;
//     [SerializeField] private Slider ambienceSlider;
//     [SerializeField] private TMP_Dropdown graphicsDropdown;
//     [SerializeField] private TMP_Dropdown resolutionDropdown;
//     [SerializeField] private Toggle vsyncToggle;
//     [SerializeField] private Transform keybindsParent;
//     [SerializeField] private GameObject keybindPrefab;

//     [Header("Audio Mixers")]
//     [SerializeField] private AudioMixer masterMixer;

//     private List<Resolution> validResolutions = new();
//     private SaveSettingsData settingsData = new();
//     private string fullPath => Path.Combine(Application.persistentDataPath, SaveFileName);
//     private readonly List<TMP_Text> keybindButtons = new();
//     private int waitingForKeyIndex = -1;

//     private void Awake()
//     {
//         if (mainCamera == null) mainCamera = Camera.main;
//         playerLook = mainCamera?.GetComponentInParent<PlayerLook>();

//         PopulateResolutions();
//         LoadSettings();
//         SetupKeybindUI();
//         AnimateAllUI();
//     }

//     private void OnEnable() => SubscribeUI();
//     private void OnDisable() => UnsubscribeUI();

//     private void SubscribeUI()
//     {
//         sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
//         renderDistanceSlider.onValueChanged.AddListener(OnRenderDistanceChanged);
//         graphicsDropdown.onValueChanged.AddListener(OnQualityChanged);
//         resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
//         musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
//         sfxSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
//         masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
//         ambienceSlider.onValueChanged.AddListener(OnAmbienceVolumeChanged);
//         vsyncToggle.onValueChanged.AddListener(OnVSyncChanged);
//     }

//     private void UnsubscribeUI()
//     {
//         sensitivitySlider.onValueChanged.RemoveListener(OnSensitivityChanged);
//         renderDistanceSlider.onValueChanged.RemoveListener(OnRenderDistanceChanged);
//         graphicsDropdown.onValueChanged.RemoveListener(OnQualityChanged);
//         resolutionDropdown.onValueChanged.RemoveListener(OnResolutionChanged);
//         musicSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
//         sfxSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
//         masterSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
//         ambienceSlider.onValueChanged.RemoveListener(OnAmbienceVolumeChanged);
//         vsyncToggle.onValueChanged.RemoveListener(OnVSyncChanged);
//     }

//     private void PopulateResolutions()
//     {
//         resolutionDropdown.ClearOptions();
//         validResolutions.Clear();
//         float targetAspect = 16f / 9f;

//         foreach (var res in Screen.resolutions)
//             if (res.width >= 640 && res.height >= 360 &&
//                 Mathf.Approximately((float)res.width / res.height, targetAspect) &&
//                 !validResolutions.Exists(r => r.width == res.width && r.height == res.height))
//                 validResolutions.Add(res);

//         var options = validResolutions.ConvertAll(r => $"{r.width} x {r.height}");
//         resolutionDropdown.AddOptions(options);
//     }

//     private void OnSensitivityChanged(float v)   { settingsData.Sensitivity = v; if (playerLook != null) playerLook.Sensitivity = v; SaveSettings(); }
//     private void OnRenderDistanceChanged(float v){ settingsData.RenderDistance = v; if (mainCamera != null) mainCamera.farClipPlane = v; SaveSettings(); }
//     private void OnQualityChanged(int i)         { settingsData.QualityLevel = i; QualitySettings.SetQualityLevel(i, true); SaveSettings(); }
//     private void OnResolutionChanged(int i)
//     {
//         settingsData.ResolutionIndex = i;
//         if (i >= 0 && i < validResolutions.Count)
//         {
//             var r = validResolutions[i];
//             Screen.SetResolution(r.width, r.height, FullScreenMode.FullScreenWindow, r.refreshRateRatio);
//         }
//         SaveSettings();
//     }
//     private void OnMusicVolumeChanged(float v)   { settingsData.MusicVolume = v; masterMixer.SetFloat("VolumeMusic", v); SaveSettings(); }
//     private void OnSfxVolumeChanged(float v)     { settingsData.SfxVolume = v; masterMixer.SetFloat("VolumeSfx", v); SaveSettings(); }
//     private void OnMasterVolumeChanged(float v)  { settingsData.MasterVolume = v; masterMixer.SetFloat("VolumeMaster", v); SaveSettings(); }
//     private void OnAmbienceVolumeChanged(float v){ settingsData.AmbienceVolume = v; masterMixer.SetFloat("VolumeAmbience", v); SaveSettings(); }
//     private void OnVSyncChanged(bool on)        { settingsData.VSync = on; QualitySettings.vSyncCount = on ? 1 : 0; SaveSettings(); }

//     private void WaitForKey(int idx)
//     {
//         waitingForKeyIndex = idx;
//         keybindButtons[idx].text = "Press a key...";
//     }

//     private void OnGUI()
//     {
//         if (waitingForKeyIndex < 0) return;
//         var e = Event.current;

//         if (e.isKey && e.type == EventType.KeyDown)
//         {
//             string action = ConfigActions[waitingForKeyIndex];
//             Keybinds.Set(action, e.keyCode);
//             keybindButtons[waitingForKeyIndex].text = $"{action}: {e.keyCode}";
//             settingsData.KeyBindings = SerializeKeybinds();
//             SaveSettings();
//             waitingForKeyIndex = -1;
//         }
//         else if (e.isMouse && e.type == EventType.MouseDown)
//         {
//             string action = ConfigActions[waitingForKeyIndex];
//             KeyCode mouseKey = MouseButtonToKeyCode(e.button);
//             Keybinds.Set(action, mouseKey);
//             keybindButtons[waitingForKeyIndex].text = $"{action}: {mouseKey}";
//             settingsData.KeyBindings = SerializeKeybinds();
//             SaveSettings();
//             waitingForKeyIndex = -1;
//         }
//     }

//     private KeyCode MouseButtonToKeyCode(int button)
//     {
//         return button switch
//         {
//             0 => KeyCode.Mouse0,
//             1 => KeyCode.Mouse1,
//             2 => KeyCode.Mouse2,
//             3 => KeyCode.Mouse3,
//             4 => KeyCode.Mouse4,
//             5 => KeyCode.Mouse5,
//             6 => KeyCode.Mouse6,
//             _ => KeyCode.None,
//         };
//     }

//     private List<KeyEntry> SerializeKeybinds()
//     {
//         var list = new List<KeyEntry>();
//         var map = typeof(Keybinds).GetField("keyMap", BindingFlags.NonPublic | BindingFlags.Static)
//                                   .GetValue(null) as Dictionary<string, KeyCode>;
//         foreach (var kv in map)
//             list.Add(new KeyEntry { Action = kv.Key, Key = kv.Value });
//         return list;
//     }

//     private void SaveSettings()
//     {
//         try { File.WriteAllText(fullPath, JsonUtility.ToJson(settingsData, true)); }
//         catch (Exception e) { Debug.LogError($"Failed to save settings: {e.Message}"); }
//     }

//     private void LoadSettings()
//     {
//         if (File.Exists(fullPath))
//         {
//             try { settingsData = JsonUtility.FromJson<SaveSettingsData>(File.ReadAllText(fullPath)); }
//             catch { settingsData = new SaveSettingsData(); }
//         }

//         if (settingsData.KeyBindings.Count > 0)
//             foreach (var entry in settingsData.KeyBindings)
//                 Keybinds.Set(entry.Action, entry.Key);

//         ApplySettingsToUI();
//     }

//     private void ApplySettingsToUI()
//     {
//         sensitivitySlider.value       = settingsData.Sensitivity;
//         renderDistanceSlider.value    = settingsData.RenderDistance;
//         graphicsDropdown.value        = settingsData.QualityLevel;
//         resolutionDropdown.value      = settingsData.ResolutionIndex;
//         musicSlider.value             = settingsData.MusicVolume;
//         sfxSlider.value               = settingsData.SfxVolume;
//         masterSlider.value            = settingsData.MasterVolume;
//         ambienceSlider.value          = settingsData.AmbienceVolume;
//         vsyncToggle.isOn              = settingsData.VSync;

//         OnSensitivityChanged(settingsData.Sensitivity);
//         OnRenderDistanceChanged(settingsData.RenderDistance);
//         OnQualityChanged(settingsData.QualityLevel);
//         OnResolutionChanged(settingsData.ResolutionIndex);
//         OnMusicVolumeChanged(settingsData.MusicVolume);
//         OnSfxVolumeChanged(settingsData.SfxVolume);
//         OnMasterVolumeChanged(settingsData.MasterVolume);
//         OnAmbienceVolumeChanged(settingsData.AmbienceVolume);
//         OnVSyncChanged(settingsData.VSync);
//     }

//     private void SetupKeybindUI()
//     {
//         keybindButtons.Clear();
//         foreach (Transform c in keybindsParent) Destroy(c.gameObject);

//         for (int i = 0; i < ConfigActions.Count; i++)
//         {
//             var go = Instantiate(keybindPrefab, keybindsParent);
//             var txt = go.GetComponentInChildren<TMP_Text>();
//             keybindButtons.Add(txt);

//             string action = ConfigActions[i];
//             var key = Keybinds.Key(action);
//             txt.text = $"{action}: {key}";

//             var trig = go.GetComponent<EventTrigger>() ?? go.AddComponent<EventTrigger>();
//             trig.triggers ??= new List<EventTrigger.Entry>();
//             int idx = i;
//             var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
//             entry.callback.AddListener(_ => WaitForKey(idx));
//             trig.triggers.Add(entry);
//         }
//     }

//     private List<string> ConfigActions
//     {
//         get
//         {
//             var cfg = Resources.Load<KeybindsConfig>("KeybindsConfig");
//             if (cfg == null)
//             {
//                 Debug.LogError("KeybindsConfig asset not found!");
//                 return new List<string>();
//             }
//             var actions = new List<string>();
//             foreach (var e in cfg.keyEntries) actions.Add(e.Action);
//             return actions;
//         }
//     }

//     public void ExitGame() => Application.Quit();

//     private void AnimateAllUI()
//     {
//         Animate(sensitivitySlider.GetComponent<RectTransform>());
//         Animate(renderDistanceSlider.GetComponent<RectTransform>());
//         Animate(musicSlider.GetComponent<RectTransform>());
//         Animate(sfxSlider.GetComponent<RectTransform>());
//         Animate(masterSlider.GetComponent<RectTransform>());
//         Animate(ambienceSlider.GetComponent<RectTransform>());
//         Animate(graphicsDropdown.GetComponent<RectTransform>());
//         Animate(resolutionDropdown.GetComponent<RectTransform>());
//         Animate(vsyncToggle.GetComponent<RectTransform>());
//     }

//     private void Animate(RectTransform t)
//     {
//         if (t == null) return;
//         float y0 = t.anchoredPosition.y;
//         t.DOAnchorPosY(y0 + 3f, 3f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine)
//          .SetDelay(UnityEngine.Random.Range(0f,1f)).SetUpdate(true);

//         var trig = t.gameObject.GetComponent<EventTrigger>() ?? t.gameObject.AddComponent<EventTrigger>();
//         trig.triggers ??= new List<EventTrigger.Entry>();
//         AddTrigger(trig, EventTriggerType.PointerEnter, () => { });
//         AddTrigger(trig, EventTriggerType.PointerClick, () =>
//         {
//             AudioManager.instance?.PlaySound("Click",1,1.15f);
//             t.DOPunchScale(Vector3.one * .05f, .2f, 3, .4f).SetUpdate(true);
//         });
//     }

//     private void AddTrigger(EventTrigger tg, EventTriggerType type, Action cb)
//     {
//         var ent = new EventTrigger.Entry { eventID = type };
//         ent.callback.AddListener(_ => cb());
//         tg.triggers.Add(ent);
//     }
// }
