using System.Collections.Generic;
using EasyButtons;
using Unity.VisualScripting;
using UnityEngine;

public class ResourceData
{
    public List<Transform> waterTransList = new();
    public List<Transform> foodTransList = new();
}

public class NaturalManager : MonoBehaviour
{
    public GameObject food;
    public GameObject water;
    public GameObject chicken;
    public GameObject tiger;

    public ResourceData resourceData = new();

    private Transform evnTransform;
    private Transform resourceTransform;
    private Transform creatureTransform;
    private Terrain terrain;

    private List<CreatureAI> creatures = new();

    PointGenerator pointGenerator = new();

    void Start()
    {
        GameEventManager.OnTerrainGenerated += GetEvnTransform;
        GameEventManager.OnHourChanged += UpdateByHour;
        GameEventManager.OnNewDay += UpdateByDay;
    }

    void OnDestroy()
    {
        GameEventManager.OnTerrainGenerated -= GetEvnTransform;
        GameEventManager.OnHourChanged -= UpdateByHour;
        GameEventManager.OnNewDay -= UpdateByDay;
    }

    void UpdateByHour(float hour)
    {
        var removeList = new List<CreatureAI>();
        foreach(var creature in creatures)
        {
            if (creature == null || creature.gameObject == null) { removeList.Add(creature); continue; }
            creature.UpdateSurvivalStats();   

            // 执行行为树决策
            if (creature.isStateLocked)
            {
                creature.stateLockTimer -= Time.deltaTime;
                if (creature.stateLockTimer <= 0)
                    creature.isStateLocked = false;
            }
            else
            {
                creature.behaviorTree.Evaluate();
            }
        }

        foreach(var creature in removeList)
        {
            creatures.Remove(creature);
        }

    }

    void UpdateByDay(int day)
    {

    }

    void GetEvnTransform(Transform trans)
    {
        evnTransform = trans;
        resourceTransform = trans.Find("_Resources");
        creatureTransform = trans.Find("_Creatures");

        terrain = trans.Find("_Terrain").GetComponent<Terrain>();

        GenerateResource();
        GenerateCreature();
    }

    [Button]
    private void GenerateResource()
    {
        // Water
        var posList = pointGenerator.GeneratePoints(2, 4);
        foreach(var pos in posList)
        {
            var waterTrans = SpawnObjectOnTerrain(pos, water, resourceTransform);
            resourceData.waterTransList.Add(waterTrans);
            
            var foodPosList = pointGenerator.GeneratePointsInRing(10, pos, 5, 30, 10, 10);
            foreach(var foodPos in foodPosList)
            {
                var foodTrans = SpawnObjectOnTerrain(foodPos, food, resourceTransform);
                resourceData.foodTransList.Add(foodTrans);
            }
        }
    }

    [Button]
    private void GenerateCreature()
    {
        var availableWater = resourceData.waterTransList;
        var dangerWater = CalculateUtils.GetRandom(availableWater);
        var tigerInitPos = pointGenerator.GeneratePointsInRing(1, dangerWater.position, 3, 20);
        var tigerTrans = SpawnObjectOnTerrain(tigerInitPos[0], tiger, creatureTransform);
        var tigerAI = tigerTrans.AddComponent<CreatureAI>();
        tigerAI.Init(new CreatureStat("3"));
        creatures.Add(tigerAI);

        availableWater.Remove(dangerWater);
        var safeWater = CalculateUtils.GetRandom(availableWater);
        var chickenInitPos = pointGenerator.GeneratePointsInRing(3, safeWater.position, 3, 20);
        foreach(var pos in chickenInitPos)
        {
            var chickenTrans = SpawnObjectOnTerrain(pos, chicken, creatureTransform);
            var chickenAI = chickenTrans.AddComponent<CreatureAI>();
            chickenAI.Init(new CreatureStat("1"));
            creatures.Add(chickenAI);
        }

    }

