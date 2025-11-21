using System;
using Unity.VisualScripting;
using UnityEngine;

public static class GameEventManager
{

/*** Events ***/

    #region 1-Geme
    public static event Action OnGameStart;
    #endregion

    #region 2-Time Events
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

    #region 3-Map & Natural
    public static event Action<Transform> OnTerrainGenerated;
    #endregion

    #region 4-UI Events
    // UI
    public static event Action OnUIShowed;
    public static event Action OnUIHided;
    // 玩家状态
    public static event Action<int> OnPlayerHealthChanged;
    public static event Action<int> OnPlayerSatietyChanged;
    public static event Action<int> OnPlayerThirstyChanged;
    public static event Action<int> OnPlayerMagicChanged;

    #endregion

    #region 5-Gameplay
    // 特殊界面
    public static event Action<CookLevel> OnCooked;
    public static event Action<PetBase, InteractionOption[]> OnPetInteraction;
    // 玩家行为
    public static event Action<ItemBase, HandType> OnItemHeld;
    public static event Action<HandType> OnHeldItemConsumed;
    public static event Action OnItemUpdate;
    public static event Action<Transform> OnPlayerLookAt;
    public static event Action<bool> OnPlayerMerge;
    public static event Action<bool> OnPlayerFeed;
    // 战斗
    public static event Action<ItemData, AttackType, float> OnWeaponAttack;
    public static event Action OnLightAttackHit;
    public static event Action<ItemData> OnItemConsumed;
    public static event Action<bool> OnAimModeChanged;

    // 灵
    public static event Action<Transform> OnInspirationPointInteract;
    public static event Action OnBattleStart;
    public static event Action<bool> OnBattleEnd;
    #endregion


/*** Triggers ***/

    #region 1-Geme
    public static void TriggerGameStart()
        => OnGameStart?.Invoke();
    #endregion

    #region 2-Time Event Triggers
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

    #region 3-Environment & Natural
    // 地形
    public static void TriggerTerrainGenerated(Transform trans)
        => OnTerrainGenerated?.Invoke(trans);
    #endregion

    #region 4-UI Events Triggers
    // UI界面
    public static void TriggerUIShowed()
        => OnUIShowed?.Invoke();
    public static void TriggerUIHided()
        => OnUIHided?.Invoke();

    // 玩家状态
    public static void TriggerPlayerHealthChanged(int value)
        => OnPlayerHealthChanged?.Invoke(value);
    public static void TriggerPlayerSatietyChanged(int value)
        => OnPlayerSatietyChanged?.Invoke(value);
    public static void TriggerPlayerThirstyChanged(int value)
        => OnPlayerThirstyChanged?.Invoke(value);
    public static void TriggerPlayerMagicChanged(int value)
        => OnPlayerMagicChanged?.Invoke(value);
    #endregion

    #region 5-Gameplay Event Triggers
    public static void TriggerCooked(CookLevel level) 
        => OnCooked?.Invoke(level);
    public static void TriggerPetInteraction(PetBase pet, InteractionOption[] options)
        => OnPetInteraction?.Invoke(pet, options);

    // 玩家行为
    public static void TriggerItemHeld(ItemBase item, HandType type = HandType.HandR)
        => OnItemHeld?.Invoke(item, type);
    public static void TriggerHeldItemConsumed(HandType type = HandType.HandR)
        => OnHeldItemConsumed?.Invoke(type);
    public static void TriggerItemUpdate()
        => OnItemUpdate?.Invoke();
    public static void TriggerPlayerLookAt(Transform target)
        => OnPlayerLookAt?.Invoke(target);
    public static void TriggerPlayerMerge(bool isTrue)
        => OnPlayerMerge?.Invoke(isTrue);
    public static void TriggerPlayerFeed(bool isTrue)
        => OnPlayerFeed?.Invoke(isTrue);
    // 玩家战斗
    public static void TriggerWeaponAttack(ItemData item, AttackType type, float power)
        => OnWeaponAttack?.Invoke(item, type, power);
    public static void TriggerLightAttackHit()
        => OnLightAttackHit?.Invoke();

    public static void TriggerItemConsumed(ItemData item)
        => OnItemConsumed?.Invoke(item);
    public static void TriggerAimModeChanged(bool isAiming)
        => OnAimModeChanged?.Invoke(isAiming);

    // 灵
    public static void TriggerInspirationPointInteract(Transform point)
        => OnInspirationPointInteract?.Invoke(point);
    public static void TriggerBattleStart()
        => OnBattleStart?.Invoke();
    public static void TriggerBattleEnd(bool isWin)
        => OnBattleEnd?.Invoke(isWin);

    #endregion
}
