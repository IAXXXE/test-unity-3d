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

    private float moistureDrainRate = 0.5f;  // 水分消耗
    private float decompositeRate = 3f; // 分解速率

    private bool isEating;

    [Header("作物属性")]
    public Transform crop;
    public float growthRate = 0;
    public float maxGrowthRate = 100;
    public bool isRipening = false;

    protected override void Update()
    {
        base.Update();

        if(isFollowing)
        {
            moisture = Mathf.Max(0, moisture - moistureDrainRate * Time.deltaTime);
        }

        if(isEating)
        {
            fertility = Mathf.Min(maxFertility, fertility + decompositeRate * Time.deltaTime);
        }
        else
        {
            fertility = Mathf.Max(maxFertility, fertility - decompositeRate * Time.deltaTime);
        }

        if(growthRate < maxGrowthRate)
        {
            growthRate = Mathf.Min(maxGrowthRate, growthRate + 1f * Time.deltaTime);
            if(growthRate == maxGrowthRate)
            {
                CropRipen();
            }
        }
    }

    public override InteractionOption[] GetAvailableInteractions()
    {
        List<InteractionOption> options = new List<InteractionOption>();

        // 浇水
        bool needWater = moisture < maxMoisture;
        var waterOption = new InteractionOption(InteractionType.Water, "Water", needWater);
        if(!needWater)
            waterOption.unavailableReason = "水分充足";
        options.Add(waterOption);

        // 喂食
        bool needFeed = fertility < maxFertility;
        var feedOption = new InteractionOption(InteractionType.Feed, "Feed", needFeed);
        if(!needFeed)
            feedOption.unavailableReason = "养分充足";
        options.Add(feedOption); 

        // 采摘
        bool canPick = CanPick();
        var pickOption = new InteractionOption(InteractionType.Pick, "Pick", canPick);
        if(!canPick)
            pickOption.unavailableReason = "没有可以采摘的";
        options.Add(pickOption);

        // 玩耍（用锄头挠挠）
        var playOption = new InteractionOption(InteractionType.Play, "Plow", true);
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
        // animation VFX UI
        moisture += 10f;
        Debug.Log("Water " + moisture);
    }

    public void BeFeed()
    {
        Debug.Log("See Food ! ");
        agent.SetDestination(followTarget.position);
        StartCoroutine(MoveToPlayer());
    }

    public void OnFeed()
    {
        // animation
        isEating = true;
        StartCoroutine(Eating());
    }

    private IEnumerator MoveToPlayer()
    {
        float distance = Vector3.Distance(transform.position, followTarget.position);
        while(distance > 2f)
        {
            yield return new WaitForSeconds(0.5f);
            distance = Vector3.Distance(transform.position, followTarget.position);
        }

        EnterInteractionMode();

        // 吃下
        GameEventManager.TriggerHeldItemConsumed();
        OnFeed();
    }

    private IEnumerator Eating()
    {
        yield return new WaitForSeconds(10f);

        isEating = false;
    }

    public void OnPick()
    {
        // animation
        isRipening = false;
        growthRate = 0;

        crop.GetChild(0).gameObject.SetActive(true);
        crop.GetChild(1).gameObject.SetActive(false);

        InventoryManager.Instance.AddItem(100013, 1);
    }

    public void OnPlay()
    {
        // animation

    }

    public bool CanPick()
    {
        return isRipening;
    }

    public bool CanHoeing()
    {
        return true;
    }

    private void CropRipen()
    {
        isRipening = true;
        crop.GetChild(0).gameObject.SetActive(false);
        crop.GetChild(1).gameObject.SetActive(true);
    }
}
