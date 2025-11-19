using UnityEngine;
using System.Collections.Generic;

public class ElementalPet : PetBase
{
    [Header("元素属性")]
    public ElementType elementType;
    public float elementalEnergy = 100f;
    public float maxElementalEnergy = 100f;
    
    [Header("元素能力")]
    public bool canProvideLight = true;
    public bool canProvideWarmth = false;
    public bool canProvideShield = false;
    
    public enum ElementType
    {
        Fire,
        Water,
        Earth,
        Air,
        Light,
        Dark
    }
    
    protected override void Update()
    {
        base.Update();
        
        // 根据环境恢复/消耗能量
        UpdateElementalEnergy();
    }
    
    private void UpdateElementalEnergy()
    {
        // 示例：白天光元素恢复能量，夜晚消耗能量
        // 实际逻辑根据游戏需求实现
    }
    
    public override InteractionOption[] GetAvailableInteractions()
    {
        List<InteractionOption> options = new List<InteractionOption>();
        
        // 交流
        options.Add(new InteractionOption(InteractionType.Talk, "交流", true));
        
        // 充能
        bool needCharge = elementalEnergy < maxElementalEnergy;
        options.Add(new InteractionOption(InteractionType.Charge, "注入元素能量", needCharge));
        
        // 元素能力
        if (canProvideLight)
        {
            options.Add(new InteractionOption(InteractionType.CustomAction, "光明", elementalEnergy > 20));
        }
        
        if (canProvideShield)
        {
            options.Add(new InteractionOption(InteractionType.CustomAction, "护盾", elementalEnergy > 30));
        }
        
        // 收回
        options.Add(new InteractionOption(InteractionType.Carry, "收回", true));
        
        return options.ToArray();
    }
    
    public override void Interact(InteractionType interactionType)
    {
        switch (interactionType)
        {
            case InteractionType.Talk:
                OnTalk();
                break;
                
            case InteractionType.Charge:
                OnCharge();
                break;
                
            case InteractionType.CustomAction:
                OnUseAbility();
                break;
                
            case InteractionType.Carry:
                OnCarry();
                break;
        }
    }
    
    private void OnTalk()
    {
        Debug.Log($"与 {petName} 交流");
        // 显示对话界面
    }
    
    private void OnCharge()
    {
        elementalEnergy = maxElementalEnergy;
        Debug.Log($"{petName} 元素能量已满");
    }
    
    private void OnUseAbility()
    {
        if (canProvideLight && elementalEnergy >= 20)
        {
            elementalEnergy -= 20;
            ProvideLight();
        }
        else if (canProvideShield && elementalEnergy >= 30)
        {
            elementalEnergy -= 30;
            ProvideShield();
        }
    }
    
    private void ProvideLight()
    {
        Debug.Log($"{petName} 提供照明");
        // 创建光源效果
    }
    
    private void ProvideShield()
    {
        Debug.Log($"{petName} 生成护盾");
        // 给玩家添加护盾效果
    }
    
    private void OnCarry()
    {
        Debug.Log($"收回了 {petName}");
        gameObject.SetActive(false);
    }
}
