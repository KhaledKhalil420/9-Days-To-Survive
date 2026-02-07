using System;
using UnityEngine;

public class UpgradeStats : Upgrade
{
    [Flags] public enum UpgradeStatSelection {Health, Stamina, Hunger}
    [SerializeField] private UpgradeStatSelection selection;
    [SerializeField] private float amount = 1.25f;
    
    public override void OnInit()
    {
        UpgradeStat();
    }

    public override void OnUpdate()
    {
        UpgradeStat();
    }

    public void UpgradeStat()
    {
        switch (selection)
        {
            case UpgradeStatSelection.Health:
            Player.stats.health.max *= amount;
            Player.stats.health.current = Player.stats.health.max;
            Player.stats.health.modifyRate *= amount;
            break;

            case UpgradeStatSelection.Stamina: 
            Player.stats.hunger.max *= amount;
            Player.stats.hunger.current = Player.stats.hunger.max;
            Player.stats.hunger.modifyRate *= amount;
            break;

            case UpgradeStatSelection.Hunger: 
            Player.stats.stamina.max *= amount;
            Player.stats.stamina.current = Player.stats.stamina.max;
            Player.stats.stamina.modifyRate *= amount;
            break;

        }   
    }
}
