using System;
using UnityEngine;

public class UpgradeBuildDamage : Upgrade
{
    [SerializeField] float amount = 1.5f;

    public override void OnInit() => UpgradeStat();
    public override void OnUpdate() => UpgradeStat();

    void UpgradeStat()
    {
        BuildingManager.Instance.extraBuildingDamage += amount;

        Building[] buildings = FindObjectsByType<Building>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (var build in buildings)
        {
            build.extraDamage = BuildingManager.Instance.extraBuildingDamage;
        }
    }
}
