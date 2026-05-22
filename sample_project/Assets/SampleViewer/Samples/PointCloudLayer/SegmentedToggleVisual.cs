using UnityEngine;
using UnityEngine.UI;

public sealed class SegmentedToggleVisual : MonoBehaviour
{
	[SerializeField] private Toggle toggle;
	[SerializeField] private Image[] targetImages;
	[SerializeField] private Sprite selectedSprite;
	[SerializeField] private Sprite normalSprite;

	private void Reset()
	{
		toggle = GetComponent<Toggle>();
		targetImages = GetComponentsInChildren<Image>();
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
		if (targetImages == null)
		{
			return;
		}

		var sprite = isSelected ? selectedSprite : normalSprite;
		if (!sprite)
		{
			return;
		}

		foreach (var targetImage in targetImages)
		{
			if (targetImage)
			{
				targetImage.sprite = sprite;
				targetImage.color = Color.white;
			}
		}
	}
}
