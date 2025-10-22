// cookbook
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleKitchen : InteractableFunction
{
    public override void Interact(PlayerController player)
    {
        Debug.Log("Enter Other Mode");
        GameEventManager.TriggerCooked(CookLevel.Simple);
    }

    public override string GetInteractText()
    {
        return $"Cook";
    }
}
