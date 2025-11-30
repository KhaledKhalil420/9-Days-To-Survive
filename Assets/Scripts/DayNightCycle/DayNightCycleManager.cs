using System;
using UnityEngine;

public class DayNightCycleManager : MonoBehaviour
{
    public static DayNightCycleManager instance;
    public delegate void DayChangeArgs(bool state);
    public static event DayChangeArgs OnDayChange;

    public enum CycleState { Day, Night }

    public float dayDurationInMinutes = 3f;
    public float nightDurationInMinutes = 1f;
    public float timeScale = 1f;

    public Material skyboxMaterial;
    public Light mainLight;
    public float lightIntensity = 1f;
    [Range(0, 1)] public float blendSpeed = 0.1f;

    public CycleState currentState = CycleState.Day;

    float timeOfDay;

    void Start()
    {
        instance = this;

        if (skyboxMaterial == null) return;

        RenderSettings.skybox = skyboxMaterial;
        RenderSettings.skybox.SetFloat("_CubemapTransition", 1f);
    }

    void Update()
    {
        UpdateTimeOfDay();
        UpdateSkyboxBlend();
    }

    void UpdateTimeOfDay()
    {
        float phaseDuration = (currentState == CycleState.Day ? dayDurationInMinutes : nightDurationInMinutes) * 60f;
        timeOfDay += Time.deltaTime / phaseDuration * timeScale;

        if (timeOfDay >= 1f)
        {
            timeOfDay = 0f;
            currentState = currentState == CycleState.Day ? CycleState.Night : CycleState.Day;

            bool isDay = currentState == CycleState.Day;

            OnDayChange?.Invoke(isDay);

            DynamicMusic.InDanger(!isDay);
        }

        mainLight.transform.eulerAngles = new Vector3(45f, timeOfDay * 360f, -14f);
    }

    void UpdateSkyboxBlend()
    {
        float targetBlend = currentState == CycleState.Day ? 0f : 1f;
        float targetLighting = currentState == CycleState.Day ? lightIntensity : 0.1f;

        float currentBlend = skyboxMaterial.GetFloat("_CubemapTransition");

        skyboxMaterial.SetFloat("_CubemapTransition", Mathf.Lerp(currentBlend, targetBlend, blendSpeed * Time.deltaTime));
        RenderSettings.ambientIntensity = Mathf.Lerp(RenderSettings.ambientIntensity, targetLighting, blendSpeed * Time.deltaTime);
        mainLight.intensity = Mathf.Lerp(mainLight.intensity, targetLighting, blendSpeed * Time.deltaTime);
    }
}
