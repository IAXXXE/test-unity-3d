using UnityEngine;

// ========== 3. 弓箭行为 ==========
public class WeaponBowBehavior : ItemBehavior
{
    [Header("Bow Settings")]
    public float maxDrawTime = 2.0f;
    public float minDrawForShot = 0.3f;
    public GameObject arrowPrefab;
    public Transform arrowSpawnPoint;
    public float maxArrowSpeed = 50f;

    private bool isAiming;
    private bool isDrawing;
    private float drawTimer;
    private GameObject drawnArrow;

    public override void OnSecondaryStart()
    {
        // 右键/ZR - 进入瞄准状态
        EnterAimMode();
    }

    public override void OnSecondaryUpdate(float deltaTime)
    {
        if (isAiming && isDrawing)
        {
            drawTimer += deltaTime;
            float drawRatio = Mathf.Clamp01(drawTimer / maxDrawTime);
            playerUI?.UpdateChargeBar(drawRatio);
            
            // TODO: 更新弓的拉弦动画
        }
    }

    public override void OnSecondaryEnd()
    {
        // 松开ZR（手柄模式）- 射箭
        if (isDrawing && drawTimer >= minDrawForShot)
        {
            ShootArrow();
        }
        
        ExitAimMode();
    }

    public override void OnPrimaryStart()
    {
        // 左键（PC模式）或开始拉弦
        if (isAiming && !isDrawing)
        {
            StartDrawing();
        }
    }

    public override void OnPrimaryEnd()
    {
        // 左键释放（PC模式）- 射箭
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
        
        // TODO: 切换到瞄准相机
        // TODO: 显示准星
        GameEventManager.TriggerAimModeChanged(true);
        
        // 自动开始拉弦（或等待主要按键）
        StartDrawing();
    }

    private void ExitAimMode()
    {
        isAiming = false;
        isDrawing = false;
        drawTimer = 0f;
        
        if (drawnArrow != null)
        {
            Destroy(drawnArrow);
            drawnArrow = null;
        }

        playerUI?.ShowChargeBar(false);
        Debug.Log("[弓箭] 退出瞄准模式");
        GameEventManager.TriggerAimModeChanged(false);
    }

    private void StartDrawing()
    {
        isDrawing = true;
        drawTimer = 0f;
        
        // 生成箭矢视觉效果
        if (arrowPrefab != null && arrowSpawnPoint != null)
        {
            drawnArrow = Instantiate(arrowPrefab, arrowSpawnPoint);
        }

        playerUI?.ShowChargeBar(true);
        Debug.Log("[弓箭] 开始拉弦");
    }

    private void ShootArrow()
    {
        float drawRatio = Mathf.Clamp01(drawTimer / maxDrawTime);
        float arrowSpeed = maxArrowSpeed * drawRatio;

        Debug.Log($"[弓箭] 射出箭矢, 拉力={drawRatio:F2}, 速度={arrowSpeed:F1}");

        // TODO: 实例化真实的箭矢物理对象
        // TODO: 应用速度和伤害
        GameEventManager.TriggerWeaponAttack(itemData, AttackType.Ranged, drawRatio);

        if (drawnArrow != null)
        {
            Destroy(drawnArrow);
            drawnArrow = null;
        }

        isDrawing = false;
        drawTimer = 0f;
        playerUI?.ShowChargeBar(false);
    }
}
