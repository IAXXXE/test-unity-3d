using UnityEngine;
using System;

/// <summary>
/// 昼夜交替系统 - 核心控制器
/// 控制太阳/月亮旋转、光照强度、天空盒、环境颜色等
/// </summary>
public class DayNightCycle : MonoBehaviour
{
    [Header("Celestial Objects")]
    [Tooltip("太阳光源（Directional Light）")]
    public Light sunLight;
    
    [Tooltip("月亮光源（Directional Light）")]
    public Light moonLight;
    
    [Tooltip("太阳Transform（用于旋转）")]
    public Transform sunTransform;
    
    [Tooltip("月亮Transform（用于旋转）")]
    public Transform moonTransform;
    
    [Tooltip("白天太阳最大强度")]
    public float maxSunIntensity = 1.5f;
    
    [Tooltip("夜晚月亮最大强度")]
    public float maxMoonIntensity = 0.3f;
    
    [Tooltip("白天太阳光颜色")]
    private Gradient sunColor;
    
    [Tooltip("夜晚月亮光颜色")]
    private Gradient moonColor;

    [Header("Ambient Light")]
    [Tooltip("环境光颜色渐变")]
    public Gradient ambientColor;
    
    [Tooltip("环境光强度曲线（0-24小时）")]
    public AnimationCurve ambientIntensityCurve;

    [Header("Fog Settings")]
    [Tooltip("启用雾效果变化")]
    public bool enableFog = true;
    
    [Tooltip("雾颜色渐变")]
    public Gradient fogColor;
    
    [Tooltip("雾密度曲线")]
    public AnimationCurve fogDensityCurve;

    [Header("Skybox")]
    [Tooltip("启用天空盒材质切换")]
    public bool enableSkyboxTransition = false;
    
    [Tooltip("白天天空盒")]
    public Material daySkybox;
    
    [Tooltip("夜晚天空盒")]
    public Material nightSkybox;
    
    [Tooltip("天空盒混合材质（需要自定义Shader）")]
    public Material blendSkybox;

    void Awake()
    {
        InitializeDefaultValues();
    }

    void Start()
    {

    }

    void Update()
    {
        // 旋转天体
        RotateCelestialObjects();

        // 更新光照
        UpdateLighting();

        // 更新环境光
        UpdateAmbientLight();

        // 更新雾效
        if (enableFog)
        {
            UpdateFog();
        }

        // 更新天空盒
        if (enableSkyboxTransition)
        {
            UpdateSkybox();
        }
    }

    void InitializeDefaultValues()
    {
        // 创建默认的太阳颜色渐变
        if (sunColor == null || sunColor.colorKeys.Length == 0)
        {
            sunColor = new Gradient();
            GradientColorKey[] sunColorKeys = new GradientColorKey[5];
            GradientAlphaKey[] sunAlphaKeys = new GradientAlphaKey[2];

            // 日出前 - 深蓝
            sunColorKeys[0].color = new Color(0.2f, 0.3f, 0.5f);
            sunColorKeys[0].time = 0f;

            // 日出 - 橙色
            sunColorKeys[1].color = new Color(1f, 0.6f, 0.4f);
            sunColorKeys[1].time = 0.25f;

            // 正午 - 亮白
            sunColorKeys[2].color = new Color(1f, 0.95f, 0.9f);
            sunColorKeys[2].time = 0.5f;

            // 日落 - 橙红
            sunColorKeys[3].color = new Color(1f, 0.5f, 0.3f);
            sunColorKeys[3].time = 0.75f;

            // 夜晚 - 深蓝
            sunColorKeys[4].color = new Color(0.2f, 0.3f, 0.5f);
            sunColorKeys[4].time = 1f;

            sunAlphaKeys[0].alpha = 1f;
            sunAlphaKeys[0].time = 0f;
            sunAlphaKeys[1].alpha = 1f;
            sunAlphaKeys[1].time = 1f;

            sunColor.SetKeys(sunColorKeys, sunAlphaKeys);
        }

        // 默认月亮色
        if(moonColor == null || moonColor.colorKeys.Length == 0)
        {
            moonColor = new Gradient();
            GradientColorKey[] moonColorKeys = new GradientColorKey[5];
            GradientAlphaKey[] moonAlphaKeys = new GradientAlphaKey[2];

            // 日落 - 深蓝
            moonColorKeys[0].color = new Color(0.2f, 0.3f, 0.5f);
            moonColorKeys[0].time = 0f;

            // 月出 - 橙色
            moonColorKeys[1].color = new Color(0.15f, 0.25f, 0.4f);
            moonColorKeys[1].time = 0.25f;

            // 正晚 - 暗黑
            moonColorKeys[2].color = new Color(0.1f, 0.15f, 0.2f);
            moonColorKeys[2].time = 0.5f;

            // 月落 - 橙红
            moonColorKeys[3].color = new Color(0.1f, 0.2f, 0.4f);
            moonColorKeys[3].time = 0.75f;

            // 日出 - 深蓝
            moonColorKeys[4].color = new Color(0.2f, 0.3f, 0.5f);
            moonColorKeys[4].time = 1f;

            moonAlphaKeys[0].alpha = 1f;
            moonAlphaKeys[0].time = 0f;
            moonAlphaKeys[1].alpha = 1f;
            moonAlphaKeys[1].time = 1f;

            moonColor.SetKeys(moonColorKeys, moonAlphaKeys);
        }

        // 创建默认的环境光强度曲线
        if (ambientIntensityCurve == null || ambientIntensityCurve.keys.Length == 0)
        {
            ambientIntensityCurve = AnimationCurve.EaseInOut(0f, 0.2f, 1f, 1f);
            ambientIntensityCurve.AddKey(0.25f, 0.6f);
            ambientIntensityCurve.AddKey(0.75f, 0.6f);
        }

        // 如果没有设置太阳和月亮，尝试自动查找
        if (sunLight == null)
        {
            Light[] lights = FindObjectsOfType<Light>();
            foreach (Light light in lights)
            {
                if (light.type == LightType.Directional && light.name.Contains("Sun"))
                {
                    sunLight = light;
                    sunTransform = light.transform;
                    break;
                }
            }
        }
    }

