using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterDatabase : MonoBehaviour
{
    public static CharacterDatabase Instance;

    [Header("角色数据列表")]
    public List<CharacterData> allCharacters = new List<CharacterData>();

    private Dictionary<string, CharacterData> characterDictionary = new Dictionary<string, CharacterData>();
    
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
    }

    public CharacterData GetCharacterData(string id)
    {
        if(characterDictionary.ContainsKey(id))
        {
            return characterDictionary[id];
        }

        return null;
    }

    public CharacterBase CreateCharacter(string id)
    {
        CharacterData data = GetCharacterData(id);
        if(data != null)
        {
            var character = new CharacterBase();
            character.LoadCharacter(data);
            return character;
        }

        return null;
    }


}

