using UnityEngine;

[System.Serializable]
public class Item
{
    public ItemData data;
    public string instanceID;

    // 构造函数
    public Item(ItemData data)
    {
        this.data = data;
        this.instanceID = System.Guid.NewGuid().ToString();
    }

    // #region 使用逻辑

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

        if (!data.isUsable)
        {
            Debug.LogWarning($"物品 {data.itemName} 不可使用！");
            return false;
        }

        // 根据物品类型执行不同的使用逻辑
        bool useSuccess = false;

        switch (data.itemType)
        {
            case ItemType.Food:
                Debug.Log("Eat " + data.name);
                useSuccess = UseAsFood();
                break;

            case ItemType.Weapon:
                break;

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

        return useSuccess;
    }

    // <summary>
    // 作为食品使用
    // </summary>
    protected virtual bool UseAsFood()
    {
        // 获取玩家状态
        PlayerStat player = FindPlayer();
        Debug.Log($"data.satietyRestore {data.satietyRestore}");
        if (data.satietyRestore > 0)
        {
            player.IncreaseSatiety(data.satietyRestore);
        }
        Debug.Log($"data.thirstyRestore {data.thirstyRestore}");
        if(data.thirstyRestore > 0)
        {
            player.IncreaseThirsty(data.thirstyRestore);
        }
    
        if (data.healthRestore > 0)
        {
            player.Heal(data.healthRestore);
            Debug.Log($"恢复 {data.healthRestore} 点生命值");
        }

    
        if (data.manaRestore > 0)
        {
            player.EnhanceMana(data.manaRestore);
            Debug.Log($"恢复 {data.manaRestore} 点魔法值");
        }

        // 应用持续效果
        if (data.effectDuration > 0)
        {
            ApplyDurationEffect(player);
        }

        return true;
    }

    // <summary>
    // 作为武器使用（装备）
    // </summary>
    protected virtual bool UseAsWeapon()
    {
        

        return true;
    }

    /// <summary>
    /// 作为护甲使用（装备）
    /// </summary>
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

    /// <summary>
    /// 作为任务物品使用
    /// </summary>
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

    /// <summary>
    /// 应用持续效果
    /// </summary>
    protected virtual void ApplyDurationEffect(PlayerStat player)
    {
        // 这里可以实现持续效果逻辑
        // 例如：创建效果管理器，添加定时效果等
        Debug.Log($"应用持续效果，持续时间: {data.effectDuration}秒");

        // 示例：启动协程应用持续效果
        // player.StartCoroutine(ApplyEffectOverTime(player));
    }

    #region 辅助方法

    /// <summary>
    /// 查找玩家状态组件
    /// </summary>
    protected virtual PlayerStat FindPlayer()
    {
        return GameInstance.Instance.PlayerStat;
    }

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

    #endregion
}
