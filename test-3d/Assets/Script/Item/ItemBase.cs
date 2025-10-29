using UnityEngine;

[System.Serializable]
public class ItemBase
{
    public ItemData data;
    public string instanceID;

    // 构造函数
    public ItemBase(ItemData data)
    {
        this.data = data;
        this.instanceID = System.Guid.NewGuid().ToString();
    }

    
}
