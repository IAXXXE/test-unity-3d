using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform;
    private CharacterController cc;

    [Header("Movement")]
    public float walkSpeed = 4f;
    public float runSpeed = 7f;
    public float crouchSpeed = 2f;
    public float acceleration = 20f;
    public float rotationSmoothTime = 0.08f;

    [Header("Jump & Gravity")]
    public float gravity = -24f;
    public float jumpHeight = 1.6f;
    public float groundedCheckDistance = 0.05f;
    public LayerMask groundMask = ~0;

    [Header("Crouch")]
    public float standingHeight = 1.8f;
    public float crouchHeight = 1.0f;
    public float heightAdjustSpeed = 6f;

    [Header("Misc")]
    public bool lockCursor = true;

    // runtime
    private float verticalVelocity = 0f;
    private float currentSpeed = 0f;
    private float speedVelocityRef;
    private float turnSmoothVelocity;
    private bool isCrouched = false;
    private bool isRunning = false;

    // input
    private PlayerInputActions inputActions;
    private Vector2 moveInput;
    private Vector2 lookInput;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        inputActions = new PlayerInputActions();
    }

    void OnEnable() => inputActions.Player.Enable();
    void OnDisable() => inputActions.Player.Disable();

    void Start()
    {
        if (lockCursor) Cursor.lockState = CursorLockMode.Locked;
        if (!cameraTransform && Camera.main) cameraTransform = Camera.main.transform;

        // hook input callbacks
        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        inputActions.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        // inputActions.Player.Crouch.performed += ctx => isCrouched = !isCrouched;
        inputActions.Player.Jump.performed += ctx => TryJump();

        cc.height = standingHeight;
        UpdateControllerCenter();
    }

    void Update()
    {
        HandleMovement();
        HandleHeightAdjust();
        HandleCameraRotation();
    }

    void HandleCameraRotation()
    {
        // 手柄或鼠标视角控制摄像机
        float lookX = lookInput.x * Time.deltaTime * 120f; // 调整灵敏度
        float lookY = -lookInput.y * Time.deltaTime * 80f;

        cameraTransform.Rotate(Vector3.up, lookX, Space.World);
        cameraTransform.Rotate(Vector3.right, lookY, Space.Self);

        // 限制俯仰角
        Vector3 camAngles = cameraTransform.localEulerAngles;
        if (camAngles.x > 180f) camAngles.x -= 360f;
        camAngles.x = Mathf.Clamp(camAngles.x, -50f, 80f);
        cameraTransform.localEulerAngles = new Vector3(camAngles.x, 0, 0);
    }

    void HandleMovement()
    {
        Vector2 inputDir = moveInput.normalized;
        bool run = Gamepad.current?.leftStickButton.isPressed == true || Keyboard.current?.leftShiftKey.isPressed == true;
        float targetSpeed = isCrouched ? crouchSpeed : (run ? runSpeed : walkSpeed);

        Vector3 forward = new Vector3(cameraTransform.forward.x, 0, cameraTransform.forward.z).normalized;
        Vector3 right = new Vector3(cameraTransform.right.x, 0, cameraTransform.right.z).normalized;

        Vector3 moveDir = (forward * inputDir.y + right * inputDir.x).normalized;
        float targetMagnitude = inputDir.magnitude;
        float desiredSpeed = targetSpeed * targetMagnitude;

        currentSpeed = Mathf.SmoothDamp(currentSpeed, desiredSpeed, ref speedVelocityRef, 1f / (acceleration + 0.0001f));
        Vector3 horizontalVelocity = moveDir * currentSpeed;

        if (moveDir.sqrMagnitude > 0.001f)
        {
            // 玩家朝向摄像机方向
            float targetAngle = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg;
            float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
        }

        ApplyMovement(horizontalVelocity);
        ApplyGravity();
    }

    void ApplyMovement(Vector3 horizontalVelocity)
    {
        cc.Move(horizontalVelocity * Time.deltaTime);
    }

    void ApplyGravity()
    {
        if (IsGrounded())
        {
            if (verticalVelocity < 0f) verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
        cc.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }

    void TryJump()
    {
        if (IsGrounded() && !isCrouched)
        {
            verticalVelocity = Mathf.Sqrt(2f * jumpHeight * -gravity);
        }
    }

    bool IsGrounded()
    {
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        float radius = cc.radius * 0.9f;
        return Physics.CheckSphere(origin, radius, groundMask);
    }

    void HandleHeightAdjust()
    {
        float targetHeight = isCrouched ? crouchHeight : standingHeight;
        cc.height = Mathf.Lerp(cc.height, targetHeight, Time.deltaTime * heightAdjustSpeed);
        UpdateControllerCenter();
    }

    void UpdateControllerCenter()
    {
        Vector3 c = cc.center;
        c.y = cc.height / 2f;
        cc.center = c;
    }

    public bool IsCrouched() => isCrouched;
}



