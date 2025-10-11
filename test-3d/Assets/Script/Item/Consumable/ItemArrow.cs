using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemArrow : Item
{
    // Start is called before the first frame update
    public ItemArrow()
    {
        base.itemID = "1";
        base.itemName = "arrow";
        base.itemType = ItemType.Consumable;
        base.maxStackSize = 20;
        base.description = "desc";
    }

    
}
