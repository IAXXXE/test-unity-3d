using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    public ItemBase item;
    public int quantity;
    public bool isLocked;

    private Image iconImage;

    public InventorySlot()
    {
        item = null;
        quantity = 0;
        isLocked = false;
    }
    
    public InventorySlot(ItemBase item, int quantity)
    {
        this.item = item;
        this.quantity = quantity;
        this.isLocked = false;
    }
    
    public bool IsEmpty()
    {
        if(item == null) return true;
        if(item.data.itemType == ItemType.Container) return false;
        return quantity <= 0;
    }
    
    public bool CanAddItem(ItemBase newItem, int addQuantity = 1)
    {
        if (isLocked) return false;
        if (IsEmpty()) return true;
        if (item.data.itemID != newItem.data.itemID) return false;
        return quantity + addQuantity <= item.data.maxStackSize;
    }
    
    public int GetRemainingSpace()
    {
        if (IsEmpty()) return 0;
        return item.data.maxStackSize - quantity;
    }
}