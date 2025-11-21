using System.Collections.Generic;
using UnityEngine;

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

    private bool isLocked;

    void Start()
    {
        inputActions = GameInstance.Instance.inputActions;
        inputActions.Player.Interact.started += ctx => TryInteract();

        if (playerCamera == null && Camera.main)
            playerCamera = Camera.main;

        GameEventManager.OnUIShowed += OnUIShowed;
        GameEventManager.OnUIHided += OnUIHided;
    }

    void Destroy()
    {
        GameEventManager.OnUIShowed -= OnUIShowed;
        GameEventManager.OnUIHided -= OnUIHided;
    }

    void OnUIShowed()
    {
        isLocked = true;
    }

    void OnUIHided()
    {
        isLocked = false;
    }

    void Update()
    {
        if(isLocked) 
        {
            pickupPrompt.Hide();
            return;
        }
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
            if (dot > bestScore && it.CanInteract())
            {
                Vector3 origin = playerCamera.transform.position;
                Vector3 toTarget = go.transform.position - origin;
                int mask = LayerMask.GetMask("Default");
                if (!Physics.Raycast(origin, toTarget.normalized, toTarget.magnitude, mask, QueryTriggerInteraction.Ignore))
                {
                    best = it;
                    bestScore = dot;
                }
            }
        }
        if(best == null) GameEventManager.TriggerPlayerLookAt(null);
        if(best == currentTarget) return;

        if (currentTarget != null)
        {
            var mb = currentTarget as MonoBehaviour;
            if (mb == null || mb.gameObject == null || !mb.gameObject.activeInHierarchy || !currentTarget.CanInteract())
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
                {
                    pickupPrompt.Show("[E] " + currentTarget.GetInteractText());
                    GameEventManager.TriggerPlayerLookAt((currentTarget as MonoBehaviour).transform);
                }
                else
                    pickupPrompt.Hide();
            }
        }
        if (currentTarget == null && pickupPrompt.gameObject.activeSelf)
        {
            pickupPrompt.Hide();
        }
    }

    void TryInteract()
    {
        if (currentTarget != null)
        {
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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
