using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUtils : MonoBehaviour
{
    protected static GameUtils instance;
    public static GameUtils Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject();
                obj.name = "SS_GameUtils";
                instance = obj.AddComponent<GameUtils>();
            }
            return instance;
        }
    }

    public void ClearChildren(Transform parent)
    {
        if (parent == null) return;

        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);
            if (child != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }
    }

    public void DestroyGameObject(GameObject obj)
    {
        Destroy(obj);
    }

    // Quaternion.Euler(Vector3)

}

