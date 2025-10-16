using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MeshDeformerInput : MonoBehaviour
{
    public float force = 10f;

    public float forceOffest = 0.1f;

    void Update()
    {
        if(Input.GetMouseButton(0))
        {
            HandleInput();
        }
    }

    void HandleInput()
    {
        Ray inputray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if(Physics.Raycast(inputray, out hit))
        {
            MeshDeformer deformer = hit.collider.GetComponent<MeshDeformer>();
            if(deformer)
            {
                Vector3 point = hit.point;
                point += hit.normal * forceOffest;
                deformer.AddDeformingForce(point, force);
            }
        }
    }


}
