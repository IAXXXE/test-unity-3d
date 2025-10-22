using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    private GameObject cookBook;
    private GameObject cookPanel;

    // Start is called before the first frame update
    void Awake()
    {
        cookBook = transform.Find("BT_CookBook").gameObject;
        cookPanel = transform.Find("UI_CookPanel").gameObject;

        GameEventManager.OnCooked += OnCookPanelShow;
    }

    void OnDestroy()
    {
        GameEventManager.OnCooked += OnCookPanelShow;
    }

    private void OnCookPanelShow(CookLevel level)
    {
        cookPanel.GetComponent<CookPanel>().InitPanel(level);
        cookPanel.SetActive(true);
    }
}
