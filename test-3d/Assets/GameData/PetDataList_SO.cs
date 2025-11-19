using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PetDataList_SO", menuName = "Data/Creature/Pet Data")]
public class PetDataList_SO : ScriptableObject
{
    public List<PetData> petDatas;
}
