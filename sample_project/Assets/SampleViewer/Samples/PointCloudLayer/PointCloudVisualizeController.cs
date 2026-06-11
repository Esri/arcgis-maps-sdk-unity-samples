using Esri.GameEngine.Layers;
using Esri.GameEngine.Layers.PointCloud;
using Esri.Standard;
using Esri.Unity;
using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class PointCloudVisualizeController : MonoBehaviour
{
	private enum RendererChoice
	{
		RGB,
		Class,
		Elevation,
		Intensity
	}

	private struct AvailableAttributes
	{
		public string RGB;
		public string ClassCode;
		public string Elevation;
		public string Intensity;
	}

	[SerializeField] private PointCloudLayerDataLoader dataLoader;
	[SerializeField] private PointCloudCustomizeController customizeController;
	[SerializeField] private Toggle colorModulationToggle;
	[SerializeField] private GameObject colorModulationLabel;
	[SerializeField] private GameObject colorModulationDivider;
	[SerializeField] private Toggle rgbToggle;
	[SerializeField] private Toggle classToggle;
	[SerializeField] private Toggle elevationToggle;
	[SerializeField] private Toggle intensityToggle;

	private const double ElevationLow = -1.5d;
	private const double ElevationMid = 1.5d;
	private const double ElevationHigh = 3.5d;
	private const double IntensityLow = 10385d;
	private const double IntensityMid = 38032d;
	private const double IntensityHigh = 65680d;

	private bool subscribedToLoader;
	private bool subscribedToToggles;
	private bool suppressToggleEvents;
	private AvailableAttributes availableAttributes;
	private ArcGISPointCloudLayer activeLayer;
	private ArcGISPointCloudRenderer activeRenderer;
	private ArcGISCollection<ArcGISPointCloudColorStop> activeStopCollection;
	private ArcGISCollection<ArcGISPointCloudColorUniqueValue> activeUniqueValueCollection;
	private ArcGISCollection<string>[] activeUniqueValueGroups;
	private ArcGISPointCloudColorStop[] activeStops;
	private ArcGISPointCloudColorUniqueValue[] activeUniqueValues;
	private ArcGISRGBColor[] activeColors;
	private ArcGISPointCloudColorModulation activeColorModulation;

	private void Reset()
	{
		FindReferences();
	}

	private void OnEnable()
	{
		FindReferences();
		Subscribe();

		if (Application.isPlaying && dataLoader && dataLoader.LoadedLayer != null)
		{
			HandleLayerLoaded(dataLoader.LoadedLayer);
		}
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

		if (!customizeController)
		{
			customizeController = GetComponent<PointCloudCustomizeController>();
		}

		if (!colorModulationToggle)
		{
			colorModulationToggle = transform.Find("VisualizePanel/Checkbox_ColorModulation")?.GetComponent<Toggle>();
		}

		if (!colorModulationLabel)
		{
			colorModulationLabel = transform.Find("VisualizePanel/Text_ColorModulation")?.gameObject;
		}

		if (!colorModulationDivider)
		{
			colorModulationDivider = transform.Find("VisualizePanel/VisualizeDivider")?.gameObject;
		}

		if (!rgbToggle)
		{
			rgbToggle = transform.Find("VisualizePanel/Radio_RGB")?.GetComponent<Toggle>();
		}

		if (!classToggle)
		{
			classToggle = transform.Find("VisualizePanel/Radio_Class")?.GetComponent<Toggle>();
		}

		if (!elevationToggle)
		{
			elevationToggle = transform.Find("VisualizePanel/Radio_Elevation")?.GetComponent<Toggle>();
		}

		if (!intensityToggle)
		{
			intensityToggle = transform.Find("VisualizePanel/Radio_Intensity")?.GetComponent<Toggle>();
		}
	}

	private void Subscribe()
	{
		if (!subscribedToLoader && dataLoader)
		{
			dataLoader.LayerLoaded += HandleLayerLoaded;
			subscribedToLoader = true;
		}

		if (subscribedToToggles)
		{
			return;
		}

		if (colorModulationToggle)
		{
			colorModulationToggle.onValueChanged.AddListener(HandleColorModulationChanged);
		}

		if (rgbToggle)
		{
			rgbToggle.onValueChanged.AddListener(HandleRendererToggleChanged);
		}

		if (classToggle)
		{
			classToggle.onValueChanged.AddListener(HandleRendererToggleChanged);
		}

		if (elevationToggle)
		{
			elevationToggle.onValueChanged.AddListener(HandleRendererToggleChanged);
		}

		if (intensityToggle)
		{
			intensityToggle.onValueChanged.AddListener(HandleRendererToggleChanged);
		}

		subscribedToToggles = true;
	}

	private void Unsubscribe()
	{
		if (subscribedToLoader && dataLoader)
		{
			dataLoader.LayerLoaded -= HandleLayerLoaded;
		}

		if (subscribedToToggles)
		{
			if (colorModulationToggle)
			{
				colorModulationToggle.onValueChanged.RemoveListener(HandleColorModulationChanged);
			}

			if (rgbToggle)
			{
				rgbToggle.onValueChanged.RemoveListener(HandleRendererToggleChanged);
			}

			if (classToggle)
			{
				classToggle.onValueChanged.RemoveListener(HandleRendererToggleChanged);
			}

			if (elevationToggle)
			{
				elevationToggle.onValueChanged.RemoveListener(HandleRendererToggleChanged);
			}

			if (intensityToggle)
			{
				intensityToggle.onValueChanged.RemoveListener(HandleRendererToggleChanged);
			}
		}

		subscribedToLoader = false;
		subscribedToToggles = false;
	}

	private void HandleLayerLoaded(ArcGISPointCloudLayer layer)
	{
		activeLayer = layer;
		activeRenderer = null;
		availableAttributes = GetAvailableAttributes(layer);

		ApplyAvailability();
		EnsureAvailableRendererSelected();
		ApplySelectedRenderer();
	}

	private void HandleRendererToggleChanged(bool isOn)
	{
		if (suppressToggleEvents || !isOn)
		{
			return;
		}

		ApplySelectedRenderer();
	}

	private void HandleColorModulationChanged(bool _)
	{
		if (suppressToggleEvents)
		{
			return;
		}

		ApplyColorModulation(activeRenderer);
		if (activeLayer != null && activeRenderer != null)
		{
			activeLayer.Renderer = activeRenderer;
		}
	}

	private void ApplyAvailability()
	{
		var hasIntensity = !string.IsNullOrEmpty(availableAttributes.Intensity);

		SetToggleVisible(rgbToggle, !string.IsNullOrEmpty(availableAttributes.RGB));
		SetToggleVisible(classToggle, !string.IsNullOrEmpty(availableAttributes.ClassCode));
		SetToggleVisible(elevationToggle, !string.IsNullOrEmpty(availableAttributes.Elevation));
		SetToggleVisible(intensityToggle, hasIntensity);
		SetFeatureVisible(colorModulationToggle ? colorModulationToggle.gameObject : null, hasIntensity);
		SetFeatureVisible(colorModulationLabel, hasIntensity);
		SetFeatureVisible(colorModulationDivider, hasIntensity);

		if (!hasIntensity && colorModulationToggle && colorModulationToggle.isOn)
		{
			suppressToggleEvents = true;
			colorModulationToggle.isOn = false;
			suppressToggleEvents = false;
		}
	}

	private void EnsureAvailableRendererSelected()
	{
		if (GetSelectedChoice() != null)
		{
			return;
		}

		suppressToggleEvents = true;

		if (rgbToggle && rgbToggle.gameObject.activeSelf)
		{
			rgbToggle.isOn = true;
		}
		else if (classToggle && classToggle.gameObject.activeSelf)
		{
			classToggle.isOn = true;
		}
		else if (elevationToggle && elevationToggle.gameObject.activeSelf)
		{
			elevationToggle.isOn = true;
		}
		else if (intensityToggle && intensityToggle.gameObject.activeSelf)
		{
			intensityToggle.isOn = true;
		}

		suppressToggleEvents = false;
	}

	private void ApplySelectedRenderer()
	{
		if (!Application.isPlaying || activeLayer == null)
		{
			return;
		}

		var choice = GetSelectedChoice();
		if (choice == null)
		{
			return;
		}

		try
		{
			activeRenderer = CreateRenderer(choice.Value);
			if (activeRenderer == null)
			{
				return;
			}

			ApplyColorModulation(activeRenderer);
			activeLayer.Renderer = activeRenderer;
			customizeController?.ApplyCurrentValues();
		}
		catch (Exception exception)
		{
			Debug.LogWarning("Failed to apply point cloud renderer: " + exception.Message);
		}
	}

	private ArcGISPointCloudRenderer CreateRenderer(RendererChoice choice)
	{
		switch (choice)
		{
			case RendererChoice.RGB:
				return string.IsNullOrEmpty(availableAttributes.RGB) ? null : new ArcGISPointCloudRGBRenderer(availableAttributes.RGB);
			case RendererChoice.Class:
				return string.IsNullOrEmpty(availableAttributes.ClassCode) ? null : CreateClassRenderer(availableAttributes.ClassCode);
			case RendererChoice.Elevation:
				return string.IsNullOrEmpty(availableAttributes.Elevation) ? null : CreateElevationRenderer(availableAttributes.Elevation);
			case RendererChoice.Intensity:
				return string.IsNullOrEmpty(availableAttributes.Intensity) ? null : CreateIntensityRenderer(availableAttributes.Intensity);
			default:
				return null;
		}
	}

	private ArcGISPointCloudUniqueValueRenderer CreateClassRenderer(string attributeName)
	{
		activeUniqueValueCollection = new ArcGISCollection<ArcGISPointCloudColorUniqueValue>();
		activeUniqueValueGroups = new ArcGISCollection<string>[7];
		activeUniqueValues = new ArcGISPointCloudColorUniqueValue[7];
		activeColors = new ArcGISRGBColor[7];

		AddClassValue(0, new[] { "1" }, "Unclassified", Color(190, 137, 12));
		AddClassValue(1, new[] { "2" }, "Ground", Color(219, 255, 104));
		AddClassValue(2, new[] { "3" }, "Low vegetation", Color(246, 44, 28));
		AddClassValue(3, new[] { "5" }, "High vegetation", Color(199, 24, 255));
		AddClassValue(4, new[] { "6" }, "Building", Color(255, 255, 112));
		AddClassValue(5, new[] { "7" }, "Low point (noise)", Color(152, 152, 152));
		AddClassValue(6, new[] { "9" }, "Water", Color(246, 244, 22));

		return new ArcGISPointCloudUniqueValueRenderer(attributeName, activeUniqueValueCollection);
	}

	private ArcGISPointCloudStretchRenderer CreateElevationRenderer(string attributeName)
	{
		activeStopCollection = new ArcGISCollection<ArcGISPointCloudColorStop>();
		activeStops = new ArcGISPointCloudColorStop[5];
		activeColors = new ArcGISRGBColor[5];

		AddStop(0, ElevationLow, Color(42, 43, 238), "< -1.5");
		AddStop(1, 0d, Color(40, 210, 246), "");
		AddStop(2, ElevationMid, Color(91, 248, 134), "1.5");
		AddStop(3, 2.5d, Color(250, 244, 73), "");
		AddStop(4, ElevationHigh, Color(255, 59, 22), "> 3.5");

		return new ArcGISPointCloudStretchRenderer(attributeName, activeStopCollection);
	}

	private ArcGISPointCloudStretchRenderer CreateIntensityRenderer(string attributeName)
	{
		activeStopCollection = new ArcGISCollection<ArcGISPointCloudColorStop>();
		activeStops = new ArcGISPointCloudColorStop[3];
		activeColors = new ArcGISRGBColor[3];

		AddStop(0, IntensityLow, Color(0, 0, 0), "< 10,385");
		AddStop(1, IntensityMid, Color(128, 128, 128), "38,032");
		AddStop(2, IntensityHigh, Color(255, 255, 255), "> 65,680");

		return new ArcGISPointCloudStretchRenderer(attributeName, activeStopCollection);
	}

	private void AddClassValue(int index, string[] values, string label, ArcGISRGBColor color)
	{
		var valueGroup = new ArcGISCollection<string>();
		foreach (var value in values)
		{
			valueGroup.Add(value);
		}

		var uniqueValue = new ArcGISPointCloudColorUniqueValue(color, valueGroup)
		{
			Label = label,
			Description = label
		};

		activeColors[index] = color;
		activeUniqueValueGroups[index] = valueGroup;
		activeUniqueValues[index] = uniqueValue;
		activeUniqueValueCollection.Add(uniqueValue);
	}

	private void AddStop(int index, double value, ArcGISRGBColor color, string label)
	{
		var stop = new ArcGISPointCloudColorStop(color, value)
		{
			Label = label
		};

		activeColors[index] = color;
		activeStops[index] = stop;
		activeStopCollection.Add(stop);
	}

	private void ApplyColorModulation(ArcGISPointCloudRenderer renderer)
	{
		if (renderer == null)
		{
			return;
		}

		if (!colorModulationToggle || !colorModulationToggle.isOn || string.IsNullOrEmpty(availableAttributes.Intensity))
		{
			renderer.ColorModulation = null;
			activeColorModulation = null;
			return;
		}

		activeColorModulation = new ArcGISPointCloudColorModulation(availableAttributes.Intensity, 0d, 65535d);
		renderer.ColorModulation = activeColorModulation;
	}

	private RendererChoice? GetSelectedChoice()
	{
		if (IsVisibleAndOn(rgbToggle))
		{
			return RendererChoice.RGB;
		}

		if (IsVisibleAndOn(classToggle))
		{
			return RendererChoice.Class;
		}

		if (IsVisibleAndOn(elevationToggle))
		{
			return RendererChoice.Elevation;
		}

		if (IsVisibleAndOn(intensityToggle))
		{
			return RendererChoice.Intensity;
		}

		return null;
	}

	private AvailableAttributes GetAvailableAttributes(ArcGISPointCloudLayer layer)
	{
		var result = new AvailableAttributes();
		if (layer == null || layer.Attributes == null)
		{
			return result;
		}

		var attributes = layer.Attributes;
		for (ulong i = 0; i < attributes.GetSize(); i++)
		{
			var attribute = attributes.At(i);
			if (attribute == null)
			{
				continue;
			}

			var name = attribute.Name;
			var normalizedName = NormalizeName(name);

			if (string.IsNullOrEmpty(result.RGB) && IsRGBAttribute(attribute, normalizedName))
			{
				result.RGB = name;
			}

			if (string.IsNullOrEmpty(result.ClassCode) && MatchesName(normalizedName, "CLASSCODE", "CLASSIFICATION", "CLASS"))
			{
				result.ClassCode = name;
			}

			if (string.IsNullOrEmpty(result.Elevation) && (MatchesName(normalizedName, "ELEVATION", "HEIGHT") || normalizedName == "Z"))
			{
				result.Elevation = name;
			}

			if (string.IsNullOrEmpty(result.Intensity) && MatchesName(normalizedName, "INTENSITY"))
			{
				result.Intensity = name;
			}
		}

		return result;
	}

	private static bool IsRGBAttribute(ArcGISPointCloudAttribute attribute, string normalizedName)
	{
		return normalizedName == "RGB" ||
			normalizedName == "RGBA" ||
			normalizedName == "COLOR" ||
			normalizedName == "COLORRGB" ||
			attribute.ValuesPerElement >= 3;
	}

	private static bool MatchesName(string normalizedName, params string[] candidates)
	{
		foreach (var candidate in candidates)
		{
			if (normalizedName == candidate || normalizedName.Contains(candidate))
			{
				return true;
			}
		}

		return false;
	}

	private static string NormalizeName(string name)
	{
		return string.IsNullOrEmpty(name) ? "" : name.Replace("_", "").Replace("-", "").Replace(" ", "").ToUpperInvariant();
	}

	private static bool IsVisibleAndOn(Toggle toggle)
	{
		return toggle && toggle.gameObject.activeSelf && toggle.isOn;
	}

	private void SetToggleVisible(Toggle toggle, bool visible)
	{
		if (!toggle)
		{
			return;
		}

		if (!visible && toggle.isOn)
		{
			suppressToggleEvents = true;
			toggle.isOn = false;
			suppressToggleEvents = false;
		}

		SetFeatureVisible(toggle.gameObject, visible);
	}

	private static void SetFeatureVisible(GameObject feature, bool visible)
	{
		if (feature && feature.activeSelf != visible)
		{
			feature.SetActive(visible);
		}
	}

	private static ArcGISRGBColor Color(byte red, byte green, byte blue)
	{
		return new ArcGISRGBColor(red, green, blue, 255);
	}
}
