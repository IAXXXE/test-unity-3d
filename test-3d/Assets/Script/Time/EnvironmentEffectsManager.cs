using UnityEngine;
using System.Collections;

/// <summary>
/// 环境效果管理器
/// 根据昼夜时间控制粒子系统、音效、后处理等
/// </summary>
[RequireComponent(typeof(DayNightCycle))]
public class EnvironmentEffectsManager : MonoBehaviour
{
    [Header("References")]
    public DayNightCycle dayNightCycle;

    [Header("Particle Systems")]
    [Tooltip("萤火虫粒子系统（夜晚）")]
    public ParticleSystem fireflies;
    
    [Tooltip("晨雾粒子系统（清晨）")]
    public ParticleSystem morningMist;
    
    [Tooltip("星星粒子系统（夜晚）")]
    public ParticleSystem stars;

    [Header("Audio")]
    [Tooltip("白天环境音效")]
    public AudioClip daytimeAmbient;
    
    [Tooltip("夜晚环境音效")]
    public AudioClip nighttimeAmbient;
    
    [Tooltip("鸟鸣音效（清晨）")]
    public AudioClip birdChirping;
    
    [Tooltip("虫鸣音效（夜晚）")]
    public AudioClip crickets;
    
    private AudioSource ambientSource;
    private AudioSource effectSource;

    [Header("Time-based Activation")]
    [Tooltip("萤火虫出现时间")]
    public float firefliesStartTime = 19f;
    public float firefliesEndTime = 5f;
    
    [Tooltip("晨雾出现时间")]
    public float mistStartTime = 5f;
    public float mistEndTime = 8f;
    
    [Tooltip("星星出现时间")]
    public float starsStartTime = 20f;
    public float starsEndTime = 6f;

    [Header("Transition Settings")]
    [Tooltip("音效淡入淡出时间")]
    public float audioFadeDuration = 3f;
    
    [Tooltip("粒子淡入淡出时间")]
    public float particleFadeDuration = 2f;

    [Header("Post Processing")]
    public bool enablePostProcessing = false;
    // 可以添加 Post Processing Volume 的引用

    private bool isTransitioningAudio = false;

    void Awake()
    {
        if (dayNightCycle == null)
            dayNightCycle = GetComponent<DayNightCycle>();

        // 创建音频源
        CreateAudioSources();
    }

    void Start()
    {
        GameEventManager.OnSunrise += OnSunrise;
        GameEventManager.OnSunset += OnSunset;
        // dayNightCycle.OnNoon += OnNoon;
        // dayNightCycle.OnMidnight += OnMidnight;

   // 初始化状态
        UpdateEnvironmentEffects(GameTime.Instance.currentTime);
    }

    void OnDestroy()
    {
        GameEventManager.OnSunrise -= OnSunrise;
        GameEventManager.OnSunset -= OnSunset;
        // dayNightCycle.OnNoon -= OnNoon;
        // dayNightCycle.OnMidnight -= OnMidnight;
    }

    void Update()
    {
        if (dayNightCycle == null) return;

        UpdateEnvironmentEffects(GameTime.Instance.currentTime);
    }

    void CreateAudioSources()
    {
        // 环境音源（循环播放）
        GameObject ambientObj = new GameObject("Ambient Audio Source");
        ambientObj.transform.SetParent(transform);
        ambientSource = ambientObj.AddComponent<AudioSource>();
        ambientSource.loop = true;
        ambientSource.spatialBlend = 0f; // 2D音效
        ambientSource.volume = 0f;

        // 效果音源（一次性播放）
        GameObject effectObj = new GameObject("Effect Audio Source");
        effectObj.transform.SetParent(transform);
        effectSource = effectObj.AddComponent<AudioSource>();
        effectSource.loop = false;
        effectSource.spatialBlend = 0f;
        effectSource.volume = 0.7f;
    }

