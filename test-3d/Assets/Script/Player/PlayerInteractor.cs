using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerController))]
public class PlayerInteractor : MonoBehaviour
{
    [Header("Detection")]
    public float detectionRadius = 2.5f;
    public float viewDotThreshold = 0.6f;
    public LayerMask interactableMask;

    [Header("References")]
    public Camera playerCamera;
    public PickupPrompt pickupPrompt;

    private PlayerInputActions inputActions;
    private List<IInteractable> nearby = new();
    private IInteractable currentTarget;

    void Start()
    {
        inputActions = GetComponent<PlayerController>().GetInputActions();
        inputActions.Player.Interact.performed += ctx => TryInteract();

        if (playerCamera == null && Camera.main)
            playerCamera = Camera.main;
    }

    void Update()
    {
        DetectNearby();
        UpdateTarget();
    }

    void DetectNearby()
    {
        nearby.Clear();
        Collider[] cols = Physics.OverlapSphere(transform.position, detectionRadius, interactableMask);
        foreach (var c in cols)
        {
            var interactable = c.GetComponent<IInteractable>();
            if (interactable != null)
                nearby.Add(interactable);
        }
    }

    void UpdateTarget()
    {
        nearby.RemoveAll(it =>
        {
            var mb = it as MonoBehaviour;
            return mb == null || mb.gameObject == null || !mb.gameObject.activeInHierarchy;
        });

        IInteractable best = null;
        float bestScore = viewDotThreshold;

        foreach (var it in nearby)
        {
            var go = (it as MonoBehaviour).gameObject;
            Vector3 dir = (go.transform.position - transform.position).normalized;
            float dot = Vector3.Dot(transform.forward, dir);
            // Debug.Log("111 : " + "dot " + dot + " bs : " + bestScore + "Can Inter " + it.CanInteract());
            if (dot > bestScore && it.CanInteract())
            {
                Vector3 origin = playerCamera.transform.position;
                Vector3 toTarget = go.transform.position - origin;
                int mask = LayerMask.GetMask("Default");
                // Debug.Log("1.5 " + !Physics.Raycast(origin, toTarget.normalized, toTarget.magnitude, mask, QueryTriggerInteraction.Ignore));
                if (!Physics.Raycast(origin, toTarget.normalized, toTarget.magnitude, mask, QueryTriggerInteraction.Ignore)) 
                {
                    best = it;
                    bestScore = dot;
                }
            }
        }

        if (currentTarget != null)
        {
            var mb = currentTarget as MonoBehaviour;
            if (mb == null || mb.gameObject == null || !mb.gameObject.activeInHierarchy)
            {
                currentTarget = null;
                if (pickupPrompt) pickupPrompt.Hide();
            }
        }

        if (best != currentTarget)
        {
            currentTarget?.SetHighlight(false);
            currentTarget = best;
            currentTarget?.SetHighlight(true);

            if (pickupPrompt)
            {
                if (currentTarget != null)
                    pickupPrompt.Show(currentTarget.GetDisplayName());
                else
                    pickupPrompt.Hide();
            }
        }
    }

    void TryInteract()
    {
        Debug.Log("press E");
        if (currentTarget != null)
        {
            Debug.Log("pick");
            if (currentTarget.CanInteract())
            {
                currentTarget.Interact(transform.GetComponent<PlayerController>());
                currentTarget = null;
            }
            else
            {
                Debug.Log("Cannot interact right now.");
            }
        }
    }

    // 示例：供 Door 或 Herb 调用
    public bool HasItem(string itemId)
    {
        // TODO: 查询背包逻辑
        return false;
    }

    public void AddItem(string itemId)
    {
        // TODO: 加入背包逻辑
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
