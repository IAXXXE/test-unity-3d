using UnityEngine;

[CreateAssetMenu(fileName = "New Character", menuName = "Character/Item Data")]
public class  CharacterData: ScriptableObject
{
    [Header("基础信息")]
    public string id;
    public string name;
    public RaceType raceType;
    public bool isCarnivore;
    [TextArea(3, 5)]
    public string description;
    
    [Header("显示设置")]
    public Sprite icon;
    public Sprite portrait;
    public GameObject worldPrefab;
    public GameObject primevalPrefab;

    [Header("基础数值")]
    public int maxHealth;
    public int maxSatiety;
    public int maxThirsty;
    public int maxMagic;

    [Header("基础能力值")]
    public int vitality;
    public int strength;
    public int dexterity;
    public int intelligence;
    public int mana;
    public float speed;

    [Header("技艺")]
    public string skill;
}


public enum RaceType
{
    Normal,

}