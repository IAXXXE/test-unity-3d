using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerWeapon))]
public class PlayerAction : MonoBehaviour
{
    [Header("References")]
    public PlayerWeapon weapon;
    public PlayerUI playerUI;

    private bool primaryPressed;
    private bool secondaryPressed;

    private PlayerInputActions inputActions;
    private bool isLocked = false;

    private void Awake()
    {
        weapon = GetComponent<PlayerWeapon>();
        inputActions = GameInstance.Instance.inputActions;

        // 主要操作（左键/ZL）
        inputActions.Player.UseL.started += ctx => OnPrimaryPressed();
        inputActions.Player.UseL.canceled += ctx => OnPrimaryReleased();

        // 次要操作（右键/ZR）
        inputActions.Player.UseR.started += ctx => OnSecondaryPressed();
        inputActions.Player.UseR.canceled += ctx => OnSecondaryReleased();

        // 使用物品
        inputActions.Player.UseL.performed += ctx => OnUsePressed();

        // 合并物品
        inputActions.Player.Merge.performed += ctx => OnMergePressed();

        // UI锁定
        GameEventManager.OnUIShowed += OnUIShowed;
        GameEventManager.OnUIHided += OnUIHided;
    }

    private void OnDestroy()
    {
        inputActions.Player.UseL.started -= ctx => OnPrimaryPressed();
        inputActions.Player.UseL.canceled -= ctx => OnPrimaryReleased();

        inputActions.Player.UseR.started -= ctx => OnSecondaryPressed();
        inputActions.Player.UseR.canceled -= ctx => OnSecondaryReleased();

        inputActions.Player.UseL.performed -= ctx => OnUsePressed();

        GameEventManager.OnUIShowed -= OnUIShowed;
        GameEventManager.OnUIHided -= OnUIHided;
    }

    private void Update()
    {
        if (isLocked) return;
        
        if(primaryPressed) weapon.OnPrimaryUpdate(Time.deltaTime);
        if(secondaryPressed) weapon.OnSecondaryUpdate(Time.deltaTime);
    }

    private void OnPrimaryPressed()
    {
        primaryPressed = true;
        if (isLocked) return;
        weapon.OnPrimaryStart();
    }

    private void OnPrimaryReleased()
    {
        primaryPressed = false;
        if (isLocked) return;
        weapon.OnPrimaryEnd();
    }

    private void OnSecondaryPressed()
    {
        secondaryPressed = true;
        if (isLocked) return;
        weapon.OnSecondaryStart();
    }

    private void OnSecondaryReleased()
    {
        secondaryPressed = false;
        if (isLocked) return;
        weapon.OnSecondaryEnd();
    }

    private void OnUsePressed()
    {
        if (isLocked) return;
        weapon.OnUse();
    }

    private void OnMergePressed()
    {
        var itemL = weapon.GetHeldItemData(HandType.HandL);
        var itemR = weapon.GetHeldItemData(HandType.HandR);
        Debug.Log($"try merge {weapon.GetHeldItemData(HandType.HandL).name} {weapon.GetHeldItemData(HandType.HandR).name}");
        // 合成表
        if(itemL == null || itemR == null) return;

            if(itemL.itemID == "W0001" && itemR.itemID == "W0001")
        {
            GameEventManager.TriggerHeldItemConsumed(HandType.HandL);
            GameEventManager.TriggerHeldItemConsumed();
            InventoryManager.Instance.AddItem("W0005");
        }
        if(itemL.itemID == "M0001" && itemR.itemID == "M0001")
        {
            GameEventManager.TriggerHeldItemConsumed(HandType.HandL);
            GameEventManager.TriggerHeldItemConsumed();
            InventoryManager.Instance.AddItem("M0003");
        }
        if(itemL.itemID == "M0002" && itemR.itemID == "M0002")
        {
            GameEventManager.TriggerHeldItemConsumed();
            InventoryManager.Instance.AddItem("W0004");
        }

        if((itemL.itemID == "M0003" && itemR.itemID == "W0005") || (itemL.itemID == "W0005" && itemR.itemID == "M0003"))
        {
            GameEventManager.TriggerHeldItemConsumed(HandType.HandL);
            GameEventManager.TriggerHeldItemConsumed();
            InventoryManager.Instance.AddItem("W0002");
        }

        if((itemL.itemID == "W0004" && itemR.itemID == "W0001") || (itemL.itemID == "W0001" && itemR.itemID == "W0004"))
        {
            GameEventManager.TriggerHeldItemConsumed(HandType.HandL);
            GameEventManager.TriggerHeldItemConsumed();
            InventoryManager.Instance.AddItem("W0003");
        }


    }

