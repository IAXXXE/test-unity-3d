using System.Collections.Generic;
using UnityEngine;

// ==================== ManaBolus 类 ====================

/// <summary>
/// 魔法值恢复物
/// </summary>
public class ManaBolus : MonoBehaviour
{
    [Header("属性")]
    [SerializeField] private int manaValue = 1;        // 恢复的魔法值
    [SerializeField] private float attractRadius = 5f;  // 吸引半径
    [SerializeField] private float attractSpeed = 10f;  // 吸引速度
    [SerializeField] private float lifetime = 30f;      // 生命周期（秒）
    
    [Header("视觉效果")]
    [SerializeField] private ParticleSystem glowEffect;
    [SerializeField] private TrailRenderer trail;
    [SerializeField] private AudioClip collectSound;
    
    private Transform playerTransform;
    private bool isBeingAttracted = false;
    private float spawnTime;
    private Vector3 initialVelocity;
    private bool hasLanded = false;

    private void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        spawnTime = Time.time;
        
        // 开始下落（物理模拟）
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null && !rb.isKinematic)
        {
            rb.velocity = initialVelocity;
        }
    }

    private void Update()
    {
        // 生命周期检查
        if (Time.time - spawnTime > lifetime)
        {
            DestroySelf();
            return;
        }

        if (playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // 在吸引范围内且已经落地
        if (distanceToPlayer <= attractRadius && hasLanded)
        {
            isBeingAttracted = true;
        }

        // 被吸引向玩家
        if (isBeingAttracted)
        {
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            transform.position += direction * attractSpeed * Time.deltaTime;

            // 到达玩家，收集
            if (distanceToPlayer < 0.5f)
            {
                Collect();
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 落地后停止物理模拟
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            hasLanded = true;
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
            }
        }
    }

    /// <summary>
    /// 设置初始弹射速度
    /// </summary>
    public void SetInitialVelocity(Vector3 velocity)
    {
        initialVelocity = velocity;
    }

    /// <summary>
    /// 设置魔法值
    /// </summary>
    public void SetManaValue(int value)
    {
        manaValue = value;
        
        // 根据魔法值调整大小
        float scale = Mathf.Lerp(0.3f, 1f, value / 10f);
        transform.localScale = Vector3.one * scale;
    }

    /// <summary>
    /// 收集魔法值
    /// </summary>
    private void Collect()
    {
        // 恢复玩家魔法值
        var playerMana = playerTransform.GetComponent<IManageable>();
        if (playerMana != null)
        {
            playerMana.RestoreMana(manaValue);
        }

        // 播放收集音效
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }

        // 播放收集特效
        if (glowEffect != null)
        {
            glowEffect.transform.SetParent(null);
            glowEffect.Stop();
            Destroy(glowEffect.gameObject, 2f);
        }

        DestroySelf();
    }

    private void DestroySelf()
    {
        Destroy(gameObject);
    }
}

// // ==================== 示例：可拾取物品基类 ====================

// /// <summary>
// /// 可拾取物品基类
// /// </summary>
// public abstract class PickupItem : MonoBehaviour
// {
//     [SerializeField] protected float attractRadius = 3f;
//     [SerializeField] protected float attractSpeed = 8f;
//     [SerializeField] protected float lifetime = 60f;
    
//     protected Transform playerTransform;
//     protected bool isBeingAttracted = false;
//     protected float spawnTime;

//     protected virtual void Start()
//     {
//         playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
//         spawnTime = Time.time;
//     }

//     protected virtual void Update()
//     {
//         if (Time.time - spawnTime > lifetime)
//         {
//             Destroy(gameObject);
//             return;
//         }

//         if (playerTransform == null) return;

//         float distance = Vector3.Distance(transform.position, playerTransform.position);

//         if (distance <= attractRadius)
//         {
//             isBeingAttracted = true;
//         }

//         if (isBeingAttracted)
//         {
//             Vector3 direction = (playerTransform.position - transform.position).normalized;
//             transform.position += direction * attractSpeed * Time.deltaTime;

//             if (distance < 0.5f)
//             {
//                 OnPickup();
//             }
//         }
//     }

//     protected abstract void OnPickup();
// }

// /// <summary>
// /// 示例：经验值拾取物
// /// </summary>
// public class ExperienceOrb : PickupItem
// {
//     [SerializeField] private int experienceValue = 10;

//     protected override void OnPickup()
//     {
//         var playerExp = playerTransform.GetComponent<IExperienceable>();
//         if (playerExp != null)
//         {
//             playerExp.AddExperience(experienceValue);
//         }

//         // 播放拾取特效和音效
//         // ...

//         Destroy(gameObject);
//     }
// }

// public interface IExperienceable
// {
//     void AddExperience(int amount);
// }

// /// <summary>
// /// 示例：金币拾取物
// /// </summary>
// public class CoinPickup : PickupItem
// {
//     [SerializeField] private int coinValue = 1;

//     protected override void OnPickup()
//     {
//         // 给玩家加金币
//         // PlayerInventory.Instance.AddCoins(coinValue);

//         Destroy(gameObject);
//     }
// }

// ==================== 配置示例 ====================

/*
在 Unity Inspector 中配置 GhostData：

Ghost Data - 基础幽灵:
  Mana Bolus Drop Amount: 5
  Mana Bolus Drop Chance: 1.0
  
  Other Drops:
    - Item Prefab: ExperienceOrb
      Drop Chance: 0.5
      Min Count: 1
      Max Count: 3
    
    - Item Prefab: CoinPickup
      Drop Chance: 0.3
      Min Count: 1
      Max Count: 2
  
  Spawn Objects:
    - Object Prefab: TombstoneDecoration
      Spawn Chance: 0.2
      Position Offset: (0, 0, 0)
      Attach To Ground: true
    
    - Object Prefab: GhostEssenceFountain
      Spawn Chance: 0.1
      Position Offset: (0, 0.5, 0)
      Attach To Ground: true

---

Ghost Data - Boss幽灵:
  Mana Bolus Drop Amount: 50  // Boss掉落更多魔法值
  Mana Bolus Drop Chance: 1.0
  
  Other Drops:
    - Item Prefab: RareLootChest
      Drop Chance: 1.0
      Min Count: 1
      Max Count: 1
  
  Spawn Objects:
    - Object Prefab: BossDefeatedMonument
      Spawn Chance: 1.0
      Position Offset: (0, 0, 0)
      Attach To Ground: true
*/