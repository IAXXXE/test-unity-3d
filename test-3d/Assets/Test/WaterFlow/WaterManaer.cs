using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WaterManaer : MonoBehaviour
{
    void OnTriggerStay(Collider collider)
    {
        if(collider.CompareTag("Player"))
        {
            Debug.Log("Player !!");
            // collider.GetComponent<CharacterController>().enabled = false;
            if(collider.TryGetComponent<CharacterBuoyancy>(out var cb))
            {
                cb.ApplyWaterPhysics();
            }
        }
    }

    // void OnTriggerExit(Collider collider)
    // {
    //     if(collider.CompareTag("Player"))
    //     {
    //         collider.GetComponent<CharacterController>().enabled = true;
    //     }
    // }

}
