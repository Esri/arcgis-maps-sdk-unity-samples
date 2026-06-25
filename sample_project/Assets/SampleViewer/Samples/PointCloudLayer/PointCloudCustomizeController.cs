// Copyright 2026 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using Esri.GameEngine.Layers;
using Esri.GameEngine.Layers.PointCloud;
using Esri.GameEngine.Map.Symbology;
using Esri.Unity;
using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class PointCloudCustomizeController : MonoBehaviour
{
	[SerializeField] private PointCloudLayerDataLoader dataLoader;
	[SerializeField] private Slider pointSizeSlider;
	[SerializeField] private Slider pointsPerInchSlider;

	private const float MaxPointSize = 16f;
	private const float MinPointSize = 2f;
	private const float MinPointsPerInch = 1f;

	private ArcGISPointCloudLayer activeLayer;
	private ArcGISPointCloudRenderer activeRenderer;
	private ArcGISPointCloudSizeAlgorithm activeSizeAlgorithm;
	private bool subscribedToLoader;
	private bool subscribedToSliders;

	private void Reset()
	{
		FindReferences();
	}

	private void OnEnable()
	{
		FindReferences();
		Subscribe();
		ApplyCurrentValues();
	}

	private void OnDisable()
	{
		Unsubscribe();
	}

	private void FindReferences()
	{
		if (!dataLoader)
		{
			dataLoader = GetComponent<PointCloudLayerDataLoader>();
		}

		if (!pointSizeSlider)
		{
			pointSizeSlider = transform.Find("CustomizePanel/Slider_Point_Size")?.GetComponent<Slider>();
		}

		ConfigurePointSizeSlider();

		if (!pointsPerInchSlider)
		{
			pointsPerInchSlider = transform.Find("CustomizePanel/Slider_Point_Per_Inch")?.GetComponent<Slider>();
		}

		ConfigurePointsPerInchSlider();
	}

	private void ConfigurePointSizeSlider()
	{
		if (!pointSizeSlider)
		{
			return;
		}

		pointSizeSlider.minValue = MinPointSize;
		pointSizeSlider.maxValue = MaxPointSize;
		pointSizeSlider.wholeNumbers = true;

		var clampedValue = Mathf.Clamp(pointSizeSlider.value, MinPointSize, MaxPointSize);
		if (!Mathf.Approximately(pointSizeSlider.value, clampedValue))
		{
			pointSizeSlider.SetValueWithoutNotify(clampedValue);
		}
	}

	private void ConfigurePointsPerInchSlider()
	{
		if (!pointsPerInchSlider)
		{
			return;
		}

		pointsPerInchSlider.minValue = MinPointsPerInch;
		var clampedValue = Mathf.Max(MinPointsPerInch, pointsPerInchSlider.value);
		if (!Mathf.Approximately(pointsPerInchSlider.value, clampedValue))
		{
			pointsPerInchSlider.SetValueWithoutNotify(clampedValue);
		}
	}

	private void Subscribe()
	{
		if (!subscribedToLoader && dataLoader)
		{
			dataLoader.LayerLoaded += HandleLayerLoaded;
			subscribedToLoader = true;
		}

		if (!subscribedToSliders)
		{
			if (pointSizeSlider)
			{
				pointSizeSlider.onValueChanged.AddListener(HandleSliderChanged);
			}

			if (pointsPerInchSlider)
			{
				pointsPerInchSlider.onValueChanged.AddListener(HandleSliderChanged);
			}

			subscribedToSliders = true;
		}
	}

	private void Unsubscribe()
	{
		if (subscribedToLoader && dataLoader)
		{
			dataLoader.LayerLoaded -= HandleLayerLoaded;
		}

		if (subscribedToSliders)
		{
			if (pointSizeSlider)
			{
				pointSizeSlider.onValueChanged.RemoveListener(HandleSliderChanged);
			}

			if (pointsPerInchSlider)
			{
				pointsPerInchSlider.onValueChanged.RemoveListener(HandleSliderChanged);
			}
		}

		subscribedToLoader = false;
		subscribedToSliders = false;
	}

	private void HandleLayerLoaded(ArcGISPointCloudLayer _)
	{
		ApplyCurrentValues();
	}

	private void HandleSliderChanged(float _)
	{
		ApplyCurrentValues();
	}

	public void ApplyCurrentValues()
	{
		if (!Application.isPlaying || !dataLoader || dataLoader.LoadedLayer == null)
		{
			return;
		}

		ApplyToLayer(dataLoader.LoadedLayer);
	}

	private void ApplyToLayer(ArcGISPointCloudLayer layer)
	{
		var renderer = GetOrCreateRenderer(layer);
		if (renderer == null)
		{
			return;
		}

		if (pointsPerInchSlider)
		{
			renderer.PointsPerInch = Math.Max(MinPointsPerInch, pointsPerInchSlider.value);
		}

		if (pointSizeSlider)
		{
			var pointSize = Math.Max(MinPointSize, Math.Min(MaxPointSize, pointSizeSlider.value));
			activeSizeAlgorithm = new ArcGISPointCloudFixedSizeAlgorithm(pointSize, ArcGISSymbolSizeUnits.DIPs);
			renderer.SizeAlgorithm = activeSizeAlgorithm;
		}
	}

	private ArcGISPointCloudRenderer GetOrCreateRenderer(ArcGISPointCloudLayer layer)
	{
		var renderer = layer.Renderer;
		if (renderer != null)
		{
			activeLayer = layer;
			activeRenderer = renderer;
			return renderer;
		}

		if (activeLayer == layer && activeRenderer != null)
		{
			return activeRenderer;
		}

		var rgbAttributeName = FindRGBAttributeName(layer);
		if (!string.IsNullOrEmpty(rgbAttributeName))
		{
			renderer = new ArcGISPointCloudRGBRenderer(rgbAttributeName);
			layer.Renderer = renderer;
			activeLayer = layer;
			activeRenderer = renderer;
			return renderer;
		}

		Debug.LogWarning("Point cloud customize sliders need an explicit point cloud renderer, but no RGB attribute was found to create one.");
		return null;
	}

	private string FindRGBAttributeName(ArcGISPointCloudLayer layer)
	{
		var attributes = layer.Attributes;
		if (attributes == null || attributes.GetSize() == 0)
		{
			return null;
		}

		string firstColorLikeAttribute = null;

		for (ulong i = 0; i < attributes.GetSize(); i++)
		{
			var attribute = attributes.At(i);
			if (attribute == null)
			{
				continue;
			}

			var name = attribute.Name;
			if (string.Equals(name, "RGB", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(name, "RGBA", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(name, "COLOR", StringComparison.OrdinalIgnoreCase))
			{
				return name;
			}

			if (firstColorLikeAttribute == null && attribute.ValuesPerElement >= 3)
			{
				firstColorLikeAttribute = name;
			}
		}

		return firstColorLikeAttribute;
	}
}
