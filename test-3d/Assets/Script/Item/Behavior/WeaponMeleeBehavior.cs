using UnityEngine;

// ========== 近战武器行为 ==========
public class WeaponMeleeBehavior : ItemBehavior
{
    [Header("Melee Settings")]
    public float lightAttackCooldown = 0.5f;
    public float chargeThreshold = 0.8f;
    public float maxChargeTime = 2.0f;

    private bool isCharging;
    private float chargeTimer;
    private float attackCooldownTimer;

    public override void OnPrimaryStart()
    {
        if (attackCooldownTimer > 0) return;

        isCharging = true;
        chargeTimer = 0f;
        playerUI?.ShowChargeBar(true);
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
            playerUI?.UpdateChargeBar(chargeTimer / maxChargeTime);
        }
    }

    public override void OnPrimaryEnd()
    {
        if (!isCharging) return;

        isCharging = false;
        playerUI?.ShowChargeBar(false);

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
        // TODO: 播放动画
        // TODO: 触发伤害检测
        GameEventManager.TriggerWeaponAttack(itemData, AttackType.Light, 1.0f);
    }

    private void PerformChargedAttack(float chargeRatio)
    {
        Debug.Log($"[近战] 蓄力攻击: {itemData.name}, 蓄力={chargeRatio:F2}");
        // TODO: 播放蓄力攻击动画
        GameEventManager.TriggerWeaponAttack(itemData, AttackType.Charged, chargeRatio);
    }

    public override void OnSecondaryStart() { }
    public override void OnSecondaryEnd() { }
    public override void OnSecondaryUpdate(float deltaTime) { }
    public override void OnUse() { }
}

public enum AttackType
{
    Light,
    Charged,
    Ranged
}