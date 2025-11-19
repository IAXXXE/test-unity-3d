using UnityEngine;
using System.Collections.Generic;

public class CreaturePet : PetBase
{
    [Header("生物属性")]
    public int hunger = 100;
    public int maxHunger = 100;
    public int affection = 50;      // 亲密度
    public int maxAffection = 100;
    public int energy = 100;
    public int maxEnergy = 100;
    
    [Header("互动效果")]
    public int petAffectionGain = 5;
    public int feedHungerGain = 30;
    public int playEnergyLoss = 20;
    public int playAffectionGain = 10;
    
    [Header("特殊能力")]
    public bool canRide = false;
    public bool canCarryItems = false;
    
    private float hungerDecayRate = 1f;  // 每秒饥饿度下降
    private float energyDecayRate = 0.5f;
    
    protected override void Update()
    {
        base.Update();
        
        // 随时间消耗
        hunger = Mathf.Max(0, hunger - (int)(hungerDecayRate * Time.deltaTime));
        energy = Mathf.Max(0, energy - (int)(energyDecayRate * Time.deltaTime));
    }
    
    public override InteractionOption[] GetAvailableInteractions()
    {
        List<InteractionOption> options = new List<InteractionOption>();
        
        // 抚摸
        options.Add(new InteractionOption(InteractionType.Pet, "抚摸", true));
        
        // 喂食
        bool canFeed = hunger < maxHunger;
        var feedOption = new InteractionOption(InteractionType.Feed, "喂食", canFeed);
        if (!canFeed)
            feedOption.unavailableReason = "不饿";
        options.Add(feedOption);
        
        // 玩耍
        bool canPlay = energy >= 20;
        var playOption = new InteractionOption(InteractionType.Play, "玩耍", canPlay);
        if (!canPlay)
            playOption.unavailableReason = "太累了";
        options.Add(playOption);
        
        // 骑乘
        if (canRide)
        {
            options.Add(new InteractionOption(InteractionType.Ride, "骑乘", true));
        }
        
        // 携带/收回
        options.Add(new InteractionOption(InteractionType.Carry, "收回", true));
        
        return options.ToArray();
    }
    
    public override void Interact(InteractionType interactionType)
    {
        switch (interactionType)
        {
            case InteractionType.Pet:
                OnPet();
                break;
                
            case InteractionType.Feed:
                OnFeed();
                break;
                
            case InteractionType.Play:
                OnPlay();
                break;
                
            case InteractionType.Ride:
                OnRide();
                break;
                
            case InteractionType.Carry:
                OnCarry();
                break;
        }
    }
    
    private void OnPet()
    {
        affection = Mathf.Min(maxAffection, affection + petAffectionGain);
        Debug.Log($"抚摸了 {petName}，亲密度 +{petAffectionGain}");
        
        // 播放抚摸动画
        if (animator != null)
            animator.SetTrigger("Happy");
            
        // 播放音效/粒子特效
    }
    
    private void OnFeed()
    {
        if (hunger >= maxHunger)
        {
            Debug.Log($"{petName} 不饿");
            return;
        }
        
        hunger = Mathf.Min(maxHunger, hunger + feedHungerGain);
        affection = Mathf.Min(maxAffection, affection + 3);
        
        Debug.Log($"喂食了 {petName}，饥饿度 +{feedHungerGain}");
        
        // 播放进食动画
        if (animator != null)
            animator.SetTrigger("Eat");
    }
    
    private void OnPlay()
    {
        if (energy < 20)
        {
            Debug.Log($"{petName} 太累了");
            return;
        }
        
        energy = Mathf.Max(0, energy - playEnergyLoss);
        affection = Mathf.Min(maxAffection, affection + playAffectionGain);
        
        Debug.Log($"和 {petName} 玩耍，亲密度 +{playAffectionGain}");
        
        // 播放玩耍动画
        if (animator != null)
            animator.SetTrigger("Play");
    }
    
    private void OnRide()
    {
        Debug.Log($"骑上了 {petName}");
        // 实现骑乘逻辑
    }
    
    private void OnCarry()
    {
        Debug.Log($"收回了 {petName}");
        // 收回宠物
        gameObject.SetActive(false);
    }
}