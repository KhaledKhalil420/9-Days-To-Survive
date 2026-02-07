using UnityEngine;
using UnityEngine.EventSystems;

public class UpgradeUI : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private UpgradeData attachedUpgrade;
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if(GameManager.StoredPoints >= attachedUpgrade.price)
        {
            GameManager.StoredPoints -= attachedUpgrade.price;
            UpgradeManager.GiveUpgrade(attachedUpgrade);
        }
        
        Destroy(gameObject);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
    }

    public void OnPointerExit(PointerEventData eventData)
    {
    }
}
