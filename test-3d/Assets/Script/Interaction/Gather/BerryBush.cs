using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using EasyButtons;
using System.Collections.Generic;

public enum BerryBushState
{
    NoBerries,      // 没有浆果
    FewBerries,     // 有少量浆果
    SomeBerries,    // 有一定量浆果
    FullBerries     // 长满了浆果
}

public class BerryBush : InteractableGather
{
    [Header("浆果树丛设置")]
    public BerryBushState currentState = BerryBushState.FullBerries;
    
    [Header("生长时间设置")]
    public float growToFewTime = 30f;    // 从无到少量
    public float growToSomeTime = 60f;   // 从少量到一定量
    public float growToFullTime = 90f;   // 从一定量到长满
    
    [Header("采集数量设置")]
    public int fewBerriesYield = 1;      // 少量时的产量
    public int someBerriesYield = 4;     // 一定量时的产量
    public int fullBerriesYield = 10;     // 长满时的产量
    
    [Header("视觉效果")]
    public List<GameObject> berryVisuals;// 浆果的视觉表现
    public ParticleSystem gatherEffect;  // 采集特效
    
    private Coroutine growthCoroutine;
    private Renderer bushRenderer;

    protected override void Start()
    {
        base.Start();
        bushRenderer = GetComponent<Renderer>();
        
        // 初始化状态
        foreach(Transform child in transform)
        {
            berryVisuals.Add(child.gameObject);
        }
        UpdateVisualState();
        
        // 如果初始状态不是Full，启动生长协程
        if (currentState != BerryBushState.FullBerries)
        {
            StartGrowthCycle();
        }
    }

    public override void Interact(PlayerController player)
    {
        if (currentState == BerryBushState.NoBerries)
        {
            Debug.Log($"{interactName} 还没有浆果可以采集");
            return;
        }

        // 根据状态获取采集数量
        int yieldAmount = GetYieldAmount();
        if (yieldAmount > 0 && itemId != null)
        {
            bool added = InventoryManager.Instance.AddItem(itemId, yieldAmount);
            
            if (added)
            {
                // 播放采集特效
                if (gatherEffect != null)
                {
                    gatherEffect.Play();
                }
                
                Debug.Log($"从 {interactName} 采集了 {yieldAmount} 个 {itemId}");
                
                // 采集后状态变为无浆果
                currentState = BerryBushState.NoBerries;
                UpdateVisualState();
                
                // 开始生长周期
                StartGrowthCycle();
                
                StartCooldown();
            }
        }
    }

    public override string GetInteractText()
    {
        if (currentState == BerryBushState.NoBerries)
            return $"{interactName}（没有浆果）";
        
        int yieldAmount = GetYieldAmount();
        return $"采集 {interactName}（可获得 {yieldAmount} 个 {itemId}）";
    }

    public override bool CanInteract()
    {
        return base.CanInteract() && currentState != BerryBushState.NoBerries;
    }

    /// <summary>
    /// 根据当前状态获取产量
    /// </summary>
    private int GetYieldAmount()
    {
        switch (currentState)
        {
            case BerryBushState.FewBerries:
                return fewBerriesYield;
            case BerryBushState.SomeBerries:
                return someBerriesYield;
            case BerryBushState.FullBerries:
                return fullBerriesYield;
            default:
                return 0;
        }
    }

    /// <summary>
    /// 开始生长周期
    /// </summary>
    private void StartGrowthCycle()
    {
        if (growthCoroutine != null)
            StopCoroutine(growthCoroutine);
            
        growthCoroutine = StartCoroutine(GrowthRoutine());
    }

    /// <summary>
    /// 生长协程
    /// </summary>
    private IEnumerator GrowthRoutine()
    {
        // 从 NoBerries -> FewBerries
        if (currentState == BerryBushState.NoBerries)
        {
            yield return new WaitForSeconds(growToFewTime);
            currentState = BerryBushState.FewBerries;
            UpdateVisualState();
        }

        // 从 FewBerries -> SomeBerries
        if (currentState == BerryBushState.FewBerries)
        {
            yield return new WaitForSeconds(growToSomeTime);
            currentState = BerryBushState.SomeBerries;
            UpdateVisualState();
        }

        // 从 SomeBerries -> FullBerries
        if (currentState == BerryBushState.SomeBerries)
        {
            yield return new WaitForSeconds(growToFullTime);
            currentState = BerryBushState.FullBerries;
            UpdateVisualState();
        }

        growthCoroutine = null;
    }

    /// <summary>
    /// 更新视觉状态
    /// </summary>
    private void UpdateVisualState()
    {
        // 更新浆果视觉显示
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(i == (int)currentState);
        }

        // 触发状态变化事件
        OnStateChanged();
    }

    public override void SetHighlight(bool on)
    {
        Debug.Log("SetHighlight Berry tree : " + (on && showOutline));
        if(transform.GetChild((int)currentState).GetComponent<Outline>().enabled != on && showOutline)
            transform.GetChild((int)currentState).GetComponent<Outline>().enabled = on && showOutline;
    }


    /// <summary>
    /// 状态变化事件
    /// </summary>
    private void OnStateChanged()
    {
        Debug.Log($"{interactName} 状态变为: {currentState}");
        
        // 可以在这里添加更多状态变化的效果
        // 比如播放生长音效、粒子效果等
    }

    /// <summary>
    /// 强制设置状态（用于调试或特殊事件）
    /// </summary>
    public void SetState(BerryBushState newState)
    {
        if (growthCoroutine != null)
        {
            StopCoroutine(growthCoroutine);
            growthCoroutine = null;
        }

        currentState = newState;
        UpdateVisualState();

        // 如果新状态不是Full，继续生长
        if (currentState != BerryBushState.FullBerries)
        {
            StartGrowthCycle();
        }
    }

    /// <summary>
    /// 获取当前状态信息
    /// </summary>
    public string GetStateInfo()
    {
        string stateText = "";
        switch (currentState)
        {
            case BerryBushState.NoBerries:
                stateText = "没有浆果";
                break;
            case BerryBushState.FewBerries:
                stateText = $"少量浆果（可采集 {fewBerriesYield} 个）";
                break;
            case BerryBushState.SomeBerries:
                stateText = $"有一些浆果（可采集 {someBerriesYield} 个）";
                break;
            case BerryBushState.FullBerries:
                stateText = $"长满了浆果（可采集 {fullBerriesYield} 个）";
                break;
        }

        return $"{interactName} - {stateText}";
    }

    // 保存/加载状态
    // public BerryBushSaveData GetSaveData()
    // {
    //     return new BerryBushSaveData
    //     {
    //         currentState = currentState,
    //         position = transform.position
    //     };
    // }

    // public void LoadSaveData(BerryBushSaveData saveData)
    // {
    //     currentState = saveData.currentState;
    //     UpdateVisualState();
        
    //     if (currentState != BerryBushState.FullBerries)
    //     {
    //         StartGrowthCycle();
    //     }
    // }

    #if UNITY_EDITOR
    [Button]
    private void SetToNoBerries() => SetState(BerryBushState.NoBerries);
    
    [Button]
    private void SetToFewBerries() => SetState(BerryBushState.FewBerries);
    
    [Button]
    private void SetToSomeBerries() => SetState(BerryBushState.SomeBerries);
    
    [Button]
    private void SetToFullBerries() => SetState(BerryBushState.FullBerries);
    
    [Button]
    private void PrintStateInfo()
    {
        Debug.Log(GetStateInfo());
    }
    #endif
}