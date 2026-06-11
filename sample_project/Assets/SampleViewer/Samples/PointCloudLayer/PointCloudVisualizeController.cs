using Esri.GameEngine.Layers;
using Esri.GameEngine.Layers.PointCloud;
using Esri.Standard;
using Esri.Unity;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;
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
	[SerializeField] private RendererLegendMockPanel rendererLegendPanel;

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
	private Coroutine classRendererInfoCoroutine;

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
		StopClassRendererInfoRequest();
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

		if (!rendererLegendPanel)
		{
			rendererLegendPanel = FindFirstObjectByType<RendererLegendMockPanel>();
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
		classRendererInfo = string.IsNullOrEmpty(availableAttributes.ClassCode) ? null : CreateStandardClassRendererInfo(availableAttributes.ClassCode);

		ApplyAvailability();
		EnsureAvailableRendererSelected();
		UpdateClassLegend(classRendererInfo);
		StartClassRendererInfoRequest(layer);
		ApplySelectedRenderer();
	}

	private void StartClassRendererInfoRequest(ArcGISPointCloudLayer layer)
	{
		StopClassRendererInfoRequest();

		if (!Application.isPlaying || layer == null || dataLoader == null ||
			string.IsNullOrEmpty(dataLoader.LoadedSource) || string.IsNullOrEmpty(availableAttributes.ClassCode))
		{
			return;
		}

		classRendererInfoCoroutine = StartCoroutine(LoadClassRendererInfo(dataLoader.LoadedSource, layer, availableAttributes.ClassCode));
	}

	private void StopClassRendererInfoRequest()
	{
		if (classRendererInfoCoroutine == null)
		{
			return;
		}

		StopCoroutine(classRendererInfoCoroutine);
		classRendererInfoCoroutine = null;
	}

	private IEnumerator LoadClassRendererInfo(string source, ArcGISPointCloudLayer layer, string fallbackAttributeName)
	{
		var metadataUrls = new List<string>();
		AddSceneServerMetadataUrls(source, metadataUrls);

		var itemInfoUrl = TryCreateArcGISItemUrl(source, false);
		if (!string.IsNullOrEmpty(itemInfoUrl))
		{
			string itemInfoJson = null;
			yield return RequestJson(AddAuthorizationQuery(itemInfoUrl), json => itemInfoJson = json);
			AddSceneServerMetadataUrlsFromJson(itemInfoJson, metadataUrls);

			string itemDataJson = null;
			yield return RequestJson(AddAuthorizationQuery(TryCreateArcGISItemUrl(source, true)), json => itemDataJson = json);
			AddSceneServerMetadataUrlsFromJson(itemDataJson, metadataUrls);
		}

		ClassRendererInfo rendererInfo = null;
		ClassRendererInfo standardFallbackInfo = null;
		foreach (var metadataUrl in metadataUrls)
		{
			string metadataJson = null;
			yield return RequestJson(AddAuthorizationQuery(metadataUrl), json => metadataJson = json);
			rendererInfo = TryParseClassRendererInfo(metadataJson, fallbackAttributeName);
			if (rendererInfo != null && rendererInfo.Values.Count > 0)
			{
				break;
			}

			standardFallbackInfo = standardFallbackInfo ?? TryCreateStandardClassRendererInfoFromMetadata(metadataJson, fallbackAttributeName);
		}

		rendererInfo = rendererInfo ?? standardFallbackInfo;

		classRendererInfoCoroutine = null;

		if (activeLayer != layer || dataLoader == null || dataLoader.LoadedSource != source)
		{
			yield break;
		}

		if (rendererInfo == null)
		{
			classRendererInfo = string.IsNullOrEmpty(fallbackAttributeName) ? null : CreateStandardClassRendererInfo(fallbackAttributeName);
			UpdateClassLegend(classRendererInfo);
			ApplyAvailability();
			EnsureAvailableRendererSelected();
			ApplySelectedRenderer();
			yield break;
		}

		classRendererInfo = rendererInfo;
		ApplyAvailability();
		EnsureAvailableRendererSelected();
		UpdateClassLegend(rendererInfo);

		var selectedChoice = GetSelectedChoice();
		if (selectedChoice == RendererChoice.Class)
		{
			ApplySelectedRenderer();
		}
	}

	private IEnumerator RequestJson(string url, Action<string> onSuccess)
	{
		if (string.IsNullOrEmpty(url))
		{
			yield break;
		}

		using (var request = UnityWebRequest.Get(url))
		{
			request.timeout = 12;
			yield return request.SendWebRequest();

			if (request.result != UnityWebRequest.Result.Success || request.downloadHandler == null)
			{
				yield break;
			}

			var json = request.downloadHandler.text;
			if (!string.IsNullOrEmpty(json))
			{
				onSuccess?.Invoke(json);
			}
		}
	}

	private string AddAuthorizationQuery(string url)
	{
		if (string.IsNullOrEmpty(url) || url.IndexOf("token=", StringComparison.OrdinalIgnoreCase) >= 0)
		{
			return url;
		}

		var apiKey = dataLoader ? dataLoader.APIKey : "";
		if (string.IsNullOrEmpty(apiKey))
		{
			return url;
		}

		return url + (url.Contains("?") ? "&" : "?") + "token=" + Uri.EscapeDataString(apiKey);
	}

	private static string TryCreateArcGISItemUrl(string source, bool dataEndpoint)
	{
		var itemId = TryGetArcGISItemId(source, out var sourceUri);
		if (string.IsNullOrEmpty(itemId))
		{
			return null;
		}

		var portalRoot = sourceUri != null ? sourceUri.GetLeftPart(UriPartial.Authority) : "https://www.arcgis.com";
		var endpoint = dataEndpoint ? "/data" : "";
		return portalRoot + "/sharing/rest/content/items/" + itemId + endpoint + "?f=json";
	}

	private static string TryGetArcGISItemId(string source, out Uri sourceUri)
	{
		sourceUri = null;
		if (string.IsNullOrWhiteSpace(source))
		{
			return null;
		}

		var trimmedSource = source.Trim();
		if (IsArcGISItemId(trimmedSource))
		{
			return trimmedSource;
		}

		if (!Uri.TryCreate(trimmedSource, UriKind.Absolute, out sourceUri))
		{
			return null;
		}

		var queryId = GetQueryValue(sourceUri.Query, "id");
		if (IsArcGISItemId(queryId))
		{
			return queryId;
		}

		var segments = sourceUri.AbsolutePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
		for (var i = 0; i < segments.Length - 1; i++)
		{
			if (segments[i].Equals("items", StringComparison.OrdinalIgnoreCase) && IsArcGISItemId(segments[i + 1]))
			{
				return segments[i + 1];
			}
		}

		return null;
	}

	private static string GetQueryValue(string query, string key)
	{
		if (string.IsNullOrEmpty(query))
		{
			return null;
		}

		var trimmedQuery = query[0] == '?' ? query.Substring(1) : query;
		foreach (var pair in trimmedQuery.Split('&'))
		{
			var equalsIndex = pair.IndexOf('=');
			if (equalsIndex <= 0)
			{
				continue;
			}

			var pairKey = Uri.UnescapeDataString(pair.Substring(0, equalsIndex));
			if (pairKey.Equals(key, StringComparison.OrdinalIgnoreCase))
			{
				return Uri.UnescapeDataString(pair.Substring(equalsIndex + 1));
			}
		}

		return null;
	}

	private static bool IsArcGISItemId(string value)
	{
		if (string.IsNullOrEmpty(value) || value.Length != 32)
		{
			return false;
		}

		for (var i = 0; i < value.Length; i++)
		{
			var character = value[i];
			if (!((character >= '0' && character <= '9') ||
				(character >= 'a' && character <= 'f') ||
				(character >= 'A' && character <= 'F')))
			{
				return false;
			}
		}

		return true;
	}

	private static void AddSceneServerMetadataUrlsFromJson(string json, List<string> urls)
	{
		if (string.IsNullOrEmpty(json))
		{
			return;
		}

		try
		{
			var root = JToken.Parse(json);
			foreach (var urlToken in root.SelectTokens("$..url"))
			{
				AddSceneServerMetadataUrls(urlToken.Type == JTokenType.String ? urlToken.Value<string>() : null, urls);
			}
		}
		catch
		{
		}
	}

	private static void AddSceneServerMetadataUrls(string source, List<string> urls)
	{
		if (string.IsNullOrEmpty(source) || urls == null)
		{
			return;
		}

		var sourceWithoutQuery = StripQueryAndFragment(source.Trim());
		var sceneServerIndex = sourceWithoutQuery.IndexOf("/SceneServer", StringComparison.OrdinalIgnoreCase);
		if (sceneServerIndex < 0)
		{
			return;
		}

		var sceneServerRoot = sourceWithoutQuery.Substring(0, sceneServerIndex + "/SceneServer".Length);
		var layerIndex = sourceWithoutQuery.IndexOf("/layers/", sceneServerIndex, StringComparison.OrdinalIgnoreCase);
		if (layerIndex >= 0)
		{
			AddMetadataJsonUrl(sourceWithoutQuery, urls);
		}
		else
		{
			AddMetadataJsonUrl(sceneServerRoot + "/layers/0", urls);
		}

		AddMetadataJsonUrl(sceneServerRoot, urls);
	}

	private static void AddMetadataJsonUrl(string url, List<string> urls)
	{
		var jsonUrl = StripQueryAndFragment(url) + "?f=json";
		foreach (var existingUrl in urls)
		{
			if (existingUrl.Equals(jsonUrl, StringComparison.OrdinalIgnoreCase))
			{
				return;
			}
		}

		urls.Add(jsonUrl);
	}

	private static string StripQueryAndFragment(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return value;
		}

		var separatorIndex = value.IndexOfAny(new[] { '?', '#' });
		return separatorIndex >= 0 ? value.Substring(0, separatorIndex) : value;
	}

	private static ClassRendererInfo TryParseClassRendererInfo(string json, string fallbackAttributeName)
	{
		if (string.IsNullOrEmpty(json))
		{
			return null;
		}

		try
		{
			var root = JToken.Parse(json);
			foreach (var renderer in GetRendererTokens(root))
			{
				var rendererInfo = TryParseUniqueValueRenderer(renderer, fallbackAttributeName);
				if (rendererInfo != null && rendererInfo.Values.Count > 0)
				{
					return rendererInfo;
				}
			}
		}
		catch
		{
		}

		return null;
	}

	private static ClassRendererInfo TryCreateStandardClassRendererInfoFromMetadata(string json, string attributeName)
	{
		var classValues = TryParseClassValues(json, attributeName);
		if (classValues == null || classValues.Count == 0)
		{
			return null;
		}

		var rendererInfo = new ClassRendererInfo
		{
			AttributeName = attributeName,
			TransformType = ArcGISPointCloudAttributeTransformType.None
		};

		var sortedValues = new List<int>(classValues);
		sortedValues.Sort();

		foreach (var classValue in sortedValues)
		{
			AddStandardClassRendererInfoValue(rendererInfo, classValue);
		}

		return rendererInfo.Values.Count > 0 ? rendererInfo : null;
	}

	private static HashSet<int> TryParseClassValues(string json, string attributeName)
	{
		if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(attributeName))
		{
			return null;
		}

		var classValues = new HashSet<int>();
		var normalizedAttributeName = NormalizeName(attributeName);

		try
		{
			var root = JToken.Parse(json);
			CollectClassValuesFromMetadata(root, normalizedAttributeName, classValues);
		}
		catch
		{
		}

		return classValues.Count > 0 ? classValues : null;
	}

	private static void CollectClassValuesFromMetadata(JToken token, string normalizedAttributeName, HashSet<int> classValues)
	{
		if (token == null)
		{
			return;
		}

		if (token is JObject jsonObject)
		{
			if (ObjectReferencesAttribute(jsonObject, normalizedAttributeName))
			{
				CollectKnownClassValueProperties(jsonObject, classValues);
			}

			foreach (var property in jsonObject.Properties())
			{
				if (NormalizeName(property.Name) == normalizedAttributeName)
				{
					CollectKnownClassValueProperties(property.Value, classValues);
				}

				CollectClassValuesFromMetadata(property.Value, normalizedAttributeName, classValues);
			}

			return;
		}

		if (token is JArray jsonArray)
		{
			foreach (var child in jsonArray.Children())
			{
				CollectClassValuesFromMetadata(child, normalizedAttributeName, classValues);
			}
		}
	}

	private static bool ObjectReferencesAttribute(JObject jsonObject, string normalizedAttributeName)
	{
		foreach (var propertyName in new[] { "name", "field", "fieldName", "attribute", "attributeName", "key" })
		{
			var value = ReadString(jsonObject, propertyName);
			if (NormalizeName(value) == normalizedAttributeName)
			{
				return true;
			}
		}

		return false;
	}

	private static void CollectKnownClassValueProperties(JToken token, HashSet<int> classValues)
	{
		if (token == null)
		{
			return;
		}

		if (token is JObject jsonObject)
		{
			foreach (var property in jsonObject.Properties())
			{
				var normalizedPropertyName = NormalizeName(property.Name);
				if (IsClassValueProperty(normalizedPropertyName))
				{
					CollectIntegralClassValues(property.Value, classValues);
				}
				else
				{
					CollectKnownClassValueProperties(property.Value, classValues);
				}
			}

			return;
		}

		if (token is JArray jsonArray)
		{
			foreach (var child in jsonArray.Children())
			{
				CollectKnownClassValueProperties(child, classValues);
			}
		}
	}

	private static bool IsClassValueProperty(string normalizedPropertyName)
	{
		switch (normalizedPropertyName)
		{
			case "VALUE":
			case "VALUES":
			case "UNIQUEVALUE":
			case "UNIQUEVALUES":
			case "CODE":
			case "CODES":
			case "CLASSCODE":
			case "CLASSCODES":
			case "CLASSIFICATION":
			case "CLASSIFICATIONS":
			case "CODEDVALUES":
				return true;
			default:
				return false;
		}
	}

	private static void CollectIntegralClassValues(JToken token, HashSet<int> classValues)
	{
		if (token == null)
		{
			return;
		}

		if (token is JArray jsonArray)
		{
			foreach (var child in jsonArray.Children())
			{
				CollectIntegralClassValues(child, classValues);
			}

			return;
		}

		if (token is JObject jsonObject)
		{
			foreach (var property in jsonObject.Properties())
			{
				var normalizedPropertyName = NormalizeName(property.Name);
				if (IsClassValueProperty(normalizedPropertyName))
				{
					CollectIntegralClassValues(property.Value, classValues);
				}
			}

			return;
		}

		if (TryReadNumber(token, out var number))
		{
			var classValue = Mathf.RoundToInt((float)number);
			if (Math.Abs(number - classValue) < 0.0001d && classValue >= 0 && classValue <= 255)
			{
				classValues.Add(classValue);
			}
		}
	}

	private static IEnumerable<JToken> GetRendererTokens(JToken root)
	{
		var directRenderer = root.SelectToken("drawingInfo.renderer") ?? root.SelectToken("renderer");
		if (directRenderer != null)
		{
			yield return directRenderer;
		}

		foreach (var renderer in root.SelectTokens("$..renderer"))
		{
			yield return renderer;
		}
	}

	private static ClassRendererInfo TryParseUniqueValueRenderer(JToken renderer, string fallbackAttributeName)
	{
		var uniqueValueInfos = GetFirstToken(renderer, "colorUniqueValueInfos", "uniqueValueInfos");
		if (uniqueValueInfos == null || uniqueValueInfos.Type != JTokenType.Array)
		{
			return null;
		}

		var rendererInfo = new ClassRendererInfo
		{
			AttributeName = ReadString(renderer, "field", "fieldName", "attribute", "attributeName") ?? fallbackAttributeName,
			TransformType = ParseTransform(ReadString(renderer, "fieldTransformType", "transformType", "fieldTransform"))
		};

		var index = 0;
		foreach (var uniqueValueInfo in uniqueValueInfos.Children())
		{
			var values = ReadValueList(GetFirstToken(uniqueValueInfo, "values", "value", "valuesCollection"));
			if (values.Length == 0)
			{
				continue;
			}

			var label = ReadString(uniqueValueInfo, "label", "description");
			if (string.IsNullOrEmpty(label))
			{
				label = string.Join(", ", values);
			}

			if (!TryReadColor(
				GetFirstToken(uniqueValueInfo, "color") ??
				uniqueValueInfo.SelectToken("symbol.color") ??
				uniqueValueInfo.SelectToken("symbol.layers[0].material.color"),
				index,
				out var red,
				out var green,
				out var blue,
				out var alpha))
			{
				GetFallbackClassColor(index, out red, out green, out blue, out alpha);
			}

			AddClassRendererInfoValue(rendererInfo, values, label, red, green, blue, alpha);
			index++;
		}

		return rendererInfo.Values.Count > 0 ? rendererInfo : null;
	}

	private static string[] ReadValueList(JToken token)
	{
		var values = new List<string>();
		AddValueToken(token, values);

		var uniqueValues = new List<string>();
		foreach (var value in values)
		{
			if (!string.IsNullOrEmpty(value) && !uniqueValues.Contains(value))
			{
				uniqueValues.Add(value);
			}
		}

		return uniqueValues.ToArray();
	}

	private static void AddValueToken(JToken token, List<string> values)
	{
		if (token == null)
		{
			return;
		}

		if (token.Type == JTokenType.Array)
		{
			foreach (var child in token.Children())
			{
				AddValueToken(child, values);
			}

			return;
		}

		if (token.Type == JTokenType.Object)
		{
			AddValueToken(GetFirstToken(token, "values", "value"), values);
			return;
		}

		if (token is JValue valueToken && valueToken.Value != null)
		{
			values.Add(Convert.ToString(valueToken.Value, CultureInfo.InvariantCulture)?.Trim());
		}
	}

	private static bool TryReadColor(JToken token, int fallbackIndex, out byte red, out byte green, out byte blue, out byte alpha)
	{
		red = 0;
		green = 0;
		blue = 0;
		alpha = 255;

		if (token == null)
		{
			return false;
		}

		if (token.Type == JTokenType.Array)
		{
			var components = new List<double>();
			foreach (var component in token.Children())
			{
				if (TryReadNumber(component, out var value))
				{
					components.Add(value);
				}
			}

			if (components.Count < 3)
			{
				return false;
			}

			var normalized = components[0] <= 1d && components[1] <= 1d && components[2] <= 1d;
			red = ToByteColorComponent(components[0], normalized);
			green = ToByteColorComponent(components[1], normalized);
			blue = ToByteColorComponent(components[2], normalized);
			alpha = components.Count > 3 ? ToByteColorComponent(components[3], normalized && components[3] <= 1d) : (byte)255;
			return true;
		}

		if (token.Type == JTokenType.Object)
		{
			var nestedColor = GetFirstToken(token, "color", "rgb", "rgba");
			if (nestedColor != null && TryReadColor(nestedColor, fallbackIndex, out red, out green, out blue, out alpha))
			{
				return true;
			}

			if (!TryReadNumber(GetFirstToken(token, "r", "red"), out var redValue) ||
				!TryReadNumber(GetFirstToken(token, "g", "green"), out var greenValue) ||
				!TryReadNumber(GetFirstToken(token, "b", "blue"), out var blueValue))
			{
				return false;
			}

			var alphaToken = GetFirstToken(token, "a", "alpha");
			var normalized = redValue <= 1d && greenValue <= 1d && blueValue <= 1d;
			red = ToByteColorComponent(redValue, normalized);
			green = ToByteColorComponent(greenValue, normalized);
			blue = ToByteColorComponent(blueValue, normalized);
			alpha = alphaToken != null && TryReadNumber(alphaToken, out var alphaValue)
				? ToByteColorComponent(alphaValue, normalized && alphaValue <= 1d)
				: (byte)255;
			return true;
		}

		return false;
	}

	private static bool TryReadNumber(JToken token, out double value)
	{
		value = 0d;
		if (token == null)
		{
			return false;
		}

		if (token.Type == JTokenType.Integer || token.Type == JTokenType.Float)
		{
			value = token.Value<double>();
			return true;
		}

		return double.TryParse(token.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out value);
	}

	private static byte ToByteColorComponent(double value, bool normalized)
	{
		return (byte)Mathf.Clamp(Mathf.RoundToInt((float)(normalized ? value * 255d : value)), 0, 255);
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

	private static JToken GetFirstToken(JToken token, params string[] names)
	{
		if (!(token is JObject jsonObject))
		{
			return null;
		}

		foreach (var name in names)
		{
			if (jsonObject.TryGetValue(name, StringComparison.OrdinalIgnoreCase, out var value))
			{
				return value;
			}
		}

		return null;
	}

	private static string ReadString(JToken token, params string[] names)
	{
		var value = GetFirstToken(token, names);
		return value == null || value.Type == JTokenType.Null ? null : value.ToString();
	}

	private static ArcGISPointCloudAttributeTransformType ParseTransform(string transform)
	{
		switch (NormalizeName(transform))
		{
			case "ABSOLUTEVALUE":
				return ArcGISPointCloudAttributeTransformType.AbsoluteValue;
			case "HIGHFOURBIT":
			case "HIGH4BIT":
				return ArcGISPointCloudAttributeTransformType.HighFourBit;
			case "LOWFOURBIT":
			case "LOW4BIT":
				return ArcGISPointCloudAttributeTransformType.LowFourBit;
			case "MODULOTEN":
			case "MODULO10":
				return ArcGISPointCloudAttributeTransformType.ModuloTen;
			default:
				return ArcGISPointCloudAttributeTransformType.None;
		}
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

	private void UpdateClassLegend(ClassRendererInfo rendererInfo)
	{
		if (!rendererLegendPanel || rendererInfo == null || rendererInfo.Values.Count == 0)
		{
			return;
		}

		var labels = new string[rendererInfo.Values.Count];
		var colors = new UnityEngine.Color[rendererInfo.Values.Count];
		for (var i = 0; i < rendererInfo.Values.Count; i++)
		{
			var value = rendererInfo.Values[i];
			labels[i] = string.IsNullOrEmpty(value.Label) ? string.Join(", ", value.Values) : value.Label;
			colors[i] = new Color32(value.Red, value.Green, value.Blue, value.Alpha);
		}

		rendererLegendPanel.SetClassLegendEntries(labels, colors);
	}

	private void ClearClassLegend()
	{
		if (rendererLegendPanel)
		{
			rendererLegendPanel.SetClassLegendEntries(Array.Empty<string>(), Array.Empty<UnityEngine.Color>());
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
