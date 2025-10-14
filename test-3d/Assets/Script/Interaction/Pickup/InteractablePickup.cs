using UnityEngine;

public class InteractablePickup : InteractableBase
{
    public string itemId;
    public int amount = 1;

    public override void Interact(PlayerController player)
    {
        InventoryManager.Instance.AddItem(itemId, amount);
        Debug.Log("Pick up" + itemId);
        Destroy(gameObject); // 或者隐藏模型
    }

    public override string GetInteractText()
    {
        return $"Pick  {interactName}";
    }
}
