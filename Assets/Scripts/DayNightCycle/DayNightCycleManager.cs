using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using EZCameraShake;

[System.Serializable]
public class DayCycleEntry
{
    public int dayToPlayIn = 0;
    public DayCyclePreset dayPreset;
    public DayCyclePreset nightPreset;
}

public class DayNightCycleManager : MonoBehaviour
{
    public static DayNightCycleManager Instance;
    public int DayCount = 0;
    public delegate void DayChangeArgs(bool state);
    public event DayChangeArgs OnDayChange;

    public enum CycleState { Day, Night }

    public Material skyboxMaterial;
    public Light mainLight;
    public float lightIntensity = 1f;
    [Range(0, 1)] public float blendSpeed = 0.1f;
    public CycleState currentState = CycleState.Day;

    [SerializeField] private List<DayCycleEntry> presetCycles;
    [SerializeField] private DayCyclePreset fallbackDayPreset;
    [SerializeField] private DayCyclePreset fallbackNightPreset;

    // Cached — resolved once per day change instead of randomly re-evaluated every frame
    public DayCyclePreset dayPreset   { get; private set; }
    public DayCyclePreset nightPreset { get; private set; }

    [Header("Feel")]
    public Volume volume;
    public AudioSource bassDropSource;
    public AudioSource source;
    public Image triggeringProgress;
    public float holdTime;
    private float holdTimer;

    private void Awake()
    {
        Instance = this;
        DayCount = 0;
        currentState = CycleState.Day;

        if (skyboxMaterial == null) return;
        RenderSettings.skybox = skyboxMaterial;
        RenderSettings.skybox.SetFloat("_CubemapTransition", 1f);
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    private void Start()
    {
        StartCoroutine(nameof(LateStart));
    }

    private IEnumerator LateStart()
    {
        yield return new WaitForEndOfFrame();
        ResolvePresetsForDay(DayCount);
        OnDayChange?.Invoke(true);
    }

    private void Update()
    {
        triggeringProgress.fillAmount = Mathf.Lerp(triggeringProgress.fillAmount, holdTimer / holdTime, 50 * Time.deltaTime);
        source.volume = Mathf.Lerp(source.volume, holdTimer / holdTime, 25f * Time.deltaTime);
        source.pitch = Mathf.Lerp(source.pitch, 1f + (holdTimer / holdTime) / 1.5f, 25f * Time.deltaTime);
        volume.weight = Mathf.Lerp(volume.weight, holdTimer / holdTime, 5 * Time.deltaTime);

        if (Input.GetKey(KeyCode.G) && currentState == CycleState.Day)
        {
            holdTimer += Time.deltaTime;

            if (holdTimer >= holdTime)
            {
                CameraShaker.Instance.ShakeOnce(5, 2, 0.2f, 1);
                holdTimer = 0;
                bassDropSource.Play();
                SetTime(currentState == CycleState.Day ? CycleState.Night : CycleState.Day);
            }
        }
        else
        {
            holdTimer = Mathf.Max(0, holdTimer - Time.deltaTime * 4);
        }

        UpdateSkyboxBlend();
    }

    public static void SetTime(CycleState cycleState)
    {
        Instance.currentState = cycleState;
        bool isDay = cycleState == CycleState.Day;
        Instance.DayCount += isDay ? 1 : 0;

        // Resolve and cache presets once now that DayCount may have changed
        Instance.ResolvePresetsForDay(Instance.DayCount);

        Instance.OnDayChange?.Invoke(isDay);

        if(cycleState == CycleState.Night)
        {
            WorldGenerator.RequestNavMeshRebake();
        }
    }

    private void ResolvePresetsForDay(int day)
    {
        DayCycleEntry match = presetCycles?.Find(c => c.dayToPlayIn == day);
        if (match == null && presetCycles?.Count > 0)
            match = presetCycles[presetCycles.Count - 1];

        dayPreset   = match?.dayPreset   ?? fallbackDayPreset;
        nightPreset = match?.nightPreset ?? fallbackNightPreset;
    }

    private void UpdateSkyboxBlend()
    {
        DayCyclePreset current = currentState == CycleState.Day ? dayPreset : nightPreset;
        if (current == null) return;

        float targetBlend = currentState == CycleState.Day ? 0f : 1f;
        float currentBlend = skyboxMaterial.GetFloat("_CubemapTransition");
        float delta = blendSpeed * Time.deltaTime;

        RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, current.fogColor, delta);
        RenderSettings.reflectionIntensity = Mathf.Lerp(RenderSettings.reflectionIntensity, current.reflectionIntensity, delta);
        RenderSettings.ambientIntensity = Mathf.Lerp(RenderSettings.ambientIntensity, current.ambientIntensity, delta);
        RenderSettings.ambientSkyColor = Color.Lerp(RenderSettings.ambientSkyColor, current.skyColor, delta);
        RenderSettings.ambientEquatorColor = Color.Lerp(RenderSettings.ambientEquatorColor, current.equatorColor, delta);
        RenderSettings.ambientGroundColor = Color.Lerp(RenderSettings.ambientGroundColor, current.groundColor, delta);

        if (current.useFogDensity) RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, current.fogDensity, delta);
        if (current.useSubtractiveShadowColor) RenderSettings.subtractiveShadowColor = Color.Lerp(RenderSettings.subtractiveShadowColor, current.subtractiveShadowColor, delta);

        skyboxMaterial.SetFloat("_CubemapTransition", Mathf.Lerp(currentBlend, targetBlend, delta));
        mainLight.intensity = Mathf.Lerp(mainLight.intensity, current.lightIntensity, delta);
        mainLight.shadowStrength = Mathf.Lerp(mainLight.shadowStrength, current.shadowStrength, delta);
        if (current.useLightColor) mainLight.color = Color.Lerp(mainLight.color, current.lightColor, delta);
    }
}