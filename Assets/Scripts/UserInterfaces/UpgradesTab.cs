using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UpgradesTab : PopupTab
{
    [Header("Datas and Upgrades")]
    [SerializeField] private List<UpgradeData> upgradesList;
    [SerializeField] private int upgradesCount;

    [Header("Ui")]
    [SerializeField] private Transform parent;
    [SerializeField] private GameObject upgradeUI;
    [SerializeField] private TMP_Text score_Text;

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
        
        score_Text.text = PointsManager.Instance.StoredPoints.ToString();
        PointsManager.Instance.onPointsChanged += UpdatePoints;
    }

    void OnDestroy()
    {
        PointsManager.Instance.onPointsChanged -= UpdatePoints;
    }

    private void UpdatePoints()
    {
        score_Text.text = PointsManager.Instance.StoredPoints.ToString();
    }

    public void PlayClosingSound()
    {
        AudioManager.Instance.PlaySound("Ui_Close");
    }
}
