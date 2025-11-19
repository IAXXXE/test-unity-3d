using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public class PetSummonSystem : MonoBehaviour
{    
    [Header("召唤设置")]
    public Transform summonPoint;
    public float summonRadius = 2f;
    
    private PetManager petManager;
    
    private void Start()
    {
        petManager = transform.parent.GetComponent<PetManager>();
        
        if (summonPoint == null)
            summonPoint = transform;
    }
    
    // 根据名称召唤宠物
    public PetBase SummonPet(string petName)
    {
        PetData data = CharacterDatabase.Instance.GetPetData(petName);
        
        if (data == null)
        {
            Debug.LogWarning($"找不到宠物: {petName}");
            return null;
        }
        
        return SummonPet(data);
    }
    
    // 根据数据召唤宠物
    public PetBase SummonPet(PetData data)
    {
        if (data.prefab == null)
        {
            Debug.LogError($"宠物预制体为空: {data.petName}");
            return null;
        }
        
        // 计算召唤位置
        Vector3 spawnPosition = GetSummonPosition();
        
        // 生成宠物
        GameObject petObject = Instantiate(data.prefab, spawnPosition, Quaternion.identity);
        PetBase pet = petObject.GetComponent<PetBase>();
        
        if (pet != null)
        {
            // 应用数据
            pet.petName = data.petName;
            pet.petType = data.petType;
            pet.followDistance = data.followDistance;
            pet.stopDistance = data.stopDistance;
            pet.teleportDistance = data.teleportDistance;
            
            // 添加到管理器
            petManager.SummonPet(pet);
            
            // 播放召唤特效
            PlaySummonEffect(spawnPosition);
            
            Debug.Log($"召唤了宠物: {data.petName}");
            
            return pet;
        }
        else
        {
            Debug.LogError($"预制体上没有PetBase组件: {data.petName}");
            Destroy(petObject);
            return null;
        }
    }
    
    private Vector3 GetSummonPosition()
    {
        // 在召唤点周围随机位置
        Vector2 randomCircle = Random.insideUnitCircle * summonRadius;
        Vector3 offset = new Vector3(randomCircle.x, 0, randomCircle.y);
        Vector3 targetPos = summonPoint.position + offset;
        
        // 确保在NavMesh上
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPos, out hit, summonRadius * 2f, NavMesh.AllAreas))
        {
            return hit.position;
        }
        
        return summonPoint.position;
    }
    
    private void PlaySummonEffect(Vector3 position)
    {
        // 播放召唤粒子特效
        // ParticleSystem effect = Instantiate(summonEffectPrefab, position, Quaternion.identity);
        // Destroy(effect.gameObject, 2f);
    }
}