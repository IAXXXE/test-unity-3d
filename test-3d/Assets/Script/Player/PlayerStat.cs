using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStat : CharacterStat
{
    private int energy;
    private int workSpeed;
    private int eatSpeed;

    private int magic;
    private int maxMagic;

    private PlayerWeapon weapon;

    public PlayerStat(string id) : base(id)
    {
        data = CharacterDatabase.Instance.GetCharacterData(id);
    }

    public void Init()
    {
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

        maxMagic = data.maxMagic;
        magic = maxMagic;
    }

    public void SetMagic(int value)
    {
        if(value < 0) value = 0;
        if(value > maxMagic) value = maxMagic;

        magic = value;
        GameEventManager.TriggerPlayerMagicChanged(magic);
    }

    public void IncreaseMagic(int value)
    {
        var newMagic = magic + value;
        magic = Mathf.Min(maxMagic, newMagic);
    }

    public void IncreaseMaxMagic(int value)
    {
        maxMagic += value;
        magic += value;
    }

    public int GetMagic()
    {
        return magic;
    }

    public int GetMaxMagic()
    {
        return maxMagic;
    }

    public override void SetHealth(int value)
    {
        base.SetHealth(value);
        GameEventManager.TriggerPlayerHealthChanged(health);
    }

    public override void SetSatiety(int value)
    {
        base.SetSatiety(value);
        GameEventManager.TriggerPlayerSatietyChanged(satiety);
    }

    public override void SetThirsty(int value)
    {
        base.SetThirsty(value);
        GameEventManager.TriggerPlayerThirstyChanged(thirsty);
    }

    public void SetWeapon(PlayerWeapon playerWeapon)
    {
        weapon = playerWeapon;
    }

    public ItemType GetHeldItemType()
    {
        return weapon.GetHeldItemType();
    }

    public List<ToolProperty> GetHeldItemProperty()
    {
        return weapon.GetHeldItem()?.data.toolProperties;
    }

    public PlayerUI GetPlayerUI()
    {
        return weapon.playerUI;
    }


}
