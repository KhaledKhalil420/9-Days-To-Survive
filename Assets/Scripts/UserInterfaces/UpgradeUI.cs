using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeUI : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    public UpgradeData AttachedUpgrade;
    [SerializeField] private UnityEvent Event;
    [SerializeField] private TMP_Text textName, textPrice, textDiscription;
    [SerializeField] private Image imageIcon;

    private Vector3 initSize;

    public void Setup()
    {
        textName.text = AttachedUpgrade.fullName;
        textPrice.text = AttachedUpgrade.price.ToString();
        textDiscription.text = AttachedUpgrade.discription;

        initSize = transform.localScale;
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if(GameManager.StoredPoints >= AttachedUpgrade.price)
        {
            GameManager.StoredPoints -= AttachedUpgrade.price;
            UpgradeManager.GiveUpgrade(AttachedUpgrade);
            Event?.Invoke();
            Destroy(gameObject);
        }
        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOScale(initSize * 1.05f, 0.5f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOScale(initSize, 0.5f);
    }
}
