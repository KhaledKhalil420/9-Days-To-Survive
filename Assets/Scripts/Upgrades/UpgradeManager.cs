using UnityEngine;
using System.Collections.Generic;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    public List<Upgrade> ActiveUpgrades = new();

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
}
