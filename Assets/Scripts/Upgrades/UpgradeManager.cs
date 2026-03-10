using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    public List<Upgrade> ActiveUpgrades = new();
    public Transform BagCanvas;
    public UpgradeUiElement UpgradeImage;
    public List<UpgradeUiElement> UiElements;

    //Events
    public Action OnEnemyDeath;
    public bool IncreaseHealthOnEnemyDeath = false;
    public float IncreaseHealthOnEnemyDeathMultiplier = 1;

    private void Awake()
    {
        Instance = this;
    }

    public static void GiveUpgrade(UpgradeData data)
    {
        foreach(Upgrade upg in Instance.ActiveUpgrades)
        {
            if(upg.data.Name == data.Name)
            {
                upg.Multiplier++;
                upg.OnUpdate();
                UpgradeUi(data, false, upg.Multiplier);
                return;
            }
        }

        Upgrade upgrade = Instantiate(data.upgrade, GameManager.Player.transform).GetComponent<Upgrade>();
        Instance.ActiveUpgrades.Add(upgrade);
        UpgradeUi(data, true, 1);
        upgrade.Setup(Instance);
    }

    public static void UpgradeUi(UpgradeData data, bool spawn, int multiplier)
    {
        if (spawn)
        {
            UpgradeUiElement ui = Instantiate(Instance.UpgradeImage.gameObject, Instance.BagCanvas).GetComponent<UpgradeUiElement>();
            ui.Data = data;
            ui.Multiplier = multiplier;
            ui.UpdateUi();
            Instance.UiElements.Add(ui);
            LayoutRebuilder.ForceRebuildLayoutImmediate(Instance.BagCanvas.GetComponent<RectTransform>());
        }

        else
        {
            foreach (var upgUI in Instance.UiElements)
            {
                if(upgUI.Data.Name == data.Name)
                {
                    upgUI.Multiplier = multiplier;
                    upgUI.UpdateUi();
                }
            }   
        }
    }

    void OnDestroy()
    {
        OnEnemyDeath = null;
    }
}
