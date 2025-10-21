using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterManager : MonoBehaviour
{
    [Header("浮力参数")]
    [Tooltip("水面高度")]
    public float waterLevel = 0f;
    
    [Tooltip("浮力强度")]
    public float buoyancyForce = 10f;
    
    [Tooltip("水的密度")]
    public float waterDensity = 1f;
    
    [Tooltip("水的阻力系数")]
    public float waterDrag = 3f;
    
    [Tooltip("水的角阻力系数")]
    public float waterAngularDrag = 1f;
    
    [Header("入水冲击缓冲")]
    [Tooltip("入水时的额外向下力")]
    public float splashDownForce = 5f;
    
    [Tooltip("入水检测速度阈值")]
    public float splashVelocityThreshold = 2f;
    
    [Tooltip("缓冲持续时间")]
    public float splashDuration = 0.5f;
    
    [Header("浮力点设置")]
    [Tooltip("浮力点位置（相对于物体中心）")]
    public Vector3[] buoyancyPoints = new Vector3[]
    {
        new Vector3(-0.5f, 0f, -0.5f),
        new Vector3(-0.5f, 0f, 0.5f),
        new Vector3(0.5f, 0f, -0.5f),
        new Vector3(0.5f, 0f, 0.5f)
    };
    
    [Header("平衡控制")]
    [Tooltip("是否启用自动平衡")]
    public bool autoStabilize = true;
    
    [Tooltip("平衡力强度")]
    public float stabilizeForce = 2f;
    
    private float defaultDrag;
    private float defaultAngularDrag;
    private float splashTimer = 0f;
    private float entryVelocity = 0f;

    private List<Rigidbody> rbList = new();

    void FixedUpdate()
    {
        foreach(var rb in rbList)
        {
            ApplyBuoyancy(rb);
        }
        
        // 更新入水计时器
        if (splashTimer > 0)
        {
            splashTimer -= Time.fixedDeltaTime;
        }
    }


    void OnTriggerEnter(Collider collider)
    {
        if(collider.TryGetComponent<PlayerController>(out var controller))
        {
            controller.isInWater = true;
            collider.GetComponent<PlayerController>().OnEnterWater();
            return;
        }
        if(collider.TryGetComponent<Rigidbody>(out var rb))
        {
            Debug.Log("Enter water RB");
            rbList.Add(rb);
            OnEnterWater(rb);
        }
    }

    // void OnTriggerStay(Collider collider)
    // {}

    void OnTriggerExit(Collider collider)
    {
        if(collider.TryGetComponent<PlayerController>(out var controller))
        {
            controller.isInWater = false;
            collider.GetComponent<PlayerController>().OnExitWater();
            return;
        }
        if(collider.TryGetComponent<Rigidbody>(out var rb))
        {
            Debug.Log("Exit water RB");
            rbList.Remove(rb);
            // OnExitWater();
        }
    }


    void ApplyBuoyancy(Rigidbody rb)
    {
        int submergedPoints = 0;
        float totalDepth = 0f;
        Vector3 centerOfBuoyancy = Vector3.zero;

        // 遍历所有浮力点
        foreach (Vector3 point in buoyancyPoints)
        {
            Vector3 worldPoint = rb.transform.TransformPoint(point);
            float depth = waterLevel - worldPoint.y;
            
            if (depth > 0)
            {
                submergedPoints++;
                totalDepth += depth;
                centerOfBuoyancy += worldPoint;
                
                // 计算浮力（使用渐进式增强，避免突然的力）
                float displacementMultiplier = Mathf.Clamp01(depth / 2f);
                
                // 根据入水状态调整浮力
                float buoyancyMultiplier = 1f;
                if (splashTimer > 0)
                {
                    // 入水期间减弱浮力
                    float splashProgress = 1f - (splashTimer / splashDuration);
                    buoyancyMultiplier = Mathf.Lerp(0.3f, 1f, splashProgress);
                }
                
                Vector3 buoyancyForceVector = buoyancyForce * buoyancyMultiplier * displacementMultiplier * waterDensity * Vector3.up;
                
                rb.AddForceAtPosition(buoyancyForceVector, worldPoint, ForceMode.Force);
                
                // 水的阻力（与速度成正比）
                Vector3 velocity = rb.GetPointVelocity(worldPoint);
                
                // 增强向下运动时的阻力
                float verticalDragMultiplier = velocity.y < 0 ? 1.5f : 1f;
                Vector3 dragForce = -velocity * waterDrag * displacementMultiplier * verticalDragMultiplier;
                rb.AddForceAtPosition(dragForce, worldPoint, ForceMode.Force);
                
                // 入水冲击力（防止弹出）
                if (splashTimer > 0 && velocity.y < 0)
                {
                    float impactForce = splashDownForce * (entryVelocity / splashVelocityThreshold);
                    rb.AddForceAtPosition(Vector3.down * impactForce, worldPoint, ForceMode.Force);
                }
            }
        }

        bool currentlyInWater = submergedPoints > 0;
        
        // 自动平衡（防止过度翻转）
        if (currentlyInWater && autoStabilize && submergedPoints > 0)
        {
            ApplyStabilization(rb);
        }

    }

    void ApplyStabilization(Rigidbody rb)
    {
        // 获取物体当前的倾斜角度
        Vector3 up = rb.transform.up;
        Vector3 targetUp = Vector3.up;
        
        // 计算需要的旋转力矩
        Vector3 torque = Vector3.Cross(up, targetUp) * stabilizeForce;
        rb.AddTorque(torque, ForceMode.Force);
    }

    void OnEnterWater(Rigidbody rb)
    {
        // 记录入水速度
        entryVelocity = Mathf.Abs(rb.velocity.y);
        
        // 如果入水速度超过阈值，启动缓冲
        // if (entryVelocity > splashVelocityThreshold)
        // {
        //     Debug.Log("Splash");
        //     splashTimer = splashDuration;
            
        //     // 立即施加一个向下的冲击力
        //     rb.AddForce(Vector3.down * splashDownForce * (entryVelocity / splashVelocityThreshold), 
        //                ForceMode.Impulse);
        // }

        rb.drag = waterDrag;
        rb.angularDrag = waterAngularDrag;
    }

    void OnExitWater(Rigidbody rb)
    {
        splashTimer = 0f;

        rb.drag = defaultDrag;
        rb.angularDrag = defaultAngularDrag;
    }

}
