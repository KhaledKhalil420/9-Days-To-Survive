using System;
using UnityEngine;

public class UpgradePlayerSpeed : Upgrade
{
    public override void OnInit() => UpgradeStat();

    public override void OnUpdate() => UpgradeStat();

    private void UpgradeStat()
    {
        Player.inventory.speedBonus += 0.25f;
    }
}
