using UnityEngine;

// 宠物接口
public interface IPet
{
    string GetPetName();
    PetType GetPetType();
    void Follow(Transform target);
    void StopFollow();
    void Interact(InteractionType interactionType);
    InteractionOption[] GetAvailableInteractions();
    bool IsFollowing();
    Transform GetTransform();
}

// 宠物类型
public enum PetType
{
    Creature,      // 生物型
    SoulSoil,      // 魂土
    Mechanical,    // 机械型
    Elemental,     // 元素型
    Spirit,        // 精灵型
    Companion      // 伴侣型
}

// 互动类型
public enum InteractionType
{
    Pet,           // 抚摸
    Feed,          // 喂食
    Play,          // 玩耍
    Command,       // 命令
    Ride,          // 骑乘
    Carry,         // 携带/收回
    Talk,          // 对话
    Repair,        // 维修（机械）
    Charge,        // 充能（元素/机械）
    Train,         // 训练
    Heal,          // 治疗
    Water,         // 浇水
    Pick,          // 采摘
    CustomAction   // 自定义动作
}

// 互动选项数据
[System.Serializable]
public class InteractionOption
{
    public InteractionType type;
    public string displayName;
    public Sprite icon;
    public bool isAvailable;
    public string unavailableReason;
    
    public InteractionOption(InteractionType type, string displayName, bool isAvailable = true)
    {
        this.type = type;
        this.displayName = displayName;
        this.isAvailable = isAvailable;
        this.unavailableReason = "";
    }
}