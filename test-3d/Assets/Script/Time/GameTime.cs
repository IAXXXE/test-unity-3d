using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTime : MonoBehaviour
{
    public static GameTime Instance;

    [Header("Time Settings")]
    [Tooltip("一个完整昼夜循环的真实秒数")]
    public float dayDurationInSeconds = 120f;
    
    [Tooltip("当前时间（0-24小时）")]
    [Range(0f, 24f)]
    public float currentTime = 12f;
    
    [Tooltip("时间流逝速度倍率")]
    public float timeMultiplier = 1f;
    
    [Tooltip("是否暂停时间")]
    public bool pauseTime = false;

    [Header("Light Settings")]
    [Tooltip("日出时间（小时）")]
    public float sunriseTime = 6f;
    
    [Tooltip("日落时间（小时）")]
    public float sunsetTime = 18f;

    [Header("Events")]
    [Tooltip("显示调试信息")]
    public bool showDebugInfo = true;

    // Cached values
    private float timeOfDay; // 0-1 normalized time
    private bool isDay = true;
    private bool wasDaytime = true;
    private int lastHour = -1;

    // Properties
    public float TimeOfDay => timeOfDay;
    public bool IsDay => isDay;
    public bool IsNight => !isDay;
    public float SunriseTime => sunriseTime;
    public float SunsetTime => sunsetTime;
    public float NormalizedTime => currentTime / 24f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (!pauseTime)
        {
            UpdateTime();
        }
        UpdateCycle();
        CheckTimeEvents();
    }

    void UpdateTime()
    {
        // 更新时间
        float timeIncrement = (24f / dayDurationInSeconds) * Time.deltaTime * timeMultiplier;
        currentTime += timeIncrement;

        // 循环回到0
        if (currentTime >= 24f)
        {
            currentTime = 0f;
        }

        // 计算归一化时间 (0-1)
        timeOfDay = currentTime / 24f;
    }

    void UpdateCycle()
    {
        // 更新昼夜状态
        isDay = currentTime >= sunriseTime && currentTime < sunsetTime;
    }


    void CheckTimeEvents()
    {
        // 检测昼夜切换
        if (isDay && !wasDaytime)
        {
            GameEventManager.TriggerSunrise();
        }
        else if (!isDay && wasDaytime)
        {
            GameEventManager.TriggerSunset();
        }

        wasDaytime = isDay;

        // 检测整点事件
        int currentHour = Mathf.FloorToInt(currentTime);
        if (currentHour != lastHour)
        {
            GameEventManager.TriggerHourChanged(currentHour);
            // if (currentHour == 12)
            // {
            //     OnNoon?.Invoke();
            //     Debug.Log("☀️ 正午！");
            // }
            // else if (currentHour == 0)
            // {
            //     OnMidnight?.Invoke();
            //     Debug.Log("🌙 午夜！");
            // }
            lastHour = currentHour;
        }
    }

    public void PauseTime(bool pause)
    {
        pauseTime = pause;
    }

    // Public methods
    public void SetTime(float hour)
    {
        currentTime = Mathf.Clamp(hour, 0f, 24f);
    }

    public void SetTimeOfDay(float normalizedTime)
    {
        currentTime = Mathf.Clamp01(normalizedTime) * 24f;
    }

    public void SkipToTime(float hour)
    {
        SetTime(hour);
        UpdateCycle();
    }

    public string GetFormattedTime()
    {
        int hours = Mathf.FloorToInt(currentTime);
        int minutes = Mathf.FloorToInt((currentTime - hours) * 60f);
        return $"{hours:00}:{minutes:00}";
    }

    void OnGUI()
    {
        if (!showDebugInfo) return;

        GUILayout.BeginArea(new Rect(10, Screen.height - 150, 300, 140));
        GUILayout.Box("=== Day/Night Cycle ===");
        GUILayout.Label($"Time: {GetFormattedTime()}");
        GUILayout.Label($"Time of Day: {(IsDay ? "Day ☀️" : "Night 🌙")}");

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Dawn (6:00)")) SkipToTime(6f);
        if (GUILayout.Button("Noon (12:00)")) SkipToTime(12f);
        if (GUILayout.Button("Dusk (18:00)")) SkipToTime(18f);
        if (GUILayout.Button("Midnight (0:00)")) SkipToTime(0f);
        GUILayout.EndHorizontal();

        GUILayout.EndArea();
    }

    void OnValidate()
    {
        // 编辑器中实时预览
        if (Application.isPlaying && gameObject.activeSelf)
        {
            UpdateCycle();
        }
    }

}