    public Transform SpawnObjectOnTerrain(Vector2 worldXZPosition, GameObject prefab, Transform objParent)
    { 
        // 获取该位置的地形高度
        float terrainHeight = terrain.SampleHeight(worldXZPosition);
        
        // 创建实例化位置（Y轴使用地形高度）
        Vector3 spawnPosition = new Vector3(
            worldXZPosition.x, 
            terrainHeight, 
            worldXZPosition.y
        );
        
        // 实例化物体
        GameObject newObject = Instantiate(prefab, spawnPosition, Quaternion.identity, objParent);
        
        // 可选：让物体朝向地形法线方向
        Vector3 terrainNormal = terrain.terrainData.GetInterpolatedNormal(
            (spawnPosition.x - terrain.transform.position.x) / terrain.terrainData.size.x,
            (spawnPosition.z - terrain.transform.position.z) / terrain.terrainData.size.z
        );
        
        newObject.transform.up = terrainNormal;

        return newObject.transform;
    }
}



#region Tools
public class PointGenerator
{
    private Vector2 areaMin = new Vector2(-128, -128);
    private Vector2 areaMax = new Vector2(128, 128);
    
    public List<Vector2> GeneratePoints(int minPoints, int maxPoints, float minDistanceBetweenPoints = 100, float minDistanceFromEdge = 50)
    {
        List<Vector2> points = new List<Vector2>();
        
        // 随机生成点位数量
        int pointCount = Random.Range(minPoints, maxPoints + 1);
        
        // 实际可用的区域（考虑边界距离）
        Vector2 usableMin = areaMin + new Vector2(minDistanceFromEdge, minDistanceFromEdge);
        Vector2 usableMax = areaMax - new Vector2(minDistanceFromEdge, minDistanceFromEdge);
        
        int maxAttempts = 1000; // 防止无限循环
        
        for (int i = 0; i < pointCount; i++)
        {
            Vector2 newPoint = Vector2.zero;
            bool validPoint = false;
            int attempts = 0;
            
            while (!validPoint && attempts < maxAttempts)
            {
                attempts++;
                
                // 生成随机点
                newPoint = new Vector2(
                    Random.Range(usableMin.x, usableMax.x),
                    Random.Range(usableMin.y, usableMax.y)
                );
                
                // 检查与其他点的距离
                validPoint = true;
                foreach (Vector2 existingPoint in points)
                {
                    if (Vector2.Distance(newPoint, existingPoint) < minDistanceBetweenPoints)
                    {
                        validPoint = false;
                        break;
                    }
                }
            }
            
            if (validPoint)
            {
                points.Add(newPoint);
            }
            else
            {
                Debug.LogWarning($"无法为第 {i + 1} 个点找到合适位置");
            }
        }
        
        return points;
    }

     /// <summary>
    /// 在圆环形区域内生成点位，距离中心越近密度越高
    /// </summary>
    /// <param name="pointCount">要生成的点位数量</param>
    /// <param name="center">中心点位置</param>
    /// <param name="minRadius">最小半径（不在此范围内生成）</param>
    /// <param name="maxRadius">最大半径</param>
    /// <param name="minDistanceBetweenPoints">点之间的最小距离</param>
    /// <param name="maxAttemptsPerPoint">每个点的最大尝试次数</param>
    /// <returns>生成的点位列表</returns>
    public List<Vector2> GeneratePointsInRing(
        int pointCount,
        Vector2 center,
        float minRadius,
        float maxRadius,
        float minDistanceBetweenPoints = 10f,
        int maxAttemptsPerPoint = 1000)
    {
        List<Vector2> points = new List<Vector2>();
        
        if (pointCount <= 0)
        {
            Debug.LogWarning("点位数量必须大于0");
            return points;
        }
        
        if (minRadius >= maxRadius)
        {
            Debug.LogWarning("最小半径必须小于最大半径");
            return points;
        }
        
        // 概率分布权重：距离中心越近权重越高
        AnimationCurve probabilityCurve = AnimationCurve.EaseInOut(0, 1, 1, 0.1f); // 可调整曲线
        
        for (int i = 0; i < pointCount; i++)
        {
            Vector2 newPoint = Vector2.zero;
            bool validPoint = false;
            int attempts = 0;
            
            while (!validPoint && attempts < maxAttemptsPerPoint)
            {
                attempts++;
                
                // 使用概率分布生成点
                newPoint = GeneratePointWithProbability(center, minRadius, maxRadius, probabilityCurve);
                
                // 检查与现有点的距离
                validPoint = IsPointValid(newPoint, points, minDistanceBetweenPoints);
            }
            
            if (validPoint)
            {
                points.Add(newPoint);
                // Debug.Log($"成功生成第 {points.Count} 个点: {newPoint}, 距离中心: {Vector2.Distance(center, newPoint)}");
            }
            else
            {
                Debug.LogWarning($"无法为第 {i + 1} 个点找到合适位置，已生成 {points.Count} 个点");
                break;
            }
        }
        
        return points;
    }
    
