using UnityEngine;

public interface IInteractable
{
    string GetInteractText();               // 提示文字（例如 “按E拾取”、“按E开门”）
    bool CanInteract();  // 是否满足交互条件（距离、冷却、道具等）
    void Interact(PlayerController player); // 执行交互逻辑

    public void SetHighlight(bool on);
    public string GetDisplayName();
}


public enum InteractableType
{
    Pickup,

}