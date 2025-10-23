using System.Collections;
using System.Collections.Generic;
using EasyButtons;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    public Transform HandL;
    public Transform HandR;

    private ItemData holdDataL;
    private ItemData holdDataR;

    public GameObject obj;

    // private 

    void Start()
    {
        GameEventManager.OnItemHeld += HoldItem;
    }

    private void HoldItem(ItemData itemData)
    {
        if(holdDataR == itemData) return;
        holdDataR = itemData;
        var item = Instantiate(itemData.worldPrefab, HandR);
        item.transform.SetLocalPositionAndRotation(itemData.posOffset, Quaternion.Euler(itemData.rotOffset));

        Debug.Log($"Weapon {itemData.name}");
    }

    [Button]
    public void HoldItemLeft()
    {
        Instantiate(obj, HandR);
    }
}
