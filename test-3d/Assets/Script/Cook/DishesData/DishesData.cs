using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dishes", menuName = "Data/Cook/Dishes Data")]
public class DishesData : ScriptableObject
{
    public CookType cookType;
    public string id;
    public Sprite icon;

    public SerializableDictionary<string, int> ingredients;

    public SerializableDictionary<string, int> foodTargetHeat;

}

public enum CookType
{
    None,
    BakeStick,
    BakeSlate,
    Boiled,
    Refine,
}