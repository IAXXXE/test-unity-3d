//现在想做一个只在夜晚可以触发的幽灵战，触发相关代码已在其他地方实现，此代码管理进入战斗后的部分
//战斗前，会与某个特殊物体（currPoint）交互，然后以此物体为圆心，生成一个已定半径范围的圆球结界，玩家只能在圆球内部行动
//第一波敌人会在currPoint上方生成，而后每间隔一定时间，从某个生成点位生成一波敌人。
//战斗中游戏世界内的计时会停止。战斗有总战斗时间，到时间后所有敌人消失。如果在最后一波敌人出现后，战斗时间结束前消灭了所有敌人，提前结束战斗。

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime;
using UnityEngine;

public enum BattlefieldType
{
    ParkingLot,
    Farmland
}

/// <summary>
/// 幽灵预制体配置
/// </summary>
[System.Serializable]
public class GhostPrefabConfig
{
    public int ghostId;
    public string ghostName;
    public GameObject prefab;
    public int poolSize = 5; // 该类型幽灵的对象池大小
}

/// <summary>
/// 波次配置
/// </summary>
[System.Serializable]
public class WaveConfig
{
    public int waveNumber;
    public List<GhostSpawnInfo> spawnInfos; // 该波次生成的幽灵信息
}

[System.Serializable]
public class GhostSpawnInfo
{
    public int ghostId;
    public int count;
}

/// <summary>
/// 管理特定类型幽灵的对象池
/// </summary>
public class GhostTypePool
{
    public int ghostId;
    public GameObject prefab;
    private Queue<GameObject> availableObjects;
    private List<GameObject> activeObjects;
    private Transform poolParent;
    private int maxSize;

    public GhostTypePool(int ghostId, GameObject prefab, int initialSize, Transform parent)
    {
        this.ghostId = ghostId;
        this.prefab = prefab;
        this.maxSize = initialSize * 2; // 允许扩展到初始大小的2倍
        this.poolParent = parent;
        
        availableObjects = new Queue<GameObject>();
        activeObjects = new List<GameObject>();

        // 预创建对象
        for (int i = 0; i < initialSize; i++)
        {
            CreateNewInstance();
        }
    }

    private GameObject CreateNewInstance()
    {
        GameObject obj = GameObject.Instantiate(prefab, poolParent);
        obj.name = $"{prefab.name}_Pool_{ghostId}_{availableObjects.Count}";
        obj.SetActive(false);
        availableObjects.Enqueue(obj);
        return obj;
    }

    public GameObject Get()
    {
        GameObject obj;

        if (availableObjects.Count > 0)
        {
            obj = availableObjects.Dequeue();
        }
        else if (activeObjects.Count < maxSize)
        {
            // 池子空了但未达到最大容量，创建新实例
            Debug.LogWarning($"Ghost pool for ID {ghostId} exhausted, creating new instance");
            obj = CreateNewInstance();
            availableObjects.Dequeue(); // 立即取出刚创建的
        }
        else
        {
            // 达到最大容量，复用最早的活跃对象
            Debug.LogWarning($"Ghost pool for ID {ghostId} at max capacity, recycling oldest");
            obj = activeObjects[0];
            activeObjects.RemoveAt(0);
        }

        obj.SetActive(true);
        activeObjects.Add(obj);
        return obj;
    }

    public void Return(GameObject obj)
    {
        if (obj == null) return;

        obj.SetActive(false);
        activeObjects.Remove(obj);
        availableObjects.Enqueue(obj);
    }

    public void ReturnAll()
    {
        List<GameObject> objectsToReturn = new List<GameObject>(activeObjects);
        foreach (var obj in objectsToReturn)
        {
            Return(obj);
        }
    }

    public int GetActiveCount() => activeObjects.Count;
    public int GetAvailableCount() => availableObjects.Count;
}

public class GhostBattleManager : MonoBehaviour
{
    [Header("战斗配置")]
    public GhostBattleData battleData;
    
    [Header("幽灵预制体配置")]
    [SerializeField] private List<GhostPrefabConfig> ghostPrefabConfigs;
    
    [Header("波次配置")]
    [SerializeField] private List<WaveConfig> waveConfigs;
    
    [Header("特效")]
    public GameObject barrierEffectPrefab;
    
