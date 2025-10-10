using UnityEngine;

/// <summary>
/// 简单的可拾取物体组件
/// 用法：物体需包含 Rigidbody + Collider（非 trigger）并把这个脚本附上
/// 支持两种持有模式：isKinematicWhileHeld = true => 持有时设为 kinematic 并 parent 到 holdPoint
///                     false => 持有时仍使用物理（少数情况使用），当前实现以 kinematic 为主（更稳定）
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Pickupable : MonoBehaviour
{
    [Header("Pickup Settings")]
    public bool isPickupEnabled = true;
    public bool isKinematicWhileHeld = true;
    public Vector3 holdPositionOffset = Vector3.zero;
    public Vector3 holdRotationEuler = Vector3.zero;

    [Header("Throw Settings")]
    public float defaultThrowForce = 8f;
    public float extraUpwardsVelocity = 1.0f;

    // runtime
    Rigidbody rb;
    Transform originalParent;
    bool isHeld = false;
    Transform holder;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        originalParent = transform.parent;
    }

    public bool IsHeld() => isHeld;

    public void PickUp(Transform holdPoint)
    {
        if (!isPickupEnabled) return;
        if (isHeld) return;

        holder = holdPoint;
        isHeld = true;

        // parent & position
        transform.SetParent(holder, false);
        transform.localPosition = holdPositionOffset;
        transform.localRotation = Quaternion.Euler(holdRotationEuler);

        if (isKinematicWhileHeld)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.detectCollisions = false; // optional to avoid clipping while holding
        }
        else
        {
            // keep physics on - could add joint here if desired (not implemented)
        }
    }

    /// <summary>
    /// Drop without imparting velocity (放下)
    /// </summary>
    public void Drop()
    {
        if (!isHeld) return;

        transform.SetParent(originalParent, true);
        isHeld = false;

        if (isKinematicWhileHeld)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
        }

        holder = null;
    }

    /// <summary>
    /// Throw with given velocity vector (世界空间)
    /// </summary>
    public void Throw(Vector3 worldVelocity)
    {
        if (!isHeld) return;

        transform.SetParent(originalParent, true);
        isHeld = false;

        if (isKinematicWhileHeld)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
        }

        // apply velocity
        rb.velocity = Vector3.zero; // reset
        rb.angularVelocity = Vector3.zero;
        rb.AddForce(worldVelocity, ForceMode.VelocityChange); // immediate velocity change
        holder = null;
    }

    /// <summary>
    /// Convenience: calculate default throw velocity in forward dir
    /// </summary>
    public Vector3 DefaultThrowVelocity(Vector3 forwardDirection, float forceMultiplier)
    {
        Vector3 vel = forwardDirection.normalized * (defaultThrowForce * forceMultiplier);
        vel.y += extraUpwardsVelocity;
        return vel;
    }
}
