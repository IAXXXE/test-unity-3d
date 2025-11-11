using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICharacterStat
{
    int GetHealth();
    int GetMaxHealth();
    void Heal(int value);
    void IncreaseMaxHealth(int value);
    void LoseHealth(int value);
    void SetHealth(int value);
}