    // 多类型对象池管理
    private Dictionary<int, GhostTypePool> ghostPools = new Dictionary<int, GhostTypePool>();
    private Transform poolContainer;
    
    // 战斗点位
    private Dictionary<int, Transform> idxToTransform = new Dictionary<int, Transform>();
    private Transform currPoint;
    private List<Vector3> spawnPoints = new List<Vector3>();
    
    // 战斗状态
    private bool isBattleActive = false;
    private float battleTimer = 0f;
    private int currentWave = 0;
    private bool lastWaveSpawned = false;
    
    // 结界
    private GameObject barrierInstance;
    
    // 玩家引用
    private Transform playerTransform;
    private Vector3 battleCenter;
    
    // 统计数据
    private int totalGhostsSpawned = 0;
    private int totalGhostsKilled = 0;

    void Awake()
    {
        // 创建对象池容器
        poolContainer = new GameObject("GhostPools").transform;
        poolContainer.SetParent(transform);
        
        // 初始化所有类型的对象池
        InitializeAllPools();
    }

    void Start()
    {
        GameEventManager.OnInspirationPointInteract += OnInteract;
        
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        if (battleData == null)
        {
            battleData = new GhostBattleData();
        }
    }

    void OnDestroy()
    {
        GameEventManager.OnInspirationPointInteract -= OnInteract;
    }

    void Update()
    {
        if (isBattleActive && playerTransform != null)
        {
            ConstrainPlayerToBattlefield();
        }
    }

    /// <summary>
    /// 初始化所有幽灵类型的对象池
    /// </summary>
    private void InitializeAllPools()
    {
        foreach (var config in ghostPrefabConfigs)
        {
            if (config.prefab == null)
            {
                Debug.LogError($"Ghost prefab for ID {config.ghostId} is null!");
                continue;
            }

            // 为每个类型创建独立的父对象
            Transform typeContainer = new GameObject($"Pool_{config.ghostName}").transform;
            typeContainer.SetParent(poolContainer);

            // 创建该类型的对象池
            GhostTypePool pool = new GhostTypePool(
                config.ghostId,
                config.prefab,
                config.poolSize,
                typeContainer
            );

            ghostPools[config.ghostId] = pool;
            
            Debug.Log($"Initialized ghost pool for {config.ghostName} (ID: {config.ghostId}) with {config.poolSize} instances");
        }
    }

    /// <summary>
    /// 从对象池获取指定类型的幽灵
    /// </summary>
    private GameObject GetGhostFromPool(int ghostId)
    {
        if (!ghostPools.ContainsKey(ghostId))
        {
            Debug.LogError($"No pool found for ghost ID: {ghostId}");
            return null;
        }

        return ghostPools[ghostId].Get();
    }

    /// <summary>
    /// 将幽灵返回对象池
    /// </summary>
    public void ReturnGhostToPool(GameObject ghost)
    {
        if (ghost == null) return;
        
        var controller = ghost.GetComponent<GhostController>();
        if (controller != null)
        {
            int ghostId = controller.GhostId;
            
            if (ghostPools.ContainsKey(ghostId))
            {
                ghostPools[ghostId].Return(ghost);
                totalGhostsKilled++;
                
                // 检查是否所有幽灵都被消灭
                CheckBattleVictory();
            }
        }
    }

    /// <summary>
    /// 交互触发战斗
    /// </summary>
    private void OnInteract(Transform interactPoint)
    {
        if (isBattleActive) return;
        
        // if (!idxToTransform.ContainsKey(idx))
        // {
        //     Debug.LogError($"Invalid index: {idx}");
        //     return;
        // }

        currPoint = interactPoint;
        battleCenter = currPoint.position;

        //Time
        GameTime.Instance.PauseTime(true);
        
        spawnPoints.Clear();
        Transform spawnParent = currPoint.Find("_SpawnPoints");
        if (spawnParent != null)
        {
            foreach (Transform spawnPoint in spawnParent)
            {
                spawnPoints.Add(spawnPoint.position);
            }
        }

        if (spawnPoints.Count == 0)
        {
            Debug.LogWarning("No spawn points found, using currPoint position");
            spawnPoints.Add(currPoint.position);
        }

        StartCoroutine(StartBattle());
    }

