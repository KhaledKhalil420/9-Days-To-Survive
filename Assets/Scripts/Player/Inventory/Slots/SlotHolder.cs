using DG.Tweening;
using UnityEngine;

public class SlotHolder : BaseSlot
{
    public bool isSelected;   
    private bool wasSelected;

    public override void Visuals()
    {
        _slotBorderImage.color = Color.Lerp(_slotBorderImage.color, isSelected ? selected : unselected, Time.deltaTime * 10f);

        if (wasSelected == isSelected)
            return;

        if (isSelected)
        {
            transform.DOLocalRotate(new Vector3(0, 0, -2), 1);
            transform.DOScale(Vector3.one * 1.15f, 1);
        }
        else
        {
            transform.DOLocalRotate(Vector3.zero, 1);
            transform.DOScale(Vector3.one, 1);
        }

        wasSelected = isSelected;
    }


    public override void OnUpdateSlot()
    {
        if (HeldItem != null)
        {            
            if (isSelected)
            {
                HeldItem.OnSelect();

                if(!wasSelected)
                {
                    HeldItem.OnSelectOnce();
                }
            }

            HeldItem.isSelected = isSelected;
            HeldItem.gameObject.SetActive(isSelected);
        }

        wasSelected = isSelected;
    }
}