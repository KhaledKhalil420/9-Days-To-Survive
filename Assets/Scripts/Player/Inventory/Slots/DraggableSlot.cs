using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(BaseSlot))]
public class DraggableSlot : MonoBehaviour, IPointerDownHandler
{
    private BaseSlot slot;

    void Awake()
    {
        slot = GetComponent<BaseSlot>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (slot.HeldItem == null)
            return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (DragSlot.Instance.isDragging) return;
            DragSlot.Instance.StartDrag(slot, slot.HeldQuantity);
            slot.UpdateSlot();
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            DragSlot.Instance.StartDrag(slot, 1);
            slot.UpdateSlot();
        }
    }
}