// using UnityEngine;
// using UnityEngine.EventSystems;

// public class DragSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
// {
//     private BaseSlot slot;
//     private Canvas canvas;
//     private CanvasGroup canvasGroup;
//     private RectTransform rectTransform;
//     private Vector2 originalPosition;
//     private static DragSlot currentlyDragging;
//     private BaseSlot hoveredSlot;

//     void Awake()
//     {
//         slot = GetComponent<BaseSlot>();
//         rectTransform = GetComponent<RectTransform>();
//         canvas = GetComponentInParent<Canvas>();
//         canvasGroup = GetComponent<CanvasGroup>();
//         if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
//     }

//     public void OnBeginDrag(PointerEventData eventData)
//     {
//         if (slot.IsEmpty()) return;
//         currentlyDragging = this;
//         originalPosition = rectTransform.anchoredPosition;
//         canvasGroup.alpha = 0.6f;
//         canvasGroup.blocksRaycasts = false;
//         AudioManager.Instance?.PlaySound("Pickup", 1f, 1f);
//     }

//     public void OnDrag(PointerEventData eventData)
//     {
//         if (slot.IsEmpty()) return;
//         rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
//     }

//     public void OnEndDrag(PointerEventData eventData)
//     {
//         if (slot.IsEmpty()) { ResetDrag(); return; }
        
//         if (hoveredSlot != null && hoveredSlot != slot)
//         {
//             if (Input.GetKey(KeyCode.LeftShift) && slot.storedQuantity > 1)
//                 SplitStack(hoveredSlot);
//             else
//                 TransferItems(hoveredSlot);
//         }
        
//         ResetDrag();
//         currentlyDragging = null;
//         AudioManager.Instance?.PlaySound("Pickup", 0.9f, 1.1f);
//     }

//     private void TransferItems(BaseSlot target)
//     {
//         // Get types
//         bool sourceIsPlayer = slot is SlotHolder;
//         bool targetIsPlayer = target is SlotHolder;

//         // CASE 1: Both same type (SlotHolder <-> SlotHolder OR ChestSlot <-> ChestSlot)
//         if (sourceIsPlayer == targetIsPlayer)
//         {
//             if (target.IsEmpty())
//             {
//                 // Move
//                 target.storedItemData = slot.storedItemData;
//                 target.storedQuantity = slot.storedQuantity;
                
//                 // Special handling for SlotHolder
//                 if (sourceIsPlayer)
//                 {
//                     SlotHolder sourceSlot = (SlotHolder)slot;
//                     SlotHolder targetSlot = (SlotHolder)target;
//                     targetSlot.HeldItem = sourceSlot.HeldItem;
//                     sourceSlot.HeldItem = null;
//                 }
                
//                 slot.ClearSlot();
//             }
//             else if (target.storedItemData == slot.storedItemData)
//             {
//                 // Merge
//                 target.storedQuantity += slot.storedQuantity;
                
//                 if (sourceIsPlayer)
//                 {
//                     SlotHolder sourceSlot = (SlotHolder)slot;
//                     if (sourceSlot.HeldItem != null) Destroy(sourceSlot.HeldItem.gameObject);
//                     sourceSlot.HeldItem = null;
//                 }
                
//                 slot.ClearSlot();
//             }
//             else
//             {
//                 // Swap
//                 ItemData tempData = target.storedItemData;
//                 int tempQty = target.storedQuantity;
                
//                 target.storedItemData = slot.storedItemData;
//                 target.storedQuantity = slot.storedQuantity;
//                 slot.storedItemData = tempData;
//                 slot.storedQuantity = tempQty;
                
//                 // Special handling for SlotHolder swap
//                 if (sourceIsPlayer)
//                 {
//                     SlotHolder sourceSlot = (SlotHolder)slot;
//                     SlotHolder targetSlot = (SlotHolder)target;
//                     Item tempItem = targetSlot.HeldItem;
//                     targetSlot.HeldItem = sourceSlot.HeldItem;
//                     sourceSlot.HeldItem = tempItem;
//                 }
//             }
//         }
//         // CASE 2: Player -> Chest (Destroy Item, Store Data)
//         else if (sourceIsPlayer && !targetIsPlayer)
//         {
//             SlotHolder sourceSlot = (SlotHolder)slot;
            
