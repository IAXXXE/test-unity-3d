using System;
using System.Collections.Generic;
using EasyButtons;
using UnityEngine;

public class PlayerIKController : MonoBehaviour
{
    public static PlayerIKController Instance;

    public Animator animator;
    public PlayerController playerController;

    [Header("IK 目标")]
    public Transform leftHandTarget;
    public Transform rightHandTarget;
    public Transform lookTarget;

    [Header("平滑参数")]
    [Range(0f, 1f)] public float ikWeight = 0f;
    [Range(0f, 1f)] public float lookWeight = 0f;
    private float targetIKWeight = 0f;
    private float targetLookWeight = 0f;
    public float transitionSpeed = 5f;
    public float lookPositionSpeed = 8f; // LookAt 位置插值速度

    [Header("控制")]
    public bool isAiming = false;
    
    private Vector3 currentLookPosition; // 当前平滑的注视位置
    private Vector3 targetLookPosition;  // 目标注视位置

    private Dictionary<AnimActionType, string> typeToString = new Dictionary<AnimActionType, string>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        playerController = transform.parent.GetComponent<PlayerController>();
        GameEventManager.OnPlayerLookAt += OnLookAt;
        
        // 初始化位置为前方
        currentLookPosition = transform.position + transform.forward * 2f;
        targetLookPosition = currentLookPosition;

        typeToString = new Dictionary<AnimActionType, string>
        {
            { AnimActionType.LightAttack, "LightAttack" },
            { AnimActionType.Farm, "IsFarming" },
            { AnimActionType.Fish, "IsFishing" },
            { AnimActionType.Gather, "IsGathering" },
            { AnimActionType.Hammer, "IsHammering" },
            { AnimActionType.Mining, "IsMining" }
        };

    }

    void OnDestroy()
    {
        GameEventManager.OnPlayerLookAt -= OnLookAt;
    }

    private void OnLookAt(Transform target)
    {
        lookTarget = target;
    }

    void Update()
    {
        isAiming = playerController.isAiming;
        
        // 设置目标权重
        targetIKWeight = isAiming ? 1f : 0f;
        targetLookWeight = (!isAiming && lookTarget != null) ? 0.5f : 0f;

        // 平滑过渡权重
        ikWeight = Mathf.Lerp(ikWeight, targetIKWeight, Time.deltaTime * transitionSpeed);
        lookWeight = Mathf.Lerp(lookWeight, targetLookWeight, Time.deltaTime * transitionSpeed);
        
        // 更新目标位置
        if (lookTarget != null)
        {
            targetLookPosition = lookTarget.position;
        }
        
        // 平滑插值当前注视位置
        currentLookPosition = Vector3.Lerp(currentLookPosition, targetLookPosition, Time.deltaTime * lookPositionSpeed);
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (animator == null) return;

        // 手部 IK
        if (leftHandTarget)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, ikWeight);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, ikWeight);
            animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.rotation);
        }

        if (rightHandTarget)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, ikWeight);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, ikWeight);
            animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);
            animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation);
        }

        // LookAt（使用平滑后的位置）
        animator.SetLookAtWeight(lookWeight);
        if (lookWeight > 0.01f)
        {
            animator.SetLookAtPosition(currentLookPosition);
        }
    }

    [Button]
    public void SetAnimBool(AnimActionType type, bool isTrue)
    {
        // animator.SetLayerWeight(animator.GetLayerIndex("BaseLayer"), isTrue ? 0 : 1);
        // animator.SetLayerWeight(animator.GetLayerIndex("ActionLayer"), isTrue ? 1 : 0);
        animator.SetBool(typeToString[type], isTrue);
    }

    public void SetAnimTrigger(AnimActionType type)
    {
        animator.SetTrigger(typeToString[type]);
    }

    public void OnAttackHit()
    {
        Debug.Log("OnHit");
        GameEventManager.TriggerLightAttackHit();
    }
}

public enum AnimActionType
{
    None,
    LightAttack,
    ComboAttack,
    Farm,
    Fish,
    Gather,
    Hammer,
    Mining

}

