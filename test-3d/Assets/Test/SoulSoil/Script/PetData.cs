using UnityEngine;

[System.Serializable]
public class PetData
{
    [Header("基础信息")]
    public string petName;
    public PetType petType;
    public Sprite icon;
    public GameObject prefab;
    
    [Header("属性")]
    public int maxHealth = 100;
    public float moveSpeed = 3.5f;
    
    [Header("跟随设置")]
    public float followDistance = 3f;
    public float stopDistance = 2f;
    public float teleportDistance = 15f;
    
    [Header("描述")]
    [TextArea(3, 6)]
    public string description;
}


/*

## 📋 完整系统集成清单

### **场景设置步骤**

1. **创建玩家对象**
   - 添加 `CharacterController`
   - 添加 `PlayerHealth`
   - 添加 Tag: "Player"

2. **创建宠物管理器对象**
   - 添加 `PetManager`
   - 添加 `PetSummonSystem`
   - 添加 `PetRideSystem`
   - 引用玩家 Transform

3. **创建UI Canvas**
   - 添加 `PetInteractionUI`
   - 添加 `TooltipSystem`
   - 设计互动面板（包含宠物信息、互动按钮）
   - 设计提示面板

4. **创建宠物预制体**
   - 添加对应的宠物脚本（CreaturePet/MechanicalPet/ElementalPet）
   - 添加 `NavMeshAgent`
   - 添加 `Animator`（可选）
   - 添加 `CreatureDamageReceiver`（如果需要受伤系统）
   - 配置 Collider

5. **创建宠物数据资源**
   - 右键 Create → Pet System → Pet Data
   - 配置每个宠物的属性

### **使用示例**
```csharp
// 召唤宠物
PetBase pet = summonSystem.SummonPet("小狗");

// 手动让宠物跟随
pet.Follow(player.transform);

// 靠近宠物按E互动
// 在互动面板中选择不同的互动选项

// 骑乘宠物（如果支持）
rideSystem.Mount(pet);

// 收回宠物
petManager.RecallPet(pet);
```

## ✨ 扩展建议

1. **宠物升级系统**：经验值、等级、技能树
2. **宠物装备系统**：可穿戴装备增强属性
3. **宠物繁殖系统**：两只宠物繁殖产生新宠物
4. **宠物战斗系统**：宠物可以参与战斗
5. **宠物情绪系统**：开心/生气/悲伤影响行为
6. **宠物背包系统**：宠物可以携带物品
7. **宠物成就系统**：完成特定条件解锁新互动
8. **多宠物编队**：同时跟随多只宠物并控制阵型

这套系统非常灵活，可以轻松添加新的宠物类型和互动方式！🐾✨

*/