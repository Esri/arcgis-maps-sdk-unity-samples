// Copyright 2026 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class PointCloudMobileUIOptimizer : MonoBehaviour
{
	private const string HitAreaName = "Generated_MobileHitArea";

	[SerializeField] private float refreshIntervalSeconds = 0.75f;
	[SerializeField] private Vector2 minimumButtonHitSize = new Vector2(112f, 80f);
	[SerializeField] private Vector2 minimumSliderHitSize = new Vector2(160f, 72f);
	[SerializeField] private Vector2 minimumToggleHitSize = new Vector2(72f, 44f);
	[SerializeField] private float mobileCanvasMatchWidthOrHeight = 0.5f;
	[SerializeField] private int mobileDragThreshold = 24;

	private Coroutine refreshCoroutine;

	private void OnEnable()
	{
		if (!Application.isPlaying || !Application.isMobilePlatform)
		{
			enabled = false;
			return;
		}

		OptimizeNow();
		refreshCoroutine = StartCoroutine(RefreshWhileUIIsGenerated());
	}

	private void OnDisable()
	{
		if (refreshCoroutine != null)
		{
			StopCoroutine(refreshCoroutine);
			refreshCoroutine = null;
		}
	}

	private IEnumerator RefreshWhileUIIsGenerated()
	{
		while (enabled)
		{
			yield return new WaitForSeconds(refreshIntervalSeconds);
			OptimizeNow();
		}
	}

	private void OptimizeNow()
	{
		UpdateCanvasScaler();
		UpdateEventSystem();
		ExpandSelectableHitAreas();
	}

	private void UpdateCanvasScaler()
	{
		var canvasScalers = FindObjectsByType<CanvasScaler>(FindObjectsInactive.Include, FindObjectsSortMode.None);
		foreach (var scaler in canvasScalers)
		{
			if (scaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize)
			{
				scaler.matchWidthOrHeight = mobileCanvasMatchWidthOrHeight;
			}
		}
	}

	private void UpdateEventSystem()
	{
		if (EventSystem.current)
		{
			EventSystem.current.pixelDragThreshold = Mathf.Max(EventSystem.current.pixelDragThreshold, mobileDragThreshold);
		}
	}

	private void ExpandSelectableHitAreas()
	{
		var selectables = FindObjectsByType<Selectable>(FindObjectsInactive.Include, FindObjectsSortMode.None);
		foreach (var selectable in selectables)
		{
			if (!selectable || selectable is Scrollbar)
			{
				continue;
			}

			var rectTransform = selectable.transform as RectTransform;
			if (!rectTransform)
			{
				continue;
			}

			var hitArea = rectTransform.Find(HitAreaName) as RectTransform;
			if (!hitArea)
			{
				hitArea = CreateHitArea(rectTransform);
			}

			var size = rectTransform.rect.size;
			var minimumSize = GetMinimumHitSize(selectable, size);
			hitArea.sizeDelta = new Vector2(
				Mathf.Max(minimumSize.x, size.x),
				Mathf.Max(minimumSize.y, size.y));
		}
	}

	private Vector2 GetMinimumHitSize(Selectable selectable, Vector2 currentSize)
	{
		if (selectable is Slider)
		{
			return new Vector2(Mathf.Max(minimumSliderHitSize.x, currentSize.x), minimumSliderHitSize.y);
		}

		if (selectable is Toggle)
		{
			return minimumToggleHitSize;
		}

		return minimumButtonHitSize;
	}

	private static RectTransform CreateHitArea(RectTransform parent)
	{
		var hitAreaObject = new GameObject(HitAreaName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
		hitAreaObject.layer = parent.gameObject.layer;
		hitAreaObject.transform.SetParent(parent, false);
		hitAreaObject.transform.SetAsFirstSibling();

		var hitArea = hitAreaObject.GetComponent<RectTransform>();
		hitArea.anchorMin = new Vector2(0.5f, 0.5f);
		hitArea.anchorMax = new Vector2(0.5f, 0.5f);
		hitArea.pivot = new Vector2(0.5f, 0.5f);
		hitArea.anchoredPosition = Vector2.zero;

		var image = hitAreaObject.GetComponent<Image>();
		image.color = Color.clear;
		image.raycastTarget = true;

		return hitArea;
	}
}
