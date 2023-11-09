using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Tooltip : MonoBehaviour
{
    public TMP_Text tooltipText;

    public void ShowTooltip(TMP_Text text)
    {
        tooltipText = text;
        gameObject.SetActive(true);
    }

    public void HideTooltip()
    {
        gameObject.SetActive(false);
    }
}
