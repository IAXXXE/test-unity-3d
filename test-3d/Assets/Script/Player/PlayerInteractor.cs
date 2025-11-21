using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerInteractor : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float detectionRadius = 2.5f;
    [SerializeField] private float viewDotThreshold = 0.6f;
    [SerializeField] private LayerMask interactableMask;
    [SerializeField] private float detectionInterval = 0.1f; // 检测间隔优化

    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private PickupPrompt pickupPrompt;

    [Header("Optimization")]
    [SerializeField] private int maxColliderCheck = 10; // 限制检测数量
    [SerializeField] private bool useDistancePriority = true; // 优先距离还是角度

    private PlayerInputActions inputActions;
    private PlayerController playerController;
    
    // 对象池优化
    private readonly List<IInteractable> nearbyInteractables = new(10);
    private readonly Collider[] colliderBuffer = new Collider[20];
    
    private IInteractable currentTarget;
    private bool isLocked;
    private float nextDetectionTime;
    
    // 缓存计算结果
    private Vector3 cameraPosition;
    private Vector3 playerPosition;
    private Vector3 playerForward;
    private int defaultLayerMask;

    #region Lifecycle

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
        defaultLayerMask = LayerMask.GetMask("Default");
    }

    void Start()
    {
        InitializeInput();
        InitializeCamera();
        SubscribeEvents();
    }

    void OnDestroy()
    {
        UnsubscribeEvents();
        if (inputActions != null)
        {
            inputActions.Player.Interact.started -= OnInteractInput;
        }
    }

    void Update()
    {
        if (isLocked)
        {
            HidePrompt();
            return;
        }

        // 降低检测频率
        if (Time.time >= nextDetectionTime)
        {
            UpdateCachedTransforms();
            DetectInteractables();
            UpdateTarget();
            nextDetectionTime = Time.time + detectionInterval;
        }
    }

    #endregion

    #region Initialization

    private void InitializeInput()
    {
        inputActions = GameInstance.Instance.inputActions;
        inputActions.Player.Interact.started += OnInteractInput;
    }

    private void InitializeCamera()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }

    private void SubscribeEvents()
    {
        GameEventManager.OnUIShowed += OnUIShowed;
        GameEventManager.OnUIHided += OnUIHided;
    }

    private void UnsubscribeEvents()
    {
        GameEventManager.OnUIShowed -= OnUIShowed;
        GameEventManager.OnUIHided -= OnUIHided;
    }

    #endregion

    #region Event Handlers

    private void OnUIShowed() => isLocked = true;
    private void OnUIHided() => isLocked = false;
    private void OnInteractInput(UnityEngine.InputSystem.InputAction.CallbackContext ctx) => TryInteract();

    #endregion

    #region Detection

    private void UpdateCachedTransforms()
    {
        playerPosition = transform.position;
        playerForward = transform.forward;
        if (playerCamera != null)
        {
            cameraPosition = playerCamera.transform.position;
        }
    }

    private void DetectInteractables()
    {
        nearbyInteractables.Clear();
        
        // 使用 NonAlloc 版本避免 GC
        int count = Physics.OverlapSphereNonAlloc(
            playerPosition, 
            detectionRadius, 
            colliderBuffer, 
            interactableMask
        );

        // 限制检测数量
        count = Mathf.Min(count, maxColliderCheck);

        for (int i = 0; i < count; i++)
        {
            var interactable = colliderBuffer[i].GetComponent<IInteractable>();
            if (interactable != null && IsInteractableValid(interactable))
            {
                nearbyInteractables.Add(interactable);
            }
        }
    }

    private bool IsInteractableValid(IInteractable interactable)
    {
        Debug.Log($"{interactable.GetInteractText()}");
        var mb = interactable as MonoBehaviour;
        if(mb == null) return false;
        Debug.Log($"{mb.gameObject.name}");
        Debug.Log($"mb != null {mb != null}. / mb.gameObject != null {mb.gameObject != null} / mb.gameObject.activeInHierarchy {mb.gameObject.activeInHierarchy}");
        return mb != null && mb.gameObject != null && mb.gameObject.activeInHierarchy;
    }

    #endregion

    #region Target Selection

    private void UpdateTarget()
    {
        // 首先验证当前目标是否仍然有效
        if (currentTarget != null && !IsInteractableValid(currentTarget))
        {
            ClearCurrentTarget();
        }

        IInteractable bestTarget = FindBestTarget();

        if (bestTarget == null)
        {
            GameEventManager.TriggerPlayerLookAt(null);
        }

        if (bestTarget == currentTarget) return;

        UpdateCurrentTarget(bestTarget);
    }

    private IInteractable FindBestTarget()
    {
        IInteractable best = null;
        float bestScore = float.MinValue;

        foreach (var interactable in nearbyInteractables)
        {
            if (!interactable.CanInteract()) continue;

            var mb = interactable as MonoBehaviour;
            Debug.Log($"FindBestTarget {mb.gameObject.name}");
            if (mb == null) continue;

            float score = CalculateInteractableScore(mb.transform.position);
            
            if (score > bestScore && IsTargetVisible(mb.transform.position))
            {
                best = interactable;
                bestScore = score;
            }
        }

        return best;
    }

    private float CalculateInteractableScore(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - playerPosition).normalized;
        float dotProduct = Vector3.Dot(playerForward, direction);
        
        // 视角检测
        if (dotProduct < viewDotThreshold)
        {
            return float.MinValue;
        }

        if (useDistancePriority)
        {
            // 优先距离：距离越近分数越高
            float distance = Vector3.Distance(playerPosition, targetPosition);
            float distanceScore = 1f - (distance / detectionRadius);
            return dotProduct * 0.3f + distanceScore * 0.7f;
        }
        else
        {
            // 优先角度
            return dotProduct;
        }
    }

    private bool IsTargetVisible(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - cameraPosition;
        float distance = direction.magnitude;
        
        // 使用 Raycast 检测遮挡
        return !Physics.Raycast(
            cameraPosition,
            direction.normalized,
            distance,
            defaultLayerMask,
            QueryTriggerInteraction.Ignore
        );
    }

    #endregion

    #region Target Management

    private void ClearCurrentTarget()
    {
        if (currentTarget != null && IsInteractableValid(currentTarget))
        {
            try
            {
                currentTarget.SetHighlight(false);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to clear highlight: {e.Message}");
            }
        }
        
        currentTarget = null;
        HidePrompt();
    }

    private void UpdateCurrentTarget(IInteractable newTarget)
    {
        // 安全清除旧目标高亮
        if (currentTarget != null && IsInteractableValid(currentTarget))
        {
            try
            {
                currentTarget.SetHighlight(false);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to clear highlight on previous target: {e.Message}");
            }
        }

        currentTarget = newTarget;

        // 设置新目标
        if (currentTarget != null && IsInteractableValid(currentTarget))
        {
            try
            {
                currentTarget.SetHighlight(true);
                ShowPromptForTarget(currentTarget);
                
                var mb = currentTarget as MonoBehaviour;
                if (mb != null)
                {
                    GameEventManager.TriggerPlayerLookAt(mb.transform);
                    Debug.Log($"best curr target {mb.gameObject.name}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to highlight new target: {e.Message}");
                currentTarget = null;
                HidePrompt();
            }
        }
        else
        {
            HidePrompt();
        }
    }

    private void ShowPromptForTarget(IInteractable target)
    {
        if (pickupPrompt != null)
        {
            string interactText = target.GetInteractText();
            pickupPrompt.Show($"[E] {interactText}");
            Debug.Log($"ShowPromptForTarget {target.GetInteractText()}");
        }
    }

    private void HidePrompt()
    {
        if (pickupPrompt != null && pickupPrompt.gameObject.activeSelf)
        {
            pickupPrompt.Hide();
        }
    }

    #endregion

    #region Interaction

    private void TryInteract()
    {
        if (isLocked || currentTarget == null) return;

        // 再次验证目标有效性
        if (!IsInteractableValid(currentTarget))
        {
            ClearCurrentTarget();
            return;
        }

        if (currentTarget.CanInteract())
        {
            try
            {
                currentTarget.Interact(playerController);
                
                // 交互后重置目标
                ClearCurrentTarget();
                
                // 立即重新检测
                nextDetectionTime = 0f;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to interact with target: {e.Message}");
                ClearCurrentTarget();
            }
        }
        else
        {
            Debug.LogWarning("Cannot interact with target right now.");
        }
    }

    #endregion

    #region Public API

    /// <summary>
    /// 强制刷新交互目标检测
    /// </summary>
    public void ForceRefresh()
    {
        nextDetectionTime = 0f;
    }

    /// <summary>
    /// 设置检测是否锁定
    /// </summary>
    public void SetLocked(bool locked)
    {
        isLocked = locked;
        if (locked)
        {
            HidePrompt();
        }
    }

    /// <summary>
    /// 获取当前目标
    /// </summary>
    public IInteractable GetCurrentTarget() => currentTarget;

    #endregion

    #region Debug

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        if (Application.isPlaying && currentTarget != null)
        {
            var mb = currentTarget as MonoBehaviour;
            if (mb != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, mb.transform.position);
                Gizmos.DrawWireSphere(mb.transform.position, 0.3f);
            }
        }
    }

    #endregion
}

