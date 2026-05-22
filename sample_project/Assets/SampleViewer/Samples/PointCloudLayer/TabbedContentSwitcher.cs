using System;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public sealed class TabbedContentSwitcher : MonoBehaviour
{
	[Serializable]
	private struct TabContent
	{
		public Toggle toggle;
		public GameObject[] content;
		public Vector2 backgroundAnchoredPosition;
		public Vector2 backgroundSizeDelta;
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
			backgroundTarget.anchoredPosition = activeTab.Value.backgroundAnchoredPosition;
			backgroundTarget.sizeDelta = activeTab.Value.backgroundSizeDelta;
		}
	}
}
