using UnityEngine;
using System.Collections.Generic;

public class ChestStorage : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject chestUI;

    [SerializeField] private Transform slotContainer;
    [SerializeField] private GameObject chestSlotPrefab;
    [SerializeField] private int slotCount = 12;
    
    private List<BaseSlot> slots = new();
    private bool isOpen = false;

    public void Interact(GameObject sender)
    {
        isOpen = !isOpen;
    }

    private void Start()
    {
        for (int i = 0; i < slotCount; i++)
        {
            Instantiate(chestSlotPrefab, slotContainer);
        }
    }
}