using UnityEngine;
using UnityEngine.UI;

public sealed class SegmentedToggleVisual : MonoBehaviour
{
	[SerializeField] private Toggle toggle;
	[SerializeField] private Graphic[] targetGraphics;
	[SerializeField] private Color selectedColor = new Color(0.48f, 0.44f, 0.58f, 1f);
	[SerializeField] private Color normalColor = new Color(0.5921569f, 0.2784314f, 1f, 1f);

	private void Reset()
	{
		toggle = GetComponent<Toggle>();
		targetGraphics = GetComponentsInChildren<Graphic>();
	}

	private void OnEnable()
	{
		if (!toggle)
		{
			toggle = GetComponent<Toggle>();
		}

		if (toggle)
		{
			toggle.onValueChanged.AddListener(UpdateVisual);
			UpdateVisual(toggle.isOn);
		}
	}

	private void OnDisable()
	{
		if (toggle)
		{
			toggle.onValueChanged.RemoveListener(UpdateVisual);
		}
	}

	private void OnValidate()
	{
		if (!toggle)
		{
			toggle = GetComponent<Toggle>();
		}

		UpdateVisual(toggle && toggle.isOn);
	}

	private void UpdateVisual(bool isSelected)
	{
		if (targetGraphics == null)
		{
			return;
		}

		var color = isSelected ? selectedColor : normalColor;

		foreach (var targetGraphic in targetGraphics)
		{
			if (targetGraphic)
			{
				targetGraphic.color = color;
			}
		}
	}
}
