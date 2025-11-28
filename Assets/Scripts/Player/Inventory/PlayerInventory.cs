using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory instance;

    [Header("Slot setup")]
    public GameObject slotPrefab;
    public Transform slotParent;

    public List<SlotHolder> SlotHolders = new();
    [Header("Visuals")]
    public Color selectedSlotColor, unselectedSlotColor;
    public CanvasGroup group, heldItemGroup;
    public TMP_Text heldItemDisplayText;
    internal SlotHolder selectedSlot;
    private Transform _camera;

    [SerializeField] private Transform hand;
    [SerializeField] private int additionalSlots = 0;
    private PlayerInteract interact;

    void Awake()
    {
        instance = this;

        additionalSlots = PlayerPrefs.GetInt("Slots", 0);

        for (int i = 0; i < additionalSlots; i++)
        {
            RetriveInventorySlots();
        }
    }

    private void Start()
    {
        _camera = PlayerLook.mainCamera.transform;
        interact = GetComponent<PlayerInteract>();
        InvokeRepeating(nameof(UpdateSlots), 0.0001f, 0.0001f);
    }

    private void Update()
    {
        Inputs();
    }

    #region Inventory Methods

    public void GiveItem(Item item)
    {
        if (FindSameTypeSlot(item, out SlotHolder sameTypeSlot) != null)
        {
            sameTypeSlot.HeldQuantity += item.HeldQuantity;
            Destroy(item.gameObject);
            sameTypeSlot.UpdateSlot();
        }

        else if (FindNearestEmptySlot(out SlotHolder emptySlot) != null)
        {
            emptySlot.HeldItem = item;
            emptySlot.HeldQuantity = item.HeldQuantity;
            item.heldby = gameObject;
            item.SetItemParent(hand);
            emptySlot.UpdateSlot();
        }
    }

    public void GiveItem(Item item, out bool given)
    {
        if (FindSameTypeSlot(item, out SlotHolder sameTypeSlot) != null)
        {
            sameTypeSlot.HeldQuantity += item.HeldQuantity;
            Destroy(item.gameObject);
            sameTypeSlot.UpdateSlot();

            given = true;
        }

        else if (FindNearestEmptySlot(out SlotHolder emptySlot) != null)
        {
            emptySlot.HeldItem = item;
            emptySlot.HeldQuantity = item.HeldQuantity;
            item.heldby = gameObject;
            item.SetItemParent(hand);
            emptySlot.UpdateSlot();

            given = true;
        }

        else
        {
            given = false;
        }
    }

    public void TakeItem(Item item, int quantity, out bool wasTaken)
    {
        FindSlotWithItem(item, out SlotHolder slot);

        if (slot != null && slot.HeldQuantity >= quantity)
        {
            slot.HeldQuantity -= quantity;
            wasTaken = true;
        }
        else
        {
            wasTaken = false;
        }


        UpdateSlots();
    }

    #endregion

    #region Inputs

    private void Inputs()
    {
        HandlePickup();
        HandleThrowing();
        HandleSlotsSwitching();
        HandleUse();
    }

    private void HandlePickup()
    {
        //if found same type slot, take as much as you can, then on next interact add the rest to a new slot
        if (Input.GetKey(Keybinds.Key("Interact")))
        {
            if (Physics.Raycast(_camera.position, _camera.forward, out RaycastHit hit, interact.raycastDistance, LayerMask.GetMask("Pickable")))
            {
                if (hit.transform.TryGetComponent(out Item item))
                {
                    GiveItem(item, out bool wasGiven);
                    
                    if(wasGiven)
                    {
                        
                    }
                }
            }
        }
    }

    private void HandleThrowing()
    {
        if (Input.GetKeyDown(Keybinds.Key("Throw")))
        {
            SlotHolder selectedSlot = SlotHolders[_currentSlotIndex];
            Item heldItem = selectedSlot.HeldItem;

            if (heldItem != null)
            {
                heldItem.OnThrow();

                heldItem.HeldQuantity = selectedSlot.HeldQuantity;
                heldItem.heldby = null;
                heldItem.SetItemParent(null);
                heldItem.transform.position = _camera.position;

                if (heldItem.TryGetComponent(out Rigidbody rigidbody))
                    rigidbody.AddForce(_camera.forward * 6, ForceMode.Impulse);

                if (heldItem.TryGetComponent(out Animator animator))
                    animator.enabled = false;

                heldItem.OnChangingItems();
                selectedSlot.ResetSlot();
            }

            UpdateSlots();
        }
    }

    private void HandleUse()
    {
        if (Input.GetKeyDown(Keybinds.Key("Use")))
        {
            SlotHolder selectedSlot = SlotHolders[_currentSlotIndex];

            if (selectedSlot.HeldItem != null)
                selectedSlot.HeldItem.OnUse();
        }

        if (Input.GetKey(Keybinds.Key("Use")))
        {
            SlotHolder selectedSlot = SlotHolders[_currentSlotIndex];

            if (selectedSlot.HeldItem != null)
                selectedSlot.HeldItem.OnUsing();
        }

        if (Input.GetKeyUp(Keybinds.Key("Use")))
        {
            SlotHolder selectedSlot = SlotHolders[_currentSlotIndex];

            if (selectedSlot.HeldItem != null)
                selectedSlot.HeldItem.OnStoppingUse();
        }

        if (Input.GetKeyDown(Keybinds.Key("Use Alt")))
        {
            SlotHolder selectedSlot = SlotHolders[_currentSlotIndex];

            if (selectedSlot.HeldItem != null)
                selectedSlot.HeldItem.OnUseAlt();
        }

        if (Input.GetKeyUp(Keybinds.Key("Use Alt")))
        {
            SlotHolder selectedSlot = SlotHolders[_currentSlotIndex];

            if (selectedSlot.HeldItem != null)
                selectedSlot.HeldItem.OnStoppingUseAlt();
        }

        if (Input.GetKeyDown(Keybinds.Key("Use Alt 2")))
        {
            SlotHolder selectedSlot = SlotHolders[_currentSlotIndex];

            if (selectedSlot.HeldItem != null)
                selectedSlot.HeldItem.OnUseMiddle();
        }
    }


    private int _currentSlotIndex = 0;

    private void HandleSlotsSwitching()
    {
        // // Scroll wheel up/down
        // float scroll = Input.GetAxis("Mouse ScrollWheel");
        // if (scroll < 0f)
        // {
        //     SlotHolders[_currentSlotIndex].HeldItem?.OnChangingItems();
        //     _currentSlotIndex = (_currentSlotIndex + 1) % SlotHolders.Count;
        // }

        // else if (scroll > 0f)
        // {
        //     SlotHolders[_currentSlotIndex].HeldItem?.OnChangingItems();
        //     _currentSlotIndex = (_currentSlotIndex - 1 + SlotHolders.Count) % SlotHolders.Count;
        // }

        // Number keys 1-9
        for (int i = 0; i < Mathf.Min(9, SlotHolders.Count); i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SlotHolders[_currentSlotIndex].HeldItem?.OnChangingItems();

                //Visuals
                if(group != null)
                {
                    DOVirtual.Float(group.alpha, 0.5f, 0.25f, value => group.alpha = value);
                    DOVirtual.Float(heldItemGroup.alpha, 1, 0.25f, value => heldItemGroup.alpha = value);
                    CancelInvoke(nameof(DecreaseAlpha));
                    Invoke(nameof(DecreaseAlpha), 4);
                }

                _currentSlotIndex = i;
                break;
            }
        }

        for (int i = 0; i < SlotHolders.Count; i++)
        {
            if (i == _currentSlotIndex)
                SlotHolders[i].isSelected = true;

            else
                SlotHolders[i].isSelected = false;

            SlotHolders[i].UpdateSlot();
        }

        //Update held slot
        selectedSlot = SlotHolders[_currentSlotIndex];

        //Update held item name
        heldItemDisplayText.text = SlotHolders[_currentSlotIndex].HeldItem != null ? SlotHolders[_currentSlotIndex].HeldItem.data.Name : "";
    }

    private void DecreaseAlpha()
    {
        if(group == null) return;   
        DOVirtual.Float(group.alpha, 0.25f, 2, value => group.alpha = value);
        DOVirtual.Float(heldItemGroup.alpha, 0.05f, 2, value => heldItemGroup.alpha = value);
    }
    #endregion

    #region Ref returns

    public SlotHolder GetSelectedSlot()
    {
        return SlotHolders[_currentSlotIndex];
    }

    public Item GetHeldItem()
    {
        return GetSelectedSlot().HeldItem;
    }

    #endregion

    #region Slots Handling
    /// <summary>
    /// Returns Nearest Empty Slot
    /// </summary>
    /// <param name="OutSlot"></param>
    /// <returns></returns>
    private SlotHolder FindNearestEmptySlot(out SlotHolder OutSlot)
    {
        foreach (var slot in SlotHolders)
        {
            if (slot.HeldItem == null)
            {
                OutSlot = slot;
                return slot;
            }
        }

        OutSlot = null;
        return null;
    }

    /// <summary>
    /// Returns a slot with the held item
    /// </summary>
    /// <param name="item"></param>
    /// <param name="OutSlot"></param>
    /// <returns></returns>
    private SlotHolder FindSameTypeSlot(Item item, out SlotHolder OutSlot)
    {
        foreach (var slot in SlotHolders)
        {
            if (slot.HeldItem != null &&
                slot.HeldItem.data.Name == item.data.Name &&
                !item.isSingleQuantityItem)
            {
                OutSlot = slot;
                return slot;
            }
        }

        OutSlot = null;
        return null;
    }

    public SlotHolder FindSlotWithItem(Item item, out SlotHolder outSlot)
    {
        foreach (var slot in SlotHolders)
        {
            if (slot.HeldItem.data == item.data)
            {
                outSlot = slot;
                return slot;
            }

        }

        outSlot = null;
        return null;
    }

    public bool HasItem(Item item)
    {
        foreach (var slot in SlotHolders)
        {
            if (slot.HeldItem?.data == item.data)
            {
                return true;
            }
        }

        return false;
    }

    public bool HasItem(Item item, int quantity)
    {
        foreach (var slot in SlotHolders)
        {
            if (slot.HeldItem?.data == item.data && slot.HeldItem.HeldQuantity >= quantity)
            {
                return true;
            }
        }

        return false;
    }

    private void UpdateSlots()
    {
        foreach (var slot in SlotHolders)
        {
            slot.UpdateSlot();
        }
    }

    public void RetriveInventorySlots()
    {
        SlotHolder slot = Instantiate(slotPrefab, slotParent).GetComponent<SlotHolder>();
        SlotHolders.Add(slot);
        UpdateSlots();
    }
    
    public void AddInventorySlot()
    {
        SlotHolder slot = Instantiate(slotPrefab, slotParent).GetComponent<SlotHolder>();
        SlotHolders.Add(slot);
        UpdateSlots();

        additionalSlots++;
    }

    #endregion
}