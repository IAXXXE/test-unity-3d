using EasyButtons;
using UnityEngine;

// ========== 5. 重构后的 PlayerWeapon ==========
public class PlayerWeapon : MonoBehaviour
{
    [Header("Hand Transforms")]
    public Transform HandL;
    public Transform HandR;

    [Header("References")]
    public PlayerUI playerUI;

    private ItemBase heldItemL;
    private ItemBase heldItemR;
    private GameObject currentItemObject;
    private ItemBehavior currentBehavior;

    // pos TEST:
    [Button]
    public void HoldItem(GameObject obj)
    {
        var holdObj = Instantiate(obj, HandR);
    }

    void Start()
    {
        GameInstance.Instance.PlayerStat.SetWeapon(this);

        GameEventManager.OnItemHeld += HoldItem;
    }

    void OnDestroy()
    {
        GameEventManager.OnItemHeld -= HoldItem;
    }

    private void HoldItem(ItemBase item)
    {
        if (heldItemR == item) return;
        
        ClearItem();
        heldItemR = item;
        
        if (item == null) return;

        // 实例化物品模型
        currentItemObject = Instantiate(item.data.worldPrefab, HandR);
        currentItemObject.transform.SetLocalPositionAndRotation(
            item.data.posOffset, 
            Quaternion.Euler(item.data.rotOffset)
        );

        // 根据物品类型附加对应的行为组件
        AttachItemBehavior(item);

        Debug.Log($"[装备] {item.data.name} ({item.data.itemType})");
    }

    private void AttachItemBehavior(ItemBase item)
    {
        if (currentBehavior != null)
        {
            currentBehavior.OnUnequipped();
            Destroy(currentBehavior);
        }

        switch (item.data.itemType)
        {
            case ItemType.Weapon:
                if (item.data.weaponType == WeaponType.Melee)
                {
                    currentBehavior = currentItemObject.AddComponent<WeaponMeleeBehavior>();
                }
                else if (item.data.weaponType == WeaponType.Bow)
                {
                    currentBehavior = currentItemObject.AddComponent<WeaponBowBehavior>();
                }
                break;

            case ItemType.Food:
            case ItemType.Potion:
                currentBehavior = gameObject.AddComponent<ConsumableBehavior>();
                break;
            case ItemType.Container:
                currentBehavior = gameObject.AddComponent<ContainerBehavior>();
                break;
        }

        if (currentBehavior != null)
        {
            currentBehavior.Initialize(item, this, playerUI);
            currentBehavior.OnEquipped();
        }
    }

    public void ClearItem()
    {
        if (currentBehavior != null)
        {
            currentBehavior.OnUnequipped();
            Destroy(currentBehavior);
            currentBehavior = null;
        }

        if (HandR.childCount > 0)
        {
            GameUtils.Instance.ClearChildren(HandR);
        }

        currentItemObject = null;
        heldItemR = null;
    }

    public ItemBase GetHeldItem() => heldItemR;
    public ItemData GetHeldItemData() => heldItemR?.data;

    public ItemType GetHeldItemType()
    {
        if(heldItemR == null) return ItemType.None;

        return heldItemR.data.itemType;
    }

    public ItemData GetArrowData()
    {
        return InventoryManager.Instance.GetItemData("W0003");
    }
    
    public ItemBehavior GetCurrentBehavior() => currentBehavior;

    // 供 PlayerAction 调用的接口
    public void OnPrimaryStart() => currentBehavior?.OnPrimaryStart();
    public void OnPrimaryEnd() => currentBehavior?.OnPrimaryEnd();
    public void OnPrimaryUpdate(float deltaTime) => currentBehavior?.OnPrimaryUpdate(deltaTime);
    
    public void OnSecondaryStart() => currentBehavior?.OnSecondaryStart();
    public void OnSecondaryEnd() => currentBehavior?.OnSecondaryEnd();
    public void OnSecondaryUpdate(float deltaTime) => currentBehavior?.OnSecondaryUpdate(deltaTime);
    
    public void OnUse() => currentBehavior?.OnUse();
}

// public class PlayerWeapon : MonoBehaviour
// {
//     public Transform HandL;
//     public Transform HandR;

//     private Item heldItemL;
//     private Item heldItemR;

//     void Start()
//     {
//         GameEventManager.OnItemHeld += HoldItem;
//     }

//     private void HoldItem(Item item)
//     {
//         if(heldItemR == item) return;
//         ClearItem();
//         heldItemR = item;
//         if(item == null) return;

//         var itemGameObject = Instantiate(item.data.worldPrefab, HandR);
//         itemGameObject.transform.SetLocalPositionAndRotation(item.data.posOffset, Quaternion.Euler(item.data.rotOffset));

//         Debug.Log($"Weapon {item.data.name}");
//     }

//     public void ClearItem()
//     {
//         if(HandR.childCount != 0) GameUtils.Instance.ClearChildren(HandR);
//         heldItemR = null;
//     }

//     public ItemData GetHeldItem()
//     {
//         if(heldItemR == null) return null;
//         else return heldItemR.data;
//     }

//     public void LightAttack()
//     {
//         Debug.Log($"轻攻击 {heldItemR.data?.name}");
//         // TODO: 播放动画、事件、特效
//     }

//     public void ChargedAttack(float charge)
//     {
//         Debug.Log($"蓄力攻击 {heldItemR.data?.name}，蓄力比={charge:F2}");
//         // TODO: 播放蓄力攻击动画
//     }

//     public void UseItem()
//     {
//         Debug.Log("Use item");
//         var isUsed = heldItemR.Use();
//         if(isUsed && heldItemR.data.isConsumable)
//         {
//             GameEventManager.TriggerHeldItemConsumed();
//         }
//     }

//     public void WaveItem()
//     {
//         // Play animation
//     }
// }
