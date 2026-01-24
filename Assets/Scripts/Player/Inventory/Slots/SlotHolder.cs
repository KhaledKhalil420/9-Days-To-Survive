using DG.Tweening;
using UnityEngine;

public class SlotHolder : BaseSlot
{
    public bool isSelected;
    private bool wasVisual, wasEvent;
    private Vector3 initialSize;

    void Start() => initialSize = transform.localScale;

    public override void Visuals()
    {
        _slotBorderImage.color = Color.Lerp(_slotBorderImage.color, isSelected ? selected : unselected, Time.deltaTime * 10f);

        if (wasVisual == isSelected) 
            return;

        transform.DOKill();
        transform.DOLocalRotate(isSelected ? new Vector3(0, 0, -2) : Vector3.zero, 1);
        transform.DOScale(isSelected ? initialSize * 1.15f : initialSize, 1);

        wasVisual = isSelected;
    }

    public override void OnUpdateSlot()
    {
        if (!HeldItem) 
            return;

        HeldItem.isSelected = isSelected;
        HeldItem.gameObject.SetActive(isSelected);

        if (isSelected && !wasEvent) HeldItem.OnSelectOnce();
        if (isSelected) HeldItem.OnSelect();

        wasEvent = isSelected;
    }
}
