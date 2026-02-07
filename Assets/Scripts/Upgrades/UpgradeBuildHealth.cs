using System;
using UnityEngine;

public class UpgradeBuildHealth : Upgrade
{
    [SerializeField] float amount = 1.15f;

    public override void OnInit() => UpgradeStat();
    public override void OnUpdate() => UpgradeStat();

    void UpgradeStat()
    {
        BuildingManager.Instance.extraBuildingHealth *= amount;
    }
}
