using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureStat : ICharacterStat
{
    public CharacterData data;

    public string id;

    protected int health;
    protected int maxHealth;
    protected int satiety;
    protected int maxSatiety;
    protected int thirsty;
    protected int maxThirsty;

    private float speed;

    [Header("情绪值")]
    protected float angerValue;      // 愤怒值 0-100
    protected float fearValue;       // 恐惧值 0-100

    public CreaturePersonality personality;

    public CreatureStat(string id)
    {
        data = CharacterDatabase.Instance.GetCharacterData(id);

        id = data.id;

        health = data.maxHealth;
        maxHealth = data.maxHealth;
        satiety = data.maxHealth;
        maxSatiety = data.maxSatiety;
        thirsty = data.maxThirsty;
        maxThirsty = data.maxThirsty;

        speed = data.speed;

        // 初始化性格（如果未提供则使用默认）
        this.personality = personality ?? new CreaturePersonality(data.raceType);
        angerValue = 0f;
        fearValue = 0f;
    }

    public virtual int GetHealth()
    {
        return health;
    }

    public virtual int GetMaxHealth()
    {
        return maxHealth;
    }

    public virtual int GetMaxSatiety()
    {
        return maxSatiety;
    }

    public virtual int GetMaxThirsty()
    {
        return maxThirsty;
    }

    public virtual int GetSatiety()
    {
        return satiety;
    }

    public virtual int GetThirsty()
    {
        return thirsty;
    }

    public virtual void Heal(int value)
    {
        var newHealth = health + value;
        SetHealth(newHealth);
    }

    public virtual void IncreaseMaxHealth(int value)
    {
        maxHealth += value;
        health += value;
    }

    public virtual void IncreaseMaxSatiety(int value)
    {
        maxSatiety += value;
        satiety += value;
    }

    public virtual void IncreaseMaxThirsty(int value)
    {
        maxThirsty += value;
        thirsty += value;
    }

    public virtual void IncreaseSatiety(int value)
    {
        var newSatiety = satiety + value;
        SetSatiety(newSatiety);
    }

    public virtual void IncreaseThirsty(int value)
    {
        var newThirsty = thirsty + value;
        SetThirsty(newThirsty);
    }

    public virtual void LoseHealth(int value)
    {
        var newHealth = health - value;
        SetHealth(newHealth);
    }

    public virtual void LoseSatiety(int value)
    {
        var newSatiety = satiety - value;
        SetSatiety(newSatiety);
    }

    public virtual void LoseThirsty(int value)
    {
        var newThirsty = thirsty - value;
        SetThirsty(newThirsty);
    }

    public virtual void SetHealth(int value)
    {
        if(value < 0) value = 0;
        if(value > maxHealth) value = maxHealth;

        health = value;
    }

    public virtual void SetSatiety(int value)
    {
        if(value < 0) value = 0;
        if(value > maxSatiety) value = maxSatiety;
        satiety = value;
    }

    public virtual void SetThirsty(int value)
    {
        if(value < 0) value = 0;
        if(value > maxThirsty) value = maxThirsty;

        thirsty = value;
    }

    public virtual float GetSpeed()
    {
        return speed;
    }

    // 情绪管理
    public void IncreaseAnger(float value)
    {
        angerValue = Mathf.Clamp(angerValue + value, 0f, 100f);
        Debug.Log($"愤怒值增加: {angerValue}");
    }
    
    public void IncreaseFear(float value)
    {
        fearValue = Mathf.Clamp(fearValue + value, 0f, 100f);
        Debug.Log($"恐惧值增加: {fearValue}");
    }
    
    public void DecayEmotions(float deltaTime)
    {
        angerValue = Mathf.Max(0f, angerValue - personality.angerDecayRate * deltaTime);
        fearValue = Mathf.Max(0f, fearValue - personality.fearDecayRate * deltaTime);
    }
    
    public float GetAnger() => angerValue;
    public float GetFear() => fearValue;
    
    // 判断情绪状态
    public EmotionState GetEmotionState()
    {
        // 愤怒和恐惧同时存在时，根据性格决定
        if (angerValue >= personality.angerThreshold && fearValue >= personality.fearThreshold)
        {
            // 攻击性高的生物选择战斗
            return personality.aggressiveness > 0.5f ? EmotionState.Angry : EmotionState.Afraid;
        }
        
        if (angerValue >= personality.angerThreshold)
            return EmotionState.Angry;
            
        if (fearValue >= personality.fearThreshold)
            return EmotionState.Afraid;
            
        return EmotionState.Calm;
    }
    
    // 重置情绪
    public void ResetEmotions()
    {
        angerValue = 0f;
        fearValue = 0f;
    }
}
