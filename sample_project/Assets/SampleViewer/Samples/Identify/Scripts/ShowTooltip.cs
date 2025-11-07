using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ShowTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private GameObject toolTipPrefab;
    private GameObject toolTip;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (toolTip != null)
        {
            toolTip.GetComponent<RectTransform>().position = Mouse.current.position.ReadValue();
            return;
        }

        toolTip = Instantiate(toolTipPrefab);
        toolTip.transform.SetParent(text.gameObject.GetComponentInParent<Canvas>().transform);
        toolTip.GetComponentInChildren<TextMeshProUGUI>().text = text.text;
        toolTip.GetComponent<RectTransform>().position = Mouse.current.position.ReadValue();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Destroy(toolTip);
    }
}
