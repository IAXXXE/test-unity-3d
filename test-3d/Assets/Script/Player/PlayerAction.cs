using System.Collections;
using Newtonsoft.Json.Linq;
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

    private bool isMerging;
    private float mergeTime;

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

        // inputActions.Player.UseL.performed -= ctx => OnUsePressed();

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
        if(isMerging) return;

        var itemL = weapon.GetHeldItemData(HandType.HandL);
        var itemR = weapon.GetHeldItemData(HandType.HandR);
        if(itemL == null || itemR == null) return;
        Debug.Log($"try merge {weapon.GetHeldItemData(HandType.HandL)?.itemName} {weapon.GetHeldItemData(HandType.HandR)?.itemName}");
        // 合成表

        if(itemL.itemID == 100000 && itemR.itemID == 100000)
        {
            StartCoroutine(Merging(100004));
        }
        if(itemL.itemID == 100001 && itemR.itemID == 100001)
        {
            StartCoroutine(Merging(100005));
        }
        if (itemL.itemID == 100002 && itemR.itemID == 100002)
        {
            StartCoroutine(Merging(100006, false));
        }
        if(itemL.itemID == 100003 && itemR.itemID == 100003)
        {
            StartCoroutine(Merging(100006, false));
        }

        if((itemL.itemID == 100006 && itemR.itemID == 100001) || (itemL.itemID == 100001 && itemR.itemID == 100006))
        {
            StartCoroutine(Merging(100010));
        }
        if((itemL.itemID == 100004 && itemR.itemID == 100005) || (itemL.itemID == 100005 && itemR.itemID == 100004))
        {
            StartCoroutine(Merging(100011));
        }
        if((itemL.itemID == 100007 && itemR.itemID == 100001) || (itemL.itemID == 100001 && itemR.itemID == 100007))
        {
            StartCoroutine(Merging(100008));
        }
    }

    private IEnumerator Merging(int id, bool bothConsumed = true, float needTime = 1f)
    {
        mergeTime = 0;

        GameEventManager.TriggerPlayerMerge(true);
        PlayerUI.Instance.ShowProgressBar(true, BarType.Using);
        while (mergeTime < needTime)
        {
            yield return null;
            mergeTime += Time.deltaTime;
            PlayerUI.Instance.UpdateProgressBar(mergeTime / needTime);
        }

        GameEventManager.TriggerHeldItemConsumed();
        if(bothConsumed) GameEventManager.TriggerHeldItemConsumed(HandType.HandL);
        InventoryManager.Instance.AddItem(id);

        PlayerUI.Instance.ShowProgressBar(false);
        GameEventManager.TriggerPlayerMerge(false);
        yield break;
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