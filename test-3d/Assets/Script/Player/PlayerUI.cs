using System;
using UnityEngine;
using UnityEngine.UI;

public enum BarType
{
    None,
    Eating,
    Drinking,
    Using,
    Drawing,
}

public class PlayerUI : MonoBehaviour
{
    public static PlayerUI Instance{ get; private set; }

    public Slider chargeBar;
    public Image handleImage;
    public GameObject crosshair;


    public SerializableDictionary<BarType, Sprite> handleIcons;

    void Awake()
    {
        // Singleton pattern
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

    void Start()
    {
        GameEventManager.OnAimModeChanged += SetCrosshair;
    }

    void OnDestroy()
    {
        GameEventManager.OnAimModeChanged -= SetCrosshair;
    }

    public void SetCrosshair(bool isAiming)
    {
        crosshair.SetActive(isAiming);
    }

    public void ShowProgressBar(bool show, BarType type = BarType.None)
    {
        chargeBar.value = 0;
        handleImage.sprite = handleIcons[type];

        chargeBar.gameObject.SetActive(show);
    }

    public void UpdateProgressBar(float ratio)
    {
        chargeBar.value = Mathf.Clamp01(ratio);
    }
}
