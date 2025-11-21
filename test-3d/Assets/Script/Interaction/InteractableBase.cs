using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class InteractableBase : MonoBehaviour, IInteractable
{
    [Header("Basic Info")]
    public string interactName = "Object";
    public float interactDistance = 3f;
    public bool showOutline = true;
    public InteractableType interactableType;

    protected Outline outlineComponent;

    protected virtual void Start()
    {
        outlineComponent = GetComponent<Outline>();
        if (outlineComponent == null && showOutline)
        {
            outlineComponent = gameObject.AddComponent<Outline>();
            outlineComponent.enabled = false;
        }
    }


    public virtual string GetInteractText()
    {
        return GetInteractTextByType();
    }

    protected virtual string GetInteractTextByType()
    {
        switch (interactableType)
        {
            case InteractableType.Pickup:
                return $"Pick {interactName}";
            case InteractableType.Gather:
                return $"Gather {interactName}";
            case InteractableType.GatherWithTool:
                return $"Gather {interactName}";
            case InteractableType.Destructible:
                return $"破坏 {interactName}";
            case InteractableType.Liftable:
                return $"举起 {interactName}";
            case InteractableType.Functional:
                return $"使用 {interactName}";
            case InteractableType.Button:
                return $"按下 {interactName}";
            case InteractableType.Container:
                return $"打开 {interactName}";
            case InteractableType.Dialogue:
                return $"对话 {interactName}";
            case InteractableType.Quest:
                return $"检查 {interactName}";
            default:
                return $"交互 {interactName}";
        }
    }

    public virtual bool CanInteract()
    {
        // return !isOnCooldown && CheckSpecificRequirements();
        return CheckSpecificRequirements();
    }

    protected virtual bool CheckSpecificRequirements()
    {
        switch (interactableType)
        {
            case InteractableType.GatherWithTool:
                return CheckToolRequirement();
            case InteractableType.Quest:
                return CheckQuestRequirement();
            default:
                return true;
        }
    }

    public abstract void Interact(PlayerController player);

    protected virtual void OnCooldownEnd() { }

    public virtual void SetHighlight(bool on)
    {
        if (outlineComponent != null) 
            outlineComponent.enabled = on && showOutline;
        else
        {
            outlineComponent = GetComponent<Outline>();
            if (outlineComponent == null) return;
            outlineComponent.enabled = on && showOutline;
        }
    }

    public string GetDisplayName()
    {
        return interactName;
    }

    // 特定类型检查方法
    protected virtual bool CheckToolRequirement()
    {
        // 在子类中实现具体工具检查
        return true;
    }

    protected virtual bool CheckQuestRequirement()
    {
        // 在子类中实现任务条件检查
        return true;
    }
}
