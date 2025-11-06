using System.Linq;
using UnityEngine;

public class WeaponMeleeBehavior : ItemBehavior
{
    [Header("Melee Settings")]
    public float lightAttackCooldown = 0f;
    public float chargeThreshold = 0.8f;
    public float maxChargeTime = 2.0f;

    [Header("Damage Settings")]
    public float lightAttackDamage = 20f;
    public float chargedAttackMultiplier = 2.5f;
    public float attackRange = 2.5f;
    public float attackAngle = 180f; // 攻击扇形角度

    [Header("Hit Detection")]
    public LayerMask damageableLayers;
    public Transform attackPoint; // 攻击检测点（通常在武器前端）
    
    [Header("Visual Feedback")]
    public GameObject hitEffectPrefab;
    public TrailRenderer weaponTrail;

    private bool isCharging;
    private float chargeTimer;
    private float attackCooldownTimer;
    private Animator weaponAnimator;

    public override void Initialize(ItemBase item, PlayerWeapon weapon, PlayerUI ui)
    {
        base.Initialize(item, weapon, ui);
        
        // 从 ItemData 获取伤害值（必须有）
        lightAttackDamage = item.data.damage;
        attackRange = item.data.attackRange;

        damageableLayers = 1 << 8 | 1 << 9;

        weaponAnimator = GetComponent<Animator>();
        
        // 如果没有指定攻击点，使用武器前端
        if (attackPoint == null)
        {
            GameObject point = new GameObject("AttackPoint");
            point.transform.SetParent(transform);
            point.transform.localPosition = Vector3.forward * 1f;
            attackPoint = point.transform;
        }

        
    }

    void OnEnable()
    {
        GameEventManager.OnLightAttackHit += OnAttackHit;
    }

    void OnDisable()
    {
        GameEventManager.OnLightAttackHit -= OnAttackHit;
    }

    public override void OnPrimaryStart()
    {
        if (attackCooldownTimer > 0) return;

        isCharging = true;
        chargeTimer = 0f;
        PlayerUI.Instance.ShowProgressBar(true, BarType.Using);
    }

    public override void OnPrimaryUpdate(float deltaTime)
    {
        if (attackCooldownTimer > 0)
        {
            attackCooldownTimer -= deltaTime;
        }

        if (isCharging)
        {
            chargeTimer += deltaTime;
            PlayerUI.Instance.UpdateProgressBar(chargeTimer / maxChargeTime);
        }
    }

    public override void OnPrimaryEnd()
    {
        if (!isCharging) return;

        isCharging = false;
        PlayerUI.Instance.ShowProgressBar(false);

        float chargeRatio = Mathf.Clamp01(chargeTimer / maxChargeTime);

        if (chargeTimer < chargeThreshold)
        {
            PerformLightAttack();
        }
        else
        {
            PerformChargedAttack(chargeRatio);
        }

        attackCooldownTimer = lightAttackCooldown;
    }

    private void PerformLightAttack()
    {
        Debug.Log($"[近战] 轻攻击: {itemData.name}");
        
        // 播放动画
        PlayerIKController.Instance.SetAnimTrigger(AnimActionType.LightAttack);
        
        // 延迟伤害检测以匹配动画（使用动画事件更好）
        // StartCoroutine(DelayedDamageDetection(0.2f, lightAttackDamage, 1.0f));
        
        GameEventManager.TriggerWeaponAttack(itemData, AttackType.Light, 1.0f);
    }

    private void PerformChargedAttack(float chargeRatio)
    {
        Debug.Log($"[近战] 蓄力攻击: {itemData.name}, 蓄力={chargeRatio:F2}");
        
        float damage = lightAttackDamage * chargedAttackMultiplier * chargeRatio;
        
        weaponAnimator?.SetTrigger("ChargedAttack");
        
        StartCoroutine(DelayedDamageDetection(0.3f, damage, chargeRatio));
        
        GameEventManager.TriggerWeaponAttack(itemData, AttackType.Charged, chargeRatio);
    }

