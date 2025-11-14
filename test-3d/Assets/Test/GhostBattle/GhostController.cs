using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// ==================== 主控制器 ====================

public class GhostController : MonoBehaviour, IDamageable
{
    [Header("幽灵ID")]
    [SerializeField] private int ghostId;

    [Header("特效预制体（可选）")]
    [SerializeField] private GameObject hurtVFXPrefab;
    [SerializeField] private GameObject deathVFXPrefab;
    [SerializeField] private GameObject teleportVFXPrefab;
    [SerializeField] private GameObject attackVFXPrefab;

    // 核心组件
    private GhostState stat;
    private GhostAnimationController animController;
    private GhostVFXController vfxController;
    
    // 行为组件
    private IGhostMovementBehavior movementBehavior;
    private IGhostAttackBehavior attackBehavior;
    private IGhostDamageReaction damageReaction;

    private Transform target;
    private GhostBattleManager battleManager;
    private bool isInitialized = false;

    // 事件
    public Action<DamageInfo> OnDamaged;
    public Action OnAttackExecuted;
    public Action OnTeleport;
    public Action OnSplit;

    // 属性
    public int GhostId => ghostId;
    public GhostState Stat => stat;
    public bool IsDead => stat != null && stat.GetHealth() <= 0;
    public float CurrentHealth => stat != null ? stat.GetHealth() : 0;
    public float MaxHealth => stat != null ? stat.GetMaxHealth() : 0;
    public float MoveSpeed => stat != null ? stat.data.speed : 0;

    private void Awake()
    {
        // 如果在编辑器中设置了ghostId，进行初始化
        if (ghostId > 0)
        {
            InitializeGhost(ghostId);
        }
    }

    /// <summary>
    /// 对象池复用时的初始化
    /// </summary>
    public void Initialize(GhostBattleManager manager, int id)
    {
        battleManager = manager;
        ghostId = id;

        // 初始化幽灵数据和行为
        InitializeGhost(id);

        // 获取玩家目标
        target = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (target == null)
        {
            Debug.LogWarning("Player target not found!");
        }

        // 重置状态
        if (stat != null)
        {
            stat.SetHealth(stat.GetMaxHealth());
        }

        // 重置动画和特效
        animController?.ResetAnimator();
        vfxController?.ResetEffects();

        isInitialized = true;
    }

    /// <summary>
    /// 初始化幽灵的所有组件和行为
    /// </summary>
    private void InitializeGhost(int id)
    {
        // 创建状态
        stat = new GhostState(id);
        stat.OnDeath += OnDeathHandler;
        stat.OnHealthChanged += OnHealthChangedHandler;
        stat.OnShieldBroken += OnShieldBrokenHandler;

        // 初始化动画控制器
        animController = new GhostAnimationController(this);

        // 初始化特效控制器
        vfxController = new GhostVFXController(this);
        vfxController.hurtVFXPrefab = hurtVFXPrefab;
        vfxController.deathVFXPrefab = deathVFXPrefab;
        vfxController.teleportVFXPrefab = teleportVFXPrefab;
        vfxController.attackVFXPrefab = attackVFXPrefab;

        // 根据幽灵类型设置行为
        SetupBehaviors(stat.data.ghostType);

        // 订阅事件
        OnDamaged += HandleDamaged;
        OnAttackExecuted += HandleAttackExecuted;
        OnTeleport += HandleTeleport;
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

        movementBehavior?.Initialize(this);
        attackBehavior?.Initialize(this);
        damageReaction?.Initialize(this);
    }

    private void Update()
    {
        if (!isInitialized || IsDead || target == null) return;

        // 执行移动
        movementBehavior?.Move(this, target);

        // 更新移动动画
        float currentSpeed = Vector3.Distance(transform.position, target.position) < 1f ? 0 : MoveSpeed;
        animController?.UpdateMovement(currentSpeed);

        // 执行攻击
        attackBehavior?.Attack(this, target);
    }

    public void TakeDamage(DamageInfo damageInfo)
    {
        if (IsDead || !isInitialized) return;

        damageReaction?.OnTakeDamage(this, damageInfo);
    }

    public bool IsWeakPoint(Vector3 hitPoint)
    {
        float headHeight = transform.position.y + 1.5f;
        return hitPoint.y >= headHeight;
    }

    // ========== 事件处理器 ==========

    private void HandleDamaged(DamageInfo damageInfo)
    {
        animController?.PlayHurt();
        vfxController?.PlayHurtEffect(damageInfo.hitPoint);
    }

    private void HandleAttackExecuted()
    {
        animController?.PlayAttack();
        vfxController?.PlayAttackEffect();
    }

