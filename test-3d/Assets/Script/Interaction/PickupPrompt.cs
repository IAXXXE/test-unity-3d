using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PickupPrompt : MonoBehaviour
{
    public TextMeshProUGUI promptText; // assign UI Text
    public GameObject root; // the parent GameObject of UI prompt

    void Awake()
    {
        
    }

    public void Show(string name)
    {
        promptText.text = name;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
