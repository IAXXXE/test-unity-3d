using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PetInteractionUI : MonoBehaviour
{
    public static PetInteractionUI Instance { get; private set; }
    
    [Header("UI引用")]
    public GameObject promptPanel;
    public TextMeshProUGUI promptText;
    
    public GameObject interactionPanel;
    public TextMeshProUGUI petNameText;
    public Transform optionsContainer;
    public GameObject optionButtonPrefab;
    
    [Header("宠物信息显示")]
    public GameObject petInfoPanel;
    public Image petIcon;
    public TextMeshProUGUI petTypeText;
    public Slider[] statBars;  // 不同类型宠物的状态条
    
    public PetManager petManager;
    private List<GameObject> activeButtons = new List<GameObject>();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {   
        HidePrompt();
        HideInteractionPanel();
    }
    
    // 显示互动提示
    public void ShowPrompt(string text)
    {
        if (promptPanel != null)
        {
            promptPanel.SetActive(true);
            promptText.text = text;
        }
    }
    
    public void HidePrompt()
    {
        if (promptPanel != null)
        {
            promptPanel.SetActive(false);
        }
    }
    
    // 显示互动面板
    public void ShowInteractionPanel(PetBase pet, InteractionOption[] options)
    {
        if (interactionPanel != null)
        {
            interactionPanel.SetActive(true);
            petNameText.text = pet.GetPetName();
            
            // 显示宠物信息
            UpdatePetInfo(pet);
            
            // 创建互动按钮
            CreateOptionButtons(options);
        }
    }
    
    public void HideInteractionPanel()
    {
        if (interactionPanel != null)
        {
            interactionPanel.SetActive(false);
            ClearOptionButtons();
        }
    }
    
    // 刷新选项
    public void RefreshOptions(InteractionOption[] options)
    {
        ClearOptionButtons();
        CreateOptionButtons(options);
    }
    
    private void CreateOptionButtons(InteractionOption[] options)
    {
        foreach (var option in options)
        {
            GameObject buttonObj = Instantiate(optionButtonPrefab, optionsContainer);
            activeButtons.Add(buttonObj);
            
            Button button = buttonObj.GetComponent<Button>();
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            
            if (buttonText != null)
            {
                buttonText.text = option.displayName;
            }
            
            // 设置按钮状态
            button.interactable = option.isAvailable;
            
            // 如果不可用，显示原因
            if (!option.isAvailable)
            {
            var tooltip = buttonObj.AddComponent<TooltipTrigger>();
            tooltip.tooltipText = option.unavailableReason;
            }

            // 添加点击事件
            InteractionType type = option.type;
            button.onClick.AddListener(() => OnInteractionButtonClicked(type));
            
            // 设置图标
            if (option.icon != null)
            {
                Image icon = buttonObj.transform.Find("Icon")?.GetComponent<Image>();
                if (icon != null)
                {
                    icon.sprite = option.icon;
                }
            }
        }
    }

    private void ClearOptionButtons()
    {
        foreach (var button in activeButtons)
        {
            Destroy(button);
        }
        activeButtons.Clear();
    }

    private void OnInteractionButtonClicked(InteractionType type)
    {
        petManager?.PerformInteraction(type);
    }

    private void UpdatePetInfo(PetBase pet)
    {
        if (petInfoPanel == null) return;
        
        petInfoPanel.SetActive(true);
        
        // 显示宠物类型
        if (petTypeText != null)
        {
            petTypeText.text = pet.GetPetType().ToString();
        }
        
        // 根据宠物类型显示不同的状态条
        UpdateStatBars(pet);
    }

    private void UpdateStatBars(PetBase pet)
    {
        // 隐藏所有状态条
        foreach (var bar in statBars)
        {
            if (bar != null)
                bar.gameObject.SetActive(false);
        }
        
        // 根据宠物类型显示对应状态
        if (pet is CreaturePet creaturePet)
        {
            ShowCreatureStats(creaturePet);
        }
        else if (pet is MechanicalPet mechanicalPet)
        {
            ShowMechanicalStats(mechanicalPet);
        }
        else if (pet is ElementalPet elementalPet)
        {
            ShowElementalStats(elementalPet);
        }
    }

    private void ShowCreatureStats(CreaturePet pet)
    {
        if (statBars.Length >= 3)
        {
            // 饥饿度
            statBars[0].gameObject.SetActive(true);
            statBars[0].value = (float)pet.hunger / pet.maxHunger;
            SetBarLabel(statBars[0], "饥饿度", pet.hunger, pet.maxHunger);
            
            // 亲密度
            statBars[1].gameObject.SetActive(true);
            statBars[1].value = (float)pet.affection / pet.maxAffection;
            SetBarLabel(statBars[1], "亲密度", pet.affection, pet.maxAffection);
            
            // 精力
            statBars[2].gameObject.SetActive(true);
            statBars[2].value = (float)pet.energy / pet.maxEnergy;
            SetBarLabel(statBars[2], "精力", pet.energy, pet.maxEnergy);
        }
    }

    private void ShowMechanicalStats(MechanicalPet pet)
    {
        if (statBars.Length >= 2)
        {
            // 电量
            statBars[0].gameObject.SetActive(true);
            statBars[0].value = pet.battery / pet.maxBattery;
            SetBarLabel(statBars[0], "电量", (int)pet.battery, (int)pet.maxBattery);
            
            // 耐久度
            statBars[1].gameObject.SetActive(true);
            statBars[1].value = pet.durability / pet.maxDurability;
            SetBarLabel(statBars[1], "耐久度", (int)pet.durability, (int)pet.maxDurability);
        }
    }

    private void ShowElementalStats(ElementalPet pet)
    {
        if (statBars.Length >= 1)
        {
            // 元素能量
            statBars[0].gameObject.SetActive(true);
            statBars[0].value = pet.elementalEnergy / pet.maxElementalEnergy;
            SetBarLabel(statBars[0], "元素能量", (int)pet.elementalEnergy, (int)pet.maxElementalEnergy);
        }
    }

    private void SetBarLabel(Slider slider, string label, int current, int max)
    {
        TextMeshProUGUI labelText = slider.GetComponentInChildren<TextMeshProUGUI>();
        if (labelText != null)
        {
            labelText.text = $"{label}: {current}/{max}";
        }
    }

}