    private void HandleTeleport()
    {
        animController?.PlayTeleport();
        vfxController?.PlayTeleportEffect();
    }

    private void OnHealthChangedHandler(int currentHealth, int maxHealth)
    {
        // 可以在这里更新UI血条
        // UIManager.Instance?.UpdateGhostHealthBar(this, currentHealth, maxHealth);
    }

    private void OnShieldBrokenHandler()
    {
        Debug.Log($"Ghost {ghostId} shield broken!");
        // 播放护盾破碎特效
    }

    private void OnDeathHandler()
    {
        Die();
    }

    public void Die()
    {
        if (!isInitialized) return;

        // 播放死亡动画和特效
        animController?.PlayDeath();
        vfxController?.PlayDeathEffect();

        // 延迟返回对象池，等待死亡动画播放
        StartCoroutine(DelayedReturn());
    }

    private IEnumerator DelayedReturn()
    {
        // 等待死亡动画播放完成
        yield return new WaitForSeconds(1f);

        // 返回对象池
        isInitialized = false;
        
        if (battleManager != null)
        {
            battleManager.ReturnGhostToPool(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDisable()
    {
        // 清理事件订阅
        if (stat != null)
        {
            stat.OnDeath -= OnDeathHandler;
            stat.OnHealthChanged -= OnHealthChangedHandler;
            stat.OnShieldBroken -= OnShieldBrokenHandler;
        }

        OnDamaged -= HandleDamaged;
        OnAttackExecuted -= HandleAttackExecuted;
        OnTeleport -= HandleTeleport;
    }

    // ========== 调试可视化 ==========

    private void OnDrawGizmosSelected()
    {
        if (stat != null)
        {
            // 攻击范围
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, stat.data.attackRange);

            // 爆炸范围
            if (stat.data.explosionRadius > 0)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, stat.data.explosionRadius);
            }

            // 到玩家的连线
            if (target != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, target.position);
            }
        }
    }
}

/// <summary>
/// 幽灵动画控制器
/// </summary>
public class GhostAnimationController
{
    private Animator animator;
    private GhostController ghost;
    
    // 动画参数名称（常量）
    private static readonly int PARAM_SPEED = Animator.StringToHash("Speed");
    private static readonly int PARAM_IS_ATTACKING = Animator.StringToHash("IsAttacking");
    private static readonly int PARAM_IS_HURT = Animator.StringToHash("IsHurt");
    private static readonly int PARAM_IS_DEAD = Animator.StringToHash("IsDead");
    private static readonly int TRIGGER_ATTACK = Animator.StringToHash("Attack");
    private static readonly int TRIGGER_HURT = Animator.StringToHash("Hurt");
    private static readonly int TRIGGER_DEATH = Animator.StringToHash("Death");
    private static readonly int TRIGGER_TELEPORT = Animator.StringToHash("Teleport");

    public GhostAnimationController(GhostController ghost)
    {
        this.ghost = ghost;
        animator = ghost.GetComponentInChildren<Animator>();
        
        if (animator == null)
        {
            Debug.LogWarning($"No Animator found on ghost {ghost.name}");
        }
    }

    public void UpdateMovement(float speed)
    {
        if (animator == null) return;
        animator.SetFloat(PARAM_SPEED, speed);
    }

    public void PlayAttack()
    {
        if (animator == null) return;
        animator.SetTrigger(TRIGGER_ATTACK);
    }

    public void PlayHurt()
    {
        if (animator == null) return;
        animator.SetTrigger(TRIGGER_HURT);
    }

    public void PlayDeath()
    {
        if (animator == null) return;
        animator.SetTrigger(TRIGGER_DEATH);
        animator.SetBool(PARAM_IS_DEAD, true);
    }

    public void PlayTeleport()
    {
        if (animator == null) return;
        animator.SetTrigger(TRIGGER_TELEPORT);
    }

    public void ResetAnimator()
    {
        if (animator == null) return;
        
        animator.SetFloat(PARAM_SPEED, 0);
        animator.SetBool(PARAM_IS_ATTACKING, false);
        animator.SetBool(PARAM_IS_HURT, false);
        animator.SetBool(PARAM_IS_DEAD, false);
    }

    public bool HasAnimator() => animator != null;
}

/// <summary>
/// 幽灵视觉效果控制器
/// </summary>
public class GhostVFXController
{
    private GhostController ghost;
    private ParticleSystem[] particleSystems;
    private TrailRenderer[] trailRenderers;
    
    // 特效预制体引用
    public GameObject hurtVFXPrefab;
    public GameObject deathVFXPrefab;
    public GameObject teleportVFXPrefab;
    public GameObject attackVFXPrefab;

