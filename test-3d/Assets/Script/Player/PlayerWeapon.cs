using EasyButtons;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    public Transform HandL;
    public Transform HandR;

    private Item heldItemL;
    private Item heldItemR;

    void Start()
    {
        GameEventManager.OnItemHeld += HoldItem;
    }

    private void HoldItem(Item item)
    {
        if(heldItemR == item) return;
        heldItemR = item;
        ClearItem();
        if(item == null) return;

        var itemGameObject = Instantiate(item.data.worldPrefab, HandR);
        itemGameObject.transform.SetLocalPositionAndRotation(item.data.posOffset, Quaternion.Euler(item.data.rotOffset));

        Debug.Log($"Weapon {item.data.name}");
    }

    public void ClearItem()
    {
        if(HandR.childCount != 0) GameInstance.Instance.Utils.ClearChildren(HandR);
        heldItemR = null;
    }

    public ItemData GetHeldItem()
    {
        if(heldItemR == null) return null;
        else return heldItemR.data;
    }

    public void LightAttack()
    {
        Debug.Log($"轻攻击 {heldItemR.data?.name}");
        // TODO: 播放动画、事件、特效
    }

    public void ChargedAttack(float charge)
    {
        Debug.Log($"蓄力攻击 {heldItemR.data?.name}，蓄力比={charge:F2}");
        // TODO: 播放蓄力攻击动画
    }

    public void UseItem()
    {
        Debug.Log("Use item");
        heldItemR?.Use();
        if(heldItemR.data.isConsumable) GameEventManager.TriggerHeldItemConsumed();
    }

    public void WaveItem()
    {
        // Play animation
    }
}
