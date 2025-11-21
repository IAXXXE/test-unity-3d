using UnityEngine;

public class InteractableGather : InteractableBase
{
    [Header("采集设置")]
    public int itemId;
    public int minQuantity = 1;
    public int maxQuantity = 3;

    protected override void Start()
    {
        base.Start();
        // interactableType = InteractableType.Gather;
    }

    public void StartGather()
    {
        PlayerIKController.Instance.SetAnimBool(AnimActionType.Gather, true);
    }

    public void StopGather()
    {
        PlayerIKController.Instance.SetAnimBool(AnimActionType.Gather, false);
    }

    public override void Interact(PlayerController player)
    {
        StartGather();
        int quantity = Random.Range(minQuantity, maxQuantity + 1);
        bool added = InventoryManager.Instance.AddItem(itemId, quantity);
        
        if (added)
        {
            Debug.Log($"采集了 {quantity} 个 {itemId}");
            // StartCooldown();
        }

        StopGather();
    }

    // 其他基础采集逻辑...
}