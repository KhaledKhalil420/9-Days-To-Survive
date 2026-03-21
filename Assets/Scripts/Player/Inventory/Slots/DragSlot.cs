using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class DragSlot : MonoBehaviour
{
    public static DragSlot Instance;
    public event Action onDrag;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image iconPrefab;
    [SerializeField] private Animator animator;
    [SerializeField] private float smoothness = 15f;

    private RectTransform dragIcon;
    private BaseSlot fromSlot;
    private int heldQuantity;
    internal bool isDragging;
    internal BaseSlot hoveredSlot;

    void Awake() => Instance = this;

    void Start()
    {
        PlayerInventory.Instance.OnInventoryOpen += state => { if (!state) StopDrag(); };
    }

    void Update()
    {
        if (isDragging && dragIcon != null)
        {
            dragIcon.position = Vector3.Lerp(dragIcon.position, Input.mousePosition, smoothness * Time.deltaTime);

            if (Input.GetMouseButtonDown(2))
                StopDrag();
        }

        if (!isDragging && hoveredSlot != null && Input.GetKeyDown(Keybinds.Key("Throw")))
        {
            if (hoveredSlot is SlotHolder slotHolder)
            {
                int idx = Player.inventory.SlotHolders.IndexOf(slotHolder);
                if (idx >= 0) Player.inventory.ThrowItem(idx);
            }
        }
    }

    public void StartDrag(BaseSlot from, int quantity)
    {
        if (from.HeldItem == null) return;

        if (isDragging)
        {
            if (fromSlot == from && from.HeldQuantity > heldQuantity)
            {
                heldQuantity++;
                UpdateIcon();
            }
            return;
        }

        fromSlot = from;
        heldQuantity = quantity;

        CreateIcon(from.HeldItem.data.sprite, quantity);
        dragIcon.position = fromSlot.transform.position;
        isDragging = true;
        onDrag?.Invoke();
    }

    public void TryDrop(BaseSlot target)
    {
        if (!isDragging || target == fromSlot) return;

        bool moved = SlotUtility.TryMove(fromSlot, target, heldQuantity);
        if (!moved) return;

        fromSlot.HeldItem?.OnChangingItems();
        target.HeldItem?.OnPick();
        AudioManager.Instance.PlaySound("BagPlace");
        DOVirtual.DelayedCall(0.001f, StopDrag);
    }

    public void TryDropOne(BaseSlot target)
    {
        if (!isDragging || target == fromSlot || heldQuantity <= 0) return;

        bool moved = SlotUtility.TryMove(fromSlot, target, 1);
        if (!moved) return;

        heldQuantity--;
        target.HeldItem?.OnPick();
        AudioManager.Instance.PlaySound("BagPlace");

        if (heldQuantity <= 0) { StopDrag(); return; }

        UpdateIcon();
    }

    void UpdateIcon()
    {
        dragIcon.GetComponent<Animator>().SetTrigger("Trigger");
        dragIcon.GetComponentInChildren<TMP_Text>().text = "x" + heldQuantity;
    }

    void StopDrag()
    {
        if (dragIcon != null) Destroy(dragIcon.gameObject);

        isDragging = false;
        fromSlot = null;
        heldQuantity = 0;
        onDrag?.Invoke();
    }

    void CreateIcon(Sprite sprite, int quantity)
    {
        dragIcon = Instantiate(iconPrefab, canvasGroup.transform).rectTransform;
        dragIcon.GetComponent<Image>().sprite = sprite;
        dragIcon.GetComponentInChildren<TMP_Text>().text = "x" + quantity;
    }
}