// using System.Collections.Generic;
// using UnityEngine;

// [RequireComponent(typeof(PlayerController))]
// public class PlayerInteractor : MonoBehaviour
// {
//     [Header("Detection")]
//     public float detectionRadius = 2.5f;
//     public float viewDotThreshold = 0.6f;
//     public LayerMask interactableMask;

//     [Header("References")]
//     public Camera playerCamera;
//     public PickupPrompt pickupPrompt;

//     private PlayerInputActions inputActions;
//     private List<IInteractable> nearby = new();
//     private IInteractable currentTarget;

//     private bool isLocked;

//     void Start()
//     {
//         inputActions = GameInstance.Instance.inputActions;
//         inputActions.Player.Interact.started += ctx => TryInteract();

//         if (playerCamera == null && Camera.main)
//             playerCamera = Camera.main;

//         GameEventManager.OnUIShowed += OnUIShowed;
//         GameEventManager.OnUIHided += OnUIHided;
//     }

//     void Destroy()
//     {
//         GameEventManager.OnUIShowed -= OnUIShowed;
//         GameEventManager.OnUIHided -= OnUIHided;
//     }

//     void OnUIShowed()
//     {
//         isLocked = true;
//     }

//     void OnUIHided()
//     {
//         isLocked = false;
//     }

