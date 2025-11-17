using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableGhostPoint : InteractableBase
{
    public int inspirationPointIdx = 0;
    // Start is called before the first frame update
    void Start()
    {
        GameEventManager.OnSunrise += OnSunrise;
        GameEventManager.OnMidnight += OnMidnight;

        transform.parent.gameObject.SetActive(false);
    }

    private void OnSunrise()
    {
        transform.parent.gameObject.SetActive(false);
    }

    private void OnMidnight()
    {
        transform.parent.gameObject.SetActive(true);
    }

    void OnDestroy()
    {
        GameEventManager.OnSunrise -= OnSunrise;
        GameEventManager.OnMidnight -= OnMidnight;
    }

    public override void Interact(PlayerController player)
    {

        GameEventManager.TriggerInspirationPointInteract(transform.parent);
        
    }

}
