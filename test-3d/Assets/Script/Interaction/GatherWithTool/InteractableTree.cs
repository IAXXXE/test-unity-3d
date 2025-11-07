using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableTree : InteractableGatherWithTool
{
    public int interactableTimes;
    public ToolProperty toolProperty;
    private bool isCutting;
    private float cutTime;

    private ParticleSystem particleSystem;

    void Start()
    {
        interactableTimes = 3;
        toolProperty = ToolProperty.Axe;

        particleSystem = transform.GetComponentInChildren<ParticleSystem>();
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
        return $"Cut Tree";
    }

    public override void SetHighlight(bool on)
    {

    }

    public void OnHit()
    {
        if(interactableTimes <= 0) return;
        particleSystem.Play();

        interactableTimes--;

        if(interactableTimes <= 0)
        {
            DropItems();
        }
    }

    public void DropItems()
    {
        InventoryManager.Instance.AddItem("M0004", 3);
        gameObject.SetActive(false);
    }

    private IEnumerator Cut()
    {
        isCutting = true;
        cutTime = 0f;

        PlayerIKController.Instance.SetAnimBool(AnimActionType.Mining, true);

        yield return new WaitForSeconds(0.2f);

        float useTime = 2.5f;
        PlayerUI.Instance.ShowProgressBar(true, BarType.Axe);

        while (cutTime < useTime)
        {
            yield return null;
            cutTime += Time.deltaTime;
            PlayerUI.Instance.UpdateProgressBar(cutTime / useTime);
        }

        InventoryManager.Instance.AddItem("M0004", 3);
        PlayerUI.Instance.ShowProgressBar(false);
        PlayerIKController.Instance.SetAnimBool(AnimActionType.Mining, false);
        isCutting = false;

        gameObject.SetActive(false);
    }
}
