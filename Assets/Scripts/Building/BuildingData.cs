using UnityEngine;

public enum BuildType {Structure, Weapons, Unlimited}

[CreateAssetMenu]
public class BuildingData : ScriptableObject
{
    public Sprite sprite;
    public string buildingName;
    public string buildingDescription;
    public float pointsWorth;
}
