using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum IKState
{
    
}

public class PlayerIK : MonoBehaviour
{
    public Animator animator;
    public PlayerController playerController;

    [Header("IK 目标")]
    public Transform leftHandTarget;
    public Transform rightHandTarget;
    public Transform lookTarget;

    [Header("平滑参数")]
    [Range(0f, 1f)] public float ikWeight = 0f;  // 当前 IK 权重
    private float targetIKWeight = 0f;           // 目标 IK 权重
    public float transitionSpeed = 5f;           // 平滑速度
    [Range(0f, 1f)] public float ikLookWeight = 0f;  // 当前 IK 权重

    [Header("控制")]
    public bool isAiming = false; // 外部控制是否进入瞄准状态

    void Start()
    {
        animator = GetComponent<Animator>();
        playerController = transform.parent.GetComponent<PlayerController>();

        GameEventManager.OnPlayerLookAt += OnLookAt;
    }

    void Destroy()
    {
        GameEventManager.OnPlayerLookAt -= OnLookAt;
    }

    private void OnLookAt(Transform target)
    {
        lookTarget = target;
    }

    void Update()
    {
        // 根据 isAiming 状态决定目标权重
        isAiming = playerController.isAiming;
        targetIKWeight = isAiming ? 1f : 0f;

        // 用 Lerp 平滑过渡
        ikWeight = Mathf.Lerp(ikWeight, targetIKWeight, Time.deltaTime * transitionSpeed);
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (animator == null) return;

        if(isAiming)
        {
            animator.SetLookAtWeight(0);
            // 左手（握弓）
            if (leftHandTarget)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, ikWeight);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, ikWeight);
                animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);
                animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.rotation);
            }

            // 右手（拉弦）
            if (rightHandTarget)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, ikWeight);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, ikWeight);
                animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);
                animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation);
            }
        }
        else
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, ikWeight);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, ikWeight);
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, ikWeight);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, ikWeight);
        
            // LookAt（瞄准）
            if (lookTarget)
            {
                animator.SetLookAtWeight(0.5f);
                animator.SetLookAtPosition(lookTarget.position);
            }
        
        }

    }
}


// public class PlayerIK : MonoBehaviour
// {
//     public Animator animator;

//     public Transform lookTarget;

//     // Start is called before the first frame update
//     void Start()
//     {
//         animator = GetComponent<Animator>();

//         GameEventManager.OnPlayerLookAt += OnLookAt;
//     }

//     void Destroy()
//     {
//         GameEventManager.OnPlayerLookAt -= OnLookAt;
//     }

//     private void OnLookAt(Transform target)
//     {
//         lookTarget = target;
//     }

//     public void OnAnimatorIK(int layerIndex)
//     {
//         if(lookTarget == null) return;
//         animator.SetLookAtWeight(1);
//         animator.SetLookAtPosition(lookTarget.position);
//     }
// }
