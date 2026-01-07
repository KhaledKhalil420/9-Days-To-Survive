using DG.Tweening;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragSlot : MonoBehaviour
{
    public static DragSlot instance;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image iconPrefab;

    private RectTransform dragIcon;

    private BaseSlot fromSlot;
    private int heldQuantity;
    internal bool isDragging;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        PlayerInventory.instance.OnInventoryOpen += UpdateDragState;
    }

    void Update()
    {
        instance = this;

        if (!isDragging || dragIcon == null)
            return;

        dragIcon.position = Input.mousePosition;
    }

    public void UpdateDragState(bool state)
    {
        if(!state)
        {
            StopDrag();
        }
    }

    public void StartDrag(BaseSlot from, int quantity)
    {
        if (from.HeldItem == null)
            return;

        StopDrag();

        fromSlot = from;
        heldQuantity = quantity;

        CreateIcon(from.HeldItem.data.sprite);
        isDragging = true;
    }

    public void TryDrop(BaseSlot target, PointerEventData eventData)
    {
        if (!isDragging || target == fromSlot)
            return;

        int qty = heldQuantity;

        bool moved = SlotUtility.TryMove(fromSlot, target, qty);

        if (moved)
        {
            DOVirtual.DelayedCall(0.001f, () => StopDrag());
        }
    }

    void StopDrag()
    {
        if (dragIcon != null)
            Destroy(dragIcon.gameObject);

        isDragging = false;
        fromSlot = null;
        heldQuantity = 0;
    }

    void CreateIcon(Sprite sprite)
    {
        dragIcon = Instantiate(iconPrefab, canvasGroup.transform).rectTransform;
        dragIcon.GetComponent<Image>().sprite = sprite;
    }
}
