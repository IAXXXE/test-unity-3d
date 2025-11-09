using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DishesDatabase : MonoBehaviour
{
    public static DishesDatabase Instance;

    [Header("菜品数据列表")]
    public List<DishesData> allDishes = new List<DishesData>();

    private Dictionary<int, DishesData> dishesDictionary = new Dictionary<int, DishesData>();
    
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
        foreach(DishesData dishesData in allDishes)
        {
            if(dishesData != null)
            {
                if(!dishesDictionary.ContainsKey(dishesData.id))
                {
                    dishesDictionary.Add(dishesData.id, dishesData);
                }
                else
                {
                    Debug.LogWarning($"重复的菜品ID: {dishesData.id}");
                }
            }

        }
        Debug.Log($"菜品数据库初始化完成，共加载 {dishesDictionary.Count} 个");
    }

    public DishesData GetCharacterData(int id)
    {
        if(dishesDictionary.ContainsKey(id))
        {
            return dishesDictionary[id];
        }

        return null;
    }

    // public CharacterBase CreateCharacter(string id)
    // {
    //     DishesData data = GetCharacterData(id);
    //     if(data != null)
    //     {
    
    //     }

    //     return null;
    // }

}
