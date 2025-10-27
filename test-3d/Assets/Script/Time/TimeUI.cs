using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 时间UI显示器
/// 显示当前游戏时间、昼夜状态等
/// </summary>
public class TimeUIDisplay : MonoBehaviour
{
    [Header("References")]
    public DayNightCycle dayNightCycle;

    [Header("UI Elements")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI dateText;
    public TextMeshProUGUI periodText; // 显示"上午"/"下午"/"夜晚"等
    public Image dayNightIcon;
    public Slider timeSlider; // 可选：显示时间进度条

    [Header("Icons")]
    public Sprite sunIcon;
    public Sprite moonIcon;
    public Sprite sunriseIcon;
    public Sprite sunsetIcon;

    [Header("Display Settings")]
    public bool use24HourFormat = false;
    public bool showSeconds = false;
    public bool showDate = true;
    public Color dayColor = Color.yellow;
    public Color nightColor = new Color(0.5f, 0.6f, 1f);

    [Header("Animation")]
    public bool animateTransition = true;
    public float transitionSpeed = 2f;

    private int currentDay = 1;
    private Color currentColor;

    void Start()
    {
        GameEventManager.OnSunrise += OnSunrise;
        GameEventManager.OnSunset += OnSunset;
        // GameEventManager.OnMidnight += OnMidnight;
        currentColor = dayColor;
    }

    void OnDestroy()
    {
        GameEventManager.OnSunrise -= OnSunrise;
        GameEventManager.OnSunset -= OnSunset;
        // GameEventManager.OnMidnight -= OnMidnight;
    }

    void Update()
    {
        UpdateTimeDisplay();
        UpdatePeriodDisplay();
        UpdateIcon();
        UpdateSlider();
        AnimateColors();
    }

    void UpdateTimeDisplay()
    {
        if (timeText == null) return;

        float time = GameTime.Instance.currentTime;
        int hours = Mathf.FloorToInt(time);
        int minutes = Mathf.FloorToInt((time - hours) * 60f);
        int seconds = Mathf.FloorToInt(((time - hours) * 60f - minutes) * 60f);

        if (use24HourFormat)
        {
            if (showSeconds)
                timeText.text = $"{hours:00}:{minutes:00}:{seconds:00}";
            else
                timeText.text = $"{hours:00}:{minutes:00}";
        }
        else
        {
            // 12小时制
            int displayHours = hours % 12;
            if (displayHours == 0) displayHours = 12;
            string period = hours >= 12 ? "PM" : "AM";

            if (showSeconds)
                timeText.text = $"{displayHours}:{minutes:00}:{seconds:00} {period}";
            else
                timeText.text = $"{displayHours}:{minutes:00} {period}";
        }
    }

    void UpdatePeriodDisplay()
    {
        if (periodText == null) return;

        float time = GameTime.Instance.currentTime;
        string period = "";

        if (time >= 5f && time < 6f)
            period = "黎明 Dawn";
        else if (time >= 6f && time < 12f)
            period = "上午 Morning";
        else if (time >= 12f && time < 13f)
            period = "正午 Noon";
        else if (time >= 13f && time < 18f)
            period = "下午 Afternoon";
        else if (time >= 18f && time < 19f)
            period = "黄昏 Dusk";
        else if (time >= 19f && time < 24f || time < 5f)
            period = "夜晚 Night";

        periodText.text = period;
    }

    void UpdateIcon()
    {
        if (dayNightIcon == null) return;

        float time = GameTime.Instance.currentTime;

        // 根据时间段选择图标
        if (time >= 5f && time < 7f && sunriseIcon != null)
            dayNightIcon.sprite = sunriseIcon;
        else if (time >= 17f && time < 19f && sunsetIcon != null)
            dayNightIcon.sprite = sunsetIcon;
        else if (time >= 7f && time < 17f && sunIcon != null)
            dayNightIcon.sprite = sunIcon;
        else if (moonIcon != null)
            dayNightIcon.sprite = moonIcon;

        // 平滑颜色过渡
        Color targetColor = GameTime.Instance.IsDay ? dayColor : nightColor;
        if (animateTransition)
        {
            currentColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * transitionSpeed);
        }
        else
        {
            currentColor = targetColor;
        }

        dayNightIcon.color = currentColor;
    }

    void UpdateSlider()
    {
        if (timeSlider == null) return;

        timeSlider.value = GameTime.Instance.NormalizedTime;
    }

    void AnimateColors()
    {
        if (!animateTransition) return;

        // 为文本添加颜色动画
        if (timeText != null)
            timeText.color = Color.Lerp(timeText.color, currentColor, Time.deltaTime * transitionSpeed);

        if (periodText != null)
            periodText.color = Color.Lerp(periodText.color, currentColor, Time.deltaTime * transitionSpeed);
    }

    void OnSunrise()
    {
        // 日出时的特殊效果
        if (animateTransition)
        {
            StartCoroutine(IconPulse());
        }
    }

    void OnSunset()
    {
        // 日落时的特殊效果
        if (animateTransition)
        {
            StartCoroutine(IconPulse());
        }
    }

    void OnMidnight()
    {
        currentDay++;
        UpdateDateDisplay();
    }

    void UpdateDateDisplay()
    {
        if (dateText == null || !showDate) return;

        dateText.text = $"Day {currentDay}";
    }

    System.Collections.IEnumerator IconPulse()
    {
        Vector3 originalScale = dayNightIcon.transform.localScale;
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float scale = 1f + Mathf.Sin((elapsed / duration) * Mathf.PI) * 0.2f;
            dayNightIcon.transform.localScale = originalScale * scale;
            yield return null;
        }

        dayNightIcon.transform.localScale = originalScale;
    }

    // Public methods for UI controls
    public void OnTimeSliderChanged(float value)
    {
        GameTime.Instance.SetTimeOfDay(value);
    }

    public void FastForward()
    {
        GameTime.Instance.timeMultiplier *= 2f;
    }

    public void SlowDown()
    {
        GameTime.Instance.timeMultiplier = Mathf.Max(0.5f, GameTime.Instance.timeMultiplier / 2f);
    }

    public void TogglePause()
    {
        GameTime.Instance.PauseTime(!GameTime.Instance.pauseTime);
    }
}