    /// <summary>
    /// 开始战斗协程
    /// </summary>
    public IEnumerator StartBattle()
    {
        isBattleActive = true;
        battleTimer = 0f;
        currentWave = 0;
        lastWaveSpawned = false;
        totalGhostsSpawned = 0;
        totalGhostsKilled = 0;

        GameEventManager.TriggerBattleStart();
        CreateBarrier();

        // 第一波在currPoint上方生成
        if (waveConfigs.Count > 0)
        {
            SpawnWaveByConfig(currPoint.position + Vector3.up * 2f, waveConfigs[0]);
            currentWave++;
        }

        // 战斗循环
        while (battleTimer < battleData.totalBattleTime)
        {
            yield return new WaitForSeconds(1f);
            battleTimer += 1f;

            // 检查是否到达生成时间
            if (currentWave < waveConfigs.Count && 
                battleTimer >= currentWave * battleData.spawnInterval)
            {
                Vector3 randomPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Count)];
                SpawnWaveByConfig(randomPoint, waveConfigs[currentWave]);
                currentWave++;
                
                if (currentWave >= waveConfigs.Count)
                {
                    lastWaveSpawned = true;
                }
            }

            // 提前胜利检查
            if (lastWaveSpawned && GetTotalActiveGhosts() == 0)
            {
                Debug.Log("All enemies defeated! Battle victory!");
                EndBattle(true);
                yield break;
            }
        }

        Debug.Log("Battle time expired!");
        EndBattle(false);
    }

    /// <summary>
    /// 根据配置生成一波幽灵
    /// </summary>
    private void SpawnWaveByConfig(Vector3 centerPoint, WaveConfig config)
    {
        Debug.Log($"Spawning wave {config.waveNumber} at {centerPoint}");

        foreach (var spawnInfo in config.spawnInfos)
        {
            for (int i = 0; i < spawnInfo.count; i++)
            {
                SpawnGhost(spawnInfo.ghostId, centerPoint);
            }
        }
    }

    /// <summary>
    /// 生成单个幽灵
    /// </summary>
    private void SpawnGhost(int ghostId, Vector3 centerPoint)
    {
        GameObject ghost = GetGhostFromPool(ghostId);
        if (ghost == null) return;

        // 在中心点周围随机位置生成
        Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * battleData.spawnRadius;
        Vector3 spawnPos = centerPoint + new Vector3(randomCircle.x, 0, randomCircle.y);

        ghost.transform.position = spawnPos;
        ghost.transform.rotation = Quaternion.identity;

        // 初始化幽灵控制器
        var controller = ghost.GetComponent<GhostController>();
        if (controller != null)
        {
            controller.Initialize(this, ghostId);
        }

        totalGhostsSpawned++;
    }

    /// <summary>
    /// 获取当前所有活跃的幽灵数量
    /// </summary>
    private int GetTotalActiveGhosts()
    {
        int total = 0;
        foreach (var pool in ghostPools.Values)
        {
            total += pool.GetActiveCount();
        }
        return total;
    }

    /// <summary>
    /// 检查战斗胜利条件
    /// </summary>
    private void CheckBattleVictory()
    {
        if (lastWaveSpawned && GetTotalActiveGhosts() == 0 && isBattleActive)
        {
            Debug.Log("All enemies defeated during battle!");
            StopAllCoroutines();
            EndBattle(true);
        }
    }

    /// <summary>
    /// 创建战斗结界
    /// </summary>
    private void CreateBarrier()
    {
        if (barrierEffectPrefab != null)
        {
            barrierInstance = Instantiate(barrierEffectPrefab, battleCenter, Quaternion.identity);
            barrierInstance.transform.localScale = Vector3.one * battleData.battleRadius * 2f;
        }
        else
        {
            GameObject barrier = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            barrier.transform.position = battleCenter;
            barrier.transform.localScale = Vector3.one * battleData.battleRadius * 2f;
            
            var renderer = barrier.GetComponent<Renderer>();
            var material = new Material(Shader.Find("Standard"));
            material.color = new Color(1f, 0f, 0f, 0.2f);
            material.SetFloat("_Mode", 3);
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;
            renderer.material = material;
            
            Destroy(barrier.GetComponent<Collider>());
            barrierInstance = barrier;
        }
    }

    /// <summary>
    /// 限制玩家在战场内
    /// </summary>
    private void ConstrainPlayerToBattlefield()
    {
        float distance = Vector3.Distance(
            new Vector3(playerTransform.position.x, battleCenter.y, playerTransform.position.z),
            battleCenter
        );

        if (distance > battleData.battleRadius)
        {
            Vector3 direction = (new Vector3(playerTransform.position.x, battleCenter.y, playerTransform.position.z) - battleCenter).normalized;
            Vector3 constrainedPos = battleCenter + direction * battleData.battleRadius;
            playerTransform.position = new Vector3(constrainedPos.x, playerTransform.position.y, constrainedPos.z);
        }
    }

    /// <summary>
    /// 结束战斗
    /// </summary>
    private void EndBattle(bool isVictory)
    {
        isBattleActive = false;

        // 清除所有活跃的幽灵
        foreach (var pool in ghostPools.Values)
        {
            pool.ReturnAll();
        }

        // 销毁结界
        if (barrierInstance != null)
        {
            Destroy(barrierInstance);
        }

        GameEventManager.TriggerBattleEnd(isVictory);

        // Time
        GameTime.Instance.PauseTime(false);

        Debug.Log($"Battle ended. Victory: {isVictory}");
        Debug.Log($"Stats - Spawned: {totalGhostsSpawned}, Killed: {totalGhostsKilled}");
    }

    /// <summary>
    /// 注册灵感点位
    /// </summary>
    public void RegisterInspirationPoint(int idx, Transform point)
    {
        idxToTransform[idx] = point;
    }

    /// <summary>
    /// 强制停止战斗（调试用）
    /// </summary>
    public void ForceStopBattle()
    {
        if (isBattleActive)
        {
            StopAllCoroutines();
            EndBattle(false);
        }
    }

    /// <summary>
    /// 获取对象池统计信息（调试用）
    /// </summary>
    public void PrintPoolStats()
    {
        Debug.Log("=== Ghost Pool Statistics ===");
        foreach (var kvp in ghostPools)
        {
            var pool = kvp.Value;
            Debug.Log($"Ghost ID {kvp.Key}: Active={pool.GetActiveCount()}, Available={pool.GetAvailableCount()}");
        }
        Debug.Log($"Total Active Ghosts: {GetTotalActiveGhosts()}");
    }
}