    /// <summary>
    /// 使用概率分布生成点（距离中心越近概率越高）
    /// </summary>
    private Vector2 GeneratePointWithProbability(Vector2 center, float minRadius, float maxRadius, AnimationCurve probabilityCurve)
    {
        // 方法1：基于半径的概率分布
        float randomValue = Random.value;
        float radius = Mathf.Lerp(minRadius, maxRadius, Mathf.Pow(randomValue, 2)); // 使用平方使点更靠近中心
        
        // 方法2：使用概率曲线（更精确的控制）
        // float normalizedRadius = GetRadiusFromProbabilityCurve(minRadius, maxRadius, probabilityCurve);
        // float radius = normalizedRadius;
        
        float angle = Random.Range(0, 2 * Mathf.PI);
        
        Vector2 point = center + new Vector2(
            Mathf.Cos(angle) * radius,
            Mathf.Sin(angle) * radius
        );
        
        return point;
    }
    
    /// <summary>
    /// 使用概率曲线生成半径（更精确的方法）
    /// </summary>
    private float GetRadiusFromProbabilityCurve(float minRadius, float maxRadius, AnimationCurve probabilityCurve)
    {
        // 使用拒绝采样方法根据概率曲线生成半径
        int maxAttempts = 100;
        
        for (int i = 0; i < maxAttempts; i++)
        {
            float randomRadius = Random.Range(minRadius, maxRadius);
            float normalizedRadius = (randomRadius - minRadius) / (maxRadius - minRadius);
            float probability = probabilityCurve.Evaluate(normalizedRadius);
            
            if (Random.value <= probability)
            {
                return randomRadius;
            }
        }
        
        // 如果拒绝采样失败，返回一个随机值
        return Random.Range(minRadius, maxRadius);
    }
    
    /// <summary>
    /// 检查点是否有效（与现有点的距离）
    /// </summary>
    private bool IsPointValid(Vector2 newPoint, List<Vector2> existingPoints, float minDistance)
    {
        foreach (Vector2 existingPoint in existingPoints)
        {
            if (Vector2.Distance(newPoint, existingPoint) < minDistance)
            {
                return false;
            }
        }
        return true;
    }
    
    /// <summary>
    /// 在圆环形区域内均匀生成点位（对比用）
    /// </summary>
    public List<Vector2> GeneratePointsInRingUniform(
        int pointCount,
        Vector2 center,
        float minRadius,
        float maxRadius,
        float minDistanceBetweenPoints = 10f)
    {
        List<Vector2> points = new List<Vector2>();
        
        for (int i = 0; i < pointCount; i++)
        {
            Vector2 newPoint = Vector2.zero;
            bool validPoint = false;
            int attempts = 0;
            
            while (!validPoint && attempts < 1000)
            {
                attempts++;
                
                // 均匀分布
                float radius = Random.Range(minRadius, maxRadius);
                float angle = Random.Range(0, 2 * Mathf.PI);
                
                newPoint = center + new Vector2(
                    Mathf.Cos(angle) * radius,
                    Mathf.Sin(angle) * radius
                );
                
                validPoint = IsPointValid(newPoint, points, minDistanceBetweenPoints);
            }
            
            if (validPoint)
            {
                points.Add(newPoint);
            }
        }
        
        return points;
    }
}
#endregion