using UnityEditor;
using UnityEngine;
// // ==================== 辅助类 ====================

/// <summary>
/// 幽灵投射物
/// </summary>
public class GhostProjectile : MonoBehaviour
{
    private Transform target;
    private int damage;
    private float speed = 2f;

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
        var targetPos = new Vector3(target.position.x, target.position.y + 1, target.position.z);
        Vector3 direction = (targetPos - transform.position).normalized;
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