// public enum BattlefieldType
// {
//     ParkingLot,
//     Farmland
// }

// public class GhostBattleManager : MonoBehaviour
// {
//     [Header("战斗配置")]
//     public GhostBattleData battleData;
//     public GameObject ghostPrefab;
//     public GameObject barrierEffectPrefab; // 结界特效预制体
    
//     [Header("对象池配置")]
//     public int poolSize = 30;
    
//     private Dictionary<int, Transform> idxToTransform = new Dictionary<int, Transform>();
//     private Transform currPoint;
//     private List<Vector3> spawnPoints = new List<Vector3>();
    
//     // 对象池
//     private Queue<GameObject> ghostPool = new Queue<GameObject>();
//     private List<GameObject> activeGhosts = new List<GameObject>();
    
//     // 战斗状态
//     private bool isBattleActive = false;
//     private float battleTimer = 0f;
//     private int currentWave = 0;
//     private int totalWaves = 0;
//     private bool lastWaveSpawned = false;
    
//     // 结界
//     private GameObject barrierInstance;
    
//     // 玩家引用
//     private Transform playerTransform;
//     private Vector3 battleCenter;

//     void Start()
//     {
//         GameEventManager.OnInspirationPointInteract += OnInteract;
        
//         // 初始化对象池
//         InitializePool();
        
//         // 获取玩家引用
//         playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        
//         if (battleData == null)
//         {
//             battleData = new GhostBattleData();
//         }
        
//         // 计算总波数
//         totalWaves = Mathf.FloorToInt(battleData.totalBattleTime / battleData.spawnInterval);
//     }

//     void OnDestroy()
//     {
//         GameEventManager.OnInspirationPointInteract -= OnInteract;
//     }

//     void Update()
//     {
//         if (isBattleActive && playerTransform != null)
//         {
//             // 限制玩家在结界内
//             ConstrainPlayerToBattlefield();
//         }
//     }

//     /// <summary>
//     /// 初始化幽灵对象池
//     /// </summary>
//     private void InitializePool()
//     {
//         if (ghostPrefab == null)
//         {
//             Debug.LogError("Ghost prefab is not assigned!");
//             return;
//         }

