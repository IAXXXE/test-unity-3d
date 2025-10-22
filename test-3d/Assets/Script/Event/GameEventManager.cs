using System;

public static class GameEventManager
{
    public static event Action OnUIShowed;
    public static event Action OnUIHided;
    public static event Action<CookLevel> OnCooked;

    public static void TriggerUIShowed()
        => OnUIShowed?.Invoke();
    public static void TriggerUIHided()
        => OnUIHided?.Invoke();
    public static void TriggerCooked(CookLevel level) 
        => OnCooked?.Invoke(level);
}
