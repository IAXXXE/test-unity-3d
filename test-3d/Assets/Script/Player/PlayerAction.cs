using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerWeapon))]
public class PlayerAction : MonoBehaviour
{
    [Header("Settings")]
    public float chargeThreshold = 0.8f; // 超过这个时间算蓄力
    public float maxChargeTime = 2.0f;

    [Header("References")]
    public PlayerWeapon weapon;
    public PlayerUI playerUI; // charge slider

    private PlayerInputActions inputActions;
    private bool isCharging;
    private float chargeTimer;

    private bool isUsing = false;
    private bool isLocked = false;

    // private bool inputLocked => UIInputLock.IsLocked; // 支持多层UI锁机制

    private void Awake()
    {
        weapon = GetComponent<PlayerWeapon>();
        inputActions = GameInstance.Instance.inputActions;

        inputActions.Player.Use.started += ctx => StartUse();
        inputActions.Player.Use.canceled += ctx => EndUse();

        GameEventManager.OnUIShowed += OnUIShowed;
        GameEventManager.OnUIHided += OnUIHided;
    }

    private void Destroy()
    {
        GameEventManager.OnUIShowed -= OnUIShowed;
        GameEventManager.OnUIHided -= OnUIHided;
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

    private void Update()
    {
        if (isCharging)
        {
            chargeTimer += Time.deltaTime;
            playerUI?.UpdateChargeBar(chargeTimer / maxChargeTime);
        }
    }

    private void StartUse()
    {
        if(isUsing) return;

        var item = weapon.GetHeldItem();
        if (item == null) return;
        
        if(item.itemType == ItemType.Food)
        {
            StartCoroutine(StartUseItem());
        }
        if (item.itemType == ItemType.Weapon)
        {
            
            isCharging = true;
            chargeTimer = 0f;
        }


        playerUI.ShowChargeBar(true);
    }

    private void EndUse()
    {
        if (!isCharging) return;
        isCharging = false;

        playerUI.ShowChargeBar(false);

        var item = weapon.GetHeldItem();
        if (item == null) return;

        float chargeRatio = Mathf.Clamp01(chargeTimer / maxChargeTime);

        if (item.itemType == ItemType.Weapon)
        {
            if (chargeTimer < chargeThreshold)
                weapon.LightAttack();
            else
                weapon.ChargedAttack(chargeRatio);
        }

    }

    private IEnumerator StartUseItem()
    {
        isUsing = true;
        var item = weapon.GetHeldItem();
        float sliceTime = 1f / 30f;
        float time = 0;
        playerUI.ShowChargeBar(true);
        while(time < item.useTime)
        {
            yield return new WaitForSeconds(sliceTime);
            time += sliceTime;
            playerUI.UpdateChargeBar(time / item.useTime);
        }
        playerUI.ShowChargeBar(false);
        weapon.UseItem();
        isUsing = false;
        yield break;
    }
}