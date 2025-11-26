using UnityEngine;
using UnityEngine.UI;

public class HeldSlot : MonoBehaviour
{
    // what I want, slot to hold an Item data, store last slot, cancel method to bring everything to normal
    //Follow mouse, click on slot, lerp, 

    private Item _currentHeldItem;
    private SlotHolder _slotTakenFrom;
    private bool isDragging;

    [SerializeField] private Image _image;
    [SerializeField] private float _lerpSpeed;

    private void Update()
    {
        //Follow here

        //Start dragging when clicked on a slot only if not holding another slot
    }

    private void BeginDragging()
    {
        _slotTakenFrom.RemoveSlotSprite();
        _currentHeldItem = _slotTakenFrom.HeldItem;
        _image.sprite = _currentHeldItem.data.sprite;
    }

    private void EndDragging()
    {

    }

    private void StopDragging()
    {
        _slotTakenFrom.ResetSlotSprite();
    }
}
