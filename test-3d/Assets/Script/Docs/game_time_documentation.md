# 🕐 GameTime 系统完整文档

## 📖 概述

GameTime 是一个完整的游戏时间管理系统，提供：
- ⏰ 精确的时间流逝控制
- 📅 完整的日期和季节系统
- 🌅 昼夜循环管理
- 📡 丰富的事件系统
- 💾 保存/加载支持

---

## 🚀 快速开始

### 1. 基础设置

```
1. 创建空物体，命名 "GameTime"
2. 添加 GameTime 脚本
3. 按 Play，完成！
```

### 2. 访问实例

```csharp
// 在任何脚本中访问
GameTime.Instance.currentTime;  // 当前时间
GameTime.Instance.CurrentDay;   // 当前天数
GameTime.Instance.CurrentSeason; // 当前季节
```

---

## 🎯 核心功能

### 1. 时间管理

#### 获取时间信息

```csharp
// 基础时间
float currentTime = GameTime.Instance.currentTime;      // 0-24 小时
float timeOfDay = GameTime.Instance.TimeOfDay;          // 0-1 归一化
int hour = GameTime.Instance.CurrentHour;               // 当前小时
int minute = GameTime.Instance.CurrentMinute;           // 当前分钟
int second = GameTime.Instance.CurrentSecond;           // 当前秒

// 格式化时间
string time24 = GameTime.Instance.GetFormattedTime();           // "14:30"
string time12 = GameTime.Instance.GetFormattedTime(false);      // "2:30 PM"
string timeWithSec = GameTime.Instance.GetFormattedTime(true, true); // "14:30:45"
```

#### 控制时间

```csharp
// 设置时间
GameTime.Instance.SetTime(12f);              // 设置为12点
GameTime.Instance.SkipToTime(18f);           // 跳到18点
GameTime.Instance.AddTime(2f);               // 增加2小时

// 时间流速
GameTime.Instance.SetTimeMultiplier(2f);     // 2倍速
GameTime.Instance.SetTimeMultiplier(0.5f);   // 0.5倍速
GameTime.Instance.PauseTime(true);           // 暂停
GameTime.Instance.PauseTime(false);          // 恢复
```

#### 时间查询

```csharp
// 检查是否是白天
bool isDay = GameTime.Instance.IsDay;
bool isNight = GameTime.Instance.IsNight;

// 检查时间段
bool isInTimeRange = GameTime.Instance.IsTimeBetween(8f, 20f);

// 获取时段
GameTime.TimeOfDayPeriod period = GameTime.Instance.GetTimeOfDayPeriod();
string periodName = GameTime.Instance.GetTimeOfDayPeriodName();

// 时段枚举值：
// - Night (夜晚 0:00-5:00)
// - Dawn (黎明 5:00-日出)
// - Morning (上午 日出-12:00)
// - Noon (正午 12:00-13:00)
// - Afternoon (下午 13:00-日落)
// - Dusk (黄昏 日落-日落+1小时)
// - Evening (傍晚 黄昏后-24:00)
```

---

### 2. 日期管理

#### 获取日期信息

```csharp
// 基础日期
int day = GameTime.Instance.CurrentDay;              // 当前天数
int year = GameTime.Instance.CurrentYear;            // 当前年份
GameTime.Season season = GameTime.Instance.CurrentSeason; // 当前季节

// 季节相关
int dayOfSeason = GameTime.Instance.DayOfSeason;     // 季节中的第几天
float progress = GameTime.Instance.SeasonProgress;   // 季节进度 (0-1)
int totalDays = GameTime.Instance.TotalDaysPassed;   // 总共过了多少天

// 格式化日期
string date = GameTime.Instance.GetFormattedDate();              // "Year 1, 春季 Spring Day 5"
string dateSimple = GameTime.Instance.GetFormattedDate(false);   // "春季 Spring Day 5"
string dateWithWeek = GameTime.Instance.GetFormattedDate(true, true); // 包含星期
```

#### 控制日期

```csharp
// 设置日期
GameTime.Instance.SetDay(10);                // 设置为第10天
GameTime.Instance.AddDays(1);                // 增加1天
GameTime.Instance.AddDays(7);                // 增加7天
GameTime.Instance.SetYear(2);                // 设置年份

// 季节控制
GameTime.Instance.SetSeason(GameTime.Season.Winter);  // 切换到冬季
```

---

### 3. 季节系统

#### 季节类型

```csharp
public enum Season
{
    Spring,  // 春季
    Summer,  // 夏季
    Autumn,  // 秋季
    Winter   // 冬季
}
```

#### 季节功能

```csharp
// 获取季节名称
string seasonName = GameTime.Instance.GetSeasonName(Season.Spring); // "春季 Spring"

// 季节自动切换
GameTime.Instance.autoSeasonChange = true;   // 启用自动切换
GameTime.Instance.daysPerSeason = 30;        // 每季节30天

// 季节影响日照
GameTime.Instance.enableSeasonalDayLength = true;
GameTime.Instance.summerDaylightAdjustment = 2f;    // 夏季日照+2小时
GameTime.Instance.winterDaylightAdjustment = -2f;   // 冬季日照-2小时
```

