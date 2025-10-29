using UnityEngine;

public abstract class ItemBehavior : MonoBehaviour
{
    protected ItemBase item;
    protected ItemData itemData;
    protected PlayerWeapon playerWeapon;
    protected PlayerUI playerUI;

    public virtual void Initialize(ItemBase item, PlayerWeapon weapon, PlayerUI ui)
    {
        this.item = item;
        itemData = item.data;
        playerWeapon = weapon;
        playerUI = ui;
    }

    // 主要操作（左键/ZL）
    public abstract void OnPrimaryStart();
    public abstract void OnPrimaryEnd();
    public abstract void OnPrimaryUpdate(float deltaTime);

    // 次要操作（右键/ZR）
    public abstract void OnSecondaryStart();
    public abstract void OnSecondaryEnd();
    public abstract void OnSecondaryUpdate(float deltaTime);

    // 使用物品（E键等）
    public abstract void OnUse();

    public virtual void OnEquipped() { }
    public virtual void OnUnequipped() { }
}