    void UpdateEnvironmentEffects(float currentTime)
    {
        // 更新萤火虫
        if (fireflies != null)
        {
            bool shouldShowFireflies = IsTimeBetween(currentTime, firefliesStartTime, firefliesEndTime);
            UpdateParticleSystem(fireflies, shouldShowFireflies);
        }

        // 更新晨雾
        if (morningMist != null)
        {
            bool shouldShowMist = IsTimeBetween(currentTime, mistStartTime, mistEndTime);
            UpdateParticleSystem(morningMist, shouldShowMist);
        }

        // 更新星星
        if (stars != null)
        {
            bool shouldShowStars = IsTimeBetween(currentTime, starsStartTime, starsEndTime);
            UpdateParticleSystem(stars, shouldShowStars);
        }
    }

    void UpdateParticleSystem(ParticleSystem ps, bool shouldPlay)
    {
        if (shouldPlay && !ps.isPlaying)
        {
            ps.Play();
        }
        else if (!shouldPlay && ps.isPlaying)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        // 平滑调整发射速率
        var emission = ps.emission;
        float targetRate = shouldPlay ? 1f : 0f;
        emission.rateOverTimeMultiplier = Mathf.Lerp(
            emission.rateOverTimeMultiplier,
            targetRate * 10f,
            Time.deltaTime / particleFadeDuration
        );
    }

    bool IsTimeBetween(float current, float start, float end)
    {
        // 处理跨午夜的时间段（例如 22:00 到 6:00）
        if (start > end)
        {
            return current >= start || current <= end;
        }
        else
        {
            return current >= start && current <= end;
        }
    }

    void OnSunrise()
    {
        Debug.Log("🌅 Environment: Sunrise effects");
        
        // 切换到白天音效
        if (daytimeAmbient != null && !isTransitioningAudio)
        {
            StartCoroutine(TransitionAmbientAudio(daytimeAmbient));
        }

        // 播放鸟鸣
        if (birdChirping != null && effectSource != null)
        {
            effectSource.PlayOneShot(birdChirping, 0.5f);
        }
    }

    void OnSunset()
    {
        Debug.Log("🌇 Environment: Sunset effects");
        
        // 切换到夜晚音效
        if (nighttimeAmbient != null && !isTransitioningAudio)
        {
            StartCoroutine(TransitionAmbientAudio(nighttimeAmbient));
        }

        // 播放虫鸣
        if (crickets != null && effectSource != null)
        {
            effectSource.PlayOneShot(crickets, 0.3f);
        }
    }

    void OnNoon()
    {
        Debug.Log("☀️ Environment: Noon - brightest time");
    }

    void OnMidnight()
    {
        Debug.Log("🌙 Environment: Midnight - darkest time");
    }

    IEnumerator TransitionAmbientAudio(AudioClip newClip)
    {
        if (ambientSource == null) yield break;

        isTransitioningAudio = true;

        // 淡出当前音效
        float startVolume = ambientSource.volume;
        float elapsed = 0f;

        while (elapsed < audioFadeDuration / 2f)
        {
            elapsed += Time.deltaTime;
            ambientSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / (audioFadeDuration / 2f));
            yield return null;
        }

        // 切换音频
        ambientSource.clip = newClip;
        ambientSource.Play();

        // 淡入新音效
        elapsed = 0f;
        while (elapsed < audioFadeDuration / 2f)
        {
            elapsed += Time.deltaTime;
            ambientSource.volume = Mathf.Lerp(0f, 0.6f, elapsed / (audioFadeDuration / 2f));
            yield return null;
        }

        ambientSource.volume = 0.6f;
        isTransitioningAudio = false;
    }

    // Public methods for manual control
    public void PlayEffect(AudioClip clip, float volume = 1f)
    {
        if (effectSource != null && clip != null)
        {
            effectSource.PlayOneShot(clip, volume);
        }
    }

    public void EnableParticleSystem(ParticleSystem ps, bool enable)
    {
        if (ps != null)
        {
            if (enable)
                ps.Play();
            else
                ps.Stop();
        }
    }
}