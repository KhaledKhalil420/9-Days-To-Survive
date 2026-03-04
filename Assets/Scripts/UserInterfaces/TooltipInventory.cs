using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipInventory : MonoBehaviour, IPointerMoveHandler, IPointerExitHandler
{
    private bool isHovering;
    [SerializeField] private CanvasGroup parent;
    [SerializeField] private TMP_Text textName;
    [SerializeField] private TMP_Text textDiscription;
    [SerializeField] private Vector3 offset;

    private void Start()
    {
        DragSlot.Instance.onDrag += UpdateUi;
    }

    private void Update()
    {
        parent.transform.position = Vector3.Lerp(parent.transform.position, Input.mousePosition + offset, 45 * Time.deltaTime);
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if(DragSlot.Instance.isDragging)
            return;

        if (eventData.pointerCurrentRaycast.gameObject != null && eventData.pointerCurrentRaycast.gameObject.TryGetComponent(out BaseSlot slot))
        {
            if (slot.HeldItem == null)
            {
                isHovering = false;
                CancelInvoke(nameof(UpdateUi));
                UpdateUi();

                return;
            }

            ItemData itemData = slot.HeldItem.data;
            textName.text = itemData.Name;
            textDiscription.text = itemData.discription;

            if (!isHovering)
            {
                isHovering = true;
                Invoke(nameof(UpdateUi), 0.25f);
            }

        }
        else
        {
            if (isHovering)
            {
                isHovering = false;
                CancelInvoke(nameof(UpdateUi));
                UpdateUi();
            }

        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        CancelInvoke(nameof(UpdateUi));
        UpdateUi();
    }

    private Tween faderTween;
    private void UpdateUi()
    {
        if(DragSlot.Instance.isDragging)
            isHovering = false;

        faderTween?.Kill();
        faderTween = parent.DOFade(isHovering ? 1 : 0, 0.2f);
    }
}