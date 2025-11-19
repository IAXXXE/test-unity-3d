using UnityEngine;
using System.Collections.Generic;

public class MechanicalPet : PetBase
{
    [Header("机械属性")]
    public float battery = 100f;
    public float maxBattery = 100f;
    public float durability = 100f;
    public float maxDurability = 100f;
    
    [Header("功能模块")]
    public bool hasScanModule = true;
    public bool hasDefenseModule = false;
    public bool hasStorageModule = true;
    
    private float batteryDrainRate = 2f;  // 电量消耗
    
    protected override void Update()
    {
        base.Update();
        
        // 电量消耗
        if (isFollowing)
        {
            battery = Mathf.Max(0, battery - batteryDrainRate * Time.deltaTime);
        }
        
        // 电量不足时停止跟随
        if (battery <= 0 && isFollowing)
        {
            Debug.Log($"{petName} 电量不足，停止运行");
            StopFollow();
        }
    }
    
    public override InteractionOption[] GetAvailableInteractions()
    {
        List<InteractionOption> options = new List<InteractionOption>();
        
        // 充能
        bool needCharge = battery < maxBattery;
        var chargeOption = new InteractionOption(InteractionType.Charge, "充能", needCharge);
        if (!needCharge)
            chargeOption.unavailableReason = "电量已满";
        options.Add(chargeOption);
        
        // 维修
        bool needRepair = durability < maxDurability;
        var repairOption = new InteractionOption(InteractionType.Repair, "维修", needRepair);
        if (!needRepair)
            repairOption.unavailableReason = "状态良好";
        options.Add(repairOption);
        
        // 命令
        options.Add(new InteractionOption(InteractionType.Command, "指令", true));
        
        // 扫描功能
        if (hasScanModule)
        {
            options.Add(new InteractionOption(InteractionType.CustomAction, "区域扫描", battery > 10));
        }
        
        // 收回
        options.Add(new InteractionOption(InteractionType.Carry, "收回", true));
        
        return options.ToArray();
    }
    
    public override void Interact(InteractionType interactionType)
    {
        switch (interactionType)
        {
            case InteractionType.Charge:
                OnCharge();
                break;
                
            case InteractionType.Repair:
                OnRepair();
                break;
                
            case InteractionType.Command:
                OnCommand();
                break;
                
            case InteractionType.CustomAction:
                OnScan();
                break;
                
            case InteractionType.Carry:
                OnCarry();
                break;
        }
    }
    
    private void OnCharge()
    {
        battery = maxBattery;
        Debug.Log($"{petName} 已充满电");
        
        // 播放充能特效
        StartCoroutine(ChargeEffect());
    }
    
    private void OnRepair()
    {
        durability = maxDurability;
        Debug.Log($"{petName} 维修完成");
        
        // 播放维修特效
    }
    
    private void OnCommand()
    {
        Debug.Log($"向 {petName} 发送指令");
        // 打开命令界面
    }
    
    private void OnScan()
    {
        if (battery < 10)
        {
            Debug.Log("电量不足，无法扫描");
            return;
        }
        
        battery -= 10;
        Debug.Log($"{petName} 正在扫描周围环境...");
        
        // 执行扫描逻辑
        ScanArea();
    }
    
    private void OnCarry()
    {
        Debug.Log($"收回了 {petName}");
        gameObject.SetActive(false);
    }
    
    private void ScanArea()
    {
        // 扫描周围资源/敌人
        Collider[] hits = Physics.OverlapSphere(transform.position, 20f);
        Debug.Log($"扫描到 {hits.Length} 个对象");
    }
    
    private System.Collections.IEnumerator ChargeEffect()
    {
        float duration = 2f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            // 播放充能粒子效果
            yield return null;
        }
    }
}