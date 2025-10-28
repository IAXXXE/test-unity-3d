# 🌅 昼夜交替系统 - 完整设置指南

## 📦 系统组成

1. **DayNightCycle.cs** - 核心控制器
2. **EnvironmentEffectsManager.cs** - 环境效果管理
3. **TimeUIDisplay.cs** - UI显示
4. **WeatherSystem.cs** - 天气系统（可选）
5. **SkyboxBlend.shader** - 天空盒混合Shader（可选）

---

## 🔧 场景设置步骤

### 第一步：创建光源

```
1. 创建 Directional Light，命名为 "Sun"
   - Intensity: 1.5
   - Color: 白色 (1, 1, 1)
   - Rotation: (50, -30, 0)

2. 创建 Directional Light，命名为 "Moon"
   - Intensity: 0.3
   - Color: 淡蓝色 (0.7, 0.8, 1)
   - Rotation: (230, -30, 0)
   - Enabled: false (初始关闭)
```

### 第二步：创建天体对象（可选）

```
3. 创建空物体 "Celestial System"
   ├─ Sun (已有的太阳光源)
   └─ Moon (已有的月亮光源)
```

### 第三步：设置昼夜控制器

```
4. 创建空物体 "DayNight Manager"
5. 添加脚本 DayNightCycle
6. 配置参数：

Time Settings:
├─ Day Duration In Seconds: 120 (2分钟一个完整昼夜)
├─ Current Time: 12 (从正午开始)
├─ Time Multiplier: 1
└─ Pause Time: false

Celestial Objects:
├─ Sun Light: [拖入 Sun]
├─ Moon Light: [拖入 Moon]
├─ Sun Transform: [拖入 Sun]
└─ Moon Transform: [拖入 Moon]

Light Settings:
├─ Sunrise Time: 6
├─ Sunset Time: 18
├─ Max Sun Intensity: 1.5
└─ Max Moon Intensity: 0.3
```

### 第四步：配置环境光和雾

```
Window → Rendering → Lighting

Environment:
├─ Skybox Material: [选择天空盒]
├─ Sun Source: Sun (光源)
├─ Environment Lighting: Gradient
└─ Ambient Mode: Trilight

Fog:
├─ Enable: ✓
├─ Mode: Exponential
├─ Color: 淡灰色
└─ Density: 0.001
```

---

## 🎨 高级设置

### 方案 A：使用默认渐变（简单）

在 DayNightCycle 中已自动创建默认渐变，无需额外设置。

### 方案 B：自定义颜色渐变

```
Sun Color Gradient:
├─ 0.00 (午夜): 深蓝 (0.2, 0.3, 0.5)
├─ 0.25 (日出): 橙色 (1.0, 0.6, 0.4)
├─ 0.50 (正午): 亮白 (1.0, 0.95, 0.9)
├─ 0.75 (日落): 橙红 (1.0, 0.5, 0.3)
└─ 1.00 (午夜): 深蓝 (0.2, 0.3, 0.5)

Ambient Color Gradient:
├─ 0.00: 深蓝 (0.1, 0.15, 0.25)
├─ 0.25: 粉橙 (0.8, 0.6, 0.5)
├─ 0.50: 淡蓝 (0.7, 0.85, 1.0)
├─ 0.75: 橙红 (0.9, 0.5, 0.4)
└─ 1.00: 深蓝 (0.1, 0.15, 0.25)

Fog Color Gradient:
├─ 0.00: 深灰 (0.2, 0.2, 0.3)
├─ 0.25: 粉橙 (0.9, 0.7, 0.6)
├─ 0.50: 淡蓝 (0.8, 0.9, 1.0)
├─ 0.75: 橙红 (1.0, 0.6, 0.5)
└─ 1.00: 深灰 (0.2, 0.2, 0.3)
```

---

## 🌟 添加环境效果

### 1. 创建粒子系统

```
GameObject → Effects → Particle System

Fireflies (萤火虫):
├─ Duration: 5
├─ Start Lifetime: 3-5
├─ Start Speed: 0.5-1
├─ Start Size: 0.05-0.1
├─ Start Color: 黄绿色发光
├─ Emission Rate: 20
└─ Shape: Sphere (Radius: 20)

Stars (星星):
├─ Duration: 5
├─ Start Lifetime: ∞
├─ Start Speed: 0
├─ Start Size: 0.02-0.05
├─ Start Color: 白色
├─ Emission Rate: 200
└─ Shape: Hemisphere (Radius: 50, 朝下)
```

### 2. 添加环境效果管理器

```
在 "DayNight Manager" 上添加脚本 EnvironmentEffectsManager

配置:
├─ Day Night Cycle: [自动检测]
├─ Fireflies: [拖入萤火虫粒子]
├─ Stars: [拖入星星粒子]
├─ Daytime Ambient: [白天环境音]
└─ Nighttime Ambient: [夜晚环境音]
```

---

## 🎮 创建UI

### 1. 创建Canvas

```
GameObject → UI → Canvas

Canvas:
├─ Render Mode: Screen Space - Overlay
└─ UI Scale Mode: Scale With Screen Size
```

### 2. 添加时间显示

```
在 Canvas 下创建:

TimePanel (Panel):
├─ Anchor: 右上角
├─ Width: 200, Height: 100
└─ 子对象:
   ├─ TimeText (TextMeshPro)
   │  ├─ Text: "12:00"
   │  └─ Font Size: 36
   ├─ PeriodText (TextMeshPro)
   │  ├─ Text: "正午 Noon"
   │  └─ Font Size: 18
   └─ DayNightIcon (Image)
      └─ Size: 40x40
```

### 3. 配置TimeUIDisplay

```
创建空物体 "Time UI Controller"
添加脚本 TimeUIDisplay

配置:
├─ Day Night Cycle: [拖入 DayNight Manager]
├─ Time Text: [拖入 TimeText]
├─ Period Text: [拖入 PeriodText]
├─ Day Night Icon: [拖入 DayNightIcon]
├─ Sun Icon: [导入太阳图标]
└─ Moon Icon: [导入月亮图标]
```

---

## 🌦️ 添加天气系统（可选）

### 1. 创建天气粒子

```
Rain (雨):
├─ Shape: Box (50x30x50)
├─ Position: (0, 30, 0)
├─ Emission: 500
├─ Start Lifetime: 2-3
├─ Start Speed: 10-15
├─ Gravity Modifier: 2
└─ Particle Texture: 雨滴纹理

Snow (雪):
├─ Shape: Box (50x30x