#### 日照时间

```csharp
// 获取调整后的日出日落时间
float sunrise = GameTime.Instance.SunriseTime;       // 实际日出时间
float sunset = GameTime.Instance.SunsetTime;         // 实际日落时间
float daylight = GameTime.Instance.DaylightDuration; // 日照时长
float night = GameTime.Instance.NightDuration;       // 夜晚时长

// 示例：夏季
// 基础日出: 6:00, 基础日落: 18:00
// 调整后:   5:00 (早1小时),  19:00 (晚1小时)
```

---

### 4. 事件系统

#### 订阅事件

```csharp
void OnEnable()
{
    // 时间事件
    GameEventManager.OnSunrise += HandleSunrise;
    GameEventManager.OnSunset += HandleSunset;
    GameEventManager.OnNoon += HandleNoon;
    GameEventManager.OnMidnight += HandleMidnight;
    GameEventManager.OnHourChanged += HandleHourChanged;
    GameEventManager.OnMinuteChanged += HandleMinuteChanged;
    
    // 日期事件
    GameEventManager.OnNewDay += HandleNewDay;
    GameEventManager.OnNewYear += HandleNewYear;
    GameEventManager.OnSeasonChanged += HandleSeasonChanged;
}

void OnDisable()
{
    // 记得取消订阅！
    GameEventManager.OnSunrise -= HandleSunrise;
    // ... 其他事件
}
```

#### 事件处理器示例

```csharp
void HandleSunrise()
{
    Debug.Log("日出了！");
    // 播放鸟叫声
    // NPC开始活动
}

void HandleHourChanged(int hour)
{
    Debug.Log($"整点报时：{hour}:00");
    
    if (hour == 8)
    {
        // 商店开门
    }
    else if (hour == 20)
    {
        // 商店关门
    }
}

void HandleNewDay(int day)
{
    Debug.Log($"新的一天！Day {day}");
    // 重置每日任务
    // 刷新商店物品
}

void HandleSeasonChanged(GameTime.Season oldSeason, GameTime.Season newSeason)
{
    Debug.Log($"季节变化：{oldSeason} → {newSeason}");
    // 改变植被颜色
    // 调整天气系统
}
```

---

### 5. 保存与加载

#### 保存时间数据

```csharp
void SaveGame()
{
    // 获取时间数据
    GameTime.GameTimeData data = GameTime.Instance.GetSaveData();
    
    // 转换为JSON
    string json = JsonUtility.ToJson(data);
    
    // 保存到PlayerPrefs
    PlayerPrefs.SetString("GameTimeData", json);
    PlayerPrefs.Save();
    
    // 或者保存到文件
    System.IO.File.WriteAllText("savegame.json", json);
}
```

#### 加载时间数据

```csharp
void LoadGame()
{
    // 从PlayerPrefs加载
    if (PlayerPrefs.HasKey("GameTimeData"))
    {
        string json = PlayerPrefs.GetString("GameTimeData");
        GameTime.GameTimeData data = JsonUtility.FromJson<GameTime.GameTimeData>(json);
        GameTime.Instance.LoadSaveData(data);
    }
    
    // 或者从文件加载
    if (System.IO.File.Exists("savegame.json"))
    {
        string json = System.IO.File.ReadAllText("savegame.json");
        GameTime.GameTimeData data = JsonUtility.FromJson<GameTime.GameTimeData>(json);
        GameTime.Instance.LoadSaveData(data);
    }
}
```

#### GameTimeData 结构

```csharp
[Serializable]
public class GameTimeData
{
    public float currentTime;        // 当前时间
    public int currentDay;           // 当前天数
    public int currentYear;          // 当前年份
    public Season currentSeason;     // 当前季节
    public float timeMultiplier;     // 时间倍速
}
```

---

## 💡 实用示例

### 示例 1：商店营业时间

```csharp
public class Shop : MonoBehaviour
{
    public float openTime = 8f;      // 早上8点开门
    public float closeTime = 20f;    // 晚上8点关门
    
    public bool IsOpen()
    {
        return GameTime.Instance.IsTimeBetween(openTime, closeTime);
    }
    
    void Update()
    {
        if (IsOpen())
        {
            // 商店开放逻辑
        }
        else
        {
            // 商店关闭逻辑
        }
    }
}
```

### 示例 2：NPC日程表

```csharp
public class NPCSchedule : MonoBehaviour
{
    void OnEnable()
    {
        GameEventManager.OnHourChanged += CheckSchedule;
    }
    
    void OnDisable()
    {
        GameEventManager.OnHourChanged -= CheckSchedule;
    }
    
    void CheckSchedule(int hour)
    {
        switch (hour)
        {
            case 6:
                // 起床
                GoTo