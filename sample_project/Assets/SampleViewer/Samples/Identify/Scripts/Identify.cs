// Copyright 2025 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Layers;
using Esri.GameEngine.MapView;
using Esri.Unity;
using System;
using System.Collections; // For IEnumerator
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Identify : MonoBehaviour
{
    [SerializeField] private ArcGISMapComponent arcGISMapComponent;
    private ArcGIS3DObjectSceneLayer buildingLayer;
    [SerializeField] private GameObject buildingToggle;
    [SerializeField] private Transform buildingListContentContainer;
    [SerializeField] private Transform contentContainer;
    private List<GameObject> buildingListItems = new List<GameObject>();
    private List<GameObject> ListItems = new List<GameObject>();
    [SerializeField] private GameObject markerGO;
    [SerializeField] private TextMeshProUGUI resultAmount;
    [SerializeField] private GameObject scrollViewItem;
    [SerializeField] private Color selectColor;
    [SerializeField] private Material selectMaterial;
    [SerializeField] private TextMeshProUGUI totalNumberOfBuildingsText;

    [SerializeField] private Button changeViewButton;
    [SerializeField] private Button showResultsButton;
    [SerializeField] private GameObject results;
    [SerializeField] private GameObject buildingsView;

    [SerializeField] private Button increaseResult;
    [SerializeField] private Button decreaseResult;

    private ArcGISImmutableCollection<ArcGISIdentifyLayerResult> resultValue;
    public ulong resultsLength;
    private float selectedID;
    [HideInInspector] public ulong SelectedResult = 0;

    // New non-blocking Identify controls
    [Header("Identify Async Settings")]
    [SerializeField] private bool useCallbackMode = false; // If true use future callback path instead of coroutine polling.
    [SerializeField] private float identifyLayersTimeoutSeconds = 5f; // <= 0 means no timeout.
    [SerializeField] private bool cancelOnTimeout = true; // If true call Cancel() on the future when timing out.
    [SerializeField] private bool logProgressWhileWaiting = true; // Coroutine only: log elapsed waiting time.
    [SerializeField] private float progressLogIntervalSeconds = 1f; // Interval between progress logs.
    [SerializeField] private bool cancelPreviousIdentify = true; // Stop previous coroutine before starting new one.

    // Internal state for async handling
    private Coroutine activeIdentifyLayersCoroutine;
    private readonly Queue<Action> mainThreadActions = new(); // Actions queued from future callbacks to run on main thread

    private void DisableButtons(bool enabled)
    {
        increaseResult.interactable = enabled;
        decreaseResult.interactable = enabled;
    }

    public void ResetButton()
    {
        Shader.SetGlobalFloat("_SelectedObjectID", 0);
        resultValue = null;
        resultAmount.text = "";
        DisableButtons(false);
    }

    public void EmptyIdentifyResults()
    {
        if (ListItems != null)
        {
            foreach (var item in ListItems)
            {
                Destroy(item.gameObject);
            }

            ListItems.Clear();
            results.SetActive(false);
        }
    }

    public void EmptyBuildingListResults()
    {
        if (buildingListItems != null)
        {
            foreach (var item in buildingListItems)
            {
                Destroy(item.gameObject);
            }

            buildingListItems.Clear();
            buildingsView.SetActive(false);
        }
    }

    private void PopulateBuildingList()
    {
        for (int i = 0; i < (int)resultsLength; i++)
        {
            var item = Instantiate(buildingToggle);
            var tmpObject = item.GetComponentInChildren<TextMeshProUGUI>();
            item.GetComponent<BuildingToggleItem>().BuildingNumber = (ulong)i;
            item.GetComponent<BuildingToggleItem>().ResultValue = resultValue;
            tmpObject.text = $"Building {i + 1}";

            if (i == 0)
            {
                item.GetComponent<BuildingToggleItem>().toggleImage.sprite = item.GetComponent<BuildingToggleItem>().isOn;
            }
            else
            {
                item.GetComponentInChildren<Image>().sprite = item.GetComponent<BuildingToggleItem>().isOff;
            }

            item.transform.SetParent(buildingListContentContainer);
            item.transform.localScale = Vector2.one;
            buildingListItems.Add(item);
        }
    }

    private void Start()
    {
        if (arcGISMapComponent)
        {
            buildingLayer = new ArcGIS3DObjectSceneLayer("https://tiles.arcgis.com/tiles/P3ePLMYs2RVChkJx/arcgis/rest/services/Buildings_NewYork_17/SceneServer", "Building Layer", 1.0f, true, "");
            arcGISMapComponent.Map.Layers.Add(buildingLayer);

            if (buildingLayer)
            {
                Setup3DAttributesFloatAndIntegerType(buildingLayer);
            }
        }

        resultAmount.text = "";
        totalNumberOfBuildingsText.text = "";
        buildingsView.SetActive(false);
        results.SetActive(false);
        DisableButtons(false);

        changeViewButton.onClick.AddListener(delegate
        {
            if (buildingsView.activeInHierarchy)
            {
                buildingsView.SetActive(false);
                results.SetActive(true);
            }
            else
            {
                buildingsView.SetActive(true);
                results.SetActive(false);
            }
        });

        showResultsButton.onClick.AddListener(delegate
        {
            buildingsView.SetActive(false);
            results.SetActive(true);
        });

        increaseResult.onClick.AddListener(delegate
        {
            if (SelectedResult < resultsLength - 1)
            {
                EmptyIdentifyResults();
                ++SelectedResult;
                resultAmount.text = $"{SelectedResult + 1} of {resultsLength}";
                ParseResults(SelectedResult, resultValue);

                if (SelectedResult == resultsLength - 1)
                {
                    increaseResult.interactable = false;
                    decreaseResult.interactable = true;
                }
                else
                {
                    DisableButtons(true);
                }
            }
        });

        decreaseResult.onClick.AddListener(delegate
        {
            if (SelectedResult > 0)
            {
                EmptyIdentifyResults();
                EmptyBuildingListResults();
                --SelectedResult;
                resultAmount.text = $"{SelectedResult + 1} of {resultsLength}";
                ParseResults(SelectedResult, resultValue);

                if (SelectedResult == 0)
                {
                    increaseResult.interactable = true;
                    decreaseResult.interactable = false;
                }
                else
                {
                    DisableButtons(true);
                }
            }
        });

        Shader.SetGlobalFloat("_SelectedObjectID", 0);
        Shader.SetGlobalColor("_HighlightColor", selectColor);
    }

    private void Update()
    {
        // Drain queued callback actions (only in callback mode but cheap to always check)
        if (mainThreadActions.Count > 0)
        {
            var pendingCount = mainThreadActions.Count;
            for (var i = 0; i < pendingCount; i++)
            {
                var action = mainThreadActions.Dequeue();
                try { action?.Invoke(); }
                catch (Exception ex) { Debug.LogError($"Identify callback action error: {ex}"); }
            }
        }
    }

    public void StartRaycast()
    {
#if !UNITY_IOS && !UNITY_ANDROID && !UNITY_VISIONOS
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
#else
        Ray ray = Camera.main.ScreenPointToRay(inputManager.touchControls.Touch.TouchPosition.ReadValue<Vector2>());
#endif

        if (Physics.Raycast(ray, out var hit))
        {
            EmptyIdentifyResults();
            EmptyBuildingListResults();
            SelectedResult = 0;
            var geoPosition = arcGISMapComponent.EngineToGeographic(hit.point);
            var cameraGeoPosition = arcGISMapComponent.EngineToGeographic(Camera.main.transform.position);

            if (useCallbackMode)
            {
                StartIdentifyLayersCallback(geoPosition, cameraGeoPosition);
            }
            else
            {
                StartIdentifyLayersCoroutine(geoPosition, cameraGeoPosition);
            }
        }
    }

    private void StartIdentifyLayersCoroutine(Esri.GameEngine.Geometry.ArcGISPoint start, Esri.GameEngine.Geometry.ArcGISPoint end)
    {
        if (cancelPreviousIdentify && activeIdentifyLayersCoroutine != null)
        {
            StopCoroutine(activeIdentifyLayersCoroutine);
            activeIdentifyLayersCoroutine = null;
        }
        activeIdentifyLayersCoroutine = StartCoroutine(IdentifyLayersRoutine(start, end));
    }

    private void StartIdentifyLayersCallback(Esri.GameEngine.Geometry.ArcGISPoint start, Esri.GameEngine.Geometry.ArcGISPoint end)
    {
        if (activeIdentifyLayersCoroutine != null)
        {
            StopCoroutine(activeIdentifyLayersCoroutine);
            activeIdentifyLayersCoroutine = null;
        }

        var future = arcGISMapComponent.View.IdentifyLayersAsync(start, end, -1);
        var startTime = Time.realtimeSinceStartup;
        var timedOut = false;

        if (identifyLayersTimeoutSeconds > 0)
        {
            StartCoroutine(CallbackTimeoutWatcher(future, startTime, () => timedOut = true));
        }

        future.TaskCompleted = () =>
        {
            // Queue processing for main thread
            mainThreadActions.Enqueue(() =>
            {
                if (timedOut)
                {
                    Debug.LogWarning("IdentifyLayersAsync (callback) completed after timeout; ignoring.");
                    return;
                }

                if (future.GetError() is Exception err)
                {
                    Debug.LogError($"IdentifyLayersAsync (callback) error: {err.Message}\n{err}");
                    return;
                }

                if (future.IsCanceled())
                {
                    Debug.LogWarning("IdentifyLayersAsync (callback) was canceled.");
                    return;
                }

                var layerResults = future.Get();
                if (layerResults == null || layerResults.IsEmpty())
                {
                    Debug.LogWarning("IdentifyLayersAsync (callback) returned no results.");
                    return;
                }

                resultValue = layerResults;
                ParseResults(SelectedResult, resultValue);
                PopulateBuildingList();
                Debug.Log($"IdentifyLayersAsync (callback) completed in {(Time.realtimeSinceStartup - startTime):F2}s; results={resultValue.At(0).GeoElements.GetSize()}");
            });
        };
    }

    private IEnumerator CallbackTimeoutWatcher(ArcGISFuture<ArcGISImmutableCollection<ArcGISIdentifyLayerResult>> future, float start, Action onTimeout)
    {
        while (!future.IsDone())
        {
            if (identifyLayersTimeoutSeconds > 0 && (Time.realtimeSinceStartup - start) > identifyLayersTimeoutSeconds)
            {
                Debug.LogWarning("IdentifyLayersAsync (callback) timed out.");
                if (cancelOnTimeout)
                {
                    future.Cancel();
                }
                onTimeout?.Invoke();
                yield break;
            }
            yield return null;
        }
    }

    private IEnumerator IdentifyLayersRoutine(Esri.GameEngine.Geometry.ArcGISPoint start, Esri.GameEngine.Geometry.ArcGISPoint end)
    {
    var future = arcGISMapComponent.View.IdentifyLayersAsync(start, end, -1);
        var identifyStartTime = Time.realtimeSinceStartup;
        var lastProgressLogTime = identifyStartTime;

        while (!future.IsDone())
        {
            if (identifyLayersTimeoutSeconds > 0 && (Time.realtimeSinceStartup - identifyStartTime) > identifyLayersTimeoutSeconds)
            {
                Debug.LogWarning("IdentifyLayersAsync (coroutine) timed out.");
                if (cancelOnTimeout)
                {
                    future.Cancel();
                }
                activeIdentifyLayersCoroutine = null;
                yield break;
            }

            if (logProgressWhileWaiting && (Time.realtimeSinceStartup - lastProgressLogTime) >= progressLogIntervalSeconds)
            {
                lastProgressLogTime = Time.realtimeSinceStartup;
                Debug.Log($"IdentifyLayersAsync (coroutine) waiting... elapsed={(lastProgressLogTime - identifyStartTime):F2}s");
            }

            yield return null;
        }

        if (future.GetError() is Exception error)
        {
            Debug.LogError($"IdentifyLayersAsync (coroutine) error: {error.Message}\n{error}");
            activeIdentifyLayersCoroutine = null;
            yield break;
        }

        if (future.IsCanceled())
        {
            Debug.LogWarning("IdentifyLayersAsync (coroutine) was canceled.");
            activeIdentifyLayersCoroutine = null;
            yield break;
        }

        var layerResults = future.Get();
        if (layerResults == null || layerResults.IsEmpty())
        {
            Debug.LogWarning("IdentifyLayersAsync (coroutine) returned no results.");
            activeIdentifyLayersCoroutine = null;
            yield break;
        }

        resultValue = layerResults;
        ParseResults(SelectedResult, resultValue);
        PopulateBuildingList();
        Debug.Log($"IdentifyLayersAsync (coroutine) completed in {(Time.realtimeSinceStartup - identifyStartTime):F2}s; results={resultValue.At(0).GeoElements.GetSize()}");
        activeIdentifyLayersCoroutine = null;
    }

    public void ParseResults(ulong NumberOfResults, ArcGISImmutableCollection<ArcGISIdentifyLayerResult> ResultValue)
    {
        if (ResultValue.IsEmpty())
        {
            Debug.LogWarning("No Results Found");
            return;
        }

        var elements = ResultValue.At(0).GeoElements;
        resultsLength = elements.GetSize();

        if (resultsLength == 0)
        {
            Debug.LogWarning("No Results Found");
            return;
        }
        else if (resultsLength == 1)
        {
            DisableButtons(false);
        }
        else if (resultsLength > 1)
        {

            increaseResult.interactable = true;
            decreaseResult.interactable = false;
        }

        totalNumberOfBuildingsText.text = $"total: {resultsLength}";
        resultAmount.text = $"{SelectedResult + 1} of {resultsLength}";
        var attributes = elements.At(NumberOfResults).Attributes;
        var keys = attributes.Keys;
        buildingsView.SetActive(false);
        results.SetActive(true);

        try
        {
            var id = attributes["OBJECTID"];
            selectedID = Convert.ToInt32(id.ToString());
            Shader.SetGlobalFloat("_SelectedObjectID", selectedID);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }

        for (ulong k = 0; k < keys.Size; k++)
        {
            try
            {
                var value = attributes[keys.At(k)];
                var item = Instantiate(scrollViewItem);
                var tmpObjects = item.GetComponentsInChildren<TextMeshProUGUI>();
                tmpObjects[0].text = $"<b>{keys.At(k)}</b>";
                tmpObjects[1].text = value.ToString();
                item.transform.SetParent(contentContainer);
                item.transform.localScale = Vector2.one;
                ListItems.Add(item);
            }
            catch
            {
                Debug.Log(keys.At(k) + ": <no conversion available>");
            }
        }

    }

    private void Setup3DAttributesFloatAndIntegerType(ArcGIS3DObjectSceneLayer layer)
    {
        var layerAttributes = ArcGISImmutableArray<String>.CreateBuilder();
        layerAttributes.Add("OBJECTID");
        layer.SetAttributesToVisualize(layerAttributes.MoveToArray());
        layer.MaterialReference = selectMaterial;
    }
}