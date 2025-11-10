using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InspirationGhost : MonoBehaviour, IDamageable
{
    // 在这个代码内完成简单的AI部分
    public bool IsDead => throw new System.NotImplementedException();

    public float CurrentHealth => throw new System.NotImplementedException();

    public float MaxHealth => throw new System.NotImplementedException();

    public bool IsWeakPoint(Vector3 hitPoint)
    {
        return true;
    }

    public void TakeDamage(DamageInfo damageInfo)
    {
        Debug.Log("Ghost Take Damage");
        Destroy(gameObject);
    }

    // // Start is called before the first frame update
    // void Start()
    // {
        
    // }

    void Update()
    {
        // 幽灵不受重力影响，缓慢飘向玩家
    }
}
