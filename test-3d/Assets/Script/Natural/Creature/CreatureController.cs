using System.Collections;
using System.Collections.Generic;
using TMPro;
using TreeEditor;
using UnityEngine;

public class CreatureController : MonoBehaviour, IDamageable
{
    public string id;
    public CreatureStat stat;

    private bool isDead;

    [Header("Feedback")]
    public GameObject damageTextPrefab;
    public Color normalDamageColor = Color.white;
    public Color criticalDamageColor = Color.red;

    public bool IsDead => stat?.GetHealth() < 0;
    public float CurrentHealth => stat.GetHealth();
    public float MaxHealth => stat.GetMaxHealth();

    void Start()
    {
        stat = new CreatureStat(id);
        var tigerAI = gameObject.AddComponent<CreatureAI>();
        tigerAI.Init(stat);
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
        }
    }

    private void ShowDamageText(float damage, bool isCritical)
    {
        if (damageTextPrefab == null) return;

        GameObject textObj = Instantiate(damageTextPrefab, transform.position + Vector3.up * 2f, Quaternion.identity);
        var text = textObj.GetComponent<TextMeshPro>();
        if (text != null)
        {
            text.text = $"{damage:F0}{(isCritical ? "!" : "")}";
            text.color = isCritical ? criticalDamageColor : normalDamageColor;
        }
        Destroy(textObj, 1.5f);
    }

    private void ApplyKnockback(Vector3 direction, float force)
    {
        var rb = GetComponent<Rigidbody>();
        if (rb != null && force > 0)
        {
            rb.AddForce(direction * force, ForceMode.Impulse);
        }
    }

    private void OnDeath()
    {
        Debug.Log($"[敌人] {gameObject.name} 死亡");
        
        // 播放死亡动画
        // animator?.SetTrigger("Death");
        
        // 掉落物品
        // LootManager.Instance?.SpawnLoot(transform.position);
        
        // 延迟销毁
        Destroy(gameObject, 2f);
    }
}
