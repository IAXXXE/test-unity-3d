using UnityEngine;

[System.Serializable]
public class Item
{
    public string itemID;
    public string itemName;
    public ItemType itemType;
    public Sprite icon;
    public int maxStackSize = 20;
    public string description;
    
 
    public virtual void Use()
    {
 
    }
}

public enum ItemType
{
    Consumable,
    Weapon,
    Armor,
    Material,
    Quest,
    Misc
}