using UnityEngine;

/// <summary>
/// 玩家拾取控制器（使用射线拾取）
/// 说明：把这个脚本挂在 Player 上，填入 cameraTransform & holdPoint（手的位置）
/// 默认按 E 拾取/放下，左键投掷（也支持自定义键）
/// </summary>
public class PlayerPickupController : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public Transform holdPoint; // 空 GameObject（放在手前）
    
    [Header("Pick Settings")]
    public float pickRange = 3.0f;
    public LayerMask pickupLayer = ~0;
    public KeyCode pickKey = KeyCode.E;

    [Header("Throw Settings")]
    public KeyCode throwKey = KeyCode.Mouse0; // left click
    public float throwForceMultiplier = 1f;
    public float maxThrowForce = 20f;
    public float minThrowForce = 2f;

    [Header("Aim / Charge (optional)")]
    public bool enableChargeThrow = false;
    public float chargeSpeed = 4f; // how fast throw force grows per second

    // runtime
    Pickupable heldItem = null;
    float currentThrowMultiplier = 1f;

    void Update()
    {
        if (playerCamera == null) return;

        // pick/drop
        if (Input.GetKeyDown(pickKey))
        {
            if (heldItem == null)
            {
                TryPick();
            }
            else
            {
                // drop (no extra velocity)
                heldItem.Drop();
                heldItem = null;
            }
        }

        // charging throw
        if (enableChargeThrow && heldItem != null && Input.GetKey(throwKey))
        {
            currentThrowMultiplier += chargeSpeed * Time.deltaTime;
            currentThrowMultiplier = Mathf.Clamp(currentThrowMultiplier, 1f, maxThrowForce / heldItem.defaultThrowForce);
        }

        // throw
        if (heldItem != null && Input.GetKeyDown(throwKey))
        {
            // immediate throw using current multiplier
            Vector3 forward = playerCamera.transform.forward;
            float mul = enableChargeThrow ? currentThrowMultiplier : throwForceMultiplier;
            mul = Mathf.Clamp(mul, minThrowForce / heldItem.defaultThrowForce, maxThrowForce / heldItem.defaultThrowForce);
            Vector3 vel = heldItem.DefaultThrowVelocity(forward, mul);
            heldItem.Throw(vel);
            heldItem = null;
            currentThrowMultiplier = 1f;
        }

        // cancel charge on release if enabled
        if (enableChargeThrow && heldItem != null && Input.GetKeyUp(throwKey))
        {
            // do nothing special here (we already threw on KeyDown) — if you want throw on KeyUp, change logic
            currentThrowMultiplier = 1f;
        }
    }

    void TryPick()
    {
        Debug.Log("try pick");
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, pickRange, pickupLayer, QueryTriggerInteraction.Ignore))
        {
            Debug.Log("Physics.Raycast");
            Pickupable p = hit.collider.GetComponentInParent<Pickupable>();
            if(hit.collider.transform != null)
            {
                Debug.Log(hit.collider.transform.gameObject.name);
            }
            if (p == null)
            {
                // sometimes collider is child; try root, etc.
                p = hit.collider.transform.GetComponent<Pickupable>();
                Debug.Log("null");
            }

            if (p != null && !p.IsHeld())
            {
                Debug.Log("pick up");
                p.PickUp(holdPoint);
                heldItem = p;
            }
        }
    }

    // Optional: draw pick ray gizmo
    void OnDrawGizmosSelected()
    {
        if (playerCamera != null)
        {
            Gizmos.color = Color.yellow;
            Vector3 origin = playerCamera.transform.position;
            Vector3 dir = playerCamera.transform.forward;
            Gizmos.DrawLine(origin, origin + dir * pickRange);
        }
    }
}
