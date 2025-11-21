using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public abstract class PetBase : MonoBehaviour, IPet, IInteractable
{
    [Header("基础信息")]
    public string petName = "Pet";
    public PetType petType;
    
    [Header("跟随设置")]
    public float followDistance = 3f;        // 跟随距离
    public float stopDistance = 2f;          // 停止距离
    public float teleportDistance = 15f;     // 传送距离
    public float updateInterval = 0.2f;      // 更新间隔
    
    [Header("互动设置")]
    public float interactionRange = 3f;      // 互动范围
    public Transform interactionPoint;       // 互动点位置
    
    [Header("组件引用")]
    public NavMeshAgent agent;
    public Animator animator;
    
    protected Transform followTarget;
    protected bool isFollowing = false;
    protected bool isInInteractionMode = false;
    protected float updateTimer;
    
    // 动画参数
    protected readonly int speedHash = Animator.StringToHash("Speed");
    protected readonly int isMovingHash = Animator.StringToHash("IsMoving");
    
    protected virtual void Start()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
            
        if (animator == null)
            animator = GetComponent<Animator>();
            
        if (interactionPoint == null)
            interactionPoint = transform;
    }
    
    protected virtual void Update()
    {
        if (isFollowing && followTarget != null && !isInInteractionMode)
        {
            UpdateFollow();
        }
        
        UpdateAnimation();
    }
    
    // 跟随逻辑
    protected virtual void UpdateFollow()
    {
        updateTimer += Time.deltaTime;
        
        if (updateTimer >= updateInterval)
        {
            updateTimer = 0f;
            
            float distance = Vector3.Distance(transform.position, followTarget.position);
            
            // 距离太远，直接传送
            if (distance > teleportDistance)
            {
                TeleportToTarget();
            }
            // 距离适中，跟随
            else if (distance > followDistance)
            {
                if (agent.enabled)
                {
                    agent.SetDestination(followTarget.position);
                }
            }
            // 距离太近，停止
            else if (distance <= stopDistance)
            {
                if (agent.enabled)
                {
                    agent.ResetPath();
                }
            }
        }
    }
    
    protected virtual void TeleportToTarget()
    {
        // 在目标附近找一个有效位置
        Vector3 randomOffset = Random.insideUnitSphere * 2f;
        randomOffset.y = 0;
        Vector3 targetPos = followTarget.position + randomOffset;
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPos, out hit, 5f, NavMesh.AllAreas))
        {
            transform.position = hit.position;
            
            // 播放传送特效
            OnTeleport();
        }
    }
    
    protected virtual void UpdateAnimation()
    {
        if (animator == null || agent == null) return;
        
        float speed = agent.velocity.magnitude;
        animator.SetFloat(speedHash, speed);
        animator.SetBool(isMovingHash, speed > 0.1f);
    }
    
    // IPet接口实现
    public string GetPetName() => petName;
    public PetType GetPetType() => petType;
    public Transform GetTransform() => transform;
    public bool IsFollowing() => isFollowing;
    
    public virtual void Follow(Transform target)
    {
        followTarget = target;
        isFollowing = true;
        
        if (agent != null)
            agent.enabled = true;
            
        OnStartFollow();
    }
    
    public virtual void StopFollow()
    {
        isFollowing = false;
        
        if (agent != null && agent.enabled)
            agent.ResetPath();
            
        OnStopFollow();
    }
    
    // 进入互动模式
    public virtual void EnterInteractionMode()
    {
        isInInteractionMode = true;
        
        if (agent != null && agent.enabled)
            agent.ResetPath();
            
        // 面向玩家
        if (followTarget != null)
        {
            Vector3 direction = followTarget.position - transform.position;
            direction.y = 0;
            if (direction != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(direction);
        }
        
        OnEnterInteractionMode();
    }
    
    // 退出互动模式
    public virtual void ExitInteractionMode()
    {
        isInInteractionMode = false;
        OnExitInteractionMode();
    }
    
    // 执行互动
    public abstract void Interact(InteractionType interactionType);
    
    // 获取可用互动选项
    public abstract InteractionOption[] GetAvailableInteractions();
    
    // 检查是否在互动范围内
    public bool IsInInteractionRange(Transform target)
    {
        return Vector3.Distance(interactionPoint.position, target.position) <= interactionRange;
    }
    
    // 事件回调（子类可重写）
    protected virtual void OnStartFollow() 
    {
        Debug.Log($"{petName} 开始跟随");
    }
    
    protected virtual void OnStopFollow() 
    {
        Debug.Log($"{petName} 停止跟随");
    }
    
    protected virtual void OnTeleport() 
    {
        Debug.Log($"{petName} 传送到主人身边");
        // 播放传送粒子特效
    }
    
    protected virtual void OnEnterInteractionMode() 
    {
        Debug.Log($"{petName} 进入互动模式");
    }
    
    protected virtual void OnExitInteractionMode() 
    {
        Debug.Log($"{petName} 退出互动模式");
    }

        // IInteractable

    public string GetInteractText()
    {
        return $"“E” 与 {petName} 互动";
    }

    public bool CanInteract()
    {
        return true;
    }

    public void Interact(PlayerController player)
    {
        PetManager.Instance.EnterInteractionMode(this);
    }

    public void SetHighlight(bool on)
    {
        return;
    }

    public string GetDisplayName()
    {
        return "soso";
    }
}
