using System;
using System.Collections.Generic;
using EasyButtons;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    private List<InventorySlot> slots;
    private int capacity = 12;

    private int selectSlotIdxL = 0;
    private int selectSlotIdxR = 0;

    public InventoryManager()
    {
        slots = new List<InventorySlot>();

        for (int i = 0; i < capacity; i++)
        {
            slots.Add(new InventorySlot());
        }
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        transform.GetChild(selectSlotIdxR).Find("_HandR").gameObject.SetActive(true);

        GameEventManager.OnHeldItemConsumed += HeldItemConsumed;
        GameEventManager.OnItemUpdate += RefreshUI;
    }

    void Destroy()
    {
        GameEventManager.OnHeldItemConsumed -= HeldItemConsumed;
        GameEventManager.OnItemUpdate -= RefreshUI;
    }

    private bool isShiftPressed = false;
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.LeftShift)) isShiftPressed = true;
        if(Input.GetKeyUp(KeyCode.LeftShift)) isShiftPressed = false;

        if(Input.GetKeyDown(KeyCode.Alpha1)) UpdateSelectIdx(0);
        else if(Input.GetKeyDown(KeyCode.Alpha2)) UpdateSelectIdx(1);
        else if(Input.GetKeyDown(KeyCode.Alpha3)) UpdateSelectIdx(2);
        else if(Input.GetKeyDown(KeyCode.Alpha4)) UpdateSelectIdx(3);
        else if(Input.GetKeyDown(KeyCode.Alpha5)) UpdateSelectIdx(4);
        else if(Input.GetKeyDown(KeyCode.Alpha6)) UpdateSelectIdx(5);
        else if(Input.GetKeyDown(KeyCode.Alpha7)) UpdateSelectIdx(6);
        else if(Input.GetKeyDown(KeyCode.Alpha8)) UpdateSelectIdx(7);
        else if(Input.GetKeyDown(KeyCode.Alpha9)) UpdateSelectIdx(8);
        else if(Input.GetKeyDown(KeyCode.Alpha0)) UpdateSelectIdx(9);
        else if(Input.GetKeyDown(KeyCode.Minus)) UpdateSelectIdx(10);
        else if(Input.GetKeyDown(KeyCode.Equals)) UpdateSelectIdx(11);

        // Debug Button
        else if(Input.GetKeyDown(KeyCode.B)) AddBow();
        else if(Input.GetKeyDown(KeyCode.M)) AddMat();

    }

    void UpdateSelectIdx(int idx)
    {
        if(isShiftPressed)
        {
            if(selectSlotIdxL == idx) return;
            transform.GetChild(selectSlotIdxL).Find("_HandL").gameObject.SetActive(false);
            selectSlotIdxL = idx;
            transform.GetChild(selectSlotIdxL).Find("_HandL").gameObject.SetActive(true);

            var item = slots[selectSlotIdxL].item;
            
            GameEventManager.TriggerItemHeld(item, HandType.HandL);
        }
        else
        {
            if(selectSlotIdxR == idx) return;
            transform.GetChild(selectSlotIdxR).Find("_HandR").gameObject.SetActive(false);
            selectSlotIdxR = idx;
            transform.GetChild(selectSlotIdxR).Find("_HandR").gameObject.SetActive(true);

            var item = slots[selectSlotIdxR].item;
            
            GameEventManager.TriggerItemHeld(item, HandType.HandR);
        }
        
    }

    [Button]
    public bool AddItem(int itemId, int quantity = 1)
    {
        Debug.Log($"add {quantity} {itemId}");
        return AddItem(ItemDatabase.Instance.CreateItem(itemId), quantity);
    }

    [Button]
    public void AddBow()
    {
        if(!HasItem(100011)) AddItem(ItemDatabase.Instance.CreateItem(100011), 1);
        
        AddItem(ItemDatabase.Instance.CreateItem(100010), 10);
    }

    [Button]
    public void AddMat()
    {   
        AddItem(ItemDatabase.Instance.CreateItem(100000), 10);
        AddItem(ItemDatabase.Instance.CreateItem(100001), 10);
        AddItem(ItemDatabase.Instance.CreateItem(100002), 10);
    }

    public bool AddItem(ItemBase item, int quantity = 1)
    {
        if (item == null || quantity <= 0) return false;
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].CanAddItem(item, quantity))
            {
                if (slots[i].IsEmpty())
                {
                    slots[i].item = item;
                    slots[i].quantity = quantity;
                }
                else
                {
                    slots[i].quantity += quantity;
                }

                UpdateSlot(i);
                return true;
            }
        }
        
        if (quantity > item.data.maxStackSize)
        {
            return AddItemInMultipleSlots(item, quantity);
        }
        
        InventorySlot emptySlot = GetFirstEmptySlot();
        if (emptySlot != null)
        {
            emptySlot.item = item;
            emptySlot.quantity = quantity;
            RefreshUI();
            return true;
        }
        
        return false;
    }
    
    private bool AddItemInMultipleSlots(ItemBase item, int totalQuantity)
    {
        int remainingQuantity = totalQuantity;
        
        while (remainingQuantity > 0)
        {
            InventorySlot targetSlot = GetSlotForItem(item);
            if (targetSlot == null)
            {
                Debug.Log("Not enough space in inventory!");
                return false;
            }
            
            int addAmount = Mathf.Min(remainingQuantity, targetSlot.GetRemainingSpace());
            if (targetSlot.IsEmpty())
            {
                targetSlot.item = item;
                targetSlot.quantity = addAmount;
            }
            else
            {
                targetSlot.quantity += addAmount;
            }
            
            remainingQuantity -= addAmount;
        }
        
        return true;
    }
    
    public bool RemoveItem(int itemID, int quantity = 1)
    {
        int remainingToRemove = quantity;
        
        for (int i = slots.Count - 1; i >= 0 && remainingToRemove > 0; i--)
        {
            if (!slots[i].IsEmpty() && slots[i].item.data.itemID == itemID)
            {
                int removeAmount = Mathf.Min(remainingToRemove, slots[i].quantity);
                slots[i].quantity -= removeAmount;
                remainingToRemove -= removeAmount;
                
                if (slots[i].quantity <= 0)
                {
                    slots[i].item = null;
                    slots[i].quantity = 0;
                }
                UpdateSlot(i);
            }
        }
        // RefreshUI();
        return remainingToRemove == 0;
    }
    
    public bool RemoveItemFromSlot(int slotIndex, int quantity = 1)
    {
        if (slotIndex < 0 || slotIndex >= slots.Count) return false;
        if (slots[slotIndex].IsEmpty()) return false;
        if (slots[slotIndex].quantity < quantity) return false;
        if(slots[slotIndex].item.data.isStackable == false)
        {
            slots[slotIndex].item = null;
            slots[slotIndex].quantity = 0;
            return true;
        }
        
        slots[slotIndex].quantity -= quantity;
        if (slots[slotIndex].quantity <= 0)
        {
            slots[slotIndex].item = null;
            slots[slotIndex].quantity = 0;
        }
        UpdateSlot(selectSlotIdxR);
        return true;
    }
    
    public InventorySlot GetSlot(int index)
    {
        if (index < 0 || index >= slots.Count) return null;
        return slots[index];
    }
    
    public InventorySlot GetFirstEmptySlot()
    {
        foreach (var slot in slots)
        {
            if (slot.IsEmpty() && !slot.isLocked)
                return slot;
        }
        return null;
    }
    
    public InventorySlot GetSlotForItem(ItemBase item)
    {
        foreach (var slot in slots)
        {
            if (slot.CanAddItem(item, 1))
                return slot;
        }
        
        return GetFirstEmptySlot();
    }

    public ItemData GetItemData(int itemID)
    {
        foreach (var slot in slots)
        {
            if (!slot.IsEmpty() && slot.item.data.itemID == itemID)
            {
                return slot.item.data;
            }
        }
        return null;
    }
    
    public int GetItemCount(int itemID)
    {
        int count = 0;
        foreach (var slot in slots)
        {
            if (!slot.IsEmpty() && slot.item.data.itemID == itemID)
            {
                count += slot.quantity;
            }
        }
        return count;
    }
    
    public bool HasItem(int itemID, int quantity = 1)
    {
        return GetItemCount(itemID) >= quantity;
    }
    
    public List<InventorySlot> GetAllSlots()
    {
        return new List<InventorySlot>(slots);
    }
    
    public int GetEmptySlotCount()
    {
        int count = 0;
        foreach (var slot in slots)
        {
            if (slot.IsEmpty() && !slot.isLocked)
                count++;
        }
        return count;
    }

    [Button]
    public void RefreshUI()
    {
        int idx = 0;
        foreach(var slot in slots)
        {
            var slotTransform = transform.GetChild(idx);
            if(slot.IsEmpty()) ClearSlot(slotTransform);
            else UpdateSlot(idx);
            idx++;
        }
    }

    public void UpdateSlot(int idx)
    {
        Transform slotTransform = transform.GetChild(idx);
        InventorySlot slot = slots[idx];
        if (slot.item == null)
        {
            ClearSlot(slotTransform);
            if(idx == selectSlotIdxR)
            {
                GameEventManager.TriggerItemHeld(slot.item, HandType.HandR);
            }
            if(idx == selectSlotIdxL)
            {
                GameEventManager.TriggerItemHeld(slot.item, HandType.HandL);
            }
            return;
        }

        var icon = slotTransform.Find("_Icon").GetComponent<Image>();
        if(slot.item.data.icon != null) icon.sprite = slot.item.data.icon;
        icon.gameObject.SetActive(true);

        var count = slotTransform.Find("_Count").GetComponent<TextMeshProUGUI>();
        count.text = slot.quantity.ToString();
        count.gameObject.SetActive(true);

        if(slot.item.data.itemType == ItemType.Container)
        {
            count.text = ((ItemContainer)slot.item).GetCapacity().ToString();
        }

        if(idx == selectSlotIdxR)
        {
            GameEventManager.TriggerItemHeld(slot.item, HandType.HandR);
            if(selectSlotIdxL == selectSlotIdxR && !CanHeldEachHand(slot))
            {
                GameEventManager.TriggerItemHeld(null, HandType.HandL);
            }
        }
        if(idx == selectSlotIdxL)
        {
            if(selectSlotIdxL != selectSlotIdxR)
                GameEventManager.TriggerItemHeld(slot.item, HandType.HandL);
            else if(CanHeldEachHand(slot))
                GameEventManager.TriggerItemHeld(slot.item, HandType.HandL);
            else
                GameEventManager.TriggerItemHeld(null, HandType.HandL);
        }
    }

    public bool CanHeldEachHand(InventorySlot slot)
    {
        if(slot.item.data.isStackable && slot.quantity > 1) return true;

        return false;
    }

    public void ClearSlot(Transform transform)
    {
        transform.Find("_Icon").gameObject.SetActive(false);
        transform.Find("_Count").gameObject.SetActive(false);
    }

    private void HeldItemConsumed(HandType type)
    {
        if(type == HandType.HandR)
        {
            RemoveItemFromSlot(selectSlotIdxR, 1);
        }
        else if(type == HandType.HandL)
        {
            RemoveItemFromSlot(selectSlotIdxL, 1);
        }
    }
}
