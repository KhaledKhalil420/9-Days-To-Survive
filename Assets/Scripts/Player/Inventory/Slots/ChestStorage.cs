using UnityEngine;
using System.Collections.Generic;

public class ChestStorage : MonoBehaviour, IInteractable
{
    public static ChestStorage OpenChest; // NEW

    [SerializeField] private GameObject chestUI;
    [SerializeField] private List<BaseSlot> slots;
    [SerializeField] private InventoryHolder inventoryHolder;
    private bool isOpen = false;

    private void Start()
    {
        foreach (BaseSlot slot in slots)
        {
            slot.heldBy = inventoryHolder;
            slot.slotContext = SlotContext.External;
        }

        PlayerInventory.Instance.OnInventoryOpen += CloseUi;
        InvokeRepeating(nameof(UpdateSlots), 0.01f, 0.01f);
    }

    private void UpdateSlots()
    {
        foreach (BaseSlot slot in slots)
        {
            slot.UpdateSlot();
            slot.HeldItem?.gameObject.SetActive(false);
        }
    }

    public BaseSlot FindEmptySlot() // NEW
    {
        foreach (BaseSlot slot in slots)
            if (slot.HeldItem == null) return slot;
        return null;
    }

    public void Interact(GameObject sender) => OpenUi();

    public void CloseUi(bool state)
    {
        if (state) return;

        OpenChest = null; // NEW
        PlayerInventory.Instance.ToggleBagNoEvent(false);
        chestUI.SetActive(false);
        isOpen = false;
    }

    public void OpenUi()
    {
        OpenChest = this; // NEW
        PlayerInventory.Instance.ToggleBagNoEvent(true);
        chestUI.SetActive(true);
        isOpen = true;
    }
}