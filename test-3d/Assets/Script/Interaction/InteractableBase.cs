using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class InteractableBase : MonoBehaviour, IInteractable
{
    [Header("Basic Info")]
    public string interactName = "Object";
    public float interactDistance = 3f;
    public bool showOutline = true;

    protected bool isOnCooldown = false;
    protected float cooldownTimer = 0f;

    protected Outline outlineComponent;

    [Header("Cooldown")]
    public float cooldown = 0f;

    protected virtual void Start()
    {
        outlineComponent = transform.GetComponent<Outline>();
    }

    protected virtual void Update()
    {
        if (isOnCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0)
            {
                isOnCooldown = false;
                OnCooldownEnd();
            }
        }
    }

    public virtual string GetInteractText()
    {
        return isOnCooldown ? $"{interactName}（冷却中）" : $"交互 {interactName}";
    }

    public virtual bool CanInteract()
    {
        return !isOnCooldown;
    }

    public abstract void Interact(PlayerController player);

    protected void StartCooldown()
    {
        if (cooldown > 0)
        {
            isOnCooldown = true;
            cooldownTimer = cooldown;
        }
    }

    protected virtual void OnCooldownEnd() { }

    public void SetHighlight(bool on)
    {
        if (outlineComponent != null) outlineComponent.enabled = on;
        else
        {
            // fallback: change material emission (simple)
            var rend = GetComponent<Renderer>();
            if (rend != null)
            {
                foreach (var m in rend.materials)
                {
                    if (on) m.EnableKeyword("_EMISSION");
                    else m.DisableKeyword("_EMISSION");
                }
            }
        }
    }

    public string GetDisplayName()
    {
        return null;
    }
}
