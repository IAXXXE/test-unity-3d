using System.Collections;
using UnityEngine;

public class UIPanelManager : MonoBehaviour
{
    private GameObject currPanel;
    private GameObject cookBook;
    private GameObject cookPanel;
    private GameObject petInteractionPanel;

    private PlayerInputActions inputActions;

    // Start is called before the first frame update
    void Awake()
    {
        cookBook = transform.Find("BT_CookBook").gameObject;
        cookPanel = transform.Find("UI_CookPanel").gameObject;

        petInteractionPanel = transform.Find("UI_PetInteraction").gameObject;
    }

    void Start()
    {
        inputActions = GameInstance.Instance.inputActions;
        inputActions.Player.Cancel.started += ctx => HidePanel();

        GameEventManager.OnCooked += OnCookPanelShow;
        GameEventManager.OnPetInteraction += OnPetInteractionPanelShow;
    }

    void OnDestroy()
    {
        inputActions.Player.Cancel.started -= ctx => HidePanel();

        GameEventManager.OnCooked -= OnCookPanelShow;
        GameEventManager.OnPetInteraction -= OnPetInteractionPanelShow;
    }

    public void HidePanel()
    {
        Debug.Log("??? QQ");
        if(currPanel == null) return;
        currPanel?.gameObject.SetActive(false);
        currPanel = null;

        GameEventManager.TriggerUIHided();
    }

    private void OnCookPanelShow(CookLevel level)
    {
        GameEventManager.TriggerUIShowed();

        currPanel = cookPanel;

        cookPanel.GetComponent<CookPanel>().ShowPanel(level);
    }

    private void OnPetInteractionPanelShow(PetBase pet, InteractionOption[] options)
    {
        GameEventManager.TriggerUIShowed();

        currPanel = petInteractionPanel;
        petInteractionPanel.GetComponent<PetInteractionUI>().ShowPanel(pet, options);
    }
}
