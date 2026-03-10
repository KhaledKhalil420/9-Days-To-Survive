using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeUiElement : MonoBehaviour
{
    public UpgradeData Data;
    public int Multiplier;
    [SerializeField] private Image image;
    [SerializeField] private TMP_Text text;

    public void UpdateUi()
    {
        image.sprite = Data.sprite;
        text.text = Multiplier > 1 ? Multiplier.ToString() : "";
    }
    
}