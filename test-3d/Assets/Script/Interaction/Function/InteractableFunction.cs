using UnityEngine;

public class InteractableFunction : InteractableBase
{
    public int amount = 1;

    public void Start()
    {
        interactableType = InteractableType.Functional;
    }

    public override void Interact(PlayerController player)
    {
        Debug.Log("Enter Other Mode");
    }

    public override string GetInteractText()
    {
        return $"Cook";
    }
}