//         for (int i = 0; i < poolSize; i++)
//         {
//             GameObject ghost = Instantiate(ghostPrefab);
//             ghost.SetActive(false);
//             ghost.transform.SetParent(transform);
//             ghostPool.Enqueue(ghost);
//         }
//     }

//     /// <summary>
//     /// 从对象池获取幽灵
//     /// </summary>
//     private GameObject GetGhostFromPool()
//     {
//         if (ghostPool.Count > 0)
//         {
//             GameObject ghost = ghostPool.Dequeue();
//             ghost.SetActive(true);
//             activeGhosts.Add(ghost);
//             return ghost;
//         }
//         else
//         {
//             // 如果池子空了，创建新的
//             GameObject ghost = Instantiate(ghostPrefab);
//             ghost.transform.SetParent(transform);
//             activeGhosts.Add(ghost);
//             Debug.LogWarning("Ghost pool exhausted, creating new instance");
//             return ghost;
//         }
//     }

//     /// <summary>
//     /// 将幽灵返回对象池
//     /// </summary>
//     public void ReturnGhostToPool(GameObject ghost)
//     {
//         if (ghost == null) return;
        
//         ghost.SetActive(false);
//         activeGhosts.Remove(ghost);
//         ghostPool.Enqueue(ghost);
//     }

//     /// <summary>
//     /// 交互触发战斗
//     /// </summary>
//     private void OnInteract(Transform point)
//     {
//         if (isBattleActive) return;

//         // if (!idxToTransform.ContainsKey(idx))
//         // {
//         //     Debug.LogError($"Invalid index: {idx}");
//         //     return;
//         // }

//         // currPoint = idxToTransform[idx];
//         currPoint = point;
//         battleCenter = currPoint.position;
        
//         // 收集生成点
//         spawnPoints.Clear();
//         Transform spawnParent = currPoint.Find("_SpawnPoints");
//         if (spawnParent != null)
//         {
//             foreach (Transform spawnPoint in spawnParent)
//             {
//                 spawnPoints.Add(spawnPoint.position);
//             }
//         }

//         if (spawnPoints.Count == 0)
//         {
//             Debug.LogWarning("No spawn points found, using currPoint position");
//             spawnPoints.Add(currPoint.position);
//         }

//         StartCoroutine(StartBattle());
//     }

//     /// <summary>
//     /// 开始战斗协程
//     /// </summary>
//     public IEnumerator StartBattle()
//     {
//         isBattleActive = true;
//         battleTimer = 0f;
//         currentWave = 0;
//         lastWaveSpawned = false;

//         // 停止游戏世界计时
//         GameTime.Instance.PauseTime(true);
//         GameEventManager.TriggerBattleStart(); // 触发战斗开始事件

//         // 创建结界
//         CreateBarrier();

//         // 第一波在currPoint上方生成
//         SpawnGhostWave(currPoint.position + Vector3.up * 2f);
//         currentWave++;

//         // 战斗循环
//         while (battleTimer < battleData.totalBattleTime)
//         {
//             yield return new WaitForSeconds(1f);
//             battleTimer += 1f;

//             // 检查是否到达生成时间
//             if (battleTimer >= currentWave * battleData.spawnInterval && currentWave < totalWaves)
//             {
//                 Vector3 randomPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Count)];
//                 SpawnGhostWave(randomPoint);
//                 currentWave++;
                
//                 // 标记最后一波
//                 if (currentWave >= totalWaves)
//                 {
//                     lastWaveSpawned = true;
//                 }
//             }

//             // 如果最后一波已生成且所有敌人被消灭，提前结束
//             if (lastWaveSpawned && activeGhosts.Count == 0)
//             {
//                 Debug.Log("All enemies defeated! Battle victory!");
//                 EndBattle(true);
//                 yield break;
//             }
//         }

//         // 时间到，结束战斗
//         Debug.Log("Battle time expired!");
//         EndBattle(false);
//     }

//     /// <summary>
//     /// 生成一波幽灵
//     /// </summary>
//     private void SpawnGhostWave(Vector3 centerPoint)
//     {
//         Debug.Log($"Spawning wave {currentWave} at {centerPoint}");

//         for (int i = 0; i < battleData.enemiesPerWave; i++)
//         {
//             // 在中心点周围随机位置生成
//             Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * battleData.spawnRadius;
//             Vector3 spawnPos = centerPoint + new Vector3(randomCircle.x, 0, randomCircle.y);

