using System;
using UnityEngine;

public static class GameEventManager
{

/*** Events ***/

    #region Geme
    public static event Action OnGameStart;
    #endregion

    #region Time Events
    // 时间
    public static event Action OnSunrise;
    public static event Action OnSunset;
    public static event Action OnNoon;
    public static event Action OnMidnight;
    public static event Action<float> OnMinuteChanged;
    public static event Action<float> OnHourChanged;
    // 日期事件
    public static event Action<int> OnNewDay;
    public static event Action<int> OnNewYear;
    public static event Action<Season, Season> OnSeasonChanged;
    #endregion

    #region UI Events
    // UI
    public static event Action OnUIShowed;
    public static event Action OnUIHided;
    // Player
    public static event Action<int> OnPlayerHealthChanged;
    public static event Action<int> OnPlayerSatietyChanged;
    public static event Action<int> OnPlayerThirstyChanged;
    public static event Action<int> OnPlayerMagicChanged;

    #endregion

    #region Gameplay
    // 特殊界面
    public static event Action<CookLevel> OnCooked;
    // 行为
    public static event Action<ItemBase> OnItemHeld;
    public static event Action OnHeldItemConsumed;
    public static event Action OnItemUpdate;
    // Action
    public static event Action<ItemData, AttackType, float> OnWeaponAttack;
    public static event Action<ItemData> OnItemConsumed;
    public static event Action<bool> OnAimModeChanged;
    #endregion

/*** Triggers ***/

    #region Geme
    public static void TriggerGameStart()
        => OnGameStart?.Invoke();
    #endregion

    #region Time Event Triggers
    public static void TriggerSunrise()
        => OnSunrise?.Invoke(); 
    public static void TriggerSunset()
        => OnSunset?.Invoke(); 
    public static void TriggerNoon()
        => OnNoon?.Invoke(); 
    public static void TriggerMidnight()
        => OnMidnight?.Invoke();
    public static void TriggerMinuteChanged(float minute)
        => OnMinuteChanged?.Invoke(minute);
    public static void TriggerHourChanged(float hour)
        => OnHourChanged?.Invoke(hour);
    public static void TriggerSeasonChanged(Season oldSeason, Season newSeason)
        => OnSeasonChanged?.Invoke(oldSeason, newSeason);


    public static void TriggerNewDay(int day)
        => OnNewDay?.Invoke(day);
    public static void TriggerNewYear(int year)
        => OnNewYear?.Invoke(year);
    #endregion

    #region UI Event Triggers
    // UI Panel
    public static void TriggerUIShowed()
        => OnUIShowed?.Invoke();
    public static void TriggerUIHided()
        => OnUIHided?.Invoke();
    // Player
    public static void TriggerPlayerHealthChanged(int value)
        => OnPlayerHealthChanged?.Invoke(value);
    public static void TriggerPlayerSatietyChanged(int value)
        => OnPlayerSatietyChanged?.Invoke(value);
    public static void TriggerPlayerThirstyChanged(int value)
        => OnPlayerThirstyChanged?.Invoke(value);
    public static void TriggerPlayerMagicChanged(int value)
        => OnPlayerMagicChanged?.Invoke(value);

    #endregion

    #region Gameplay Event Triggers
    public static void TriggerCooked(CookLevel level) 
        => OnCooked?.Invoke(level);
    public static void TriggerItemHeld(ItemBase item)
        => OnItemHeld?.Invoke(item);

    // Action
    public static void TriggerHeldItemConsumed()
        => OnHeldItemConsumed?.Invoke();
    public static void TriggerItemUpdate()
        => OnItemUpdate?.Invoke();

    public static void TriggerWeaponAttack(ItemData item, AttackType type, float power)
        => OnWeaponAttack?.Invoke(item, type, power);

    public static void TriggerItemConsumed(ItemData item)
        => OnItemConsumed?.Invoke(item);

    public static void TriggerAimModeChanged(bool isAiming)
        => OnAimModeChanged?.Invoke(isAiming);
    
    #endregion
}
