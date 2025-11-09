using System.Collections.Generic;
using UnityEngine;

public class DishesData
{
    public CookType cookType;
    public int id;
    public string dishesName;
    public Sprite icon;

    public SerializableDictionary<int, int> ingredients;

    public SerializableDictionary<int, int> foodTargetHeat;

}

public enum CookType
{
    None,
    BakeStick,
    BakeSlate,
    Boiled,
    Refine,
}