// [RequireComponent(typeof(Animator))]
// public class PlayerIKController : MonoBehaviour
// {
//     private Animator animator;
//     private PlayerController playerController;

//     [Header("公共平滑参数")]
//     [Range(0f, 10f)] public float transitionSpeed = 5f;

//     [Header("LookAt 目标")]
//     public Transform lookTarget;
//     [Range(0f, 1f)] public float lookWeight;

//     [Header("Aim IK 目标（弓箭）")]
//     public Transform leftHandTarget;
//     public Transform rightHandTarget;
//     [Range(0f, 1f)] public float aimWeight;

//     [Header("Melee IK 目标（近战）")]
//     public Transform meleeTarget;     // 攻击方向或敌人中心
//     [Range(0f, 1f)] public float meleeWeight;

//     private IKState currentState = IKState.None;
//     private IKState targetState = IKState.None;

//     private float currentWeight = 0f; // 当前状态的总权重
//     private float targetWeight = 0f;

//     void Start()
//     {
//         animator = GetComponent<Animator>();
//         playerController = transform.parent.GetComponent<PlayerController>();

//         GameEventManager.OnPlayerLookAt += OnLookAt;
//         GameEventManager.OnAimModeChanged += OnAimModeChanged;
//     }

//     void Destroy()
//     {
//         GameEventManager.OnPlayerLookAt -= OnLookAt;
//         GameEventManager.OnAimModeChanged += OnAimModeChanged;
//     }

//     private void OnLookAt(Transform target)
//     {
//         lookTarget = target;
//         targetState = IKState.LookAt;
//     }

//     private void OnAimModeChanged(bool isAiming)
//     {
//         if(isAiming) targetState = IKState.Aim;
//         else targetState = IKState.LookAt;
//     }

//     void Update()
//     {
//         // 平滑过渡状态权重
//         currentWeight = Mathf.MoveTowards(currentWeight, targetWeight, Time.deltaTime * transitionSpeed);

//         // 根据当前目标状态，计算各通道权重
//         lookWeight  = (targetState == IKState.LookAt) ? currentWeight : 0f;
//         aimWeight   = (targetState == IKState.Aim)    ? currentWeight : 0f;
//         meleeWeight = (targetState == IKState.Melee)  ? currentWeight : 0f;
//     }

//     public void SetState(IKState newState)
//     {
//         if (newState == targetState) return;

//         targetState = newState;
//         targetWeight = (newState == IKState.None) ? 0f : 1f;
//     }

//     void OnAnimatorIK(int layerIndex)
//     {
//         if (!animator) return;

//         // ---------- LOOKAT ----------
//         if (lookTarget && lookWeight > 0.01f)
//         {
//             animator.SetLookAtWeight(lookWeight);
//             animator.SetLookAtPosition(lookTarget.position);
//         }

//         // ---------- AIM (双手) ----------
//         if (aimWeight > 0.01f)
//         {
//             if (leftHandTarget)
//             {
//                 animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, aimWeight);
//                 animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, aimWeight);
//                 animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);
//                 animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.rotation);
//             }

//             if (rightHandTarget)
//             {
//                 animator.SetIKPositionWeight(AvatarIKGoal.RightHand, aimWeight);
//                 animator.SetIKRotationWeight(AvatarIKGoal.RightHand, aimWeight);
//                 animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);
//                 animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation);
//             }

//             if (lookTarget)
//             {
//                 animator.SetLookAtWeight(aimWeight);
//                 animator.SetLookAtPosition(lookTarget.position);
//             }
//         }

//         // ---------- MELEE ----------
//         if (meleeWeight > 0.01f && meleeTarget)
//         {
//             // 近战时轻微引导角色上半身朝向敌人中心
//             animator.SetLookAtWeight(meleeWeight * 0.8f);
//             animator.SetLookAtPosition(meleeTarget.position);

//             // 可选：让右手稍微“指向”目标（增加命中感）
//             animator.SetIKPositionWeight(AvatarIKGoal.RightHand, meleeWeight * 0.5f);
//             animator.SetIKPosition(AvatarIKGoal.RightHand, meleeTarget.position + meleeTarget.forward * 0.2f);
//         }
//     }
// }