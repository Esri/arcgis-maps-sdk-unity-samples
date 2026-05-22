using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public sealed class SliderValueLabel : MonoBehaviour
{
	[SerializeField] private Slider slider;
	[SerializeField] private TMP_Text valueText;
	[SerializeField] private string format = "0";

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