    private void OnUIShowed()
    {
        isLocked = true;
        inputActions.Disable();
    }

    private void OnUIHided()
    {
        isLocked = false;
        inputActions.Enable();
    }
}


// [RequireComponent(typeof(PlayerWeapon))]
// public class PlayerAction : MonoBehaviour
// {
//     [Header("Settings")]
//     public float chargeThreshold = 0.8f; // 超过这个时间算蓄力
//     public float maxChargeTime = 2.0f;

//     [Header("References")]
//     public PlayerWeapon weapon;
//     public PlayerUI playerUI; // charge slider

//     private PlayerInputActions inputActions;
//     private bool isCharging;
//     private float chargeTimer;

//     private bool isUsing = false;
//     private bool isLocked = false;

//     // private bool inputLocked => UIInputLock.IsLocked; // 支持多层UI锁机制

//     private void Awake()
//     {
//         weapon = GetComponent<PlayerWeapon>();
//         inputActions = GameInstance.Instance.inputActions;

//         inputActions.Player.Use.started += ctx => StartUse();
//         inputActions.Player.Use.canceled += ctx => EndUse();

//         GameEventManager.OnUIShowed += OnUIShowed;
//         GameEventManager.OnUIHided += OnUIHided;
//     }

//     private void Destroy()
//     {
//         GameEventManager.OnUIShowed -= OnUIShowed;
//         GameEventManager.OnUIHided -= OnUIHided;
//     }

//     private void OnUIShowed()
//     {
//         isLocked = true;
//         inputActions.Disable();
//     }

//     private void OnUIHided()
//     {
//         isLocked = false;
//         inputActions.Enable();
//     }

//     private void Update()
//     {
//         if (isCharging)
//         {
//             chargeTimer += Time.deltaTime;
//             playerUI?.UpdateChargeBar(chargeTimer / maxChargeTime);
//         }
//     }

//     private void StartUse()
//     {
//         if(isUsing) return;

//         var item = weapon.GetHeldItem();
//         if (item == null) return;
        
//         if(item.itemType == ItemType.Food)
//         {
//             StartCoroutine(StartUseItem());
//         }
//         if (item.itemType == ItemType.Weapon)
//         {
            
//             isCharging = true;
//             chargeTimer = 0f;
//         }


//         playerUI.ShowChargeBar(true);
//     }

//     private void EndUse()
//     {
//         if (!isCharging) return;
//         isCharging = false;

//         playerUI.ShowChargeBar(false);

//         var item = weapon.GetHeldItem();
//         if (item == null) return;

//         float chargeRatio = Mathf.Clamp01(chargeTimer / maxChargeTime);

//         if (item.itemType == ItemType.Weapon)
//         {
//             if (chargeTimer < chargeThreshold)
//                 weapon.LightAttack();
//             else
//                 weapon.ChargedAttack(chargeRatio);
//         }

//     }

//     private IEnumerator StartUseItem()
//     {
//         isUsing = true;
//         var item = weapon.GetHeldItem();
//         float sliceTime = 1f / 30f;
//         float time = 0;
//         playerUI.ShowChargeBar(true);
//         while(time < item.useTime)
//         {
//             yield return new WaitForSeconds(sliceTime);
//             time += sliceTime;
//             playerUI.UpdateChargeBar(time / item.useTime);
//         }
//         playerUI.ShowChargeBar(false);
//         weapon.UseItem();
//         isUsing = false;
//         yield break;
//     }
// }