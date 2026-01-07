using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BaseSlot : MonoBehaviour
{
    public Item HeldItem;
    public int HeldQuantity;
    internal InventoryHolder heldBy;

    [SerializeField] internal Image _itemIconImage;
    [SerializeField] internal Image _slotBorderImage;
    [SerializeField] internal TMP_Text _itemQuantityText;
    [SerializeField] internal Sprite _empty;
    [SerializeField] internal Color unselected, selected;

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
            HeldItem.HeldQuantity = HeldQuantity;
        }

        _itemQuantityText.text = HeldItem != null ? HeldQuantity.ToString() : "";
        _itemQuantityText.text = HeldQuantity > 1 ? HeldQuantity.ToString() : "";

        _itemIconImage.sprite = HeldItem != null ? HeldItem.data.sprite : _empty;

        Visuals();
        OnUpdateSlot();
    }

    public virtual void Visuals()
    {
        
    }

    public virtual void OnUpdateSlot()
    {
        
    }

    public void CreateItem(ItemData data)
    {
        Item item = Instantiate(data.prefab).GetComponent<Item>();
        item.heldby = heldBy.parent.gameObject;
        item.SetItemParent(heldBy.hand);
        item.HeldQuantity = HeldQuantity;

        _itemQuantityText.text = HeldItem != null ? HeldQuantity.ToString() : "";
        _itemQuantityText.text = HeldQuantity > 1 ? HeldQuantity.ToString() : "";

        _itemIconImage.sprite = HeldItem != null ? HeldItem.data.sprite : _empty;
        HeldItem = item;
    }

    public void ResetSlot()
    {
        HeldItem.isSelected = false;
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