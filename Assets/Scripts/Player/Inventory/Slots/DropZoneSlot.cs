using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(BaseSlot))]
public class DropZoneSlot : MonoBehaviour, IPointerDownHandler
{
    private BaseSlot slot;

    void Awake() => slot = GetComponent<BaseSlot>();

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!DragSlot.Instance.isDragging) return;
        if (eventData.button == PointerEventData.InputButton.Middle) return;

        if (eventData.button == PointerEventData.InputButton.Right)
            DragSlot.Instance.TryDropOne(slot);
        else
            DragSlot.Instance.TryDrop(slot);

        slot.UpdateSlot();
    }
}