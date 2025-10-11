using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemStick : Item
{
    // Start is called before the first frame update
    public ItemStick()
    {
        base.itemID = "2";
        base.itemName = "stick";
        base.itemType = ItemType.Consumable;
        base.maxStackSize = 20;
        base.description = "stick";
    }

    
}
