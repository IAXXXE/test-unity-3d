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
    [Header("持有时显示偏移设置")]
    public Vector3 posOffset;
    public Vector3 rotOffset;
    public Vector3 scale;

    [Header("堆叠设置")]
    public int maxStackSize = 20;
    public bool isStackable = true;

    [Header("价值设置")]
    public int buyPrice = 10;
    public int sellPrice = 5;

    [Header("使用设置")]
    public bool isUsable = true;
    public bool isConsumable = true;
    public float useTime = 0f;

    [Header("效果数值 - 食物")]
    public int satietyRestore = 0;
    public int thirstyRestore = 0;

    [Header("效果数值 - 药品")]
    public int healthRestore = 0;
    public int manaRestore = 0;
    public float effectDuration = 0f;


    [Header("武器 - ")]
    public WeaponType weaponType;
    public float damage = 0f;
    public float damageMultiplier = 1;

    [Header("装备 - ")]
    public float defense = 0f;
}

public enum ItemType
{
    Food,
    Weapon,
    Armor,
    Material,
    Quest,
    Misc
}

public enum WeaponType
{
    Stick,
    Sword,
    Bow,
}