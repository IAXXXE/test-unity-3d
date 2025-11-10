using EasyButtons;
using Unity.VisualScripting;
using UnityEngine;

public enum HandType
{
    HandL,
    HandR
}

public class PlayerWeapon : MonoBehaviour
{
    [Header("Hand Transforms")]
    public Transform HandL;
    public Transform HandR;

    [Header("References")]
    public PlayerUI playerUI;

    private ItemBase heldItemL;
    private ItemBase heldItemR;
    private GameObject currentItemObjectL;
    private GameObject currentItemObjectR;
    private ItemBehavior currentBehavior;

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

    private void HoldItem(ItemBase item, HandType type)
    {
        Transform itemParent = null;
        if(type == HandType.HandR)
        {
            if (heldItemR == item) return;
            ClearItem(HandType.HandR);
            heldItemR = item;
            if(item == null) return;
            // 实例化物品模型
            currentItemObjectR = Instantiate(item.data.worldPrefab, HandR);
            currentItemObjectR.transform.SetLocalPositionAndRotation(
                item.data.posOffset, 
                Quaternion.Euler(item.data.rotOffset)
            );
            currentItemObjectR.transform.localScale = item.data.scale;
            // 根据物品类型附加对应的行为组件
            AttachItemBehavior(item);
            Debug.Log($"[装备] {item.data.itemName} ({item.data.itemType})");
        }
        else
        {
            if (heldItemL == item) return;
            ClearItem(HandType.HandL);
            heldItemL = item;
            if(item == null) return;
            // 实例化物品模型
            currentItemObjectL = Instantiate(item.data.worldPrefab, HandL);

            var (leftHandPos, leftHandRot) = ConvertToLeftHand(item.data.posOffset, item.data.rotOffset);
            currentItemObjectL.transform.SetLocalPositionAndRotation(
                leftHandPos,
                Quaternion.Euler(leftHandRot)
            );
            currentItemObjectL.transform.localScale = item.data.scale;
        }

        if (item == null) return;
    }

    public (Vector3 position, Vector3 rotation) ConvertToLeftHand(Vector3 rightHandPos, Vector3 rightHandRot)
    {
        // 创建镜像矩阵（在X轴上镜像）
        Matrix4x4 mirrorMatrix = Matrix4x4.Scale(new Vector3(-1, 1, 1));
        
        // 变换位置
        Vector4 rightPos4 = new Vector4(rightHandPos.x, rightHandPos.y, rightHandPos.z, 1);
        Vector4 leftPos4 = mirrorMatrix * rightPos4;
        Vector3 leftHandPos = new Vector3(leftPos4.x, leftPos4.y, leftPos4.z);
        
        // 变换旋转（通过四元数）
        Quaternion rightRot = Quaternion.Euler(rightHandRot);
        Matrix4x4 rightMatrix = Matrix4x4.TRS(Vector3.zero, rightRot, Vector3.one);
        Matrix4x4 leftMatrix = mirrorMatrix * rightMatrix * mirrorMatrix;
        
        Quaternion leftRot = leftMatrix.rotation;
        Vector3 leftHandRot = leftRot.eulerAngles;
        
        return (leftHandPos, leftHandRot);
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
                    currentBehavior = currentItemObjectR.AddComponent<WeaponMeleeBehavior>();
                }
                else if (item.data.weaponType == WeaponType.Bow)
                {
                    currentBehavior = currentItemObjectR.AddComponent<WeaponBowBehavior>();
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

    public void ClearItem(HandType type)
    {
        if(type == HandType.HandR)
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

            currentItemObjectR = null;
            heldItemR = null;
        }
        else if(type == HandType.HandL)
        {
            if (HandL.childCount > 0)
            {
                GameUtils.Instance.ClearChildren(HandL);
            }
            currentItemObjectL = null;
            heldItemL = null;
        }
    }

    public ItemBase GetHeldItem(HandType type = HandType.HandR)
    {
        if(type == HandType.HandL)
        {
            return heldItemL;
        }
        else if(type == HandType.HandR)
        {
            return heldItemR;
        }

        return null;
    }
    public ItemData GetHeldItemData(HandType type = HandType.HandR)
    {
        if(type == HandType.HandL)
        {
            return heldItemL?.data;
        }
        else if(type == HandType.HandR)
        {
            return heldItemR?.data;
        }

        return null;
    }

    public ItemType GetHeldItemType(HandType type = HandType.HandR)
    {
        if(type == HandType.HandL)
        {
            if(heldItemL == null) return ItemType.None;
            return heldItemL.data.itemType;
        }
        else if(type == HandType.HandR)
        {
            if(heldItemR == null) return ItemType.None;
            return heldItemR.data.itemType;
        }

        return ItemType.None;
    }

    public ItemData GetArrowData()
    {
        return InventoryManager.Instance.GetItemData(100010);
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
