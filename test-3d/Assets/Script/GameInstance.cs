using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor.Playables;
using UnityEngine;

public class GameInstance : MonoBehaviour
{
    protected static GameInstance instance;
    public static GameInstance Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject();
                obj.name = "SS_GameInstance";
                instance = obj.AddComponent<GameInstance>();
            }
            return instance;
        }
    }

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else if(instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // #if UNITY_EDITOR
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        // #endif

        Apply1080P();

        Init();
    }

    public void Apply1080P() => Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);

    #region Gameplay
    // public PlayerStat PlayerStat { get; private set; }
    public GameUtils Utils{ get; set; }
    // public InventoryManager Inventory { get; set; }

    public void Init()
    {
        Utils = transform.GetComponent<GameUtils>();
        // Inventory = transform.parent.Find("UI_Canvas/MN_Inventory").GetComponent<InventoryManager>();
    }

    #endregion
}
