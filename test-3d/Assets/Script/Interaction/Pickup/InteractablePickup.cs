using UnityEngine;

public class InteractableCookbook : InteractableFunction
{
    public int amount = 1;

    public void Start()
    {
        interactableType = InteractableType.Pickup;
    }

    public override void Interact(PlayerController player)
    {
        
        
        Destroy(gameObject); // 或者隐藏模型
    }

    public override string GetInteractText()
    {
        return $"Pick  Cookbook";
    }
}
