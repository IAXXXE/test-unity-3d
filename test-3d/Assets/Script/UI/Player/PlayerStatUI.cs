using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatUI : MonoBehaviour
{
    private PlayerStat playerStat;

    public TextMeshProUGUI healthText;
    public TextMeshProUGUI maxHealthText;
    public TextMeshProUGUI satietyText;
    public TextMeshProUGUI maxSatietyText;
    public TextMeshProUGUI thirstyText;
    public TextMeshProUGUI maxThirstyText;
    public TextMeshProUGUI magicText;
    public TextMeshProUGUI maxMagicText;

    void Awake()
    {
        GameEventManager.OnGameStart += OnGameStart;

        GameEventManager.OnHourChanged += OnHourChanged;

        GameEventManager.OnPlayerHealthChanged += OnPlayerHealthChanged; 
        GameEventManager.OnPlayerMagicChanged += OnPlayerMagicChanged;
        GameEventManager.OnPlayerSatietyChanged += OnPlayerSatietyChanged;
        GameEventManager.OnPlayerThirstyChanged += OnPlayerThirstyChanged;
    }

    void Destroy()
    {
        GameEventManager.OnGameStart -= OnGameStart;

        GameEventManager.OnHourChanged -= OnHourChanged;

        GameEventManager.OnPlayerHealthChanged -= OnPlayerHealthChanged; 
        GameEventManager.OnPlayerMagicChanged -= OnPlayerMagicChanged;
        GameEventManager.OnPlayerSatietyChanged -= OnPlayerSatietyChanged;
        GameEventManager.OnPlayerThirstyChanged -= OnPlayerThirstyChanged;   
    }

    void OnGameStart()
    {
        playerStat = GameInstance.Instance.PlayerStat;

        maxHealthText.text = playerStat.GetMaxHealth().ToString();
        maxSatietyText.text = playerStat.GetMaxSatiety().ToString();
        maxThirstyText.text = playerStat.GetMaxThirsty().ToString();
        maxMagicText.text = playerStat.GetMaxMagic().ToString();

        healthText.text = playerStat.GetHealth().ToString();
        satietyText.text = playerStat.GetSatiety().ToString();
        thirstyText.text = playerStat.GetThirsty().ToString();
        magicText.text = playerStat.GetMagic().ToString();

        Debug.Log($"max health {playerStat.GetMaxHealth()}  health {playerStat.GetHealth()}");
    }

    void OnPlayerHealthChanged(int value)
    {
        healthText.text = value.ToString();
    }
    void OnPlayerMagicChanged(int value)
    {
        magicText.text = value.ToString();
    }
    void OnPlayerSatietyChanged(int value)
    {
        satietyText.text = value.ToString();
    }
    void OnPlayerThirstyChanged(int value)
    {
        thirstyText.text = value.ToString();
    }

    void OnHourChanged(float hour)
    {
        playerStat ??= GameInstance.Instance.PlayerStat;
        if(playerStat == null) return;

        playerStat.LoseSatiety(1);
        playerStat.LoseThirsty(1);
    }
}
