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
    }

    public virtual int GetHealth()
    {
        return health;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    public int GetMaxSatiety()
    {
        return maxSatiety;
    }

    public int GetMaxThirsty()
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
        Debug.Log("Set satiety " + value);
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
}