    public GhostVFXController(GhostController ghost)
    {
        this.ghost = ghost;
        particleSystems = ghost.GetComponentsInChildren<ParticleSystem>();
        trailRenderers = ghost.GetComponentsInChildren<TrailRenderer>();
    }

    public void PlayHurtEffect(Vector3 hitPoint)
    {
        if (hurtVFXPrefab != null)
        {
            GameObject vfx = GameObject.Instantiate(hurtVFXPrefab, hitPoint, Quaternion.identity);
            GameObject.Destroy(vfx, 2f);
        }

        // 闪烁效果
        ghost.StartCoroutine(FlashEffect());
    }

    public void PlayDeathEffect()
    {
        if (deathVFXPrefab != null)
        {
            GameObject vfx = GameObject.Instantiate(deathVFXPrefab, ghost.transform.position, Quaternion.identity);
            GameObject.Destroy(vfx, 3f);
        }

        // 停止所有粒子系统
        foreach (var ps in particleSystems)
        {
            ps.Stop();
        }
    }

    public void PlayTeleportEffect()
    {
        if (teleportVFXPrefab != null)
        {
            // 在旧位置播放消失特效
            GameObject vfxOut = GameObject.Instantiate(teleportVFXPrefab, ghost.transform.position, Quaternion.identity);
            GameObject.Destroy(vfxOut, 2f);
        }
    }

    public void PlayAttackEffect()
    {
        if (attackVFXPrefab != null)
        {
            GameObject vfx = GameObject.Instantiate(
                attackVFXPrefab, 
                ghost.transform.position + ghost.transform.forward * 1f, 
                ghost.transform.rotation
            );
            GameObject.Destroy(vfx, 2f);
        }
    }

    private IEnumerator FlashEffect()
    {
        Renderer[] renderers = ghost.GetComponentsInChildren<Renderer>();
        Color originalColor = Color.white;
        
        // 变红
        foreach (var renderer in renderers)
        {
            foreach (var material in renderer.materials)
            {
                if (material.HasProperty("_Color"))
                {
                    originalColor = material.color;
                    material.color = Color.red;
                }
            }
        }

        yield return new WaitForSeconds(0.1f);

        // 恢复原色
        foreach (var renderer in renderers)
        {
            foreach (var material in renderer.materials)
            {
                if (material.HasProperty("_Color"))
                {
                    material.color = originalColor;
                }
            }
        }
    }

    public void ResetEffects()
    {
        // 重启粒子系统
        foreach (var ps in particleSystems)
        {
            ps.Clear();
            ps.Play();
        }

        // 清除拖尾
        foreach (var trail in trailRenderers)
        {
            trail.Clear();
        }
    }
}

// public class GhostController : MonoBehaviour, IDamageable
// {
//     [Header("配置")]
//     [SerializeField] private int ghostId;
    
//     [Header("行为组件")]
//     private IGhostMovementBehavior movementBehavior;
//     private IGhostAttackBehavior attackBehavior;
//     private IGhostDamageReaction damageReaction;

//     private GhostState stat;
//     private Transform target;
//     private GhostBattleManager battleManager;

//     // 事件
//     // public event Action<DamageInfo> OnDamaged;
//     // public event Action OnAttackExecuted;
//     // public event Action OnTeleport;
//     // public event Action OnSplit;

//     public GhostState Stat => stat;
//     public bool IsDead => stat.GetHealth() <= 0;
//     public float CurrentHealth => stat.GetHealth();
//     public float MaxHealth => stat.GetMaxHealth();
//     public float MoveSpeed => stat.data.speed;

//     private void Awake()
//     {
//         InitializeGhost(ghostId);
//     }

//     /// <summary>
//     /// 初始化幽灵（对象池复用时调用）
//     /// </summary>
//     public void Initialize(GhostBattleManager manager, int id = -1)
//     {
//         battleManager = manager;
        
//         if (id >= 0)
//         {
//             ghostId = id;
//             InitializeGhost(id);
//         }

//         // 获取玩家目标
//         target = GameInstance.Instance.PlayerStat?.GetPlayerTransform();
        
//         // 重置状态
//         stat.SetHealth(stat.GetMaxHealth());
//     }

//     private void InitializeGhost(int id)
//     {
//         stat = new GhostState(id);
        
//         // 根据幽灵类型设置行为
//         SetupBehaviors(stat.data.ghostType);
        
//         // 订阅死亡事件
//         stat.OnDeath += OnDeathHandler;
//     }

//     private void SetupBehaviors(GhostType type)
//     {
//         switch (type)
//         {
//             case GhostType.Basic:
//                 movementBehavior = new FloatingChaseMovement();
//                 attackBehavior = new MeleeAttack();
//                 damageReaction = new StandardDamageReaction();
//                 break;

