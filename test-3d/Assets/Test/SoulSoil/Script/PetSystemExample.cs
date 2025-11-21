using System.Collections;
using System.Collections.Generic;
using EasyButtons;
using UnityEngine;


public class PetSystemExample : MonoBehaviour
{
    public PetManager petManager;
    public PetSummonSystem summonSystem;
    
    private void Update()
    {
        // 快捷键测试
        if (Input.GetKeyDown(KeyCode.P))
        {
            summonSystem.SummonPet("soso");
        }
        
        // 收回所有宠物
        if (Input.GetKeyDown(KeyCode.R))
        {
            RecallAllPets();
        }
    }
    
    [Button]
    private void SummonInitialPet()
    {
        // 召唤第一只宠物
        summonSystem.SummonPet("soso");
    }
    
    private void RecallAllPets()
    {
        foreach (var pet in petManager.ownedPets)
        {
            if (pet != null)
            {
                petManager.RecallPet(pet);
            }
        }
    }
}