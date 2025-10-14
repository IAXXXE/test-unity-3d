using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ObjectManager : MonoBehaviour
{
    public List<GameObject> objects;

    public Dropdown dropdown;

    public Transform ObjectContainer;

    public CameraOrbit cameraOrbit;

    public TextMeshProUGUI text;

    // Start is called before the first frame update
    void Start()
    {
        dropdown.ClearOptions();
        dropdown.AddOptions(objects.Select(r => r.gameObject.name).ToList());
    }

    public void LoadObject()
    {
        if (dropdown.value >= objects.Count)
            throw new UnityException("Error: selected object is out of range");

        var SelectedObject = objects[dropdown.value];

        // Clear children
        foreach (Transform child in ObjectContainer)
            GameObject.Destroy(child.gameObject);

        // Load new object in container and set to camera orbit
        cameraOrbit.target = Instantiate(SelectedObject, ObjectContainer).transform;

        if(dropdown.value == 0)
        {
            text.text = "Cut into 2 pieces";
        }
        else if(dropdown.value == 1)
        {
            text.text = "Cut into 4 pieces";
        }
        else if(dropdown.value == 2)
        {
            text.text = "Cut into 7 pieces";
        }
    }
}
