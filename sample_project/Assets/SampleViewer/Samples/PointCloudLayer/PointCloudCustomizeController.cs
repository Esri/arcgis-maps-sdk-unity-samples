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

	private bool subscribedToLoader;
	private bool subscribedToSliders;
	private ArcGISPointCloudLayer activeLayer;
	private ArcGISPointCloudRenderer activeRenderer;
	private ArcGISPointCloudSizeAlgorithm activeSizeAlgorithm;

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

		if (!pointsPerInchSlider)
		{
			pointsPerInchSlider = transform.Find("CustomizePanel/Slider_Point_Per_Inch")?.GetComponent<Slider>();
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
			renderer.PointsPerInch = Math.Max(0d, pointsPerInchSlider.value);
		}

		if (pointSizeSlider)
		{
			activeSizeAlgorithm = new ArcGISPointCloudFixedSizeAlgorithm(Math.Max(0d, pointSizeSlider.value), ArcGISSymbolSizeUnits.DIPs);
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
