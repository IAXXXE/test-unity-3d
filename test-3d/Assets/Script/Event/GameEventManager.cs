using System;
using UnityEngine;

public static class GameEventManager
{
    public static event Action OnUIShowed;
    public static event Action OnUIHided;

    public static event Action OnSunrise;
    public static event Action OnSunset;
    // public static event Action OnNoon;
    // public static event Action OnMidnight;
    public static event Action<float> OnHourChanged;

    public static event Action<CookLevel> OnCooked;

    public static event Action<Item> OnItemHeld;
    public static event Action OnHeldItemConsumed;

    public static void TriggerUIShowed()
        => OnUIShowed?.Invoke();
    public static void TriggerUIHided()
        => OnUIHided?.Invoke();

    public static void TriggerSunrise()
        => OnSunrise?.Invoke(); 
    public static void TriggerSunset()
        => OnSunset?.Invoke(); 
    // public static void TriggerNoon()
    //     => OnNoon?.Invoke(); 
    // public static void TriggerMidnight()
    //     => OnMidnight?.Invoke(); 
    public static void TriggerHourChanged(float hour)
        => OnHourChanged?.Invoke(hour); 


    public static void TriggerCooked(CookLevel level) 
        => OnCooked?.Invoke(level);


    public static void TriggerItemHeld(Item item)
        => OnItemHeld?.Invoke(item);
    public static void TriggerHeldItemConsumed()
        => OnHeldItemConsumed?.Invoke();
}
