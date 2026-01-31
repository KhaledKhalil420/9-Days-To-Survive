using UnityEngine;

[CreateAssetMenu(fileName = "LightingPreset", menuName = "Lighting/Preset")]
public class LightingPreset : ScriptableObject
{
    [Header("Environment")]
    [ColorUsage(true, true)] public Color skyColor = Color.white;
    [ColorUsage(true, true)] public Color equatorColor = Color.gray;
    [ColorUsage(true, true)] public Color groundColor = Color.black;
    public float ambientIntensity = 1f;
    public float reflectionIntensity = 1f;

    [Header("Fog")]
    [ColorUsage(true, true)] public Color fogColor = Color.gray;
    public bool useFogDensity;
    [Range(0, 1)] public float fogDensity = 0.01f;

    [Header("Directional Light")]
    [Range(0, 8)] public float lightIntensity = 1f;
    [Range(0, 1)] public float shadowStrength = 1f;
    public bool useLightColor;
    [ColorUsage(false, true)] public Color lightColor = Color.white;

    [Header("Optional")]
    public bool useSubtractiveShadowColor;
    [ColorUsage(false, true)] public Color subtractiveShadowColor = new Color(0.42f, 0.48f, 0.63f);
}