using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GhostDataList_SO", menuName = "Data/Creature/Ghost Data")]
public class GhostDataList_SO : ScriptableObject
{
    public List<GhostData> ghostDataList;
}
