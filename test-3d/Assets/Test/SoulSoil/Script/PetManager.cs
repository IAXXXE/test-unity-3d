using UnityEngine;
using System.Collections.Generic;

public class PetManager : MonoBehaviour
{
    public static PetManager Instance;

    [Header("玩家引用")]
    public Transform player;
    
    [Header("宠物列表")]
    public List<PetBase> ownedPets = new List<PetBase>();
    
    [Header("当前激活宠物")]
    public PetBase activePet;
    
    [Header("互动设置")]
    public KeyCode interactKey = KeyCode.E;
    public float interactionCheckRadius = 5f;
    
    private PetBase nearbyPet;
    private bool isInInteractionMode = false;

    private void Awake()
    {
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
    
    private void Start()
    {
        // 让所有拥有的宠物跟随
        foreach (var pet in ownedPets)
        {
            if (pet != null)
            {
                pet.Follow(player);
            }
        }
    }
    
    private void Update()
    {
        CheckNearbyPets();
        HandleInteractionInput();
    }
    
    private void CheckNearbyPets()
    {
        if (isInInteractionMode) return;
        
        nearbyPet = null;
        float closestDistance = interactionCheckRadius;
        
        foreach (var pet in ownedPets)
        {
            if (pet != null && pet.gameObject.activeInHierarchy)
            {
                float distance = Vector3.Distance(player.position, pet.GetTransform().position);
                
                if (distance < closestDistance && pet.IsInInteractionRange(player))
                {
                    closestDistance = distance;
                    nearbyPet = pet;
                }
            }
        }
    }
    
    private void HandleInteractionInput()
    {
        if (Input.GetKeyDown(interactKey))
        {
            if (isInInteractionMode)
            {
                // 退出互动模式
                ExitInteractionMode();
            }
            else if (nearbyPet != null)
            {
                // 进入互动模式
                EnterInteractionMode(nearbyPet);
            }
        }
    }
    
    public void EnterInteractionMode(PetBase pet)
    {
        isInInteractionMode = true;
        activePet = pet;
        
        pet.EnterInteractionMode();
        
        // 禁用玩家移动
        DisablePlayerMovement();
        
        // 显示互动UI
        ShowInteractionUI(pet);
    }
    
    private void ExitInteractionMode()
    {
        if (activePet != null)
        {
            activePet.ExitInteractionMode();
        }
        
        isInInteractionMode = false;
        activePet = null;
        
        // 启用玩家移动
        EnablePlayerMovement();
        
        // 隐藏互动UI
        HideInteractionUI();
    }
    
    // 执行互动
    public void PerformInteraction(InteractionType interactionType)
    {
        if (activePet != null)
        {
            activePet.Interact(interactionType);
            
            // 刷新UI显示可用选项
            RefreshInteractionUI();
        }
    }
    
    // 召唤/收回宠物
    public void SummonPet(PetBase pet)
    {
        if (pet == null) return;
        
        pet.gameObject.SetActive(true);
        pet.Follow(player);
        
        if (!ownedPets.Contains(pet))
        {
            ownedPets.Add(pet);
        }
    }
    
    public void RecallPet(PetBase pet)
    {
        if (pet == null) return;
        
        pet.StopFollow();
        pet.gameObject.SetActive(false);
    }
    
    private void ShowInteractionUI(PetBase pet)
    {
        InteractionOption[] options = pet.GetAvailableInteractions();
        GameEventManager.TriggerPetInteraction(pet, options);
        // PetInteractionUI.Instance?.ShowInteractionPanel(pet, options);
    }
    
    private void HideInteractionUI()
    {
        PetInteractionUI.Instance?.HideInteractionPanel();
    }
    
    private void RefreshInteractionUI()
    {
        if (activePet != null)
        {
            InteractionOption[] options = activePet.GetAvailableInteractions();
            PetInteractionUI.Instance?.RefreshOptions(options);
            PetInteractionUI.Instance?.UpdateStatBars(activePet);
        }
    }
    
    private void DisablePlayerMovement()
    {
        GameEventManager.TriggerUIShowed();
        // 禁用玩家控制脚本
        var playerController = player.GetComponent<CharacterController>();
        if (playerController != null)
            playerController.enabled = false;
    }
    
    private void EnablePlayerMovement()
    {
        GameEventManager.TriggerUIHided();
        // 启用玩家控制脚本
        var playerController = player.GetComponent<CharacterController>();
        if (playerController != null)
            playerController.enabled = true;
    }
}
