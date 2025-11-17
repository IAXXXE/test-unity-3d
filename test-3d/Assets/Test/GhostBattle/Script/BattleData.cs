using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "BattleData_SO", menuName = "Data/Battle Data")]
public class GhostBattleData : ScriptableObject
{
    public BattlefieldType battlefieldType;
    public float battleRadius = 20f;
    public float totalBattleTime = 233f;
    public float spawnInterval = 20f;
    public int enemiesPerWave = 5;
    public float spawnRadius = 3f;

    public List<int> ghostIdList = new List<int>();
    public List<WaveConfig> waveConfigs = new List<WaveConfig>();

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