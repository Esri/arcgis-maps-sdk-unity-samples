using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public sealed class RendererLegendMockPanel : MonoBehaviour
{
	private enum RendererMode
	{
		RGB,
		Class,
		Elevation,
		Intensity
	}

	[SerializeField] private Toggle visualizeTab;
	[SerializeField] private Toggle rgbToggle;
	[SerializeField] private Toggle classToggle;
	[SerializeField] private Toggle elevationToggle;
	[SerializeField] private Toggle intensityToggle;
	[SerializeField] private Font font;
	[SerializeField] private Vector2 panelOffset = new Vector2(-140f, 80f);

	private readonly Color panelColor = new Color(0.08f, 0.08f, 0.08f, 0.82f);
	private readonly Color accentColor = new Color(0.56f, 0.25f, 1f, 1f);
	private readonly Color textColor = new Color(0.95f, 0.95f, 0.95f, 1f);
	private readonly Color mutedTextColor = new Color(0.78f, 0.78f, 0.78f, 1f);

	private RectTransform rectTransform;
	private Image background;
	private CanvasGroup canvasGroup;
	private RectTransform panelRect;
	private Image panelBackground;
	private Sprite circleSprite;
	private Sprite triangleSprite;
	private bool isSubscribed;

	private void OnEnable()
	{
		EnsureComponents();
		Subscribe();
		Refresh();
	}

	private void OnDisable()
	{
		Unsubscribe();
	}

	private void OnValidate()
	{
		EnsureComponents();
		Refresh();
	}

	private void EnsureComponents()
	{
		rectTransform = GetComponent<RectTransform>();
		background = GetComponent<Image>();
		canvasGroup = GetComponent<CanvasGroup>();

		if (!rectTransform)
		{
			rectTransform = gameObject.AddComponent<RectTransform>();
		}

		if (!background)
		{
			background = gameObject.AddComponent<Image>();
		}

		if (!canvasGroup)
		{
			canvasGroup = gameObject.AddComponent<CanvasGroup>();
		}

		if (!font)
		{
			font = Resources.GetBuiltinResource<Font>("Arial.ttf");
		}

		background.color = Color.clear;
		background.raycastTarget = false;
		canvasGroup.interactable = false;
		canvasGroup.blocksRaycasts = false;

		rectTransform.anchorMin = Vector2.zero;
		rectTransform.anchorMax = Vector2.one;
		rectTransform.pivot = new Vector2(0.5f, 0.5f);
		rectTransform.offsetMin = Vector2.zero;
		rectTransform.offsetMax = Vector2.zero;
		rectTransform.anchoredPosition = Vector2.zero;
	}

	private void Subscribe()
	{
		if (isSubscribed)
		{
			return;
		}

		AddListener(visualizeTab);
		AddListener(rgbToggle);
		AddListener(classToggle);
		AddListener(elevationToggle);
		AddListener(intensityToggle);
		isSubscribed = true;
	}

	private void Unsubscribe()
	{
		if (!isSubscribed)
		{
			return;
		}

		RemoveListener(visualizeTab);
		RemoveListener(rgbToggle);
		RemoveListener(classToggle);
		RemoveListener(elevationToggle);
		RemoveListener(intensityToggle);
		isSubscribed = false;
	}

	private void AddListener(Toggle toggle)
	{
		if (toggle)
		{
			toggle.onValueChanged.RemoveListener(OnToggleChanged);
			toggle.onValueChanged.AddListener(OnToggleChanged);
		}
	}

	private void RemoveListener(Toggle toggle)
	{
		if (toggle)
		{
			toggle.onValueChanged.RemoveListener(OnToggleChanged);
		}
	}

	private void OnToggleChanged(bool _)
	{
		Refresh();
	}

	private void Refresh()
	{
		if (!rectTransform || !background || !canvasGroup)
		{
			return;
		}

		var shouldShow = visualizeTab && visualizeTab.isOn;
		canvasGroup.alpha = shouldShow ? 1f : 0f;
		canvasGroup.interactable = shouldShow;
		canvasGroup.blocksRaycasts = shouldShow;

		if (!shouldShow)
		{
			return;
		}

		BuildLegend(GetSelectedMode());
	}

	private RendererMode GetSelectedMode()
	{
		if (classToggle && classToggle.isOn)
		{
			return RendererMode.Class;
		}

		if (elevationToggle && elevationToggle.isOn)
		{
			return RendererMode.Elevation;
		}

		if (intensityToggle && intensityToggle.isOn)
		{
			return RendererMode.Intensity;
		}

		return RendererMode.RGB;
	}

	private void BuildLegend(RendererMode mode)
	{
		ClearGeneratedChildren();

		switch (mode)
		{
			case RendererMode.Class:
				CreatePanel(500f, 440f);
				AddAccent();
				AddTitle(150f);
				AddText("Generated_ClassHeader", "Class Code", new Vector2(80f, 70f), new Vector2(430f, 40f), 28, TextAnchor.MiddleLeft, textColor);
				AddClassRows();
				break;
			case RendererMode.Elevation:
				CreatePanel(500f, 400f);
				AddAccent();
				AddTitle(145f);
				AddStretchLegend("Elevation", true);
				break;
			case RendererMode.Intensity:
				CreatePanel(500f, 400f);
				AddAccent();
				AddTitle(145f);
				AddStretchLegend("Intensity", false);
				break;
			default:
				CreatePanel(500f, 110f);
				AddAccent();
				AddText("Generated_NoLegend", "No legend", Vector2.zero, new Vector2(430f, 60f), 34, TextAnchor.MiddleCenter, textColor);
				break;
		}
	}

	private void CreatePanel(float width, float height)
	{
		var go = new GameObject("Generated_Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
		go.layer = gameObject.layer;
		go.transform.SetParent(transform, false);

		panelRect = go.GetComponent<RectTransform>();
		panelRect.anchorMin = new Vector2(1f, 0f);
		panelRect.anchorMax = new Vector2(1f, 0f);
		panelRect.pivot = new Vector2(1f, 0f);
		panelRect.anchoredPosition = panelOffset;
		panelRect.sizeDelta = new Vector2(width, height);

		panelBackground = go.GetComponent<Image>();
		panelBackground.color = panelColor;
		panelBackground.raycastTarget = false;
	}

	private void AddAccent()
	{
		var accent = AddImage("Generated_Accent", Vector2.zero, new Vector2(8f, 0f), accentColor);
		accent.rectTransform.anchorMin = new Vector2(0f, 0f);
		accent.rectTransform.anchorMax = new Vector2(0f, 1f);
		accent.rectTransform.pivot = new Vector2(0f, 0.5f);
		accent.rectTransform.anchoredPosition = Vector2.zero;
		accent.rectTransform.sizeDelta = new Vector2(8f, 0f);
	}

	private void AddTitle(float y)
	{
		AddText("Generated_Title", "Tallinn punktipilv", new Vector2(0f, y), new Vector2(430f, 56f), 36, TextAnchor.MiddleCenter, mutedTextColor);
	}

	private void AddClassRows()
	{
		var labels = new[]
		{
			"Unclassified",
			"Ground",
			"Low vegetation",
			"High vegetation",
			"Building",
			"Low point (noise)",
			"Water"
		};

		var colors = new[]
		{
			new Color(0.75f, 0.47f, 0.04f, 1f),
			new Color(0.84f, 1f, 0.39f, 1f),
			new Color(1f, 0.13f, 0.08f, 1f),
			new Color(0.77f, 0.06f, 1f, 1f),
			new Color(1f, 1f, 0.48f, 1f),
			new Color(0.62f, 0.62f, 0.57f, 1f),
			new Color(0.93f, 0.93f, 0.05f, 1f)
		};

		const float rowSpacing = 36f;
		const float viewportHeight = 250f;
		var contentHeight = labels.Length * rowSpacing;
		var content = AddClassScrollArea(new Vector2(25f, -82f), new Vector2(340f, viewportHeight), contentHeight);

		for (var i = 0; i < labels.Length; i++)
		{
			var y = contentHeight * 0.5f - rowSpacing * 0.5f - i * rowSpacing;
			AddCircle("Generated_ClassDot_" + i, new Vector2(-145f, y), new Vector2(26f, 26f), colors[i], content);
			AddText("Generated_ClassLabel_" + i, labels[i], new Vector2(40f, y), new Vector2(260f, 38f), 26, TextAnchor.MiddleLeft, textColor, content);
		}
	}

	private RectTransform AddClassScrollArea(Vector2 anchoredPosition, Vector2 size, float contentHeight)
	{
		var scrollRoot = new GameObject("Generated_ClassScrollRect", typeof(RectTransform), typeof(ScrollRect));
		scrollRoot.layer = gameObject.layer;
		scrollRoot.transform.SetParent(panelRect, false);

		var scrollRectTransform = scrollRoot.GetComponent<RectTransform>();
		scrollRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
		scrollRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
		scrollRectTransform.pivot = new Vector2(0.5f, 0.5f);
		scrollRectTransform.anchoredPosition = anchoredPosition;
		scrollRectTransform.sizeDelta = size;

		var viewport = new GameObject("Generated_ClassViewport", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(RectMask2D));
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

		var content = new GameObject("Generated_ClassContent", typeof(RectTransform));
		content.layer = gameObject.layer;
		content.transform.SetParent(viewport.transform, false);

		var contentRect = content.GetComponent<RectTransform>();
		contentRect.anchorMin = new Vector2(0f, 1f);
		contentRect.anchorMax = new Vector2(1f, 1f);
		contentRect.pivot = new Vector2(0.5f, 1f);
		contentRect.anchoredPosition = Vector2.zero;
		contentRect.sizeDelta = new Vector2(0f, contentHeight);

		var scrollbar = AddClassScrollbar(new Vector2(205f, -82f), new Vector2(28f, size.y));

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

	private Scrollbar AddClassScrollbar(Vector2 anchoredPosition, Vector2 size)
	{
		var scrollbarGo = new GameObject("Generated_ClassScrollbar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Scrollbar));
		scrollbarGo.layer = gameObject.layer;
		scrollbarGo.transform.SetParent(panelRect, false);

		var scrollbarRect = scrollbarGo.GetComponent<RectTransform>();
		scrollbarRect.anchorMin = new Vector2(0.5f, 0.5f);
		scrollbarRect.anchorMax = new Vector2(0.5f, 0.5f);
		scrollbarRect.pivot = new Vector2(0.5f, 0.5f);
		scrollbarRect.anchoredPosition = anchoredPosition;
		scrollbarRect.sizeDelta = size;

		var track = scrollbarGo.GetComponent<Image>();
		track.color = new Color(0.28f, 0.28f, 0.28f, 0.82f);
		track.raycastTarget = true;

		var slidingArea = new GameObject("Generated_ClassScrollbarSlidingArea", typeof(RectTransform));
		slidingArea.layer = gameObject.layer;
		slidingArea.transform.SetParent(scrollbarGo.transform, false);

		var slidingAreaRect = slidingArea.GetComponent<RectTransform>();
		slidingAreaRect.anchorMin = Vector2.zero;
		slidingAreaRect.anchorMax = Vector2.one;
		slidingAreaRect.pivot = new Vector2(0.5f, 0.5f);
		slidingAreaRect.offsetMin = Vector2.zero;
		slidingAreaRect.offsetMax = Vector2.zero;

		var handle = new GameObject("Generated_ClassScrollbarHandle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
		handle.layer = gameObject.layer;
		handle.transform.SetParent(slidingArea.transform, false);

		var handleRect = handle.GetComponent<RectTransform>();
		handleRect.anchorMin = Vector2.zero;
		handleRect.anchorMax = Vector2.one;
		handleRect.pivot = new Vector2(0.5f, 0.5f);
		handleRect.offsetMin = Vector2.zero;
		handleRect.offsetMax = Vector2.zero;

		var handleImage = handle.GetComponent<Image>();
		handleImage.color = new Color(0.75f, 0.75f, 0.75f, 1f);
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

		AddText("Generated_StretchHeader", label, new Vector2(0f, 70f), new Vector2(270f, 40f), 28, TextAnchor.MiddleLeft, textColor);

		var gradientColors = isElevation
			? new[]
			{
				new Color(0.95f, 0.12f, 0.08f, 1f),
				new Color(1f, 0.9f, 0.2f, 1f),
				new Color(0.35f, 0.95f, 0.48f, 1f),
				new Color(0.25f, 0.82f, 1f, 1f),
				new Color(0.22f, 0.12f, 1f, 1f)
			}
			: new[]
			{
				Color.white,
				new Color(0.65f, 0.65f, 0.65f, 1f),
				new Color(0.16f, 0.16f, 0.16f, 1f),
				Color.black
			};

		AddGradient("Generated_Gradient", new Vector2(-80f, rampCenterY), new Vector2(56f, 170f), gradientColors);

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
		AddTriangle("Generated_Triangle_" + label, new Vector2(-35f, y), new Vector2(18f, 22f), textColor);
		AddText("Generated_BreakLabel_" + label, label, new Vector2(88f, y), new Vector2(210f, 38f), 28, TextAnchor.MiddleLeft, textColor);
	}

	private Text AddText(string name, string text, Vector2 anchoredPosition, Vector2 size, int fontSize, TextAnchor alignment, Color color)
	{
		return AddText(name, text, anchoredPosition, size, fontSize, alignment, color, panelRect ? panelRect : rectTransform);
	}

	private Text AddText(string name, string text, Vector2 anchoredPosition, Vector2 size, int fontSize, TextAnchor alignment, Color color, Transform parent)
	{
		var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
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
		textComponent.horizontalOverflow = HorizontalWrapMode.Overflow;
		textComponent.verticalOverflow = VerticalWrapMode.Overflow;
		textComponent.color = color;
		textComponent.raycastTarget = false;
		return textComponent;
	}

	private Image AddImage(string name, Vector2 anchoredPosition, Vector2 size, Color color)
	{
		return AddImage(name, anchoredPosition, size, color, panelRect ? panelRect : rectTransform);
	}

	private Image AddImage(string name, Vector2 anchoredPosition, Vector2 size, Color color, Transform parent)
	{
		var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
		go.layer = gameObject.layer;
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

	private void AddCircle(string name, Vector2 anchoredPosition, Vector2 size, Color color)
	{
		AddCircle(name, anchoredPosition, size, color, panelRect ? panelRect : rectTransform);
	}

	private void AddCircle(string name, Vector2 anchoredPosition, Vector2 size, Color color, Transform parent)
	{
		var image = AddImage(name, anchoredPosition, size, color, parent);
		image.sprite = GetCircleSprite();
		image.preserveAspect = true;
	}

	private void AddTriangle(string name, Vector2 anchoredPosition, Vector2 size, Color color)
	{
		var image = AddImage(name, anchoredPosition, size, color);
		image.sprite = GetTriangleSprite();
		image.preserveAspect = true;
	}

	private void AddGradient(string name, Vector2 anchoredPosition, Vector2 size, Color[] colors)
	{
		var image = AddImage(name, anchoredPosition, size, Color.white);
		image.sprite = CreateGradientSprite(colors);
		image.preserveAspect = false;
	}

	private Sprite GetCircleSprite()
	{
		if (circleSprite)
		{
			return circleSprite;
		}

		const int size = 32;
		var texture = new Texture2D(size, size, TextureFormat.ARGB32, false);
		texture.wrapMode = TextureWrapMode.Clamp;

		var center = (size - 1) * 0.5f;
		var radius = center - 1f;

		for (var y = 0; y < size; y++)
		{
			for (var x = 0; x < size; x++)
			{
				var distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
				var alpha = Mathf.Clamp01(radius + 0.75f - distance);
				texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
			}
		}

		texture.Apply();
		circleSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
		return circleSprite;
	}

	private Sprite GetTriangleSprite()
	{
		if (triangleSprite)
		{
			return triangleSprite;
		}

		const int width = 28;
		const int height = 32;
		var texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
		texture.wrapMode = TextureWrapMode.Clamp;

		for (var y = 0; y < height; y++)
		{
			for (var x = 0; x < width; x++)
			{
				var halfHeight = height * 0.5f;
				var maxX = Mathf.Lerp(width - 1f, 2f, Mathf.Abs(y - halfHeight) / halfHeight);
				texture.SetPixel(x, y, x <= maxX ? Color.white : Color.clear);
			}
		}

		texture.Apply();
		triangleSprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 100f);
		return triangleSprite;
	}

	private Sprite CreateGradientSprite(Color[] colors)
	{
		const int width = 16;
		const int height = 128;
		var texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
		texture.wrapMode = TextureWrapMode.Clamp;

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

	private static Color EvaluateGradient(Color[] colors, float t)
	{
		if (colors == null || colors.Length == 0)
		{
			return Color.white;
		}

		if (colors.Length == 1)
		{
			return colors[0];
		}

		var scaled = Mathf.Clamp01(t) * (colors.Length - 1);
		var index = Mathf.Min(Mathf.FloorToInt(scaled), colors.Length - 2);
		var localT = scaled - index;
		return Color.Lerp(colors[index], colors[index + 1], localT);
	}

	private void ClearGeneratedChildren()
	{
		panelRect = null;
		panelBackground = null;

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
}
