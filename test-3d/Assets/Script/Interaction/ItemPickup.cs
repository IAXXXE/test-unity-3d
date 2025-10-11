using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class ItemPickup : MonoBehaviour
{
    [Header("Info")]
    public string pickupName = "Item";

    private Outline outlineComponent; // assign Outline (cakeslice) or leave null

    Rigidbody rb;
    Collider col;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        outlineComponent = GetComponent<Outline>();

        // ensure proper physics
        rb.isKinematic = false;
        rb.useGravity = true;
    }

    public void SetHighlight(bool on)
    {
        if (outlineComponent != null) outlineComponent.enabled = on;
        else
        {
            // fallback: change material emission (simple)
            var rend = GetComponent<Renderer>();
            if (rend != null)
            {
                foreach (var m in rend.materials)
                {
                    if (on) m.EnableKeyword("_EMISSION");
                    else m.DisableKeyword("_EMISSION");
                }
            }
        }
    }

    public void OnPicked()
    {
        // optional: disable physics collisions while held
        if (rb != null) { rb.isKinematic = true; rb.detectCollisions = false; }
        if (col != null) col.enabled = false;
    }

    public void OnDropped()
    {
        if (rb != null) { rb.isKinematic = false; rb.detectCollisions = true; }
        if (col != null) col.enabled = true;
    }
}
