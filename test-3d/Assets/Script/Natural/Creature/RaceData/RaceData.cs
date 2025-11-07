using UnityEngine;


public enum RaceType
{
    Canidae, //犬科
    Terrestores, //陆禽
    Felidae, // 猫科
}

[CreateAssetMenu(fileName = "New Rece", menuName = "Data/Creature/Rece Data")]
public class RaceData : ScriptableObject
{
    public int id;
    public string Name;
    public RaceType type;

// ---

    [Header("AI配置")]
    public float wanderRadius = 20f;
    public float detectionRange = 120f;
    public float attackRange = 2f;
    public float eatRange = 2f;
    public float drinkRange = 1.5f;

    [Header("战斗配置")]
    public float fleeDistance = 20f;        // 逃跑距离
    public float combatCheckInterval = 0.5f; // 战斗检测间隔
    public int attackDamage = 10;            // 攻击伤害
    
    [Header("需求阈值")]
    public int hungerThreshold;  // 低于此值开始寻找食物
    public int thirstThreshold;  // 低于此值开始寻找水源

// ---------------------------

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


