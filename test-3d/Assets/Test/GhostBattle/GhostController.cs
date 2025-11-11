using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// ==================== 主控制器 ====================

public class GhostController : MonoBehaviour, IDamageable
{
    [Header("配置")]
    [SerializeField] private int ghostId;
    
    [Header("行为组件")]
    private IGhostMovementBehavior movementBehavior;
    private IGhostAttackBehavior attackBehavior;
    private IGhostDamageReaction damageReaction;

    private GhostState stat;
    private Transform target;
    private GhostBattleManager battleManager;

    // 事件
    // public event Action<DamageInfo> OnDamaged;
    // public event Action OnAttackExecuted;
    // public event Action OnTeleport;
    // public event Action OnSplit;

    public GhostState Stat => stat;
    public bool IsDead => stat.GetHealth() <= 0;
    public float CurrentHealth => stat.GetHealth();
    public float MaxHealth => stat.GetMaxHealth();
    public float MoveSpeed => stat.data.speed;

    private void Awake()
    {
        InitializeGhost(ghostId);
    }

    /// <summary>
    /// 初始化幽灵（对象池复用时调用）
    /// </summary>
    public void Initialize(GhostBattleManager manager, int id = -1)
    {
        battleManager = manager;
        
        if (id >= 0)
        {
            ghostId = id;
            InitializeGhost(id);
        }

        // 获取玩家目标
        target = GameInstance.Instance.PlayerStat?.GetPlayerTransform();
        
        // 重置状态
        stat.SetHealth(stat.GetMaxHealth());
    }

    private void InitializeGhost(int id)
    {
        stat = new GhostState(id);
        
        // 根据幽灵类型设置行为
        SetupBehaviors(stat.data.ghostType);
        
        // 订阅死亡事件
        stat.OnDeath += OnDeathHandler;
    }

    private void SetupBehaviors(GhostType type)
    {
        switch (type)
        {
            case GhostType.Basic:
                movementBehavior = new FloatingChaseMovement();
                attackBehavior = new MeleeAttack();
                damageReaction = new StandardDamageReaction();
                break;

            case GhostType.Teleporter:
                movementBehavior = new TeleportMovement();
                attackBehavior = new MeleeAttack();
                damageReaction = new TeleportDamageReaction();
                break;

            case GhostType.Explosive:
                movementBehavior = new DirectChaseMovement();
                attackBehavior = new ExplosiveAttack();
                damageReaction = new StandardDamageReaction();
                break;

            case GhostType.Shielded:
                movementBehavior = new FloatingChaseMovement();
                attackBehavior = new MeleeAttack();
                damageReaction = new StandardDamageReaction();
                break;

            case GhostType.Ranged:
                movementBehavior = new DirectChaseMovement();
                attackBehavior = new RangedAttack();
                damageReaction = new StandardDamageReaction();
                break;

            case GhostType.Summoner:
                movementBehavior = new FloatingChaseMovement();
                attackBehavior = new MeleeAttack();
                damageReaction = new SplitDamageReaction();
                break;

            default:
                movementBehavior = new DirectChaseMovement();
                attackBehavior = new MeleeAttack();
                damageReaction = new StandardDamageReaction();
                break;
        }

        // 初始化所有行为
        movementBehavior?.Initialize(this);
        attackBehavior?.Initialize(this);
        damageReaction?.Initialize(this);
    }

    private void Update()
    {
        if (IsDead || target == null) return;

        // 执行移动
        movementBehavior?.Move(this, target);

        // 执行攻击
        attackBehavior?.Attack(this, target);
    }

    public void TakeDamage(DamageInfo damageInfo)
    {
        if (IsDead) return;

        // 执行受伤反应
        damageReaction?.OnTakeDamage(this, damageInfo);
    }

    public bool IsWeakPoint(Vector3 hitPoint)
    {
        // 可以根据hitPoint判断是否击中弱点
        // 例如：头部是弱点
        float headHeight = transform.position.y + 1.5f;
        return hitPoint.y >= headHeight;
    }

    public void OnDamaged(DamageInfo damageInfo)
    {
        if(IsDead) Destroy(gameObject);
    }
    public void OnAttackExecuted()
    {

    }
    public void OnTeleport()
    {

    }
    public void OnSplit()
    {

    }

    private void OnDeathHandler()
    {
        Die();
    }

    public void Die()
    {
        // 播放死亡特效
        // TODO: Play death VFX/SFX

        // 返回对象池
        if (battleManager != null)
        {
            battleManager.ReturnGhostToPool(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (stat != null)
        {
            stat.OnDeath -= OnDeathHandler;
        }
    }

    // 绘制攻击范围（调试用）
    private void OnDrawGizmosSelected()
    {
        if (stat != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, stat.data.attackRange);
            
            if (stat.data.explosionRadius > 0)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, stat.data.explosionRadius);
            }
        }
    }
}

// ==================== 辅助类 ====================

/// <summary>
/// 幽灵投射物
/// </summary>
public class GhostProjectile : MonoBehaviour
{
    private Transform target;
    private int damage;
    private float speed = 10f;

    public void Initialize(Transform target, int damage)
    {
        this.target = target;
        this.damage = damage;
    }

    private void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        // 检测碰撞
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        if (distanceToTarget < 0.5f)
        {
            var damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                DamageInfo damageInfo = new DamageInfo
                {
                    damage = damage,
                    attacker = transform.gameObject,
                    damageType = DamageType.Projectile
                };
                damageable.TakeDamage(damageInfo);
            }
            Destroy(gameObject);
        }
    }
}
