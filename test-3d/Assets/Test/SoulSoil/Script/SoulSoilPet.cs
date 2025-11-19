using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoulSoilPet : PetBase
{
    [Header("土壤属性")]
    public float moisture = 100;
    public float maxMoisture = 100;
    public float fertility = 100;
    public float maxFertility = 100;
    public float soulPower = 100;
    public float maxSoulPower = 100;

    private float moistureDrainRate = 1f;  // 水分消耗
    private float decompositeRate = 3f; // 分解速率

    private bool isEating;

    [Header("作物属性")]
    public Transform crop;

    protected override void Update()
    {
        base.Update();

        if(isFollowing)
        {
            moisture = Mathf.Max(0, moisture - moistureDrainRate * Time.deltaTime);
        }

        if(isEating)
        {
            fertility = Mathf.Max(maxFertility, fertility + decompositeRate * Time.deltaTime);
        }
    }

    public override InteractionOption[] GetAvailableInteractions()
    {
        List<InteractionOption> options = new List<InteractionOption>();

        // 浇水
        bool needWater = moisture < maxMoisture;
        var waterOption = new InteractionOption(InteractionType.Water, "浇水", needWater);
        if(!needWater)
            waterOption.unavailableReason = "水分充足";
        options.Add(waterOption);

        // 喂食
        bool needFeed = fertility < maxFertility;
        var feedOption = new InteractionOption(InteractionType.Feed, "", needFeed);
        if(!needFeed)
            feedOption.unavailableReason = "养分充足";
        options.Add(feedOption); 

        // 采摘
        bool canPick = CanPick();
        var pickOption = new InteractionOption(InteractionType.Pick, "", canPick);
        if(!canPick)
            pickOption.unavailableReason = "没有可以采摘的";
        options.Add(pickOption);

        // 玩耍（用锄头挠挠）
        var playOption = new InteractionOption(InteractionType.Play, "", true);
        options.Add(playOption);

        return options.ToArray();
    }

    public override void Interact(InteractionType interactionType)
    {
        switch(interactionType)
        {
            case InteractionType.Water:
                OnWater();
                break;
            case InteractionType.Feed:
                OnFeed();
                break;
            case InteractionType.Pick:
                OnPick();
                break;
            case InteractionType.Play:
                OnPlay();
                break;
        }
    }

    public void OnWater()
    {
        // animation
        moisture += 10f;
    }

    public void OnFeed()
    {
        // animation

    }

    public void OnPick()
    {
        // animation

    }

    public void OnPlay()
    {
        // animation

    }

    public bool CanPick()
    {
        return true;
    }

    public bool CanHoeing()
    {
        return true;
    }
}
