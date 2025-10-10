using UnityEngine;

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
    Pickupable currPickupableItem = null;
    Pickupable heldItem = null;
    float currentThrowMultiplier = 1f;

    void Update()
    {
        if (playerCamera == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, pickRange, pickupLayer, QueryTriggerInteraction.Ignore))
        {
            Pickupable p = hit.collider.transform.GetComponent<Pickupable>();
            if(hit.collider.transform != null)
            {
            }
            else
            {
                Debug.Log("null Pickupable Script");
            }

            if (p != null && !p.IsHeld())
            {
                if(currPickupableItem != p && currPickupableItem != null) currPickupableItem.OnLoseFocus();
                p.OnFocus();
                currPickupableItem = p;
            }
        }
        else
        {
            if(currPickupableItem != null)
            {
                currPickupableItem.OnLoseFocus();
                currPickupableItem = null;
            }
        }
        
        if (Input.GetKeyDown(pickKey))
        {
            if (heldItem == null)
            {
                TryPick();
            }
            else
            {
                heldItem.Drop();
                heldItem = null;
            }
        }

        if (enableChargeThrow && heldItem != null && Input.GetKey(throwKey))
        {
            currentThrowMultiplier += chargeSpeed * Time.deltaTime;
            currentThrowMultiplier = Mathf.Clamp(currentThrowMultiplier, 1f, maxThrowForce / heldItem.defaultThrowForce);
        }

        if (heldItem != null && Input.GetKeyDown(throwKey))
        {
            Vector3 forward = playerCamera.transform.forward;
            float mul = enableChargeThrow ? currentThrowMultiplier : throwForceMultiplier;
            mul = Mathf.Clamp(mul, minThrowForce / heldItem.defaultThrowForce, maxThrowForce / heldItem.defaultThrowForce);
            Vector3 vel = heldItem.DefaultThrowVelocity(forward, mul);
            heldItem.Throw(vel);
            heldItem = null;
            currentThrowMultiplier = 1f;
        }

        if (enableChargeThrow && heldItem != null && Input.GetKeyUp(throwKey))
        {
            currentThrowMultiplier = 1f;
        }
    }

    void TryPick()
    {

        if(currPickupableItem != null)
        {
            currPickupableItem.PickUp(holdPoint);
            heldItem = currPickupableItem;
            currPickupableItem.OnLoseFocus();
            currPickupableItem = null;
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
