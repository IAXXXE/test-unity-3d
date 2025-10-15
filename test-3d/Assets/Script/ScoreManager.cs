using System;
using System.Collections.Generic;
using EasyButtons;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public Transform ObjectContainer;

    public Transform resultPanel;

    public Dropdown dropdown;

    public void SubmitObject()
    {
        UpdateScore();
        resultPanel.gameObject.SetActive(true);
    }

    public void UpdateScore()
    {
        var volumes = GetSlicedObjectsVolum();
        Transform results = resultPanel.Find("_Result");
        int idx = 0;
        foreach(Transform child in results)
        {
            if(idx >= 7) break;
            if(idx < volumes.Count)
            {
                child.GetComponent<TextMeshProUGUI>().text = "v" + (idx + 1).ToString() + " : " + volumes[idx];
                child.gameObject.SetActive(true);
            }
            else
            {
                child.gameObject.SetActive(false);
            }
            idx++;
        }

        int targetPieces = 0;
        switch(dropdown.value)
        {
            case 0:
                targetPieces = 2;
                break;
            case 1:
                targetPieces = 4;
                break;
            case 2:
                targetPieces = 7;
                break;
        }
        var result = new CuttingResult(ObjectContainer.childCount, volumes, 7.12f / targetPieces, targetPieces);
        var score = CalculateUtils.EvaluateCuttingScore(result);
        resultPanel.Find("_Title").GetComponent<TextMeshProUGUI>().text = "Score : " + score.totalScore + " ! ";

    }

    public void HideResultPanel()
    {
        resultPanel.gameObject.SetActive(false);
    }

    [Button]
    public List<float> GetSlicedObjectsVolum()
    {
        var volumes = new List<float>();
        foreach(Transform obj in ObjectContainer)
        {
            var volume = CalculateUtils.GetVolume(obj) * 10000;
            volumes.Add(volume);
        }
        return volumes;
    }

    public void ClearObjectContainer()
    {
        GameInstance.Instance.Utils.ClearChildren(ObjectContainer);
    }
}

public class CuttingResult
{
    public CuttingResult(int pieceCount, List<float> pieceVolumes, float targetVolume, int targetPieces)
    {   
        this.pieceCount = pieceCount;
        this.pieceVolumes = pieceVolumes;
        this.targetVolume = targetVolume;
        this.targetPieces = targetPieces;
    }   

    public int pieceCount;           // 切出的块数
    public List<float> pieceVolumes; // 每块的体积
    public float targetVolume;       // 目标总体积
    public int targetPieces;         // 目标块数
}

public class EvaluateScore
{
    public int totalScore;
    public int pieceCountScore;
    public int volumeUniformityScore;
    public int volumeUtilizationScore;

    public EvaluateScore(int totalScore, int pieceCountScore, int volumeUniformityScore, int volumeUtilizationScore)
    {
        this.totalScore = totalScore;
        this.pieceCountScore = pieceCountScore;
        this.volumeUniformityScore = volumeUniformityScore;
        this.volumeUtilizationScore = volumeUtilizationScore;
    }
}
