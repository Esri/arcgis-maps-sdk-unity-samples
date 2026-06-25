// Copyright 2026 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public sealed class SliderValueLabel : MonoBehaviour
{
	[SerializeField] private string format = "0";
	[SerializeField] private Slider slider;
	[SerializeField] private TMP_Text valueText;

	private void Reset()
	{
		slider = GetComponent<Slider>();
	}

	private void OnEnable()
	{
		if (!slider)
		{
			slider = GetComponent<Slider>();
		}

		if (slider)
		{
			slider.onValueChanged.AddListener(UpdateLabel);
			UpdateLabel(slider.value);
		}
	}

	private void OnDisable()
	{
		if (slider)
		{
			slider.onValueChanged.RemoveListener(UpdateLabel);
		}
	}

	private void OnValidate()
	{
		if (!slider)
		{
			slider = GetComponent<Slider>();
		}

		UpdateLabel(slider ? slider.value : 0f);
	}

	private void UpdateLabel(float value)
	{
		if (valueText)
		{
			valueText.text = value.ToString(format, CultureInfo.InvariantCulture);
		}
	}
}
