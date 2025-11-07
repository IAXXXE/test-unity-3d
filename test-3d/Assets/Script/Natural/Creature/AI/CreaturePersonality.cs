using UnityEngine;

// 生物性格配置
[System.Serializable]
public class CreaturePersonality
{
    [Header("性格类型")]
    public PersonalityType type = PersonalityType.Neutral;
    
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

    public CreaturePersonality(RaceType raceType)
    {
        var raceData = CharacterDatabase.Instance.GetRaceData(raceType);
        type = raceData.personalityType;
        aggressiveness = raceData.aggressiveness;
        courage = raceData.courage;
        angerThreshold = raceData.angerThreshold;
        fearThreshold = raceData.fearThreshold;
        angerDecayRate = raceData.angerDecayRate;
        fearDecayRate = raceData.fearDecayRate;
    }
}

public enum PersonalityType
{
    Passive,      // 被动型：总是逃跑
    Neutral,      // 中立型：根据情况决定
    Aggressive,   // 攻击型：总是反击
    Territorial   // 领地型：在领地内攻击，领地外逃跑
}

// 情绪状态
public enum EmotionState
{
    Calm,      // 平静
    Afraid,    // 害怕（逃跑）
    Angry      // 愤怒（攻击）
}
