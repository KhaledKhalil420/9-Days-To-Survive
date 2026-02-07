using System;
using UnityEngine;

public class UpgradeStats : Upgrade
{
    [Flags] public enum UpgradeStatSelection{Health = 1 << 0, Stamina = 1 << 1, Hunger = 1 << 2}

    [SerializeField] UpgradeStatSelection selection;
    [SerializeField] float amount = 1.25f;

    public override void OnInit() => UpgradeStat();
    public override void OnUpdate() => UpgradeStat();

    void UpgradeStat()
    {
        if ((selection & UpgradeStatSelection.Health) != 0)
        {
            Player.stats.health.max *= amount;
            Player.stats.health.current = Player.stats.health.max;
            Player.stats.health.modifyRate *= amount;
        }

        if ((selection & UpgradeStatSelection.Stamina) != 0)
        {
            Player.stats.stamina.max *= amount;
            Player.stats.stamina.current = Player.stats.stamina.max;
            Player.stats.stamina.modifyRate *= amount;
        }

        if ((selection & UpgradeStatSelection.Hunger) != 0)
        {
            Player.stats.hunger.max *= amount;
            Player.stats.hunger.current = Player.stats.hunger.max;
            Player.stats.hunger.modifyRate *= amount;
        }
    }
}
