using UnityEngine;


public enum RaceType
{
    Canidae,
    Terrestores, //陆禽
    Felidae,
}

[CreateAssetMenu(fileName = "New Rece", menuName = "Data/Creature/Rece Data")]
public class RaceData : ScriptableObject
{
    public int id;
    public string Name;
    public RaceType type;

    [Header("性格类型")]
    public PersonalityType personalityType;
    
    [Header("战斗倾向")]
    [Range(0f, 1f)]
    public float aggressiveness = 0.5f;  // 攻击性（0=胆小，1=好斗）
    
    [Range(0f, 1f)]
    public float courage = 0.5f;  // 勇气（影响是否逃跑）
    
    [Header("情绪阈值")]
    public float angerThreshold = 50f;   // 愤怒阈值
    public float fearThreshold = 30f;    // 恐惧阈值
    
    [Header("情绪衰减")]
    public float angerDecayRate = 5f;    // 愤怒值衰减速度/秒
    public float fearDecayRate = 10f;    // 恐惧值衰减速度/秒
}


