using UnityEngine;
using System.Collections.Generic;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance;

    [Header("物品数据列表")]
    public ItemDataList_SO itemDataList;
    
    private Dictionary<int, ItemData> itemDictionary = new Dictionary<int, ItemData>();

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

    void Start()
    {
        InitializeDatabase();
    }

    void InitializeDatabase()
    {
        foreach (ItemData itemData in itemDataList.itemDataList)
        {
            if (itemData != null)
            {
                if (!itemDictionary.ContainsKey(itemData.itemID))
                {
                    itemDictionary.Add(itemData.itemID, itemData);
                }
                else
                {
                    Debug.LogWarning($"重复的物品ID: {itemData.itemID}");
                }
            }
        }
        
        Debug.Log($"物品数据库初始化完成，共加载 {itemDictionary.Count} 个物品");
    }
    
    public ItemData GetItemData(int itemID)
    {
        if (itemDictionary.ContainsKey(itemID))
        {
            return itemDictionary[itemID];
        }
        
        Debug.LogWarning($"未找到物品: {itemID}");
        return null;
    }
    
    public ItemBase CreateItem(int itemID, int quantity = 1)
    {
        ItemData data = GetItemData(itemID);
        if (data == null) return null;

        ItemBase item = null;
        switch (data.itemType)
        {
            case ItemType.Container:
                item = new ItemContainer(data);
                break;
            case ItemType.Food:
            case ItemType.Potion:
            case ItemType.Weapon:
            case ItemType.Armor:
            case ItemType.Material:
            case ItemType.Quest:
            case ItemType.Misc:
            case ItemType.Props:
            case ItemType.None:
            default:
                item = new ItemBase(data);
                break;
        }

        return item;
    }

    public List<ItemData> GetItemsByType(ItemType type)
    {
        List<ItemData> result = new List<ItemData>();
        foreach (var itemData in itemDataList.itemDataList)
        {
            if (itemData != null && itemData.itemType == type)
            {
                result.Add(itemData);
            }
        }
        return result;
    }
    
    // 编辑器方法：重新加载所有物品
//     #if UNITY_EDITOR
//     [ContextMenu("重新加载所有物品")]
//     void ReloadAllItems()
//     {
//         // 从Resources文件夹或其他位置加载所有ItemData
//         ItemData[] items = Resources.FindObjectsOfTypeAll<ItemData>();
//         allItems.Clear();
//         allItems.AddRange(items);
//         InitializeDatabase();
//     }
// #endif
}