using System.Collections;
using UnityEngine;

public class ContainerBehavior : ItemBehavior
{
    ItemContainer itemContainer;

    private bool isUsing;
    private float usingTimer;
    private float totalUseTime;

    void Start()
    {
        itemContainer = item as ItemContainer;
    }


    public override void OnUse()
    {
        // Use键改为开始使用
        if (isUsing || itemContainer.IsEmpty())
        {
            Debug.Log("空");
            return;
        }
        StartDrinking();
    }

    public override void OnPrimaryStart()
    {
        // 主要按键也可以触发使用
        if (isUsing || itemContainer.IsEmpty()) return;
        StartDrinking();
    }

    public override void OnPrimaryUpdate(float deltaTime)
    {
        // 按住时持续积累进度
        if (isUsing)
        {
            usingTimer += deltaTime;
            float progress = Mathf.Clamp01(usingTimer / totalUseTime);
            PlayerUI.Instance.UpdateProgressBar(progress);

            // 进度完成
            if (usingTimer >= totalUseTime)
            {
                CompleteDrink();
            }
        }
    }

    public override void OnPrimaryEnd()
    {
        // 松手时取消使用
        if (isUsing)
        {
            CancelDrink();
        }
    }

    private void StartDrinking()
    {
        isUsing = true;
        usingTimer = 0f;
        totalUseTime = itemData.useTime > 0 ? itemData.useTime : 1f;

        PlayerUI.Instance.ShowProgressBar(true, BarType.Drinking);
        PlayerUI.Instance.UpdateProgressBar(0f);

        Debug.Log($"[消耗品] 开始使用: {itemData.itemName} (按住以继续)");

        // TODO: 播放使用动画
        // TODO: 播放使用音效
    }

    private void CompleteDrink()
    {
        isUsing = false;
        PlayerUI.Instance.ShowProgressBar(false);

        // 触发物品效果
        Debug.Log($"[消耗品] 使用完成: {itemData.itemName}");
        itemContainer.Drink();
        switch(itemContainer.GetFillingType())
        {
            case FillingType.Water:
                Drink(ItemDatabase.Instance.GetItemData(100018));
                break;
        }

        // TODO: 播放完成音效
    }

    private bool Drink(ItemData data)
    {
        PlayerStat player = GameInstance.Instance.PlayerStat;
        
        if (data.satietyRestore > 0)
        {
            player.IncreaseSatiety(data.satietyRestore);
        }
        if (data.thirstyRestore > 0)
        {
            player.IncreaseThirsty(data.thirstyRestore);
        }
        if (data.healthRestore > 0)
        {
            player.Heal(data.healthRestore);
        }
        if (data.manaRestore > 0)
        {
            player.EnhanceMana(data.manaRestore);
        }

        // 应用持续效果
        if (data.effectDuration > 0)
        {
            ApplyDurationEffect(player);
        }

        return true;
    }

    private void CancelDrink()
    {
        Debug.Log($"[消耗品] 取消使用: {itemData.itemName} (进度: {usingTimer / totalUseTime:P0})");
        
        isUsing = false;
        usingTimer = 0f;
        
        PlayerUI.Instance.ShowProgressBar(false);

        // TODO: 播放取消音效
    }


    /// <summary>
    /// 应用持续效果
    /// </summary>
    protected virtual void ApplyDurationEffect(PlayerStat player)
    {
        // 这里可以实现持续效果逻辑
        // 例如：创建效果管理器，添加定时效果等
        Debug.Log($"应用持续效果，持续时间: {itemData.effectDuration}秒");

        // 示例：启动协程应用持续效果
        // player.StartCoroutine(ApplyEffectOverTime(player));
    }
    public override void OnSecondaryStart()
    {
        if (isUsing || itemContainer.IsEmpty()) return;
        StartPouring();
    }
    public override void OnSecondaryUpdate(float deltaTime)
    {
        // 按住时持续积累进度
        if (isUsing)
        {
            usingTimer += deltaTime;
            float progress = Mathf.Clamp01(usingTimer / totalUseTime);
            PlayerUI.Instance.UpdateProgressBar(progress);

            // 进度完成
            if (usingTimer >= totalUseTime)
            {
                CompletePouring();
            }
        }
    }
    public override void OnSecondaryEnd()
    {

    }
    private void StartPouring()
    {
        isUsing = true;
        usingTimer = 0f;
        totalUseTime = itemData.useTime > 0 ? itemData.useTime : 1f;

        PlayerUI.Instance.ShowProgressBar(true, BarType.Using);
        PlayerUI.Instance.UpdateProgressBar(0f);

        Debug.Log($"[容器] 开始倾倒");

        // TODO: 播放使用动画
        // TODO: 播放使用音效
    }

    private void CompletePouring()
    {
        isUsing = false;
        PlayerUI.Instance.ShowProgressBar(false);

        Debug.Log($"[容器] 倾倒完成");
        itemContainer.Pour();
        // TODO: 播放完成音效
    }
}
