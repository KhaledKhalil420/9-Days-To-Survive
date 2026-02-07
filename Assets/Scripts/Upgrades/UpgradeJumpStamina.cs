using System;
using UnityEngine;

public class UpgradeJumpStamina : Upgrade
{
    public override void OnInit() => UpgradeStat();

    void UpgradeStat()
    {
        Player.stats.jumpingConsumingStamina = false;
    }
}
