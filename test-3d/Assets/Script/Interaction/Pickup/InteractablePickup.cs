using UnityEngine;

public class InteractablePickup : InteractableBase
{
    public string itemId;
    public int amount = 1;

    public void Start()
    {
        interactableType = InteractableType.Pickup;
    }

    public override void Interact(PlayerController player)
    {
        InventoryManager.Instance.AddItem(itemId, amount);
        Destroy(gameObject); // 或者隐藏模型
    }

    public override string GetInteractText()
    {
        return $"Pick  {interactName}";
    }
}