    private System.Collections.IEnumerator DelayedDamageDetection(float delay, float damage, float powerMultiplier)
    {
        yield return new WaitForSeconds(delay);
        
        // 启用武器拖尾
        if (weaponTrail != null)
        {
            weaponTrail.enabled = true;
            yield return new WaitForSeconds(0.3f);
            weaponTrail.enabled = false;
        }
        
        PerformDamageDetection(damage, powerMultiplier);
    }

    /// <summary>
    /// 执行伤害检测 - 使用扇形范围检测
    /// </summary>
    private void PerformDamageDetection(float damage, float powerMultiplier)
    {
        Vector3 origin = attackPoint != null ? attackPoint.position : transform.position;
        Vector3 forward = attackPoint != null ? attackPoint.forward : transform.forward;

        // 方法1: 使用 OverlapSphere + 角度检测（推荐用于近战）
        Collider[] hits = Physics.OverlapSphere(origin, attackRange, damageableLayers);

        Debug.Log("Collider[] hits " + hits.Length);
        int hitCount = 0;
        foreach (var hit in hits)
        {
            // 检查是否在攻击扇形内
            Vector3 directionToTarget = (hit.transform.position - origin).normalized;
            float angleToTarget = Vector3.Angle(forward, directionToTarget);
            Debug.Log(" name : " + hit.gameObject.name + " angleToTarget " + angleToTarget);
            if (angleToTarget <= attackAngle / 2f)
            {
                // 尝试造成伤害
                if (TryDamageTarget(hit.gameObject, damage, powerMultiplier, directionToTarget))
                {
                    hitCount++;
                    SpawnHitEffect(hit.ClosestPoint(origin));
                }
            }
        }

        Debug.Log($"[近战] 命中 {hitCount} 个目标，伤害={damage:F1}");
    }

    /// <summary>
    /// 尝试对目标造成伤害
    /// </summary>
    private bool TryDamageTarget(GameObject target, float damage, float knockbackPower, Vector3 direction)
    {
        Debug.Log("Try Damage Target"); 
        var damageable = target.GetComponent<IDamageable>();
        if (damageable == null)
        {
            damageable = target.GetComponentInParent<IDamageable>();
        }
        
        if (damageable != null && !damageable.IsDead)
        {
            Vector3 hitPoint = target.GetComponent<Collider>()?.ClosestPoint(attackPoint.position) ?? target.transform.position;
            
            DamageInfo damageInfo = new DamageInfo
            {
                damage = damage,
                damageType = DamageType.Melee,
                attacker = playerWeapon.gameObject,
                weaponData = itemData,
                knockbackDirection = direction,
                knockbackForce = 5f * knockbackPower,
                isCritical = damageable.IsWeakPoint(hitPoint)
            };
            
            damageable.TakeDamage(damageInfo);
            return true;
        }
        
        return false;
    }

    private void SpawnHitEffect(Vector3 position)
    {
        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, position, Quaternion.identity);
            Destroy(effect, 2f);
        }
    }

    // 供动画事件调用的方法
    public void OnAttackHit()
    {
        // 动画事件触发的伤害检测点
        PerformDamageDetection(lightAttackDamage, 1.0f);
    }

    public override void OnSecondaryStart() { }
    public override void OnSecondaryEnd() { }
    public override void OnSecondaryUpdate(float deltaTime) { }
    public override void OnUse() { }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        // 绘制攻击范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        
        // 绘制攻击扇形
        Vector3 forward = attackPoint.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, attackAngle / 2f, 0) * forward;
        Vector3 leftBoundary = Quaternion.Euler(0, -attackAngle / 2f, 0) * forward;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(attackPoint.position, rightBoundary * attackRange);
        Gizmos.DrawRay(attackPoint.position, leftBoundary * attackRange);
    }
#endif
}
public enum AttackType
{
    Light,
    Charged,
    Ranged
}