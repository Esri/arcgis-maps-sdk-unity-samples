using Esri.ArcGISMapsSDK.Components;
using Esri.ArcGISMapsSDK.Utils;
using Esri.GameEngine;
using Esri.GameEngine.Layers;
using Esri.Unity;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed class PointCloudLayerDataLoader : MonoBehaviour
{
	private const string DefaultPointCloudSource = "https://tiles.arcgis.com/tiles/V6ZHFr6zdgNZuVG0/arcgis/rest/services/BARNEGAT_BAY_LiDAR_UTM/SceneServer";

	[SerializeField] private InputField sourceInput;
	[SerializeField] private Button loadButton;
	[SerializeField] private Text statusText;
	[SerializeField] private float loadTimeoutSeconds = 30f;
	[SerializeField] private float loadPollSeconds = 0.25f;
	[SerializeField] private string defaultSource = DefaultPointCloudSource;
	[SerializeField] private bool loadDefaultOnStart = true;

	private readonly Color failureColor = new Color(1f, 0.36f, 0.49f, 1f);
	private readonly Color loadingColor = new Color(0.78f, 0.78f, 0.78f, 1f);
	private readonly Color successColor = new Color(0.55f, 1f, 0.62f, 1f);

	private ArcGISMapComponent arcGISMapComponent;
	private Coroutine loadCoroutine;
	private ArcGISPointCloudLayer userLoadedLayer;
	private string loadedSource;
	private bool subscribed;
	private bool defaultLoadStarted;

	public event Action<ArcGISPointCloudLayer> LayerLoaded;

	public ArcGISPointCloudLayer LoadedLayer => userLoadedLayer;
	public string LoadedSource => loadedSource;
	public string APIKey => GetAPIKey();

	private void Reset()
	{
		FindReferences();
	}

	private void OnEnable()
	{
		FindReferences();
		ApplyDefaultSource();
		Subscribe();

		if (Application.isPlaying)
		{
			HideStatus();
		}
	}

	private void Start()
	{
		if (!Application.isPlaying || !loadDefaultOnStart || defaultLoadStarted)
		{
			return;
		}

		defaultLoadStarted = true;
		ApplyDefaultSource();
		HandleLoadClicked();
	}

	private void OnDisable()
	{
		Unsubscribe();
	}

	private void FindReferences()
	{
		if (!sourceInput)
		{
			sourceInput = transform.Find("DataLoadingPanel/SourceInput")?.GetComponent<InputField>();
		}

		if (!loadButton)
		{
			loadButton = transform.Find("DataLoadingPanel/LoadButton")?.GetComponent<Button>();
		}

		if (!statusText)
		{
			statusText = transform.Find("DataLoadingPanel/StatusText")?.GetComponent<Text>();
		}
	}

	private void ApplyDefaultSource()
	{
		if (!sourceInput || !string.IsNullOrWhiteSpace(sourceInput.text))
		{
			return;
		}

		sourceInput.text = string.IsNullOrWhiteSpace(defaultSource) ? DefaultPointCloudSource : defaultSource;
	}

	private void Subscribe()
	{
		if (subscribed || !loadButton)
		{
			return;
		}

		loadButton.onClick.AddListener(HandleLoadClicked);
		subscribed = true;
	}

	private void Unsubscribe()
	{
		if (!subscribed || !loadButton)
		{
			subscribed = false;
			return;
		}

		loadButton.onClick.RemoveListener(HandleLoadClicked);
		subscribed = false;
	}

	private void HandleLoadClicked()
	{
		if (!Application.isPlaying)
		{
			ShowStatus("Enter Play Mode to load point scene layer.", loadingColor);
			return;
		}

		var source = sourceInput ? sourceInput.text.Trim() : "";
		if (string.IsNullOrEmpty(source))
		{
			ShowFailure();
			return;
		}

		if (loadCoroutine != null)
		{
			StopCoroutine(loadCoroutine);
		}

		loadCoroutine = StartCoroutine(LoadPointCloudLayer(source));
	}

	private IEnumerator LoadPointCloudLayer(string source)
	{
		SetControlsInteractable(false);
		ShowStatus("Loading point scene layer...", loadingColor);

		arcGISMapComponent = FindFirstObjectByType<ArcGISMapComponent>();
		if (!arcGISMapComponent || arcGISMapComponent.Map == null)
		{
			ShowFailure();
			SetControlsInteractable(true);
			yield break;
		}

		ArcGISPointCloudLayer newLayer = null;
		var newLayerIndex = arcGISMapComponent.Map.Layers.GetSize();
		try
		{
			newLayer = new ArcGISPointCloudLayer(source, "UserPCL", 1f, true, GetAPIKey());
			arcGISMapComponent.Map.Layers.Add(newLayer);

			if (newLayer.LoadStatus == ArcGISLoadStatus.NotLoaded)
			{
				newLayer.Load();
			}
		}
		catch (Exception exception)
		{
			Debug.LogWarning("Failed to start loading point cloud layer: " + exception.Message);
			ShowFailure();
			SetControlsInteractable(true);
			yield break;
		}

		var startTime = Time.realtimeSinceStartup;
		while ((newLayer.LoadStatus == ArcGISLoadStatus.NotLoaded || newLayer.LoadStatus == ArcGISLoadStatus.Loading) &&
			Time.realtimeSinceStartup - startTime < loadTimeoutSeconds)
		{
			yield return new WaitForSeconds(loadPollSeconds);
		}

		if (newLayer.LoadStatus == ArcGISLoadStatus.Loaded && HasUsablePointCloudData(newLayer))
		{
			userLoadedLayer = newLayer;
			loadedSource = source;
			RemovePointCloudLayersExcept(newLayerIndex);
			LayerLoaded?.Invoke(newLayer);
			yield return ZoomToLoadedLayer(newLayer);
			ShowStatus("Layer loaded successfully...", successColor);
		}
		else
		{
			RemoveLayerAt(newLayerIndex);
			ShowFailure();
		}

		SetControlsInteractable(true);
		loadCoroutine = null;
	}

	private bool HasUsablePointCloudData(ArcGISPointCloudLayer layer)
	{
		if (layer == null)
		{
			return false;
		}

		try
		{
			var attributes = layer.Attributes;
			if (attributes == null || attributes.GetSize() == 0)
			{
				return false;
			}

			var extent = layer.Extent;
			return extent != null && extent.Center != null;
		}
		catch (Exception exception)
		{
			Debug.LogWarning("Loaded point cloud layer did not expose valid point cloud metadata: " + exception.Message);
			return false;
		}
	}

	private string GetAPIKey()
	{
		if (arcGISMapComponent && !string.IsNullOrEmpty(arcGISMapComponent.APIKey))
		{
			return arcGISMapComponent.APIKey;
		}

		return ArcGISProjectSettingsAsset.Instance ? ArcGISProjectSettingsAsset.Instance.APIKey : "";
	}

	private IEnumerator ZoomToLoadedLayer(ArcGISPointCloudLayer layer)
	{
		var zoomTarget = GetZoomCameraGameObject();
		if (!zoomTarget)
		{
			Debug.LogWarning("Point cloud layer loaded, but no ArcGIS camera GameObject was found for zoom to layer.");
			yield break;
		}

		var startTime = Time.realtimeSinceStartup;
		while (!arcGISMapComponent.HasSpatialReference() && Time.realtimeSinceStartup - startTime < loadTimeoutSeconds)
		{
			yield return null;
		}

		System.Threading.Tasks.Task<bool> zoomTask;
		try
		{
			zoomTask = arcGISMapComponent.ZoomToLayer(zoomTarget, layer);
		}
		catch (Exception exception)
		{
			Debug.LogWarning("Point cloud layer loaded, but zoom to layer could not start: " + exception.Message);
			yield break;
		}

		while (!zoomTask.IsCompleted)
		{
			yield return null;
		}

		if (zoomTask.IsFaulted)
		{
			Debug.LogWarning("Point cloud layer loaded, but zoom to layer failed: " + zoomTask.Exception.GetBaseException().Message);
			yield break;
		}

		if (!zoomTask.Result)
		{
			Debug.LogWarning("Point cloud layer loaded, but the SDK zoom to layer call returned false.");
		}
	}

	private GameObject GetZoomCameraGameObject()
	{
		var mainCamera = Camera.main;
		if (mainCamera && mainCamera.GetComponentInParent<ArcGISMapComponent>() == arcGISMapComponent)
		{
			return mainCamera.gameObject;
		}

		var arcGISCamera = arcGISMapComponent ? arcGISMapComponent.GetComponentInChildren<ArcGISCameraComponent>(true) : null;
		if (arcGISCamera)
		{
			return arcGISCamera.gameObject;
		}

		var camera = arcGISMapComponent ? arcGISMapComponent.GetComponentInChildren<Camera>(true) : null;
		return camera ? camera.gameObject : null;
	}

	private void RemovePointCloudLayersExcept(ulong keepIndex)
	{
		if (!arcGISMapComponent || arcGISMapComponent.Map == null)
		{
			return;
		}

		var layers = arcGISMapComponent.Map.Layers;
		for (var i = (long)layers.GetSize() - 1; i >= 0; --i)
		{
			var index = (ulong)i;
			if (index != keepIndex && layers.At(index) is ArcGISPointCloudLayer)
			{
				layers.Remove(index);
			}
		}
	}

	private void RemoveLayerAt(ulong index)
	{
		if (!arcGISMapComponent || arcGISMapComponent.Map == null)
		{
			return;
		}

		var layers = arcGISMapComponent.Map.Layers;
		if (index < layers.GetSize())
		{
			layers.Remove(index);
		}
	}

	private void SetControlsInteractable(bool interactable)
	{
		if (sourceInput)
		{
			sourceInput.interactable = interactable;
		}

		if (loadButton)
		{
			loadButton.interactable = interactable;
		}
	}

	private void HideStatus()
	{
		if (statusText)
		{
			statusText.text = "";
			statusText.gameObject.SetActive(false);
		}
	}

	private void ShowFailure()
	{
		ShowStatus("Failed to load point scene layer!", failureColor);
	}

	private void ShowStatus(string message, Color color)
	{
		if (!statusText)
		{
			return;
		}

		statusText.gameObject.SetActive(true);
		statusText.text = message;
		statusText.color = color;
	}
}
