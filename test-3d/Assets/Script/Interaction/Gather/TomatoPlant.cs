using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

public enum TomatoPlantState
{
    Empty,
    Full
}

public class TomatoPlant : InteractableGather
{
    [Header("设置")]
    public TomatoPlantState currentState = TomatoPlantState.Full;

    public int quantity = 3;

    [Header("生长时间设置")]
    public float growTime = 300f;
    
    [Header("视觉效果")]
    public List<GameObject> berryVisuals;
    public ParticleSystem gatherEffect;
    
    private Coroutine growthCoroutine;

    protected override void Start()
    {
        base.Start();

        foreach(Transform child in transform)
        {
            berryVisuals.Add(child.gameObject);
        }   
    }

    public override void Interact(PlayerController player)
    {
        if (currentState == TomatoPlantState.Empty)
        {
            Debug.Log($"{interactName} 还没有番茄可以采集");
            return;
        }

        int yieldAmount = quantity;
        if (yieldAmount > 0 && itemId != null)
        {
            bool added = InventoryManager.Instance.AddItem(itemId, yieldAmount);
            
            if (added)
            {
                if (gatherEffect != null)
                {
                    gatherEffect.Play();
                }
                
                Debug.Log($"从 {interactName} 采集了 {yieldAmount} 个 {itemId}");
                currentState = TomatoPlantState.Empty;
                UpdateVisualState();
                StartGrowthCycle();
            }
        }
    }

    public override string GetInteractText()
    {
        if (currentState == TomatoPlantState.Empty)
            return $"{interactName}（Empty）";
        return $"Picking {interactName}（Get {itemId} * {quantity}）";
    }

    public override bool CanInteract()
    {
        return base.CanInteract() && currentState != TomatoPlantState.Empty;
    }

    private void StartGrowthCycle()
    {
        if (growthCoroutine != null)
            StopCoroutine(growthCoroutine);
            
        growthCoroutine = StartCoroutine(GrowthRoutine());
    }

    private IEnumerator GrowthRoutine()
    {
        if (currentState == TomatoPlantState.Empty)
        {
            yield return new WaitForSeconds(growTime);
            currentState = TomatoPlantState.Full;
            UpdateVisualState();
        }
        growthCoroutine = null;
    }

    private void UpdateVisualState()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(i == (int)currentState);
        }

        OnStateChanged();
    }

    public override void SetHighlight(bool on)
    {
        if(transform.GetChild((int)currentState).GetComponent<Outline>() == null) return;
        if(transform.GetChild((int)currentState).GetComponent<Outline>().enabled != on && showOutline)
            transform.GetChild((int)currentState).GetComponent<Outline>().enabled = on && showOutline;
    }

    private void OnStateChanged()
    {
        Debug.Log($"{interactName} 状态变为: {currentState}");
        
        // Effect
    }

    public void SetState(TomatoPlantState newState)
    {
        if (growthCoroutine != null)
        {
            StopCoroutine(growthCoroutine);
            growthCoroutine = null;
        }

        currentState = newState;
        UpdateVisualState();

        if (currentState != TomatoPlantState.Full)
        {
            StartGrowthCycle();
        }
    }

    public string GetStateInfo()
    {
        string stateText = "";
        switch (currentState)
        {
            case TomatoPlantState.Empty:
                stateText = "Empty";
                break;
            case TomatoPlantState.Full:
                stateText = $"Full（{quantity} ）";
                break;
        }

        return $"{interactName} - {stateText}";
    }


}
