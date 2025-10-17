using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Cook/Cook Data")]
public class CookData : ScriptableObject
{
    public int id;
    public CookType cookType;

    public Dictionary<string, int> ingredients;

    public Dictionary<string, int> foodTargetHeat;

}

public enum CookType
{
    None,
    BakeStick,
    BakeSlate,
    Boiled,
}