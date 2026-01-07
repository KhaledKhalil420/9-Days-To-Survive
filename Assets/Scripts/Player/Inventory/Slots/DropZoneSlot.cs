using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(SlotHolder))]
public class DropZoneSlot : MonoBehaviour, IPointerDownHandler
{
    private BaseSlot slot;

    void Awake()
    {
        slot = GetComponent<BaseSlot>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (DragSlot.instance.isDragging)
        {
            DragSlot.instance.TryDrop(slot, eventData);
        }
    }
}
