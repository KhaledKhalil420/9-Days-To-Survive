using System;
using UnityEngine;

public enum BuildType {Structure, Weapons, Unlimited}

[CreateAssetMenu]
public class BuildingData : ScriptableObject
{
    public Sprite sprite;
    public string buildingName;
    public string buildingDescription;
    public float pointsWorth;

    [Header("Pivoting data")]
    public bool requireSnapping = false;
    public bool usesPivots = true;
    public bool affectedByGridSizePosition = true;

    [Header("Ingrediets")]
    public Ingredient[] ingredients = Array.Empty<Ingredient>();
    public bool dropResourcesOnDestory = false;
}
