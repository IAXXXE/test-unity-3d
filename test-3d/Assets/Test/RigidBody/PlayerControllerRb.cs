using UnityEngine;
using UnityEngine.InputSystem;

public enum PlayerMoveState
{
    Idle,
    Walk,
    Run,
    Swim,
    Climb,
    Fly
}

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform;
    public Transform modelTransform; // 角色模型，用于旋转
    private Rigidbody rb;
    private CapsuleCollider capsule;
    private Animator animator;

    [Header("Movement")]
    public float walkSpeed = 4f;
    public float runSpeed = 7f;
    public float crouchSpeed = 2f;
    public float swimSpeed = 3f;
    public float swimAccSpeed = 5f;
    public float moveAcceleration = 20f;
    public float rotationSpeed = 10f;
    public float airControlMultiplier = 0.3f;

    [Header("Jump & Gravity")]
    public float jumpForce = 8f;
    public float gravityMultiplier = 2f;
    public float maxFallSpeed = 20f;
    public float groundCheckDistance = 0.2f;
    public LayerMask groundMask = ~0;

    [Header("Crouch")]
    public float standingHeight = 1.8f;
    public float crouchHeight = 1.0f;
    public float crouchTransitionSpeed = 10f;

    [Header("Water & Buoyancy")]
    public float waterLevel = 0f;
    public float buoyancyForce = 15f;
    public float waterDrag = 3f;
    public float waterAngularDrag = 1f;
    public float swimUpForce = 3f;
    public float swimDownForce = 3f;
    public float splashDownForce = 8f;
    public float splashVelocityThreshold = 3f;
    public float splashDuration = 0.4f;
    public float fullSubmergeDepth = 1.5f;
    
    [Header("Physics")]
    public float groundDrag = 6f;
    public float airDrag = 0.1f;
    public float slopeLimit = 45f;
    
    [Header("Misc")]
    public bool lockCursor = true;
    public bool showDebug = false;

    // State
    private PlayerMoveState moveState = PlayerMoveState.Idle;
    private bool isGrounded = false;
    private bool isCrouched = false;
    public bool isInWater = false;
    private bool isFullySubmerged = false;
    
    // Water physics
    private float waterDepth = 0f;
    private float splashTimer = 0f;
    private float entryVelocity = 0f;
    private float defaultDrag = 0f;
    private float defaultAngularDrag = 0f;

    // Animation
    private float animSpeedParam = 0f;
    private float animSpeedVelocity = 0f;
    private float animSpeedSmoothTime = 0.1f;

    // Input
    private PlayerInputActions inputActions;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool jumpPressed = false;
    private bool sprintPressed = false;
    private bool swimUpPressed = false;
    private bool swimDownPressed = false;

    // Runtime
    private Vector3 moveDirection;
    private float currentSpeed = 0f;
    public bool isLocked = false;

    // Properties
    public bool IsGrounded => isGrounded;
    // public bool IsInWater => isInWater;
    public bool IsFullySubmerged => isFullySubmerged;
    public PlayerMoveState MoveState => moveState;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        animator = GetComponentInChildren<Animator>();
        inputActions = new PlayerInputActions();

        // Rigidbody setup
        rb.freezeRotation = true; // 防止物理旋转
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        
        defaultDrag = rb.drag;
        defaultAngularDrag = rb.angularDrag;

        // 如果没有设置模型Transform，就用自己
        if (modelTransform == null)
            modelTransform = transform;

        GameEventManager.OnUIShowed += OnUIShowed;
        GameEventManager.OnUIHided += OnUIHided;
    }

    void OnDestroy()
    {
        GameEventManager.OnUIShowed -= OnUIShowed;
        GameEventManager.OnUIHided -= OnUIHided;
    }

    private void OnUIShowed()
    {
        isLocked = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void OnUIHided()
    {
        isLocked = false;
        if (lockCursor)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    void OnEnable() => inputActions.Player.Enable();
    void OnDisable() => inputActions.Player.Disable();

    void Start()
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        if (!cameraTransform && Camera.main)
            cameraTransform = Camera.main.transform;

        // Input binding
        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        inputActions.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        inputActions.Player.Jump.performed += ctx => jumpPressed = true;
        inputActions.Player.Jump.canceled += ctx => jumpPressed = false;

        capsule.height = standingHeight;
        UpdateCapsuleCenter();
    }

    void Update()
    {
        if (isLocked) return;

        // Gather input
        sprintPressed = Keyboard.current?.leftShiftKey.isPressed == true;
        swimUpPressed = Keyboard.current?.spaceKey.isPressed == true;
        swimDownPressed = Keyboard.current?.leftCtrlKey.isPressed == true;

        HandleCameraRotation();
        UpdateGroundCheck();
        // UpdateWaterStatus();
        UpdateMoveState();
        UpdateAnimator();
        HandleHeightAdjust();

        // Update splash timer
        if (splashTimer > 0)
            splashTimer -= Time.deltaTime;
    }

    void FixedUpdate()
    {
        if (isLocked) return;

        ApplyMovement();
        ApplyRotation();

        if (isInWater)
        {
            ApplyBuoyancy();
        }
        else
        {
            ApplyGravity();
        }

        // Update drag based on state
        UpdatePhysicsDrag();
    }

    void HandleCameraRotation()
    {
        if (cameraTransform == null) return;

        float lookX = lookInput.x * Time.deltaTime * 120f;
        float lookY = -lookInput.y * Time.deltaTime * 80f;

        cameraTransform.Rotate(Vector3.up, lookX, Space.World);
        cameraTransform.Rotate(Vector3.right, lookY, Space.Self);

        Vector3 camAngles = cameraTransform.localEulerAngles;
        if (camAngles.x > 180f) camAngles.x -= 360f;
        camAngles.x = Mathf.Clamp(camAngles.x, -50f, 80f);
        cameraTransform.localEulerAngles = new Vector3(camAngles.x, 0, 0);
    }

    void UpdateGroundCheck()
    {
        Vector3 origin = transform.position;
        float checkRadius = capsule.radius * 0.9f;
        float checkDistance = (capsule.height * 0.5f) + groundCheckDistance;
        
        isGrounded = Physics.SphereCast(origin, checkRadius, Vector3.down, 
            out RaycastHit hit, checkDistance, groundMask);
    }

    void UpdateWaterStatus()
    {
        bool wasInWater = isInWater;
        
        // Calculate water depth
        waterDepth = waterLevel - transform.position.y;
        
        // Check if feet are in water
        float feetY = transform.position.y - (capsule.height * 0.5f);
        isInWater = feetY < waterLevel;
        
        // Check if head is underwater
        float headY = transform.position.y + (capsule.height * 0.5f);
        isFullySubmerged = headY < waterLevel;
        
        // Water entry event
        if (isInWater && !wasInWater)
        {
            OnEnterWater();
        }
        else if (!isInWater && wasInWater)
        {
            OnExitWater();
        }
    }

    public void OnEnterWater()
    {
        entryVelocity = Mathf.Max(0, -rb.velocity.y);
        
        if (entryVelocity > splashVelocityThreshold)
        {
            splashTimer = splashDuration;
            // Add downward force on impact
            rb.AddForce(Vector3.down * splashDownForce * (entryVelocity / splashVelocityThreshold), 
                ForceMode.Impulse);
            
            Debug.Log($"Splash! Entry velocity: {entryVelocity}");
        }
    }

    public void OnExitWater()
    {
        splashTimer = 0f;
    }

    void UpdateMoveState()
    {
        if (isInWater)
        {
            moveState = PlayerMoveState.Swim;
        }
        else if (isGrounded)
        {
            if (moveInput.sqrMagnitude < 0.01f)
                moveState = PlayerMoveState.Idle;
            else if (sprintPressed && !isCrouched)
                moveState = PlayerMoveState.Run;
            else
                moveState = PlayerMoveState.Walk;
        }
        else
        {
            // In air, keep previous state
        }
    }

    void ApplyMovement()
    {
        Vector3 camForward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
        Vector3 camRight = cameraTransform.right;
        moveDirection = (camForward * moveInput.y + camRight * moveInput.x).normalized;

        float targetSpeed = GetTargetSpeed();
        
        // Calculate desired velocity
        Vector3 targetVelocity = moveDirection * targetSpeed;
        Vector3 currentVelocityXZ = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        
        // Calculate force needed
        Vector3 velocityDiff = targetVelocity - currentVelocityXZ;
        float acceleration = isGrounded ? moveAcceleration : moveAcceleration * airControlMultiplier;
        Vector3 force = velocityDiff * acceleration;
        
        // Apply force
        rb.AddForce(force, ForceMode.Acceleration);
        
        // Handle jumping
        if (jumpPressed)
        {
            TryJump();
            jumpPressed = false; // Consume jump input
        }
        
        // Swimming vertical movement
        if (isInWater)
        {
            if (swimUpPressed)
            {
                rb.AddForce(Vector3.up * swimUpForce, ForceMode.Acceleration);
            }
            else if (swimDownPressed)
            {
                rb.AddForce(Vector3.down * swimDownForce, ForceMode.Acceleration);
            }
        }
        
        // Calculate current speed for animation
        currentSpeed = currentVelocityXZ.magnitude;
    }

    void ApplyRotation()
    {
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            modelTransform.rotation = Quaternion.Slerp(modelTransform.rotation, targetRotation, 
                rotationSpeed * Time.fixedDeltaTime);
        }
    }

    void ApplyGravity()
    {
        if (!isGrounded)
        {
            // Apply extra gravity for better feel
            rb.AddForce(Vector3.down * Physics.gravity.magnitude * (gravityMultiplier - 1f), 
                ForceMode.Acceleration);
            
            // Limit fall speed
            if (rb.velocity.y < -maxFallSpeed)
            {
                rb.velocity = new Vector3(rb.velocity.x, -maxFallSpeed, rb.velocity.z);
            }
        }
    }

    void ApplyBuoyancy()
    {
        Debug.Log("Player ApplyBuoyancy");
        // Calculate submersion ratio
        float submersionRatio = Mathf.Clamp01((waterLevel - (transform.position.y - capsule.height * 0.5f)) / capsule.height);
        
        // Splash dampening period
        if (splashTimer > 0)
        {
            float splashRatio = splashTimer / splashDuration;
            float dampening = Mathf.Lerp(1f, 0.3f, splashRatio);
            submersionRatio *= dampening;
        }
        
        // Apply buoyancy force
        float buoyancy = buoyancyForce * submersionRatio;
        rb.AddForce(Vector3.up * buoyancy, ForceMode.Acceleration);
        
        // Add slight downward force when not actively swimming (natural sinking)
        // if (!swimUpPressed && rb.velocity.y > 0)
        // {
        //     rb.AddForce(Vector3.down * 2f, ForceMode.Acceleration);
        // }
    }

    void TryJump()
    {
        if (isInWater)
        {
            // Swim up impulse
            rb.velocity = new Vector3(rb.velocity.x, swimUpForce, rb.velocity.z);
        }
        else if (isGrounded && !isCrouched)
        {
            // Ground jump
            rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
            
            if (animator)
                animator.SetTrigger("Jump");
        }
    }

    float GetTargetSpeed()
    {
        if (isCrouched)
            return crouchSpeed;
        
        if (isInWater)
            return sprintPressed ? swimAccSpeed : swimSpeed;
        
        if (sprintPressed)
            return runSpeed;
        
        return walkSpeed;
    }

    void UpdatePhysicsDrag()
    {
        if (isInWater)
        {
            rb.drag = waterDrag;
            rb.angularDrag = waterAngularDrag;
        }
        else if (isGrounded)
        {
            rb.drag = groundDrag;
            rb.angularDrag = defaultAngularDrag;
        }
        else
        {
            rb.drag = airDrag;
            rb.angularDrag = defaultAngularDrag;
        }
    }

    void HandleHeightAdjust()
    {
        float targetHeight = isCrouched ? crouchHeight : standingHeight;
        capsule.height = Mathf.Lerp(capsule.height, targetHeight, 
            Time.deltaTime * crouchTransitionSpeed);
        UpdateCapsuleCenter();
    }

    void UpdateCapsuleCenter()
    {
        capsule.center = new Vector3(0, capsule.height * 0.5f, 0);
    }

    void UpdateAnimator()
    {
        if (!animator) return;

        // Calculate normalized speed
        float maxSpeed = sprintPressed ? runSpeed : walkSpeed;
        float normalizedSpeed = Mathf.Clamp01(currentSpeed / maxSpeed);
        animSpeedParam = Mathf.SmoothDamp(animSpeedParam, normalizedSpeed, 
            ref animSpeedVelocity, animSpeedSmoothTime);

        // Basic parameters
        animator.SetFloat("Speed", animSpeedParam);
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsCrouched", isCrouched);
        
        // Swimming animation parameters
        animator.SetBool("IsSwimming", isInWater);
        animator.SetBool("IsUnderwater", isFullySubmerged);
        animator.SetFloat("WaterDepth", waterDepth);
        
        // Vertical velocity for swim animations
        if (isInWater)
        {
            float normalizedVertical = Mathf.Clamp(rb.velocity.y / 5f, -1f, 1f);
            animator.SetFloat("SwimVertical", normalizedVertical);
        }
        else
        {
            animator.SetFloat("SwimVertical", 0f);
        }
        
        // Vertical velocity for jump/fall
        animator.SetFloat("VerticalVelocity", rb.velocity.y);
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying || !showDebug) return;

        // Draw water surface
        Gizmos.color = new Color(0, 0.5f, 1f, 0.3f);
        Vector3 waterPos = new Vector3(transform.position.x, waterLevel, transform.position.z);
        Gizmos.DrawWireCube(waterPos, new Vector3(10f, 0.1f, 10f));

        // Draw ground check
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Vector3 origin = transform.position;
        float checkRadius = capsule.radius * 0.9f;
        float checkDistance = (capsule.height * 0.5f) + groundCheckDistance;
        Gizmos.DrawWireSphere(origin + Vector3.down * checkDistance, checkRadius);

        // Draw character state
        Gizmos.color = isInWater ? Color.cyan : (isGrounded ? Color.green : Color.yellow);
        Gizmos.DrawWireSphere(transform.position, 0.5f);

        // Draw velocity
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, rb.velocity * 0.5f);
    }

    void OnGUI()
    {
        if (!showDebug) return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 250));
        GUILayout.Box("=== Player Debug Info ===");
        GUILayout.Label($"Move State: {moveState}");
        GUILayout.Label($"Grounded: {isGrounded}");
        GUILayout.Label($"In Water: {isInWater}");
        GUILayout.Label($"Fully Submerged: {isFullySubmerged}");
        GUILayout.Label($"Water Depth: {waterDepth:F2}");
        GUILayout.Label($"Speed: {currentSpeed:F2}");
        GUILayout.Label($"Velocity: {rb.velocity}");
        GUILayout.Label($"Splash Timer: {splashTimer:F2}");
        
        if (isInWater)
        {
            GUILayout.Label("--- Swimming Controls ---");
            GUILayout.Label("Space: Swim Up");
            GUILayout.Label("Ctrl: Dive Down");
        }
        GUILayout.EndArea();
    }

    // Public methods
    public bool IsCrouched() => isCrouched;
    public PlayerInputActions GetInputActions() => inputActions;
    public void SetCrouch(bool crouch) => isCrouched = crouch;
}

