using UnityEngine;
using System.Collections.Generic;
using System;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    public List<Upgrade> ActiveUpgrades = new();

    //Events
    public static Action OnEnemyDeath;
    public static bool IncreaseHealthOnEnemyDeath = false;

    private void Awake()
    {
        Instance = this;
    }

    public static void GiveUpgrade(UpgradeData data)
    {
        foreach(Upgrade upg in Instance.ActiveUpgrades)
        {
            if(upg.data == data)
            {
                upg.Multiplier++;
                upg.OnUpdate();
                return;
            }
        }

        Upgrade upgrade = Instantiate(data.upgrade, GameManager.Player.transform).GetComponent<Upgrade>();
        upgrade.Setup(Instance);
    }

    void OnDestroy()
    {
        OnEnemyDeath = null;
    }
}
