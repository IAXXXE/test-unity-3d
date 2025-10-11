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
    }

    [Button]
    public bool AddItem(string itemId, int quantity = 1)
    {
        Debug.Log($"add {quantity} {itemId}");
        return AddItem(ItemDatabase.Instance.CreateItem(itemId), quantity);
    }

    [Button]
    public bool AddItem(Item item, int quantity = 1)
    {
        if (item == null || quantity <= 0) return false;
        
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].CanAddItem(item, quantity))
            {
                Debug.Log("can add slot i :" + i);
                if (slots[i].IsEmpty())
                {
                    slots[i].item = item;
                    slots[i].quantity = quantity;
                }
                else
                {
                    slots[i].quantity += quantity;
                }
                UpdateSlot(transform.GetChild(i), slots[i]);
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
    
    private bool AddItemInMultipleSlots(Item item, int totalQuantity)
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
    
    public bool RemoveItem(string itemID, int quantity = 1)
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
            }
        }
        
        return remainingToRemove == 0;
    }
    
    public bool RemoveItemFromSlot(int slotIndex, int quantity = 1)
    {
        if (slotIndex < 0 || slotIndex >= slots.Count) return false;
        if (slots[slotIndex].IsEmpty()) return false;
        if (slots[slotIndex].quantity < quantity) return false;
        
        slots[slotIndex].quantity -= quantity;
        if (slots[slotIndex].quantity <= 0)
        {
            slots[slotIndex].item = null;
            slots[slotIndex].quantity = 0;
        }
        
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
    
    public InventorySlot GetSlotForItem(Item item)
    {
        foreach (var slot in slots)
        {
            if (slot.CanAddItem(item, 1))
                return slot;
        }
        
        return GetFirstEmptySlot();
    }
    
    public int GetItemCount(string itemID)
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
    
    public bool HasItem(string itemID, int quantity = 1)
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
            else UpdateSlot(slotTransform, slot);
            idx++;
        }
    }

    public void UpdateSlot(Transform transform, InventorySlot slot)
    {
        var icon = transform.Find("_Icon").GetComponent<Image>();
        if(slot.item.data.icon != null) icon.sprite = slot.item.data.icon;
        icon.gameObject.SetActive(true);

        var count = transform.Find("_Count").GetComponent<TextMeshProUGUI>();
        count.text = slot.quantity.ToString();
        count.gameObject.SetActive(true);
    }

    public void ClearSlot(Transform transform)
    {
        transform.Find("_Icon").gameObject.SetActive(false);
        transform.Find("_Count").gameObject.SetActive(false);
    }
}
