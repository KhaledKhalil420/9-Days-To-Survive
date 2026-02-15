using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeUI : MonoBehaviour, IPointerDownHandler
{
    public UpgradeData AttachedUpgrade;
    [SerializeField] private UnityEvent Event;
    [SerializeField] private TMP_Text textName, textPrice, textDiscription;
    [SerializeField] private Image imageIcon;

    private Vector3 initSize;

    public void Setup()
    {
        textName.text = AttachedUpgrade.fullName;
        textPrice.text = AttachedUpgrade.price.ToString() + "POINTS";
        textDiscription.text = AttachedUpgrade.discription;
        imageIcon.sprite = AttachedUpgrade.sprite;

        initSize = transform.localScale;
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if(GameManager.StoredPoints >= AttachedUpgrade.price)
        {
            AudioManager.Instance.PlaySound("Ui_Click");
            GameManager.StoredPoints -= AttachedUpgrade.price;
            UpgradeManager.GiveUpgrade(AttachedUpgrade);
            Event?.Invoke();
            Destroy(gameObject);
        }
        
    }
}
