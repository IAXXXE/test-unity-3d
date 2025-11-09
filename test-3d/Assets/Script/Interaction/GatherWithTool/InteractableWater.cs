using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableWater : InteractableGatherWithTool
{
    private bool isScooping;
    private float scoopTime;

    private ItemContainer itemContainer;

    void Start()
    {
        toolType = ItemType.Container;
    }

    public override bool CanInteract()
    {
        var player = GameInstance.Instance.PlayerStat;

        return player.GetHeldItemType() == toolType && !isScooping;
    }

    public override void Interact(PlayerController player)
    {
        if (isScooping) return;
        var weapon = player.GetComponent<PlayerWeapon>();
        itemContainer = weapon.GetHeldItem() as ItemContainer;
        itemContainer ??= weapon.GetHeldItem(HandType.HandL) as ItemContainer;
        if(itemContainer == null) Debug.Log("Null Item Container");
        StartCoroutine(ScoopWater());
    }

    public override string GetInteractText()
    {
        return $"Scoop Water";
    }

    public override void SetHighlight(bool on)
    {

    }

    private IEnumerator ScoopWater()
    {
        isScooping = true;
        scoopTime = 0f;

        PlayerIKController.Instance.SetAnimBool(AnimActionType.Gather, true);

        yield return new WaitForSeconds(0.2f);

        float useTime = 0.2f * (itemContainer.GetMaxCapacity() - itemContainer.GetCapacity());
        PlayerUI.Instance.ShowProgressBar(true, BarType.Using);
        Debug.Log($"开始打水: {itemContainer.data.itemName}");

        while (scoopTime < useTime)
        {
            yield return null;
            scoopTime += Time.deltaTime;
            PlayerUI.Instance.UpdateProgressBar(scoopTime / useTime);
        }

        PlayerUI.Instance.ShowProgressBar(false);
        PlayerIKController.Instance.SetAnimBool(AnimActionType.Gather, false);

        itemContainer.Scoop(FillingType.Water);
        Debug.Log($"打完水了: {itemContainer.data.itemName}");
        isScooping = false;
    }
}
