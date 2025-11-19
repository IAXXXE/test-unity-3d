using TMPro;
using UnityEngine;

// Tooltip组件
public class TooltipTrigger : MonoBehaviour, UnityEngine.EventSystems.IPointerEnterHandler, UnityEngine.EventSystems.IPointerExitHandler
{
    public string tooltipText;

    public void OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData)
    {
        TooltipSystem.Instance?.Show(tooltipText);
    }

    public void OnPointerExit(UnityEngine.EventSystems.PointerEventData eventData)
    {
        TooltipSystem.Instance?.Hide();
    }

}

// 简单的Tooltip系统
public class TooltipSystem : MonoBehaviour
{
    public static TooltipSystem Instance { get; private set; }
    public GameObject tooltipPanel;
    public TextMeshProUGUI tooltipText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        Hide();
    }

    public void Show(string text)
    {
        tooltipPanel.SetActive(true);
        tooltipText.text = text;
    }

    public void Hide()
    {
        tooltipPanel.SetActive(false);
    }
}
