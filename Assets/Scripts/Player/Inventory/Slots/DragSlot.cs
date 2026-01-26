using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DragSlot : MonoBehaviour
{
    public static DragSlot instance;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image iconPrefab;

    private RectTransform dragIcon;
    [SerializeField] private float smoothness = 15f;

    private BaseSlot fromSlot;
    private int heldQuantity;
    internal bool isDragging;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        PlayerInventory.Instance.OnInventoryOpen += UpdateDragState;
    }

    void Update()
    {
        if (!isDragging || dragIcon == null)
            return;

        dragIcon.position = Vector3.Lerp(dragIcon.position, Input.mousePosition, smoothness * Time.deltaTime);

        if(Input.GetMouseButtonDown(2))
        {
            StopDrag();
        }
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

        CreateIcon(from.HeldItem.data.sprite, quantity);
        dragIcon.position = Input.mousePosition;
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
            AudioManager.Instance.PlaySound("BagPlace");
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

    void CreateIcon(Sprite sprite, int quantity)
    {
        dragIcon = Instantiate(iconPrefab, canvasGroup.transform).rectTransform;
        dragIcon.GetComponent<Image>().sprite = sprite;
        dragIcon.GetComponentInChildren<TMP_Text>().text =  "x" + quantity.ToString();
    }
}
