using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum CookLevel
{
    None,
    Simple,
    Normal,
    Expert
}

public class CookPanel : MonoBehaviour
{
    public CookLevel level = CookLevel.Simple;

    private CookLevel currentLevel = CookLevel.None;

    private int currIdx;
    private DishesData currDishes;

    private Transform dishesPanel;
    private Transform infoPanel;

    private Dictionary<int, DishesData> dishesDictionary = new Dictionary<int, DishesData>();
    // Start is called before the first frame update
    void Start()
    {
        // level = transform
        dishesPanel = transform.Find("_Menu/_DishesPanel");
        infoPanel = transform.Find("_Menu/_InfoPanel");

        infoPanel.gameObject.SetActive(false);
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

    public void InitPanel(CookLevel level)
    {
        if(currentLevel == level)
        {
            return;
        }

        switch(level)
        {
            case CookLevel.Simple:
                InitDishes();
                break;
            case CookLevel.Normal:
                break;
            case CookLevel.Expert:
                break;
        }
        currentLevel = level;
    }

    void InitDishes()
    {
        var dishesDatabase = DishesDatabase.Instance;
        for (int i = 0; i < dishesPanel.childCount; i++)
        {
            var slot = dishesPanel.GetChild(i);
            if(dishesDatabase.allDishes.Count > i)
            {
                var dishesData = dishesDatabase.allDishes[i];
                dishesDictionary.Add(i, dishesData);
                slot.Find("_Icon").GetComponent<Image>().sprite = dishesData.icon;
                slot.gameObject.SetActive(true);
            }
            else
            {
                slot.gameObject.SetActive(false);
            }
        }
    }


    public void OnDishesSelect(int idx)
    {
        currIdx = idx;
        currDishes = dishesDictionary[idx];
        ShowIngredients();

        dishesPanel.GetChild(idx).Find("_BackHover").gameObject.SetActive(false);
        dishesPanel.GetChild(idx).Find("_BackSelect").gameObject.SetActive(true);
    }

    public void ShowIngredients()
    {
        // UI
        infoPanel.Find("_DishesName").GetComponent<TextMeshProUGUI>().text = currDishes.name;

        int ingredientCount = currDishes.ingredients.Count;
        int idx = 0;
        foreach(Transform child in infoPanel.Find("_Ingredients"))
        {
            if(idx < ingredientCount)
            {
                var ingredient = currDishes.ingredients.Keys.ToList()[idx];
                child.Find("_Icon").GetComponent<Image>().sprite = ItemDatabase.Instance.GetItemData(ingredient).icon;
                child.Find("_Amount").GetComponent<TextMeshProUGUI>().text = "x " + currDishes.ingredients[ingredient];
            }
            child.gameObject.SetActive(idx < ingredientCount);
            idx++;
        }
        infoPanel.gameObject.SetActive(true);
    }
    
    public void Cooking()
    {
        var inventory = InventoryManager.Instance;
        foreach(var key in currDishes.ingredients.Keys)
        {
            if(!inventory.HasItem(key, currDishes.ingredients[key]))
            {
                //食材不足
                Debug.Log("No Ingredients !!!");
                return;
            }
        }
        foreach(var ingredient in currDishes.ingredients.Keys)
        {
            var count = currDishes.ingredients[ingredient];
            inventory.RemoveItem(ingredient, count);
        }
        inventory.AddItem(currDishes.id, 1);
        
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