// using UnityEngine;

// [RequireComponent(typeof(CharacterController))]
// public class PlayerController : MonoBehaviour
// {
//     [Header("References")]
//     public Transform cameraTransform;
//     CharacterController cc;

//     [Header("Movement")]
//     public float walkSpeed = 4f;
//     public float runSpeed = 7f;
//     public float crouchSpeed = 2f;
//     public float acceleration = 20f;
//     public float rotationSmoothTime = 0.08f;

//     [Header("Jump & Gravity")]
//     public float gravity = -24f;
//     public float jumpHeight = 1.6f;
//     public float groundedCheckDistance = 0.05f;
//     public LayerMask groundMask = ~0;

//     [Header("Crouch")]
//     public float standingHeight = 1.8f;
//     public float crouchHeight = 1.0f;
//     public float heightAdjustSpeed = 6f;

//     [Header("Misc")]
//     public bool lockCursor = true;

//     // runtime
//     float verticalVelocity = 0f;
//     float currentSpeed = 0f;
//     float speedVelocityRef;
//     float turnSmoothVelocity;
//     bool isCrouched = false;

//     void Start()
//     {
//         cc = GetComponent<CharacterController>();
//         if (lockCursor) Cursor.lockState = CursorLockMode.Locked;
//         if (cameraTransform == null && Camera.main) cameraTransform = Camera.main.transform;
//         // ensure controller height set
//         cc.height = standingHeight;
//         Vector3 center = cc.center;
//         center.y = cc.height / 2f;
//         cc.center = center;
//     }

//     void Update()
//     {
//         HandleMovement();
//         HandleHeightAdjust();
//     }

//     void HandleMovement()
//     {
//         // input
//         float inputX = Input.GetAxisRaw("Horizontal");
//         float inputZ = Input.GetAxisRaw("Vertical");
//         Vector2 input = new Vector2(inputX, inputZ);
//         Vector2 inputDir = input.normalized;

//         bool run = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
//         bool crouchToggle = Input.GetKeyDown(KeyCode.C);
//         if (crouchToggle) isCrouched = !isCrouched;

//         float targetSpeed;
//         if (isCrouched) targetSpeed = crouchSpeed;
//         else targetSpeed = run ? runSpeed : walkSpeed;

//         float targetMagnitude = inputDir.magnitude;
//         float desiredSpeed = targetSpeed * targetMagnitude;
//         currentSpeed = Mathf.SmoothDamp(currentSpeed, desiredSpeed, ref speedVelocityRef, 1f / (acceleration + 0.0001f));

//         // rotation & move dir based on camera forward
//         if (inputDir.sqrMagnitude > 0.0001f)
//         {
//             float targetAngle = Mathf.Atan2(inputDir.x, inputDir.y) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
//             float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, rotationSmoothTime);
//             transform.rotation = Quaternion.Euler(0f, angle, 0f);

//             Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
//             Vector3 horizontalVelocity = moveDir.normalized * currentSpeed;
//             // vertical handled separately
//             ApplyMovement(horizontalVelocity);
//         }
//         else
//         {
//             ApplyMovement(Vector3.zero);
//         }

//         // Jump & gravity
//         if (IsGrounded())
//         {
//             if (verticalVelocity < 0f) verticalVelocity = -2f; // small stick to ground

//             if (Input.GetButtonDown("Jump") && !isCrouched)
//             {
//                 verticalVelocity = Mathf.Sqrt(2f * jumpHeight * -gravity);
//             }
//         }
//         else
//         {
//             verticalVelocity += gravity * Time.deltaTime;
//             cc.Move(Vector3.up * verticalVelocity * Time.deltaTime);
//         }
//     }

//     void ApplyMovement(Vector3 horizontalVelocity)
//     {
//         Vector3 vel = horizontalVelocity;
//         // ensure vertical component preserved by cc.Move earlier, so combine
//         vel += Vector3.up * 0f; // vertical handled separately in Update
//         cc.Move(vel * Time.deltaTime);
//     }

//     bool IsGrounded()
//     {
//         // CharacterController has isGrounded but can be jittery; also do a sphere/capsule check
//         Vector3 origin = transform.position + Vector3.up * 0.1f;
//         float radius = cc.radius * 0.9f;
//         return Physics.CheckSphere(origin, radius, groundMask);
//     }

//     void HandleHeightAdjust()
//     {
//         float targetHeight = isCrouched ? crouchHeight : standingHeight;
//         if (Mathf.Approximately(cc.height, targetHeight)) return;
//         cc.height = Mathf.Lerp(cc.height, targetHeight, Time.deltaTime * heightAdjustSpeed);

//         Vector3 center = cc.center;
//         center.y = cc.height / 2f;
//         cc.center = center;
//     }

//     // Optional: expose toggles for external CameraController to query
//     public bool IsCrouched() => isCrouched;
// }
