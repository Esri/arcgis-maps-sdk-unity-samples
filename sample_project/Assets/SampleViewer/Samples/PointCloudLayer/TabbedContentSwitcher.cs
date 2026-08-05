// Copyright 2026 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using System;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public sealed class TabbedContentSwitcher : MonoBehaviour
{
	[Serializable]
	private struct TabContent
	{
		public float backgroundBottomExtension;
		public GameObject[] content;
		public Toggle toggle;
	}

	[SerializeField] private RectTransform backgroundTarget;
	[SerializeField] private TabContent[] tabs;

	private void OnEnable()
	{
		Subscribe();
		Refresh();
	}

	private void OnDisable()
	{
		Unsubscribe();
	}

	private void OnValidate()
	{
		Refresh();
	}

	private void Subscribe()
	{
		if (tabs == null)
		{
			return;
		}

		foreach (var tab in tabs)
		{
			if (tab.toggle)
			{
				tab.toggle.onValueChanged.RemoveListener(OnTabChanged);
				tab.toggle.onValueChanged.AddListener(OnTabChanged);
			}
		}
	}

	private void Unsubscribe()
	{
		if (tabs == null)
		{
			return;
		}

		foreach (var tab in tabs)
		{
			if (tab.toggle)
			{
				tab.toggle.onValueChanged.RemoveListener(OnTabChanged);
			}
		}
	}

	private void OnTabChanged(bool _)
	{
		Refresh();
	}

	private void Refresh()
	{
		if (tabs == null)
		{
			return;
		}

		TabContent? activeTab = null;

		foreach (var tab in tabs)
		{
			var isActive = tab.toggle && tab.toggle.isOn;
			if (isActive)
			{
				activeTab = tab;
			}

			if (tab.content == null)
			{
				continue;
			}

			foreach (var contentObject in tab.content)
			{
				if (contentObject && contentObject.activeSelf != isActive)
				{
					contentObject.SetActive(isActive);
				}
			}
		}

		if (backgroundTarget && activeTab.HasValue)
		{
			backgroundTarget.offsetMin = new Vector2(0f, -activeTab.Value.backgroundBottomExtension);
			backgroundTarget.offsetMax = Vector2.zero;
		}
	}
}
