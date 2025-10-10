using UnityEngine;
// using cakeslice; // 引用 QuickOutline 命名空间

[RequireComponent(typeof(Outline))]
[RequireComponent(typeof(Rigidbody))]
public class PickupItem : MonoBehaviour, IInteractable
{
    private Outline outline;
    private bool isFocused;
    private PlayerInteraction player;

    void Awake()
    {
        outline = GetComponent<Outline>();
        outline.enabled = false;   
    }

    public void OnFocus()
    {
        outline.enabled = true;
        isFocused = true;
    }

    public void OnLoseFocus()
    {
        outline.enabled = false;
        isFocused = false;
    }

    public void OnInteract()
    {
        if (player == null)
            player = FindObjectOfType<PlayerInteraction>();

        Debug.Log("player : " + player == null );
        if (player != null)
        {
            player.PickUpObject(gameObject);
            outline.enabled = false;
        }
    }

    public string GetInteractText()
    {
        return "按 [E] 拾取";
    }
}
