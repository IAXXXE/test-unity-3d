using UnityEngine;

[System.Serializable]
public class Item
{
    public ItemData data;
    public int quantity;
    public string instanceID;

    // 构造函数
    public Item(ItemData data, int quantity = 1)
    {
        this.data = data;
        this.quantity = quantity;
        this.instanceID = System.Guid.NewGuid().ToString();
    }

    #region 使用逻辑

    /// <summary>
    /// 使用物品
    /// </summary>
    /// <returns>使用是否成功</returns>
    public virtual bool Use()
    {
        if (data == null)
        {
            Debug.LogWarning("物品数据为空！");
            return false;
        }

        if (quantity <= 0)
        {
            Debug.LogWarning("物品数量不足！");
            return false;
        }

        if (!data.isUsable)
        {
            Debug.LogWarning($"物品 {data.itemName} 不可使用！");
            return false;
        }

        // 根据物品类型执行不同的使用逻辑
        bool useSuccess = false;

        switch (data.itemType)
        {
            case ItemType.Consumable:
                useSuccess = UseAsConsumable();
                break;

            // case ItemType.Weapon:
            //     useSuccess = UseAsWeapon();
            //     break;

            // case ItemType.Armor:
            //     useSuccess = UseAsArmor();
            //     break;

            // case ItemType.Quest:
            //     useSuccess = UseAsQuestItem();
            //     break;

            case ItemType.Material:
                Debug.Log($"材料物品 {data.itemName} 通常用于合成");
                useSuccess = true;
                break;

            case ItemType.Misc:
                Debug.Log($"杂项物品 {data.itemName} 被使用");
                useSuccess = true;
                break;

            default:
                Debug.LogWarning($"未知的物品类型: {data.itemType}");
                break;
        }

        // 如果使用成功且是消耗品，减少数量
        if (useSuccess && data.itemType == ItemType.Consumable)
        {
            quantity--;
            Debug.Log($"使用消耗品成功，剩余数量: {quantity}");
        }

        return useSuccess;
    }

    /// <summary>
    /// 作为消耗品使用
    /// </summary>
    protected virtual bool UseAsConsumable()
    {
        if (data.healthRestore <= 0 && data.manaRestore <= 0)
        {
            Debug.LogWarning($"消耗品 {data.itemName} 没有设置恢复效果！");
            return false;
        }
        return true;

        // 获取玩家状态
        // PlayerStats player = FindPlayer();
        // if (player == null)
        // {
        //     Debug.LogWarning("未找到玩家状态组件！");
        //     return false;
        // }

        // bool effectApplied = false;

        // // 恢复生命值
        // if (data.healthRestore > 0)
        // {
        //     player.RestoreHealth(data.healthRestore);
        //     Debug.Log($"恢复 {data.healthRestore} 点生命值");
        //     effectApplied = true;
        // }

        // // 恢复魔法值
        // if (data.manaRestore > 0)
        // {
        //     player.RestoreMana(data.manaRestore);
        //     Debug.Log($"恢复 {data.manaRestore} 点魔法值");
        //     effectApplied = true;
        // }

        // // 应用持续效果
        // if (data.effectDuration > 0 && effectApplied)
        // {
        //     ApplyDurationEffect(player);
        // }

        // return effectApplied;
    }

    // /// <summary>
    // /// 作为武器使用（装备）
    // /// </summary>
    // protected virtual bool UseAsWeapon()
    // {
    //     EquipmentManager equipmentManager = FindEquipmentManager();
    //     if (equipmentManager == null)
    //     {
    //         Debug.LogWarning("未找到装备管理器！");
    //         return false;
    //     }

    //     bool equipped = equipmentManager.EquipWeapon(this);
    //     if (equipped)
    //     {
    //         Debug.Log($"装备武器: {data.itemName}, 伤害: {data.damage}");
    //     }

    //     return equipped;
    // }

    // /// <summary>
    // /// 作为护甲使用（装备）
    // /// </summary>
    // protected virtual bool UseAsArmor()
    // {
    //     EquipmentManager equipmentManager = FindEquipmentManager();
    //     if (equipmentManager == null)
    //     {
    //         Debug.LogWarning("未找到装备管理器！");
    //         return false;
    //     }

    //     bool equipped = equipmentManager.EquipArmor(this);
    //     if (equipped)
    //     {
    //         Debug.Log($"装备护甲: {data.itemName}, 防御: {data.defense}");
    //     }

    //     return equipped;
    // }

    // /// <summary>
    // /// 作为任务物品使用
    // /// </summary>
    // protected virtual bool UseAsQuestItem()
    // {
    //     QuestManager questManager = FindQuestManager();
    //     if (questManager == null)
    //     {
    //         Debug.LogWarning("未找到任务管理器！");
    //         return false;
    //     }

    //     bool usedInQuest = questManager.UseQuestItem(this);
    //     if (usedInQuest)
    //     {
    //         Debug.Log($"任务物品 {data.itemName} 被用于任务");
    //         quantity--; // 任务物品使用后通常消失
    //     }

    //     return usedInQuest;
    // }

    // /// <summary>
    // /// 应用持续效果
    // /// </summary>
    // protected virtual void ApplyDurationEffect(PlayerStats player)
    // {
    //     // 这里可以实现持续效果逻辑
    //     // 例如：创建效果管理器，添加定时效果等
    //     Debug.Log($"应用持续效果，持续时间: {data.effectDuration}秒");

    //     // 示例：启动协程应用持续效果
    //     // player.StartCoroutine(ApplyEffectOverTime(player));
    // }

    #endregion

    // #region 辅助方法

    // /// <summary>
    // /// 查找玩家状态组件
    // /// </summary>
    // protected virtual PlayerStats FindPlayer()
    // {
    //     return GameObject.FindObjectOfType<PlayerStats>();
    // }

    // /// <summary>
    // /// 查找装备管理器
    // /// </summary>
    // protected virtual EquipmentManager FindEquipmentManager()
    // {
    //     return GameObject.FindObjectOfType<EquipmentManager>();
    // }

    // /// <summary>
    // /// 查找任务管理器
    // /// </summary>
    // protected virtual QuestManager FindQuestManager()
    // {
    //     return GameObject.FindObjectOfType<QuestManager>();
    // }

    // #endregion

    #region 物品管理方法

    public bool IsStackable()
    {
        return data != null && data.isStackable;
    }

    public bool CanStackWith(Item other)
    {
        if (data == null || other.data == null) return false;
        return data.itemID == other.data.itemID && IsStackable();
    }

    public int GetRemainingStackSpace()
    {
        if (data == null) return 0;
        return data.maxStackSize - quantity;
    }

    public Item Split(int splitQuantity)
    {
        if (quantity <= splitQuantity || splitQuantity <= 0) return null;

        quantity -= splitQuantity;
        return new Item(data, splitQuantity);
    }

    public void Merge(Item other)
    {
        if (!CanStackWith(other)) return;

        int total = quantity + other.quantity;
        int maxStack = data.maxStackSize;

        if (total <= maxStack)
        {
            quantity = total;
            other.quantity = 0;
        }
        else
        {
            quantity = maxStack;
            other.quantity = total - maxStack;
        }
    }

    public string GetInfo()
    {
        if (data == null) return "无效物品";

        return $"{data.itemName} x{quantity}\n{data.description}";
    }

    #endregion
}
