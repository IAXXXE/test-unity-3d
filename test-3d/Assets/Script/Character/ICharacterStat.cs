using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICharacterStat
{
    int GetHealth();
    int GetSatiety();
    int GetThirsty();
    int GetMaxHealth();
    int GetMaxSatiety();
    int GetMaxThirsty();
    void Heal(int value);
    void IncreaseMaxHealth(int value);
    void IncreaseMaxSatiety(int value);
    void IncreaseMaxThirsty(int value);
    void IncreaseSatiety(int value);
    void IncreaseThirsty(int value);
    void LoseHealth(int value);
    void LoseSatiety(int value);
    void LoseThirsty(int value);
    void SetHealth(int value);
    void SetSatiety(int value);
    void SetThirsty(int value);
}
