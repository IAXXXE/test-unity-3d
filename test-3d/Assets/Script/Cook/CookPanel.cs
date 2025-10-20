using System.Collections.Generic;
using TreeEditor;
using UnityEngine;

public enum CookLevel
{
    Simple,
    Normal,
    Expert
}

public class CookPanel : MonoBehaviour
{
    public CookLevel level = CookLevel.Simple;

    private CookLevel currentLevel;

    private int currIdx;

    private Transform dishesPanel;
    private Transform infoPanel;

    private Dictionary<int, DishesData> dishesDictionary = new Dictionary<int, DishesData>();
    // Start is called before the first frame update
    void Start()
    {
        // level = transform

        dishesPanel = transform.Find("_Menu/_DishesPanel");
        infoPanel = transform.Find("_Menu/_InfoPanel");

        GameEventManager.OnCooked += OnCookPanelShow;

        gameObject.SetActive(false);
    }

    void OnEnable()
    {
        GameEventManager.TriggerUIShowed();
    }

    void OnDisable()
    {
        GameEventManager.TriggerUIHided();
    }

    void OnDestroy()
    {
        GameEventManager.OnCooked += OnCookPanelShow;
    }

    private void OnCookPanelShow(int kitchenType)
    {
        currentLevel = (CookLevel)kitchenType;
        gameObject.SetActive(true);
    }

    private void InitPanel(CookLevel level)
    {
        if(currentLevel == level)
        {
            return;
        }

        switch(level)
        {
            case CookLevel.Simple:
                
                break;
            case CookLevel.Normal:
                break;
            case CookLevel.Expert:
                break;
        }
    }


    public void OnDishesSelect(int idx)
    {
        currIdx = idx;
        ShowIngredients(dishesDictionary[idx]);

        dishesPanel.GetChild(idx).Find("_BackHover").gameObject.SetActive(false);
        dishesPanel.GetChild(idx).Find("_BackSelect").gameObject.SetActive(true);
    }

    public void ShowIngredients(DishesData dishesData)
    {
        // UI
        infoPanel.gameObject.SetActive(true);
    }
    
    public void Cooking()
    {
        var inventory = InventoryManager.Instance;
        if(inventory.RemoveItem("C0002", 3))
        {
            inventory.AddItem("C0003", 1);
        }
    }

    public void OnEnterDishes(Transform transform)
    {
        Debug.Log("mouse Enter");
        if(transform.Find("_BackSelect").gameObject.activeSelf) return;
        transform.Find("_BackHover").gameObject.SetActive(true);
    }

    public void OnExitDishes(Transform transform)
    {
        if(transform.Find("_BackSelect").gameObject.activeSelf) return;
        transform.Find("_BackHover").gameObject.SetActive(false);
    }

}


