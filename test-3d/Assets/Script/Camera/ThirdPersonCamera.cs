using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;
    public Vector3 targetOffset = new Vector3(0, 1.7f, 0);

    [Header("Distance Settings")]
    public float normalDistance = 4f;
    public float aimDistance = 2.2f;
    public float minDistance = 1.5f;
    public float maxDistance = 6f;
    public float zoomSpeed = 2f;

    [Header("Rotation Settings")]
    public float rotationSpeed = 150f;
    public float minPitch = -30f;
    public float maxPitch = 70f;
    public float smoothTime = 0.1f;

    [Header("Aiming Settings")]
    public Vector3 normalOffset = new Vector3(0f, 1.7f, 0f);
    public Vector3 aimOffset = new Vector3(0.6f, 1.6f, 0f); // 右肩偏移
    public float aimTransitionSpeed = 10f;

    [Header("Collision Settings")]
    public LayerMask collisionMask = ~0;
    public float cameraRadius = 0.2f;

    private PlayerInputActions inputActions;
    private Vector2 lookInput;
    private float zoomInput;

    private float yaw;
    private float pitch;
    private Vector3 currentVelocity;
    private Vector3 desiredPosition;
    private Camera cam;

    private bool isLocked;
    private bool isAiming;

    private float currentDistance;
    private Vector3 currentOffset;

    void Awake()
    {
        cam = GetComponent<Camera>();
        inputActions = new PlayerInputActions();

        GameEventManager.OnUIShowed += OnUIShowed;
        GameEventManager.OnUIHided += OnUIHided;

        currentDistance = normalDistance;
        currentOffset = normalOffset;
    }

    void OnEnable()
    {
        inputActions.Enable();

        // 注册输入事件
        inputActions.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += _ => lookInput = Vector2.zero;

        GameEventManager.OnAimModeChanged += SetAiming;
    }

    void OnDisable()
    {
        inputActions.Disable();
        inputActions.Player.Look.performed -= ctx => lookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled -= _ => lookInput = Vector2.zero;
        
        GameEventManager.OnAimModeChanged -= SetAiming;
    }

    void LateUpdate()
    {
        if (!target) return;
        if (isLocked) return;

        HandleRotation();
        HandleAimingTransition();
        HandleCollision();

        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothTime);
        transform.LookAt(target.position + currentOffset);
    }

    void HandleRotation()
    {
        yaw += lookInput.x * rotationSpeed * Time.deltaTime;
        pitch -= lookInput.y * rotationSpeed * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    }

    void HandleAimingTransition()
    {
        // 平滑切换 offset / distance
        Vector3 targetOffsetPos = isAiming ? aimOffset : normalOffset;
        float targetDist = isAiming ? aimDistance : normalDistance;

        currentOffset = Vector3.Lerp(currentOffset, targetOffsetPos, Time.deltaTime * aimTransitionSpeed);
        currentDistance = Mathf.Lerp(currentDistance, targetDist, Time.deltaTime * aimTransitionSpeed);
    }

    void HandleCollision()
    {
        Vector3 targetPos = target.position + currentOffset;
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 desiredDir = rotation * Vector3.back;
        Vector3 idealPos = targetPos + desiredDir * currentDistance;

        if (Physics.SphereCast(targetPos, cameraRadius, desiredDir, out RaycastHit hit, currentDistance, collisionMask))
        {
            float adjustedDist = Mathf.Max(hit.distance - 0.2f, minDistance);
            desiredPosition = targetPos + desiredDir * adjustedDist;
        }
        else
        {
            desiredPosition = idealPos;
        }
    }

    public void SetAiming(bool aiming)
    {
        isAiming = aiming;
    }

    public bool IsAiming() => isAiming;

    public Vector3 GetCameraForwardFlat()
    {
        Vector3 forward = transform.forward;
        forward.y = 0;
        return forward.normalized;
    }

    private void OnUIShowed() => isLocked = true;
    private void OnUIHided() => isLocked = false;
}


// using UnityEngine;
// using UnityEngine.InputSystem;

// [RequireComponent(typeof(Camera))]
// public class ThirdPersonCamera : MonoBehaviour
// {
//     [Header("Target Settings")]
//     public Transform target;
//     public Vector3 targetOffset = new Vector3(0, 1.7f, 0);

//     [Header("Distance Settings")]
//     public float distance = 4f;
//     public float minDistance = 1.5f;
//     public float maxDistance = 6f;
//     public float zoomSpeed = 2f;

//     [Header("Rotation Settings")]
//     public float rotationSpeed = 150f;
//     public float minPitch = -30f;
//     public float maxPitch = 70f;
//     public float smoothTime = 0.1f;

//     [Header("Collision Settings")]
//     public LayerMask collisionMask = ~0;
//     public float cameraRadius = 0.2f;

//     private PlayerInputActions inputActions;

//     private Vector2 lookInput;
//     private float zoomInput;

//     private float yaw;
//     private float pitch;
//     private Vector3 currentVelocity;
//     private Vector3 desiredPosition;

//     private Camera cam;

//     private bool isLocked;

//     void Awake()
//     {
//         cam = GetComponent<Camera>();
//         inputActions = new PlayerInputActions();

//         GameEventManager.OnUIShowed += OnUIShowed;
//         GameEventManager.OnUIHided += OnUIHided;
//     }

//     void OnEnable()
//     {
//         inputActions.Enable();

//         // 注册输入事件
//         inputActions.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
//         inputActions.Player.Look.canceled += _ => lookInput = Vector2.zero;
//     }

//     void OnDisable()
//     {
//         inputActions.Disable();
//     }

//     void LateUpdate()
//     {
//         if (!target) return;
//         if(isLocked) return;

//         HandleRotation();
//         HandleCollision();

//         transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothTime);
//         transform.LookAt(target.position + targetOffset);
//     }

//     void HandleRotation()
//     {
//         yaw += lookInput.x * rotationSpeed * Time.deltaTime;
//         pitch -= lookInput.y * rotationSpeed * Time.deltaTime;
//         pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
//     }

//     void HandleZoom()
//     {
//         distance -= zoomInput * zoomSpeed * Time.deltaTime;

//         // 鼠标滚轮辅助
//         if (Mouse.current != null)
//         {
//             float scroll = Mouse.current.scroll.ReadValue().y;
//             if (Mathf.Abs(scroll) > 0.01f)
//                 distance -= scroll * 0.1f;
//         }

//         distance = Mathf.Clamp(distance, minDistance, maxDistance);
//     }

//     void HandleCollision()
//     {
//         Vector3 targetPos = target.position + targetOffset;
//         Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
//         Vector3 desiredDir = rotation * Vector3.back;
//         Vector3 idealPos = targetPos + desiredDir * distance;

//         if (Physics.SphereCast(targetPos, cameraRadius, desiredDir, out RaycastHit hit, distance, collisionMask))
//         {
//             float adjustedDist = Mathf.Max(hit.distance - 0.2f, minDistance);
//             desiredPosition = targetPos + desiredDir * adjustedDist;
//         }
//         else
//         {
//             desiredPosition = idealPos;
//         }
//     }

//     private void OnUIShowed()
//     {
//         isLocked = true;
//     }
    
//     private void OnUIHided()
//     {
//         isLocked = false;
//     }
// }
