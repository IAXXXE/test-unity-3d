using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform;
    CharacterController cc;

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
    float verticalVelocity = 0f;
    float currentSpeed = 0f;
    float speedVelocityRef;
    float turnSmoothVelocity;
    bool isCrouched = false;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        if (lockCursor) Cursor.lockState = CursorLockMode.Locked;
        if (cameraTransform == null && Camera.main) cameraTransform = Camera.main.transform;
        // ensure controller height set
        cc.height = standingHeight;
        Vector3 center = cc.center;
        center.y = cc.height / 2f;
        cc.center = center;
    }

    void Update()
    {
        HandleMovement();
        HandleHeightAdjust();
    }

    void HandleMovement()
    {
        // input
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputZ = Input.GetAxisRaw("Vertical");
        Vector2 input = new Vector2(inputX, inputZ);
        Vector2 inputDir = input.normalized;

        bool run = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool crouchToggle = Input.GetKeyDown(KeyCode.C);
        if (crouchToggle) isCrouched = !isCrouched;

        float targetSpeed;
        if (isCrouched) targetSpeed = crouchSpeed;
        else targetSpeed = run ? runSpeed : walkSpeed;

        float targetMagnitude = inputDir.magnitude;
        float desiredSpeed = targetSpeed * targetMagnitude;
        currentSpeed = Mathf.SmoothDamp(currentSpeed, desiredSpeed, ref speedVelocityRef, 1f / (acceleration + 0.0001f));

        // rotation & move dir based on camera forward
        if (inputDir.sqrMagnitude > 0.0001f)
        {
            float targetAngle = Mathf.Atan2(inputDir.x, inputDir.y) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            Vector3 horizontalVelocity = moveDir.normalized * currentSpeed;
            // vertical handled separately
            ApplyMovement(horizontalVelocity);
        }
        else
        {
            ApplyMovement(Vector3.zero);
        }

        // Jump & gravity
        if (IsGrounded())
        {
            if (verticalVelocity < 0f) verticalVelocity = -2f; // small stick to ground

            if (Input.GetButtonDown("Jump") && !isCrouched)
            {
                verticalVelocity = Mathf.Sqrt(2f * jumpHeight * -gravity);
            }
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
            cc.Move(Vector3.up * verticalVelocity * Time.deltaTime);
        }
    }

    void ApplyMovement(Vector3 horizontalVelocity)
    {
        Vector3 vel = horizontalVelocity;
        // ensure vertical component preserved by cc.Move earlier, so combine
        vel += Vector3.up * 0f; // vertical handled separately in Update
        cc.Move(vel * Time.deltaTime);
    }

    bool IsGrounded()
    {
        // CharacterController has isGrounded but can be jittery; also do a sphere/capsule check
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        float radius = cc.radius * 0.9f;
        return Physics.CheckSphere(origin, radius, groundMask);
    }

    void HandleHeightAdjust()
    {
        float targetHeight = isCrouched ? crouchHeight : standingHeight;
        if (Mathf.Approximately(cc.height, targetHeight)) return;
        cc.height = Mathf.Lerp(cc.height, targetHeight, Time.deltaTime * heightAdjustSpeed);

        Vector3 center = cc.center;
        center.y = cc.height / 2f;
        cc.center = center;
    }

    // Optional: expose toggles for external CameraController to query
    public bool IsCrouched() => isCrouched;
}