//             if (target.IsEmpty())
//             {
//                 target.storedItemData = slot.storedItemData;
//                 target.storedQuantity = slot.storedQuantity;
//                 if (sourceSlot.HeldItem != null) Destroy(sourceSlot.HeldItem.gameObject);
//                 sourceSlot.HeldItem = null;
//                 slot.ClearSlot();
//             }
//             else if (target.storedItemData == slot.storedItemData)
//             {
//                 target.storedQuantity += slot.storedQuantity;
//                 if (sourceSlot.HeldItem != null) Destroy(sourceSlot.HeldItem.gameObject);
//                 sourceSlot.HeldItem = null;
//                 slot.ClearSlot();
//             }
//             else
//             {
//                 // Swap: Chest data -> Player (spawn item), Player item -> Chest (destroy & store)
//                 ItemData tempData = target.storedItemData;
//                 int tempQty = target.storedQuantity;
                
//                 target.storedItemData = slot.storedItemData;
//                 target.storedQuantity = slot.storedQuantity;
                
//                 if (sourceSlot.HeldItem != null) Destroy(sourceSlot.HeldItem.gameObject);
//                 sourceSlot.HeldItem = null;
                
//                 slot.storedItemData = tempData;
//                 slot.storedQuantity = tempQty;
//             }
//         }
//         // CASE 3: Chest -> Player (Spawn Item from Data)
//         else if (!sourceIsPlayer && targetIsPlayer)
//         {
//             SlotHolder targetSlot = (SlotHolder)target;
            
//             if (target.IsEmpty())
//             {
//                 target.storedItemData = slot.storedItemData;
//                 target.storedQuantity = slot.storedQuantity;
//                 slot.ClearSlot();
//             }
//             else if (target.storedItemData == slot.storedItemData)
//             {
//                 target.storedQuantity += slot.storedQuantity;
//                 slot.ClearSlot();
//             }
//             else
//             {
//                 // Swap: Player item -> Chest, Chest data -> Player
//                 if (targetSlot.HeldItem != null) Destroy(targetSlot.HeldItem.gameObject);
//                 targetSlot.HeldItem = null;
                
//                 ItemData tempData = target.storedItemData;
//                 int tempQty = target.storedQuantity;
                
//                 target.storedItemData = slot.storedItemData;
//                 target.storedQuantity = slot.storedQuantity;
//                 slot.storedItemData = tempData;
//                 slot.storedQuantity = tempQty;
//             }
//         }

//         slot.UpdateSlot();
//         target.UpdateSlot();
//     }

//     private void SplitStack(BaseSlot target)
//     {
//         int splitAmount = Mathf.FloorToInt(slot.storedQuantity / 2f);
//         slot.storedQuantity -= splitAmount;

//         if (target.storedItemData == slot.storedItemData)
//             target.storedQuantity += splitAmount;
//         else if (target.IsEmpty())
//         {
//             target.storedItemData = slot.storedItemData;
//             target.storedQuantity = splitAmount;
//         }

//         slot.UpdateSlot();
//         target.UpdateSlot();
//     }

//     private void ResetDrag()
//     {
//         canvasGroup.alpha = 1f;
//         canvasGroup.blocksRaycasts = true;
//         rectTransform.anchoredPosition = originalPosition;
//         hoveredSlot = null;
//     }

//     public void OnPointerEnter(PointerEventData eventData)
//     {
//         if (currentlyDragging != null && currentlyDragging.slot != slot)
//             currentlyDragging.hoveredSlot = slot;
//     }

//     public void OnPointerExit(PointerEventData eventData)
//     {
//         if (currentlyDragging != null && currentlyDragging.hoveredSlot == slot)
//             currentlyDragging.hoveredSlot = null;
//     }
// }