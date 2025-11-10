using UnityEngine;


[CreateAssetMenu(fileName = "BattleData_SO", menuName = "Data/Battle Data")]
public class GhostBattleData : ScriptableObject
{
    public BattlefieldType battlefieldType;
    public float battleRadius = 20f;
    public float totalBattleTime = 113f;
    public float spawnInterval = 20f;
    public int enemiesPerWave = 5;
    public float spawnRadius = 3f;
}