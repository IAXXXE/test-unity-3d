using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PickupPrompt : MonoBehaviour
{
    public TextMeshProUGUI promptText; // assign UI Text
    public GameObject root; // the parent GameObject of UI prompt

    void Awake()
    {
        if (root != null) root.SetActive(false);
    }

    public void Show(string name)
    {
        if (root != null) root.SetActive(true);
        if (promptText != null) promptText.text = $"[E] Pick {name}";
    }

    public void Hide()
    {
        if (root != null) root.SetActive(false);
    }
}
