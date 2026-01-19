using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(BaseSlot))]
public class DropZoneSlot : MonoBehaviour, IPointerDownHandler
{
    private BaseSlot slot;

    void Awake()
    {
        slot = GetComponent<BaseSlot>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Middle && DragSlot.instance.isDragging)
        {
            DragSlot.instance.TryDrop(slot, eventData);
        }
    }
}
