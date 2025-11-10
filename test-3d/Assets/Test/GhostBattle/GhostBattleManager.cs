//现在想做一个只在夜晚可以触发的幽灵战，触发相关代码已在其他地方实现，此代码管理进入战斗后的部分
//战斗前，会与某个特殊物体（currPoint）交互，然后以此物体为圆心，生成一个已定半径范围的圆球结界，玩家只能在圆球内部行动
//第一波敌人会在currPoint上方生成，而后每间隔一定时间，从某个生成点位生成一波敌人。
//战斗中游戏世界内的计时会停止。战斗有总战斗时间，到时间后所有敌人消失。如果在最后一波敌人出现后，战斗时间结束前消灭了所有敌人，提前结束战斗。

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattlefieldType
{
    ParkingLot,
    Farmland
}

public class GhostBattleManager : MonoBehaviour
{
    [Header("战斗配置")]
    public GhostBattleData battleData;
    public GameObject ghostPrefab;
    public GameObject barrierEffectPrefab; // 结界特效预制体
    
    [Header("对象池配置")]
    public int poolSize = 30;
    
    private Dictionary<int, Transform> idxToTransform = new Dictionary<int, Transform>();
    private Transform currPoint;
    private List<Vector3> spawnPoints = new List<Vector3>();
    
    // 对象池
    private Queue<GameObject> ghostPool = new Queue<GameObject>();
    private List<GameObject> activeGhosts = new List<GameObject>();
    
    // 战斗状态
    private bool isBattleActive = false;
    private float battleTimer = 0f;
    private int currentWave = 0;
    private int totalWaves = 0;
    private bool lastWaveSpawned = false;
    
    // 结界
    private GameObject barrierInstance;
    
    // 玩家引用
    private Transform playerTransform;
    private Vector3 battleCenter;

    void Start()
    {
        GameEventManager.OnInspirationPointInteract += OnInteract;
        
        // 初始化对象池
        InitializePool();
        
        // 获取玩家引用
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        if (battleData == null)
        {
            battleData = new GhostBattleData();
        }
        
        // 计算总波数
        totalWaves = Mathf.FloorToInt(battleData.totalBattleTime / battleData.spawnInterval);
    }

    void OnDestroy()
    {
        GameEventManager.OnInspirationPointInteract -= OnInteract;
    }

    void Update()
    {
        if (isBattleActive && playerTransform != null)
        {
            // 限制玩家在结界内
            ConstrainPlayerToBattlefield();
        }
    }

    /// <summary>
    /// 初始化幽灵对象池
    /// </summary>
    private void InitializePool()
    {
        if (ghostPrefab == null)
        {
            Debug.LogError("Ghost prefab is not assigned!");
            return;
        }

        for (int i = 0; i < poolSize; i++)
        {
            GameObject ghost = Instantiate(ghostPrefab);
            ghost.SetActive(false);
            ghost.transform.SetParent(transform);
            ghostPool.Enqueue(ghost);
        }
    }

    /// <summary>
    /// 从对象池获取幽灵
    /// </summary>
    private GameObject GetGhostFromPool()
    {
        if (ghostPool.Count > 0)
        {
            GameObject ghost = ghostPool.Dequeue();
            ghost.SetActive(true);
            activeGhosts.Add(ghost);
            return ghost;
        }
        else
        {
            // 如果池子空了，创建新的
            GameObject ghost = Instantiate(ghostPrefab);
            ghost.transform.SetParent(transform);
            activeGhosts.Add(ghost);
            Debug.LogWarning("Ghost pool exhausted, creating new instance");
            return ghost;
        }
    }

    /// <summary>
    /// 将幽灵返回对象池
    /// </summary>
    public void ReturnGhostToPool(GameObject ghost)
    {
        if (ghost == null) return;
        
        ghost.SetActive(false);
        activeGhosts.Remove(ghost);
        ghostPool.Enqueue(ghost);
    }

