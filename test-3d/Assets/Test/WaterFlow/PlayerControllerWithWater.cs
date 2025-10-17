using UnityEngine;

/// <summary>
/// 整合了水浮力系统的第一人称/第三人称控制器
/// 配合 CharacterBuoyancy 使用
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(CharacterBuoyancy))]
public class PlayerControllerWithWater : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float gravity = -20f;
    
    [Header("水中控制")]
    [SerializeField] private float swimUpSpeed = 3f;
    [SerializeField] private float swimDownSpeed = 3f;
    
    [Header("相机设置（第一人称）")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 80f;
    
    private CharacterController controller;
    private CharacterBuoyancy buoyancy;
    private float cameraPitch = 0f;

    private Animator m_Animator;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        buoyancy = GetComponent<CharacterBuoyancy>();
        
        // 锁定鼠标
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        m_Animator = GetComponent<Animator>();
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
    }

    void HandleMouseLook()
    {
        if (cameraTransform == null) return;
        
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        // 水平旋转（旋转整个角色）
        transform.Rotate(Vector3.up * mouseX);
        // m_Animator.transform.localRotation = transform.rotation;
        
        // 垂直旋转（只旋转相机）
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -maxLookAngle, maxLookAngle);
        cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
    }

    void HandleMovement()
    {
        // 获取输入
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && !buoyancy.IsInWater;
        bool jumpPressed = Input.GetButtonDown("Jump");
        
        // 计算移动方向
        Vector3 moveDirection = transform.right * horizontal + transform.forward * vertical;
        moveDirection.Normalize();
        
        // 确定移动速度
        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;
        
        // 处理跳跃
        if (jumpPressed)
        {
            buoyancy.Jump(jumpForce);
        }
        
        // 在水中时的垂直控制
        if (buoyancy.IsInWater && buoyancy.IsFullySubmerged)
        {
            // 按空格上浮
            if (Input.GetKey(KeyCode.Space))
            {
                buoyancy.SetVerticalVelocity(swimUpSpeed);
            }
            // 按 Ctrl 或 C 下潜
            else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C))
            {
                buoyancy.Dive(swimDownSpeed);
            }
        }
        
        // 应用移动
        buoyancy.MoveCharacter(moveDirection, currentSpeed);
        
        // 在陆地上应用重力
        if (!buoyancy.IsInWater)
        {
            buoyancy.ApplyGravity(gravity);
        }

        if(currentSpeed != 0)
        {
            m_Animator.SetFloat("Vert", currentSpeed == 0 ? 0 : 1);
            m_Animator.SetFloat("State", currentSpeed);
            
        }
        else
        {
            m_Animator.SetFloat("Vert", 0);
            m_Animator.SetFloat("State", 0);
        }
    }
}