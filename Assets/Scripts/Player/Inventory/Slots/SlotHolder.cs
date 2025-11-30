using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlotHolder : MonoBehaviour
{
    public Item HeldItem;
    public int HeldQuantity;
    public bool isSelected;
    
    private bool wasSelected;

    [SerializeField] private Image _itemIconImage;
    [SerializeField] private Image _slotBorderImage;
    [SerializeField] private TMP_Text _itemQuantityText;
    [SerializeField] private Sprite _empty;
    [SerializeField] private Color unselected, selected;

    private void Visuals()
    {
        if(isSelected)
        {
            transform.DOLocalRotate(new Vector3(0, 0, -2), 1);
            transform.DOScale(new Vector3(1.15f, 1.15f, 1.15f), 1);
        }
        else
        {
            transform.DOLocalRotate(new Vector3(0, 0, 0), 1);
            transform.DOScale(new Vector3(1, 1, 1), 1);
        }
    }

    public void UpdateSlot()
    {
        if (HeldQuantity <= 0 && HeldItem != null)
        {
            _itemQuantityText.text = "";
            HeldItem.OnChangingItems();
            Destroy(HeldItem.gameObject);
            HeldItem = null;
        }

        if (HeldItem != null)
        {
            if (isSelected && !wasSelected)
            {
                HeldItem.OnSelect();
                HeldItem.OnSelectOnce(); // I want it to update once unlike the one above that keeps updating
            }
            
            HeldItem.gameObject.SetActive(isSelected);
        }
        
        wasSelected = isSelected;

        _slotBorderImage.color = Color.Lerp(_slotBorderImage.color, isSelected ? selected : unselected, Time.deltaTime * 10f);

        _itemQuantityText.text = HeldItem != null ? HeldQuantity.ToString() : "";
        _itemQuantityText.text = HeldQuantity > 1 ? HeldQuantity.ToString() : "";

        _itemIconImage.sprite = HeldItem != null ? HeldItem.data.sprite : _empty;

        Visuals();
    }

    public void ResetSlot()
    {
        HeldQuantity = 0;
        HeldItem = null;
        UpdateSlot();
    }

    public void RemoveSlotSprite()
    {
        _itemIconImage.sprite = _empty;
    }

    public void ResetSlotSprite()
    {
        _itemIconImage.sprite = HeldItem.data.sprite;
    }
}