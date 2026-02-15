using System.Collections.Generic;
using UnityEngine;

public class UpgradesTab : PopupTab
{
    [Header("Datas and Upgrades")]
    [SerializeField] private List<UpgradeData> upgradesList;
    [SerializeField] private int upgradesCount;

    [Header("Ui")]
    [SerializeField] private Transform parent;
    [SerializeField] private GameObject upgradeUI;

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

    public void PlayClosingSound()
    {
        AudioManager.Instance.PlaySound("Ui_Close");
    }
}
