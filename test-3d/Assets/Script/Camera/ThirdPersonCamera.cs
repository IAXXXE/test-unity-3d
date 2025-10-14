using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;
    public Vector3 targetOffset = new Vector3(0, 1.7f, 0);

    [Header("Distance Settings")]
    public float distance = 4f;
    public float minDistance = 1.5f;
    public float maxDistance = 6f;
    public float zoomSpeed = 2f;

    [Header("Rotation Settings")]
    public float rotationSpeed = 150f;
    public float minPitch = -30f;
    public float maxPitch = 70f;
    public float smoothTime = 0.1f;

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

    void Awake()
    {
        cam = GetComponent<Camera>();
        inputActions = new PlayerInputActions();
    }

    void OnEnable()
    {
        inputActions.Enable();

        // 注册输入事件
        inputActions.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += _ => lookInput = Vector2.zero;

        // inputActions.Player.Zoom.performed += ctx => zoomInput = ctx.ReadValue<float>();
        // inputActions.Player.Zoom.canceled += _ => zoomInput = 0;
    }

    void OnDisable()
    {
        inputActions.Disable();
    }

    void LateUpdate()
    {
        if (!target) return;

        HandleRotation();
        // HandleZoom();
        HandleCollision();

        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothTime);
        transform.LookAt(target.position + targetOffset);
    }

    void HandleRotation()
    {
        yaw += lookInput.x * rotationSpeed * Time.deltaTime;
        pitch -= lookInput.y * rotationSpeed * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    }

    void HandleZoom()
    {
        distance -= zoomInput * zoomSpeed * Time.deltaTime;

        // 鼠标滚轮辅助
        if (Mouse.current != null)
        {
            float scroll = Mouse.current.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.01f)
                distance -= scroll * 0.1f;
        }

        distance = Mathf.Clamp(distance, minDistance, maxDistance);
    }

    void HandleCollision()
    {
        Vector3 targetPos = target.position + targetOffset;
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 desiredDir = rotation * Vector3.back;
        Vector3 idealPos = targetPos + desiredDir * distance;

        if (Physics.SphereCast(targetPos, cameraRadius, desiredDir, out RaycastHit hit, distance, collisionMask))
        {
            float adjustedDist = Mathf.Max(hit.distance - 0.2f, minDistance);
            desiredPosition = targetPos + desiredDir * adjustedDist;
        }
        else
        {
            desiredPosition = idealPos;
        }
    }
}



// using UnityEngine;

// public class CameraController : MonoBehaviour
// {
//     public enum ViewMode { FirstPerson, ThirdPerson }
//     public ViewMode currentView = ViewMode.ThirdPerson;

//     [Header("References")]
//     public Transform player;
//     public Transform playerHead;
//     PlayerController playerController;

//     [Header("Third Person")]
//     public Vector3 thirdPersonOffset = new Vector3(0f, 1.6f, -3.2f);
//     public float followSmoothTime = 0.08f;
//     public float rotationSmoothTime = 0.08f;

//     [Header("First Person")]
//     public Vector3 firstPersonOffset = new Vector3(0f, 1.6f, 0f);
//     public float mouseSensitivity = 1.8f;
//     public float pitchMin = -60f;
//     public float pitchMax = 80f;

//     [Header("Collision")]
//     public LayerMask collisionMask = ~0;
//     public float collisionRadius = 0.3f;
//     public float collisionOffset = 0.2f;

//     [Header("Zoom")]
//     public float minDistance = 0.6f;
//     public float maxDistance = 4.5f;
//     public float scrollSpeed = 2f;

//     // runtime
//     float yaw = 0f;
//     float pitch = 10f;
//     Vector3 currentVelocity;

//     void Start()
//     {
//         if (player == null) Debug.LogError("CameraController: player missing");
//         playerController = player ? player.GetComponent<PlayerController>() : null;
//         Vector3 e = transform.eulerAngles;
//         yaw = e.y; pitch = e.x;

//         Cursor.visible = true;
//     }

//     void Update()
//     {
//         // toggle view
//         if (Input.GetKeyDown(KeyCode.V))
//         {
//             currentView = currentView == ViewMode.FirstPerson ? ViewMode.ThirdPerson : ViewMode.FirstPerson;
//         }

//         // mouse look
//         float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
//         float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
//         yaw += mouseX;
//         pitch -= mouseY;
//         pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

//         if (currentView == ViewMode.FirstPerson) DoFirstPerson();
//         else DoThirdPerson();
//     }

//     void DoFirstPerson()
//     {
//         // place camera at head (or player + firstPersonOffset)
//         Vector3 targetPos = (playerHead ? playerHead.position : player.position + firstPersonOffset);
//         transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref currentVelocity, followSmoothTime);
//         transform.rotation = Quaternion.Euler(pitch, yaw + player.eulerAngles.y, 0f);

//         // optionally rotate player yaw only (so movement aligns with camera)
//         Vector3 playerRot = player.eulerAngles;
//         player.eulerAngles = new Vector3(playerRot.x, yaw, playerRot.z);
//     }

//     void DoThirdPerson()
//     {
//         // desired camera world position from player + offset rotated by yaw/pitch
//         Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
//         Vector3 desiredLocal = rot * thirdPersonOffset; // this is camera offset in world relative to player
//         Vector3 desiredWorldPos = player.position + desiredLocal + Vector3.up * 0f;

//         // collision: cast from player head toward desiredWorldPos
//         Vector3 origin = player.position + Vector3.up * (playerController ? playerController.GetComponent<CharacterController>().height * 0.9f : 1.6f);
//         Vector3 dir = (desiredWorldPos - origin).normalized;
//         float desiredDist = Vector3.Distance(origin, desiredWorldPos);

//         RaycastHit hit;
//         float finalDist = desiredDist;
//         if (Physics.SphereCast(origin, collisionRadius, dir, out hit, desiredDist + collisionOffset, collisionMask, QueryTriggerInteraction.Ignore))
//         {
//             finalDist = hit.distance - collisionOffset;
//             finalDist = Mathf.Max(finalDist, minDistance);
//         }

//         Vector3 finalPos = origin + dir * finalDist;
//         transform.position = Vector3.SmoothDamp(transform.position, finalPos, ref currentVelocity, followSmoothTime);

//         // look at target point (player head)
//         Vector3 lookTarget = player.position + Vector3.up * (playerController ? playerController.GetComponent<CharacterController>().height * 0.9f : 1.6f);
//         transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookTarget - transform.position), Time.deltaTime * (1f / rotationSmoothTime));

//         // optionally rotate player yaw only when aiming or moving
//         Vector3 playerRot = player.eulerAngles;
//         player.eulerAngles = new Vector3(playerRot.x, yaw, playerRot.z);
//     }

//     void LateUpdate()
//     {
//         // scroll wheel zoom (affects thirdPersonOffset.z magnitude)
//         if (currentView == ViewMode.ThirdPerson)
//         {
//             float scroll = Input.GetAxis("Mouse ScrollWheel");
//             if (Mathf.Abs(scroll) > 0.0001f)
//             {
//                 float curDist = Mathf.Abs(thirdPersonOffset.z);
//                 curDist = Mathf.Clamp(curDist - scroll * scrollSpeed, minDistance, maxDistance);
//                 thirdPersonOffset.z = -curDist;
//             }
//         }
//     }
// }