    /// <summary>
    /// 交互触发战斗
    /// </summary>
    private void OnInteract(Transform point)
    {
        if (isBattleActive) return;

        // if (!idxToTransform.ContainsKey(idx))
        // {
        //     Debug.LogError($"Invalid index: {idx}");
        //     return;
        // }

        // currPoint = idxToTransform[idx];
        currPoint = point;
        battleCenter = currPoint.position;
        
        // 收集生成点
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

        // 停止游戏世界计时
        Time.timeScale = 1f; // 如果需要暂停其他系统，在这里处理
        GameEventManager.TriggerBattleStart(); // 触发战斗开始事件

        // 创建结界
        CreateBarrier();

        // 第一波在currPoint上方生成
        SpawnGhostWave(currPoint.position + Vector3.up * 2f);
        currentWave++;

        // 战斗循环
        while (battleTimer < battleData.totalBattleTime)
        {
            yield return new WaitForSeconds(1f);
            battleTimer += 1f;

            // 检查是否到达生成时间
            if (battleTimer >= currentWave * battleData.spawnInterval && currentWave < totalWaves)
            {
                Vector3 randomPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Count)];
                SpawnGhostWave(randomPoint);
                currentWave++;
                
                // 标记最后一波
                if (currentWave >= totalWaves)
                {
                    lastWaveSpawned = true;
                }
            }

            // 如果最后一波已生成且所有敌人被消灭，提前结束
            if (lastWaveSpawned && activeGhosts.Count == 0)
            {
                Debug.Log("All enemies defeated! Battle victory!");
                EndBattle(true);
                yield break;
            }
        }

        // 时间到，结束战斗
        Debug.Log("Battle time expired!");
        EndBattle(false);
    }

    /// <summary>
    /// 生成一波幽灵
    /// </summary>
    private void SpawnGhostWave(Vector3 centerPoint)
    {
        Debug.Log($"Spawning wave {currentWave} at {centerPoint}");

        for (int i = 0; i < battleData.enemiesPerWave; i++)
        {
            // 在中心点周围随机位置生成
            Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * battleData.spawnRadius;
            Vector3 spawnPos = centerPoint + new Vector3(randomCircle.x, 0, randomCircle.y);

            GameObject ghost = GetGhostFromPool();
            ghost.transform.position = spawnPos;
            
            // 设置幽灵参数（如果有Enemy组件）
            var enemyController = ghost.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                enemyController.Initialize(this); // 传递管理器引用，用于死亡时回收
            }
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
            // 如果没有特效，创建简单的可视化
            GameObject barrier = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            barrier.transform.position = battleCenter;
            barrier.transform.localScale = Vector3.one * battleData.battleRadius * 2f;
            
            // 半透明材质
            var renderer = barrier.GetComponent<Renderer>();
            var material = new Material(Shader.Find("Standard"));
            material.color = new Color(1f, 0f, 0f, 0.2f);
            material.SetFloat("_Mode", 3); // Transparent mode
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;
            renderer.material = material;
            
            // 移除碰撞体
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
            // 将玩家推回结界内
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
        List<GameObject> ghostsToRemove = new List<GameObject>(activeGhosts);
        foreach (var ghost in ghostsToRemove)
        {
            ReturnGhostToPool(ghost);
        }

        // 销毁结界
        if (barrierInstance != null)
        {
            Destroy(barrierInstance);
        }

        // 恢复游戏世界计时
        GameEventManager.TriggerBattleEnd(isVictory);

        Debug.Log($"Battle ended. Victory: {isVictory}");
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
}

// 简单的敌人控制器示例（需要根据实际项目调整）
public class EnemyController : MonoBehaviour
{
    private GhostBattleManager battleManager;

    public void Initialize(GhostBattleManager manager)
    {
        battleManager = manager;
    }

    // 当敌人死亡时调用
    public void OnDeath()
    {
        if (battleManager != null)
        {
            battleManager.ReturnGhostToPool(gameObject);
        }
    }
}