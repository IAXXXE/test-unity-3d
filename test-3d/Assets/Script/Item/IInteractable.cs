using UnityEngine;

public interface IInteractable
{
    void OnFocus();
    void OnLoseFocus();
    void OnInteract();
    string GetInteractText();
}
