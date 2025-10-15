using UnityEngine;

[CreateAssetMenu(fileName = "New Character", menuName = "Character/Item Data")]
public class  CharacterData: ScriptableObject
{
    [Header("基础信息")]
    public string id;
    public string name;
    public RaceType raceType;
    [TextArea(3, 5)]
    public string description;
    
    [Header("显示设置")]
    public Sprite icon;
    public Sprite portrait;
    public GameObject worldPrefab;
    
    [Header("可操控设置")]
    public bool isUsable = true;
    
    [Header("基础数值")]
    public float health = 0f;
    
    [Header("非直接影响数值")]
    public float strength = 0f;

    [Header("技艺")]
    public float skill = 0f;
    
}


public enum RaceType
{
    Normal,

}