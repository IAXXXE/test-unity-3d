using UnityEngine;

// ========== 4. 消耗品行为 ==========
public class ConsumableBehavior : ItemBehavior
{
    private bool isConsuming;
    private float consumeTimer;
    private float totalUseTime;

    public override void OnUse()
    {
        // Use键改为开始使用
        if (isConsuming) return;
        StartConsuming();
    }

    public override void OnPrimaryStart()
    {
        // 主要按键也可以触发使用
        if (isConsuming) return;
        StartConsuming();
    }

    public override void OnPrimaryUpdate(float deltaTime)
    {
        // 按住时持续积累进度
        if (isConsuming)
        {
            consumeTimer += deltaTime;
            float progress = Mathf.Clamp01(consumeTimer / totalUseTime);
            PlayerUI.Instance.UpdateProgressBar(progress);

            // 进度完成
            if (consumeTimer >= totalUseTime)
            {
                CompleteConsume();
            }
        }
    }

    public override void OnPrimaryEnd()
    {
        // 松手时取消使用
        if (isConsuming)
        {
            CancelConsume();
        }
    }

    private void StartConsuming()
    {
        isConsuming = true;
        consumeTimer = 0f;
        totalUseTime = itemData.useTime > 0 ? itemData.useTime : 1f;

        PlayerUI.Instance.ShowProgressBar(true, BarType.Eating);
        PlayerUI.Instance.UpdateProgressBar(0f);

        Debug.Log($"[消耗品] 开始使用: {itemData.itemName} (按住以继续)");

        // TODO: 播放使用动画
        // TODO: 播放使用音效
    }

    private void CompleteConsume()
    {
        isConsuming = false;
        PlayerUI.Instance.ShowProgressBar(false);

        // 触发物品效果
        bool useSuccess = UseItem();

        if (useSuccess)
        {
            Debug.Log($"[消耗品] 使用完成: {itemData.itemName}");
            GameEventManager.TriggerHeldItemConsumed();
        }

        // TODO: 播放完成音效
    }

    private void CancelConsume()
    {
        Debug.Log($"[消耗品] 取消使用: {itemData.itemName} (进度: {consumeTimer / totalUseTime:P0})");
        
        isConsuming = false;
        consumeTimer = 0f;
        
        PlayerUI.Instance.ShowProgressBar(false);

        // TODO: 播放取消音效
    }

    private bool UseItem()
    {
        PlayerStat player = GameInstance.Instance.PlayerStat;
        
        if (itemData.satietyRestore > 0)
        {
            player.IncreaseSatiety(itemData.satietyRestore);
        }
        if (itemData.thirstyRestore > 0)
        {
            player.IncreaseThirsty(itemData.thirstyRestore);
        }
        if (itemData.healthRestore > 0)
        {
            player.Heal(itemData.healthRestore);
        }
        if (itemData.manaRestore > 0)
        {
            player.EnhanceMana(itemData.manaRestore);
        }

        // 应用持续效果
        if (itemData.effectDuration > 0)
        {
            ApplyDurationEffect(player);
        }

        return true;
    }

    /// <summary>
    /// 应用持续效果
    /// </summary>
    protected virtual void ApplyDurationEffect(PlayerStat player)
    {
        Debug.Log($"应用持续效果，持续时间: {itemData.effectDuration}秒");
        // TODO: 实现持续效果系统
    }

    public override void OnSecondaryStart() { }
    public override void OnSecondaryEnd() { }
    public override void OnSecondaryUpdate(float deltaTime) { }
}

// public class ConsumableBehavior : ItemBehavior
// {
//     private bool isConsuming;
//     private float consumeTimer;

//     public override void OnUse()
//     {
//         if (isConsuming) return;
//         StartCoroutine(ConsumeItem());
//     }

//     private System.Collections.IEnumerator ConsumeItem()
//     {
//         isConsuming = true;
//         consumeTimer = 0f;

//         float useTime = itemData.useTime > 0 ? itemData.useTime : 1f;
//         playerUI?.ShowChargeBar(true);

//         Debug.Log($"[消耗品] 开始使用: {itemData.itemName}");

//         while (consumeTimer < useTime)
//         {
//             yield return null;
//             consumeTimer += Time.deltaTime;
//             playerUI?.UpdateChargeBar(consumeTimer / useTime);
//         }

//         playerUI?.ShowChargeBar(false);

//         // 触发物品效果
//         var useSuccess = UseItem();

//         if(useSuccess)
//         {
//             Debug.Log($"[消耗品] 使用完成: {itemData.itemName}");
//             // GameEventManager.TriggerItemConsumed(itemData);
//             GameEventManager.TriggerHeldItemConsumed();
//         }

//         isConsuming = false;
//     }

//     private bool UseItem()
//     {
//         PlayerStat player = GameInstance.Instance.PlayerStat;
//         if (itemData.satietyRestore > 0)
//         {
//             player.IncreaseSatiety(itemData.satietyRestore);
//         }
//         if(itemData.thirstyRestore > 0)
//         {
//             player.IncreaseThirsty(itemData.thirstyRestore);
//         }
    
//         if (itemData.healthRestore > 0)
//         {
//             player.Heal(itemData.healthRestore);
//         }
    
//         if (itemData.manaRestore > 0)
//         {
//             player.EnhanceMana(itemData.manaRestore);
//         }

//         // 应用持续效果
//         if (itemData.effectDuration > 0)
//         {
//             ApplyDurationEffect(player);
//         }

//         return true;
//     }

//     /// <summary>
//     /// 应用持续效果
//     /// </summary>
//     protected virtual void ApplyDurationEffect(PlayerStat player)
//     {
//         // 这里可以实现持续效果逻辑
//         // 例如：创建效果管理器，添加定时效果等
//         Debug.Log($"应用持续效果，持续时间: {itemData.effectDuration}秒");

//         // 示例：启动协程应用持续效果
//         // player.StartCoroutine(ApplyEffectOverTime(player));
//     }

//     public override void OnPrimaryStart() { }
//     public override void OnPrimaryEnd() { }
//     public override void OnPrimaryUpdate(float deltaTime) { }
//     public override void OnSecondaryStart() { }
//     public override void OnSecondaryEnd() { }
//     public override void OnSecondaryUpdate(float deltaTime) { }
// }
