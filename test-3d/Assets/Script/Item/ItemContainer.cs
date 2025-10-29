using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FillingType
{
    Empty,
    Water,
    Seawater,
}

public class ItemContainer : ItemBase
{
    private FillingType fillingType;

    private int maxCap;
    private int currCap;

    public ItemContainer(ItemData data) : base(data)
    {
        maxCap = data.capacity;
        fillingType = FillingType.Empty;
        currCap = 0;
    }

    public bool IsEmpty() {return fillingType == FillingType.Empty || currCap == 0;}
    public FillingType GetFillingType(){ return fillingType;}
    public int GetCapacity(){ return currCap;}
    public int GetMaxCapacity(){ return maxCap;}

    public bool Scoop(FillingType type, int cap = 0)
    {
        if(fillingType != FillingType.Empty && fillingType != type) return false;

        fillingType = type;
        if(cap == 0) SetCurrentCap(maxCap);
        else SetCurrentCap(maxCap + cap);

        return true;
    }

    public void Drink()
    {
        SetCurrentCap(currCap - 1);
    }

    public void Pour()
    {
        fillingType = FillingType.Empty;
        SetCurrentCap(0);
    }

    private void SetCurrentCap(int value)
    {
        if(value <= 0) currCap = 0;
        else if(value >= maxCap) currCap = maxCap;
        else currCap = value;

        GameEventManager.TriggerItemUpdate();
    }
    
}
