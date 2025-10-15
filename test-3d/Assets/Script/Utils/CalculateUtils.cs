using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CalculateUtils
{
    #region 表面积计算
    public static float GetArea(this Transform obj, Action callbackError = null)
    {
        Mesh mesh = obj.GetComponent<MeshFilter>().mesh;
        if (mesh == null)
        {
            Debug.LogWarning("There is no 'MeshFilter' component!");
            callbackError?.Invoke();
            return -1;
        }

        Vector3[] vertices = mesh.vertices;
        Vector3 lossyScale = obj.lossyScale;

        float area = 0;
        for (int i = 0; i < mesh.subMeshCount; i++)
        {
            int[] triangles = mesh.GetTriangles(i);
            for (int j = 0; j < triangles.Length; j += 3)
            {
                area += CalculateTriangleArea(vertices[triangles[j]], vertices[triangles[j + 1]], vertices[triangles[j + 2]], lossyScale);
            }
        }

        return area;
    }

    private static float CalculateTriangleArea(Vector3 point1, Vector3 point2, Vector3 point3, Vector3 lossyScale)
    {
        //计算缩放
        point1 = new Vector3(point1.x * lossyScale.x, point1.y * lossyScale.y, point1.z * lossyScale.z);
        point2 = new Vector3(point2.x * lossyScale.x, point2.y * lossyScale.y, point2.z * lossyScale.z);
        point3 = new Vector3(point3.x * lossyScale.x, point3.y * lossyScale.y, point3.z * lossyScale.z);

        //计算边长
        float l1 = (point2 - point1).magnitude;
        float l2 = (point3 - point2).magnitude;
        float l3 = (point1 - point3).magnitude;
        float p = (l1 + l2 + l3) * 0.5f;

        //计算面积  S=√[p(p-l1)(p-l2)(p-l3)]（p为半周长）
        return Mathf.Sqrt(p * (p - l1) * (p - l2) * (p - l3));
    }
    #endregion
    
    #region 体积计算
    public static float GetVolume(this Transform obj, Action callbackError = null)
    {
        Mesh mesh = obj.GetComponent<MeshFilter>().mesh;
        if (mesh == null)
        {
            Debug.LogWarning("There is no 'MeshFilter' component!");
            callbackError?.Invoke();
            return -1;
        }

        Vector3[] vertices = mesh.vertices;
        Vector3 lossyScale = obj.lossyScale;
        Vector3 o = GetCenter(vertices);

        float volume = 0;
        for (int i = 0; i < mesh.subMeshCount; i++)
        {
            int[] triangles = mesh.GetTriangles(i);
            for (int j = 0; j < triangles.Length; j += 3)
            {
                volume += CalculateVolumeOfTriangle(vertices[triangles[j]], vertices[triangles[j + 1]], vertices[triangles[j + 2]], o, lossyScale);
            }
        }

        return Mathf.Abs(volume);
    }

    private static Vector3 GetCenter(Vector3[] points)
    {
        Vector3 center = Vector3.zero;
        for (int i = 0; i < points.Length; i++)
        {
            center += points[i];
        }
        center = center / points.Length;
        return center;
    }

    private static float CalculateVolumeOfTriangle(Vector3 point1, Vector3 point2, Vector3 point3, Vector3 center, Vector3 lossyScale)
    {
        //计算缩放
        point1 = new Vector3(point1.x * lossyScale.x, point1.y * lossyScale.y, point1.z * lossyScale.z);
        point2 = new Vector3(point2.x * lossyScale.x, point2.y * lossyScale.y, point2.z * lossyScale.z);
        point3 = new Vector3(point3.x * lossyScale.x, point3.y * lossyScale.y, point3.z * lossyScale.z);

        //向量
        Vector3 v1 = point1 - center;
        Vector3 v2 = point2 - center;
        Vector3 v3 = point3 - center;

        //计算体积
        //首先我们求以这三个向量为邻棱的平行六面体的面积
        //那就是（a×b）·c的绝对值
        //然后四面体的体积是平行六面体的六分之一
        //因为四面体的底是平行六面体的一半,而且要多乘一个三分之一
        float v = Vector3.Dot(Vector3.Cross(v1, v2), v3) / 6f;
        return v;
    }
    #endregion

    #region 切工计算
    /// <summary>
    /// 评估切割分数
    /// </summary>
    /// <param name="result">切割结果数据</param>
    /// <returns>0-100分的评分</returns>
    public static EvaluateScore EvaluateCuttingScore(CuttingResult result)
    {
        // 1. 块数匹配度 (30% 权重)
        float pieceCountScore = EvaluatePieceCountScore(result.pieceCount, result.targetPieces);
        Debug.Log("块数匹配度 : " + pieceCountScore);
        // 2. 体积均匀度 (40% 权重)  
        float volumeUniformityScore = EvaluateVolumeUniformityScore(result.pieceVolumes, result.targetVolume);
        Debug.Log("体积均匀度 : " + volumeUniformityScore);
        // 3. 体积利用率 (30% 权重)
        float volumeUtilizationScore = EvaluateVolumeUtilizationScore(result.pieceVolumes, result.targetVolume);
        Debug.Log("体积利用率 : " + volumeUtilizationScore);
        // 综合评分
        float totalScore = pieceCountScore * 0.3f + volumeUniformityScore * 0.4f + volumeUtilizationScore * 0.3f;

        var score = new EvaluateScore((int)totalScore, (int)pieceCountScore, (int)volumeUniformityScore, (int)volumeUniformityScore);
        return score;
    }
    
    /// <summary>
    /// 评估块数匹配度
    /// </summary>
    private static float EvaluatePieceCountScore(int actualCount, int targetCount)
    {
        if (actualCount == targetCount) return 100f;
        
        float difference = Mathf.Abs(actualCount - targetCount);
        float penalty = difference / targetCount * 100f; // 每多或少一块的惩罚
        
        return Mathf.Max(0f, 100f - penalty);
    }
    
    /// <summary>
    /// 评估体积均匀度
    /// </summary>
    private static float EvaluateVolumeUniformityScore(List<float> volumes, float targetVolume)
    {
        if (volumes.Count == 0) return 0f;
        float totalDeviation = 0f;
        float avgScore = 100 / volumes.Count;
        float score = 0;

        foreach (float volume in volumes)
        {
            float deviation = volume / targetVolume;
            deviation = Mathf.Abs(1 - deviation);
            if(deviation <= 0.15f)
            {
                score += avgScore;
            }
            else
            {
                score += avgScore * (1 - deviation);
            }
            Debug.Log($"volume {volume}, targetVolume : {targetVolume}, deviation {deviation}");
        }
        
        return score;
    }
    
    /// <summary>
    /// 评估体积利用率（避免切出太小碎块）
    /// </summary>
    private static float EvaluateVolumeUtilizationScore(List<float> volumes, float targetVolume)
    {
        if (volumes.Count == 0) return 0f;
        
        float minAcceptableVolume = targetVolume * 0.3f; // 最小可接受体积为理想值的30%
        float maxAcceptableVolume = targetVolume * 2f; // 最大可接受体积为理想值的200%
        int validPieces = 0;
        
        foreach (float volume in volumes)
        {
            if (volume >= minAcceptableVolume && volume <= maxAcceptableVolume)
            {
                validPieces++;
            }
        }
        
        return (float)validPieces / volumes.Count * 100f;
    }
    #endregion


}
