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
