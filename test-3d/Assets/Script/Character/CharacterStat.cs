using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStat : CreatureStat
{
    protected int vitality;
    protected int strength;
    protected int dexterity;
    protected int intelligence;
    protected int mana;

    public CharacterStat(string id) : base(id)
    {
        data = CharacterDatabase.Instance.GetCharacterData(id);

        id = data.id;

        health = data.maxHealth;
        maxHealth = data.maxHealth;
        satiety = data.maxHealth;
        maxSatiety = data.maxSatiety;
        thirsty = data.maxThirsty;
        maxThirsty = data.maxThirsty;

        vitality = data.vitality;
        strength = data.strength;
        dexterity = data.dexterity;
        intelligence = data.intelligence;
        mana = data.mana;
    }

    // public void Update()
    // {

    // }

    public virtual void EnhanceVitality(int value)
    {
        vitality += value;
    }

    public virtual void EnhanceStrength(int value)
    {
        strength += value;
    }

    public virtual void EnhanceDexterity(int value)
    {
        dexterity += value;
    }

    public virtual void EnhanceIntelligence(int value)
    {
        intelligence += value;
    }

    public virtual void EnhanceMana(int value)
    {
        mana += value;
    }
    
}
