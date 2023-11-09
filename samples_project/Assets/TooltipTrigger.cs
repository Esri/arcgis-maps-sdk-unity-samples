using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TMP_Text tooltipText;
    private Tooltip tooltip;

    private void Start()
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/TooltipPanel");
        GameObject tooltipGO = Instantiate(prefab, transform);
        Tooltip tooltipInstance = tooltipGO.GetComponent<Tooltip>();
        tooltip.HideTooltip();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        tooltip.ShowTooltip(tooltipText);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltip.HideTooltip();
    }
}
