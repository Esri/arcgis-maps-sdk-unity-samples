// Copyright 2026 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using UnityEngine;
using UnityEngine.UI;

public sealed class SegmentedToggleVisual : MonoBehaviour
{
	[SerializeField] private Sprite normalSprite;
	[SerializeField] private Sprite selectedSprite;
	[SerializeField] private Image[] targetImages;
	[SerializeField] private Toggle toggle;

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
