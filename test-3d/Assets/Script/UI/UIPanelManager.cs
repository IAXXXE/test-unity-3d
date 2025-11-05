using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPanelManager : MonoBehaviour
{
    private GameObject currPanel;
    private GameObject cookBook;
    private GameObject cookPanel;

    private PlayerInputActions inputActions;

    // Start is called before the first frame update
    void Awake()
    {
        cookBook = transform.Find("BT_CookBook").gameObject;
        cookPanel = transform.Find("UI_CookPanel").gameObject;

        GameEventManager.OnCooked += OnCookPanelShow;
        
    }

    void Start()
    {
        inputActions = GameInstance.Instance.inputActions;
        inputActions.Player.Cancel.started += ctx => HidePanel();
    }

    void OnDestroy()
    {
        GameEventManager.OnCooked += OnCookPanelShow;
    }

    private void HidePanel()
    {
        currPanel.gameObject.SetActive(false);

        GameEventManager.TriggerUIHided();
    }

    private void OnCookPanelShow(CookLevel level)
    {
        currPanel = cookPanel;

        cookPanel.GetComponent<CookPanel>().InitPanel(level);
        cookPanel.SetActive(true);
    }
}
