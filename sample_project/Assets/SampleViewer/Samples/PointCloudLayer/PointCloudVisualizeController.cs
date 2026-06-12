using Esri.GameEngine.Layers;
using Esri.GameEngine.Layers.PointCloud;
using Esri.Standard;
using Esri.Unity;
using System;
using System.Collections.Generic;
using System.Globalization;
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

	private sealed class ClassRendererInfo
	{
		public string AttributeName;
		public ArcGISPointCloudAttributeTransformType TransformType;
		public readonly List<ClassValueInfo> Values = new List<ClassValueInfo>();
	}

	private sealed class ClassValueInfo
	{
		public string Label;
		public string Description;
		public string[] Values;
		public byte Red;
		public byte Green;
		public byte Blue;
		public byte Alpha = 255;
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
	[SerializeField] private Toggle visualizeTab;
	[SerializeField] private RectTransform rendererLegendRoot;
	[SerializeField] private Font rendererLegendFont;
	[SerializeField] private Vector2 rendererLegendPanelOffset = new Vector2(-140f, 80f);

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
	private ClassRendererInfo classRendererInfo;
	private Image legendRootBackground;
	private CanvasGroup legendCanvasGroup;
	private RectTransform legendPanelRect;
	private Sprite legendCircleSprite;
	private Sprite legendTriangleSprite;

	private readonly UnityEngine.Color legendPanelColor = new UnityEngine.Color(0.08f, 0.08f, 0.08f, 0.82f);
	private readonly UnityEngine.Color legendAccentColor = new UnityEngine.Color(0.56f, 0.25f, 1f, 1f);
	private readonly UnityEngine.Color legendTextColor = new UnityEngine.Color(0.95f, 0.95f, 0.95f, 1f);
	private readonly UnityEngine.Color legendMutedTextColor = new UnityEngine.Color(0.78f, 0.78f, 0.78f, 1f);

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

		if (!visualizeTab)
		{
			visualizeTab = transform.Find("Tab/VisualizeToggle")?.GetComponent<Toggle>();
		}

		if (!rendererLegendRoot)
		{
			var legendObject = GameObject.Find("RendererLegendPanel");
			rendererLegendRoot = legendObject ? legendObject.GetComponent<RectTransform>() : null;
		}

		if (!rendererLegendFont)
		{
			rendererLegendFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
		}

		EnsureLegendRoot();
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

		if (visualizeTab)
		{
			visualizeTab.onValueChanged.AddListener(HandleVisualizeTabChanged);
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

			if (visualizeTab)
			{
				visualizeTab.onValueChanged.RemoveListener(HandleVisualizeTabChanged);
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
		classRendererInfo = string.IsNullOrEmpty(availableAttributes.ClassCode) ? null : CreateStandardClassRendererInfo(availableAttributes.ClassCode);

		ApplyAvailability();
		EnsureAvailableRendererSelected();
		UpdateClassLegend(classRendererInfo);
		ApplySelectedRenderer();
		RefreshRendererLegend();
	}

	private void HandleRendererToggleChanged(bool isOn)
	{
		if (suppressToggleEvents || !isOn)
		{
			return;
		}

		ApplySelectedRenderer();
		RefreshRendererLegend();
	}

	private void HandleVisualizeTabChanged(bool _)
	{
		RefreshRendererLegend();
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
		if (classRendererInfo == null || classRendererInfo.Values.Count == 0)
		{
			classRendererInfo = CreateStandardClassRendererInfo(attributeName);
		}

		activeUniqueValueCollection = new ArcGISCollection<ArcGISPointCloudColorUniqueValue>();
		activeUniqueValueGroups = new ArcGISCollection<string>[classRendererInfo.Values.Count];
		activeUniqueValues = new ArcGISPointCloudColorUniqueValue[classRendererInfo.Values.Count];
		activeColors = new ArcGISRGBColor[classRendererInfo.Values.Count];

		for (var i = 0; i < classRendererInfo.Values.Count; i++)
		{
			var value = classRendererInfo.Values[i];
			AddClassValue(i, value.Values, value.Label, Color(value.Red, value.Green, value.Blue, value.Alpha));
		}

		return new ArcGISPointCloudUniqueValueRenderer(
			string.IsNullOrEmpty(classRendererInfo.AttributeName) ? attributeName : classRendererInfo.AttributeName,
			activeUniqueValueCollection)
		{
			TransformType = classRendererInfo.TransformType
		};
	}

	private static ClassRendererInfo CreateStandardClassRendererInfo(string attributeName)
	{
		var rendererInfo = new ClassRendererInfo
		{
			AttributeName = attributeName,
			TransformType = ArcGISPointCloudAttributeTransformType.None
		};

		for (var classValue = 0; classValue <= 18; classValue++)
		{
			AddStandardClassRendererInfoValue(rendererInfo, classValue);
		}

		return rendererInfo;
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

	private static void AddClassRendererInfoValue(ClassRendererInfo rendererInfo, string[] values, string label, byte red, byte green, byte blue, byte alpha = 255)
	{
		rendererInfo.Values.Add(new ClassValueInfo
		{
			Label = label,
			Description = label,
			Values = values,
			Red = red,
			Green = green,
			Blue = blue,
			Alpha = alpha
		});
	}

	private static void AddStandardClassRendererInfoValue(ClassRendererInfo rendererInfo, int classValue)
	{
		GetStandardClassInfo(classValue, out var label, out var red, out var green, out var blue);
		AddClassRendererInfoValue(rendererInfo, new[] { classValue.ToString(CultureInfo.InvariantCulture) }, label, red, green, blue);
	}

	private static void GetStandardClassInfo(int classValue, out string label, out byte red, out byte green, out byte blue)
	{
		switch (classValue)
		{
			case 0:
				label = "Created, never classified";
				red = 128;
				green = 128;
				blue = 128;
				return;
			case 1:
				label = "Unclassified";
				red = 190;
				green = 137;
				blue = 12;
				return;
			case 2:
				label = "Ground";
				red = 219;
				green = 255;
				blue = 104;
				return;
			case 3:
				label = "Low vegetation";
				red = 246;
				green = 44;
				blue = 28;
				return;
			case 4:
				label = "Medium vegetation";
				red = 244;
				green = 102;
				blue = 32;
				return;
			case 5:
				label = "High vegetation";
				red = 199;
				green = 24;
				blue = 255;
				return;
			case 6:
				label = "Building";
				red = 255;
				green = 255;
				blue = 112;
				return;
			case 7:
				label = "Low point (noise)";
				red = 152;
				green = 152;
				blue = 152;
				return;
			case 8:
				label = "Model key-point";
				red = 255;
				green = 186;
				blue = 87;
				return;
			case 9:
				label = "Water";
				red = 246;
				green = 244;
				blue = 22;
				return;
			case 10:
				label = "Rail";
				red = 209;
				green = 98;
				blue = 224;
				return;
			case 11:
				label = "Road surface";
				red = 218;
				green = 218;
				blue = 218;
				return;
			case 12:
				label = "Overlap points";
				red = 84;
				green = 167;
				blue = 255;
				return;
			case 13:
				label = "Wire guard";
				red = 255;
				green = 121;
				blue = 198;
				return;
			case 14:
				label = "Wire conductor";
				red = 255;
				green = 160;
				blue = 67;
				return;
			case 15:
				label = "Transmission tower";
				red = 255;
				green = 92;
				blue = 92;
				return;
			case 16:
				label = "Wire connector";
				red = 136;
				green = 255;
				blue = 218;
				return;
			case 17:
				label = "Bridge deck";
				red = 141;
				green = 108;
				blue = 255;
				return;
			case 18:
				label = "High noise";
				red = 80;
				green = 80;
				blue = 80;
				return;
			default:
				label = "Class " + classValue.ToString(CultureInfo.InvariantCulture);
				GetFallbackClassColor(classValue, out red, out green, out blue, out _);
				return;
		}
	}

	private static void GetFallbackClassColor(int index, out byte red, out byte green, out byte blue, out byte alpha)
	{
		var palette = new[]
		{
			new byte[] { 190, 137, 12 },
			new byte[] { 219, 255, 104 },
			new byte[] { 246, 44, 28 },
			new byte[] { 199, 24, 255 },
			new byte[] { 255, 255, 112 },
			new byte[] { 152, 152, 152 },
			new byte[] { 246, 244, 22 },
			new byte[] { 69, 188, 255 },
			new byte[] { 255, 121, 198 }
		};

		var color = palette[Mathf.Abs(index) % palette.Length];
		red = color[0];
		green = color[1];
		blue = color[2];
		alpha = 255;
	}

	private void UpdateClassLegend(ClassRendererInfo rendererInfo)
	{
		RefreshRendererLegend();
	}

	private void EnsureLegendRoot()
	{
		if (!rendererLegendRoot)
		{
			return;
		}

		legendRootBackground = rendererLegendRoot.GetComponent<Image>();
		legendCanvasGroup = rendererLegendRoot.GetComponent<CanvasGroup>();

		if (!legendRootBackground)
		{
			legendRootBackground = rendererLegendRoot.gameObject.AddComponent<Image>();
		}

		if (!legendCanvasGroup)
		{
			legendCanvasGroup = rendererLegendRoot.gameObject.AddComponent<CanvasGroup>();
		}

		legendRootBackground.color = UnityEngine.Color.clear;
		legendRootBackground.raycastTarget = false;
		legendCanvasGroup.interactable = false;
		legendCanvasGroup.blocksRaycasts = false;

		rendererLegendRoot.anchorMin = Vector2.zero;
		rendererLegendRoot.anchorMax = Vector2.one;
		rendererLegendRoot.pivot = new Vector2(0.5f, 0.5f);
		rendererLegendRoot.offsetMin = Vector2.zero;
		rendererLegendRoot.offsetMax = Vector2.zero;
		rendererLegendRoot.anchoredPosition = Vector2.zero;
	}

	private void RefreshRendererLegend()
	{
		EnsureLegendRoot();
		if (!rendererLegendRoot || !legendCanvasGroup)
		{
			return;
		}

		var shouldShow = visualizeTab && visualizeTab.isOn;
		legendCanvasGroup.alpha = shouldShow ? 1f : 0f;
		legendCanvasGroup.interactable = shouldShow;
		legendCanvasGroup.blocksRaycasts = shouldShow;

		if (!shouldShow)
		{
			return;
		}

		BuildRendererLegend(GetSelectedChoice() ?? RendererChoice.RGB);
	}

	private void BuildRendererLegend(RendererChoice mode)
	{
		ClearGeneratedLegendChildren();

		switch (mode)
		{
			case RendererChoice.Class:
				CreateLegendPanel(500f, 440f);
				AddLegendAccent();
				AddLegendTitle(150f);
				AddLegendText("Generated_ClassHeader", "Class Code", new Vector2(80f, 70f), new Vector2(430f, 40f), 28, TextAnchor.MiddleLeft, legendTextColor);
				AddClassLegendRows();
				break;
			case RendererChoice.Elevation:
				CreateLegendPanel(500f, 400f);
				AddLegendAccent();
				AddLegendTitle(145f);
				AddStretchLegend("Elevation", true);
				break;
			case RendererChoice.Intensity:
				CreateLegendPanel(500f, 400f);
				AddLegendAccent();
				AddLegendTitle(145f);
				AddStretchLegend("Intensity", false);
				break;
			default:
				CreateLegendPanel(500f, 110f);
				AddLegendAccent();
				AddLegendText("Generated_NoLegend", "No legend", Vector2.zero, new Vector2(430f, 60f), 34, TextAnchor.MiddleCenter, legendTextColor);
				break;
		}
	}

	private void CreateLegendPanel(float width, float height)
	{
		var go = new GameObject("Generated_Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
		go.layer = rendererLegendRoot.gameObject.layer;
		go.transform.SetParent(rendererLegendRoot, false);

		legendPanelRect = go.GetComponent<RectTransform>();
		legendPanelRect.anchorMin = new Vector2(1f, 0f);
		legendPanelRect.anchorMax = new Vector2(1f, 0f);
		legendPanelRect.pivot = new Vector2(1f, 0f);
		legendPanelRect.anchoredPosition = rendererLegendPanelOffset;
		legendPanelRect.sizeDelta = new Vector2(width, height);

		var panelBackground = go.GetComponent<Image>();
		panelBackground.color = legendPanelColor;
		panelBackground.raycastTarget = false;
	}

	private void AddLegendAccent()
	{
		var accent = AddLegendImage("Generated_Accent", Vector2.zero, new Vector2(8f, 0f), legendAccentColor);
		accent.rectTransform.anchorMin = new Vector2(0f, 0f);
		accent.rectTransform.anchorMax = new Vector2(0f, 1f);
		accent.rectTransform.pivot = new Vector2(0f, 0.5f);
		accent.rectTransform.anchoredPosition = Vector2.zero;
		accent.rectTransform.sizeDelta = new Vector2(8f, 0f);
	}

	private void AddLegendTitle(float y)
	{
		AddLegendText("Generated_Title", "Tallinn punktipilv", new Vector2(0f, y), new Vector2(430f, 56f), 36, TextAnchor.MiddleCenter, legendMutedTextColor);
	}

	private void AddClassLegendRows()
	{
		if (classRendererInfo == null || classRendererInfo.Values.Count == 0)
		{
			AddLegendText("Generated_NoClassLegend", "No legend", new Vector2(25f, -45f), new Vector2(360f, 60f), 30, TextAnchor.MiddleCenter, legendTextColor);
			return;
		}

		const float rowSpacing = 36f;
		const float viewportHeight = 250f;
		var contentHeight = Mathf.Max(viewportHeight, classRendererInfo.Values.Count * rowSpacing);
		var content = AddClassLegendScrollArea(new Vector2(25f, -82f), new Vector2(340f, viewportHeight), contentHeight);

		for (var i = 0; i < classRendererInfo.Values.Count; i++)
		{
			var value = classRendererInfo.Values[i];
			var y = contentHeight * 0.5f - rowSpacing * 0.5f - i * rowSpacing;
			var color = new Color32(value.Red, value.Green, value.Blue, value.Alpha);
			var label = string.IsNullOrEmpty(value.Label) ? string.Join(", ", value.Values) : value.Label;

			AddLegendCircle("Generated_ClassDot_" + i, new Vector2(-145f, y), new Vector2(26f, 26f), color, content);
			AddLegendText("Generated_ClassLabel_" + i, label, new Vector2(40f, y), new Vector2(260f, 38f), 26, TextAnchor.MiddleLeft, legendTextColor, content);
		}
	}

	private RectTransform AddClassLegendScrollArea(Vector2 anchoredPosition, Vector2 size, float contentHeight)
	{
		var scrollRoot = new GameObject("Generated_ClassScrollRect", typeof(RectTransform), typeof(ScrollRect));
		scrollRoot.layer = rendererLegendRoot.gameObject.layer;
		scrollRoot.transform.SetParent(legendPanelRect, false);

		var scrollRectTransform = scrollRoot.GetComponent<RectTransform>();
		scrollRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
		scrollRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
		scrollRectTransform.pivot = new Vector2(0.5f, 0.5f);
		scrollRectTransform.anchoredPosition = anchoredPosition;
		scrollRectTransform.sizeDelta = size;

		var viewport = new GameObject("Generated_ClassViewport", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(RectMask2D));
		viewport.layer = rendererLegendRoot.gameObject.layer;
		viewport.transform.SetParent(scrollRoot.transform, false);

		var viewportRect = viewport.GetComponent<RectTransform>();
		viewportRect.anchorMin = Vector2.zero;
		viewportRect.anchorMax = Vector2.one;
		viewportRect.pivot = new Vector2(0.5f, 0.5f);
		viewportRect.offsetMin = Vector2.zero;
		viewportRect.offsetMax = Vector2.zero;

		var viewportImage = viewport.GetComponent<Image>();
		viewportImage.color = UnityEngine.Color.clear;
		viewportImage.raycastTarget = true;

		var content = new GameObject("Generated_ClassContent", typeof(RectTransform));
		content.layer = rendererLegendRoot.gameObject.layer;
		content.transform.SetParent(viewport.transform, false);

		var contentRect = content.GetComponent<RectTransform>();
		contentRect.anchorMin = new Vector2(0f, 1f);
		contentRect.anchorMax = new Vector2(1f, 1f);
		contentRect.pivot = new Vector2(0.5f, 1f);
		contentRect.anchoredPosition = Vector2.zero;
		contentRect.sizeDelta = new Vector2(0f, contentHeight);

		var scrollbar = AddClassLegendScrollbar(new Vector2(205f, -82f), new Vector2(28f, size.y));

		var scrollRect = scrollRoot.GetComponent<ScrollRect>();
		scrollRect.content = contentRect;
		scrollRect.viewport = viewportRect;
		scrollRect.horizontal = false;
		scrollRect.vertical = true;
		scrollRect.movementType = ScrollRect.MovementType.Clamped;
		scrollRect.inertia = false;
		scrollRect.scrollSensitivity = 36f;
		scrollRect.verticalScrollbar = scrollbar;
		scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
		scrollRect.verticalNormalizedPosition = 1f;

		scrollbar.value = 1f;
		scrollbar.size = Mathf.Clamp01(size.y / Mathf.Max(size.y, contentHeight));
		return contentRect;
	}

	private Scrollbar AddClassLegendScrollbar(Vector2 anchoredPosition, Vector2 size)
	{
		var scrollbarGo = new GameObject("Generated_ClassScrollbar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Scrollbar));
		scrollbarGo.layer = rendererLegendRoot.gameObject.layer;
		scrollbarGo.transform.SetParent(legendPanelRect, false);

		var scrollbarRect = scrollbarGo.GetComponent<RectTransform>();
		scrollbarRect.anchorMin = new Vector2(0.5f, 0.5f);
		scrollbarRect.anchorMax = new Vector2(0.5f, 0.5f);
		scrollbarRect.pivot = new Vector2(0.5f, 0.5f);
		scrollbarRect.anchoredPosition = anchoredPosition;
		scrollbarRect.sizeDelta = size;

		var track = scrollbarGo.GetComponent<Image>();
		track.color = new UnityEngine.Color(0.28f, 0.28f, 0.28f, 0.82f);
		track.raycastTarget = true;

		var slidingArea = new GameObject("Generated_ClassScrollbarSlidingArea", typeof(RectTransform));
		slidingArea.layer = rendererLegendRoot.gameObject.layer;
		slidingArea.transform.SetParent(scrollbarGo.transform, false);

		var slidingAreaRect = slidingArea.GetComponent<RectTransform>();
		slidingAreaRect.anchorMin = Vector2.zero;
		slidingAreaRect.anchorMax = Vector2.one;
		slidingAreaRect.pivot = new Vector2(0.5f, 0.5f);
		slidingAreaRect.offsetMin = Vector2.zero;
		slidingAreaRect.offsetMax = Vector2.zero;

		var handle = new GameObject("Generated_ClassScrollbarHandle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
		handle.layer = rendererLegendRoot.gameObject.layer;
		handle.transform.SetParent(slidingArea.transform, false);

		var handleRect = handle.GetComponent<RectTransform>();
		handleRect.anchorMin = Vector2.zero;
		handleRect.anchorMax = Vector2.one;
		handleRect.pivot = new Vector2(0.5f, 0.5f);
		handleRect.offsetMin = Vector2.zero;
		handleRect.offsetMax = Vector2.zero;

		var handleImage = handle.GetComponent<Image>();
		handleImage.color = new UnityEngine.Color(0.75f, 0.75f, 0.75f, 1f);
		handleImage.raycastTarget = true;

		var scrollbar = scrollbarGo.GetComponent<Scrollbar>();
		scrollbar.direction = Scrollbar.Direction.BottomToTop;
		scrollbar.targetGraphic = handleImage;
		scrollbar.handleRect = handleRect;
		scrollbar.transition = Selectable.Transition.None;
		return scrollbar;
	}

	private void AddStretchLegend(string label, bool isElevation)
	{
		const float rampCenterY = -60f;
		const float topBreakY = 10f;
		const float middleBreakY = -60f;
		const float bottomBreakY = -130f;

		AddLegendText("Generated_StretchHeader", label, new Vector2(0f, 70f), new Vector2(270f, 40f), 28, TextAnchor.MiddleLeft, legendTextColor);

		var gradientColors = isElevation
			? new[]
			{
				new UnityEngine.Color(0.95f, 0.12f, 0.08f, 1f),
				new UnityEngine.Color(1f, 0.9f, 0.2f, 1f),
				new UnityEngine.Color(0.35f, 0.95f, 0.48f, 1f),
				new UnityEngine.Color(0.25f, 0.82f, 1f, 1f),
				new UnityEngine.Color(0.22f, 0.12f, 1f, 1f)
			}
			: new[]
			{
				UnityEngine.Color.white,
				new UnityEngine.Color(0.65f, 0.65f, 0.65f, 1f),
				new UnityEngine.Color(0.16f, 0.16f, 0.16f, 1f),
				UnityEngine.Color.black
			};

		AddLegendGradient("Generated_Gradient", new Vector2(-80f, rampCenterY), new Vector2(56f, 170f), gradientColors);

		if (isElevation)
		{
			AddBreakLabel("> 3.5", topBreakY);
			AddBreakLabel("1.5", middleBreakY);
			AddBreakLabel("< -1.5", bottomBreakY);
		}
		else
		{
			AddBreakLabel("> 65,680", topBreakY);
			AddBreakLabel("38,032", middleBreakY);
			AddBreakLabel("<10,385", bottomBreakY);
		}
	}

	private void AddBreakLabel(string label, float y)
	{
		AddLegendTriangle("Generated_Triangle_" + label, new Vector2(-35f, y), new Vector2(18f, 22f), legendTextColor);
		AddLegendText("Generated_BreakLabel_" + label, label, new Vector2(88f, y), new Vector2(210f, 38f), 28, TextAnchor.MiddleLeft, legendTextColor);
	}

	private Text AddLegendText(string name, string text, Vector2 anchoredPosition, Vector2 size, int fontSize, TextAnchor alignment, UnityEngine.Color color)
	{
		return AddLegendText(name, text, anchoredPosition, size, fontSize, alignment, color, legendPanelRect ? legendPanelRect : rendererLegendRoot);
	}

	private Text AddLegendText(string name, string text, Vector2 anchoredPosition, Vector2 size, int fontSize, TextAnchor alignment, UnityEngine.Color color, Transform parent)
	{
		var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
		go.layer = rendererLegendRoot.gameObject.layer;
		go.transform.SetParent(parent, false);

		var rect = go.GetComponent<RectTransform>();
		rect.anchorMin = new Vector2(0.5f, 0.5f);
		rect.anchorMax = new Vector2(0.5f, 0.5f);
		rect.pivot = new Vector2(0.5f, 0.5f);
		rect.anchoredPosition = anchoredPosition;
		rect.sizeDelta = size;

		var textComponent = go.GetComponent<Text>();
		textComponent.text = text;
		textComponent.font = rendererLegendFont ? rendererLegendFont : Resources.GetBuiltinResource<Font>("Arial.ttf");
		textComponent.fontSize = fontSize;
		textComponent.alignment = alignment;
		textComponent.horizontalOverflow = HorizontalWrapMode.Overflow;
		textComponent.verticalOverflow = VerticalWrapMode.Overflow;
		textComponent.color = color;
		textComponent.raycastTarget = false;
		return textComponent;
	}

	private Image AddLegendImage(string name, Vector2 anchoredPosition, Vector2 size, UnityEngine.Color color)
	{
		return AddLegendImage(name, anchoredPosition, size, color, legendPanelRect ? legendPanelRect : rendererLegendRoot);
	}

	private Image AddLegendImage(string name, Vector2 anchoredPosition, Vector2 size, UnityEngine.Color color, Transform parent)
	{
		var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
		go.layer = rendererLegendRoot.gameObject.layer;
		go.transform.SetParent(parent, false);

		var rect = go.GetComponent<RectTransform>();
		rect.anchorMin = new Vector2(0.5f, 0.5f);
		rect.anchorMax = new Vector2(0.5f, 0.5f);
		rect.pivot = new Vector2(0.5f, 0.5f);
		rect.anchoredPosition = anchoredPosition;
		rect.sizeDelta = size;

		var image = go.GetComponent<Image>();
		image.color = color;
		image.raycastTarget = false;
		return image;
	}

	private void AddLegendCircle(string name, Vector2 anchoredPosition, Vector2 size, UnityEngine.Color color, Transform parent)
	{
		var image = AddLegendImage(name, anchoredPosition, size, color, parent);
		image.sprite = GetLegendCircleSprite();
		image.preserveAspect = true;
	}

	private void AddLegendTriangle(string name, Vector2 anchoredPosition, Vector2 size, UnityEngine.Color color)
	{
		var image = AddLegendImage(name, anchoredPosition, size, color);
		image.sprite = GetLegendTriangleSprite();
		image.preserveAspect = true;
	}

	private void AddLegendGradient(string name, Vector2 anchoredPosition, Vector2 size, UnityEngine.Color[] colors)
	{
		var image = AddLegendImage(name, anchoredPosition, size, UnityEngine.Color.white);
		image.sprite = CreateLegendGradientSprite(colors);
		image.preserveAspect = false;
	}

	private Sprite GetLegendCircleSprite()
	{
		if (legendCircleSprite)
		{
			return legendCircleSprite;
		}

		const int size = 32;
		var texture = new Texture2D(size, size, TextureFormat.ARGB32, false)
		{
			wrapMode = TextureWrapMode.Clamp
		};

		var center = (size - 1) * 0.5f;
		var radius = center - 1f;

		for (var y = 0; y < size; y++)
		{
			for (var x = 0; x < size; x++)
			{
				var distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
				var alpha = Mathf.Clamp01(radius + 0.75f - distance);
				texture.SetPixel(x, y, new UnityEngine.Color(1f, 1f, 1f, alpha));
			}
		}

		texture.Apply();
		legendCircleSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
		return legendCircleSprite;
	}

	private Sprite GetLegendTriangleSprite()
	{
		if (legendTriangleSprite)
		{
			return legendTriangleSprite;
		}

		const int width = 28;
		const int height = 32;
		var texture = new Texture2D(width, height, TextureFormat.ARGB32, false)
		{
			wrapMode = TextureWrapMode.Clamp
		};

		for (var y = 0; y < height; y++)
		{
			for (var x = 0; x < width; x++)
			{
				var halfHeight = height * 0.5f;
				var maxX = Mathf.Lerp(width - 1f, 2f, Mathf.Abs(y - halfHeight) / halfHeight);
				texture.SetPixel(x, y, x <= maxX ? UnityEngine.Color.white : UnityEngine.Color.clear);
			}
		}

		texture.Apply();
		legendTriangleSprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 100f);
		return legendTriangleSprite;
	}

	private Sprite CreateLegendGradientSprite(UnityEngine.Color[] colors)
	{
		const int width = 16;
		const int height = 128;
		var texture = new Texture2D(width, height, TextureFormat.ARGB32, false)
		{
			wrapMode = TextureWrapMode.Clamp
		};

		for (var y = 0; y < height; y++)
		{
			var t = 1f - y / (float)(height - 1);
			var color = EvaluateGradient(colors, t);

			for (var x = 0; x < width; x++)
			{
				texture.SetPixel(x, y, color);
			}
		}

		texture.Apply();
		return Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 100f);
	}

	private static UnityEngine.Color EvaluateGradient(UnityEngine.Color[] colors, float t)
	{
		if (colors == null || colors.Length == 0)
		{
			return UnityEngine.Color.white;
		}

		if (colors.Length == 1)
		{
			return colors[0];
		}

		var scaled = Mathf.Clamp01(t) * (colors.Length - 1);
		var index = Mathf.Min(Mathf.FloorToInt(scaled), colors.Length - 2);
		var localT = scaled - index;
		return UnityEngine.Color.Lerp(colors[index], colors[index + 1], localT);
	}

	private void ClearGeneratedLegendChildren()
	{
		legendPanelRect = null;

		if (!rendererLegendRoot)
		{
			return;
		}

		for (var i = rendererLegendRoot.childCount - 1; i >= 0; i--)
		{
			var child = rendererLegendRoot.GetChild(i);
			if (!child.name.StartsWith("Generated_"))
			{
				continue;
			}

			if (Application.isPlaying)
			{
				Destroy(child.gameObject);
			}
			else
			{
				DestroyImmediate(child.gameObject);
			}
		}
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

	private static ArcGISRGBColor Color(byte red, byte green, byte blue, byte alpha)
	{
		return new ArcGISRGBColor(red, green, blue, alpha);
	}
}
