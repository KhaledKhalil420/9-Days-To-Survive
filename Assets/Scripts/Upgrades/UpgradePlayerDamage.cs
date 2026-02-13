using System;
using UnityEngine;

public class UpgradePlayerDamage : Upgrade
{
    public override void OnInit() => UpgradeStat();

    public override void OnUpdate() => UpgradeStat();

    private void UpgradeStat()
    {
        Player.inventory.damageBonus += 0.25f;
    }
}
