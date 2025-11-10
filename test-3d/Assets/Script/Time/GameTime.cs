// using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

#region Enums
public enum Season
{
    Spring,  // 春
    Summer,  // 夏
    Autumn,  // 秋
    Winter   // 冬
}

public enum TimeOfDayPeriod
{
    Night,      // 夜晚 (0:00-5:00)
    Dawn,       // 黎明 (5:00-6:00)
    Morning,    // 上午 (6:00-12:00)
    Noon,       // 正午 (12:00-13:00)
    Afternoon,  // 下午 (13:00-18:00)
    Dusk,       // 黄昏 (18:00-19:00)
    Evening     // 傍晚 (19:00-24:00)
}
#endregion

/// <summary>
/// 游戏时间管理系统
/// 统一管理游戏内的时间、日期、季节等
/// </summary>
public class GameTime : MonoBehaviour
{
    public static GameTime Instance { get; private set; }

    #region Date Settings
    [Header("Date Settings")]
    [Tooltip("当前天数（从1开始）")]
    public int currentDay = 1;
    
    [Tooltip("当前季节")]
    public Season currentSeason = Season.Spring;
    
    [Tooltip("每个季节的天数")]
    public int daysPerSeason = 30;
    
    [Tooltip("是否自动切换季节")]
    public bool autoSeasonChange = true;
    
    [Tooltip("年份（可选）")]
    public int currentYear = 1;
    #endregion

    #region Time Settings
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
    #endregion

    #region Light Settings
    [Header("Light Settings")]
    [Tooltip("日出时间（小时）")]
    public float sunriseTime = 6f;
    
    [Tooltip("日落时间（小时）")]
    public float sunsetTime = 18f;
    
    [Tooltip("季节对日照时长的影响")]
    public bool enableSeasonalDayLength = true;
    
    [Tooltip("夏季日照时长调整（小时）")]
    public float summerDaylightAdjustment = 2f;
    
    [Tooltip("冬季日照时长调整（小时）")]
    public float winterDaylightAdjustment = -2f;
    #endregion

    #region Display Settings
    [Header("Display Settings")]
    [Tooltip("显示调试信息")]
    public bool showDebugInfo = true;
    
    [Tooltip("使用自定义日期名称")]
    public bool useCustomDayNames = false;
    
    [Tooltip("自定义日期名称列表")]
    public List<string> customDayNames = new List<string>();
    #endregion

    #region Cached Values
    // Time
    private float timeOfDay; // 0-1 normalized time
    private bool isDay = true;
    private bool wasDaytime = true;
    private int lastHour = -1;
    private int lastMinute = -1;
    
    // Date
    private int lastDay = -1;
    private Season lastSeason;
    private float adjustedSunriseTime;
    private float adjustedSunsetTime;
    
    // Performance
    private float nextMinuteCheck = 0f;
    private const float MINUTE_CHECK_INTERVAL = 0.1f;
    #endregion

    #region Properties
    // Time Properties
    public float TimeOfDay => timeOfDay;
    public bool IsDay => isDay;
    public bool IsNight => !isDay;
    public float NormalizedTime => currentTime / 24f;
    public int CurrentHour => Mathf.FloorToInt(currentTime);
    public int CurrentMinute => Mathf.FloorToInt((currentTime - CurrentHour) * 60f);
    public int CurrentSecond => Mathf.FloorToInt(((currentTime - CurrentHour) * 60f - CurrentMinute) * 60f);
    
    // Date Properties
    public int CurrentDay => currentDay;
    public Season CurrentSeason => currentSeason;
    public int CurrentYear => currentYear;
    public int DayOfSeason => ((currentDay - 1) % daysPerSeason) + 1;
    public int DaysInCurrentSeason => daysPerSeason;
    public float SeasonProgress => (float)DayOfSeason / daysPerSeason;
    
    // Light Properties
    public float SunriseTime => adjustedSunriseTime;
    public float SunsetTime => adjustedSunsetTime;
    public float DaylightDuration => adjustedSunsetTime - adjustedSunriseTime;
    public float NightDuration => 24f - DaylightDuration;
    
