using System;
using UnityEngine;
using System.Collections;

public class DayNightCycleManager : MonoBehaviour
{
    public static DayNightCycleManager Instance;
    public static int DayCount = 0;
    public delegate void DayChangeArgs(bool state);
    public static event DayChangeArgs OnDayChange;
    public enum CycleState { Day, Night }

    public Material skyboxMaterial;
    public Light mainLight;
    public float lightIntensity = 1f;
    [Range(0, 1)] public float blendSpeed = 0.1f;

    [ColorUsage(true, true)]
    public Color fogDay, fogNight;
    public CycleState currentState = CycleState.Day;

    void Awake()
    {
        Instance = this;

        if (skyboxMaterial == null) 
            return;

        RenderSettings.skybox = skyboxMaterial;
        RenderSettings.skybox.SetFloat("_CubemapTransition", 1f);
    }

    private void Start()
    {
        StartCoroutine(nameof(LateStart));
    }

    private IEnumerator LateStart()
    {
        yield return new WaitForEndOfFrame();
        OnDayChange?.Invoke(true);
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.G))
            SetTime(currentState == CycleState.Day ? CycleState.Night : CycleState.Day);
        
        UpdateSkyboxBlend();
    }

    public static void SetTime(CycleState cycleState)
    {
        Instance.currentState = cycleState;
        bool isDay = cycleState == CycleState.Day;
        
        DayCount += isDay ? 1 : 0;
        
        OnDayChange?.Invoke(isDay);

    }

    void UpdateSkyboxBlend()
    {
        float targetBlend = currentState == CycleState.Day ? 0f : 1f;        
        float targetLighting = currentState == CycleState.Day ? lightIntensity : 0.01f;
        float targetShadows = currentState == CycleState.Day ? 1 : 0f;

        float currentBlend = skyboxMaterial.GetFloat("_CubemapTransition");
        
        RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, currentState == CycleState.Day ? fogDay : fogNight, blendSpeed * Time.deltaTime);
        RenderSettings.reflectionIntensity = currentState == CycleState.Day ? 0.325f : 0;
        RenderSettings.ambientIntensity = Mathf.Lerp(RenderSettings.ambientIntensity, targetLighting, blendSpeed * Time.deltaTime);

        skyboxMaterial.SetFloat("_CubemapTransition", Mathf.Lerp(currentBlend, targetBlend, blendSpeed * Time.deltaTime));
        mainLight.intensity = Mathf.Lerp(mainLight.intensity, targetLighting, blendSpeed * Time.deltaTime);
        mainLight.shadowStrength = Mathf.Lerp(mainLight.shadowStrength, targetShadows, blendSpeed * Time.deltaTime);
    }
}
