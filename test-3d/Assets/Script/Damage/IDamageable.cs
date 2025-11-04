using UnityEngine;

public enum DamageType
{
    Melee,      // 近战
    Ranged,     // 远程
    Magic,      // 魔法
    Fire,       // 火焰
    Ice,        // 冰霜
    Poison      // 毒素
}

public struct DamageInfo
{
    public float damage;
    public DamageType damageType;
    public GameObject attacker;
    public ItemData weaponData;
    public Vector3 knockbackDirection;
    public float knockbackForce;
    public bool isCritical;
    public float criticalMultiplier;
}

public interface IDamageable
{
    void TakeDamage(DamageInfo damageInfo);
    bool IsWeakPoint(Vector3 hitPoint);
    bool IsDead { get; }
    float CurrentHealth { get; }
    float MaxHealth { get; }
}