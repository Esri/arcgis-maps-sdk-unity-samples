using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	private Tooltip tooltip;

	private void Start()
	{
		GameObject prefab = Resources.Load<GameObject>("Prefabs/TooltipPanel");
		if (prefab == null)
		{
			Debug.LogError("Tooltip prefab could not be loaded. Ensure it is located in 'Resources/Prefabs'.");
			return;
		}

		GameObject tooltipGO = Instantiate(prefab, transform);
		tooltip = tooltipGO.GetComponent<Tooltip>();

		tooltip.HideTooltip();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		{
			tooltip.ShowTooltip();
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (tooltip != null)
		{
			tooltip.HideTooltip();
		}
	}
}