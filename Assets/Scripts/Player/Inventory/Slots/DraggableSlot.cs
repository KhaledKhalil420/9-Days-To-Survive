using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(SlotHolder))]
public class DraggableSlot : MonoBehaviour, IPointerDownHandler
{
    private SlotHolder slot;

    void Awake()
    {
        slot = GetComponent<SlotHolder>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (slot.HeldItem == null || DragSlot.instance.isDragging)
            return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            DragSlot.instance.StartDrag(slot, slot.HeldQuantity);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            DragSlot.instance.StartDrag(slot, 1);
        }
    }
}
