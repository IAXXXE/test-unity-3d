// ==================== 数据层 ====================
using System;
using UnityEngine;

[System.Serializable]
public class GhostData
{
    public int id;
    public string ghostName;
    public GhostType ghostType;
    public int maxHealth;
    public float speed;
    public float attackRange;
    public int attackDamage;
    public float attackCooldown;
    
    // 特殊属性
    public bool hasShield;          // 是否有护盾
    public bool teleportOnHit;      // 受击时传送
    public float explosionRadius;   // 爆炸范围（爆炸型幽灵）
}

public enum GhostType
{
    Basic,          // 基础幽灵：直接飘向玩家
    Teleporter,     // 传送型：受击时传送
    Explosive,      // 爆炸型：接近玩家时爆炸
    Shielded,       // 护盾型：需要破盾才能伤害
    Summoner,       // 召唤型：召唤小幽灵
    Ranged          // 远程型：远程攻击
}

// ==================== 状态层 ====================
public class GhostState : ICharacterStat
{
    public GhostData data { get; private set; }
    
    protected int health;
    protected int maxHealth;
    protected bool hasShield;
    protected float lastAttackTime;

    public event Action OnDeath;
    public event Action<int, int> OnHealthChanged; // currentHealth, maxHealth
    public event Action OnShieldBroken;

    public GhostState(int id)
    {
        data = CharacterDatabase.Instance.GetGhostData(id);

        maxHealth = data.maxHealth;
        health = data.maxHealth;
        hasShield = data.hasShield;
        lastAttackTime = -data.attackCooldown;
    }
    
    public virtual int GetHealth() => health;
    public virtual int GetMaxHealth() => maxHealth;
    public bool HasShield() => hasShield;
    public bool CanAttack() => Time.time >= lastAttackTime + data.attackCooldown;

    public void RecordAttack()
    {
        lastAttackTime = Time.time;
    }

    public virtual void Heal(int value)
    {
        var newHealth = health + value;
        SetHealth(newHealth);
    }

    public virtual void IncreaseMaxHealth(int value)
    {
        maxHealth += value;
        health += value;
        OnHealthChanged?.Invoke(health, maxHealth);
    }

    public virtual void LoseHealth(int value)
    {
        // 如果有护盾，先破盾
        if (hasShield)
        {
            hasShield = false;
            OnShieldBroken?.Invoke();
            return;
        }

        var newHealth = health - value;
        SetHealth(newHealth);
        
        if (health <= 0)
        {
            OnDeath?.Invoke();
        }
    }

    public virtual void SetHealth(int value)
    {
        if (value < 0) value = 0;
        if (value > maxHealth) value = maxHealth;

        health = value;
        OnHealthChanged?.Invoke(health, maxHealth);
    }

    public void RemoveShield()
    {
        if (hasShield)
        {
            hasShield = false;
            OnShieldBroken?.Invoke();
        }
    }
}