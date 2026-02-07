using Sortify;
using UnityEngine;

public class Upgrade : MonoBehaviour
{
    [Header("References")]
    [ReadOnly] public UpgradeManager UpgradeManager;
    [ReadOnly] public UpgradeData data;

    [Header("Logic")]
    public int Multiplier = 1;

    public void Setup(UpgradeManager upgradeManager)
    {
        UpgradeManager = upgradeManager;

        OnInit();
    }

    public virtual void OnInit(){}
    public virtual void OnUpdate(){}

}
