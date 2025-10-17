using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterManaer : MonoBehaviour
{
    void OnTriggerStay(Collider collider)
    {
        if(collider.CompareTag("Player"))
        {
            if(collider.TryGetComponent<CharacterBuoyancy>(out var cb))
            {
                
            }
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if(collider.CompareTag("Player"))
        {
            ;
        }
    }

}
