using UnityEngine;
using System.Collections.Generic;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance;
    
    [Header("物品数据列表")]
    public List<ItemData> allItems = new List<ItemData>();
    
    private Dictionary<string, ItemData> itemDictionary = new Dictionary<string, ItemData>();
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDatabase();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void InitializeDatabase()
    {
        foreach (ItemData itemData in allItems)
        {
            if (itemData != null && !string.IsNullOrEmpty(itemData.itemID))
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
    
    public ItemData GetItemData(string itemID)
    {
        if (itemDictionary.ContainsKey(itemID))
        {
            return itemDictionary[itemID];
        }
        
        Debug.LogWarning($"未找到物品: {itemID}");
        return null;
    }
    
    public Item CreateItem(string itemID, int quantity = 1)
    {
        ItemData data = GetItemData(itemID);
        if (data != null)
        {
            var item = new Item(data, quantity);
            return item;
        }
        return null;
    }
    
    public List<ItemData> GetItemsByType(ItemType type)
    {
        List<ItemData> result = new List<ItemData>();
        foreach (var itemData in allItems)
        {
            if (itemData != null && itemData.itemType == type)
            {
                result.Add(itemData);
            }
        }
        return result;
    }
    
    // 编辑器方法：重新加载所有物品
    #if UNITY_EDITOR
    [ContextMenu("重新加载所有物品")]
    void ReloadAllItems()
    {
        // 从Resources文件夹或其他位置加载所有ItemData
        ItemData[] items = Resources.FindObjectsOfTypeAll<ItemData>();
        allItems.Clear();
        allItems.AddRange(items);
        InitializeDatabase();
    }
    #endif
}