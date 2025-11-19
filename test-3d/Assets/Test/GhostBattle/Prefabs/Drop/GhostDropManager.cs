using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// ==================== 掉落管理器 ====================

/// <summary>
/// 幽灵掉落管理器
/// </summary>
public class GhostDropManager : MonoBehaviour
{
    public static GhostDropManager Instance;

    [Header("预制体")]
    [SerializeField] private GameObject manaBolusSmallPrefab;   // 小型魔法球 (1 mana)
    [SerializeField] private GameObject manaBolusMediumPrefab;  // 中型魔法球 (3 mana)
    [SerializeField] private GameObject manaBolusLargePrefab;   // 大型魔法球 (5 mana)
    
    [Header("掉落参数")]
    [SerializeField] private float explosionForce = 5f;         // 爆炸力度
    [SerializeField] private float explosionRadius = 2f;        // 爆炸半径
    [SerializeField] private float upwardForce = 3f;            // 向上的力

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 处理幽灵死亡掉落
    /// </summary>
    public void HandleGhostDrop(GhostData ghostData, Vector3 dropPosition, Quaternion dropRotation)
    {
        // 1. 掉落 ManaBolus
        DropManaBolus(ghostData, dropPosition);

        // 2. 掉落其他物品
        DropOtherItems(ghostData, dropPosition, dropRotation);

        // 3. 生成场景物体
        SpawnSceneObjects(ghostData, dropPosition, dropRotation);
    }

    /// <summary>
    /// 掉落魔法值球
    /// </summary>
    private void DropManaBolus(GhostData ghostData, Vector3 dropPosition)
    {
        // 检查掉落概率
        if (Random.value > ghostData.manaBolusDropChance) return;

        int totalMana = ghostData.manaBolusDropAmount;
        if (totalMana <= 0) return;

        // 根据总量决定掉落策略
        List<string> manaValues = CalculateManaDistribution(totalMana);

        // 生成魔法球并炸开
        foreach (string manaSize in manaValues)
        {
            GameObject prefab = GetManaBolusPrefab(manaSize);
            if (prefab == null) continue;

            // 在掉落位置周围随机偏移
            Vector3 randomOffset = Random.insideUnitSphere * 0.5f;
            randomOffset.y = Mathf.Abs(randomOffset.y); // 确保在上方
            Vector3 spawnPos = dropPosition + Vector3.up * 1f + randomOffset;

            GameObject bolusObj = Instantiate(prefab, spawnPos, Random.rotation);
            
            // 设置魔法值
            ManaBolus bolus = bolusObj.GetComponent<ManaBolus>();
            if (bolus != null)
            {
                bolus.SetManaValue(manaSize);
            }

            // 添加爆炸效果
            Rigidbody rb = bolusObj.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = bolusObj.AddComponent<Rigidbody>();
            }

            // 随机爆炸方向
            Vector3 explosionDir = Random.onUnitSphere;
            explosionDir.y = Mathf.Abs(explosionDir.y) * 0.5f; // 偏向向上

            Vector3 force = explosionDir * explosionForce + Vector3.up * upwardForce;
            rb.AddForce(force, ForceMode.Impulse);
            
            // 添加随机旋转
            rb.AddTorque(Random.insideUnitSphere * 2f, ForceMode.Impulse);

            // 传递初始速度给 ManaBolus
            if (bolus != null)
            {
                bolus.SetInitialVelocity(rb.velocity);
            }
        }
    }

    /// <summary>
    /// 计算魔法值分配
    /// 策略：优先生成大球，余数生成小球
    /// </summary>
    private List<string> CalculateManaDistribution(int totalMana)
    {
        List<string> distribution = new();
        
        // 策略：30个一组生成大球，10个一组生成中球，余下的生成小球
        int valueL = 30, valueM = 10, valueS = 1;
        
        int largeBolus = totalMana / valueL;
        int remainder = totalMana % valueL;
        // 添加大球
        for(int i = 0; i < largeBolus; i++)
        {
            distribution.Add("L");
        }

        // 处理余数
        int midBolus = remainder / valueM;
        remainder = remainder % valueM;
        for(int i = 0; i < midBolus; i++)
        {
            distribution.Add("M");
        }

        // 剩余的生成小球
        int smallBolus = remainder / valueS;
        for(int i = 0; i < smallBolus; i++)
        {
            distribution.Add("S");
        }

        return distribution;
    }

    /// <summary>
    /// 根据魔法值获取对应预制体
    /// </summary>
    private GameObject GetManaBolusPrefab(string size)
    {
        if (size == "L")
            return manaBolusLargePrefab;
        else if (size == "M")
            return manaBolusMediumPrefab;
        else
            return manaBolusSmallPrefab;
    }

    /// <summary>
    /// 掉落其他物品
    /// </summary>
    private void DropOtherItems(GhostData ghostData, Vector3 dropPosition, Quaternion dropRotation)
    {
        if (ghostData.otherDrops == null || ghostData.otherDrops.Count == 0) return;

        foreach (var dropConfig in ghostData.otherDrops)
        {
            // 检查掉落概率
            if (Random.value > dropConfig.dropChance) continue;

            // 随机数量
            int dropCount = Random.Range(dropConfig.minCount, dropConfig.maxCount + 1);

            for (int i = 0; i < dropCount; i++)
            {
                // 随机偏移
                Vector3 randomOffset = Random.insideUnitSphere * 1f;
                randomOffset.y = Mathf.Abs(randomOffset.y);
                Vector3 spawnPos = dropPosition + Vector3.up * 1f + randomOffset;

                GameObject dropItem = Instantiate(dropConfig.itemPrefab, spawnPos, dropRotation);

                // 添加物理效果
                Rigidbody rb = dropItem.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 explosionDir = Random.onUnitSphere;
                    explosionDir.y = Mathf.Abs(explosionDir.y) * 0.5f;
                    Vector3 force = explosionDir * explosionForce * 0.5f + Vector3.up * upwardForce * 0.5f;
                    rb.AddForce(force, ForceMode.Impulse);
                }
            }
        }
    }

    /// <summary>
    /// 生成场景物体
    /// </summary>
    private void SpawnSceneObjects(GhostData ghostData, Vector3 spawnPosition, Quaternion spawnRotation)
    {
        if (ghostData.spawnObjects == null || ghostData.spawnObjects.Count == 0) return;

        foreach (var spawnConfig in ghostData.spawnObjects)
        {
            // 检查生成概率
            if (Random.value > spawnConfig.spawnChance) continue;

            Vector3 finalPosition = spawnPosition + spawnConfig.positionOffset;

            // 如果需要贴地
            if (spawnConfig.attachToGround)
            {
                RaycastHit hit;
                if (Physics.Raycast(finalPosition + Vector3.up * 10f, Vector3.down, out hit, 20f))
                {
                    finalPosition = hit.point;
                }
            }

            Instantiate(spawnConfig.objectPrefab, finalPosition, spawnRotation);
        }
    }
}