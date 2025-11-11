using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// ==================== 行为策略接口 ====================

/// <summary>
/// 移动行为接口
/// </summary>
public interface IGhostMovementBehavior
{
    void Move(GhostController ghost, Transform target);
    void Initialize(GhostController ghost);
}

/// <summary>
/// 攻击行为接口
/// </summary>
public interface IGhostAttackBehavior
{
    void Attack(GhostController ghost, Transform target);
    void Initialize(GhostController ghost);
}

/// <summary>
/// 受伤反应接口
/// </summary>
public interface IGhostDamageReaction
{
    void OnTakeDamage(GhostController ghost, DamageInfo damageInfo);
    void Initialize(GhostController ghost);
}

// ==================== 具体移动行为 ====================

/// <summary>
/// 直线追踪移动
/// </summary>
public class DirectChaseMovement : IGhostMovementBehavior
{
    private GhostController ghost;

    public void Initialize(GhostController ghost)
    {
        this.ghost = ghost;
    }

    public void Move(GhostController ghost, Transform target)
    {
        if (target == null) return;

        Vector3 direction = (target.position - ghost.transform.position).normalized;
        ghost.transform.position += direction * ghost.MoveSpeed * Time.deltaTime;
        
        // 面向目标
        ghost.transform.LookAt(new Vector3(target.position.x, ghost.transform.position.y, target.position.z));
    }
}

/// <summary>
/// 漂浮式移动（带上下浮动）
/// </summary>
public class FloatingChaseMovement : IGhostMovementBehavior
{
    private GhostController ghost;
    private float floatAmplitude = 0.5f;
    private float floatFrequency = 2f;
    private float startY;

    public void Initialize(GhostController ghost)
    {
        this.ghost = ghost;
        startY = ghost.transform.position.y;
    }

    public void Move(GhostController ghost, Transform target)
    {
        if (target == null) return;

        Vector3 direction = (target.position - ghost.transform.position).normalized;
        Vector3 horizontalMove = new Vector3(direction.x, 0, direction.z) * ghost.MoveSpeed * Time.deltaTime;
        
        // 上下浮动
        float newY = startY + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        
        ghost.transform.position += horizontalMove;
        ghost.transform.position = new Vector3(
            ghost.transform.position.x,
            newY,
            ghost.transform.position.z
        );
        
        ghost.transform.LookAt(new Vector3(target.position.x, ghost.transform.position.y, target.position.z));
    }
}

/// <summary>
/// 传送式移动（间隔传送）
/// </summary>
public class TeleportMovement : IGhostMovementBehavior
{
    private GhostController ghost;
    private float teleportInterval = 3f;
    private float lastTeleportTime;
    private float teleportRange = 5f;

    public void Initialize(GhostController ghost)
    {
        this.ghost = ghost;
        lastTeleportTime = Time.time;
    }

    public void Move(GhostController ghost, Transform target)
    {
        if (target == null) return;

        // 间隔传送
        if (Time.time >= lastTeleportTime + teleportInterval)
        {
            Vector3 directionToTarget = (target.position - ghost.transform.position).normalized;
            Vector3 teleportPos = ghost.transform.position + directionToTarget * teleportRange;
            
            ghost.transform.position = teleportPos;
            lastTeleportTime = Time.time;
            
            // 触发传送特效
            ghost.OnTeleport();
        }
    }
}

// ==================== 具体攻击行为 ====================

/// <summary>
/// 近战攻击
/// </summary>
public class MeleeAttack : IGhostAttackBehavior
{
    private GhostController ghost;

    public void Initialize(GhostController ghost)
    {
        this.ghost = ghost;
    }

    public void Attack(GhostController ghost, Transform target)
    {
        if (target == null || !ghost.Stat.CanAttack()) return;

        float distance = Vector3.Distance(ghost.transform.position, target.position);
        
        if (distance <= ghost.Stat.data.attackRange)
        {
            var damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                DamageInfo damageInfo = new DamageInfo
                {
                    damage = ghost.Stat.data.attackDamage,
                    attacker = ghost.gameObject,
                    damageType = DamageType.Ghost
                };
                
                damageable.TakeDamage(damageInfo);
                ghost.Stat.RecordAttack();
                ghost.OnAttackExecuted();
            }
            else if(target.CompareTag("Player"))
            {
                DamageInfo damageInfo = new DamageInfo
                {
                    damage = ghost.Stat.data.attackDamage,
                    attacker = ghost.gameObject,
                    damageType = DamageType.Ghost
                };

                GameInstance.Instance.PlayerStat.LoseHealth((int)damageInfo.damage);
                ghost.Stat.RecordAttack();
                ghost.OnAttackExecuted();
            }
        }
    }
}

/// <summary>
/// 爆炸攻击（自杀式）
/// </summary>
public class ExplosiveAttack : IGhostAttackBehavior
{
    private GhostController ghost;

    public void Initialize(GhostController ghost)
    {
        this.ghost = ghost;
    }