//             GameObject ghost = GetGhostFromPool();
//             ghost.transform.position = spawnPos;
            
//             // 设置幽灵参数（如果有Enemy组件）
//             var enemyController = ghost.GetComponent<GhostController>();
//             if (enemyController != null)
//             {
//                 // TODO: Random
//                 var randomId = UnityEngine.Random.Range(0,5);
//                 enemyController.Initialize(this, 1000 + randomId); // 传递管理器引用，用于死亡时回收
//             }
//         }
//     }

//     /// <summary>
//     /// 创建战斗结界
//     /// </summary>
//     private void CreateBarrier()
//     {
//         if (barrierEffectPrefab != null)
//         {
//             barrierInstance = Instantiate(barrierEffectPrefab, battleCenter, Quaternion.identity);
//             barrierInstance.transform.localScale = Vector3.one * battleData.battleRadius * 2f;
//         }
//         else
//         {
//             // 如果没有特效，创建简单的可视化
//             GameObject barrier = GameObject.CreatePrimitive(PrimitiveType.Sphere);
//             barrier.transform.position = battleCenter;
//             barrier.transform.localScale = Vector3.one * battleData.battleRadius * 2f;
            
//             // 半透明材质
//             var renderer = barrier.GetComponent<Renderer>();
//             var material = new Material(Shader.Find("Standard"));
//             material.color = new Color(1f, 0f, 0f, 0.2f);
//             material.SetFloat("_Mode", 3); // Transparent mode
//             material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
//             material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
//             material.SetInt("_ZWrite", 0);
//             material.DisableKeyword("_ALPHATEST_ON");
//             material.EnableKeyword("_ALPHABLEND_ON");
//             material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
//             material.renderQueue = 3000;
//             renderer.material = material;
            
//             // 移除碰撞体
//             Destroy(barrier.GetComponent<Collider>());
//             barrierInstance = barrier;
//         }
//     }

//     /// <summary>
//     /// 限制玩家在战场内
//     /// </summary>
//     private void ConstrainPlayerToBattlefield()
//     {
//         float distance = Vector3.Distance(
//             new Vector3(playerTransform.position.x, battleCenter.y, playerTransform.position.z),
//             battleCenter
//         );

//         if (distance > battleData.battleRadius)
//         {
//             // 将玩家推回结界内
//             Vector3 direction = (new Vector3(playerTransform.position.x, battleCenter.y, playerTransform.position.z) - battleCenter).normalized;
//             Vector3 constrainedPos = battleCenter + direction * battleData.battleRadius;
//             playerTransform.position = new Vector3(constrainedPos.x, playerTransform.position.y, constrainedPos.z);
//         }
//     }

//     /// <summary>
//     /// 结束战斗
//     /// </summary>
//     private void EndBattle(bool isVictory)
//     {
//         isBattleActive = false;

//         // 清除所有活跃的幽灵
//         List<GameObject> ghostsToRemove = new List<GameObject>(activeGhosts);
//         foreach (var ghost in ghostsToRemove)
//         {
//             ReturnGhostToPool(ghost);
//         }

//         // 销毁结界
//         if (barrierInstance != null)
//         {
//             Destroy(barrierInstance);
//         }

//         // 恢复游戏世界计时
//         GameEventManager.TriggerBattleEnd(isVictory);
//         GameTime.Instance.PauseTime(false);

//         Debug.Log($"Battle ended. Victory: {isVictory}");
//     }

//     /// <summary>
//     /// 注册灵感点位
//     /// </summary>
//     public void RegisterInspirationPoint(int idx, Transform point)
//     {
//         idxToTransform[idx] = point;
//     }

//     /// <summary>
//     /// 强制停止战斗（调试用）
//     /// </summary>
//     public void ForceStopBattle()
//     {
//         if (isBattleActive)
//         {
//             StopAllCoroutines();
//             EndBattle(false);
//         }
//     }
// }

// // 简单的敌人控制器示例（需要根据实际项目调整）
// public class EnemyController : MonoBehaviour
// {
//     private GhostBattleManager battleManager;

//     public void Initialize(GhostBattleManager manager)
//     {
//         battleManager = manager;
//     }

//     // 当敌人死亡时调用
//     public void OnDeath()
//     {
//         if (battleManager != null)
//         {
//             battleManager.ReturnGhostToPool(gameObject);
//         }
//     }
// }