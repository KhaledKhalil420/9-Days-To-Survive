using System.Collections.Generic;
using UnityEngine;

public class UpgradesTab : PopupTab
{
    [Header("Datas and Upgrades")]
    [SerializeField] List<UpgradeData> upgradesList;
    [SerializeField] int upgradesCount;

    [Header("Ui")]
    [SerializeField] Transform parent;
    [SerializeField] GameObject upgradeUI;

    void Start()
    {
        List<UpgradeData> datas = new(upgradesList);

        for (int i = 0; i < upgradesCount; i++)
        {
            UpgradeData selectedUpgrade = datas[Random.Range(0, datas.Count)];
            datas.Remove(selectedUpgrade);

            UpgradeUI ui = Instantiate(upgradeUI, parent).GetComponent<UpgradeUI>();

            ui.AttachedUpgrade = selectedUpgrade;
            ui.Setup();
        }
    }
}
