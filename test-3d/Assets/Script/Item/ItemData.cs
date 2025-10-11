using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("基础信息")]
    public string itemID;
    public string itemName;
    public ItemType itemType;
    [TextArea(3, 5)]
    public string description;
    
    [Header("显示设置")]
    public Sprite icon;
    public GameObject worldPrefab;
    
    [Header("堆叠设置")]
    public int maxStackSize = 20;
    public bool isStackable = true;
    
    [Header("价值设置")]
    public int buyPrice = 10;
    public int sellPrice = 5;
    
    [Header("使用设置")]
    public bool isUsable = true;
    
    [Header("效果数值")]
    public float healthRestore = 0f;
    public float manaRestore = 0f;
    public float effectDuration = 0f;
    public float damage = 0f;
    public float defense = 0f;
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