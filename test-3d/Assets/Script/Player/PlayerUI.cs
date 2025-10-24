using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public Slider chargeBar;

    public void ShowChargeBar(bool show)
    {
        if (chargeBar)
        {
            chargeBar.gameObject.SetActive(show);
            chargeBar.value = 0;
        }
    }

    public void UpdateChargeBar(float ratio)
    {
        if (chargeBar)
            chargeBar.value = Mathf.Clamp01(ratio);
    }
}
