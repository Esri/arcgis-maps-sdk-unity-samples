using UnityEngine;

public class Tooltip : MonoBehaviour
{
	public void ShowTooltip()
	{
		gameObject.SetActive(true);
	}

	public void HideTooltip()
	{
		gameObject.SetActive(false);
	}
}