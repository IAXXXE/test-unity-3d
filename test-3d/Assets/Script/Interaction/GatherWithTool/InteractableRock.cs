using System.Collections;
using UnityEngine;

public class InteractableRock : InteractableGatherWithTool
{
    public int interactableTimes;
    public ToolProperty toolProperty;
    private bool isDigging;
    private float digTime;

    private ParticleSystem particleSystem;

    public void Start()
    {
        interactableTimes = 3;
        toolProperty = ToolProperty.Pickaxe;
    }

    public override bool CanInteract()
    {
        var player = GameInstance.Instance.PlayerStat;

        if(player.GetHeldItemProperty() == null) return false;

        return player.GetHeldItemProperty().Contains(toolProperty) && !isDigging;
    }

    public override void Interact(PlayerController player)
    {
        if (isDigging) return;
        StartCoroutine(Cut());
    }

    public override string GetInteractText()
    {
        return $"Digging Stones";
    }

    public override void SetHighlight(bool on)
    {

    }

    public void OnHit()
    {
        if(interactableTimes <= 0) return;
        particleSystem?.Play();
        interactableTimes--;
        if(interactableTimes <= 0)
        {
            DropItems();
        }
    }

    public void DropItems()
    {
        InventoryManager.Instance.AddItem(100002, 3);
        InventoryManager.Instance.AddItem(100003, 2);
        gameObject.SetActive(false);
    }

    private IEnumerator Cut()
    {
        isDigging = true;
        digTime = 0f;

        PlayerIKController.Instance.SetAnimBool(AnimActionType.Mining, true);

        yield return new WaitForSeconds(0.2f);

        float useTime = 2.5f;
        PlayerUI.Instance.ShowProgressBar(true, BarType.Pickaxe);

        while (digTime < useTime)
        {
            yield return null;
            digTime += Time.deltaTime;
            PlayerUI.Instance.UpdateProgressBar(digTime / useTime);
        }

        InventoryManager.Instance.AddItem(100002, 3);
        InventoryManager.Instance.AddItem(100003, 2);
        PlayerUI.Instance.ShowProgressBar(false);
        PlayerIKController.Instance.SetAnimBool(AnimActionType.Mining, false);
        isDigging = false;

        gameObject.SetActive(false);
    }
}
