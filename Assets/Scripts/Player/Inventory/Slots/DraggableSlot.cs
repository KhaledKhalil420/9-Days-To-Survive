using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(BaseSlot))]
public class DraggableSlot : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    private BaseSlot slot;

    void Awake() => slot = GetComponent<BaseSlot>();

    public void OnPointerEnter(PointerEventData eventData) => DragSlot.Instance.hoveredSlot = slot;
    public void OnPointerExit(PointerEventData eventData) => DragSlot.Instance.hoveredSlot = null;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (slot.HeldItem == null) return;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            QuickMove(slot);
            return;
        }

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (DragSlot.Instance.isDragging) return;
            DragSlot.Instance.StartDrag(slot, slot.HeldQuantity);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            DragSlot.Instance.StartDrag(slot, Mathf.Max(1, slot.HeldQuantity / 2));
        }

        slot.UpdateSlot();
    }

    void QuickMove(BaseSlot from)
    {
        if (from.HeldItem == null) return;

        BaseSlot target = from.slotContext switch
        {
            SlotContext.Hotbar => PlayerInventory.Instance.FindFirstAvailable(false)
            ?? ChestStorage.OpenChest?.FindEmptySlot(),
            SlotContext.Bag    => PlayerInventory.Instance.FindFirstAvailable(true) ?? ChestStorage.OpenChest?.FindEmptySlot(),
            _=> PlayerInventory.Instance.FindFirstAvailable(true)
            ?? PlayerInventory.Instance.FindFirstAvailable(false),
        };

        if (target == null) return;

        SlotUtility.TryMove(from, target, from.HeldQuantity);
        AudioManager.Instance.PlaySound("BagPlace");
    }
}