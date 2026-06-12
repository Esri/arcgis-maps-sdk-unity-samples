using System.Collections;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public sealed class PointCloudFilterMockPanel : MonoBehaviour
{
	[SerializeField] private Font font;
	[SerializeField] private Sprite checkboxOutlineSprite;

	private readonly Color accentColor = new Color(0.56f, 0.25f, 1f, 1f);
	private readonly Color textColor = Color.white;
	private readonly Color mutedTextColor = new Color(0.78f, 0.78f, 0.78f, 1f);
	private readonly Color scrollbarTrackColor = new Color(0.28f, 0.28f, 0.28f, 0.82f);
	private readonly Color scrollbarHandleColor = new Color(0.75f, 0.75f, 0.75f, 1f);

	private RectTransform rectTransform;
	private Coroutine runtimeRebuildCoroutine;
#if UNITY_EDITOR
	private bool rebuildQueued;
#endif

	private void OnEnable()
	{
		QueueBuild();
	}

	private void OnDisable()
	{
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

		AddFilterGroup(
			"Class Code",
			new[]
			{
				"<all>",
				"Ground",
				"High vegetation",
				"Unclassified",
				"Building",
				"Water",
				"Low vegetation",
				"Medium vegetation",
				"Low point (noise)"
			},
			175f,
			38f,
			200f);

		AddSeparator(-130f);

		AddFilterGroup(
			"Returns",
			new[]
			{
				"<all>",
				"First of many",
				"Last",
				"Last of many",
				"Single",
				"First return",
				"Intermediate"
			},
			-190f,
			-318f,
			190f);
	}

	private void AddFilterGroup(string title, string[] options, float titleY, float viewportCenterY, float viewportHeight)
	{
		const float rowSpacing = 48f;
		var contentHeight = Mathf.Max(viewportHeight + 120f, options.Length * rowSpacing + 24f);

		AddText("Generated_FilterTitle_" + title, title, new Vector2(15f, titleY), new Vector2(560f, 48f), 30, TextAnchor.MiddleLeft, mutedTextColor, rectTransform);

		var content = AddScrollArea(
			"Generated_FilterScroll_" + title,
			new Vector2(-35f, viewportCenterY),
			new Vector2(480f, viewportHeight),
			contentHeight,
			new Vector2(275f, viewportCenterY));

		for (var i = 0; i < options.Length; i++)
		{
			var y = contentHeight * 0.5f - rowSpacing * 0.5f - i * rowSpacing;
			AddCheckbox("Generated_FilterCheckbox_" + title + "_" + i, new Vector2(-210f, y), content);
			AddText("Generated_FilterLabel_" + title + "_" + i, options[i], new Vector2(30f, y), new Vector2(410f, 42f), 28, TextAnchor.MiddleLeft, textColor, content);
		}
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

	private void AddCheckbox(string name, Vector2 anchoredPosition, Transform parent)
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
