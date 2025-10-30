using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameEventManager.TriggerTerrainGenerated(transform);
    }
}
