using UnityEngine;

public class WeaponBowBehavior : ItemBehavior
{
    [Header("Bow Settings")]
    public float maxDrawTime = 2.0f;
    public float minDrawForShot = 0.3f;
    public float maxArrowSpeed = 50f;
    public float minArrowSpeed = 15f;
    
    [Header("Arrow Settings")]
    public GameObject arrowPrefab;
    public Transform arrowSpawnPoint;
    public float arrowDamage = 30f;
    public float arrowLifetime = 5f;
    
    // [Header("Aim Settings")]
    public Camera aimCamera;
    // public float aimFOV = 40f;
    // public float normalFOV = 60f;
    // public float aimSensitivity = 0.5f;

    private bool isAiming;
    private bool isDrawing;
    private float drawTimer;
    private GameObject drawnArrowVisual;
    // private float originalFOV;

    public override void Initialize(ItemBase item, PlayerWeapon weapon, PlayerUI ui)
    {
        base.Initialize(item, weapon, ui);
        
        if (item.data.damage > 0)
            arrowDamage = item.data.damage;
        
        if (aimCamera == null)
            aimCamera = Camera.main;

        arrowSpawnPoint = transform.Find("_ArrowSpawnPoint");

        // originalFOV = aimCamera.fieldOfView;
    }

    public override void OnSecondaryStart()
    {
        EnterAimMode();
    }

    public override void OnSecondaryUpdate(float deltaTime)
    {
        if (isAiming && isDrawing)
        {
            drawTimer += deltaTime;
            float drawRatio = Mathf.Clamp01(drawTimer / maxDrawTime);
            playerUI?.UpdateChargeBar(drawRatio);
            
            // 更新弓拉弦动画/音效
            UpdateBowDrawVisual(drawRatio);
        }
    }

    public override void OnSecondaryEnd()
    {
        if (isDrawing && drawTimer >= minDrawForShot)
        {
            ShootArrow();
        }
        
        ExitAimMode();
    }

    public override void OnPrimaryStart()
    {
        if (isAiming && !isDrawing)
        {
            StartDrawing();
        }
    }

    public override void OnPrimaryEnd()
    {
        if (isDrawing && drawTimer >= minDrawForShot)
        {
            ShootArrow();
        }
    }

    public override void OnPrimaryUpdate(float deltaTime) { }
    public override void OnUse() { }

    private void EnterAimMode()
    {
        isAiming = true;
        Debug.Log("[弓箭] 进入瞄准模式");
        
        // 调整FOV
        // StartCoroutine(SmoothFOVTransition(normalFOV, aimFOV, 0.2f));

        // 显示准星
        playerUI.SetCrosshair(true);
        GameEventManager.TriggerAimModeChanged(true);
        
        // 自动开始拉弦
        StartDrawing();
    }

    private void ExitAimMode()
    {
        isAiming = false;
        isDrawing = false;
        drawTimer = 0f;
        
        if (drawnArrowVisual != null)
        {
            Destroy(drawnArrowVisual);
            drawnArrowVisual = null;
        }

        // 恢复FOV
        // StartCoroutine(SmoothFOVTransition(aimFOV, normalFOV, 0.2f));
        
        playerUI?.ShowChargeBar(false);
        playerUI?.SetCrosshair(false);
        
        Debug.Log("[弓箭] 退出瞄准模式");
        GameEventManager.TriggerAimModeChanged(false);
    }

    private void StartDrawing()
    {
        isDrawing = true;
        drawTimer = 0f;

        // 生成箭矢视觉效果
        arrowSpawnPoint = transform;
        //TODO:
        arrowPrefab = InventoryManager.Instance.GetItemData("W0003")?.worldPrefab;

        if (arrowPrefab != null && arrowSpawnPoint != null)
        {
            drawnArrowVisual = Instantiate(arrowPrefab, arrowSpawnPoint);
            drawnArrowVisual.transform.localPosition = Vector3.zero;
            drawnArrowVisual.transform.localRotation = Quaternion.identity;
            
            // 禁用物理碰撞，仅作视觉展示
            var rb = drawnArrowVisual.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;
            var col = drawnArrowVisual.GetComponent<MeshCollider>();
            if(col != null) col.enabled = false;
        }

        playerUI?.ShowChargeBar(true);
        Debug.Log("[弓箭] 开始拉弦");
    }

    private void UpdateBowDrawVisual(float drawRatio)
    {
        // 更新弓弦拉力的视觉效果
        // 可以通过Animator参数或直接修改弓的mesh
        
        // 示例：通过Animator
        var animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetFloat("DrawAmount", drawRatio);
        }
    }

    private void ShootArrow()
    {
        float drawRatio = Mathf.Clamp01(drawTimer / maxDrawTime);
        float arrowSpeed = Mathf.Lerp(minArrowSpeed, maxArrowSpeed, drawRatio);
        float damage = arrowDamage * (0.5f + 0.5f * drawRatio); // 伤害随拉力变化

        Debug.Log($"[弓箭] 射出箭矢, 拉力={drawRatio:F2}, 速度={arrowSpeed:F1}, 伤害={damage:F1}");

        // 实例化真实的箭矢物理对象
        if (arrowPrefab != null && arrowSpawnPoint != null)
        {
            // 计算射击方向（从相机中心）
            Vector3 shootDirection = GetShootDirection();
            
            GameObject arrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, Quaternion.LookRotation(shootDirection));
            arrow.transform.rotation = Quaternion.Euler(new Vector3(0, -90f, 0));

            // 配置箭矢组件
            Arrow arrowComponent = arrow.GetComponent<Arrow>();
            if (arrowComponent == null)
            {
                arrowComponent = arrow.AddComponent<Arrow>();
            }
            
            arrowComponent.Initialize(damage, arrowSpeed, shootDirection, playerWeapon.gameObject, arrowLifetime);
        }

        GameEventManager.TriggerWeaponAttack(itemData, AttackType.Ranged, drawRatio);

        // 清理视觉箭矢
        if (drawnArrowVisual != null)
        {
            Destroy(drawnArrowVisual);
            drawnArrowVisual = null;
        }

        isDrawing = false;
        drawTimer = 0f;
        playerUI?.ShowChargeBar(false);

        InventoryManager.Instance.RemoveItem("W0003", 1);

        // 播放射击音效
        // AudioManager.Instance?.PlaySound("bow_shoot");
    }

    private Vector3 GetShootDirection()
    {
        // 从屏幕中心发射射线
        Ray ray = aimCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        
        // 如果射线击中物体，朝向击中点
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
        {
            return (hit.point - arrowSpawnPoint.position).normalized;
        }
        
        // 否则朝向射线方向
        return ray.direction;
    }

    private System.Collections.IEnumerator SmoothFOVTransition(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            aimCamera.fieldOfView = Mathf.Lerp(from, to, t);
            yield return null;
        }
        aimCamera.fieldOfView = to;
    }

    public override void OnUnequipped()
    {
        base.OnUnequipped();
        
        // 确保退出瞄准状态
        if (isAiming)
        {
            ExitAimMode();
        }
        
        // 恢复FOV
        // if (aimCamera != null)
        // {
        //     aimCamera.fieldOfView = originalFOV;
        // }
    }
}
