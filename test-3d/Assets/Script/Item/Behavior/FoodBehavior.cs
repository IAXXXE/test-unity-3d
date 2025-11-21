using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// === 食物行为
public class FoodBehavior : ItemBehavior
{
    private bool isEating;
    private float eatTimer;
    private float totalEatTime;

    public override void OnUse()
    {
        if (isEating) return;
        StartEating();
    }

    public override void OnPrimaryStart()
    {
        if (isEating) return;
        StartEating();
    }

    public override void OnPrimaryUpdate(float deltaTime)
    {
        // 按住时持续积累进度
        if (isEating)
        {
            eatTimer += deltaTime;
            float progress = Mathf.Clamp01(eatTimer / totalEatTime);
            PlayerUI.Instance.UpdateProgressBar(progress);

            // 进度完成
            if (eatTimer >= totalEatTime)
            {
                CompleteEat();
            }
        }
    }

    public override void OnPrimaryEnd()
    {
        // 松手时取消使用
        if (isEating)
        {
            CancelEat();
        }
    }

    private void StartEating()
    {
        isEating = true;
        eatTimer = 0f;
        totalEatTime = itemData.useTime > 0 ? itemData.useTime : 1f;

        PlayerUI.Instance.ShowProgressBar(true, BarType.Eating);
        PlayerUI.Instance.UpdateProgressBar(0f);

        Debug.Log($"[食物] 开始食用: {itemData.itemName} (按住以继续)");

        // TODO: 播放使用动画
        // TODO: 播放使用音效
    }

    private void CompleteEat()
    {
        isEating = false;
        PlayerUI.Instance.ShowProgressBar(false);

        // 触发物品效果
        bool useSuccess = UseItem();

        if (useSuccess)
        {
            Debug.Log($"[食物] 食用完成: {itemData.itemName}");
            GameEventManager.TriggerHeldItemConsumed();
        }

        // TODO: 播放完成音效
    }

    private void CancelEat()
    {
        Debug.Log($"[[食物] 取消食用: {itemData.itemName} (进度: {eatTimer / totalEatTime:P0})");
        
        isEating = false;
        eatTimer = 0f;
        
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

    // Feed
    public bool isFeeding;
    public override void OnSecondaryStart()
    {
        if(isFeeding) return;
        StartFeeding();
    }

    public override void OnSecondaryEnd()
    {
        isFeeding = false;
        GameEventManager.TriggerPlayerFeed(false);
    }

    public override void OnSecondaryUpdate(float deltaTime)
    {
        if(item == null) GameEventManager.TriggerPlayerFeed(false);
    }

    private void StartFeeding()
    {
        isFeeding = true;
        GameEventManager.TriggerPlayerFeed(true);
        var pets = PetManager.Instance.ownedPets;
        if(pets.Count == 0) return;
        foreach(PetBase pet in pets)
        {
            pet.TryFeed();
        }
    }
}
