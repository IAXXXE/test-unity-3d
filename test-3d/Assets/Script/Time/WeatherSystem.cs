using UnityEngine;
using System.Collections;

/// <summary>
/// 天气系统
/// 控制雨、雪、雾等天气效果
/// </summary>
public class WeatherSystem : MonoBehaviour
{
    public enum WeatherType
    {
        Clear,
        Cloudy,
        Rain,
        Storm,
        Snow,
        Fog
    }

    [Header("References")]
    public DayNightCycle dayNightCycle;

    [Header("Current Weather")]
    public WeatherType currentWeather = WeatherType.Clear;
    
    [Header("Weather Particles")]
    public ParticleSystem rainParticles;
    public ParticleSystem snowParticles;
    public ParticleSystem stormParticles;
    
    [Header("Audio")]
    public AudioClip rainSound;
    public AudioClip thunderSound;
    public AudioClip windSound;
    private AudioSource weatherAudioSource;
    
    [Header("Lighting Adjustments")]
    [Tooltip("天气对光照强度的影响")]
    public float clearLightMultiplier = 1f;
    public float cloudyLightMultiplier = 0.7f;
    public float rainyLightMultiplier = 0.5f;
    public float stormLightMultiplier = 0.3f;
    public float snowLightMultiplier = 0.8f;
    public float fogLightMultiplier = 0.6f;
    
    [Header("Fog Settings")]
    public bool enableWeatherFog = true;
    public float clearFogDensity = 0.001f;
    public float foggyFogDensity = 0.05f;
    public float rainyFogDensity = 0.02f;
    
    [Header("Transition Settings")]
    public float weatherTransitionDuration = 5f;
    public bool autoChangeWeather = true;
    public float weatherChangeCooldown = 60f; // 秒
    
    [Header("Thunder Settings")]
    public float thunderChance = 0.1f; // 暴风雨中闪电概率
    public float thunderInterval = 10f;
    public Light lightningLight;
    
    private WeatherType targetWeather;
    private float weatherTimer = 0f;
    private float thunderTimer = 0f;
    private bool isTransitioning = false;
    private Coroutine transitionCoroutine;

    void Awake()
    {
        CreateAudioSource();
        targetWeather = currentWeather;
    }

    void Start()
    {
        ApplyWeather(currentWeather, true);
        
        if (autoChangeWeather)
        {
            weatherTimer = weatherChangeCooldown;
        }
    }

    void Update()
    {
        if (autoChangeWeather)
        {
            weatherTimer -= Time.deltaTime;
            if (weatherTimer <= 0f)
            {
                ChangeToRandomWeather();
                weatherTimer = weatherChangeCooldown;
            }
        }

        // 暴风雨闪电效果
        if (currentWeather == WeatherType.Storm)
        {
            thunderTimer -= Time.deltaTime;
            if (thunderTimer <= 0f && Random.value < thunderChance)
            {
                StartCoroutine(TriggerLightning());
                thunderTimer = thunderInterval;
            }
        }
    }

    void CreateAudioSource()
    {
        GameObject audioObj = new GameObject("Weather Audio Source");
        audioObj.transform.SetParent(transform);
        weatherAudioSource = audioObj.AddComponent<AudioSource>();
        weatherAudioSource.loop = true;
        weatherAudioSource.spatialBlend = 0f;
        weatherAudioSource.volume = 0.5f;
    }

    public void ChangeWeather(WeatherType newWeather)
    {
        if (currentWeather == newWeather || isTransitioning)
            return;

        targetWeather = newWeather;

        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        transitionCoroutine = StartCoroutine(TransitionWeather(currentWeather, newWeather));
    }

    public void ChangeToRandomWeather()
    {
        WeatherType[] weatherTypes = (WeatherType[])System.Enum.GetValues(typeof(WeatherType));
        WeatherType randomWeather;
        
        do
        {
            randomWeather = weatherTypes[Random.Range(0, weatherTypes.Length)];
        }
        while (randomWeather == currentWeather);

        ChangeWeather(randomWeather);
    }

    IEnumerator TransitionWeather(WeatherType from, WeatherType to)
    {
        isTransitioning = true;
        float elapsed = 0f;

        // 获取初始值
        float startLightMultiplier = GetLightMultiplier(from);
        float targetLightMultiplier = GetLightMultiplier(to);
        
        float startFogDensity = GetFogDensity(from);
        float targetFogDensity = GetFogDensity(to);

        while (elapsed < weatherTransitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / weatherTransitionDuration;

            // 平滑插值
            float currentLightMultiplier = Mathf.Lerp(startLightMultiplier, targetLightMultiplier, t);
            float currentFogDensity = Mathf.Lerp(startFogDensity, targetFogDensity, t);

            // 应用光照变化
            ApplyLightingMultiplier(currentLightMultiplier);
            
            // 应用雾效变化
            if (enableWeatherFog)
            {
                RenderSettings.fogDensity = currentFogDensity;
            }

            yield return null;
        }

        // 完成过渡
        currentWeather = to;
        ApplyWeather(to, false);
        isTransitioning = false;

        Debug.Log($"Weather changed to: {to}");
    }