    void RotateCelestialObjects()
    {
        // 计算太阳角度（0度=日出，180度=日落）
        float sunAngle = (GameTime.Instance.currentTime / 24f) * 360f - 90f;

        if (sunTransform != null)
        {
            sunTransform.rotation = Quaternion.Euler(sunAngle, 0f, 0f);
        }
        else if (sunLight != null)
        {
            sunLight.transform.rotation = Quaternion.Euler(sunAngle, 0f, 0f);
        }

        // 月亮旋转（与太阳相反）
        if (moonTransform != null)
        {
            moonTransform.rotation = Quaternion.Euler(sunAngle + 180f, 0f, 0f);
        }
        else if (moonLight != null)
        {
            moonLight.transform.rotation = Quaternion.Euler(sunAngle + 180f, 0f, 0f);
        }
    }

    void UpdateLighting()
    {
        // 计算日出日落的淡入淡出
        float sunIntensity = CalculateSunIntensity();
        float moonIntensity = 1f - sunIntensity;

        // 更新太阳
        if (sunLight != null)
        {
            sunLight.intensity = sunIntensity * maxSunIntensity;
            if(sunColor != null)
                sunLight.color = sunColor.Evaluate(GameTime.Instance.TimeOfDay);
            sunLight.enabled = sunIntensity > 0.01f;
        }

        // 更新月亮
        if (moonLight != null)
        {
            moonLight.intensity = moonIntensity * maxMoonIntensity;
            if (moonColor != null)
                moonLight.color = moonColor.Evaluate(GameTime.Instance.TimeOfDay);
            else
                moonLight.color = new Color(0.7f, 0.8f, 1f); // 默认月光色
            moonLight.enabled = moonIntensity > 0.01f;
        }
    }

    float CalculateSunIntensity()
    {
        // 在日出和日落时间段内平滑过渡
        float transitionDuration = 2f; // 过渡持续时间（小时）

        if (GameTime.Instance.currentTime < GameTime.Instance.SunriseTime - transitionDuration)
        {
            return 0f; // 夜晚
        }
        else if (GameTime.Instance.currentTime < GameTime.Instance.SunriseTime + transitionDuration)
        {
            // 日出
            float t = (GameTime.Instance.currentTime - (GameTime.Instance.SunriseTime - transitionDuration)) / (transitionDuration * 2f);
            return Mathf.SmoothStep(0f, 1f, t);
        }
        else if (GameTime.Instance.currentTime < GameTime.Instance.SunsetTime - transitionDuration)
        {
            return 1f; // 白天
        }
        else if (GameTime.Instance.currentTime < GameTime.Instance.SunsetTime + transitionDuration)
        {
            // 日落
            float t = (GameTime.Instance.currentTime - (GameTime.Instance.SunsetTime - transitionDuration)) / (transitionDuration * 2f);
            return Mathf.SmoothStep(1f, 0f, t);
        }
        else
        {
            return 0f; // 夜晚
        }
    }

    void UpdateAmbientLight()
    {
        if (ambientColor != null)
        {
            RenderSettings.ambientLight = ambientColor.Evaluate(GameTime.Instance.TimeOfDay);
        }

        if (ambientIntensityCurve != null)
        {
            RenderSettings.ambientIntensity = ambientIntensityCurve.Evaluate(GameTime.Instance.TimeOfDay);
        }
    }

    void UpdateFog()
    {
        if (fogColor != null)
        {
            RenderSettings.fogColor = fogColor.Evaluate(GameTime.Instance.TimeOfDay);
        }

        if (fogDensityCurve != null)
        {
            RenderSettings.fogDensity = fogDensityCurve.Evaluate(GameTime.Instance.TimeOfDay);
        }
    }

    void UpdateSkybox()
    {
        if (blendSkybox != null && daySkybox != null && nightSkybox != null)
        {
            // 使用自定义混合材质
            float blend = CalculateSunIntensity();
            blendSkybox.SetFloat("_Blend", blend);
            RenderSettings.skybox = blendSkybox;
        }
        else if (daySkybox != null && nightSkybox != null)
        {
            // 简单切换
            RenderSettings.skybox = GameTime.Instance.IsDay ? daySkybox : nightSkybox;
        }
    }

}