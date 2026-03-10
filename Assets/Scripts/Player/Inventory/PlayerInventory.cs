using TMPro;
using System;
using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

[Serializable]
public class InventoryHolder
{
    public Transform parent;
    public Transform hand;
}

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance;

    [Header("Slot setup")]
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] internal List<SlotHolder> SlotHolders = new();
    [SerializeField] internal CanvasGroup group;

    [Header("Item use")]
    [SerializeField] internal InventoryHolder holder;
    private PlayerInteract interact;
    internal float damageBonus = 1, speedBonus = 1;
    internal bool canUse = true;

    [Header("Pickup UI")]
    [SerializeField] private GameObject pickedUpUIPrefab;
    [SerializeField] private Transform pickedUpUIParent;
    private Dictionary<string, UiPickedUpItemInfo> activePickedUpUIs = new();
    
    [Header("HotBar")]
    [SerializeField] private int mainSlots = 7;
    [SerializeField] private Transform slotParent; //parent for hotbar
    public static bool CanScroll = true;

    [Header("Bag")]
    [SerializeField] private CanvasGroup bagParent; //same as slot parent, remember ya ana
    private bool isBagOpen = false;
    public event Action<bool> OnInventoryOpen;

    [Header("Visuals")]
    [SerializeField] private Color selectedSlotColor;
    [SerializeField] private Color unselectedSlotColor;
    [SerializeField] private TMP_Text heldItemDisplayText;
    internal SlotHolder selectedSlot;
    private Transform _camera;
    private int lastSelectedSlot = 1;

    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _camera = PlayerLook.mainCamera.transform;
        interact = GetComponent<PlayerInteract>();
        InvokeRepeating(nameof(UpdateSlots), 0.0001f, 0.0001f);

        foreach (SlotHolder slot in SlotHolders)
        {
            slot.heldBy = holder;
        }
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
            int added = item.HeldQuantity;
            Destroy(item.gameObject);
            sameTypeSlot.UpdateSlot();

            SpawnPickedUpUI(item.data, added);
        }

        else if (FindNearestEmptySlot(out SlotHolder emptySlot) != null)
        {
            emptySlot.HeldItem = item;
            emptySlot.HeldQuantity = item.HeldQuantity;
            item.heldby = gameObject;
            item.SetItemParent(holder.hand);
            emptySlot.UpdateSlot();

            SpawnPickedUpUI(item.data, item.HeldQuantity);
        }

        UpdateSlots();
    }

    public void GiveItem(Item item, out bool given)
    {
        if (FindSameTypeSlot(item, out SlotHolder sameTypeSlot) != null)
        {
            sameTypeSlot.HeldQuantity += item.HeldQuantity;
            int added = item.HeldQuantity;
            Destroy(item.gameObject);
            sameTypeSlot.UpdateSlot();

            SpawnPickedUpUI(item.data, added);

            given = true;
        }

        else if (FindNearestEmptySlot(out SlotHolder emptySlot) != null)
        {
            emptySlot.HeldItem = item;
            emptySlot.HeldQuantity = item.HeldQuantity;
            item.heldby = gameObject;
            item.SetItemParent(holder.hand);
            if(selectedSlot == emptySlot)
            {
                item.isSelected = true;
            }
            item.OnPick();
            emptySlot.UpdateSlot();

            SpawnPickedUpUI(item.data, item.HeldQuantity);

            given = true;
        }

        else
        {
            given = false;
        }
        
        UpdateSlots();
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
        HandleBag();
        HandlePickup();
        HandleThrowing();
        HandleSlotsSwitching();
        HandleUse();
    }

    private void HandleBag()
    {
        if(Input.GetKeyDown(Keybinds.Key("InventoryOpen")) && canUse)
        {
            ToggleBag();
        }
    }

    public void ToggleBag()
    {
        isBagOpen = !isBagOpen;
        AudioManager.Instance.PlaySound(isBagOpen ? "BagOpen" : "BagClose");
        bagParent.interactable = isBagOpen;
        bagParent.alpha = isBagOpen ? 1 : 0;
        bagParent.interactable = isBagOpen;
        UiManager.ToggleUi(isBagOpen);
        OnInventoryOpen?.Invoke(isBagOpen);
    }

    public void ToggleBag(bool state)
    {
        isBagOpen = state;
        bagParent.interactable = state;
        bagParent.alpha = state ? 1 : 0;
        bagParent.interactable = state;
        UiManager.ToggleUi(state);
        OnInventoryOpen?.Invoke(state);
    }

    public void ToggleBagNoEvent(bool state)
    {
        isBagOpen = state;
        bagParent.interactable = state;
        bagParent.alpha = state ? 1 : 0;
        bagParent.interactable = state;
        UiManager.ToggleUi(state);
    }

    public void ToggleBagNoToggleUi(bool state)
    {
        isBagOpen = state;
        bagParent.interactable = state;
        bagParent.alpha = state ? 1 : 0;
        bagParent.interactable = state;
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

                    if (wasGiven)
                    {
                        AudioManager.Instance?.PlaySound("Pickup", 0.9f, 1.1f);
                    }
                }
            }
        }
    }

    private void HandleThrowing()
    {
        if (Input.GetKeyDown(Keybinds.Key("Throw")))
        {
            ThrowItem(_currentSlotIndex);
        }
    }

    public void ThrowItem(int throwAt)
    {
        SlotHolder selectedSlot = SlotHolders[throwAt];
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

            selectedSlot.ResetSlot();
            heldItem.OnChangingItems();
        }

        OnInventoryOpen?.Invoke(false);
        UpdateSlots();
    }

    private void HandleUse()
    {
        if(isBagOpen || !canUse) 
            return;

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

    private int _currentSlotIndex = 1;

    private void HandleSlotsSwitching()
    {
        // Number keys 1-9
        for (int i = 0; i < mainSlots; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                if(i == lastSelectedSlot) 
                return;
                SlotHolders[_currentSlotIndex].HeldItem?.OnChangingItems();
                
                lastSelectedSlot = i;
                _currentSlotIndex = i;

                holder.hand.transform.localEulerAngles = new Vector3(45, 0, 0);
                holder.hand.transform.localPosition = new Vector3(1, -2f, 1);

                break;
            }
        }

        if(CanScroll)
        {
            // Scroll wheel up/down
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll < 0f)
            {
                SlotHolders[_currentSlotIndex].HeldItem?.OnChangingItems();
                _currentSlotIndex = (_currentSlotIndex + 1) % mainSlots;
                lastSelectedSlot = _currentSlotIndex;

                holder.hand.transform.localEulerAngles = new Vector3(45, 0, 0);
                holder.hand.transform.localPosition = new Vector3(1, -2f, 1);
            }

            else if (scroll > 0f)
            {
                SlotHolders[_currentSlotIndex].HeldItem?.OnChangingItems();
                _currentSlotIndex = (_currentSlotIndex - 1 + mainSlots) % mainSlots;
                lastSelectedSlot = _currentSlotIndex;

                holder.hand.transform.localEulerAngles = new Vector3(45, 0, 0);
                holder.hand.transform.localPosition = new Vector3(1, -2f, 1);
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
            if (slot?.HeldItem?.data == item.data)
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
            if (slot.HeldItem != null ? slot.HeldItem.data : null == item.data)
            {
                return true;
            }
        }

        return false;
    }

    public bool HasItem(Item item, int quantity)
    {
        UpdateSlots();

        foreach (var slot in SlotHolders)
        {
            if(slot.HeldItem != null)
            if (slot.HeldItem.data == item.data && slot.HeldItem.HeldQuantity >= quantity)
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

    #endregion

    void SpawnPickedUpUI(ItemData itemData, int amount)
    {
        if (pickedUpUIPrefab == null) return;
        if (itemData == null) return;

        string key = itemData.Name;

        if (activePickedUpUIs.TryGetValue(key, out var existing))
        {
            existing.AddQuantity(amount);
            return;
        }

        var go = Instantiate(pickedUpUIPrefab, pickedUpUIParent);
        var info = go.GetComponent<UiPickedUpItemInfo>();
        if (info == null)
        {
            Destroy(go);
            return;
        }

        info.Init(itemData, amount);
        info.onFinished = () =>
        {
            if (activePickedUpUIs.ContainsKey(key))
                activePickedUpUIs.Remove(key);
        };

        activePickedUpUIs[key] = info;
    }

    private void OnDisable()
    {
        group.DOFade(0, 1f).SetUpdate(true);
    }

    private void OnEnable()
    {
        group.DOFade(1, 0.1f).SetUpdate(true);
    }

    void OnValidate()
    {
        Instance = this;
    }
}