    public void Attack(GhostController ghost, Transform target)
    {
        if (target == null) return;

        float distance = Vector3.Distance(ghost.transform.position, target.position);
        
        if (distance <= ghost.Stat.data.attackRange)
        {
            // 范围伤害
            Collider[] hits = Physics.OverlapSphere(
                ghost.transform.position,
                ghost.Stat.data.explosionRadius
            );

            foreach (var hit in hits)
            {
                var damageable = hit.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    DamageInfo damageInfo = new DamageInfo
                    {
                        damage = ghost.Stat.data.attackDamage,
                        attacker = ghost.gameObject,
                        damageType = DamageType.Explosion
                    };
                    
                    damageable.TakeDamage(damageInfo);
                }
                else if(target.CompareTag("Player"))
                {
                    DamageInfo damageInfo = new DamageInfo
                    {
                        damage = ghost.Stat.data.attackDamage,
                        attacker = ghost.gameObject,
                        damageType = DamageType.Ghost
                    };

                    GameInstance.Instance.PlayerStat.LoseHealth((int)damageInfo.damage);
                }
            }

            ghost.OnAttackExecuted();
            ghost.Die(); // 爆炸后死亡
        }
    }
}

/// <summary>
/// 远程射击攻击
/// </summary>
public class RangedAttack : IGhostAttackBehavior
{
    private GhostController ghost;
    public GameObject projectilePrefab;

    public void Initialize(GhostController ghost)
    {
        this.ghost = ghost;
    }

    public void Attack(GhostController ghost, Transform target)
    {
        if (target == null || !ghost.Stat.CanAttack()) return;

        float distance = Vector3.Distance(ghost.transform.position, target.position);
        
        if (distance <= ghost.Stat.data.attackRange)
        {
            // 生成投射物
            if (projectilePrefab != null)
            {
                GameObject projectile = GameObject.Instantiate(
                    projectilePrefab,
                    ghost.transform.position + Vector3.up,
                    Quaternion.identity
                );

                var projectileScript = projectile.GetComponent<GhostProjectile>();
                if (projectileScript != null)
                {
                    projectileScript.Initialize(target, ghost.Stat.data.attackDamage);
                }
            }

            ghost.Stat.RecordAttack();
            ghost.OnAttackExecuted();
        }
    }
}

// ==================== 受伤反应 ====================

/// <summary>
/// 标准受伤反应
/// </summary>
public class StandardDamageReaction : IGhostDamageReaction
{
    private GhostController ghost;

    public void Initialize(GhostController ghost)
    {
        this.ghost = ghost;
    }

    public void OnTakeDamage(GhostController ghost, DamageInfo damageInfo)
    {
        ghost.Stat.LoseHealth((int)damageInfo.damage);
        ghost.OnDamaged(damageInfo);
    }
}

/// <summary>
/// 传送反应（受击时传送）
/// </summary>
public class TeleportDamageReaction : IGhostDamageReaction
{
    private GhostController ghost;
    private float teleportDistance = 8f;

    public void Initialize(GhostController ghost)
    {
        this.ghost = ghost;
    }

    public void OnTakeDamage(GhostController ghost, DamageInfo damageInfo)
    {
        ghost.Stat.LoseHealth((int)damageInfo.damage);
        ghost.OnDamaged(damageInfo);

        // 传送到随机位置
        if (!ghost.IsDead)
        {
            Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * teleportDistance;
            Vector3 teleportOffset = new Vector3(randomCircle.x, 0, randomCircle.y);
            ghost.transform.position += teleportOffset;
            ghost.OnTeleport();
        }
    }
}

/// <summary>
/// 分裂反应（受击时分裂成小幽灵）
/// </summary>
public class SplitDamageReaction : IGhostDamageReaction
{
    private GhostController ghost;
    private GameObject minorGhostPrefab;
    private bool hasSplit = false;
    private float splitHealthThreshold = 0.5f; // 50%血量时分裂

    public void Initialize(GhostController ghost)
    {
        this.ghost = ghost;
    }

    public void OnTakeDamage(GhostController ghost, DamageInfo damageInfo)
    {
        ghost.Stat.LoseHealth((int)damageInfo.damage);
        ghost.OnDamaged(damageInfo);

        // 血量低于阈值时分裂
        if (!hasSplit && ghost.CurrentHealth / ghost.MaxHealth <= splitHealthThreshold)
        {
            hasSplit = true;
            SpawnMinorGhosts(ghost);
        }
    }

    private void SpawnMinorGhosts(GhostController ghost)
    {
        if (minorGhostPrefab == null) return;

        // 生成2-3个小幽灵
        int count = UnityEngine.Random.Range(2, 4);
        for (int i = 0; i < count; i++)
        {
            Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * 2f;
            Vector3 spawnPos = ghost.transform.position + new Vector3(randomOffset.x, 0, randomOffset.y);
            
            var newGhost = GameObject.Instantiate(minorGhostPrefab, spawnPos, Quaternion.identity);
        }

        ghost.OnSplit();
    }
}