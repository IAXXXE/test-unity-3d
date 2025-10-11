using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    public Item item;
    public int quantity;
    public bool isLocked;

    private Image iconImage;

    public InventorySlot()
    {
        item = null;
        quantity = 0;
        isLocked = false;
    }
    
    public InventorySlot(Item item, int quantity)
    {
        this.item = item;
        this.quantity = quantity;
        this.isLocked = false;
    }
    
    public bool IsEmpty()
    {
        return item == null || quantity <= 0;
    }
    
    public bool CanAddItem(Item newItem, int addQuantity = 1)
    {
        if (isLocked) return false;
        if (IsEmpty()) return true;
        Debug.Log("new item id : " + newItem.itemID + " old item id : " + item.itemID);
        if (item.itemID != newItem.itemID) return false;
        return quantity + addQuantity <= item.maxStackSize;
    }
    
    public int GetRemainingSpace()
    {
        if (IsEmpty()) return 0;
        return item.maxStackSize - quantity;
    }
}