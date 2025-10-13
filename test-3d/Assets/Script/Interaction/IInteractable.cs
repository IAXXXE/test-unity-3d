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
    // 散落的资源，可以直接拾取到背包中，拾取后在场景中消失
    Pickup,
    
    // 采集的资源 - 不需要工具
    Gather,
    
    // 采集的资源 - 需要特定工具
    GatherWithTool,
    
    // 可以被破坏的物体，破坏后可能掉落物品
    Destructible,
    
    // 可以被举起的物体，举起后可以放下/扔下
    Liftable,
    
    // 功能性互动物体
    Functional,

    // 按钮
    Button,
    
    // 容器（箱子、宝箱等）
    Container,
    
    // 对话/NPC
    Dialogue,
    
    // 任务相关
    Quest
}