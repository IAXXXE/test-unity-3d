using UnityEngine;

public class CameraController : MonoBehaviour
{
    public enum ViewMode { FirstPerson, ThirdPerson }
    public ViewMode currentView = ViewMode.ThirdPerson;

    [Header("References")]
    public Transform player;
    public Transform playerHead;
    PlayerController playerController;

    [Header("Third Person")]
    public Vector3 thirdPersonOffset = new Vector3(0f, 1.6f, -3.2f);
    public float followSmoothTime = 0.08f;
    public float rotationSmoothTime = 0.08f;

    [Header("First Person")]
    public Vector3 firstPersonOffset = new Vector3(0f, 1.6f, 0f);
    public float mouseSensitivity = 1.8f;
    public float pitchMin = -60f;
    public float pitchMax = 80f;

    [Header("Collision")]
    public LayerMask collisionMask = ~0;
    public float collisionRadius = 0.3f;
    public float collisionOffset = 0.2f;

    [Header("Zoom")]
    public float minDistance = 0.6f;
    public float maxDistance = 4.5f;
    public float scrollSpeed = 2f;

    // runtime
    float yaw = 0f;
    float pitch = 10f;
    Vector3 currentVelocity;

    void Start()
    {
        if (player == null) Debug.LogError("CameraController: player missing");
        playerController = player ? player.GetComponent<PlayerController>() : null;
        Vector3 e = transform.eulerAngles;
        yaw = e.y; pitch = e.x;
    }

    void Update()
    {
        // toggle view
        if (Input.GetKeyDown(KeyCode.V))
        {
            currentView = currentView == ViewMode.FirstPerson ? ViewMode.ThirdPerson : ViewMode.FirstPerson;
        }

        // mouse look
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

        if (currentView == ViewMode.FirstPerson) DoFirstPerson();
        else DoThirdPerson();
    }

    void DoFirstPerson()
    {
        // place camera at head (or player + firstPersonOffset)
        Vector3 targetPos = (playerHead ? playerHead.position : player.position + firstPersonOffset);
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref currentVelocity, followSmoothTime);
        transform.rotation = Quaternion.Euler(pitch, yaw + player.eulerAngles.y, 0f);

        // optionally rotate player yaw only (so movement aligns with camera)
        Vector3 playerRot = player.eulerAngles;
        player.eulerAngles = new Vector3(playerRot.x, yaw, playerRot.z);
    }

    void DoThirdPerson()
    {
        // desired camera world position from player + offset rotated by yaw/pitch
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredLocal = rot * thirdPersonOffset; // this is camera offset in world relative to player
        Vector3 desiredWorldPos = player.position + desiredLocal + Vector3.up * 0f;

        // collision: cast from player head toward desiredWorldPos
        Vector3 origin = player.position + Vector3.up * (playerController ? playerController.GetComponent<CharacterController>().height * 0.9f : 1.6f);
        Vector3 dir = (desiredWorldPos - origin).normalized;
        float desiredDist = Vector3.Distance(origin, desiredWorldPos);

        RaycastHit hit;
        float finalDist = desiredDist;
        if (Physics.SphereCast(origin, collisionRadius, dir, out hit, desiredDist + collisionOffset, collisionMask, QueryTriggerInteraction.Ignore))
        {
            finalDist = hit.distance - collisionOffset;
            finalDist = Mathf.Max(finalDist, minDistance);
        }

        Vector3 finalPos = origin + dir * finalDist;
        transform.position = Vector3.SmoothDamp(transform.position, finalPos, ref currentVelocity, followSmoothTime);

        // look at target point (player head)
        Vector3 lookTarget = player.position + Vector3.up * (playerController ? playerController.GetComponent<CharacterController>().height * 0.9f : 1.6f);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookTarget - transform.position), Time.deltaTime * (1f / rotationSmoothTime));

        // optionally rotate player yaw only when aiming or moving
        Vector3 playerRot = player.eulerAngles;
        player.eulerAngles = new Vector3(playerRot.x, yaw, playerRot.z);
    }

    void LateUpdate()
    {
        // scroll wheel zoom (affects thirdPersonOffset.z magnitude)
        if (currentView == ViewMode.ThirdPerson)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.0001f)
            {
                float curDist = Mathf.Abs(thirdPersonOffset.z);
                curDist = Mathf.Clamp(curDist - scroll * scrollSpeed, minDistance, maxDistance);
                thirdPersonOffset.z = -curDist;
            }
        }
    }
}
