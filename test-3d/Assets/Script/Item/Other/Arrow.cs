using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Arrow : MonoBehaviour
{
    [Header("Settings")]
    public float gravity = 9.81f;
    public bool stickToTarget = true;
    public LayerMask hitLayers = -1; // 默认所有层
    public bool useRaycast = true;   // 是否使用射线检测
    public bool useTrigger = true;   // 是否使用触发器检测
    
    [Header("Debug")]
    public bool showDebugRays = true;

    private float damage;
    private float speed;
    private Vector3 direction;
    private GameObject shooter;
    private float lifetime;
    private bool hasHit = false;
    private Vector3 lastPosition;
    
    private Rigidbody rb;
    private TrailRenderer trail;
    private Collider arrowCollider;

    public void Initialize(float dmg, float spd, Vector3 dir, GameObject source, float life)
    {
        damage = dmg;
        speed = spd;
        direction = dir.normalized;
        shooter = source;
        lifetime = life;

        rb = GetComponent<Rigidbody>();
        trail = GetComponent<TrailRenderer>();
        arrowCollider = GetComponent<Collider>();

        // 配置刚体
        rb.useGravity = false; // 手动处理重力
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; // 重要！防止高速穿透
        rb.velocity = direction * speed;
        
        // 配置碰撞体
        if (arrowCollider != null)
        {
            arrowCollider.isTrigger = useTrigger; // 使用触发器模式更可靠
        }
        
        // 记录初始位置
        lastPosition = transform.position;
        
        // 设置旋转朝向飞行方向
        transform.rotation = Quaternion.LookRotation(direction);

        // 忽略与射手的碰撞
        if (shooter != null)
        {
            Collider[] shooterColliders = shooter.GetComponentsInChildren<Collider>();
            foreach (var col in shooterColliders)
            {
                if (arrowCollider != null)
                {
                    Physics.IgnoreCollision(arrowCollider, col);
                }
            }
        }

        // 自动销毁
        Destroy(gameObject, lifetime);
        
        Debug.Log($"[箭矢] 初始化完成 - 速度:{speed}, 方向:{direction}, HitLayers:{hitLayers.value}");
    }

    private void FixedUpdate()
    {
        if (hasHit) return;

        // 应用重力
        rb.velocity += Vector3.down * gravity * Time.fixedDeltaTime;

        // 更新旋转以匹配飞行方向
        if (rb.velocity.magnitude > 0.1f)
        {
            transform.rotation = Quaternion.LookRotation(rb.velocity);
        }

        // 射线检测碰撞（可选，作为额外保险）
        if (useRaycast)
        {
            CheckCollisionWithRaycast();
        }
        
        lastPosition = transform.position;
    }

    private void CheckCollisionWithRaycast()
    {
        Vector3 currentPosition = transform.position;
        Vector3 moveDirection = currentPosition - lastPosition;
        float moveDistance = moveDirection.magnitude;

        if (moveDistance < 0.01f) return;

        // 绘制调试射线
        if (showDebugRays)
        {
            Debug.DrawRay(lastPosition, moveDirection, Color.red, 1f);
        }

        // 从上一帧位置发射射线到当前位置
        RaycastHit[] hits = Physics.RaycastAll(lastPosition, moveDirection.normalized, moveDistance, hitLayers);
        
        if (hits.Length > 0)
        {
            // 找到最近的击中点
            RaycastHit closestHit = hits[0];
            float closestDistance = hits[0].distance;
            
            foreach (var hit in hits)
            {
                // 忽略射手
                if (hit.collider.gameObject == shooter || 
                    (shooter != null && hit.collider.transform.IsChildOf(shooter.transform)))
                {
                    continue;
                }
                
                // 忽略箭矢自己
                if (hit.collider.gameObject == gameObject)
                {
                    continue;
                }
                
                if (hit.distance < closestDistance)
                {
                    closestHit = hit;
                    closestDistance = hit.distance;
                }
            }
            
            // 确保 collider 不为空
            if (closestHit.collider != null && 
                closestHit.collider.gameObject != shooter &&
                closestHit.collider.gameObject != gameObject)
            {
                Debug.Log($"[箭矢-射线] 检测到碰撞: {closestHit.collider.name} 距离:{closestDistance:F2}");
                OnHit(closestHit.collider, closestHit.point, closestHit.normal);
            }
        }
    }

    // 触发器检测（主要方法）
    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;
        
        // 忽略射手
        if (other.gameObject == shooter || 
            (shooter != null && other.transform.IsChildOf(shooter.transform)))
        {
            return;
        }

        // 检查Layer
        if (hitLayers != (hitLayers | (1 << other.gameObject.layer)))
        {
            Debug.Log($"[箭矢-触发器] 忽略层级: {other.gameObject.name} (Layer: {LayerMask.LayerToName(other.gameObject.layer)})");
            return;
        }

        Debug.Log($"[箭矢-触发器] 检测到碰撞: {other.name}");
        
        Vector3 hitPoint = other.ClosestPoint(transform.position);
        Vector3 hitNormal = (transform.position - hitPoint).normalized;
        
        OnHit(other, hitPoint, hitNormal);
    }

    // 物理碰撞检测（备用方法）
    private void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return;
        
        // 忽略射手
        if (collision.gameObject == shooter || 
            (shooter != null && collision.transform.IsChildOf(shooter.transform)))
        {
            return;
        }

        if (collision.contacts.Length > 0)
        {
            Debug.Log($"[箭矢-碰撞] 检测到碰撞: {collision.collider.name}");
            OnHit(collision.collider, collision.contacts[0].point, collision.contacts[0].normal);
        }
    }

    private void OnHit(Collider hitCollider, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (hasHit) return;
        if (hitCollider == null)
        {
            Debug.LogError("[箭矢] OnHit 收到 null collider！");
            return;
        }
        
        hasHit = true;

        Debug.Log($"[箭矢] 命中: {hitCollider.name}, 位置:{hitPoint}, GameObject:{hitCollider.gameObject.name}");

        // 尝试造成伤害
        var damageable = hitCollider.GetComponent<IDamageable>();
        if (damageable == null)
        {
            // 尝试在父对象中查找
            damageable = hitCollider.GetComponentInParent<IDamageable>();
        }
        
        if (damageable != null)
        {
            DamageInfo damageInfo = new DamageInfo
            {
                damage = damage,
                damageType = DamageType.Ranged,
                attacker = shooter,
                knockbackDirection = direction,
                knockbackForce = 2f,
                isCritical = Random.value < 0.15f // 15%暴击率
            };

            damageable.TakeDamage(damageInfo);
            Debug.Log($"[箭矢] 对 {hitCollider.name} 造成 {damage:F1} 伤害");
        }
        else
        {
            Debug.Log($"[箭矢] {hitCollider.name} 没有 IDamageable 组件");
        }

        // 粘附效果
        if (stickToTarget)
        {
            StickToSurface(hitCollider, hitPoint, hitNormal);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void StickToSurface(Collider hitCollider, Vector3 hitPoint, Vector3 hitNormal)
    {
        // 停止物理模拟
        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // 禁用碰撞体避免继续触发
        if (arrowCollider != null)
        {
            arrowCollider.enabled = false;
        }

        // 粘附到目标
        transform.position = hitPoint;
        transform.rotation = Quaternion.LookRotation(hitNormal); // 箭头朝向表面内部

        // 如果命中的是可移动物体，成为其子对象
        // Rigidbody hitRb = hitCollider.attachedRigidbody;
        // if (hitRb != null)
        // {
            transform.SetParent(hitCollider.transform);
        // }

        // 禁用拖尾
        if (trail != null)
        {
            trail.enabled = false;
        }

        Debug.Log($"[箭矢] 已粘附到 {hitCollider.name}");

        // 几秒后消失
        Destroy(gameObject, 30f);
    }
}
