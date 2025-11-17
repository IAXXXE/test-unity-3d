using UnityEngine;

/// <summary>
/// 掉落物配置
/// </summary>
[System.Serializable]
public class DropItemConfig
{
    public GameObject itemPrefab;     // 掉落物预制体
    public float dropChance = 0.3f;   // 掉落概率
    public int minCount = 1;          // 最小掉落数量
    public int maxCount = 1;          // 最大掉落数量
}

/// <summary>
/// 场景物体生成配置
/// </summary>
[System.Serializable]
public class SpawnObjectConfig
{
    public GameObject objectPrefab;   // 要生成的物体
    public float spawnChance = 1f;    // 生成概率
    public Vector3 positionOffset;    // 位置偏移
    public bool attachToGround = true;// 是否贴地
}

/// <summary>
/// 玩家魔法值管理接口
/// </summary>
public interface IManageable
{
    void RestoreMana(int amount);
    int GetCurrentMana();
    int GetMaxMana();
}
