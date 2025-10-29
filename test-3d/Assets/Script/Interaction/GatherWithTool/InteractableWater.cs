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

        float useTime = 0.2f * (itemContainer.GetMaxCapacity() - itemContainer.GetCapacity());
        var playerUI = GameInstance.Instance.PlayerStat.GetPlayerUI();
        playerUI?.ShowChargeBar(true);
        Debug.Log($"开始打水: {itemContainer.data.name}");

        while (scoopTime < useTime)
        {
            yield return null;
            scoopTime += Time.deltaTime;
            playerUI?.UpdateChargeBar(scoopTime / useTime);
        }

        playerUI?.ShowChargeBar(false);

        itemContainer.Scoop(FillingType.Water);
        Debug.Log($"打完水了: {itemContainer.data.name}");
        isScooping = false;
    }
}