    // Combined Properties
    public string FormattedTime => GetFormattedTime();
    public string FormattedDate => GetFormattedDate();
    public string FormattedDateTime => $"{FormattedDate} {FormattedTime}";
    public int TotalDaysPassed => currentDay - 1;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        lastSeason = currentSeason;
        UpdateSeasonalDaylight();
        UpdateCycle();
    }

    void Update()
    {
        if (!pauseTime)
        {
            UpdateTime();
        }
        
        UpdateCycle();
        CheckTimeEvents();
        CheckDateEvents();
    }
    #endregion

    #region Initialization
    void Initialize()
    {
        // 初始化自定义日期名称
        if (customDayNames.Count == 0)
        {
            customDayNames = new List<string>
            {
                "星期一", "星期二", "星期三", "星期四", 
                "星期五", "星期六", "星期日"
            };
        }
        
        lastDay = currentDay;
        adjustedSunriseTime = sunriseTime;
        adjustedSunsetTime = sunsetTime;
    }
    #endregion

    #region Time Update
    void UpdateTime()
    {
        // 更新时间
        float timeIncrement = (24f / dayDurationInSeconds) * Time.deltaTime * timeMultiplier;
        currentTime += timeIncrement;

        // 检查是否进入新的一天
        if (currentTime >= 24f)
        {
            currentTime -= 24f;
            AdvanceDay();
        }

        // 计算归一化时间 (0-1)
        timeOfDay = currentTime / 24f;
    }

    void AdvanceDay()
    {
        currentDay++;
        GameEventManager.TriggerNewDay(currentDay);
        
        // 检查季节变化
        if (autoSeasonChange)
        {
            CheckSeasonChange();
        }
        
        Debug.Log($"📅 新的一天！Day {currentDay} - {GetSeasonName(currentSeason)}");
    }

    void UpdateCycle()
    {
        // 更新昼夜状态
        isDay = currentTime >= adjustedSunriseTime && currentTime < adjustedSunsetTime;
    }
    #endregion

    #region Season Management
    void CheckSeasonChange()
    {
        int dayInYear = (currentDay - 1) % (daysPerSeason * 4);
        Season newSeason = (Season)(dayInYear / daysPerSeason);
        
        if (newSeason != currentSeason)
        {
            Season oldSeason = currentSeason;
            currentSeason = newSeason;
            UpdateSeasonalDaylight();
            GameEventManager.TriggerSeasonChanged(oldSeason, currentSeason);
            
            Debug.Log($"🍂 季节变化！{GetSeasonName(oldSeason)} → {GetSeasonName(currentSeason)}");
            
            // 检查是否进入新年
            if (newSeason == Season.Spring && oldSeason == Season.Winter)
            {
                currentYear++;
                GameEventManager.TriggerNewYear(currentYear);
                Debug.Log($"🎊 新年快乐！Year {currentYear}");
            }
        }
    }

    void UpdateSeasonalDaylight()
    {
        if (!enableSeasonalDayLength)
        {
            adjustedSunriseTime = sunriseTime;
            adjustedSunsetTime = sunsetTime;
            return;
        }
        
        float adjustment = GetSeasonalDaylightAdjustment();
        adjustedSunriseTime = sunriseTime - adjustment / 2f;
        adjustedSunsetTime = sunsetTime + adjustment / 2f;
        
        // 确保在合理范围内
        adjustedSunriseTime = Mathf.Clamp(adjustedSunriseTime, 4f, 8f);
        adjustedSunsetTime = Mathf.Clamp(adjustedSunsetTime, 16f, 20f);
    }

    float GetSeasonalDaylightAdjustment()
    {
        switch (currentSeason)
        {
            case Season.Summer:
                return summerDaylightAdjustment;
            case Season.Winter:
                return winterDaylightAdjustment;
            case Season.Spring:
            case Season.Autumn:
                return 0f;
            default:
                return 0f;
        }
    }

    public void SetSeason(Season season)
    {
        Season oldSeason = currentSeason;
        currentSeason = season;
        UpdateSeasonalDaylight();
        GameEventManager.TriggerSeasonChanged(oldSeason, currentSeason);
    }
    #endregion

    #region Event Checking
    void CheckTimeEvents()
    {
        // 检测昼夜切换
        if (isDay && !wasDaytime)
        {
            GameEventManager.TriggerSunrise();
            Debug.Log("🌅 日出！");
        }
        else if (!isDay && wasDaytime)
        {
            GameEventManager.TriggerSunset();
            Debug.Log("🌇 日落！");
        }
        wasDaytime = isDay;

        // 检测整点事件
        int currentHour = CurrentHour;
        if (currentHour != lastHour)
        {
            GameEventManager.TriggerHourChanged(currentHour);
            
            if (currentHour == 12)
            {
                GameEventManager.TriggerNoon();
                Debug.Log("☀️ 正午！");
            }
            else if (currentHour == 0)
            {
                GameEventManager.TriggerMidnight();
                Debug.Log("🌙 午夜！");
            }
            
            lastHour = currentHour;
        }
        
        // 每分钟检查（性能优化）
        if (Time.time >= nextMinuteCheck)
        {
            int currentMinute = CurrentMinute;
            if (currentMinute != lastMinute)
            {
                GameEventManager.TriggerMinuteChanged(currentMinute);
                lastMinute = currentMinute;
            }
            nextMinuteCheck = Time.time + MINUTE_CHECK_INTERVAL;
        }
    }

    void CheckDateEvents()
    {
        // 检测季节变化
        if (currentSeason != lastSeason)
        {
            lastSeason = currentSeason;
        }
    }
    #endregion

    #region Public Methods - Time Control
    public void SetTime(float hour)
    {
        currentTime = Mathf.Clamp(hour, 0f, 24f);
        UpdateCycle();
    }

    public void SetTimeOfDay(float normalizedTime)
    {
        currentTime = Mathf.Clamp01(normalizedTime) * 24f;
        UpdateCycle();
    }

    public void SkipToTime(float hour)
    {
        SetTime(hour);
    }

    public void AddTime(float hours)
    {
        currentTime += hours;
        while (currentTime >= 24f)
        {
            currentTime -= 24f;
            AdvanceDay();
        }
        UpdateCycle();
    }

    public void PauseTime(bool pause)
    {
        pauseTime = pause;
    }

    public void SetTimeMultiplier(float multiplier)
    {
        timeMultiplier = Mathf.Max(0f, multiplier);
    }
    #endregion

    #region Public Methods - Date Control
    public void SetDay(int day)
    {
        currentDay = Mathf.Max(1, day);
        if (autoSeasonChange)
        {
            CheckSeasonChange();
        }
    }

    public void AddDays(int days)
    {
        for (int i = 0; i < days; i++)
        {
            AdvanceDay();
        }
    }

    public void SetYear(int year)
    {
        currentYear = Mathf.Max(1, year);
    }
    #endregion

    #region Public Methods - Queries
    public TimeOfDayPeriod GetTimeOfDayPeriod()
    {
        if (currentTime >= 0f && currentTime < 5f)
            return TimeOfDayPeriod.Night;
        else if (currentTime >= 5f && currentTime < adjustedSunriseTime)
            return TimeOfDayPeriod.Dawn;
        else if (currentTime >= adjustedSunriseTime && currentTime < 12f)
            return TimeOfDayPeriod.Morning;
        else if (currentTime >= 12f && currentTime < 13f)
            return TimeOfDayPeriod.Noon;
        else if (currentTime >= 13f && currentTime < adjustedSunsetTime)
            return TimeOfDayPeriod.Afternoon;
        else if (currentTime >= adjustedSunsetTime && currentTime < adjustedSunsetTime + 1f)
            return TimeOfDayPeriod.Dusk;
        else
            return TimeOfDayPeriod.Evening;
    }

    public string GetTimeOfDayPeriodName()
    {
        switch (GetTimeOfDayPeriod())
        {
            case TimeOfDayPeriod.Night: return "夜晚";
            case TimeOfDayPeriod.Dawn: return "黎明";
            case TimeOfDayPeriod.Morning: return "上午";
            case TimeOfDayPeriod.Noon: return "正午";
            case TimeOfDayPeriod.Afternoon: return "下午";
            case TimeOfDayPeriod.Dusk: return "黄昏";
            case TimeOfDayPeriod.Evening: return "傍晚";
            default: return "未知";
        }
    }

    public bool IsTimeBetween(float startHour, float endHour)
    {
        if (startHour > endHour)
        {
            // 跨午夜的情况 (例如 22:00 到 6:00)
            return currentTime >= startHour || currentTime <= endHour;
        }
        else
        {
            return currentTime >= startHour && currentTime <= endHour;
        }
    }
    #endregion

    #region Formatting
    public string GetFormattedTime(bool use24Hour = true, bool showSeconds = false)
    {
        int hours = CurrentHour;
        int minutes = CurrentMinute;
        int seconds = CurrentSecond;

        if (use24Hour)
        {
            if (showSeconds)
                return $"{hours:00}:{minutes:00}:{seconds:00}";
            else
                return $"{hours:00}:{minutes:00}";
        }
        else
        {
            // 12小时制
            int displayHours = hours % 12;
            if (displayHours == 0) displayHours = 12;
            string period = hours >= 12 ? "PM" : "AM";

            if (showSeconds)
                return $"{displayHours}:{minutes:00}:{seconds:00} {period}";
            else
                return $"{displayHours}:{minutes:00} {period}";
        }
    }

    public string GetFormattedDate(bool includeYear = true, bool includeWeekday = false)
    {
        string result = "";
        
        if (includeYear)
        {
            result += $"Year {currentYear}, ";
        }
        
        result += $"{GetSeasonName(currentSeason)} Day {DayOfSeason}";
        
        if (includeWeekday && useCustomDayNames && customDayNames.Count > 0)
        {
            int weekdayIndex = (currentDay - 1) % customDayNames.Count;
            result += $" ({customDayNames[weekdayIndex]})";
        }
        
        return result;
    }

    public string GetSeasonName(Season season)
    {
        switch (season)
        {
            case Season.Spring: return "春季 Spring";
            case Season.Summer: return "夏季 Summer";
            case Season.Autumn: return "秋季 Autumn";
            case Season.Winter: return "冬季 Winter";
            default: return "Unknown";
        }
    }
    #endregion

    #region Save/Load Support
    [Serializable]
    public class GameTimeData
    {
        public float currentTime;
        public int currentDay;
        public int currentYear;
        public Season currentSeason;
        public float timeMultiplier;
    }

    public GameTimeData GetSaveData()
    {
        return new GameTimeData
        {
            currentTime = this.currentTime,
            currentDay = this.currentDay,
            currentYear = this.currentYear,
            currentSeason = this.currentSeason,
            timeMultiplier = this.timeMultiplier
        };
    }

    public void LoadSaveData(GameTimeData data)
    {
        currentTime = data.currentTime;
        currentDay = data.currentDay;
        currentYear = data.currentYear;
        currentSeason = data.currentSeason;
        timeMultiplier = data.timeMultiplier;
        
        UpdateSeasonalDaylight();
        UpdateCycle();
    }
    #endregion

    #region Debug GUI
    void OnGUI()
    {
        if (!showDebugInfo) return;

        GUILayout.BeginArea(new Rect(10, Screen.height - 200, 350, 190));
        GUILayout.Box("=== Game Time System ===");
        
        GUILayout.Label($"时间: {FormattedTime}");
        GUILayout.Label($"日期: {FormattedDate}");
        GUILayout.Label($"时段: {GetTimeOfDayPeriodName()} {(IsDay ? "☀️" : "🌙")}");
        GUILayout.Label($"日出/日落: {adjustedSunriseTime:F1} / {adjustedSunsetTime:F1}");
        GUILayout.Label($"速度: x{timeMultiplier:F1} {(pauseTime ? "[暂停]" : "")}");

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("黎明")) SkipToTime(6f);
        if (GUILayout.Button("正午")) SkipToTime(12f);
        if (GUILayout.Button("黄昏")) SkipToTime(18f);
        if (GUILayout.Button("午夜")) SkipToTime(0f);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("+1天")) AddDays(1);
        if (GUILayout.Button("+7天")) AddDays(7);
        if (GUILayout.Button("暂停")) PauseTime(!pauseTime);
        GUILayout.EndHorizontal();

        GUILayout.EndArea();
    }
    #endregion

    #region Editor Validation
    void OnValidate()
    {
        if (Application.isPlaying && gameObject.activeSelf)
        {
            UpdateSeasonalDaylight();
            UpdateCycle();
        }
    }
    #endregion
}