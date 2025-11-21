using System.Collections;
using System.Collections.Generic;
using TMPro;
using TreeEditor;
using UnityEngine;
using UnityEngine.AI;

public class CreatureController : MonoBehaviour, IDamageable
{
    public string id;
    public CreatureStat stat;
    private Animator animator;
    private NavMeshAgent agent;
    private CreatureAI ai;

    public bool IsDead => stat?.GetHealth() <= 0;
    public float CurrentHealth => stat.GetHealth();
    public float MaxHealth => stat.GetMaxHealth();

    [Header("Weak Points")]
    public Transform[] weakPointTransforms; // 头部、背部等
    public float weakPointRadius = 0.3f;

    private bool isDead;
    [Header("死亡替换为Ragdoll")]
    public GameObject ragdollPrefab;
    public float ragdollLifetime = 120f;
    public float deathForce = 3f;

    [Header("伤害反馈")]
    public float damageToAngerMultiplier = 2f;   // 伤害转愤怒倍率
    public float damageToFearMultiplier = 1.5f;  // 伤害转恐惧倍率


    void Start()
    {
        stat = new CreatureStat(id);
        ai = gameObject.AddComponent<CreatureAI>();
        ai.Init(stat);

        animator = GetComponent<Animator>();

        agent = ai.agent;
    }

    public bool IsWeakPoint(Vector3 hitPoint)
    {
        foreach (var wp in weakPointTransforms)
        {
            if (Vector3.Distance(hitPoint, wp.position) <= weakPointRadius)
                return true;
        }
        return false;
    }

    public void TakeDamage(DamageInfo damageInfo)
    {
        if (IsDead) return;

        Debug.Log(stat.data.name + " Take Damage " + damageInfo.damage);

        float finalDamage = damageInfo.damage;
        
        // 暴击伤害
        if (damageInfo.isCritical)
        {
            finalDamage *= damageInfo.criticalMultiplier > 0 ? damageInfo.criticalMultiplier : 2f;
        }

        stat.LoseHealth((int)finalDamage);
        
        Debug.Log($"[敌人] 受到 {finalDamage:F1} 伤害 ({damageInfo.damageType}) " +
                  $"{(damageInfo.isCritical ? "【暴击】" : "")} 剩余生命: {stat.GetHealth():F1}/{stat.GetMaxHealth():F1}");

        // 显示伤害数字
        ShowDamageText(finalDamage, damageInfo.isCritical);

        // 击退效果
        ApplyKnockback(damageInfo.knockbackDirection, damageInfo.knockbackForce);

        // 受击动画
        // animator?.SetTrigger("Hit");

        if (IsDead)
        {
            OnDeath();
            return;
        }

        CalculateEmotionalResponse(damageInfo.damage, damageInfo.attacker);
    }

    private void ShowDamageText(float damage, bool isCritical)
    {
        FloatingTextPool.Instance.ShowDamage(transform.position + Vector3.up * 2f, (int)damage, isCritical, 1.5f);
    }

    private void ApplyKnockback(Vector3 direction, float force)
    {
        var rb = GetComponent<Rigidbody>();
        if (rb != null && force > 0)
        {
            rb.AddForce(direction * force, ForceMode.Impulse);
        }
    }

    private void CalculateEmotionalResponse(float damage, GameObject attacker)
    {
        // 基于伤害和性格计算情绪变化
        float damagePercent = damage / stat.GetMaxHealth() * 100f;
        
        // 计算愤怒增加（攻击性高的生物更容易愤怒）
        float angerIncrease = damagePercent * damageToAngerMultiplier * stat.personality.aggressiveness;
        stat.IncreaseAnger(angerIncrease);
        
        // 计算恐惧增加（勇气低的生物更容易害怕）
        float fearIncrease = damagePercent * damageToFearMultiplier * (1f - stat.personality.courage);
        stat.IncreaseFear(fearIncrease);
        
        // 如果生命值过低，大幅增加恐惧
        float healthPercent = (float)stat.GetHealth() / stat.GetMaxHealth();
        if (healthPercent < 0.3f)
        {
            stat.IncreaseFear(30f);
        }

        ai.OnDamageReceived(damage, attacker);
    }

    private void OnDeath()
    {
        Debug.Log($"[敌人] {gameObject.name} 死亡");
        Die();

        // 播放死亡动画
        // animator?.SetTrigger("Death");

        // 掉落物品
        // LootManager.Instance?.SpawnLoot(transform.position);
        
        // 延迟销毁
        // Destroy(gameObject, 2f);
    }

    /// <summary>
    /// 由CreatureStat或AI在死亡时调用
    /// </summary>
    public void Die()
    {
        // 1. 停止AI与控制
        if (ai != null) ai.enabled = false;
        if (agent != null) agent.enabled = false;
        if (animator != null) animator.enabled = false;

        // 2. 生成Ragdoll预制体
        if (ragdollPrefab != null)
        {
            GameObject ragdoll = Instantiate(ragdollPrefab, transform.position, transform.rotation);

            // 3. 复制骨骼姿势
            CopyPoseRecursive(transform, ragdoll.transform);

            // 4. 给Ragdoll施加惯性（继承NavMeshAgent速度）
            Vector3 velocity = agent != null ? agent.velocity : Vector3.zero;
            ApplyVelocityToRagdoll(ragdoll, velocity, deathForce);
            ragdoll.SetActive(true);

            // 5. 延迟销毁尸体
            Destroy(ragdoll, ragdollLifetime);
        }

        // 6. 销毁原AI对象
        Destroy(gameObject);
    }

    /// <summary>
    /// 递归复制Transform的姿势（根据名称匹配）
    /// </summary>
    private void CopyPoseRecursive(Transform source, Transform target)
    {
        foreach (Transform child in source)
        {
            Transform targetChild = target.Find(child.name);
            if (targetChild)
            {
                targetChild.localPosition = child.localPosition;
                targetChild.localRotation = child.localRotation;
                CopyPoseRecursive(child, targetChild);
            }
        }
    }

    /// <summary>
    /// 给ragdoll的每个Rigidbody施加速度和冲击力
    /// </summary>
    private void ApplyVelocityToRagdoll(GameObject ragdoll, Vector3 inheritVelocity, float impulseForce)
    {
        foreach (var rb in ragdoll.GetComponentsInChildren<Rigidbody>())
        {
            rb.velocity = inheritVelocity;
            rb.AddForce(Random.insideUnitSphere * impulseForce, ForceMode.Impulse);
        }
    }

}