using System;
using UnityEngine;

public static class GameEventManager
{
    public static event Action OnUIShowed;
    public static event Action OnUIHided;

    public static event Action<CookLevel> OnCooked;

    public static event Action<ItemData> OnItemHeld;

    public static void TriggerUIShowed()
        => OnUIShowed?.Invoke();
    public static void TriggerUIHided()
        => OnUIHided?.Invoke();


    public static void TriggerCooked(CookLevel level) 
        => OnCooked?.Invoke(level);


    public static void TriggerItemHeld(ItemData itemData)
        => OnItemHeld?.Invoke(itemData);
}