//     void Update()
//     {
//         if(isLocked) 
//         {
//             pickupPrompt.Hide();
//             return;
//         }
//         DetectNearby();
//         UpdateTarget();
//     }

//     void DetectNearby()
//     {
//         nearby.Clear();
//         Collider[] cols = Physics.OverlapSphere(transform.position, detectionRadius, interactableMask);
//         foreach (var c in cols)
//         {
//             var interactable = c.GetComponent<IInteractable>();
//             if (interactable != null)
//                 nearby.Add(interactable);
//         }
//     }

//     void UpdateTarget()
//     {
//         nearby.RemoveAll(it =>
//         {
//             var mb = it as MonoBehaviour;
//             return mb == null || mb.gameObject == null || !mb.gameObject.activeInHierarchy;
//         });

//         IInteractable best = null;
//         float bestScore = viewDotThreshold;

//         foreach (var it in nearby)
//         {
//             var go = (it as MonoBehaviour).gameObject;
//             Vector3 dir = (go.transform.position - transform.position).normalized;
//             float dot = Vector3.Dot(transform.forward, dir);
//             if (dot > bestScore && it.CanInteract())
//             {
//                 Vector3 origin = playerCamera.transform.position;
//                 Vector3 toTarget = go.transform.position - origin;
//                 int mask = LayerMask.GetMask("Default");
//                 if (!Physics.Raycast(origin, toTarget.normalized, toTarget.magnitude, mask, QueryTriggerInteraction.Ignore))
//                 {
//                     best = it;
//                     bestScore = dot;
//                 }
//             }
//         }
//         if(best == null) GameEventManager.TriggerPlayerLookAt(null);
//         if(best == currentTarget) return;

//         if (currentTarget != null)
//         {
//             var mb = currentTarget as MonoBehaviour;
//             if (mb == null || mb.gameObject == null || !mb.gameObject.activeInHierarchy || !currentTarget.CanInteract())
//             {
//                 currentTarget = null;
//                 if (pickupPrompt) pickupPrompt.Hide();
//             }
//         }

//         if (best != currentTarget)
//         {
//             currentTarget?.SetHighlight(false);
//             currentTarget = best;
//             currentTarget?.SetHighlight(true);

//             if (pickupPrompt)
//             {
//                 if (currentTarget != null)
//                 {
//                     pickupPrompt.Show("[E] " + currentTarget.GetInteractText());
//                     GameEventManager.TriggerPlayerLookAt((currentTarget as MonoBehaviour).transform);
//                 }
//                 else
//                     pickupPrompt.Hide();
//             }
//         }
//         if (currentTarget == null && pickupPrompt.gameObject.activeSelf)
//         {
//             pickupPrompt.Hide();
//         }
//     }

//     void TryInteract()
//     {
//         if (currentTarget != null)
//         {
//             if (currentTarget.CanInteract())
//             {
//                 currentTarget.Interact(transform.GetComponent<PlayerController>());
//                 currentTarget = null;
//             }
//             else
//             {
//                 Debug.Log("Cannot interact right now.");
//             }
//         }
//     }

//     void OnDrawGizmosSelected()
//     {
//         Gizmos.color = Color.cyan;
//         Gizmos.DrawWireSphere(transform.position, detectionRadius);
//     }
// }
