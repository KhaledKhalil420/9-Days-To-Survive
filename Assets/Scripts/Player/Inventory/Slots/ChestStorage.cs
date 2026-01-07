using UnityEngine;
using System.Collections.Generic;

public class ChestStorage : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject chestUI;
    [SerializeField] private List<BaseSlot> slots;
    [SerializeField] private InventoryHolder inventoryHolder;
    private bool isOpen = false;


    private void Start()
    {
        foreach(BaseSlot slot in slots)
        {
            slot.heldBy = inventoryHolder;
        }

        PlayerInventory.instance.OnInventoryOpen += CloseUi;

        // InvokeRepeating(nameof(UpdateSlots), 0.01f, 0.01f);
    }

    private void UpdateSlots()
    {
        foreach(BaseSlot slot in slots)
        {
            slot.UpdateSlot();
            slot.HeldItem?.gameObject.SetActive(false);
        }
    }

    public void Interact(GameObject sender)
    {
        OpenUi();
    }

    public void CloseUi(bool state)
    {
        if(state) return;

        PlayerInventory.instance.ToggleBagNoEvent(false);
        chestUI.SetActive(false);
        isOpen = false;
    }

    public void OpenUi()
    {
        PlayerInventory.instance.ToggleBagNoEvent(true);
        chestUI.SetActive(true);
        isOpen = true;
    }
}