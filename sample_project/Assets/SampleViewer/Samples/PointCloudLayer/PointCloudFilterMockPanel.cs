using Esri.GameEngine.Layers;
using Esri.GameEngine.Layers.PointCloud;
using Esri.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public sealed class PointCloudFilterMockPanel : MonoBehaviour
{
	private enum FilterGroupKind
	{
		ClassCode,
		Returns
	}

	private sealed class FilterOption
	{
		public string Label;
		public double ClassCode;
		public ArcGISPointCloudReturnsType ReturnType;
		public Toggle Toggle;
	}

	private sealed class FilterGroupState
	{
		public FilterGroupKind Kind;
		public string AttributeName;
		public Toggle AllToggle;
		public readonly List<FilterOption> Options = new List<FilterOption>();
	}

	[SerializeField] private Font font;
	[SerializeField] private Sprite checkboxOutlineSprite;
	[SerializeField] private PointCloudLayerDataLoader dataLoader;
	[SerializeField] private bool logFilterDiagnostics;

	private readonly Color accentColor = new Color(0.56f, 0.25f, 1f, 1f);
	private readonly Color textColor = Color.white;
	private readonly Color mutedTextColor = new Color(0.78f, 0.78f, 0.78f, 1f);
	private readonly Color scrollbarTrackColor = new Color(0.28f, 0.28f, 0.28f, 0.82f);
	private readonly Color scrollbarHandleColor = new Color(0.75f, 0.75f, 0.75f, 1f);

	private readonly List<FilterGroupState> groups = new List<FilterGroupState>();

	private RectTransform rectTransform;
	private Coroutine runtimeRebuildCoroutine;
	private ArcGISPointCloudLayer activeLayer;
	private ArcGISCollection<ArcGISPointCloudFilter> activeFilterCollection;
	private ArcGISCollection<double> activeClassCodeValues;
	private ArcGISCollection<ArcGISPointCloudReturnsType> activeReturnsValues;
	private ArcGISPointCloudValueFilter activeClassCodeFilter;
	private ArcGISPointCloudReturnFilter activeReturnsFilter;
	private bool subscribed;
	private bool suppressToggleEvents;
#if UNITY_EDITOR
	private bool rebuildQueued;
#endif

	private void OnEnable()
	{
		FindReferences();
		Subscribe();
		activeLayer = Application.isPlaying && dataLoader ? dataLoader.LoadedLayer : null;
		QueueBuild();
	}

	private void OnDisable()
	{
		Unsubscribe();

		if (runtimeRebuildCoroutine != null)
		{
			StopCoroutine(runtimeRebuildCoroutine);
			runtimeRebuildCoroutine = null;
		}

#if UNITY_EDITOR
		if (rebuildQueued)
		{
			EditorApplication.delayCall -= RebuildInEditor;
			rebuildQueued = false;
		}
#endif
	}

	private void OnValidate()
	{
		FindReferences();
		QueueBuild();
	}

	private void FindReferences()
	{
		if (!dataLoader)
		{
			dataLoader = GetComponentInParent<PointCloudLayerDataLoader>();
		}
	}

	private void Subscribe()
	{
		if (subscribed || !dataLoader)
		{
			return;
		}

		dataLoader.LayerLoaded += HandleLayerLoaded;
		subscribed = true;
	}

	private void Unsubscribe()
	{
		if (subscribed && dataLoader)
		{
			dataLoader.LayerLoaded -= HandleLayerLoaded;
		}

		subscribed = false;
	}

	private void HandleLayerLoaded(ArcGISPointCloudLayer layer)
	{
		activeLayer = layer;
		QueueBuild();
	}

	private void QueueBuild()
	{
#if UNITY_EDITOR
		if (!Application.isPlaying)
		{
			if (rebuildQueued)
			{
				return;
			}

			rebuildQueued = true;
			EditorApplication.delayCall += RebuildInEditor;
			return;
		}
#endif

		if (runtimeRebuildCoroutine != null || !isActiveAndEnabled)
		{
			return;
		}

		runtimeRebuildCoroutine = StartCoroutine(RebuildNextFrame());
	}

	private IEnumerator RebuildNextFrame()
	{
		yield return null;
		runtimeRebuildCoroutine = null;

		if (!this || !isActiveAndEnabled)
		{
			yield break;
		}

		EnsureComponents();
		Build();
	}

#if UNITY_EDITOR
	private void RebuildInEditor()
	{
		EditorApplication.delayCall -= RebuildInEditor;
		rebuildQueued = false;

		if (!this || Application.isPlaying)
		{
			return;
		}

		EnsureComponents();
		Build();
	}
#endif

	private void EnsureComponents()
	{
		rectTransform = GetComponent<RectTransform>();

		if (!rectTransform)
		{
			rectTransform = gameObject.AddComponent<RectTransform>();
		}

		if (!font)
		{
			font = Resources.GetBuiltinResource<Font>("Arial.ttf");
		}

		if (!checkboxOutlineSprite)
		{
			checkboxOutlineSprite = Resources.Load<Sprite>("Images/checkboxOutline");
		}
	}

	private void Build()
	{
		if (!rectTransform)
		{
			return;
		}

		ClearGeneratedChildren();
		groups.Clear();

		if (!Application.isPlaying || activeLayer == null)
		{
			return;
		}

		var classCodeAttribute = FindAttributeName(activeLayer, "CLASSCODE", "CLASSIFICATION", "CLASS");
		var returnsAttribute = FindAttributeName(activeLayer, "RETURNS");
		var hasClassCodeFilter = !string.IsNullOrEmpty(classCodeAttribute);
		var hasReturnsFilter = !string.IsNullOrEmpty(returnsAttribute);

		if (!hasClassCodeFilter && !hasReturnsFilter)
		{
			ClearActiveFilters();
			return;
		}

		const float filterPanelYOffset = -35f;

		if (hasClassCodeFilter)
		{
			AddFilterGroup(FilterGroupKind.ClassCode, classCodeAttribute, "Class Code", CreateClassCodeOptions(), 175f + filterPanelYOffset, 38f + filterPanelYOffset, 212f);
		}

		if (hasClassCodeFilter && hasReturnsFilter)
		{
			AddSeparator(-105f + filterPanelYOffset);
		}

		if (hasReturnsFilter)
		{
			var titleY = (hasClassCodeFilter ? -150f : 175f) + filterPanelYOffset;
			var viewportCenterY = (hasClassCodeFilter ? -270f : 38f) + filterPanelYOffset;
			var viewportHeight = hasClassCodeFilter ? 180f : 210f;
			AddFilterGroup(FilterGroupKind.Returns, returnsAttribute, "Returns", CreateReturnOptions(), titleY, viewportCenterY, viewportHeight);
		}

		ApplyFilters();
	}

	private void AddFilterGroup(FilterGroupKind kind, string attributeName, string title, FilterOption[] options, float titleY, float viewportCenterY, float viewportHeight)
	{
		const float rowSpacing = 44f;
		const float verticalPadding = 0f;
		var rowCount = options.Length + 1;
		var contentHeight = Mathf.Max(viewportHeight, rowCount * rowSpacing + verticalPadding * 2f);
		var state = new FilterGroupState
		{
			Kind = kind,
			AttributeName = attributeName
		};

		AddText("Generated_FilterTitle_" + title, title, new Vector2(15f, titleY), new Vector2(560f, 48f), 30, TextAnchor.MiddleLeft, mutedTextColor, rectTransform);

		var content = AddScrollArea(
			"Generated_FilterScroll_" + title,
			new Vector2(-35f, viewportCenterY),
			new Vector2(480f, viewportHeight),
			contentHeight,
			new Vector2(275f, viewportCenterY));

		var allRowY = contentHeight * 0.5f - verticalPadding - rowSpacing * 0.5f;
		state.AllToggle = AddCheckbox("Generated_FilterCheckbox_" + title + "_All", new Vector2(-210f, allRowY), content);
		AddText("Generated_FilterLabel_" + title + "_All", "<all>", new Vector2(30f, allRowY), new Vector2(410f, 42f), 28, TextAnchor.MiddleLeft, textColor, content);
		state.AllToggle.onValueChanged.AddListener(isOn => HandleAllToggleChanged(state, isOn));

		for (var i = 0; i < options.Length; i++)
		{
			var option = options[i];
			var y = allRowY - (i + 1) * rowSpacing;
			option.Toggle = AddCheckbox("Generated_FilterCheckbox_" + title + "_" + i, new Vector2(-210f, y), content);
			option.Toggle.onValueChanged.AddListener(_ => HandleOptionToggleChanged(state));
			state.Options.Add(option);
			AddText("Generated_FilterLabel_" + title + "_" + i, option.Label, new Vector2(30f, y), new Vector2(410f, 42f), 28, TextAnchor.MiddleLeft, textColor, content);
		}

		groups.Add(state);
	}

	private RectTransform AddScrollArea(string name, Vector2 anchoredPosition, Vector2 size, float contentHeight, Vector2 scrollbarPosition)
	{
		var scrollRoot = new GameObject(name, typeof(RectTransform), typeof(ScrollRect));
		MarkGeneratedObject(scrollRoot);
		scrollRoot.layer = gameObject.layer;
		scrollRoot.transform.SetParent(rectTransform, false);

		var scrollRectTransform = scrollRoot.GetComponent<RectTransform>();
		scrollRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
		scrollRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
		scrollRectTransform.pivot = new Vector2(0.5f, 0.5f);
		scrollRectTransform.anchoredPosition = anchoredPosition;
		scrollRectTransform.sizeDelta = size;

		var viewport = new GameObject(name + "_Viewport", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(RectMask2D));
		MarkGeneratedObject(viewport);
		viewport.layer = gameObject.layer;
		viewport.transform.SetParent(scrollRoot.transform, false);

		var viewportRect = viewport.GetComponent<RectTransform>();
		viewportRect.anchorMin = Vector2.zero;
		viewportRect.anchorMax = Vector2.one;
		viewportRect.pivot = new Vector2(0.5f, 0.5f);
		viewportRect.offsetMin = Vector2.zero;
		viewportRect.offsetMax = Vector2.zero;

		var viewportImage = viewport.GetComponent<Image>();
		viewportImage.color = Color.clear;
		viewportImage.raycastTarget = true;

		var content = new GameObject(name + "_Content", typeof(RectTransform));
		MarkGeneratedObject(content);
		content.layer = gameObject.layer;
		content.transform.SetParent(viewport.transform, false);

		var contentRect = content.GetComponent<RectTransform>();
		contentRect.anchorMin = new Vector2(0f, 1f);
		contentRect.anchorMax = new Vector2(1f, 1f);
		contentRect.pivot = new Vector2(0.5f, 1f);
		contentRect.anchoredPosition = Vector2.zero;
		contentRect.sizeDelta = new Vector2(0f, contentHeight);

		var scrollbar = AddScrollbar(name + "_Scrollbar", scrollbarPosition, new Vector2(28f, size.y));

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
		scrollbar.size = Mathf.Clamp01(size.y / contentHeight);
		return contentRect;
	}

	private Scrollbar AddScrollbar(string name, Vector2 anchoredPosition, Vector2 size)
	{
		var scrollbarGo = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Scrollbar));
		MarkGeneratedObject(scrollbarGo);
		scrollbarGo.layer = gameObject.layer;
		scrollbarGo.transform.SetParent(rectTransform, false);

		var scrollbarRect = scrollbarGo.GetComponent<RectTransform>();
		scrollbarRect.anchorMin = new Vector2(0.5f, 0.5f);
		scrollbarRect.anchorMax = new Vector2(0.5f, 0.5f);
		scrollbarRect.pivot = new Vector2(0.5f, 0.5f);
		scrollbarRect.anchoredPosition = anchoredPosition;
		scrollbarRect.sizeDelta = size;

		var track = scrollbarGo.GetComponent<Image>();
		track.color = scrollbarTrackColor;
		track.raycastTarget = true;

		var slidingArea = new GameObject(name + "_SlidingArea", typeof(RectTransform));
		MarkGeneratedObject(slidingArea);
		slidingArea.layer = gameObject.layer;
		slidingArea.transform.SetParent(scrollbarGo.transform, false);

		var slidingAreaRect = slidingArea.GetComponent<RectTransform>();
		slidingAreaRect.anchorMin = Vector2.zero;
		slidingAreaRect.anchorMax = Vector2.one;
		slidingAreaRect.pivot = new Vector2(0.5f, 0.5f);
		slidingAreaRect.offsetMin = Vector2.zero;
		slidingAreaRect.offsetMax = Vector2.zero;

		var handle = new GameObject(name + "_Handle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
		MarkGeneratedObject(handle);
		handle.layer = gameObject.layer;
		handle.transform.SetParent(slidingArea.transform, false);

		var handleRect = handle.GetComponent<RectTransform>();
		handleRect.anchorMin = Vector2.zero;
		handleRect.anchorMax = Vector2.one;
		handleRect.pivot = new Vector2(0.5f, 0.5f);
		handleRect.offsetMin = Vector2.zero;
		handleRect.offsetMax = Vector2.zero;

		var handleImage = handle.GetComponent<Image>();
		handleImage.color = scrollbarHandleColor;
		handleImage.raycastTarget = true;

		var scrollbar = scrollbarGo.GetComponent<Scrollbar>();
		scrollbar.direction = Scrollbar.Direction.BottomToTop;
		scrollbar.targetGraphic = handleImage;
		scrollbar.handleRect = handleRect;
		scrollbar.transition = Selectable.Transition.None;
		return scrollbar;
	}

	private Toggle AddCheckbox(string name, Vector2 anchoredPosition, Transform parent)
	{
		var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Toggle));
		MarkGeneratedObject(go);
		go.layer = gameObject.layer;
		go.transform.SetParent(parent, false);

		var rect = go.GetComponent<RectTransform>();
		rect.anchorMin = new Vector2(0.5f, 0.5f);
		rect.anchorMax = new Vector2(0.5f, 0.5f);
		rect.pivot = new Vector2(0.5f, 0.5f);
		rect.anchoredPosition = anchoredPosition;
		rect.sizeDelta = new Vector2(30f, 30f);

		var hitArea = go.GetComponent<Image>();
		hitArea.color = Color.clear;
		hitArea.raycastTarget = true;

		var fill = new GameObject(name + "_Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
		MarkGeneratedObject(fill);
		fill.layer = gameObject.layer;
		fill.transform.SetParent(go.transform, false);

		var fillRect = fill.GetComponent<RectTransform>();
		fillRect.anchorMin = Vector2.zero;
		fillRect.anchorMax = Vector2.one;
		fillRect.pivot = new Vector2(0.5f, 0.5f);
		fillRect.offsetMin = new Vector2(5f, 5f);
		fillRect.offsetMax = new Vector2(-5f, -5f);

		var fillImage = fill.GetComponent<Image>();
		fillImage.color = accentColor;
		fillImage.raycastTarget = false;

		var outline = new GameObject(name + "_Outline", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
		MarkGeneratedObject(outline);
		outline.layer = gameObject.layer;
		outline.transform.SetParent(go.transform, false);

		var outlineRect = outline.GetComponent<RectTransform>();
		outlineRect.anchorMin = Vector2.zero;
		outlineRect.anchorMax = Vector2.one;
		outlineRect.pivot = new Vector2(0.5f, 0.5f);
		outlineRect.offsetMin = Vector2.zero;
		outlineRect.offsetMax = Vector2.zero;

		var outlineImage = outline.GetComponent<Image>();
		outlineImage.sprite = checkboxOutlineSprite;
		outlineImage.color = Color.white;
		outlineImage.preserveAspect = true;
		outlineImage.raycastTarget = false;

		var toggle = go.GetComponent<Toggle>();
		toggle.transition = Selectable.Transition.None;
		toggle.targetGraphic = hitArea;
		toggle.graphic = fillImage;
		toggle.isOn = true;
		return toggle;
	}

	private void AddSeparator(float y)
	{
		var line = new GameObject("Generated_FilterSeparator", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
		MarkGeneratedObject(line);
		line.layer = gameObject.layer;
		line.transform.SetParent(rectTransform, false);

		var rect = line.GetComponent<RectTransform>();
		rect.anchorMin = new Vector2(0.5f, 0.5f);
		rect.anchorMax = new Vector2(0.5f, 0.5f);
		rect.pivot = new Vector2(0.5f, 0.5f);
		rect.anchoredPosition = new Vector2(0f, y);
		rect.sizeDelta = new Vector2(250f, 3f);

		var image = line.GetComponent<Image>();
		image.color = mutedTextColor;
		image.raycastTarget = false;
	}

	private Text AddText(string name, string text, Vector2 anchoredPosition, Vector2 size, int fontSize, TextAnchor alignment, Color color, Transform parent)
	{
		var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
		MarkGeneratedObject(go);
		go.layer = gameObject.layer;
		go.transform.SetParent(parent, false);

		var rect = go.GetComponent<RectTransform>();
		rect.anchorMin = new Vector2(0.5f, 0.5f);
		rect.anchorMax = new Vector2(0.5f, 0.5f);
		rect.pivot = new Vector2(0.5f, 0.5f);
		rect.anchoredPosition = anchoredPosition;
		rect.sizeDelta = size;

		var textComponent = go.GetComponent<Text>();
		textComponent.text = text;
		textComponent.font = font;
		textComponent.fontSize = fontSize;
		textComponent.alignment = alignment;
		textComponent.supportRichText = false;
		textComponent.horizontalOverflow = HorizontalWrapMode.Overflow;
		textComponent.verticalOverflow = VerticalWrapMode.Overflow;
		textComponent.color = color;
		textComponent.raycastTarget = false;
		return textComponent;
	}

	private void HandleAllToggleChanged(FilterGroupState changedGroup, bool isOn)
	{
		if (suppressToggleEvents)
		{
			return;
		}

		suppressToggleEvents = true;

		foreach (var option in changedGroup.Options)
		{
			if (option.Toggle)
			{
				option.Toggle.isOn = isOn;
			}
		}

		if (changedGroup.Kind == FilterGroupKind.ClassCode && !isOn)
		{
			var returnsGroup = GetGroup(FilterGroupKind.Returns);
			if (returnsGroup != null)
			{
				SetGroupSelection(returnsGroup, false);
			}
		}

		suppressToggleEvents = false;
		ApplyFilters();
	}

	private void HandleOptionToggleChanged(FilterGroupState changedGroup)
	{
		if (suppressToggleEvents)
		{
			return;
		}

		var allSelected = AreAllOptionsSelected(changedGroup);
		var anySelected = IsAnyOptionSelected(changedGroup);
		suppressToggleEvents = true;

		if (changedGroup.AllToggle && changedGroup.AllToggle.isOn != allSelected)
		{
			changedGroup.AllToggle.isOn = allSelected;
		}

		if (changedGroup.Kind == FilterGroupKind.ClassCode && !anySelected)
		{
			var returnsGroup = GetGroup(FilterGroupKind.Returns);
			if (returnsGroup != null)
			{
				SetGroupSelection(returnsGroup, false);
			}
		}

		suppressToggleEvents = false;
		ApplyFilters();
	}

	private void SetGroupSelection(FilterGroupState group, bool isOn)
	{
		if (group.AllToggle)
		{
			group.AllToggle.isOn = isOn;
		}

		foreach (var option in group.Options)
		{
			if (option.Toggle)
			{
				option.Toggle.isOn = isOn;
			}
		}
	}

	private void ApplyFilters()
	{
		if (!Application.isPlaying || activeLayer == null)
		{
			return;
		}

		try
		{
			activeFilterCollection = new ArcGISCollection<ArcGISPointCloudFilter>();
			activeClassCodeValues = null;
			activeReturnsValues = null;
			activeClassCodeFilter = null;
			activeReturnsFilter = null;

			var classGroup = GetGroup(FilterGroupKind.ClassCode);
			var returnsGroup = GetGroup(FilterGroupKind.Returns);

			if (classGroup != null && !AreAllOptionsSelected(classGroup))
			{
				activeFilterCollection.Add(CreateClassCodeFilter(classGroup));
			}

			if (returnsGroup != null && !AreAllOptionsSelected(returnsGroup))
			{
				activeFilterCollection.Add(CreateReturnsFilter(returnsGroup));
			}

			activeLayer.Filters = activeFilterCollection;
			LogFilterDiagnostics(classGroup, returnsGroup);
		}
		catch (Exception exception)
		{
			Debug.LogWarning("Failed to apply point cloud filters: " + exception.Message);
		}
	}

	private ArcGISPointCloudValueFilter CreateClassCodeFilter(FilterGroupState group)
	{
		activeClassCodeValues = new ArcGISCollection<double>();
		foreach (var option in group.Options)
		{
			if (option.Toggle && option.Toggle.isOn)
			{
				activeClassCodeValues.Add(option.ClassCode);
			}
		}

		activeClassCodeFilter = new ArcGISPointCloudValueFilter(group.AttributeName, activeClassCodeValues, ArcGISPointCloudValueFilterMode.Include);
		return activeClassCodeFilter;
	}

	private ArcGISPointCloudReturnFilter CreateReturnsFilter(FilterGroupState group)
	{
		activeReturnsValues = new ArcGISCollection<ArcGISPointCloudReturnsType>();
		foreach (var option in group.Options)
		{
			if (option.Toggle && option.Toggle.isOn)
			{
				activeReturnsValues.Add(option.ReturnType);
			}
		}

		activeReturnsFilter = new ArcGISPointCloudReturnFilter(group.AttributeName, activeReturnsValues);
		return activeReturnsFilter;
	}

	private void ClearActiveFilters()
	{
		if (activeLayer != null)
		{
			activeFilterCollection = new ArcGISCollection<ArcGISPointCloudFilter>();
			activeLayer.Filters = activeFilterCollection;
		}

		activeClassCodeValues = null;
		activeReturnsValues = null;
		activeClassCodeFilter = null;
		activeReturnsFilter = null;
	}

	private void LogFilterDiagnostics(FilterGroupState classGroup, FilterGroupState returnsGroup)
	{
		if (!logFilterDiagnostics)
		{
			return;
		}

		var filterCount = activeFilterCollection == null ? 0ul : activeFilterCollection.GetSize();
		var classCount = activeClassCodeValues == null ? 0ul : activeClassCodeValues.GetSize();
		var returnsCount = activeReturnsValues == null ? 0ul : activeReturnsValues.GetSize();
		Debug.LogFormat(
			"Point cloud filters applied. filters={0}, classAttribute={1}, selectedClasses={2}, returnsAttribute={3}, selectedReturns={4}",
			filterCount,
			classGroup == null ? "<none>" : classGroup.AttributeName,
			classCount,
			returnsGroup == null ? "<none>" : returnsGroup.AttributeName,
			returnsCount);
	}

	private FilterGroupState GetGroup(FilterGroupKind kind)
	{
		foreach (var group in groups)
		{
			if (group.Kind == kind)
			{
				return group;
			}
		}

		return null;
	}

	private static bool AreAllOptionsSelected(FilterGroupState group)
	{
		if (group == null || group.Options.Count == 0)
		{
			return false;
		}

		foreach (var option in group.Options)
		{
			if (!option.Toggle || !option.Toggle.isOn)
			{
				return false;
			}
		}

		return true;
	}

	private static bool IsAnyOptionSelected(FilterGroupState group)
	{
		if (group == null)
		{
			return false;
		}

		foreach (var option in group.Options)
		{
			if (option.Toggle && option.Toggle.isOn)
			{
				return true;
			}
		}

		return false;
	}

	private static FilterOption[] CreateClassCodeOptions()
	{
		var options = new FilterOption[19];
		for (var classCode = 0; classCode < options.Length; classCode++)
		{
			options[classCode] = new FilterOption
			{
				Label = GetClassCodeLabel(classCode),
				ClassCode = classCode
			};
		}

		return options;
	}

	private static FilterOption[] CreateReturnOptions()
	{
		return new[]
		{
			new FilterOption { Label = "First of many", ReturnType = ArcGISPointCloudReturnsType.FirstOfMany },
			new FilterOption { Label = "Last", ReturnType = ArcGISPointCloudReturnsType.Last },
			new FilterOption { Label = "Last of many", ReturnType = ArcGISPointCloudReturnsType.LastOfMany },
			new FilterOption { Label = "Single", ReturnType = ArcGISPointCloudReturnsType.Single }
		};
	}

	private static string GetClassCodeLabel(int classCode)
	{
		switch (classCode)
		{
			case 0:
				return "Created, never classified";
			case 1:
				return "Unclassified";
			case 2:
				return "Ground";
			case 3:
				return "Low vegetation";
			case 4:
				return "Medium vegetation";
			case 5:
				return "High vegetation";
			case 6:
				return "Building";
			case 7:
				return "Low point (noise)";
			case 8:
				return "Model key-point";
			case 9:
				return "Water";
			case 10:
				return "Rail";
			case 11:
				return "Road surface";
			case 12:
				return "Overlap points";
			case 13:
				return "Wire guard";
			case 14:
				return "Wire conductor";
			case 15:
				return "Transmission tower";
			case 16:
				return "Wire connector";
			case 17:
				return "Bridge deck";
			case 18:
				return "High noise";
			default:
				return "Class " + classCode.ToString(CultureInfo.InvariantCulture);
		}
	}

	private static string FindAttributeName(ArcGISPointCloudLayer layer, params string[] candidates)
	{
		if (layer == null || layer.Attributes == null)
		{
			return "";
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
			foreach (var candidate in candidates)
			{
				if (normalizedName == candidate || normalizedName.Contains(candidate))
				{
					return name;
				}
			}
		}

		return "";
	}

	private static string NormalizeName(string name)
	{
		return string.IsNullOrEmpty(name) ? "" : name.Replace("_", "").Replace("-", "").Replace(" ", "").ToUpperInvariant();
	}

	private void ClearGeneratedChildren()
	{
		for (var i = transform.childCount - 1; i >= 0; i--)
		{
			var child = transform.GetChild(i);
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

	private static void MarkGeneratedObject(GameObject generatedObject)
	{
#if UNITY_EDITOR
		if (!Application.isPlaying)
		{
			generatedObject.hideFlags = HideFlags.DontSaveInEditor;
		}
#endif
	}
}