    void ApplyWeather(WeatherType weather, bool immediate)
    {
        // 停止所有粒子
        StopAllWeatherParticles();

        // 应用新天气
        switch (weather)
        {
            case WeatherType.Clear:
                ApplyClearWeather();
                break;
            case WeatherType.Cloudy:
                ApplyCloudyWeather();
                break;
            case WeatherType.Rain:
                ApplyRainWeather();
                break;
            case WeatherType.Storm:
                ApplyStormWeather();
                break;
            case WeatherType.Snow:
                ApplySnowWeather();
                break;
            case WeatherType.Fog:
                ApplyFogWeather();
                break;
        }

        if (immediate)
        {
            ApplyLightingMultiplier(GetLightMultiplier(weather));
            if (enableWeatherFog)
            {
                RenderSettings.fogDensity = GetFogDensity(weather);
            }
        }
    }

    void StopAllWeatherParticles()
    {
        if (rainParticles != null) rainParticles.Stop();
        if (snowParticles != null) snowParticles.Stop();
        if (stormParticles != null) stormParticles.Stop();
    }

    void ApplyClearWeather()
    {
        StopWeatherAudio();
    }

    void ApplyCloudyWeather()
    {
        StopWeatherAudio();
    }

    void ApplyRainWeather()
    {
        if (rainParticles != null)
            rainParticles.Play();

        PlayWeatherAudio(rainSound, 0.4f);
    }

    void ApplyStormWeather()
    {
        if (stormParticles != null)
            stormParticles.Play();
        else if (rainParticles != null)
            rainParticles.Play();

        PlayWeatherAudio(rainSound, 0.6f);
        thunderTimer = thunderInterval;
    }

    void ApplySnowWeather()
    {
        if (snowParticles != null)
            snowParticles.Play();

        PlayWeatherAudio(windSound, 0.3f);
    }

    void ApplyFogWeather()
    {
        StopWeatherAudio();
    }

    void PlayWeatherAudio(AudioClip clip, float volume)
    {
        if (weatherAudioSource != null && clip != null)
        {
            weatherAudioSource.clip = clip;
            weatherAudioSource.volume = volume;
            weatherAudioSource.Play();
        }
    }

    void StopWeatherAudio()
    {
        if (weatherAudioSource != null && weatherAudioSource.isPlaying)
        {
            weatherAudioSource.Stop();
        }
    }

    void ApplyLightingMultiplier(float multiplier)
    {
        if (dayNightCycle != null && dayNightCycle.sunLight != null)
        {
            Light sun = dayNightCycle.sunLight;
            float baseIntensity = dayNightCycle.maxSunIntensity;
            sun.intensity = baseIntensity * multiplier * CalculateSunIntensityFromTime();
        }
    }

    float CalculateSunIntensityFromTime()
    {        
        float time = GameTime.Instance.currentTime;
        float sunrise = GameTime.Instance.sunriseTime;
        float sunset = GameTime.Instance.sunsetTime;

        if (time < sunrise || time > sunset)
            return 0.1f; // 夜晚
        else if (time >= sunrise && time <= sunset)
            return 1f; // 白天
        
        return 1f;
    }

    float GetLightMultiplier(WeatherType weather)
    {
        switch (weather)
        {
            case WeatherType.Clear: return clearLightMultiplier;
            case WeatherType.Cloudy: return cloudyLightMultiplier;
            case WeatherType.Rain: return rainyLightMultiplier;
            case WeatherType.Storm: return stormLightMultiplier;
            case WeatherType.Snow: return snowLightMultiplier;
            case WeatherType.Fog: return fogLightMultiplier;
            default: return 1f;
        }
    }

    float GetFogDensity(WeatherType weather)
    {
        switch (weather)
        {
            case WeatherType.Clear: return clearFogDensity;
            case WeatherType.Fog: return foggyFogDensity;
            case WeatherType.Rain:
            case WeatherType.Storm: return rainyFogDensity;
            default: return clearFogDensity;
        }
    }

    IEnumerator TriggerLightning()
    {
        if (lightningLight != null)
        {
            lightningLight.enabled = true;
            lightningLight.intensity = Random.Range(2f, 5f);
        }

        // 播放雷声
        if (thunderSound != null && weatherAudioSource != null)
        {
            weatherAudioSource.PlayOneShot(thunderSound, 0.8f);
        }

        // 闪烁效果
        yield return new WaitForSeconds(0.05f);
        if (lightningLight != null)
            lightningLight.enabled = false;

        yield return new WaitForSeconds(0.1f);
        if (lightningLight != null)
        {
            lightningLight.enabled = true;
            lightningLight.intensity = Random.Range(1f, 3f);
        }

        yield return new WaitForSeconds(0.05f);
        if (lightningLight != null)
            lightningLight.enabled = false;
    }

    // Public API
    public void SetClearWeather() => ChangeWeather(WeatherType.Clear);
    public void SetRainWeather() => ChangeWeather(WeatherType.Rain);
    public void SetStormWeather() => ChangeWeather(WeatherType.Storm);
    public void SetSnowWeather() => ChangeWeather(WeatherType.Snow);
    public void SetFogWeather() => ChangeWeather(WeatherType.Fog);
}