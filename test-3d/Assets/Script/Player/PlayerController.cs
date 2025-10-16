using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform;
    private CharacterController cc;
    private Animator animator;

    public Animator tempDogAnimator;

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

    // animation
    private float animSpeedParam = 0f;
    private float animSpeedVelocity = 0f;
    private float animSpeedSmoothTime = 0.1f;

    // input
    private PlayerInputActions inputActions;
    private Vector2 moveInput;
    private Vector2 lookInput;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        inputActions = new PlayerInputActions();
    }

    void OnEnable() => inputActions.Player.Enable();
    void OnDisable() => inputActions.Player.Disable();

    void Start()
    {
        if (lockCursor) Cursor.lockState = CursorLockMode.Locked;
        if (!cameraTransform && Camera.main) cameraTransform = Camera.main.transform;

        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        inputActions.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        inputActions.Player.Jump.performed += ctx => TryJump();

        cc.height = standingHeight;
        UpdateControllerCenter();
    }

    void Update()
    {
        HandleMovement();
        HandleHeightAdjust();
        HandleCameraRotation();
        UpdateAnimator();
    }

    void HandleCameraRotation()
    {
        float lookX = lookInput.x * Time.deltaTime * 120f;
        float lookY = -lookInput.y * Time.deltaTime * 80f;

        cameraTransform.Rotate(Vector3.up, lookX, Space.World);
        cameraTransform.Rotate(Vector3.right, lookY, Space.Self);

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

        Vector3 camForward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
        Vector3 camRight = cameraTransform.right;
        Vector3 moveDir = camForward * moveInput.y + camRight * moveInput.x;

        float targetMagnitude = inputDir.magnitude;
        float desiredSpeed = targetSpeed * targetMagnitude;

        currentSpeed = Mathf.SmoothDamp(currentSpeed, desiredSpeed, ref speedVelocityRef, 1f / (acceleration + 0.0001f));
        Vector3 horizontalVelocity = moveDir * currentSpeed;

        if (moveDir.sqrMagnitude > 0.001f)
        {
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
            if (animator) animator.SetTrigger("Jump"); // 跳跃动画
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

    void UpdateAnimator()
    {
        if (!animator) return;

        float normalizedSpeed = Mathf.Clamp01(currentSpeed / runSpeed);
        animSpeedParam = Mathf.SmoothDamp(animSpeedParam, normalizedSpeed, ref animSpeedVelocity, animSpeedSmoothTime);
        animator.SetFloat("Speed", animSpeedParam);
        animator.SetBool("IsGrounded", IsGrounded());

        if (tempDogAnimator != null)
        {
            if(animSpeedParam <= 0.001)
            {
                tempDogAnimator.SetFloat("Vert", 0);
                tempDogAnimator.SetFloat("State", 0);

            }
            else
            {
                tempDogAnimator.SetFloat("Vert", animSpeedParam == 0 ? 0 : 1);
                tempDogAnimator.SetFloat("State", animSpeedParam);
                tempDogAnimator.transform.localRotation = transform.rotation;
            }
        }

    }

    public bool IsCrouched() => isCrouched;
    public PlayerInputActions GetInputActions() => inputActions;
}
