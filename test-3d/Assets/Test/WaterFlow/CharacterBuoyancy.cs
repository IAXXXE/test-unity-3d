using TMPro;
using UnityEngine;

/// <summary>
/// CharacterController 专用浮力系统
/// 适用于第一人称/第三人称角色控制器
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class CharacterBuoyancy : MonoBehaviour
{
    [Header("水体设置")]
    [Tooltip("水面高度")]
    public float waterLevel = 0f;
    
    [Header("浮力参数")]
    [Tooltip("浮力加速度")]
    public float buoyancyAcceleration = 20f;
    
    [Tooltip("最大上浮速度")]
    public float maxBuoyancySpeed = 3f;
    
    [Tooltip("水中下沉速度（负值表示下沉）")]
    public float sinkSpeed = -1f;
    
    [Header("水中运动")]
    [Tooltip("水中移动速度倍数")]
    public float waterMovementMultiplier = 0.5f;
    
    [Tooltip("水中跳跃力度")]
    public float waterJumpForce = 3f;
    
    [Tooltip("水的阻力（0-1）")]
    public float waterDrag = 0.9f;
    
    [Header("入水缓冲")]
    [Tooltip("入水时保持向下的时间")]
    public float splashDuration = 0.3f;
    
    [Tooltip("入水速度阈值")]
    public float splashThreshold = 5f;
    
    [Header("游泳检测")]
    [Tooltip("完全浸入水中的深度")]
    public float fullSubmergeDepth = 1.5f;
    
    [Tooltip("脚部位置偏移（用于检测站在水底）")]
    public float footOffset = 1f;
    
    private CharacterController controller;
    private Vector3 velocity;
    private bool isInWater = false;
    private bool isFullySubmerged = false;
    private float splashTimer = 0f;
    private float entryVelocity = 0f;
    
    // 公开属性供外部访问
    public bool IsInWater => isInWater;
    public bool IsFullySubmerged => isFullySubmerged;
    public float WaterDepth { get; private set; }
    public Vector3 Velocity => velocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        CheckWaterStatus();
        ApplyWaterPhysics();
        
        if (splashTimer > 0)
        {
            splashTimer -= Time.deltaTime;
        }

        UpdateInfoText();
    }

    private void UpdateInfoText()
    {
        var text = transform.GetComponentInChildren<TextMeshProUGUI>();
        if(text != null) return;
        text.text = $"In Water : {isInWater}/n";
    }

    void CheckWaterStatus()
    {
        // 计算角色中心点的深度
        float centerDepth = waterLevel - transform.position.y;
        WaterDepth = centerDepth;
        
        bool wasInWater = isInWater;
        
        // 检查是否在水中（脚部低于水面）
        isInWater = (transform.position.y - footOffset) < waterLevel;
        
        // 检查是否完全浸入
        isFullySubmerged = centerDepth > fullSubmergeDepth;
        
        // 入水事件
        if (isInWater && !wasInWater)
        {
            OnEnterWater();
        }
        else if (!isInWater && wasInWater)
        {
            OnExitWater();
        }
    }

    void ApplyWaterPhysics()
    {
        if (!isInWater) return;
        
        float depth = WaterDepth;
        
        // 入水缓冲期间
        if (splashTimer > 0)
        {
            // 保持向下的动量，减弱浮力
            float splashProgress = splashTimer / splashDuration;
            velocity.y -= entryVelocity * splashProgress * Time.deltaTime;
            return;
        }
        
        // 根据深度计算浮力
        if (isFullySubmerged)
        {
            // 完全浸入时的浮力
            float buoyancyForce = buoyancyAcceleration * Time.deltaTime;
            velocity.y += buoyancyForce;
            
            // 限制上浮速度
            velocity.y = Mathf.Clamp(velocity.y, sinkSpeed, maxBuoyancySpeed);
        }
        else
        {
            // 部分浸入时，根据深度插值
            float submersionRatio = Mathf.Clamp01(depth / fullSubmergeDepth);
            float buoyancyForce = buoyancyAcceleration * submersionRatio * Time.deltaTime;
            velocity.y += buoyancyForce;
        }
        
        // 应用水的阻力
        velocity *= Mathf.Pow(waterDrag, Time.deltaTime * 10f);
    }

    /// <summary>
    /// 外部调用此方法来移动角色（替代CharacterController.Move）
    /// </summary>
    public void MoveCharacter(Vector3 moveDirection, float speed)
    {
        if (isInWater)
        {
            // 水中移动速度降低
            speed *= waterMovementMultiplier;
            
            // 允许三维移动（游泳）
            if (isFullySubmerged)
            {
                velocity.x = moveDirection.x * speed;
                velocity.z = moveDirection.z * speed;
            }
            else
            {
                // 浅水中仍然可以正常行走
                velocity.x = moveDirection.x * speed;
                velocity.z = moveDirection.z * speed;
            }
        }
        else
        {
            // 陆地上的正常移动
            velocity.x = moveDirection.x * speed;
            velocity.z = moveDirection.z * speed;
        }
        
        // 应用移动
        controller.Move(velocity * Time.deltaTime);
    }

    /// <summary>
    /// 应用重力（在陆地上）
    /// </summary>
    public void ApplyGravity(float gravity)
    {
        if (!isInWater && !controller.isGrounded)
        {
            velocity.y += gravity * Time.deltaTime;
        }
        else if (!isInWater && controller.isGrounded)
        {
            // 在地面上时重置垂直速度
            velocity.y = -2f; // 小的负值保持贴地
        }
    }

    /// <summary>
    /// 跳跃或向上游
    /// </summary>
    public void Jump(float jumpForce)
    {
        if (isInWater)
        {
            // 水中跳跃（向上游）
            velocity.y = waterJumpForce;
        }
        else if (controller.isGrounded)
        {
            // 陆地跳跃
            velocity.y = jumpForce;
        }
    }

    /// <summary>
    /// 主动下潜（游泳时）
    /// </summary>
    public void Dive(float diveForce)
    {
        if (isInWater && isFullySubmerged)
        {
            velocity.y -= diveForce * Time.deltaTime;
        }
    }

    void OnEnterWater()
    {
        Debug.Log("Enter Water");
        entryVelocity = Mathf.Abs(velocity.y);
        
        // 如果入水速度够快，启动缓冲
        if (entryVelocity > splashThreshold)
        {
            splashTimer = splashDuration;
        }
    }

    void OnExitWater()
    {
        splashTimer = 0f;
        // 可以在这里添加出水的音效或粒子效果
    }

    /// <summary>
    /// 重置速度（用于特殊情况）
    /// </summary>
    public void ResetVelocity()
    {
        velocity = Vector3.zero;
    }

    /// <summary>
    /// 设置垂直速度
    /// </summary>
    public void SetVerticalVelocity(float yVelocity)
    {
        velocity.y = yVelocity;
    }

    void OnDrawGizmos()
    {
        // 绘制水面
        Gizmos.color = new Color(0, 0.5f, 1f, 0.3f);
        Gizmos.DrawCube(new Vector3(transform.position.x, waterLevel, transform.position.z), 
                       new Vector3(10f, 0.1f, 10f));
        
        if (!Application.isPlaying) return;
        
        // 显示浸入状态
        Gizmos.color = isInWater ? Color.blue : Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        
        // 显示完全浸入深度
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(new Vector3(transform.position.x, 
                                         waterLevel - fullSubmergeDepth, 
                                         transform.position.z), 0.3f);
    }
}