using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterDatabase : MonoBehaviour
{
    public static CharacterDatabase Instance;

    [Header("角色数据列表")]
    public List<CharacterData> allCharacters = new List<CharacterData>();
    private Dictionary<string, CharacterData> characterDictionary = new Dictionary<string, CharacterData>();

    // TODO: 暂放
    [Header("种族数据列表")]
    public List<RaceData> allRaceData = new List<RaceData>();
    private Dictionary<RaceType, RaceData> raceDictionary = new Dictionary<RaceType, RaceData>();

    [Header("幽灵数据列表")]
    public GhostDataList_SO ghostDataList_SO;
    private List<GhostData> allGhostData = new();
    private Dictionary<int, GhostData> ghostDictionary = new Dictionary<int, GhostData>();

    [Header("宠物数据列表")]
    public PetDataList_SO petDataList_SO;
    private List<PetData> allPetData = new();
    private Dictionary<string, PetData> petDictionary = new Dictionary<string, PetData>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDatabase();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeDatabase()
    {
        foreach(CharacterData characterData in allCharacters)
        {
            if(characterData != null && !string.IsNullOrEmpty(characterData.id))
            {
                if(!characterDictionary.ContainsKey(characterData.id))
                {
                    characterDictionary.Add(characterData.id, characterData);
                }
                else
                {
                    Debug.LogWarning($"重复的角色ID: {characterData.id}");
                }
            }

        }
        Debug.Log($"角色数据库初始化完成，共加载 {characterDictionary.Count} 个角色");

        foreach(RaceData raceData in allRaceData)
        {
            if(raceData != null)
            {
                if(!raceDictionary.ContainsKey(raceData.type))
                {
                    raceDictionary.Add(raceData.type, raceData);
                }
                else
                {
                    Debug.LogWarning($"重复的Race Type: {raceData.id}");
                }
            }

        }

        allGhostData = ghostDataList_SO.ghostDataList;
        foreach(var ghost in allGhostData)
        {
            if(ghost != null)
            {
                if(!ghostDictionary.ContainsKey(ghost.id))
                {
                    ghostDictionary.Add(ghost.id, ghost);
                }
                else
                {
                    Debug.LogWarning($"重复的Ghost ID: {ghost.id}");
                }
            }
        }

        allPetData = petDataList_SO.petDatas;
        foreach(var pet in allPetData)
        {
            if(pet != null)
            {
                if(!petDictionary.ContainsKey(pet.petName))
                {
                    petDictionary.Add(pet.petName, pet);
                }
                else
                {
                    Debug.LogWarning($"重复的Pet Name: {pet.petName}");
                }
            }
        }
    }

    public CharacterData GetCharacterData(string id)
    {
        if(characterDictionary.ContainsKey(id))
        {
            return characterDictionary[id];
        }

        return null;
    }

    public RaceData GetRaceData(RaceType type)
    {
        if(raceDictionary.ContainsKey(type))
        {
            return raceDictionary[type];
        }

        return null;
    }

    public GhostData GetGhostData(int id)
    {
        if(ghostDictionary.ContainsKey(id))
        {
            return ghostDictionary[id];
        }
        return null;
    }

    public PetData GetPetData(string name)
    {
        if(petDictionary.ContainsKey(name))
        {
            return petDictionary[name];
        }
        return null;
    }

}

