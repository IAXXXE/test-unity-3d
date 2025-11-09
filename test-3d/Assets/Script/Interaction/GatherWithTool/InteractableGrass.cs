using System.Collections;
using UnityEngine;

public class InteractableGrass : InteractableGatherWithTool
{
    public int itemId;
    public ToolProperty toolProperty;
    private bool isCutting;
    private float cutTime;

    public void Start()
    {
        toolProperty = ToolProperty.Sickle;
    }

    public override bool CanInteract()
    {
        var player = GameInstance.Instance.PlayerStat;

        if(player.GetHeldItemProperty() == null) return false;

        return player.GetHeldItemProperty().Contains(toolProperty) && !isCutting;
    }

    public override void Interact(PlayerController player)
    {
        if (isCutting) return;
        StartCoroutine(Cut());
    }

    public override string GetInteractText()
    {
        return $"Cut Grass";
    }

    public override void SetHighlight(bool on)
    {

    }

    public void DropItems()
    {
        InventoryManager.Instance.AddItem(itemId, 2);
        gameObject.SetActive(false);
    }

    private IEnumerator Cut()
    {
        isCutting = true;
        cutTime = 0f;

        PlayerIKController.Instance.SetAnimBool(AnimActionType.Gather, true);

        yield return new WaitForSeconds(0.2f);

        float useTime = 1f;
        PlayerUI.Instance.ShowProgressBar(true, BarType.Split);

        while (cutTime < useTime)
        {
            yield return null;
            cutTime += Time.deltaTime;
            PlayerUI.Instance.UpdateProgressBar(cutTime / useTime);
        }

        InventoryManager.Instance.AddItem(itemId, 2);
        PlayerUI.Instance.ShowProgressBar(false);
        PlayerIKController.Instance.SetAnimBool(AnimActionType.Gather, false);
        isCutting = false;

        gameObject.SetActive(false);
    }
}