//             case GhostType.Teleporter:
//                 movementBehavior = new TeleportMovement();
//                 attackBehavior = new MeleeAttack();
//                 damageReaction = new TeleportDamageReaction();
//                 break;

//             case GhostType.Explosive:
//                 movementBehavior = new DirectChaseMovement();
//                 attackBehavior = new ExplosiveAttack();
//                 damageReaction = new StandardDamageReaction();
//                 break;

//             case GhostType.Shielded:
//                 movementBehavior = new FloatingChaseMovement();
//                 attackBehavior = new MeleeAttack();
//                 damageReaction = new StandardDamageReaction();
//                 break;

//             case GhostType.Ranged:
//                 movementBehavior = new DirectChaseMovement();
//                 attackBehavior = new RangedAttack();
//                 damageReaction = new StandardDamageReaction();
//                 break;

//             case GhostType.Summoner:
//                 movementBehavior = new FloatingChaseMovement();
//                 attackBehavior = new MeleeAttack();
//                 damageReaction = new SplitDamageReaction();
//                 break;

//             default:
//                 movementBehavior = new DirectChaseMovement();
//                 attackBehavior = new MeleeAttack();
//                 damageReaction = new StandardDamageReaction();
//                 break;
//         }

//         // 初始化所有行为
//         movementBehavior?.Initialize(this);
//         attackBehavior?.Initialize(this);
//         damageReaction?.Initialize(this);
//     }

//     private void Update()
//     {
//         if (IsDead || target == null) return;

//         // 执行移动
//         movementBehavior?.Move(this, target);

//         // 执行攻击
//         attackBehavior?.Attack(this, target);
//     }

//     public void TakeDamage(DamageInfo damageInfo)
//     {
//         if (IsDead) return;

//         // 执行受伤反应
//         damageReaction?.OnTakeDamage(this, damageInfo);
//     }

//     public bool IsWeakPoint(Vector3 hitPoint)
//     {
//         // 可以根据hitPoint判断是否击中弱点
//         // 例如：头部是弱点
//         float headHeight = transform.position.y + 1.5f;
//         return hitPoint.y >= headHeight;
//     }

//     public void OnDamaged(DamageInfo damageInfo)
//     {
//         if(IsDead) Destroy(gameObject);
//     }
//     public void OnAttackExecuted()
//     {

//     }
//     public void OnTeleport()
//     {

//     }
//     public void OnSplit()
//     {

//     }

//     private void OnDeathHandler()
//     {
//         Die();
//     }

//     public void Die()
//     {
//         // 播放死亡特效
//         // TODO: Play death VFX/SFX

//         // 返回对象池
//         if (battleManager != null)
//         {
//             battleManager.ReturnGhostToPool(gameObject);
//         }
//         else
//         {
//             Destroy(gameObject);
//         }
//     }

//     private void OnDestroy()
//     {
//         if (stat != null)
//         {
//             stat.OnDeath -= OnDeathHandler;
//         }
//     }

//     // 绘制攻击范围（调试用）
//     private void OnDrawGizmosSelected()
//     {
//         if (stat != null)
//         {
//             Gizmos.color = Color.red;
//             Gizmos.DrawWireSphere(transform.position, stat.data.attackRange);
            
//             if (stat.data.explosionRadius > 0)
//             {
//                 Gizmos.color = Color.yellow;
//                 Gizmos.DrawWireSphere(transform.position, stat.data.explosionRadius);
//             }
//         }
//     }
// }

// // ==================== 辅助类 ====================

// /// <summary>
// /// 幽灵投射物
// /// </summary>
// public class GhostProjectile : MonoBehaviour
// {
//     private Transform target;
//     private int damage;
//     private float speed = 10f;

//     public void Initialize(Transform target, int damage)
//     {
//         this.target = target;
//         this.damage = damage;
//     }

//     private void Update()
//     {
//         if (target == null)
//         {
//             Destroy(gameObject);
//             return;
//         }

//         Vector3 direction = (target.position - transform.position).normalized;
//         transform.position += direction * speed * Time.deltaTime;

//         // 检测碰撞
//         float distanceToTarget = Vector3.Distance(transform.position, target.position);
//         if (distanceToTarget < 0.5f)
//         {
//             var damageable = target.GetComponent<IDamageable>();
//             if (damageable != null)
//             {
//                 DamageInfo damageInfo = new DamageInfo
//                 {
//                     damage = damage,
//                     attacker = transform.gameObject,
//                     damageType = DamageType.Projectile
//                 };
//                 damageable.TakeDamage(damageInfo);
//             }
//             Destroy(gameObject);
//         }
//     }
// }
